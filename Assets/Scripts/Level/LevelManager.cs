using System.Collections;
using Core;
using UI;
using UnityEngine;
using Zombies;

namespace Level
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance {get; private set;}
        [SerializeField] private int openNewDoorWait = 3;
        [SerializeField] private Level[] levels;
        private Level _currentLevel;
        private int _currentRound, _openDoorRound;
        private System.Action<int> _onRoundStart;
        
        
        private void Awake()
        {
            Instance = this;
        }

        private IEnumerator Start()
        {
            _currentLevel = levels[Random.Range(0, levels.Length)];
           // UIManager.Instance.SetRoundCount(_currentRound+1, _currentLevel.GetMaxRoundsCount());
            yield return new WaitUntil(() => GameManager.State == GameState.Play);
            if(!GameManager.Instance.tutorialLevel)ZombiesSpawner.Instance.OpenRandomDoor();
            else ZombiesSpawner.Instance.OpenDoor(1);
            StartCoroutine(StartRound(0));
        }

        public void SetRoundStartsEvents(System.Action<int> onRoundStart) => _onRoundStart = onRoundStart;
        
        private IEnumerator StartRound(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            if (openNewDoorWait <= _openDoorRound)
            {
                _openDoorRound = 0;
                if(!GameManager.Instance.tutorialLevel) ZombiesSpawner.Instance.OpenRandomDoor();
            }
            _currentRound++;
            _openDoorRound++;
            RoundUI.Instance.SetRoundCount(_currentRound);
           // UIManager.Instance.SetRoundCount(_currentRound, _currentLevel.GetMaxRoundsCount());
            _onRoundStart?.Invoke(_currentRound);
            StartCoroutine(ZombiesSpawner.Instance.SpawnZombies(_currentLevel.GetRound(_currentRound-1), () =>
            {
                if (_currentRound < _currentLevel.GetMaxRoundsCount())
                {
                    StartCoroutine(StartRound(5));
                }
            }));
        }

       
    }
}
