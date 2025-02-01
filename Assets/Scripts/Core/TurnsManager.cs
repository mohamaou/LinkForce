using System.Collections;
using DG.Tweening;
using Players;
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

        public PlayerTeam GetLastTurnWinner() => _lastTurnWinner;

        private void Awake()
        {
            Instance = this;
            PlayState = PlayState.Summon;
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => GameManager.State == GameState.Play);
        }
    }
}