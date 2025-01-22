using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Start_UI
{
    public class LeaderboardPanel : MonoBehaviour
    {
       [SerializeField] private LeaderboardPlayerBar leaderboardPlayerBar;
       [SerializeField] private ScrollRect scrollbar;
       [SerializeField] private TextMeshProUGUI leagueNameText;

       private void Start()
       {
           var leagueName = Profile.Instance.LeagueName();
           var trophies = Profile.Instance.GetTrophyRange(leagueName);
           leagueNameText.text = leagueName;
           var playerBar = Instantiate(leaderboardPlayerBar, leaderboardPlayerBar.transform.parent);
           playerBar.SetStats(true, Profile.Instance.GetPlayerName(), Profile.Instance.GetPlayerTrophies());
           var players = new List<LeaderboardPlayerBar>();
           players.Add(playerBar);
           for (int i = 0; i < 200; i++)
           {
               var p = Instantiate(leaderboardPlayerBar,leaderboardPlayerBar.transform.parent);
               p.SetStats(false,NameGenerator.GetRandomName(),Random.Range(trophies.min,trophies.max));
               players.Add(p);
           }
           players.Sort((a, b) =>
           {
               var rankA = a.GetTrophiesCount(); 
               var rankB = b.GetTrophiesCount();
               return rankB.CompareTo(rankA); 
           });
           for (int i = 0; i < players.Count; i++)
           {
               players[i].SetRankers(i+1);
               players[i].transform.SetSiblingIndex(i);
           }
           scrollbar.verticalNormalizedPosition = 1f- playerBar.GetRank() / (float)players.Count;
           leaderboardPlayerBar.gameObject.SetActive(false);
       }
    }
}
