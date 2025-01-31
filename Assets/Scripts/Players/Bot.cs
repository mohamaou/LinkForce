using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Core;
using DG.Tweening;
using Models;
using Players;
using Troops;
using UI;
using UnityEngine;


namespace AI
{
    public class Bot : MonoBehaviour
    {
        private string _difficultyKey = "Difficulty";
        public static Bot Instance { get; private set; }
        [SerializeField] private BuildingCard[] buildingCards;
        [SerializeField] private Link link;
        [SerializeField] [Range(1,5)] private int minimumTroopBuildings = 1;
        private readonly List<BuildingCard> _selectedBuilding= new ();
        private readonly List<Building> _buildingsOnBoard = new();
        private readonly List<Troop> _myTroops = new ();
        [SerializeField] private List<Link> _myLinks = new();
        private bool _ready;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SetDeck();
            UIManager.Instance.startUI.SetPlayer2Card(_selectedBuilding.ToArray());
            StartCoroutine(AIPlay());
        }
        private void SetDeck()
        {
            var selectedIndices = new HashSet<int>();
            var troopBuildings = new List<int>();

            for (int i = 0; i < buildingCards.Length; i++)
            {
                if (buildingCards[i].GetBuildingType() == BuildingType.Troops)
                {
                    troopBuildings.Add(i);
                }
            }

            int requiredTroops = Mathf.Min(minimumTroopBuildings, troopBuildings.Count);

            while (selectedIndices.Count < requiredTroops)
            {
                int randomTroopIndex = troopBuildings[Random.Range(0, troopBuildings.Count)];
                if (!selectedIndices.Contains(randomTroopIndex))
                {
                    selectedIndices.Add(randomTroopIndex);
                    _selectedBuilding.Add(buildingCards[randomTroopIndex]);
                }
            }

            while (selectedIndices.Count < 5)
            {
                int randomIndex = Random.Range(0, buildingCards.Length);
                if (!selectedIndices.Contains(randomIndex))
                {
                    selectedIndices.Add(randomIndex);
                    _selectedBuilding.Add(buildingCards[randomIndex]);
                }
            }
        }
        public void AddTroop(Troop troop)
        {
            _myTroops.Add(troop);
            troop.SetDeathEvent(()=>_myTroops.Remove(troop));
        }
        public List<Troop> GetTroops() => _myTroops;
        public bool IsReady() => _ready;


        private IEnumerator AIPlay()
        {
            yield return new WaitUntil(() => GameManager.State == GameState.Play);
            yield return new WaitForSeconds(Random.Range(0.5f, 2f));
            while (GameManager.State  == GameState.Play)
            {
                if (GameManager.Instance.currentLevel.coinsPerSpawn <=
                    CoinsManager.Instance.GetCoinsAmount(PlayerTeam.Player2))
                {
                    CoinsManager.Instance.UseCoins(GameManager.Instance.currentLevel.coinsPerSpawn, PlayerTeam.Player2);
                    SummonRandomBuilding();
                    yield return new WaitForSeconds(Random.Range(0.2f, .5f));
                    yield return TryLinkBuildings();
                }
                else
                {
                    yield return TryLinkBuildings();
                    _ready = true;
                }
                yield return new WaitForSeconds(Random.Range(0.2f, 1f));
            }
        }

        private IEnumerator TryLinkBuildings()
        {
            if (_buildingsOnBoard.Count == 0) yield break;
            foreach (var troopBuilding in _buildingsOnBoard.Where(b => b.GetBuildingType() == BuildingType.Troops))
            {
                foreach (var weaponBuilding in _buildingsOnBoard.Where(b=> b.GetBuildingType() == BuildingType.Weapon))
                {
                    if(TryToLinkBuildings(troopBuilding, weaponBuilding)) yield return new WaitForSeconds(0.5f);
                }
            }
            foreach (var weaponBuilding in _buildingsOnBoard.Where(b => b.GetBuildingType() == BuildingType.Weapon))
            {
                foreach (var buffBuilding in _buildingsOnBoard.Where(b=> b.GetBuildingType() == BuildingType.Buff))
                {
                    if(TryToLinkBuildings(weaponBuilding, buffBuilding)) yield return new WaitForSeconds(0.5f);
                }
            }
        }

        private bool TryToLinkBuildings(Building building1, Building building2)
        {
            if (Player.ValidateLink(building1, building2))
            {
                var l = Instantiate(link, transform);
                l.SetLink(PlayerTeam.Player2);
                l.ShowLink(building1.transform.position, building2.transform.position);
                l.SetBuildings(building1, building2);
                building1.SetBuildingLink(l);
                building2.SetBuildingLink(l);
                _myLinks.Add(l);
                
                
                // Increment Weapon to Troops links number
                if (building1.GetBuildingType() == BuildingType.Weapon &&
                    building2.GetBuildingType() == BuildingType.Troops) building1.IncrementLinksToTroops();
                else if (building1.GetBuildingType() == BuildingType.Troops &&
                         building2.GetBuildingType() == BuildingType.Weapon)
                    building2.IncrementLinksToTroops();

                // Increment Weapon to Buffs links number
                if (building1.GetBuildingType() == BuildingType.Weapon &&
                    building2.GetBuildingType() == BuildingType.Buff) building1.IncrementLinksToBuffs();
                else if (building1.GetBuildingType() == BuildingType.Buff &&
                         building2.GetBuildingType() == BuildingType.Weapon)
                    building2.IncrementLinksToBuffs();
                
                return true;
            }
            return false;
        }
        
        private void SummonRandomBuilding()
        {
            for (int i = 0; i < _selectedBuilding.Count; i++)
            {
                int index = (Random.Range(0, _selectedBuilding.Count) + i) % _selectedBuilding.Count;
                var targetBuilding = _selectedBuilding[index].GetBuilding();

                if (!TroopBuildingAvailable() && targetBuilding.GetBuildingType() != BuildingType.Troops)
                    continue;

                var summonPos = Board.Instance.GetRandomBoardPoint(PlayerTeam.Player2, targetBuilding.GetBuildingType());
                if (summonPos == Vector3.zero) 
                    continue;

                var b = Instantiate(targetBuilding, summonPos, Quaternion.identity, transform);
                b.Set(PlayerTeam.Player2);
                _buildingsOnBoard.Add(b);
                break;
            }
        }


        private bool TroopBuildingAvailable()
        {
            foreach (var building in _buildingsOnBoard)
            {
                if(building.GetBuildingType() == BuildingType.Troops) return true;
            }
            return false;
        }
        private void MergeBuildings(Building target, Building source)
        {
            source.transform.DOMove(target.transform.position, .3f).OnComplete(() =>
            {
                target.RunGFX();
                target.IncrementLevel();
                _buildingsOnBoard.Remove(source);

                var sourceLinks = source.GetMyLinks();
                foreach (var link in sourceLinks)
                {
                    var otherBuilding = link.GetLinkedBuilding(source);
                    otherBuilding.RemoveLink(link);

                    var newLink = otherBuilding.CreateLink();
                    newLink.SetLink(PlayerTeam.Player1);

                    newLink.SetBuildings(otherBuilding, target);
                    target.SetBuildingLink(newLink);
                    otherBuilding.SetBuildingLink(newLink);

                    newLink.ShowLink(otherBuilding.transform.position, target.transform.position);

                    _myLinks.Remove(link);
                    _myLinks.Add(newLink);
                    Destroy(link.gameObject);
                }

                target.SetLinksToTroops(target.GetLinksToTroops() + source.GetLinksToTroops());
                target.SetLinksToBuffs(target.GetLinksToBuffs() + source.GetLinksToBuffs());
                Destroy(source.gameObject);
                CoinsManager.Instance.AddMergeReward(PlayerTeam.Player1);
            });
        }
    }
}
