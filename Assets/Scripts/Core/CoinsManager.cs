using System;
using Players;
using UI;
using UnityEngine;

namespace Core
{
    public class CoinsManager : MonoBehaviour
    {
        public static CoinsManager Instance {get; private set;}
        private int _player1coins, _player2coins, _turnIndex;
        private int coinsPerTurn;
        private int coinsPerTurnWin;
        private int coinsPerTurnLoss;
        private int coinsPerMerge;
        private int coinsPerSpawn;
        private int coinsPerRefund;

        public int GetCoinsPerRefund() => coinsPerRefund;
        
        private void Awake()
        {
            Instance = this;
        }

        public void Initialize(Level level)
        {
            coinsPerTurn = level.coinsPerTurn;
            coinsPerTurnWin = level.coinsPerTurnWin;
            coinsPerTurnLoss = level.coinsPerTurnLoss;
            coinsPerMerge = level.coinsPerMerge;
            coinsPerSpawn = level.coinsPerSpawn;
            coinsPerRefund = level.coinsPerRefund;
        }

        public bool HasCoinsToSummon(PlayerTeam player)
        {
            if (player == PlayerTeam.Player1)
                return _player1coins >= coinsPerSpawn;

            return _player2coins >= coinsPerSpawn;
        }

        public void InitializeTurnCoins(PlayerTeam lastTurnWinner, bool isFirstTurn = false)
        {
            var player1Award = isFirstTurn ? 0 :
                lastTurnWinner == PlayerTeam.Player1 ? coinsPerTurnWin : coinsPerTurnLoss;
            var player2Award = isFirstTurn ? 0 :
                lastTurnWinner == PlayerTeam.Player2 ? coinsPerTurnWin : coinsPerTurnLoss;

            // playerCoins = rest from turn + win/loss award + turn fixed amount
            _player1coins = _player1coins + player1Award + coinsPerTurn;
            _player2coins = _player2coins + player2Award + coinsPerTurn;

            UIManager.Instance.playPanel.UpdateAvailableCoins(_player1coins);
            _turnIndex++;
        }

        public void UseCoins(PlayerTeam team)
        {
            if (team == PlayerTeam.Player1)
            {
                _player1coins -= coinsPerSpawn;
                UIManager.Instance.playPanel.UpdateAvailableCoins(_player1coins, !HasCoinsToSummon(PlayerTeam.Player1));
            }

            if (team == PlayerTeam.Player2) _player2coins -= coinsPerSpawn;
        }

        public void AddDestroyReward(PlayerTeam team)
        {
            if (team == PlayerTeam.Player1)
            {
                _player1coins += coinsPerRefund;
                UIManager.Instance.playPanel.UpdateAvailableCoins(_player1coins, !HasCoinsToSummon(PlayerTeam.Player1));
            }

            if (team == PlayerTeam.Player2) _player2coins += coinsPerRefund;
        }

        public void AddMergeReward(PlayerTeam team)
        {
            if (team == PlayerTeam.Player1)
            {
                _player1coins += coinsPerMerge;
                UIManager.Instance.playPanel.UpdateAvailableCoins(_player1coins, !HasCoinsToSummon(PlayerTeam.Player1));
            }

            if (team == PlayerTeam.Player2) _player2coins += coinsPerMerge;
        }
        
        public int GetCoinsAmount(PlayerTeam team) => team == PlayerTeam.Player1 ? _player1coins : _player2coins;

   
    }
}