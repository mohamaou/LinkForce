using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BuildingUI : MonoBehaviour
    {
        [SerializeField] private Image buildingIcon;
        [SerializeField] private TextMeshProUGUI buildingLevelText;
        [SerializeField] private Image buildingLevelIcon;
        [SerializeField] private Sprite[] levelsSprites;
        [SerializeField] private Transform availableLinksParent;
        [SerializeField] private Transform linkIcon;
        private List<GameObject> _linkIcons = new List<GameObject>();

        public void InitializePanel(Sprite icon, int level)
        {
            buildingIcon.sprite = icon;
            buildingLevelText.text = level.ToString();
            buildingLevelIcon.sprite = levelsSprites[level - 1];
        }

        public void UpdateLevelNumber(int level)
        {
            buildingLevelText.text = level.ToString();
        }

        public void AddLinkPoint()
        {
            var obj = Instantiate(linkIcon, availableLinksParent).gameObject;
            _linkIcons.Add(obj);
        }

        public void RemoveLinkPoint()
        {
            var obj = _linkIcons.Last();
            Destroy(obj);
        }
    }
}