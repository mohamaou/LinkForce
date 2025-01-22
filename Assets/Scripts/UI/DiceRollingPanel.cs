using System;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;
using Zombies;

namespace UI
{
    public class DiceRollingPanel : MonoBehaviour
    {
        [Serializable]
        public struct Panel
        {
            [SerializeField] private Image image;
            [SerializeField] private RawImage rawImage;
            [SerializeField] private Color player1, player2;

            public void SetColor(PlayerTeam team)
            {
                var c = team == PlayerTeam.Player1? player1: player2;
                if(rawImage != null) rawImage.color = c;
                else if(image != null) image.color = c;
            }
        }
        public static DiceRollingPanel Instance { get; private set; }
        [SerializeField] private Image background;
        [SerializeField] private RectTransform content;
        [SerializeField] private Button rollButton;
        [SerializeField] private MMFeedbacks panelOpenFeedbacks, diceRollingFeedbacks;
        [SerializeField] private Panel[] images;
        private Color _originalColor;
        private float _originalWidth;


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _originalColor = background.color;
            _originalWidth = content.rect.width;
            rollButton.onClick.AddListener(() =>
            {
                rollButton.gameObject.SetActive(false);
                diceRollingFeedbacks?.PlayFeedbacks();
                //DiceRollingManager.Instance.Roll(PlayerTeam.Player1,true);
            });
            background.enabled = false;
            rollButton.gameObject.SetActive(false);
            content.gameObject.SetActive(false);
        }

        public void RollTheDices(PlayerTeam team)
        {
            rollButton.gameObject.SetActive(false);
            diceRollingFeedbacks?.PlayFeedbacks();
           
        }

        public void ShowRollButton() => rollButton.gameObject.SetActive(true);
        
        public void Open(PlayerTeam playerTeam)
        {
            gameObject.SetActive(true);
            background.enabled = true;
            content.gameObject.SetActive(true);
            panelOpenFeedbacks?.PlayFeedbacks();
            var animationSpeed = .7f;
            background.color = Color.clear;
            content.sizeDelta = new Vector2(86f, content.sizeDelta.y);
            
            background.DOColor(_originalColor, animationSpeed);
            content.DOSizeDelta(new Vector2(_originalWidth, content.sizeDelta.y), animationSpeed).SetEase(Ease.OutBounce);

            for (int i = 0; i < images.Length; i++)
            {
                images[i].SetColor(playerTeam);
            }
        }

        public void Close()
        {
            var animationSpeed = .4f;
            background.DOColor(Color.clear, animationSpeed);
            content.DOSizeDelta(new Vector2(89, content.sizeDelta.y), animationSpeed).OnComplete(
                () =>
                {
                    content.gameObject.SetActive(false);
                    background.enabled = false;
                });
        }
    }
}
