using System;
using System.Collections;
using DG.Tweening;
using Players;
using UI;
using UnityEngine;


namespace Core
{
    public enum PlayState
    {
        Summon,
        Wait,
        Battle
    }
    
    public class TurnsManager : MonoBehaviour
    {
        public static TurnsManager Instance {get; private set;}
        public static PlayState PlayState;
        private PlayerTeam _playerTurn;
        private PlayerTeam _lastTurnWinner;
        
        private void Awake()
        {
            Instance = this;
            PlayState = PlayState.Summon;
        }

        private void Start()
        {
            CoinsManager.Instance.InitializeTurnCoins(_lastTurnWinner, true);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.V)) EndBattle(PlayerTeam.Player1);
        }

        public void EndBattle(PlayerTeam team)
        {
            CoinsManager.Instance.InitializeTurnCoins(_lastTurnWinner, false);
            PlayState = PlayState.Summon;
            if (team == PlayerTeam.Player2) UIManager.Instance.playersHealth.PlayerTakesDamage();
            else UIManager.Instance.playersHealth.EnemyTakesDamage();
            UIManager.Instance.playPanel.SetPlayUI(PlayState);
        }
    }
}