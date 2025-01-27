using Cards;
using Core;
using Start_UI;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UI
{
    public class EndGameUI : MonoBehaviour
    {
        public static EndGameUI Instance {get; private set;}
        [SerializeField] private GameObject winPanel, losePanel, drawPanel;
        [SerializeField] private Reward[] rewards;
        [SerializeField] private BuildingCard[] rewardsType;
        [SerializeField] private TextMeshProUGUI[] trophiesText, goldToAddText;
        [SerializeField] private TextMeshProUGUI endGameText;
        private GameResult _gameResult;
        
        
        public void Awake()
        {
            Instance = this;
            winPanel.SetActive(false);
            losePanel.SetActive(false);
            drawPanel.SetActive(false);
            endGameText.gameObject.SetActive(false);
        }

        public void ShowUI(GameResult result, bool deathMatch)
        {
            endGameText.gameObject.SetActive(deathMatch);
            endGameText.text = result switch
            {
                GameResult.Draw => "No more space to play",
                GameResult.Win => "All opponent troops are slayed",
                GameResult.Lose => "All your troops are slayed",
            };
            endGameText.color = result switch
            {
                GameResult.Draw => Color.white,
                GameResult.Win => Color.green,
                GameResult.Lose => Color.red,
            };
            _gameResult = result;
            switch (result)
            {
                case GameResult.Win:
                    winPanel.SetActive(true);
                    break;
                case GameResult.Lose:
                    losePanel.SetActive(true);
                    break;
                case GameResult.Draw:
                    drawPanel.SetActive(true);
                    break;
            }
            SetReward();
        }

        private void SetReward()
        {
            var trophies = PlayerPrefs.GetInt("Trophies", 0);
            if(_gameResult == GameResult.Win) trophies += 30;
            if(_gameResult == GameResult.Lose) trophies -= 30;
            PlayerPrefs.SetInt("Trophies", trophies);
            for (int i = 0; i < trophiesText.Length; i++)
            {
                if (_gameResult == GameResult.Win) trophiesText[i].text = "+30";
                if (_gameResult == GameResult.Lose) trophiesText[i].text = "-30";
                if (_gameResult == GameResult.Draw) trophiesText[i].text = "+0";
            }

            if (Currencies.Instance != null)
            {
                Currencies.Instance.AddCoins(_gameResult switch 
                { 
                    GameResult.Win => 50, GameResult.Lose => 25, GameResult.Draw => 15, 
                });
            }
            for (int i = 0; i < goldToAddText.Length; i++)
            {
                if (_gameResult == GameResult.Win) goldToAddText[i].text = "+50";
                if(_gameResult == GameResult.Lose) goldToAddText[i].text = "+25";
                if(_gameResult == GameResult.Draw) goldToAddText[i].text = "+15";
            }

            for (int i = 0; i < rewards.Length; i++)
            {
                if (!rewards[i].IsFull())
                {
                    int rewardCount = Random.Range(3, 6);
                    var rewardLoots = new RewardLoot[rewardCount];

                    rewards[i].full = true;
                    for (int j = 0; j < rewardCount; j++)
                    {
                        if (j == 0)
                        {
                            rewardLoots[j] = new RewardLoot(Random.Range(100, 120), true);
                        }
                        else
                        {
                            rewardLoots[j] = new RewardLoot(Random.Range(5, 10), false);
                        }
                    }
                    rewards[i].SetReward(rewardLoots);
                    break;
                }
            }
        }
    }
}
