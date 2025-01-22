using System;
using UnityEngine;
using UnityEngine.UI;

namespace Start_UI
{
    public class MainMenuTutorial : MonoBehaviour
    {
        public static MainMenuTutorial Instance { get; private set; }
        [SerializeField] private Button hand;
        [SerializeField] private Button chest, cardsPanel;
        private bool _stop;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            var first = PlayerPrefs.GetInt("First",1) == 1;
            if(!first) gameObject.SetActive(false);
            hand.onClick.AddListener(() =>
            {
                _stop = true;
                chest.onClick.Invoke();
                hand.onClick.RemoveAllListeners();
                hand.gameObject.SetActive(false);
                ChestOpened();
            });
        }

        private void Update()
        {
            if (_stop) return;
            hand.transform.position = chest.transform.position;
        }

        public void ChestOpened()
        {
            hand.gameObject.SetActive(true);
            hand.transform.position = cardsPanel.transform.position;
            hand.onClick.AddListener(() =>
            {
                cardsPanel.onClick.Invoke();
                gameObject.SetActive(false);
                PlayerPrefs.SetInt("First",0);
            });
        }
    }
}
