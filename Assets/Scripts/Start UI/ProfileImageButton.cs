using System;
using UnityEngine;
using UnityEngine.UI;

namespace Start_UI
{
    public class ProfileImageButton : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Button button;
        private int _index;
        private Action<int> _onClick;


        private void Start()
        {
            button.onClick.AddListener(()=> _onClick.Invoke(_index));
        }

        public void SetUp(Sprite sprite, int index, System.Action<int> onClick)
        {
            _index = index;
            image.sprite = sprite;
            _onClick = onClick;
        }
    }
}
