using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ACHNarrativeDriver.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewNarrativeSequence", menuName = "ACH Narrative Driver / NarrativeSequence")]
    public class NarrativeSequence : ScriptableObject
    {
        [Serializable]
        public class CharacterDialogueInfo
        {
            [SerializeField] private Character _character;
            [SerializeField] private Character _characterTwo;
            [SerializeField] private bool _hasPoseIndex;
            [SerializeField] private bool _hasSecondCharacterPoseIndex;
            [SerializeField] private int _poseIndex;
            [SerializeField] private int _poseIndexTwo;
            [SerializeField] private string _text;
            [SerializeField] private bool _leftCharacterTalking = true;
            [SerializeField] private bool _narratorSpeaking = false;

            public Character Character
            {
                get => _character;
                set => _character = value;
            }

            public Character CharacterTwo
            {
                get => _characterTwo;
                set => _characterTwo = value;
            }

            public int? PoseIndex
            {
                get => _hasPoseIndex ? _poseIndex : null;
                set
                {
                    if (value.HasValue)
                    {
                        _poseIndex = value.Value;
                        _hasPoseIndex = true;
                    }
                    else
                    {
                        _hasPoseIndex = false;
                    }
                }
            }

            public int? SecondPoseIndex
            {
                get => _hasSecondCharacterPoseIndex ? _poseIndexTwo : null;
                set
                {
                    if (value.HasValue)
                    {
                        _poseIndexTwo = value.Value;
                        _hasSecondCharacterPoseIndex = true;
                    }
                    else
                    {
                        _hasSecondCharacterPoseIndex = false;
                    }
                }
            }

            public bool LeftCharacterTalking
            {
                get => _hasSecondCharacterPoseIndex ? _leftCharacterTalking : true;
                set
                {
                    _leftCharacterTalking = value;
                }
            }

            public bool NarratorSpeaking
            {
                get => _narratorSpeaking;
                set
                {
                    _narratorSpeaking = value;
                }
            }


            public string Text
            {
                get => _text;
                set => _text = value;
            }

            public override string ToString()
            {
                return $"Character: {Character.Name}, HasPoseIndex: {_hasPoseIndex}, {(_hasPoseIndex ? "PoseIndex: " + PoseIndex + ", " : string.Empty)}Text: {Text}";
            }
        }

        [Serializable]
        public class ChoiceInfo
        {
            [SerializeField] private string _choiceText;
            [SerializeField] private NarrativeSequence _narrativeResponse;

            public string ChoiceText
            {
                get => _choiceText;
                set => _choiceText = value;
            }

            public NarrativeSequence NarrativeResponse
            {
                get => _narrativeResponse;
                set => _narrativeResponse = value;
            }
        }

        [SerializeField] private Sprite _backgroundSprite;
        [SerializeField] private NarrativeSequence _nextSequence;
        [SerializeField] private List<AudioClip> _musicFiles;
        [SerializeField] private List<AudioClip> _soundEffectFiles;
        [SerializeField] private List<CharacterDialogueInfo> _characterDialoguePairs;
        [SerializeField] private List<ChoiceInfo> _choices;

        public List<ChoiceInfo> Choices
        {
            get
            {
                _choices ??= new();
                return _choices;
            }

            set => _choices = value;
        }

        public NarrativeSequence NextSequence
        {
            get => _nextSequence;
            set => _nextSequence = value;
        }

        public List<AudioClip> MusicFiles
        {
            get => _musicFiles;
            set => _musicFiles = value;
        }

        public List<AudioClip> SoundEffectFiles
        {
            get => _soundEffectFiles;
            set => _soundEffectFiles = value;
        }

        public List<CharacterDialogueInfo> CharacterDialoguePairs
        {
            get => _characterDialoguePairs;
            set => _characterDialoguePairs = value;
        }

        public Sprite BackgroundSprite
        {
            get => _backgroundSprite;
            set => _backgroundSprite = value;
        }

        [field: SerializeField, HideInInspector]
        public string SourceScript { get; set; }
    }
}
