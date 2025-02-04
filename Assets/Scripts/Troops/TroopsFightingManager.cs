using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using DG.Tweening;
using Models;
using Players;
using UI;
using UnityEngine;

namespace Troops
{
    public class TroopsFightingManager : MonoBehaviour
    {
        public static TroopsFightingManager Instance { get; private set; }
        private List<Troop> _player1Troop = new(), _player2Troop = new();
        private float _player1Health, _player2Health;
        private int _player1Wins, _player2Wins;
        
        private void Awake()
        {
            Instance = this;
        }

        public void AddTroop(Troop troop, PlayerTeam team)
        {
            if (team == PlayerTeam.Player1)
            {
                _player1Health += troop.GetHealth();
                _player1Troop.Add(troop);
                troop.SetDeathEvent(()=>  _player1Troop.Remove(troop));
            }
            else
            {
                _player2Health += troop.GetHealth();
                _player2Troop.Add(troop);
                troop.SetDeathEvent(()=>  _player2Troop.Remove(troop));
            }
            
        }

        public void BattleStart()
        {
            StartCoroutine(BattleState());
        }
        

        private IEnumerator BattleState()
        {
            yield return new WaitUntil(() => _player1Troop.Count > 0 && _player2Troop.Count > 0);
            while ( _player1Troop.Count > 0  && _player2Troop.Count > 0)
            {
                _player1Troop.RemoveAll(t => t == null);
                _player2Troop.RemoveAll(t => t == null);
                var player1CurrentHealth = 0f; 
                var player2CurrentHealth = 0f;
                foreach (var troop in _player1Troop)
                {
                    player1CurrentHealth += troop.GetHealth();
                }

                foreach (var troop in _player2Troop)
                {
                    player2CurrentHealth += troop.GetHealth();
                }
                UIManager.Instance.battlePanel.SetPlayer1Health(player1CurrentHealth/ _player1Health);
                UIManager.Instance.battlePanel.SetPlayer2Health(player2CurrentHealth/ _player2Health);
                yield return null;
            }

            BattleEnds(_player2Troop.Count > 0 ? PlayerTeam.Player2 : PlayerTeam.Player1);
        }
        
        public void BattleEnds(PlayerTeam winingPlayer)
        {
            Board.Instance.BoardMovement(PlayState.Summon);
            DOVirtual.DelayedCall(2f, () =>
            {
                TurnsManager.Instance.EndBattle(winingPlayer);
            });
            foreach (var troop in _player1Troop)
            {
                troop.transform.DOScale(Vector3.zero, 0.4f ).SetEase(Ease.InBounce).OnComplete(()=> troop.Death(false));
            }
            foreach (var troop in _player2Troop)
            {
                troop.transform.DOScale(Vector3.zero, 0.4f ).SetEase(Ease.InBounce).OnComplete(()=> troop.Death(false));
            }
            _player1Troop.Clear();
            _player2Troop.Clear();
            _player1Health = _player2Health = 0;
            if (winingPlayer == PlayerTeam.Player1) _player1Wins++;
            if (winingPlayer == PlayerTeam.Player2) _player2Wins++;
            if(_player1Wins >= 4) GameManager.Instance.GameEnd(GameResult.Win);
            if (_player2Wins >= 4) GameManager.Instance.GameEnd(GameResult.Lose);
        }
    }
}