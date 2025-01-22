using System.Collections;
using System.Collections.Generic;
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
        private List<Building> _selectedBuilding = new List<Building>(), _buildingsOnBoard = new List<Building>();
        private Camera _cam;
        
        
        private void Awake()
        {
            Instance = this;
            _cam = Camera.main;
        }

        private void Start()
        {
            UIManager.Instance.playPanel.GetSummonButton().onClick.AddListener(SummonButtonClicked);
            StartCoroutine(SelectBuilding());
        }

        private void SummonButtonClicked()
        {
            var summonPos = Board.Instance.GetRandomBoardPoint(PlayerTeam.Player1);
            if (summonPos == Vector3.zero)
            {
                UIManager.Instance.playPanel.ShowSpaceErrorText();
                return;
            }
            var b = Instantiate(buildings[Random.Range(0, buildings.Length)], summonPos, Quaternion.identity,transform);
            b.Set(PlayerTeam.Player1);
            _buildingsOnBoard.Add(b);
        }

        private IEnumerator SelectBuilding()
        {
            
            yield return new WaitUntil(() => GameManager.State == GameState.Play);
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
            var link = building.CreateLink();
            link.SetLink(PlayerTeam.Player1);
            while (Input.GetMouseButton(0))
            {
                var ray = _cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, Mathf.Infinity,groundLayer))
                {
                    link.ShowLink(building.transform.position, hit.point);
                }
                yield return null;
            }

            if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out var b, Mathf.Infinity, buildingLayer))
            {
                link.ShowLink(building.transform.position, b.transform.position);
                var targetBuilding = b.transform.gameObject.GetComponent<Building>();
                if (ValidLink(targetBuilding, building))
                {
                    link.SetBuildings(building, targetBuilding);
                    building.SetBuildingLink(link);
                    targetBuilding.SetBuildingLink(link);
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
