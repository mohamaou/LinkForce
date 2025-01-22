using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "SettingsData", menuName = "ScriptableObjects/SettingsData", order = 1)]
    public class SettingsData : ScriptableObject
    {
        public float sfxVolume;
        public float musicVolume;
        public float uiVolume;
        public bool isToggleOn;

        public void SaveSFXVolume(float sfx)
        {
            sfxVolume = sfx;
        }

        public void SaveMusicVolume(float music)
        {
            musicVolume = music;
        }

        public void SaveUIVolume(float ui)
        {
            uiVolume = ui;
        }

        public void SaveToggleState(bool toggle)
        {
            isToggleOn = toggle;
        }

        public void LoadSettings(out float sfx, out float music, out float ui, out bool toggle)
        {
            sfx = sfxVolume;
            music = musicVolume;
            ui = uiVolume;
            toggle = isToggleOn;
        }
    }
}