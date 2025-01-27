using Cards;
using Core;
using DG.Tweening;
using TMPro;
using Troops;
using UnityEngine;
using UnityEngine.UI;

namespace Start_UI
{
    public class MainMenuCard : MonoBehaviour
    {
        [SerializeField] private Button selectButton, upgradeButton, useButton;
        [SerializeField] private GameObject lockPanel, selectPanel, levelHolder, cardAvailable, cardNotAvailable;
        [SerializeField] private Image towerImage, upgradeArrow;
        [SerializeField] private Slider towerSlider;
        [SerializeField] private TextMeshProUGUI progressSliderText, upgradeCostText, cardLevelText, troopLevelText, troopNameText;
        [Header("Building Type")]
        [SerializeField] private Color troopsColor;
        [SerializeField] private Color weaponColor, buffColor;
        [SerializeField] private Image[] images;
        private bool _selected;
        private BuildingCard _troop;

        private void Start()
        {
            upgradeButton.onClick.AddListener(ShowInfoPanel);
        }

        public void SetCard(BuildingCard buildingCard)
        {
            _troop = buildingCard;
            _selected = false; 
            var isLocked = buildingCard.IsLocked();
            levelHolder.SetActive(!isLocked);
            lockPanel.SetActive(isLocked);
            towerSlider.gameObject.SetActive(!isLocked);
            selectPanel.SetActive(false);
            troopLevelText.gameObject.SetActive(!isLocked);
            useButton.gameObject.SetActive(false);
            cardLevelText.text = buildingCard.GetCardLevel().ToString();
            towerImage.sprite = buildingCard.GetSprite();
            cardAvailable.SetActive(UpgradeAvailable());
            cardNotAvailable.SetActive(!UpgradeAvailable());
            selectButton.onClick.AddListener(()=>
            {
                CardSelected();
                SelectToUse();
            });
            useButton.onClick.AddListener(UseCard);
            buildingCard.SetCardChangedEvent(() =>
            {
                progressSliderText.text = $"{buildingCard.AvailableCard()} / {RequiredCardToLevelUp(buildingCard.GetCardLevel())}";
                towerSlider.value = buildingCard.AvailableCard()/(float) RequiredCardToLevelUp(buildingCard.GetCardLevel());
                var locked = buildingCard.IsLocked(); 
                if(lockPanel!= null) lockPanel.SetActive(locked);
                if(towerSlider!= null)towerSlider.gameObject.SetActive(!locked);
                if(troopLevelText!= null) troopLevelText.gameObject.SetActive(!locked);
                SetCardStats(buildingCard);
            });
            if (UpgradeAvailable())
            {
                upgradeArrow.transform.DOScale(Vector3.one*1.2f, .5f).SetEase(Ease.OutBounce).SetLoops(-1,LoopType.Yoyo);
            }
            Currencies.Instance.SetCoinsChangeEvent(() =>
            {
                upgradeCostText.color = Currencies.Instance.IsEnoughCoins(GetUpgradeCost(buildingCard.GetCardLevel()))
                    ? Color.white
                    : Color.red;
            });
            SetCardStats(buildingCard);
            foreach (var image in images)
            {
                image.color = buildingCard.GetBuildingType() switch
                {
                    BuildingType.Troops => troopsColor,
                    BuildingType.Weapon => weaponColor,
                    BuildingType.Buff => buffColor,
                    _ => Color.white
                };
            }
        }

        
        private void SetCardStats(BuildingCard towerCard)
        {
            if(upgradeArrow != null) upgradeArrow.color = UpgradeAvailable() ? Color.green : Color.white;
            troopNameText.text = towerCard.GetTroopName();
            troopLevelText.text = $"Level {towerCard.GetCardLevel()}";
            progressSliderText.text = $"{towerCard.AvailableCard()} / {RequiredCardToLevelUp(towerCard.GetCardLevel())}";
            towerSlider.value = towerCard.AvailableCard()/(float) RequiredCardToLevelUp(towerCard.GetCardLevel());
            upgradeCostText.text = GetUpgradeCost(towerCard.GetCardLevel()).ToShortString();
        }

        #region Upgrade
        private bool UpgradeAvailable()
        {
            return _troop.AvailableCard() >= RequiredCardToLevelUp(_troop.GetCardLevel()); 
        }
        private int RequiredCardToLevelUp(int currentLevel)
        {
            return currentLevel switch
            {
                1 => 5, 2 => 10, 3 => 20, 4 => 40, 5 => 80, 6 => 160, 7 => 320, 8 => 640, 9 => 1280
            };
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
        private void ShowInfoPanel()
        {
            BuildingStatsPanel.Instance.SetTroop(_troop);
        }
        #endregion
        
        #region Use The Card
        private void SelectToUse()
        {
            if (!_troop.IsSelected() || _troop.IsLocked()) return;
            MainMenuCardsPanel.Instance.SelectACard(this);
        }
        private void UseCard()
        {
            MainMenuCardsPanel.Instance.CardSelected(this);
            DeselectCard();
            useButton.gameObject.SetActive(false);
        }
        public bool IsUsed() => _troop.IsSelected();
        public void UseInDeck()=> _troop.Select();
        public void RemoveFromDeck()
        {
            transform.DOScale(Vector3.one, 0.1f);
            _troop.Deselect();
        }
        #endregion

        #region Selected
        private void CardSelected()
        {
            if (_troop.IsLocked()) return;
            if (_selected)
            {
                DeselectCard();
                return;
            }
            selectPanel.SetActive(true);
            levelHolder.SetActive(false);
            selectPanel.transform.localScale = Vector3.zero;
            selectPanel.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack);
            if (!IsUsed()) useButton.gameObject.SetActive(true);
            _selected = true;
            upgradeButton.transform.localScale = Vector3.zero;
            upgradeButton.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            useButton.transform.localScale = Vector3.zero;
            upgradeButton.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            useButton.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            towerSlider.gameObject.SetActive(!UpgradeAvailable());
            foreach (var card in FindObjectsOfType<MainMenuCard>())
            {
                if(card != this) card.DeselectCard();
            }
            if (!IsUsed()) MainMenuCardsPanel.Instance.CancelCardSelected();
        }
        public void DeselectCard()
        {
            if (_troop.IsLocked()) return;
            selectPanel.SetActive(false);
            levelHolder.SetActive(true);
            _selected = false;
            upgradeButton.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.OutBack);
            useButton.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.OutBack).onComplete =
                () => useButton.gameObject.SetActive(false);
            if(!_troop.IsLocked())towerSlider.gameObject.SetActive(true);
        }
        #endregion
    }
}
