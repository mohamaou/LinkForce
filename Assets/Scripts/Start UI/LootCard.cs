using System;
using Cards;
using Core;
using DG.Tweening;
using TMPro;
using Towers;
using Troops;
using UnityEngine;
using UnityEngine.UI;


namespace Start_UI
{
    public class LootCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cardAmount;
        [SerializeField] private Image towerImage;
        [SerializeField] private GameObject frontFace, backFace, goldImage;
        [SerializeField] private BuildingCard[] towerCards;
        private bool _animationDone;

        private void Start()
        {
            frontFace.SetActive(false);
            backFace.SetActive(true);
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.4f);
            var targetPos = new Vector2(Screen.width / 2f, Screen.height / 1.5f);
            transform.DOMove(targetPos, 1f);
            transform.DORotate(new Vector3(0, 360, 0),1).OnComplete(()=>
            {
                _animationDone = true;
                backFace.SetActive(false);
                frontFace.gameObject.SetActive(true);
                transform.DOScale(Vector3.one * 1.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
            }).SetRelative(true).SetEase(Ease.Linear);
        }

        public bool IsAnimationDone()
        {
            if (_animationDone) return true;
            DOTween.Kill(transform);
            var targetPos = new Vector2(Screen.width / 2f, Screen.height / 1.5f);
            transform.position = targetPos;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            transform.DOScale(Vector3.one * 1.4f, 0.1f).SetLoops(2,LoopType.Yoyo);
            backFace.SetActive(false);
            frontFace.SetActive(true);
            _animationDone = true;
            return false;
        }
        
        private BuildingCard GetRandomTroop() => towerCards[UnityEngine.Random.Range(0, towerCards.Length)];

        public void SetReward(int rewardAmount, bool isGold)
        {
            var tower = GetRandomTroop();
            goldImage.gameObject.SetActive(isGold);
            towerImage.gameObject.SetActive(!isGold);
            cardAmount.text = $"X{rewardAmount.ToShortString()}";
            towerImage.sprite = tower.GetSprite();
            if(isGold) Currencies.Instance.AddCoins(rewardAmount);
            else tower.AddCard(rewardAmount);
        }
    }
}
