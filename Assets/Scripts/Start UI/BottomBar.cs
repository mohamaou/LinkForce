using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Start_UI;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    [Serializable]
    public class BottomButton
    {
        private const float AnimationSpeed = 0.2f;
        
        [SerializeField] private Button button;
        [SerializeField] private Image[] backgroundImage;
        [SerializeField] private Transform icon;
        [SerializeField] private RectTransform panel;
        [SerializeField] private bool showBackground;

        private Vector3 _startPos, _panelStartPos;
        private int _index;
        private List<Color> _backgroundColors = new List<Color>();

        public Button GetButton() => button;
        public bool ShowBackground => showBackground;

        public void SetButton(bool active, int index, int mainIndex)
        {
            _startPos = icon.transform.position;
            _panelStartPos = panel.transform.position;
            for (int i = 0; i < backgroundImage.Length; i++)
            {
                _backgroundColors.Add(backgroundImage[i].color);
            }
            icon.transform.localScale = Vector3.one * (active ? 1 : 0.7f);
            
            for (int i = 0; i < backgroundImage.Length; i++)
            {
                backgroundImage[i].DOColor(active? backgroundImage[i].color : Color.clear, AnimationSpeed);
            }
            _index = index;
            panel.gameObject.SetActive(active);
            if (active)
            {
                var buttonRect = icon.GetComponent<RectTransform>();
                var moveDistance = buttonRect.rect.height * 0.1f;
                var targetPos = _startPos + Vector3.up * moveDistance;
                icon.transform.position = targetPos;
            }
            else
            {
                float screenWidth = Screen.width;
                panel.transform.position += (mainIndex < _index ? Vector3.right : Vector3.left) * screenWidth;
            }
        }
        
        public void Active()
        {
            icon.DOScale(Vector3.one, AnimationSpeed).SetEase(Ease.OutBack);
            var buttonRect = icon.GetComponent<RectTransform>();
            var moveDistance = buttonRect.rect.height * 0.1f;
            var targetPos = _startPos + Vector3.up * moveDistance;
            icon.transform.DOMove(targetPos, AnimationSpeed).SetEase(Ease.OutBack);
            for (int i = 0; i < backgroundImage.Length; i++)
            { 
                backgroundImage[i].DOColor(_backgroundColors[i],AnimationSpeed);
            }
           
            panel.gameObject.SetActive(true);
            panel.transform.DOMove(_panelStartPos, AnimationSpeed).SetEase(Ease.Flash);
        }

        public void Deactivate(int activeIndex)
        {
            icon.DOScale(Vector3.one*.7f, AnimationSpeed).SetEase(Ease.OutBack);
            icon.DOMove(_startPos , AnimationSpeed).SetEase(Ease.OutBack);
            for (int i = 0; i < backgroundImage.Length; i++)
            { 
                backgroundImage[i].DOColor(Color.clear,AnimationSpeed);
            }
            var x = (activeIndex < _index ? 1 : -1) * Screen.width;
            panel.transform.DOMove(_panelStartPos + Vector3.right * x, AnimationSpeed)
                .OnComplete(() => panel.gameObject.SetActive(false));
        }
    }
    public class BottomBar : MonoBehaviour
    {
        [SerializeField] private int mainButton = 2;
        [SerializeField] private GameObject background;
        [SerializeField] private BottomButton[] buttons;
 

        private IEnumerator Start()
        {
            SetButtonsEvents();
            yield return new WaitForSeconds(Time.deltaTime);
            for (int i = 0; i < buttons.Length; i++)
            { 
                buttons[i].SetButton(i == mainButton,i,mainButton);
            }
            background.SetActive(true);
        }
        

        private void SetButtonsEvents()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                var index = i;
                buttons[i].GetButton().onClick.AddListener(() => ButtonClicked(index));
            }
        }

        private void ButtonClicked(int index)
        {
            if (MainMenuCardsPanel.Instance.IsCardSelected()) return;
            for (int i = 0; i < buttons.Length; i++)
            {
                if(i == index) 
                {
                    background.SetActive(buttons[i].ShowBackground);
                    buttons[i].Active();
                }
                else buttons[i].Deactivate(index);
            }
        }
    }
}
