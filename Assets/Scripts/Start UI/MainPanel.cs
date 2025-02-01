using UnityEngine;
using UnityEngine.SceneManagement;


namespace Start_UI
{
    public class MainPanel : MonoBehaviour
    {
        public static MainPanel Instance { get; private set; }
        [SerializeField] private bool loadToturialLevel;
        [SerializeField] private GameObject bottomBar, searchPanel, settingButton, mainPanel;
        [SerializeField] private Reward reward;

        private void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 120;
            settingButton.SetActive(true);
            mainPanel.SetActive(!false);
            bottomBar.SetActive(!false);
            searchPanel.SetActive(!true);

            if (PlayerPrefs.GetInt("First_Load", 1) == 1 && loadToturialLevel)
            {
                SceneManager.LoadScene(2);
            }
        }

        public void Play()
        {
            settingButton.SetActive(false);
            mainPanel.SetActive(false);
            bottomBar.SetActive(false);
            searchPanel.SetActive(true);
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) SetReward();
        }

        private void SetReward()
        {
            int rewardCount = Random.Range(3, 6);
            var rewardLoots = new RewardLoot[rewardCount];
            reward.full = true;
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

            reward.SetReward(rewardLoots);
            SceneManager.LoadScene(0);
        }
    }
}