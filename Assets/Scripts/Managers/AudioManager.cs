using Controllers;
using Saveables;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using Utilities;

public class AudioManager : MonoSingleton<AudioManager>
{
    [Header("Dependencies")]

    [SerializeField]
    private AudioMixer _soundEffectsMixer;

    [SerializeField]
    private AudioMixer _musicMixer;

    [Header("Read-only Values")]
    [SerializeField]
    private float _sfxVolume = 75f;

    [SerializeField]
    private float _musicVolume = 75f;


    public UnityEvent<int> AddLayer;
    public UnityEvent<int> RemoveLayer; 
    public UnityEvent<float> UpdateSoundEffectVolume;
    public UnityEvent<float> UpdateMusicVolume;

    private float _lowestDecibelLimit = -80.0f;
    private float _highestDecibelLimit = 20.0f;
    private float _decibelRange = 100.0f;

    protected override void OnInit()
    {
        AddLayer = new();
        RemoveLayer = new();
        
        _sfxVolume = PreferencesController.Instance.Settings.SFXVolume;
        _musicVolume = PreferencesController.Instance.Settings.MusicVolume;
        PreferencesController.Instance.SettingsUpdated.AddListener((controller) => UpdateSoundSettingsFromPreferences(controller.Settings));

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

    }

    private void RemoveLayerFromMusic(int layer)
    {

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

}
