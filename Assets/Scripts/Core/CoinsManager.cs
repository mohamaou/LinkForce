using System;
using Players;
using UI;
using UnityEngine;

namespace Core
{
    public class CoinsManager : MonoBehaviour
    {
        public static CoinsManager Instance {get; private set;}
        [SerializeField] private int[] coinsToAddEachTurn;
        [SerializeField] private int mergeReward;
        private int _player1coins, _player2coins, _turnIndex; 
            
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            TurnStart();
        }

        public void TurnStart()
        {
            _player1coins = _player2coins = coinsToAddEachTurn[_turnIndex];
            UIManager.Instance.playPanel.UpdateAvailableCoins(_player1coins);
            _turnIndex++;
        }
        public void AddMergeReward(PlayerTeam team)
        {
            if (team == PlayerTeam.Player1)
            {
                _player1coins += mergeReward;
                UIManager.Instance.playPanel.UpdateAvailableCoins(_player1coins);
            }
            if(team == PlayerTeam.Player2) _player2coins += mergeReward;
        }

        public int GetCoinsAmount(PlayerTeam team) => team == PlayerTeam.Player1 ? _player1coins : _player2coins;

        public void UseCoins(int amount, PlayerTeam team)
        {
            if (team == PlayerTeam.Player1)
            {
                _player1coins -= amount;
                UIManager.Instance.playPanel.UpdateAvailableCoins(_player1coins);
            }
            if(team == PlayerTeam.Player2) _player2coins -= amount;
        }
    }
}
