using System;
using System.Collections;
using DG.Tweening;
using Players;
using Troops;
using UI;
using Unity.VisualScripting;
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
        private int _turnCount;
        
        private void Awake()
        {
            Instance = this;
            PlayState = PlayState.Summon;
        }

        private void Start()
        {
            CoinsManager.Instance.InitializeTurnCoins(_lastTurnWinner, true);
        }

        public void BattleStarted()
        {
            PlayState = PlayState.Battle;
            UIManager.Instance.playPanel.SetPlayUI(PlayState.Battle);
            TroopsFightingManager.Instance.BattleStart();
            Time.timeScale = 1.5f;
        }
        public void EndBattle(PlayerTeam winingPlayer)
        {
            _turnCount++;
            Time.timeScale = 1f;
            CoinsManager.Instance.InitializeTurnCoins(winingPlayer, false);
            PlayState = PlayState.Summon;
            if (winingPlayer == PlayerTeam.Player2) UIManager.Instance.playersHealth.PlayerTakesDamage();
            else UIManager.Instance.playersHealth.EnemyTakesDamage();
            UIManager.Instance.playPanel.ShowTurnResult(winingPlayer, _turnCount);
            UIManager.Instance.playPanel.SetPlayUI(PlayState);
        }
    }
}