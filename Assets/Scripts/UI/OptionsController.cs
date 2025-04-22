using Managers;
using Saveables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI 
{
    public class OptionsController : MonoBehaviour
    {
        public Slider SfxSlider;
        public Slider MusicSlider;
        public TextMeshProUGUI MusicText;
        public TextMeshProUGUI SfxText;

        private float _sfx;
        private float _music;
        private bool _dirty = false;

        void Start()
        {
            SfxSlider.value = PreferencesManager.Instance.Settings.SFXVolume;
            MusicSlider.value = PreferencesManager.Instance.Settings.MusicVolume;
        }

        public void UpdateSfxVolume()
        {
            _dirty = true;
            _sfx = SfxSlider.value;
            SfxText.text = _sfx.ToString();
        }

        public void UpdatMusicVolume()
        {
            _dirty = true;
            _music = MusicSlider.value;
            MusicText.text = _music.ToString();
        }

        public void SaveSettings()
        {
            if(_dirty)
            {
                var prefs = PreferencesManager.Instance.Settings;
                prefs.SFXVolume = _sfx;
                prefs.MusicVolume = _music;
                PreferencesManager.Instance.SaveSettings(prefs);
            }
        }

        public void ExitGame()
        {
            if(_dirty)
            {
                var prefs = PreferencesManager.Instance.Settings;
                prefs.SFXVolume = _sfx;
                prefs.MusicVolume = _music;
                PreferencesManager.Instance.SaveSettings(prefs);
            }

            Application.Quit();
        }

    }
}