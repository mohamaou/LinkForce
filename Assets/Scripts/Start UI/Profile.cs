using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Start_UI
{
    public struct League
    {
        
    }
    public struct PlayerStats
    {
        public string LoadPlayerName() => PlayerPrefs.GetString("PlayerName", "You");
        public int LoadProfileImage() => PlayerPrefs.GetInt("ProfileImage", 0);
        public int LoadTrophies() => PlayerPrefs.GetInt("Trophies", 0);
        
        public void SavePlayerName(string newName)
        {
            PlayersProfiles.Instance.SetPlayer1Name(newName);
            PlayerPrefs.SetString("PlayerName", newName);
        }

        public void SaveProfileImage(int newImage) => PlayerPrefs.SetInt("ProfileImage", newImage);
        public void SaveTrophies(int amount)
        {
            var trophies = LoadTrophies();
            trophies += amount;
            PlayerPrefs.SetInt("Trophies", trophies);
        }
    }
    public class Profile : MonoBehaviour
    {
        public static Profile Instance { get; private set; }
        private PlayerStats _playerStats;
        [SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private TextMeshProUGUI[] playerNameText;
        [SerializeField] private TextMeshProUGUI[] trophiesText;


        [SerializeField] private Transform profileImagesPanel;
        [SerializeField] private Sprite[] profileSprites;
        [SerializeField] private Image[] profileImages;
        [SerializeField] private ProfileImageButton profileImageButton;

        private void Awake()
        {
            Instance = this;
            _playerStats = new PlayerStats();
          
        }

        private void Start()
        {
            for (int i = 0; i < trophiesText.Length; i++)
            {
                trophiesText[i].text = _playerStats.LoadTrophies().ToString();
            }
            PlayersProfiles.Instance.SetPlayer1Trophies(_playerStats.LoadTrophies());
            PlayersProfiles.Instance.SetPlayer1Name(_playerStats.LoadPlayerName());
            Closed();
            ChangeProfileImage(_playerStats.LoadProfileImage());
            playerNameInputField.onValueChanged.AddListener(value=>
            {
                _playerStats.SavePlayerName(value);
            });
            for (int i = 0; i < profileSprites.Length; i++)
            {
               var p = Instantiate(profileImageButton, profileImageButton.transform.parent);
               p.SetUp(profileSprites[i],i,ChangeProfileImage);
            }
        }

        private void ChangeProfileImage(int index)
        {
            _playerStats.SaveProfileImage(index);
            PlayersProfiles.Instance.SetPlayer1Sprite(GetSprite(index));
            for (int i = 0; i < profileImages.Length; i++)
            {
                profileImages[i].sprite = profileSprites[index];
            }
            ShowProfileImages(false);
        }
        public void Closed()
        {
            for (int i = 0; i < playerNameText.Length; i++)
            {
                playerNameText[i].text = _playerStats.LoadPlayerName();
            }
            ShowProfileImages(false);
        }

        public void ShowProfileImages(bool show)
        {
            if(show) profileImagesPanel.gameObject.SetActive(true);
            profileImagesPanel.DOScale(show? Vector3.one : Vector3.zero,0.2f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                if(!show) profileImagesPanel.gameObject.SetActive(false);
            });
        }

        public string LeagueName()
        {
            var trophiesCount = _playerStats.LoadTrophies();
            return trophiesCount switch
            {
                <= 399 => "Wooden League",
                <= 999 => "Bronze League",
                <= 1999 => "Silver League",
                <= 2999 => "Golden League",
                <= 3999 => "Platinum League",
                <= 4999 => "Diamond League",
                <= 5999 => "Master League",
                <= 6999 => "Champion League",
                <= 7999 => "Grand Champion League",
                _ => "Legendary League",
            };
        }
        public (int min, int max) GetTrophyRange(string leagueName)
        {
            return leagueName switch
            {
                "Wooden League" => (0, 399),
                "Bronze League" => (400, 999),
                "Silver League" => (1000, 1999),
                "Golden League" => (2000, 2999),
                "Platinum League" => (3000, 3999),
                "Diamond League" => (4000, 4999),
                "Master League" => (5000, 5999),
                "Champion League" => (6000, 6999),
                "Grand Champion League" => (7000, 7999),
                "Legendary League" => (8000, int.MaxValue),
                _ => (0, 0)
            };
        }
        public Sprite GetPlayerSprite()=> profileSprites[_playerStats.LoadProfileImage()];
        public Sprite GetSprite(int index) => profileSprites[index];
        public Sprite GetRandomSprite() => profileSprites[Random.Range(0, profileSprites.Length)];
        public int GetPlayerTrophies() => _playerStats.LoadTrophies();
        public string GetPlayerName() => _playerStats.LoadPlayerName();
    }
}
