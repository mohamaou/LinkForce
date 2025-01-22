using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Start_UI
{
    public class SearchPanel : MonoBehaviour
    { 
        [SerializeField] private TextMeshProUGUI player1Name, player2Name;
        [SerializeField] private Image player1Image, player2Image;
        [SerializeField] private GameObject searchIcon, player2Bar;
        
        private IEnumerator Start()
        {
            searchIcon.SetActive(true);
            player2Bar.SetActive(false);
            player1Name.text = PlayersProfiles.Instance.Player1Name;
            var enemyName = NameGenerator.GetRandomName();
            PlayersProfiles.Instance.SetPlayer2Name(enemyName);
            var sprite = Profile.Instance.GetRandomSprite();
            player2Name.text = enemyName;
            player2Image.sprite = sprite;
            player1Image.sprite = PlayersProfiles.Instance.Player1Sprite;
            PlayersProfiles.Instance.SetPlayer2Sprite(sprite);
            yield return new WaitForSeconds(2f);
            searchIcon.SetActive(false);
            player2Bar.SetActive(true);
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene(1);
        }
    }
}
