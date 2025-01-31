using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Core;
using Troops;
using Models;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

//---------------------------------------
// TODO: Remove link to one of Buffs/Troops on merge when two of them are already linked
//---------------------------------------
namespace Players
{
    public enum PlayerTeam
    {
        Player1,
        Player2
    }

    public class Player : MonoBehaviour
    {
        [SerializeField] private BuildingCard[] buildingsCards;
        [SerializeField] private LayerMask buildingLayer, groundLayer;
        [SerializeField] private GameObject trail;
        [SerializeField] private Sprite hammer;
        [SerializeField] private GameObject hammerCursor;
        [SerializeField] private List<Link> _myLinks = new();
        private readonly List<Building> _buildingsOnBoard = new();
        private Camera _cam;
        private bool _linking;
        private List<BuildingCard> _selectedBuilding = new();
        private List<Troop> _myTroops = new();
        public static Player Instance { get; private set; }
        public bool isDestroyEnabled = false;

        private void Awake()
        {
            Instance = this;
            _cam = Camera.main;
        }

        
        private IEnumerator Start()
        {
            foreach (var card in buildingsCards)
            {
                if(card.IsSelected()) _selectedBuilding.Add(card);
            }
            UIManager.Instance.startUI.SetPlayer1Card(_selectedBuilding.ToArray());
            
            yield return new WaitUntil(() => GameManager.State == GameState.Play);
            
            UIManager.Instance.playPanel.GetSummonButton().onClick.AddListener(SummonButtonClicked);
            StartCoroutine(SelectBuilding());
            StartCoroutine(CutLinks());
            var t = trail;
            trail = Instantiate(t, transform);
        }
        
        public void AddTroop(Troop troop)
        {
            _myTroops.Add(troop);
            troop.SetDeathEvent(() =>
            {
                _myTroops.Remove(troop);
            });
        }

        public List<Troop> GetTroops() => _myTroops;
        

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) SummonButtonClicked();
            if(Input.GetKeyDown(KeyCode.B)) GameManager.Instance.StartBattle();

            if (isDestroyEnabled)
            {
                CheckCursorOverBuilding();
                DestroyBuilding();
                if (hammerCursor.activeSelf) UpdateHammerCursorPosition();
            }

        }

        private void SummonButtonClicked()
        {
            if (GameManager.Instance.currentLevel.coinsPerSpawn > CoinsManager.Instance.GetCoinsAmount(PlayerTeam.Player1))
            {
                UIManager.Instance.playPanel.ShowNotEnoughGoldEffect();
                return;
            }

            for (var i = 0; i < _selectedBuilding.Count; i++)
            {
                var targetBuilding = _selectedBuilding[(Random.Range(0, _selectedBuilding.Count) + i) % _selectedBuilding.Count].GetBuilding();
                var summonPos =
                    Board.Instance.GetRandomBoardPoint(PlayerTeam.Player1, targetBuilding.GetBuildingType());

                if (summonPos != Vector3.zero)
                {
                    var b = Instantiate(targetBuilding, summonPos, Quaternion.identity, transform);
                    b.Set(PlayerTeam.Player1);
                    _buildingsOnBoard.Add(b);
                    CoinsManager.Instance.UseCoins(GameManager.Instance.currentLevel.coinsPerSpawn, PlayerTeam.Player1);
                    return;
                }
            }

            UIManager.Instance.playPanel.ShowSpaceErrorText();
        }

        private IEnumerator SelectBuilding()
        {
            while (GameManager.State == GameState.Play && !isDestroyEnabled)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var ray = _cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out var hit, buildingLayer))
                        foreach (var building in _buildingsOnBoard)
                            if (building.transform == hit.transform)
                                yield return LinkBuildings(building);
                }

                yield return null;
            }
        }

        private IEnumerator LinkBuildings(Building building)
        {
            _linking = true;
            var link = building.CreateLink();
            link.SetLink(PlayerTeam.Player1);
            while (Input.GetMouseButton(0))
            {
                var ray = _cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayer))
                    link.ShowLink(building.transform.position, hit.point);

                foreach (var availableBuilding in _buildingsOnBoard)
                    if (ValidateLink(availableBuilding, building) && availableBuilding != building)
                        availableBuilding.Highlight();

                yield return null;
            }

            _linking = false;
            foreach (var availableBuilding in _buildingsOnBoard) availableBuilding.RemoveHighlight();

            if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out var b, Mathf.Infinity, buildingLayer))
            {
                link.ShowLink(building.transform.position, b.transform.position);
                var targetBuilding = b.transform.gameObject.GetComponent<Building>();
                if (ValidateLink(targetBuilding, building))
                {
                    if (building.GetBuildingType() == targetBuilding.GetBuildingType())
                    {
                        Destroy(link.gameObject);
                        MergeBuildings(targetBuilding, building);
                    }
                    else
                    {
                        link.SetBuildings(building, targetBuilding);
                        targetBuilding.SetBuildingLink(link);
                        building.SetBuildingLink(link);
                        _myLinks.Add(link);

                        // Increment Weapon to Troops links number
                        if (building.GetBuildingType() == BuildingType.Weapon &&
                            targetBuilding.GetBuildingType() == BuildingType.Troops) building.IncrementLinksToTroops();
                        else if (building.GetBuildingType() == BuildingType.Troops &&
                                 targetBuilding.GetBuildingType() == BuildingType.Weapon)
                            targetBuilding.IncrementLinksToTroops();

                        // Increment Weapon to Buffs links number
                        if (building.GetBuildingType() == BuildingType.Weapon &&
                            targetBuilding.GetBuildingType() == BuildingType.Buff) building.IncrementLinksToBuffs();
                        else if (building.GetBuildingType() == BuildingType.Buff &&
                                 targetBuilding.GetBuildingType() == BuildingType.Weapon)
                            targetBuilding.IncrementLinksToBuffs();
                    }
                }
                else
                {
                    Destroy(link.gameObject);
                }
            }
            else
            {
                Destroy(link.gameObject);
            }
        }

        public static bool ValidateLink(Building building1, Building building2)
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

        private IEnumerator CutLinks()
        {
            while (GameManager.State == GameState.Play && !isDestroyEnabled)
            {
                if (TurnsManager.PlayState != PlayState.Summon)
                {
                    yield return null;
                    continue;
                }
                trail.SetActive(Input.GetMouseButton(0) && !_linking);
                if (Input.GetMouseButton(0) && !_linking)
                {
                    var ray = _cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayer))
                    {
                        trail.transform.position = hit.point + Vector3.up * 0.15f;
                        foreach (var link in _myLinks.ToList().Where(link => link.IsBeenCut(hit.point)))
                        {
                            UpdateBuildingsLinksCountAfterLinkCut(link);
                        }
                    }
                }

                yield return null;
            }
        }

        private void UpdateBuildingsLinksCountAfterLinkCut(Link link)
        {
            link.Cut();
            _myLinks.Remove(link);
            Destroy(link.gameObject);
            UpdateBuildingsVisualAfterLinkCut();

            // Decrement Weapon to Buff links number
            if (link._building1.GetBuildingType() == BuildingType.Weapon &&
                link._building2.GetBuildingType() == BuildingType.Buff)
                link._building1.DecrementLinksToBuffs();
            else if (link._building1.GetBuildingType() == BuildingType.Buff &&
                     link._building2.GetBuildingType() == BuildingType.Weapon)
                link._building2.DecrementLinksToBuffs();

            // Decrement Weapon to Troops links number
            if (link._building1.GetBuildingType() == BuildingType.Weapon &&
                link._building2.GetBuildingType() == BuildingType.Troops)
            {
                link._building1.DecrementLinksToTroops();

                if (link._building1.GetLinksToBuffs() > 1 && link._building1.GetLinksToBuffs() >
                    link._building1.GetLinksToTroops())
                {
                    var weaponLinks = link._building1.GetMyLinks();
                    for (var i = weaponLinks.Count - 1; i >= 0; i--)
                    {
                        var item = weaponLinks[i];
                        if (item._building1.GetBuildingType() != BuildingType.Buff &&
                            item._building2.GetBuildingType() != BuildingType.Buff) continue;

                        item.Cut();
                        _myLinks.Remove(item);
                        Destroy(item.gameObject);
                        UpdateBuildingsVisualAfterLinkCut();

                        if (item._building1.GetBuildingType() == BuildingType.Weapon)
                            item._building1.DecrementLinksToBuffs();
                        else item._building2.DecrementLinksToBuffs();

                        break;
                    }
                }
            }
            else if (link._building1.GetBuildingType() == BuildingType.Troops &&
                     link._building2.GetBuildingType() == BuildingType.Weapon)
            {
                link._building2.DecrementLinksToTroops();

                if (link._building2.GetLinksToBuffs() > 1 && link._building2.GetLinksToBuffs() >
                    link._building2.GetLinksToTroops())
                {
                    var weaponLinks = link._building2.GetMyLinks();
                    for (var i = weaponLinks.Count - 1; i >= 0; i--)
                    {
                        var item = weaponLinks[i];
                        if (item._building1.GetBuildingType() != BuildingType.Buff &&
                            item._building2.GetBuildingType() != BuildingType.Buff) continue;

                        item.Cut();
                        _myLinks.Remove(item);
                        Destroy(item.gameObject);
                        UpdateBuildingsVisualAfterLinkCut();

                        if (item._building1.GetBuildingType() == BuildingType.Weapon)
                            item._building1.DecrementLinksToBuffs();
                        else item._building2.DecrementLinksToBuffs();

                        break;
                    }
                }
            }
        }

        private void UpdateBuildingsVisualAfterLinkCut()
        {
            var reachableBuildings = new HashSet<Building>();
            var troopsBuildings = _buildingsOnBoard
                .Where(b => b.GetBuildingType() == BuildingType.Troops && b.IsActive()).ToList();

            foreach (var troopsBuilding in troopsBuildings)
            {
                var queue = new Queue<Building>();
                queue.Enqueue(troopsBuilding);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();

                    if (reachableBuildings.Contains(current)) continue;

                    reachableBuildings.Add(current);

                    foreach (var link in current.GetMyLinks())
                    {
                        var linkedBuilding = link.GetLinkedBuilding(current);
                        if (!reachableBuildings.Contains(linkedBuilding)) queue.Enqueue(linkedBuilding);
                    }
                }
            }

            foreach (var building in _buildingsOnBoard)
                if (!reachableBuildings.Contains(building) && building.GetBuildingType() != BuildingType.Troops)
                    building.SetActive(false);
        }

        public void SetDestroyEnabled()
        {
            isDestroyEnabled = !isDestroyEnabled;

            if (isDestroyEnabled)
            {
                foreach (var building in _buildingsOnBoard)
                    building.Highlight();
            }
            else
            {
                StartCoroutine(SelectBuilding());
                StartCoroutine(CutLinks());
                foreach (var building in _buildingsOnBoard)
                    building.RemoveHighlight();
            }
        }

        private void DestroyBuilding()
        {
            if (!isDestroyEnabled) return;


            if (!Input.GetMouseButtonDown(0)) return;

            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, buildingLayer)) return;

            var buildingToDestroy = _buildingsOnBoard.FirstOrDefault(b => b.transform == hit.transform);

            if (buildingToDestroy == null) return;

            foreach (var link in buildingToDestroy.GetMyLinks().ToList())
            {
                UpdateBuildingsLinksCountAfterLinkCut(link);
            }

            _buildingsOnBoard.Remove(buildingToDestroy);
            buildingToDestroy.GetLandFeedback().PlayFeedbacks();
            buildingToDestroy.GetBuildingPanel().gameObject.SetActive(false);
            var renderers = buildingToDestroy.GetRenderers();
            foreach (var r in renderers)
                r.gameObject.SetActive(false);

            Destroy(buildingToDestroy.gameObject, 0.75f);
            UpdateBuildingsVisualAfterLinkCut();
        }


        private void CheckCursorOverBuilding()
        {
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, buildingLayer))
            {
                Cursor.visible = false;
                hammerCursor.SetActive(true);
            }
            else
            {
                Cursor.visible = true;
                hammerCursor.SetActive(false);
            }
        }

        private void UpdateHammerCursorPosition()
        {
            var mousePosition = Input.mousePosition;
            mousePosition.z = 0f;
            hammerCursor.transform.position = mousePosition;
        }
    }
}