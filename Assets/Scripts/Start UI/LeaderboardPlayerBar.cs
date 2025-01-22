using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Start_UI
{
    public class LeaderboardPlayerBar : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerNameText, trophiesText, rankText;
        [SerializeField] private Image playerImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color playerColor, randomPersonColor;
        private int _trophiesCount, _rank;


        public void SetStats(bool isPlayer, string playerName, int trophiesCount)
        {
            _trophiesCount = trophiesCount;
            backgroundImage.color = isPlayer ? playerColor : randomPersonColor;
            trophiesText.text = trophiesCount.ToString();
            playerNameText.text = playerName;
            playerImage.sprite = isPlayer? Profile.Instance.GetPlayerSprite(): Profile.Instance.GetRandomSprite();
        }

        public void SetRankers(int rank)
        {
            _rank = rank;
            rankText.text = $"#{rank}";
        } 
        public int GetTrophiesCount() => _trophiesCount;
        public int GetRank() => _rank;
    }
}
