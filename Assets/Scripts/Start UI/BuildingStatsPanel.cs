using Cards;
using Core;
using TMPro;
using Troops;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Start_UI
{
    public class BuildingStatsPanel : MonoBehaviour
    {
        public static BuildingStatsPanel Instance{get; private set;}
        [SerializeField] private TextMeshProUGUI troopNameText, healthText, damageText, upgradeCostText, cardCollectionText, descriptionText, cardLevelText;
        [SerializeField] private TextMeshProUGUI buildingTypeText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private PopupPanel popupPanel;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Image troopImage;
        [Header("Building Type")]
        [SerializeField] private Color troopsColor;
        [SerializeField] private Color weaponColor, buffColor;
        [SerializeField] private Image[] images;
        private BuildingCard _troop;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            upgradeButton.onClick.AddListener(Upgrade);
        }

        private void Upgrade()
        {
            if (_troop == null || !UpgradeAvailable()) return;
            var currentLevel = _troop.GetCardLevel();
            if (!Currencies.Instance.IsEnoughCoins(GetUpgradeCost(currentLevel))) return;
            Currencies.Instance.AddCoins(-GetUpgradeCost(currentLevel));
            _troop.LevelUp(RequiredCardToLevelUp(currentLevel));
            UpdateStats();
        }

        public void SetTroop(BuildingCard building)
        {
            popupPanel.Open();
            troopNameText.text = building.GetTroopName();
            descriptionText.text = building.GetDescription();
            troopImage.sprite = building.GetSprite();
            _troop = building;
            buildingTypeText.text = building.GetBuildingType() switch
            {
                BuildingType.Troops=> "Spawner",
                BuildingType.Buff => "Buff",
                BuildingType.Weapon => "Weapon"
            };
            foreach (Image image in images)
            {
                image.color = building.GetBuildingType() switch
                {
                    BuildingType.Troops => troopsColor,
                    BuildingType.Buff => buffColor,
                    BuildingType.Weapon => weaponColor
                };
            }
            UpdateStats();
        }

        private void UpdateStats()
        {
           // healthText.text = $"{_troop.GetHealth() + _troop.GetCardLevel()}";
           // damageText.text = $"{_troop.GetDamage() + _troop.GetCardLevel()}";
            upgradeCostText.text = GetUpgradeCost(_troop.GetCardLevel()).ToString();
            upgradeCostText.color = Currencies.Instance.IsEnoughCoins(GetUpgradeCost(_troop.GetCardLevel())) ? Color.white : Color.red;
            cardLevelText.text = $"Level {_troop.GetCardLevel()}";
            cardCollectionText.text = $"{_troop.AvailableCard()} / {RequiredCardToLevelUp(_troop.GetCardLevel())}";
            progressSlider.value = _troop.AvailableCard()/(float) RequiredCardToLevelUp(_troop.GetCardLevel());
        }
        
        private bool UpgradeAvailable()
        {
            return _troop.AvailableCard() >= RequiredCardToLevelUp(_troop.GetCardLevel()); 
        }
        private int GetUpgradeCost(int currentLevel)
        {
            return currentLevel switch
            {
                1 => 100,
                2 => 200,
                3 => 500,
                4 => 1000,
                5 => 2000,
                6 => 4000,
                7 => 8000,
                8 => 16000,
                9 => 32000,
                _ => 0
            };
        }
        private int RequiredCardToLevelUp(int currentLevel)
        {
            return currentLevel switch
            {
                1 => 5, 2 => 10, 3 => 20, 4 => 40, 5 => 80, 6 => 160, 7 => 320, 8 => 640, 9 => 1280
            };
        }
    }
}
