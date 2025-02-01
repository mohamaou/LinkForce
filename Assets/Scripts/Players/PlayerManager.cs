using System.Collections.Generic;
using System.Linq;
using Cards;
using Core;
using DG.Tweening;
using Models;
using Troops;
using UI;
using UnityEngine;

namespace Players
{
    public enum PlayerTeam
    {
        Player1,
        Player2
    }
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] private BuildingCard[] buildingCards;
        [SerializeField] private PlayerTeam team;
        private readonly List<BuildingCard> _selectedBuilding = new();
        private readonly List<Building> _buildingsOnBoard = new();
        private readonly List<Troop> _myTroops = new ();
        private List<Link> _myLinks = new();
        
        
        protected void SetDeck()
        {
            if (team == PlayerTeam.Player1)
            {
                foreach (var card in buildingCards)
                {
                    if(card.IsSelected()) _selectedBuilding.Add(card);
                }
                UIManager.Instance.startUI.SetPlayer1Card(_selectedBuilding.ToArray());
            }
            else
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

                var requiredTroops = Mathf.Min(2, troopBuildings.Count);

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
                UIManager.Instance.startUI.SetPlayer2Card(_selectedBuilding.ToArray());
            }
            
        }
        protected List<BuildingCard> GetSelectedBuildingCards() => _selectedBuilding;

        #region public Variable
        public void AddTroop(Troop troop)
        {
            _myTroops.Add(troop);
            troop.SetDeathEvent(()=>_myTroops.Remove(troop));
        }
        public List<Troop> GetTroops() => _myTroops;
        protected List<Building> BuildingsOnBoard => _buildingsOnBoard;
        protected List<Link> MyLinks() => _myLinks;
        #endregion

        #region Summon Building
        protected bool SummonRandomBuilding()
        {
            for (int i = 0; i < _selectedBuilding.Count; i++)
            {
                var index = (Random.Range(0, _selectedBuilding.Count) + i) % _selectedBuilding.Count;
                var targetBuilding = _selectedBuilding[index].GetBuilding();

                if (!TroopBuildingAvailable() && targetBuilding.GetBuildingType() != BuildingType.Troops)
                    continue;

                var summonPos = Board.Instance.GetRandomBoardPoint(team, targetBuilding.GetBuildingType());
                if (summonPos == Vector3.zero) 
                    continue;

                var b = Instantiate(targetBuilding, summonPos, Quaternion.identity, transform);
                b.Set(team);
                _buildingsOnBoard.Add(b);
                return true;
            }
            return false;
        }
        
        private bool TroopBuildingAvailable()
        {
            foreach (var building in _buildingsOnBoard)
            {
                if(building.GetBuildingType() == BuildingType.Troops) return true;
            }
            return false;
        }
        #endregion

        #region Links Manage
        protected  bool ValidateLink(Building building1, Building building2)
        {
            if (building1 == building2) return false;
            var type1 = building1.GetBuildingType();
            var type2 = building2.GetBuildingType();
            var availableLinksToBuffs1 = building1.GetLinksToTroops() == 0
                ? 1 - building1.GetLinksToBuffs()
                : building1.GetLinksToBuffs() - building1.GetLinksToTroops();
            var availableLinksToBuffs2 = building2.GetLinksToTroops() == 0
                ? 1 - building2.GetLinksToBuffs()
                : building2.GetLinksToBuffs() - building2.GetLinksToTroops();
            var linksCount1 = building1.GetLinksCount();
            var linksCount2 = building2.GetLinksCount();
            var level1 = building1.GetLevel();
            var level2 = building2.GetLevel();

            var isTroopsBuff = (type1 == BuildingType.Troops && type2 == BuildingType.Buff) ||
                               (type1 == BuildingType.Buff && type2 == BuildingType.Troops);
            var isTroopsTroops = type1 == BuildingType.Troops && type2 == BuildingType.Troops;
            var sameType = type1 == type2;
            var sameId = building1.GetId() == building2.GetId();
            var sameLevel = level1 == level2;

            var validTypes = !isTroopsBuff && !isTroopsTroops;
            var noLinksToBuff = (type1 == BuildingType.Weapon && type2 == BuildingType.Buff &&
                                 availableLinksToBuffs1 == 0) || (type2 == BuildingType.Weapon &&
                                                                  type1 == BuildingType.Buff &&
                                                                  availableLinksToBuffs2 == 0);
            var troopsAlreadyLinked = (type1 == BuildingType.Troops && linksCount1 > 0) ||
                                      (type2 == BuildingType.Troops && linksCount2 > 0);

            var canLink = !sameType && !noLinksToBuff && !troopsAlreadyLinked;
            var canMerge = sameId && sameLevel && sameType;
            return (validTypes && canLink) || canMerge;
        }
        
        protected bool TryMergeBuildings(Building building1, Building  building2)
        {
            if (building2.GetId() != building1.GetId() || building2.GetLevel() != building1.GetLevel())
                return false;

            building1.transform.DOMove(building2.transform.position, .3f).OnComplete(() =>
            {
                building2.RunGFX();
                building2.IncrementLevel();
                _buildingsOnBoard.Remove(building1);

                if (building1.GetBuildingType() == BuildingType.Troops && building1.GetMyLinks().Count > 0 &&
                    building2.GetMyLinks().Count > 0)
                {
                    var sourceLinks = building1.GetMyLinks();
                    foreach (var link in sourceLinks)
                    {
                        var otherBuilding = link.GetLinkedBuilding(building1);
                        otherBuilding.RemoveLink(link);
                        otherBuilding.SetActive(false);
                        otherBuilding.SetLinksToTroops(otherBuilding.GetLinksToTroops() - 1);
                        _myLinks.Remove(link);
                        Destroy(link.gameObject);
                    }
                }
                else
                {
                    var sourceLinks = building1.GetMyLinks();
                    foreach (var link in sourceLinks)
                    {
                        var otherBuilding = link.GetLinkedBuilding(building1);
                        otherBuilding.RemoveLink(link);

                        var newLink = otherBuilding.CreateLink();
                        newLink.SetLink(PlayerTeam.Player1);

                        newLink.SetBuildings(otherBuilding, building2);
                        building2.SetBuildingLink(newLink);
                        otherBuilding.SetBuildingLink(newLink);

                        newLink.ShowLink(otherBuilding.transform.position, building2.transform.position);

                        _myLinks.Remove(link);
                        _myLinks.Add(newLink);
                        Destroy(link.gameObject);
                    }

                    building2.SetLinksToTroops(building2.GetLinksToTroops() + building1.GetLinksToTroops());
                    building2.SetLinksToBuffs(building2.GetLinksToBuffs() + building1.GetLinksToBuffs());
                }

                Destroy(building1.gameObject);
                CoinsManager.Instance.AddMergeReward(team);
            });
            return true;
        }
        
        protected bool TryToLinkBuildings(Building building1, Building building2)
        {
            if (!ValidateLink(building1, building2)) return false;
            var l = building1.CreateLink();
            l.SetLink(team);
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
        #endregion
    }
}