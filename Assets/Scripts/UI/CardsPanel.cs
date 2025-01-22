using Cards;
using UnityEngine;

namespace UI
{
    public class CardsPanel : MonoBehaviour
    {
        public static CardsPanel Instance { get; private set; }
        [SerializeField] private CardUI[] cardsUI;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Transform towersParent;
        private TroopCard[] _selectedCard; 


        private void Awake()
        {
            Instance = this;
        }


        public void SetUpCards(TroopCard[] cards)
        {
            _selectedCard = cards;
            for (int i = 0; i < cardsUI.Length; i++)
            {
                if (i < cards.Length)
                {
                  
                   // cardsUI[i].SetTower(groundLayer,towersParent, cards[i].GetSprite(Player.Instance.GetCurrentAge()));
                    cardsUI[i].gameObject.SetActive(true);
                }
                else
                {
                    cardsUI[i].gameObject.SetActive(false);
                }
            }
        }

        public void UpdateCards()
        { 
            for (int i = 0; i < cardsUI.Length; i++)
            {
                if (i < _selectedCard.Length)
                {
                  
                  //  cardsUI[i].SetTower(groundLayer,towersParent, _selectedCard[i].GetSprite(Player.Instance.GetCurrentAge()));
                    cardsUI[i].gameObject.SetActive(true);
                }
                else
                {
                    cardsUI[i].gameObject.SetActive(false);
                }
            }
            
        }
    }
}
