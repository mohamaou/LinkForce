using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Level;
using Troops;
using UI;
using UnityEngine;
using Zombies;
using Random = UnityEngine.Random;

namespace Players
{
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }
        [SerializeField] private Building[] buildings;
        private List<Building> _selectedBuilding = new List<Building>(), _buildingsOnBoard = new List<Building>();
        
        private void Awake()
        {
            Instance = this;
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
            var cam = Camera.main;
            yield return new WaitUntil(() => GameManager.State == GameState.Play);
           // while (GameManager.State == GameState.Play)
            {
                var ray = cam.ScreenPointToRay(Input.mousePosition);
               // if(Input.GetMouseButtonDown(0))
            }
        }
        
    }
}
