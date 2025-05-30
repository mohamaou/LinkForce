using System.Collections.Generic;
using System.Linq;
using Core;
using Players;
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
        [SerializeField] private Image linkIcon;
        private List<GameObject> _linkIcons = new List<GameObject>();

        public void InitializePanel(Sprite icon, int level, BuildingType buildingType, PlayerTeam team)
        {
            buildingIcon.sprite = icon;
            buildingLevelText.text = level.ToString();
            buildingLevelIcon.sprite = levelsSprites[level - 1];
            var color = team == PlayerTeam.Player1
                ? GameManager.Instance.player1Color
                : GameManager.Instance.player2Color;
 
            buildingLevelIcon.color = color;

            if (team == PlayerTeam.Player2) return;
            if (buildingType is BuildingType.Weapon)
            {
                linkIcon.color = color;
                var i = Instantiate(linkIcon, availableLinksParent);
            }
        }

        public void UpdateLevelNumber(int level)
        {
            buildingLevelText.text = level.ToString();
            buildingLevelIcon.sprite = levelsSprites[level - 1];
        }

        public void AddLinkPoint(Color color)
        {
            linkIcon.color = color;
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