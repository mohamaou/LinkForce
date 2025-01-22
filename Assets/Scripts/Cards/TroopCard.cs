using System;
using AI;
using Core;
using Players;
using Troops;
using UnityEngine;
using Zombies;

namespace Cards
{
    [CreateAssetMenu(fileName = "NewCard", menuName = "ScriptableObjects/Monster Card", order = 1)]
    public class TroopCard : ScriptableObject
    {
        [SerializeField] private new string name;
        [SerializeField] private Sprite cardSprite;
        [SerializeField] private Troop troop;
        [SerializeField] private bool defaultTroop;
        [SerializeField] private int troopLevel;
        [SerializeField] private int damage, health;
        private Action _onCardAddedEvent;



        
        public Sprite GetSprite() => cardSprite;
        public string GetTroopName() => name;
        public bool IsLocked()
        {
            var key = $"IsLocked_{name}";
            return PlayerPrefs.GetInt(key, defaultTroop ? 0 : 1) == 1;
        }
        public bool IsSelected()
        {
            var key = $"IsSelected_{name}";
            return PlayerPrefs.GetInt(key, defaultTroop ? 1 : 0) == 1;
        }
        public int GetTroopLevel() => troopLevel;
        public int GetDamage() => damage;
        public int GetHealth() => health;
        public Troop GetTroop() => troop;
        
        

        #region Card Level
        public void SetCardChangedEvent(System.Action evento) => _onCardAddedEvent += evento;
        public int GetCardLevel()
        {
            var key = $"CardLevel_{name}";
            return PlayerPrefs.GetInt(key, 1);
        }
        public int AvailableCard()
        {
            var key = $"AvailableCard{name}";
            return PlayerPrefs.GetInt(key, 0);
        }
        public void AddCard(int card)
        {
            if (IsLocked())
            {
                var keyLock = $"IsLocked_{name}";
                PlayerPrefs.SetInt(keyLock, 0);
            }
            var key = $"AvailableCard{name}";
            var availableCard = AvailableCard();
            PlayerPrefs.SetInt(key, card + availableCard);
            _onCardAddedEvent?.Invoke();
        }
        public void LevelUp(int cardsCost)
        {
            AddCard(-cardsCost);
            var key = $"CardLevel_{name}";
            var currentLevel = GetCardLevel() +1;
            PlayerPrefs.SetInt(key, currentLevel);
            _onCardAddedEvent?.Invoke();
        }
        #endregion

        public void Select() => PlayerPrefs.SetInt($"IsSelected_{name}", 1);
        public void Deselect() => PlayerPrefs.SetInt($"IsSelected_{name}", 0);
    }
}
