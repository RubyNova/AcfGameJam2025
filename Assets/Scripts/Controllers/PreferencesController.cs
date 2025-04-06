using System;
using Saveables;
using UnityEngine;
using UnityEngine.Events;
using Utilities;


namespace Controllers 
{
    public class PreferencesController : MonoSingleton<PreferencesController>
    {
        public UnityEvent<PreferencesController> SettingsUpdated;

        private const string _playerPrefencesName = "settings";

        public Preferences Settings = new();

        protected override void OnInit()
        {
            if(SettingsUpdated == null)
            {
                SettingsUpdated = new();
            }
            LoadPrefs();
        }

        public void SaveSettings(Preferences preferredPrefs)
        {
            Settings = preferredPrefs;
            var json = JsonUtility.ToJson(Settings);
            PlayerPrefs.SetString(_playerPrefencesName, json);
            SettingsUpdated.Invoke(this);
        }

        public void LoadPrefs()
        {
            var json = PlayerPrefs.GetString(_playerPrefencesName);

            if(string.IsNullOrWhiteSpace(json))
            {
                Debug.Log("Could not load settings - recreating!");
                
                //Should create defaults here;
                Settings = new();
                
                PlayerPrefs.SetString(_playerPrefencesName, JsonUtility.ToJson(Settings));
                SettingsUpdated.Invoke(this);
                return;
            }
            else
            {
                Settings = JsonUtility.FromJson<Preferences>(json);
                SettingsUpdated.Invoke(this);
            }
        }
    }

}
