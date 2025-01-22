using System.Collections;
using Cards;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Zombies
{
    public class ZombieCardUI : MonoBehaviour
    {
        [SerializeField] private Image sprite;
        [SerializeField] private TextMeshProUGUI cost;
        [SerializeField] private EventTrigger button;
        [SerializeField] private GameObject lockedObject, zombieObject;
        private ZombieType _zombie;
        private Coroutine buttonHoldCoroutine;
        private int _cost;
        
        private void Start()
        {
            SetClockState(true);
            var pointerDownEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            pointerDownEntry.callback.AddListener((eventData) => StartButtonHold());
            button.triggers.Add(pointerDownEntry);
            
            var pointerUpEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };
            pointerUpEntry.callback.AddListener((eventData) => StopButtonHold());
            button.triggers.Add(pointerUpEntry);
            
        }

        public void SetClockState(bool isLocked)
        {
            lockedObject.SetActive(isLocked);
            zombieObject.SetActive(!isLocked);
        }

        public void SetCard(ZombieCard zombie)
        {
            sprite.sprite = zombie.GetSprite();
            cost.text = zombie.GetCost().ToString();
            _cost = zombie.GetCost();
            _zombie = zombie.GetZombie();
         //   cost.color = InGameCurrency.Instance.IsCoinEnough(_cost) ? Color.white : Color.red;
           // InGameCurrency.Instance.SetCoinChangeEvent((coinsAvailable) => cost.color = coinsAvailable >= _cost ? Color.white : Color.red);
        }

        public ZombieType GetZombie() => _zombie;
        public bool IsLocked() => lockedObject.activeSelf;
        private void ButtonClicked()
        {
           // if (lockedObject.activeSelf || !InGameCurrency.Instance.IsCoinEnough(_cost)) return;
           // InGameCurrency.Instance.SpendCoins(_cost);
            ZombiesPanel.Instance.SpawnZombie(_zombie);
        }
        
        private void StartButtonHold()
        {
            if (buttonHoldCoroutine == null)
            {
                buttonHoldCoroutine = StartCoroutine(HoldButton());
            }
        }

        private void StopButtonHold()
        {
            if (buttonHoldCoroutine != null)
            {
                StopCoroutine(buttonHoldCoroutine);
                buttonHoldCoroutine = null;
            }
        }

        private IEnumerator HoldButton()
        {
            while (true)
            {
                ButtonClicked();
                yield return new WaitForSeconds(0.2f); 
            }
        }
    }
}
