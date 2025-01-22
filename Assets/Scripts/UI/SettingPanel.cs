using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingPanel : MonoBehaviour
    {
        [SerializeField] private MMSoundManager soundManager;
        [SerializeField] private Image sfxImage, musicImage;
        [SerializeField] private Slider sfxSlider, musicSlider, uiSlider;
        [SerializeField] private TextMeshProUGUI uiText;
        [SerializeField] private Sprite sfxOn, sfxOff, musicOn, musicOff;
        [SerializeField] private RectTransform toggleRect; 
        [SerializeField] private Transform toggleParent; 
        [SerializeField] private Sprite toggleOnSprite; 
        [SerializeField] private Sprite toggleOffSprite; 
        private bool _isToggleOn = false;

        private const string SfxVolumeKey = "SfxVolume";
        private const string MusicVolumeKey = "MusicVolume";
        private const string UiVolumeKey = "UiVolume";
        private const string ToggleStateKey = "ToggleState";

        private void ToggleSwitch() 
        { 
            _isToggleOn = !_isToggleOn;
            toggleParent.GetComponent<Image>().sprite = _isToggleOn ? toggleOnSprite : toggleOffSprite;
            float xPosition = _isToggleOn ? 50f : -50f;
            toggleRect.anchoredPosition = new Vector2(xPosition, toggleRect.anchoredPosition.y); 

            PlayerPrefs.SetInt(ToggleStateKey, _isToggleOn ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void Start()
        {
            // Load settings from PlayerPrefs
            float sfx = PlayerPrefs.GetFloat(SfxVolumeKey, 1f); // Default value is 1
            float music = PlayerPrefs.GetFloat(MusicVolumeKey, 1f); // Default value is 1
            float ui = PlayerPrefs.GetFloat(UiVolumeKey, 1f); // Default value is 1
            _isToggleOn = PlayerPrefs.GetInt(ToggleStateKey, 0) == 1; // Default value is 0 (off)

            // Set UI elements based on loaded settings
            sfxSlider.value = sfx;
            musicSlider.value = music;
            uiSlider.value = ui;
            toggleParent.GetComponent<Image>().sprite = _isToggleOn ? toggleOnSprite : toggleOffSprite;
            float xPosition = _isToggleOn ? 50f : -50f;
            toggleRect.anchoredPosition = new Vector2(xPosition, toggleRect.anchoredPosition.y); 
            sfxImage.sprite = sfx > 0 ? sfxOn : sfxOff;
            musicImage.sprite = music > 0 ? musicOn : musicOff;
            uiText.color = ui > 0 ? Color.white : Color.gray;

            // Apply settings to the sound manager
            soundManager.SetVolumeUI(ui); 
            soundManager.SetVolumeSfx(sfx); 
            soundManager.SetVolumeMusic(music);

            // Add listeners to sliders
            sfxSlider.onValueChanged.AddListener((value) =>
            {
                sfxImage.sprite = value > 0 ? sfxOn : sfxOff;
                PlayerPrefs.SetFloat(SfxVolumeKey, value); 
                PlayerPrefs.Save();
                soundManager.SetVolumeSfx(value); 
            });
            musicSlider.onValueChanged.AddListener((value) =>
            { 
                musicImage.sprite = value > 0 ? musicOn : musicOff; 
                PlayerPrefs.SetFloat(MusicVolumeKey, value); 
                PlayerPrefs.Save();
                soundManager.SetVolumeMusic(value);
            });
            uiSlider.onValueChanged.AddListener((value) =>
            {
                uiText.color = value > 0 ? Color.white : Color.gray;
                PlayerPrefs.SetFloat(UiVolumeKey, value);
                PlayerPrefs.Save();
                soundManager.SetVolumeUI(value); 
            });

            // Add button to toggle switch
            var button = toggleParent.gameObject.AddComponent<Button>();
            button.onClick.AddListener(ToggleSwitch);
        }
    }
}
