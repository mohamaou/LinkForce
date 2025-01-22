using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using DG.Tweening;
using Towers;
using Troops;
using UnityEngine;

namespace Start_UI
{
    [Serializable]
    public struct RewardLoot
    {
        public int Amount;
        public bool IsGold;

        public RewardLoot(int amount, bool isGold)
        {
            Amount = amount;
            IsGold = isGold;    
        }
    }
    public class LootPanel : MonoBehaviour
    {
        public static LootPanel Instance {get; protected set;}
        [SerializeField] private GameObject lootPanel, chest;
        [SerializeField] private LootCard lootCard;
        [SerializeField] private Transform chestRender;
        private List<LootCard> _lootCards = new List<LootCard>();
        private Transform _currentChest;
        private List<RewardLoot> _allRewards = new List<RewardLoot>();

        private void Awake()
        {
            Instance = this;
            chest.SetActive(false);
        }

        public IEnumerator CreateBox(List<RewardLoot> rewardLoots)
        {
            lootPanel.SetActive(true);
            _currentChest = Instantiate(chest, chest.transform.position, chest.transform.rotation).transform;
            _currentChest.gameObject.SetActive(true);
            _allRewards = rewardLoots;
            yield return new WaitForSeconds(1.9f);
            StartCoroutine(OpenLoot(true));
        }
        private IEnumerator OpenLoot(bool firstTime)
        {
            if (_allRewards.Count == 0)
            {
                Close();
                yield break;
            }
            for (int i = 0; i < _lootCards.Count; i++)
            {
                _lootCards[i].transform.DOScale(Vector3.zero, 0.1f);
            }

            if (!firstTime)
            {
                var targetScale = new Vector3(1f, 1.1f, 1f);
                _currentChest.DOScale(targetScale, 0.2f).SetLoops(2, LoopType.Yoyo);
            }
            
            var card = Instantiate(lootCard,transform);
            card.transform.position = chestRender.position;
            var reward = _allRewards[0];
            card.SetReward(reward.Amount, reward.IsGold);
            _allRewards.RemoveAt(0);
            _lootCards.Add(card);
            
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            if (card.IsAnimationDone())
            {
                yield return null;
                StartCoroutine(OpenLoot(false));
            }
            else
            {
                yield return null;
                yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
                yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
                yield return null;
                StartCoroutine(OpenLoot(false));
            }
        }

        private void Close()
        {
            lootPanel.SetActive(false);
            Destroy(_currentChest.gameObject);
            for (int i = 0; i < _lootCards.Count; i++)
            {
                Destroy(_lootCards[i].gameObject);
            }
            _lootCards.Clear();
            _currentChest = null;
        }
    }
}
