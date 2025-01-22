using UnityEngine;

namespace Start_UI
{
    public class PlayersProfiles : MonoBehaviour
    { 
        public static PlayersProfiles Instance { get; private set; }
        private string _player1Name, _player2Name; 
        private Sprite _player1Sprite, _player2Sprite; 
        private int _player1Trophies, _player2Trophies;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }
        
        public void SetPlayer1Name(string playerName)=> _player1Name = playerName;
        public void SetPlayer2Name(string playerName)=> _player2Name = playerName;
        public void SetPlayer1Sprite(Sprite playerSprite) => _player1Sprite = playerSprite;
        public void SetPlayer2Sprite(Sprite playerSprite) => _player2Sprite = playerSprite;
        public void SetPlayer1Trophies(int playerTrophies) => _player1Trophies = playerTrophies;
        public void SetPlayer2Trophies(int playerTrophies) => _player2Trophies = playerTrophies;
        
        public string Player1Name => _player1Name;
        public string Player2Name => _player2Name;
        public Sprite Player1Sprite => _player1Sprite;
        public Sprite Player2Sprite => _player2Sprite;
        public int Player1Trophies => _player1Trophies;
        public int Player2Trophies => _player2Trophies;
    }
}
