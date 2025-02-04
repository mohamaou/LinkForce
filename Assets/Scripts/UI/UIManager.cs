using System;
using System.Collections;
using System.Collections.Generic;
using Cards;
using Core;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Players;
using Start_UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace UI
{
    [Serializable]
    public class StartUI
    {
        [SerializeField] private TextMeshProUGUI player1Name, player2Name;
        [SerializeField] private TextMeshProUGUI player1Trophies, player2Trophies;
        [SerializeField] private Image player1Image, player2Image;
        [SerializeField] private Transform vsIcon, player1Holder, player2Holder;
        [SerializeField] private Image background;
        [SerializeField] private CardUI[] player1Cards, player2Cards;


        public void Start(Action onFinished)
        {
            if (PlayersProfiles.Instance != null)
            {
                player1Name.text = PlayersProfiles.Instance.Player1Name;
                player2Name.text = PlayersProfiles.Instance.Player2Name;
                player1Trophies.text = PlayersProfiles.Instance.Player1Trophies.ToString();
                player2Trophies.text =
                    $"{Mathf.Clamp(PlayersProfiles.Instance.Player1Trophies + Random.Range(-10, 10), 0, Mathf.Infinity)}";
                player1Image.sprite = PlayersProfiles.Instance.Player1Sprite;
                player2Image.sprite = PlayersProfiles.Instance.Player2Sprite;
            }
            
            var sequence = DOTween.Sequence();

            var player1StartPos = player1Holder.position;
            player1Holder.position += Vector3.left * Screen.width / 1.2f;
            sequence.Append(player1Holder.DOMove(player1StartPos, 0.4f).SetEase(Ease.OutBack));

            var player2StartPos = player2Holder.position;
            player2Holder.position += Vector3.right * Screen.width / 1.2f;
            sequence.Join(player2Holder.DOMove(player2StartPos, 0.4f).SetEase(Ease.OutBack));

            var vsIconStartScale = vsIcon.localScale;
            vsIcon.localScale = Vector3.zero;
            sequence.Join(vsIcon.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack));

            sequence.Join(vsIcon.DORotate(new Vector3(0, 0, 360), 0.4f, RotateMode.FastBeyond360).SetEase(Ease.Linear));

            sequence.AppendInterval(1f);

            sequence.Append(player1Holder.DOMove(player1StartPos + Vector3.left * Screen.width, 0.4f).SetEase(Ease.InBack));
            sequence.Join(player2Holder.DOMove(player2StartPos + Vector3.right * Screen.width, 0.4f).SetEase(Ease.InBack));
            sequence.Join(vsIcon.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack));
            sequence.Join(background.DOColor(Color.clear, 0.4f));
            sequence.OnComplete(() => onFinished?.Invoke());

        }

        public void SetPlayer1Card(BuildingCard[] buildingCards)
        {
            for (int i = 0; i < buildingCards.Length; i++)
            {
                player1Cards[i].SetCard(buildingCards[i]);
            }
        }

        public void SetPlayer2Card(BuildingCard[] buildingCards)
        {
            for (int i = 0; i < buildingCards.Length; i++)
            {
                player2Cards[i].SetCard(buildingCards[i]);
            }
        }
    }

    [Serializable]
    public class PlayersHealth
    {
        [SerializeField] private List<Image> player1Health, player2Health;
        [SerializeField] private Color deathColor;

        public void PlayerTakesDamage()
        {
            if (player1Health.Count == 0) return;
            var hearth = player1Health[0];
            player1Health.RemoveAt(0);
            var animationSpeed = 0.2f;
            hearth.DOColor(deathColor, animationSpeed);
            hearth.transform.DOScale(Vector3.one * 2, animationSpeed).OnComplete(() =>
            {
                var newColor = deathColor;
                newColor.a = 1;
                hearth.color = newColor;
            }).SetLoops(2, LoopType.Yoyo);
        }

        public void EnemyTakesDamage()
        {
            if (player2Health.Count == 0) return;
            var hearth = player2Health[0];
            player2Health.RemoveAt(0);
            var animationSpeed = 0.2f;
            hearth.DOColor(deathColor, animationSpeed);
            hearth.transform.DOScale(Vector3.one * 2, animationSpeed).OnComplete(() =>
            {
                var newColor = deathColor;
                newColor.a = 1;
                hearth.color = newColor;
            }).SetLoops(2, LoopType.Yoyo);
        }
    }

    [Serializable]
    public class BattlePanel
    {
        [SerializeField] private Slider player1Health, player2Health;
        
        
        public void SetPlayer1Health(float value) => player1Health.value = value;
        public void SetPlayer2Health(float value) => player2Health.value = value;
    }

    [Serializable]  
    public class PlayPanel
    {
        [SerializeField] private GameObject gameStartText;
        [SerializeField] private TextMeshProUGUI player1Name, player2Name;
        [SerializeField] private Image player1Image, player2Image;
        [SerializeField] private TextMeshProUGUI timerText, costToSummon, availableGold;
        [SerializeField] private Button summonButton, battleButton, destroyButton;
        [SerializeField] private TextMeshProUGUI spaceErrorText;
        [SerializeField] public GameObject summonPanel, waitePanel, battlePanel, buildingsPanel;
        [SerializeField] public DOTweenAnimation CoinsParent;
        [Header("Turn Result")]
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI roundCountText;
        [SerializeField] private Transform resultHolder;
        
        
        
        public void Start()
        {
            SetPlayUI(PlayState.Summon);
            spaceErrorText.gameObject.SetActive(false);
            resultHolder.gameObject.SetActive(false);
            gameStartText.SetActive(false);
            
            if (PlayersProfiles.Instance == null) return;
            player1Name.text = PlayersProfiles.Instance.Player1Name;
            player2Name.text = PlayersProfiles.Instance.Player2Name;
            player1Image.sprite = PlayersProfiles.Instance.Player1Sprite;
            player2Image.sprite = PlayersProfiles.Instance.Player2Sprite;
        }

        public void GameStart()
        {
            gameStartText.gameObject.SetActive(true);
            gameStartText.transform.localScale = Vector3.zero;
            gameStartText.transform.DOScale(Vector3.one, .4f).SetEase(Ease.OutBack);
            DOVirtual.DelayedCall(1f, () =>
            { 
                gameStartText.transform.DOScale(Vector3.zero, .4f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    gameStartText.gameObject.SetActive(false);
                });
            }, false);
        }
        
        public TextMeshProUGUI GetTimerText() => timerText;
        public TextMeshProUGUI GetAvailableGold() => availableGold;
        public Button GetSummonButton() => summonButton;
        public Button GetDestroyButton() => destroyButton;
        public Button GetBattleButton() => battleButton;

        public void ShowSpaceErrorText()
        {
            var errorTextCopy = Object.Instantiate(spaceErrorText, spaceErrorText.transform.parent);
            errorTextCopy.gameObject.SetActive(true);

            var originalPosition = errorTextCopy.rectTransform.anchoredPosition;
            var targetPosition = originalPosition + new Vector2(0, 50);

            var sequence = DOTween.Sequence();
            sequence.Append(errorTextCopy.rectTransform.DOAnchorPos(targetPosition, 1f).SetEase(Ease.OutQuad));
            sequence.Join(errorTextCopy.DOFade(0, 1f));
            sequence.OnComplete(() => Object.Destroy(errorTextCopy.gameObject));
        }

        public void UpdateAvailableCoins(int coinAmount, bool turnToRed = false)
        {
            availableGold.text = coinAmount.ToString();
            availableGold.color = turnToRed ? Color.red : Color.white;
        }

        public void ShowNotEnoughGoldEffect()
        {
           CoinsParent.DORestart();
        }

        public void SetPlayUI(PlayState playPanel)
        {
            summonPanel.SetActive(false);
            battlePanel.SetActive(false);
            waitePanel.SetActive(false);
            switch (playPanel)
            {
                case  PlayState.Summon: summonPanel.SetActive(true); break;
                case  PlayState.Wait: waitePanel.SetActive(true); break;
                case  PlayState.Battle:  battlePanel.SetActive(true); break;
            }
        }

        public void ShowTurnResult(PlayerTeam winingPlayer, int turnCount)
        {
            roundCountText.text = $"Round {turnCount}";
            resultText.text = winingPlayer == PlayerTeam.Player1? "Victory!" : "Defeat!";
            resultText.color = winingPlayer == PlayerTeam.Player1 ? Color.green : Color.red;
            resultHolder.gameObject.SetActive(true);
            resultHolder.transform.localScale = Vector3.zero;
            resultHolder.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            DOVirtual.DelayedCall(2f, () =>
            {
                resultHolder.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack);
            },false);
        }
    }

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        public StartUI startUI;
        public PlayersHealth playersHealth;
        public PlayPanel playPanel;
        public BattlePanel battlePanel;
        public BuildingUI BuildingPanel;
        public DestroyUI destroyPanelPrefab;

        [Space(10)]
        [SerializeField] private GameObject startPanel;
        [SerializeField] private GameObject playPanelObject;
        

        [Header("Feedbacks")]
        [SerializeField] private MMFeedbacks gameStartFeedback;
        [SerializeField] private MMFeedbacks playerLoseFeedback, playerWinFeedback;


        private void Awake()
        {
            Instance = this;
        }

        private IEnumerator Start()
        {
            playPanelObject.SetActive(false);
            startPanel.SetActive(true);
            
            var isStartUIComplete = false;
            GameManager.State = GameState.Start;
            startUI.Start(() => isStartUIComplete = true);
            yield return new WaitUntil(() => isStartUIComplete || !GameManager.Instance.showPlayerProfile);
            
            gameStartFeedback?.PlayFeedbacks();
            playPanel.Start();
            playPanel.GameStart();
            startPanel.SetActive(false);
            playPanelObject.SetActive(true);
            GameManager.State = GameState.Play;
        }
        

        public void GameEnd(bool win)
        {
            playPanelObject.SetActive(false);
            if (win) playerWinFeedback?.PlayFeedbacks();
            else playerLoseFeedback?.PlayFeedbacks();
        }
    }
}