using UnityEngine;
using UnityEngine.SceneManagement;


namespace Start_UI
{
   public class MainPanel : MonoBehaviour
   {
      public static MainPanel Instance{get; private set;}
      [SerializeField] private bool loadToturialLevel;
      [SerializeField] private GameObject bottomBar, searchPanel, settingButton, mainPanel;
      
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
   }
}
