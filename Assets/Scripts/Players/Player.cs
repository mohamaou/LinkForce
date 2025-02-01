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
    public class Player : PlayerManager
    { 
        public static Player Instance { get; private set; }
        [SerializeField] private LayerMask buildingLayer, groundLayer;
        [SerializeField] private GameObject trail;
        [SerializeField] private Sprite hammer;
        [SerializeField] private GameObject hammerCursor;
        [SerializeField] private List<Link> _myLinks = new();
        [SerializeField] private List<Building> _buildingsOnBoard = new();
        private Camera _cam;
        private bool _linking;
       
        public bool isDestroyEnabled = false;
        public List<Building> GetBuildingsOnBoard() => _buildingsOnBoard;

        private void Awake()
        {
            Instance = this;
            _cam = Camera.main;
        }

        
        private IEnumerator Start()
        {
            SetDeck();
            yield return new WaitUntil(() => GameManager.State == GameState.Play);
            
            UIManager.Instance.playPanel.GetSummonButton().onClick.AddListener(SummonButtonClicked);
            UIManager.Instance.playPanel.GetDestroyButton().onClick.AddListener(SetDestroyEnabled);
            StartCoroutine(SelectBuilding());
            StartCoroutine(CutLinks());
            var t = trail;
            trail = Instantiate(t, transform);
        }
        
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
            if (!CoinsManager.HasCoinsToSummon(PlayerTeam.Player1))
            {
                UIManager.Instance.playPanel.ShowNotEnoughGoldEffect();
                return;
            }

            CoinsManager.Instance.UseCoins(PlayerTeam.Player1);
            if(!SummonRandomBuilding()) UIManager.Instance.playPanel.ShowSpaceErrorText();
        }

        private IEnumerator SelectBuilding()
        {
            while (GameManager.State == GameState.Play && !isDestroyEnabled)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var ray = _cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out var hit, buildingLayer))
                        foreach (var building in BuildingsOnBoard)
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

                foreach (var availableBuilding in BuildingsOnBoard)
                    if (ValidateLink(availableBuilding, building) && availableBuilding != building)
                        availableBuilding.Highlight();

                yield return null;
            }

            _linking = false;
            foreach (var availableBuilding in BuildingsOnBoard) availableBuilding.RemoveHighlight();

            if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out var b, Mathf.Infinity, buildingLayer))
            {
                var targetBuilding = b.transform.gameObject.GetComponent<Building>();
                TryToLinkBuildings(building, targetBuilding);
                TryMergeBuildings(building, targetBuilding);
            }
            Destroy(link.gameObject);
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
                        foreach (var link in MyLinks().ToList().Where(link => link.IsBeenCut(hit.point)))
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
            MyLinks().Remove(link);
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
                        MyLinks().Remove(item);
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
                        MyLinks().Remove(item);
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
            var troopsBuildings = BuildingsOnBoard
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

            foreach (var building in BuildingsOnBoard)
                if (!reachableBuildings.Contains(building) && building.GetBuildingType() != BuildingType.Troops)
                    building.SetActive(false);
        }

        public void SetDestroyEnabled()
        {
            isDestroyEnabled = !isDestroyEnabled;

            if (isDestroyEnabled)
            {
                foreach (var building in BuildingsOnBoard)
                    building.Highlight();
            }
            else
            {
                Cursor.visible = false;
                hammerCursor.SetActive(true);
                foreach (var building in BuildingsOnBoard)
                    building.RemoveHighlight();
                StartCoroutine(SelectBuilding());
                StartCoroutine(CutLinks());
            }
        }

        private void DestroyBuilding()
        {
            if (!isDestroyEnabled) return;


            if (!Input.GetMouseButtonDown(0)) return;

            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, buildingLayer)) return;

            var buildingToDestroy = BuildingsOnBoard.FirstOrDefault(b => b.transform == hit.transform);

            if (buildingToDestroy == null) return;

            foreach (var link in buildingToDestroy.GetMyLinks().ToList())
            {
                UpdateBuildingsLinksCountAfterLinkCut(link);
            }

            BuildingsOnBoard.Remove(buildingToDestroy);
            buildingToDestroy.GetLandFeedback().PlayFeedbacks();
            buildingToDestroy.GetBuildingPanel().gameObject.SetActive(false);
            var renderers = buildingToDestroy.GetRenderers();
            foreach (var r in renderers)
                r.gameObject.SetActive(false);

            Destroy(buildingToDestroy.gameObject, 0.75f);
            UpdateBuildingsVisualAfterLinkCut();
            CoinsManager.Instance.AddDestroyReward(PlayerTeam.Player1);
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