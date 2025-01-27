using Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CardUI : MonoBehaviour
    {
        [SerializeField] private Button cardButton;
        [SerializeField] private TextMeshProUGUI troopName, troopLevelText, attackText, healthText;
        [SerializeField] private Image troopImage;
        private BuildingCard _dice;

        public Button GetCardButton() => cardButton;

        
        public void SetCard(BuildingCard dice)
        {
            _dice = dice;
            troopName.text = dice.name;
            troopLevelText.text = dice.GetDescription().ToString();
            troopImage.sprite = dice.GetSprite();
            //attackText.text = dice.GetDamage().ToString();
           // healthText.text = dice.GetHealth().ToString();
        }
    }
}
