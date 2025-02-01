using System.Collections.Generic;
using System.Linq;
using TMPro;
using Troops;
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

        public void InitializePanel(Sprite icon, int level, BuildingType buildingType, Color color)
        {
            buildingIcon.sprite = icon;
            buildingLevelText.text = level.ToString();
            buildingLevelIcon.sprite = levelsSprites[level - 1];
            buildingLevelIcon.color = color;
            if (buildingType is BuildingType.Weapon)
            {
                Instantiate(linkIcon, availableLinksParent);
            }
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