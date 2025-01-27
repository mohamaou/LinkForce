using System.Collections;
using System.Collections.Generic;
using Cards;
using Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Start_UI
{
    public class MainMenuCardsPanel : MonoBehaviour
    { 
        public static MainMenuCardsPanel Instance {get; private set;}
        [SerializeField] private TroopCard[] troopCards;
        [SerializeField] private MainMenuCard card;
        [SerializeField] private Transform[] selectedCardsContainers;
        [SerializeField] private Transform cardsContainer;
        [SerializeField] private TextMeshProUGUI coinsText;
        
        private MainMenuCard  _currentSelectedCard;
        private List<MainMenuCard> _troopCards = new List<MainMenuCard>(); 
        
        private void Awake()
        {
            Instance = this;
        }

        private IEnumerator Start()
        {
            Currencies.Instance.UpdateCurrencyTexts(null,coinsText,null);
            var containerIndex = 0;
            foreach (var c in troopCards)
            {
                var newCard = Instantiate(card, card.transform.parent);
                newCard.SetCard(c);
                _troopCards.Add(newCard);
            }
            Destroy(card.gameObject);
            yield return new WaitForSeconds(Time.deltaTime);
            for (int i = 0; i < _troopCards.Count; i++)
            {
                var carSelected = _troopCards[i].IsUsed();
                if (containerIndex > selectedCardsContainers.Length - 1)
                {
                    _troopCards[i].RemoveFromDeck();
                    carSelected = false;
                }

                _troopCards[i].transform.SetParent(carSelected ? selectedCardsContainers[containerIndex] : cardsContainer);
                _troopCards[i].transform.localScale = Vector3.one;
                _troopCards[i].transform.localPosition = Vector3.zero;
                if (carSelected) containerIndex++;
            }
        }

        public void CardSelected(MainMenuCard cardInUse)
        {
            for (int i = 0; i < selectedCardsContainers.Length; i++)
            {
                selectedCardsContainers[i].DOShakeRotation(
                    duration: .2f, 
                    strength: new Vector3(0, 0, 4f), 
                    vibrato: 10, 
                    randomness: 90,
                    true
                ).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear).SetDelay(Random.Range(0f, 0.1f));
            }
            cardInUse.transform.SetParent(transform);
            var pos = new Vector2(Screen.width / 2f, Screen.height / 2f);
            cardInUse.transform.DOMove(pos, .2f);
            cardInUse.transform.DOScale(Vector3.one, duration: .2f);
            _currentSelectedCard = cardInUse;
        }

        public void CancelCardSelected()
        {
            if (_currentSelectedCard == null) return;
            _currentSelectedCard.transform.SetParent(cardsContainer);
            _currentSelectedCard.transform.DOLocalRotate(Vector3.zero, 0.2f);
            _currentSelectedCard.transform.DOScale(Vector3.one, 0.2f);
            _currentSelectedCard.DeselectCard();
            _currentSelectedCard = null;
            for (int i = 0; i < selectedCardsContainers.Length; i++)
            {
                var card = selectedCardsContainers[i];
                card.DOKill(true);
                card.DORotate(Vector3.zero, 0.5f).SetEase(Ease.OutQuad);
            }
        }

        public void SelectACard(MainMenuCard cardToRemove)
        {
            if (_currentSelectedCard == null) return;
            for (int i = 0; i < selectedCardsContainers.Length; i++)
            {
                var card = selectedCardsContainers[i];
                card.DOKill(true);
                card.DORotate(Vector3.zero, 0.5f).SetEase(Ease.OutQuad);
               if(selectedCardsContainers[i].GetChild(0) == cardToRemove.transform)
                   _currentSelectedCard.transform.SetParent(selectedCardsContainers[i]);
            }
            _currentSelectedCard.transform.DOLocalMove(Vector3.zero,0.2f); 
            _currentSelectedCard.transform.DOLocalRotate(Vector3.zero, 0.2f);
            _currentSelectedCard.transform.DOScale(Vector3.one, duration: .2f);
            _currentSelectedCard.UseInDeck();
            _currentSelectedCard = null;
            cardToRemove.transform.SetParent(cardsContainer);
            cardToRemove.transform.DOLocalRotate(Vector3.zero, 0.2f);
            cardToRemove.DeselectCard();
            cardToRemove.RemoveFromDeck();
        }
    }
}
