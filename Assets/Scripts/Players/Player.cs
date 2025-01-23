using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using Level;
using Troops;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Players
{
    public enum PlayerTeam
    {
        Player1, Player2
    }
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }
        [SerializeField] private Building[] buildings;
        [SerializeField] private LayerMask buildingLayer, groundLayer;
        [SerializeField] private GameObject trail;
        private List<Building> _selectedBuilding = new List<Building>(), _buildingsOnBoard = new List<Building>();
        private List<Link> _myLinks = new List<Link>();
        private Camera _cam;
        private bool _linking;
        
        
        private void Awake()
        {
            Instance = this;
            _cam = Camera.main;
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => GameManager.State == GameState.Play);
            UIManager.Instance.playPanel.GetSummonButton().onClick.AddListener(SummonButtonClicked);
            StartCoroutine(SelectBuilding());
            StartCoroutine(CutLinks());
            var t = trail;
            trail = Instantiate(t,transform);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space)) SummonButtonClicked();
        }

        private void SummonButtonClicked()
        {
            for (int i = 0; i < buildings.Length; i++)
            {
                var targetBuilding = buildings[(Random.Range(0, buildings.Length) + i) % buildings.Length];
                var summonPos = Board.Instance.GetRandomBoardPoint(PlayerTeam.Player1, targetBuilding.GetBuildingType());
        
                if (summonPos != Vector3.zero)
                {
                    var b = Instantiate(targetBuilding, summonPos, Quaternion.identity, transform);
                    b.Set(PlayerTeam.Player1);
                    _buildingsOnBoard.Add(b);
                    return;
                }
            }
            UIManager.Instance.playPanel.ShowSpaceErrorText();
        }

        private IEnumerator CutLinks()
        {
            while (GameManager.State == GameState.Play)
            {
                trail.SetActive(Input.GetMouseButton(0) && !_linking);
                if (Input.GetMouseButton(0) && !_linking)
                {
                    var ray = _cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out var hit, Mathf.Infinity,groundLayer))
                    {
                        trail.transform.position = hit.point+ Vector3.up * 0.15f;
                        foreach (var link in _myLinks.ToList().Where(link => link.IsBeenCut(hit.point)))
                        {
                            link.Cut();
                            _myLinks.Remove(link);
                            Destroy(link.gameObject);
                        }
                    }
                }
                yield return null;
            }
        }

        private IEnumerator SelectBuilding()
        {
            while (GameManager.State == GameState.Play)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var ray = _cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out var hit, buildingLayer))
                    {
                        foreach (var building in _buildingsOnBoard)
                        {
                            if (building.transform == hit.transform)
                            {
                                yield return LinkBuildings(building);
                            }
                        }
                    }
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
                if (Physics.Raycast(ray, out var hit, Mathf.Infinity,groundLayer))
                {
                    link.ShowLink(building.transform.position, hit.point);
                }

                foreach (var availableBuilding in _buildingsOnBoard)
                {
                    if(ValidLink(availableBuilding,building) && availableBuilding != building) availableBuilding.Highlite();
                }
                yield return null;
            }
            _linking = false;
            foreach (var availableBuilding in _buildingsOnBoard)
            {
                availableBuilding.RemoveHighlite();
            }
            if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out var b, Mathf.Infinity, buildingLayer))
            {
                link.ShowLink(building.transform.position, b.transform.position);
                var targetBuilding = b.transform.gameObject.GetComponent<Building>();
                if (ValidLink(targetBuilding, building))
                {
                    link.SetBuildings(building, targetBuilding);
                    targetBuilding.SetBuildingLink(link);
                    building.SetBuildingLink(link);
                    _myLinks.Add(link);
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
        private bool ValidLink(Building building1, Building building2)
        {
            if (building1.GetBuildingType() == BuildingType.Troops && building2.GetBuildingType() == BuildingType.Weapon) return true;
            if (building2.GetBuildingType() == BuildingType.Troops && building1.GetBuildingType() == BuildingType.Weapon) return true;
            if (building2.GetBuildingType() == BuildingType.Weapon && building1.GetBuildingType() == BuildingType.Buff) return true;
            if (building1.GetBuildingType() == BuildingType.Weapon && building2.GetBuildingType() == BuildingType.Buff) return true;
            if (building1.GetBuildingType() == building2.GetBuildingType())
            {
                return building1.GetId() == building2.GetId();
            }
            return false;
        }
        
    }
}
