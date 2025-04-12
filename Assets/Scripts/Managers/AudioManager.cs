using System;
using System.Collections;
using Saveables;
using ScriptableObjects.Audio;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using Unity.VisualScripting;

namespace Managers 
{
    public class AudioManager : MonoBehaviour
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
        private float _fadeInSpeed = 0.75f;

        [SerializeField]
        private float _fadeOutSpeed = 0.75f;

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
        public UnityEvent<float> UpdateSoundEffectVolume = new();
        
        [HideInInspector]
        public UnityEvent<float> UpdateMusicVolume = new();

        private float _lowestDecibelLimit = -80.0f;
        private float _highestDecibelLimit = 20.0f;
        private float _decibelRange = 100.0f;


        //Singleton-specific Setup
        public static string PrefabPath = "Prefabs/AudioManager";

        private static AudioManager _instance;

		public static AudioManager Instance
		{
			get
			{
				if (_instance == null)
				{
					var result = FindFirstObjectByType<AudioManager>();
					if(result != null)
					{
						_instance = result;
					}
					else
					{
                        var o = Resources.Load(PrefabPath);
						_instance = Instantiate(o).GetComponent<AudioManager>();
					}
				}

				return _instance;
			}
		}

        public static bool HasInstanceCreated => _instance != null;

		private bool _isInitialised;

        private void Awake()
        {
            if (_isInitialised)
			{
				return;
			}

			if (HasInstanceCreated)
			{
				throw new InvalidOperationException("Multiple instances of a singleton have been instantiated. This is not allowed.");
			}

			Init();
        }

        public void Init()
        {
			DontDestroyOnLoad(gameObject);
			OnInit();
			_instance = this;
			_isInitialised = true;
        }


        protected void OnInit()
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

            PlayLayeredTrack(LevelState.BeamTutorial);
        }
        //End Singleton-specific Setup

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
                    StartCoroutine(FadeIn(_musicSourceTwo, _musicVolume));
                    break;
                }
                case 2:
                {
                    StartCoroutine(FadeIn(_musicSourceThree, _musicVolume));
                    break;
                }
                case 3:
                {
                    StartCoroutine(FadeIn(_musicSourceFour, _musicVolume));
                    break;
                }
                case 4:
                {
                    StartCoroutine(FadeIn(_musicSourceFive, _musicVolume));
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
                    StartCoroutine(FadeOut(_musicSourceTwo));
                    break;
                }
                case 2:
                {
                    StartCoroutine(FadeOut(_musicSourceThree));
                    break;
                }
                case 3:
                {
                    StartCoroutine(FadeOut(_musicSourceFour));
                    break;
                }
                case 4:
                {
                    StartCoroutine(FadeOut(_musicSourceFive));
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

        private float GetNormalizedVolume(float percentage) => Mathf.Clamp(percentage, 0, 100) /100;

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
                if (normalizedVolume - source.volume < 0.1)
                {
                    source.volume = normalizedVolume;
                }
                yield return null;
            }

            yield break;
        }

        private IEnumerator FadeOut(AudioSource source)
        {
            while(!Mathf.Approximately(source.volume, 0))
            {
                source.volume -= _fadeOutSpeed * Time.deltaTime;
                if (source.volume < 0.05)
                {
                    source.volume = 0;
                }
                yield return null;
            }

            yield break;
        }

        private IEnumerator PlayAllTracks(double baseDuration)
        {
            double baseStartTime = AudioSettings.dspTime;
            double layerStartTime = baseStartTime;

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