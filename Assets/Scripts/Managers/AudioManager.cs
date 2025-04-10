using System;
using System.Collections;
using Controllers;
using Saveables;
using ScriptableObjects.Audio;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using Utilities;

namespace Managers 
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        public enum LevelState
        {
            BeamTutorial,
            LFTutorial,
            Puzzle0
        }

        [Header("Dependencies")]

        [SerializeField]
        private AudioMixer _soundEffectsMixer;

        [SerializeField]
        private AudioMixer _musicMixer;

        [SerializeField]
        private AudioSource _musicSourceOne;
        
        [SerializeField]
        private AudioSource _musicSourceTwo;
        
        [SerializeField]
        private AudioSource _musicSourceThree;
        
        [SerializeField]
        private AudioSource _musicSourceFour;
        
        [SerializeField]
        private AudioSource _musicSourceFive;

        [Header("Configuration")]
        [SerializeField]
        private float _fadeInSpeed;

        [SerializeField]
        private float _fadeOutSpeed;

        [Header("Music Values")]
        [SerializeField]
        private LayeredAudioTrack _tutorialMusicTrack;
        

        [Header("Read-only Values")]
        [SerializeField]
        private float _sfxVolume = 75f;

        [SerializeField]
        private float _musicVolume = 75f;

        [SerializeField]
        private float _currentLayerPlayingCount = 0;

        [HideInInspector]
        public UnityEvent<int> AddLayer;
        
        [HideInInspector]
        public UnityEvent<int> RemoveLayer; 
        
        [HideInInspector]
        public UnityEvent<float> UpdateSoundEffectVolume;
        
        [HideInInspector]
        public UnityEvent<float> UpdateMusicVolume;

        private float _lowestDecibelLimit = -80.0f;
        private float _highestDecibelLimit = 20.0f;
        private float _decibelRange = 100.0f;

        protected override void OnInit()
        {
            AddLayer = new();
            RemoveLayer = new();
            
            _sfxVolume = PreferencesManager.Instance.Settings.SFXVolume;
            _musicVolume = PreferencesManager.Instance.Settings.MusicVolume;
            PreferencesManager.Instance.SettingsUpdated.AddListener((controller) => UpdateSoundSettingsFromPreferences(controller.Settings));

            AddLayer.AddListener((layerNumber) => AddLayerToMusic(layerNumber));
            RemoveLayer.AddListener((layerNumber) => RemoveLayerFromMusic(layerNumber));
            UpdateMusicVolume.AddListener(percentage => UpdateMusicVolumeIndependently(percentage));
            UpdateSoundEffectVolume.AddListener(percentage => UpdateSoundEffectVolumeIndependently(percentage));
        }

        private void UpdateSoundSettingsFromPreferences(Preferences settings)
        {
            _sfxVolume = settings.SFXVolume;
            _musicVolume = settings.MusicVolume;

            //Update audio mixer groups
            _soundEffectsMixer.SetFloat("Volume", GetVolumeInDecibels(_sfxVolume));
            _musicMixer.SetFloat("Volume", GetVolumeInDecibels(_musicVolume));
        }

        private void UpdateMusicVolumeIndependently(float percentage)
        {
            _musicVolume = GetVolumeInDecibels(percentage);
            _musicMixer.SetFloat("Volume", GetVolumeInDecibels(_musicVolume));
        }

        private void UpdateSoundEffectVolumeIndependently(float percentage)
        {
            _sfxVolume = GetVolumeInDecibels(percentage);
            _soundEffectsMixer.SetFloat("Volume", GetVolumeInDecibels(_musicVolume));
        }

        private void AddLayerToMusic(int layer)
        {
            //0-based
            if(layer > 4 || layer < 1)  return;

            switch(layer)
            {
                case 1:
                {
                    StartCoroutine(FadeIn(_musicSourceOne, _musicVolume));
                    break;
                }
                case 2:
                {
                    StartCoroutine(FadeIn(_musicSourceTwo, _musicVolume));
                    break;
                }
                case 3:
                {
                    StartCoroutine(FadeIn(_musicSourceThree, _musicVolume));
                    break;
                }
                case 4:
                {
                    StartCoroutine(FadeIn(_musicSourceFour, _musicVolume));
                    break;
                }
            }

            _currentLayerPlayingCount++;
        }

        private void RemoveLayerFromMusic(int layer)
        {
            if(layer > 4) return;

            if(layer > 4 || layer < 1)  return;

            switch(layer)
            {
                case 1:
                {
                    StartCoroutine(FadeOut(_musicSourceOne));
                    break;
                }
                case 2:
                {
                    StartCoroutine(FadeOut(_musicSourceTwo));
                    break;
                }
                case 3:
                {
                    StartCoroutine(FadeOut(_musicSourceThree));
                    break;
                }
                case 4:
                {
                    StartCoroutine(FadeOut(_musicSourceFour));
                    break;
                }
            }

            _currentLayerPlayingCount--;
        }

        private float GetVolumeInDecibels(float percentage)
        {
            if(percentage <= 0.0f)
            {
                return _lowestDecibelLimit;
            }

            if(percentage >= 100.0f)
            {
                return _highestDecibelLimit;
            }

            return Mathf.Clamp(_lowestDecibelLimit + (_decibelRange * percentage), _lowestDecibelLimit, _highestDecibelLimit);
        }

        private float GetNormalizedVolume(float percentage) => 1 * percentage;

        private void StopAllTracks()
        {
            _musicSourceOne.Stop();
            _musicSourceTwo.Stop();
            _musicSourceThree.Stop();
            _musicSourceFour.Stop();
            _musicSourceFive.Stop();
        }

        public void PlayLayeredTrack(LevelState level)
        {
            LayeredAudioTrack track;
            switch(level)
            {
                default:
                {
                    track = _tutorialMusicTrack;
                    break;
                }
            }

            StopAllTracks();

            double baseDuration = 0.0f;
            int layerCount = track.LayeredTracks.Count;

            for(int i = 0; i < layerCount; i++)
            {
                switch(i)
                {
                    case 0:
                    {
                        _musicSourceOne.clip = track.LayeredTracks[i];
                        baseDuration = track.LayeredTracks[i].samples / track.LayeredTracks[i].frequency;
                        break;
                    }
                    case 1:
                    {
                        _musicSourceTwo.clip = track.LayeredTracks[i];
                        break;
                    }
                    case 2:
                    {
                        _musicSourceThree.clip = track.LayeredTracks[i];
                        break;
                    }
                    case 3:
                    {
                        _musicSourceFour.clip = track.LayeredTracks[i];
                        break;
                    }
                    case 4:
                    {
                        _musicSourceFive.clip = track.LayeredTracks[i];
                        break;
                    }
                }
            }

            StartCoroutine(PlayAllTracks(baseDuration));    
            _currentLayerPlayingCount++;       
        }

        //Coroutines
        private IEnumerator FadeIn(AudioSource source, float desiredVolumeInPercentage)
        {
            float normalizedVolume = GetNormalizedVolume(desiredVolumeInPercentage);
            while(!Mathf.Approximately(source.volume, normalizedVolume))
            {
                source.volume += normalizedVolume * _fadeInSpeed * Time.deltaTime;
                yield return null;
            }

            yield break;
        }

        private IEnumerator FadeOut(AudioSource source)
        {
            while(!Mathf.Approximately(source.volume, 0))
            {
                source.volume -= _fadeOutSpeed * Time.deltaTime;
                yield return null;
            }

            yield break;
        }

        private IEnumerator PlayAllTracks(double baseDuration)
        {
            double baseStartTime = AudioSettings.dspTime;
            double layerStartTime = baseStartTime + baseDuration;

            _musicSourceTwo.volume = 0.0f;
            _musicSourceThree.volume = 0.0f;
            _musicSourceFour.volume = 0.0f;
            _musicSourceFive.volume = 0.0f;

            //immediate - base layer
            _musicSourceOne.PlayScheduled(baseStartTime);
            _musicSourceTwo.PlayScheduled(layerStartTime);
            _musicSourceThree.PlayScheduled(layerStartTime);
            _musicSourceFour.PlayScheduled(layerStartTime);
            _musicSourceFive.PlayScheduled(layerStartTime);

            yield break;
        }

    }

}