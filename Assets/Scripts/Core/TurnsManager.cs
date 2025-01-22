using System.Collections;
using AI;
using DG.Tweening;
using Players;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Zombies;

namespace Core
{
    public enum TurnStats
    {
        Draw, Rolling, TroopSelection, TroopPlay, EndPhase
    }
    public class TurnsManager : MonoBehaviour
    {
       public static TurnsManager Instance{get; private set;}
       public static TurnStats TurnStats;
       private PlayerTeam _playerTurn;
       

       private void Awake()
       {
           Instance = this;
           TurnStats = TurnStats.Draw;
       }
       
       public void SetFirstPlayer(PlayerTeam firstPlayer)
       {
           _playerTurn = firstPlayer;
       }
       public bool IsPlayerTurn(PlayerTeam player) => _playerTurn == player;
       

       private IEnumerator Start()
       {
          // UIManager.Instance.turnUI.SetEndTurnButtonListener(EndTurn);
           yield return new WaitUntil(() => GameManager.State == GameState.Play);
           DrawDice();
       }

       private void DrawDice()
       {
          // if(_playerTurn == PlayerTeam.Player1) Player.Instance.DrawDice();
           if(_playerTurn == PlayerTeam.Player2) Bot.Instance.DrawDice();
           TurnStats = TurnStats.Rolling;
       }

       public void DiceRolled()
       {
           TurnStats = TurnStats.TroopSelection;
       }
       
       public void StartTroopsPlay()
       {
           TurnStats = TurnStats.TroopPlay;
           if (_playerTurn == PlayerTeam.Player1)
           {
              // UIManager.Instance.turnUI.EndTurnButtonSetActive(true);
              // Player.Instance.StartTroopPlay();
           }
       }

       public void EndTurn()
       {
           _playerTurn = _playerTurn == PlayerTeam.Player1 ? PlayerTeam.Player2 : PlayerTeam.Player1;
           TurnStats = TurnStats.Draw;
          // UIManager.Instance.turnUI.EndTurnButtonSetActive(false);
           DOVirtual.DelayedCall(1.5f, DrawDice,false);
           Bot.Instance.TurnEnd();
          // Player.Instance.TurnEnd();
          // UIManager.Instance.turnUI.TurnEnds(_playerTurn);
       }

       private void Update()
       {
         if(Input.GetKeyDown(KeyCode.Space) && TurnStats == TurnStats.TroopPlay) EndTurn();
       }
    }
}
