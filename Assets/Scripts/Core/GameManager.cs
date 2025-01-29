using System.Collections.Generic;
using AI;
using DG.Tweening;
using Players;
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
        public bool fastGame, tutorialLevel, showPlayerProfile;
        private float _timer;
        private float _fpsTimer = 0.0f;
        private int _fpsCount = 0;  
        private int _fpsDisplay = 0;
        private bool _player1NotEnoughSpace, _player2NotEnoughSpace;
        public float timeRemaining = 0;

        
        private void Awake()
        {
            Instance = this;
            Camera = Camera.main;
            State = GameState.Start;
            Application.targetFrameRate = 120;
            Time.timeScale = fastGame ? 4f : 1f;
            Level = PlayerPrefs.GetInt("Level", 1);
            timeRemaining = GameManager.Instance.levels[Level - 1].summonTime;
        }

        private void Start()
        { 
            TinySauce.OnGameStarted(levelNumber:Level);
        }

        private void Update()
        {
            Keyboard();
            if (State == GameState.Play && TurnsManager.PlayState == PlayState.Summon) CountDown();
        }

        private void CountDown()
        {
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
            TurnsManager.PlayState = PlayState.Battle;
            UIManager.Instance.ShowBattleUI();
            print("Start Fight");
        }
        
        private void Keyboard()
        {
            if (Input.GetKeyDown(KeyCode.S)) Time.timeScale = Time.timeScale == 1 ? 0.2f : 1;
            if (Input.GetKeyDown(KeyCode.F)) Time.timeScale = Time.timeScale == 1 ? 0 : 1;
            if (Input.GetKeyDown(KeyCode.D)) Time.timeScale = Time.timeScale == 1 ? 4 : 1;
            if (Input.GetKeyDown(KeyCode.R)) Restart();
            if (Input.GetKeyDown(KeyCode.H)) SceneManager.LoadScene(0);
            if (Input.GetKeyDown(KeyCode.N)) GameEnd(GameResult.Win);
            if(Input.GetKeyDown(KeyCode.G)) CheckIfGameEnds(PlayerTeam.Player2);
        }
  
        private void OnGUI()
        {
            return;
            // Timer Display
            int totalSeconds = Mathf.FloorToInt(_timer);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            string timeText = $"{minutes:D2}:{seconds:D2}";

            GUIStyle timerStyle = new GUIStyle
            {
                fontSize = Mathf.RoundToInt(Screen.height * 0.03f), // 3% of screen height
                normal = { textColor = Color.black }, // Blue text
                fontStyle = FontStyle.Bold // Thick font
            };

            Rect timerPosition = new Rect(Screen.width * 0.01f, Screen.height * 0.01f, Screen.width, Screen.height);
            GUI.Label(timerPosition, $"{timeText}", timerStyle);

            // FPS Display
            string fpsText = $"{_fpsDisplay}";

            GUIStyle fpsStyle = new GUIStyle
            {
                fontSize = Mathf.RoundToInt(Screen.height * 0.03f), // 3% of screen height
                normal = { textColor = Color.green }, // Green text
                fontStyle = FontStyle.Bold // Thick font
            };

            Rect fpsPosition = new Rect(Screen.width * 0.88f, Screen.height * 0.01f, Screen.width, Screen.height);
            GUI.Label(fpsPosition, fpsText, fpsStyle);
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
        public void CheckIfGameEnds(PlayerTeam playerTeam)
        {
            var spaceAvailable = playerTeam == PlayerTeam.Player1? _player1NotEnoughSpace: _player2NotEnoughSpace;
            if (!spaceAvailable) return;
           // var troopsCount = playerTeam == PlayerTeam.Player1
              //  ? Player.Instance.GetTroopsCount()
              //  : Bot.Instance.GetTroopCount();
           // if (troopsCount == 0)
                GameEnd(playerTeam == PlayerTeam.Player1 ? GameResult.Lose : GameResult.Win, false, true);
            //if (!Board.Instance.IsPathConnected() && _player1NotEnoughSpace && _player2NotEnoughSpace)
            {
                GameEnd(GameResult.Draw,false, true);
            }
        }

        public void GameEnd(GameResult result, bool tutorial = false, bool deathMatch = false)
        {
            if (State != GameState.Play) return;
            Result = result;
            State = GameState.GameOver;
            //Bot.Instance.AdjustDifficulty(result == GameResult.Win);
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