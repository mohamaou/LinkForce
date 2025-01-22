using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PopupPanel : MonoBehaviour
    {
        [SerializeField] private GameObject popup;
        [SerializeField] private Image background;
        private float _alpha;

        private void Awake()
        {
            _alpha = background.color.a;
        }

        public void Open()
        {
            gameObject.SetActive(true);
            popup.SetActive(true);
            background.enabled = true;
            background.gameObject.SetActive(true);
            popup.transform.localScale = Vector3.one;
            var target = popup.transform.position;
            popup.transform.position += Vector3.left * Screen.width;
            popup.transform.DOMove(target, 0.3f);
            background.color =new Color(background.color.r,background.color.g,background.color.b,0);
            StartCoroutine(ShowBackground(false));
        }
        private IEnumerator ShowBackground(bool fade)
        {
            yield return null;
            while (true)
            {
                if (!fade && background.color.a == _alpha) yield break;
                if (background.color.a == 0 && fade) yield break;
                var a = Mathf.MoveTowards(background.color.a,fade ? 0: _alpha, 4 * Time.deltaTime);
                background.color = new Color(background.color.r,background.color.g,background.color.b,a);
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
        public void Close()
        {
            StartCoroutine(ShowBackground(true));
            popup.transform.DOScale(Vector3.zero, 0.3f).onComplete += () =>
            {
                background.enabled = false;
                popup.SetActive(false);
            };
        }
    }
}
