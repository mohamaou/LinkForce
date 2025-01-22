using System;
using DG.Tweening;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

namespace UI
{
    public class RoundUI : MonoBehaviour
    {
        public static RoundUI Instance { get; private set; }
        [SerializeField] private TextMeshProUGUI roundCountText;
        [SerializeField] private Transform holder;
        [SerializeField] private MMFeedbacks roundStartFeedbacks;
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            holder.gameObject.SetActive(false);
        }

        public void SetRoundCount(int roundCount)
        {
            roundStartFeedbacks.PlayFeedbacks();
            holder.gameObject.SetActive(true);
            roundCountText.text = roundCount.ToString();
            holder.localScale = Vector3.zero;
            holder.DOScale(Vector3.one, 0.8f).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    holder.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack).SetDelay(1).OnComplete(() =>
                    {
                        holder.gameObject.SetActive(false);
                    });
                }
            );
        }
    }
}
