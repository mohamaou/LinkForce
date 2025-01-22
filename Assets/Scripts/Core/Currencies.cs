using UnityEngine;
using TMPro;
using System;
using Start_UI;

namespace Core
{
    public class Currencies : MonoBehaviour
    {
        public static Currencies Instance {get; private set;}
        private int _gear, _coins, _xp;
        public event Action OnCurrencyChanged, CoinsChanged;
        [SerializeField] private Reward[] rewards;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                for (int i = 0; i < rewards.Length; i++)
                {
                    rewards[i].LoadData();
                }
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            
        }

        private void Start()
        {
            LoadCurrencies();
        }
        private void LoadCurrencies()
        {
            _gear = PlayerPrefs.GetInt("Gear", 0);
            _coins = PlayerPrefs.GetInt("Coins", 0);
            _xp = PlayerPrefs.GetInt("XP", 0);
            OnCurrencyChanged?.Invoke();
        }
        public void SetCoinsChangeEvent( Action coinsChanged) => CoinsChanged += coinsChanged;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                AddGear(100);
                AddCoins(100);
            }
        }

        public void UpdateCurrencyTexts(TextMeshProUGUI gearText, TextMeshProUGUI coinsText, TextMeshProUGUI xpText)
        {
            if(gearText != null) gearText.text = _gear.ToString();
            if(coinsText != null) coinsText.text = _coins.ToString();
            if(xpText != null) xpText.text =  $"XP {_xp}";
            OnCurrencyChanged += () =>
            {
                if(gearText != null) gearText.text = _gear.ToString();
                if(coinsText != null) coinsText.text = _coins.ToString();
                if(xpText != null)xpText.text = $"XP {_xp}";
            };
        }
        
      
        public void AddGear(int amount)
        {
            _gear += amount;
            SaveCurrencies();
            OnCurrencyChanged?.Invoke();
        }
        public void AddCoins(int amount)
        {
            _coins += amount;
            OnCurrencyChanged?.Invoke();
            CoinsChanged?.Invoke();
            SaveCurrencies();
        }
        public void AddXP(int amount)
        {
            _xp += amount;
            OnCurrencyChanged?.Invoke();
            SaveCurrencies();
        }

   
        public bool IsEnoughGear(int amount) => _gear >= amount;
        public bool IsEnoughCoins(int amount) => _coins >= amount;
        public bool IsEnoughXP(int amount) => _xp >= amount;

        public int GetGear() => _gear;
        public int GetCoins() => _coins;
        public int GetXp() => _xp;

      
        private void SaveCurrencies()
        {
            PlayerPrefs.SetInt("Gear", _gear);
            PlayerPrefs.SetInt("Coins", _coins);
            PlayerPrefs.SetInt("XP", _xp);
            PlayerPrefs.Save();
        }
    }
}
