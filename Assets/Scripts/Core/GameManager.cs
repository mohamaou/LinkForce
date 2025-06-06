using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Models;
using Players;
using Troops;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;



public static class IntExtensions
{
    public static string ToShortString(this int number)
    {
        if (number >= 1000000000)
            return (number / 1000000000f).ToString("0.#") + "B";
        if (number >= 1000000)
            return (number / 1000000f).ToString("0.#") + "M";
        if (number >= 10000)
            return (number / 1000f).ToString("0.#") + "K";
        return number.ToString();
    }
}


namespace Core
{
    public enum GameState
    {
        Start, Play, GameOver
    }

    public enum GameResult
    {
        Win,Lose,Draw
    }

    public enum Age
    {
        Stone, Egypt, Roman, Kingdom, Samurai, Modern, Apocalypse, Cyber , Null
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public static GameState State;
        public static GameResult Result;
        public static Camera Camera;
        public static int Level;
        public List<Level> levels = new List<Level>();
        public Color player1Color, player2Color;
        public bool fastGame, showPlayerProfile, tutorialLevel;
        private float _fpsTimer = 0.0f;
        private int _fpsCount = 0;  
        private int _fpsDisplay = 0;
        private bool _player1NotEnoughSpace, _player2NotEnoughSpace;
        private float timeRemaining = 0;
        private Level currentLevel;
        
        private void Awake()
        {
            Instance = this;
            Camera = Camera.main;
            State = GameState.Start;
            Application.targetFrameRate = 120;
            Time.timeScale = fastGame ? 4f : 1f;
            Level = PlayerPrefs.GetInt("Level", 1);
            currentLevel = levels[Level - 1];
            timeRemaining = currentLevel.summonTime;
        }

        private void Start()
        {
            CoinsManager.Instance.Initialize(currentLevel);
            TinySauce.OnGameStarted(levelNumber:Level);
            if(tutorialLevel) UIManager.Instance.playPanel.HideTimer();
        }

        private void Update()
        {
            Keyboard();
            if (State == GameState.Play && TurnsManager.PlayState == PlayState.Summon) CountDown();
        }

        private void CountDown()
        {
            if (tutorialLevel) return;
            if (timeRemaining <= 0)
            {
                StartBattle();
                return;
            }

            timeRemaining -= Time.deltaTime;

            var minutes = Mathf.FloorToInt(timeRemaining / 60);
            var seconds = Mathf.FloorToInt(timeRemaining % 60);
            if (minutes < 0 && seconds < 0) return;
            UIManager.Instance.playPanel.GetTimerText().text = $"{minutes}:{seconds:00}";
        }
        

        public void StartBattle()
        {
            StartCoroutine(WaitForEnemy());
        }

        private IEnumerator WaitForEnemy()
        {
            Player.Instance.SetDestroyDisabled();
            TurnsManager.PlayState = PlayState.Wait;
            UIManager.Instance.playPanel.SetPlayUI(PlayState.Wait);
            yield return new WaitUntil(() => Bot.Instance.IsReady());
            bool done = false;
            Board.Instance.BoardMovement(PlayState.Battle, () => done = true);
            yield return new WaitUntil(() => done);
            timeRemaining = currentLevel.summonTime;
            TurnsManager.Instance.BattleStarted();
            foreach (var b in Player.Instance.GetBuildingsOnBoard) 
                if (b.GetBuildingType() == BuildingType.Troops) yield break;
            yield return new WaitForSeconds(2f);
            TroopsFightingManager.Instance.BattleEnds(PlayerTeam.Player2);
        }

        
        private void Keyboard()
        {
            if (Input.GetKeyDown(KeyCode.S)) Time.timeScale = Time.timeScale == 1 ? 0.2f : 1;
            if (Input.GetKeyDown(KeyCode.F)) Time.timeScale = Time.timeScale == 1 ? 0 : 1;
            if (Input.GetKeyDown(KeyCode.D)) Time.timeScale = Time.timeScale == 1 ? 4 : 1;
            if (Input.GetKeyDown(KeyCode.R)) Restart();
            if (Input.GetKeyDown(KeyCode.H)) SceneManager.LoadScene(0);
            if (Input.GetKeyDown(KeyCode.N)) GameEnd(GameResult.Win);
        }
        
        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        public void QuitGame()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(0);
        }
        public void NotEnoughSpace(PlayerTeam team)
        {
            if(team == PlayerTeam.Player1) _player1NotEnoughSpace = true;
            if(team == PlayerTeam.Player2) _player2NotEnoughSpace = true;
        }

        public void GameEnd(GameResult result, bool tutorial = false, bool deathMatch = false)
        {
            if (State != GameState.Play) return;
            Result = result;
            State = GameState.GameOver;
            Bot.Instance.AdjustDifficulty(result == GameResult.Win);
            if (result == GameResult.Win)
            {
                PlayerPrefs.SetInt("Level", Level++);
            }
            TinySauce.OnGameFinished(result == GameResult.Win,0,PlayerPrefs.GetInt("Level", 0));
            PlayerPrefs.SetInt("First_Load", 0);
            DOVirtual.DelayedCall(1f,()=>
            {
                EndGameUI.Instance.ShowUI(Result, deathMatch);
                UIManager.Instance.GameEnd(Result == GameResult.Win);
            },false);
        }
    }
}