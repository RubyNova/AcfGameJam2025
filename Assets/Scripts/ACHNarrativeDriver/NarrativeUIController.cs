using System.Collections;
using System.Text;
using ACHNarrativeDriver.Api;
using ACHNarrativeDriver.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ACHNarrativeDriver
{
    public class NarrativeUIController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _narrativeTextBox;
        [SerializeField] private TMP_Text _characterNameTextBox;
        [SerializeField] private Image _characterRenderer;
        [SerializeField] private Image _backgroundRenderer;
        [SerializeField] private Image _nameplateRenderer;
        [SerializeField] private Transform _choicesButtonView;
        [SerializeField] private GameObject _buttonPrefab;
        [SerializeField] private GameObject _nextButton;
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private UnityEvent _preNarrativeEvent;
        [SerializeField] private UnityEvent _postNarrativeEvent;
        
        //private AudioController _audioController; // this is such a hack reeeeeeee
        public UnityEvent listNextEvent;

        private Coroutine _rollingTextRoutine;
        private readonly WaitForSeconds _rollingCharacterTime = new(0.04f);
        private NarrativeSequence _currentNarrativeSequence;
        private bool _isCurrentlyExecuting;
        private bool _nextDialogueLineRequested;
        private int _currentDialogueIndex;
        private Interpreter _narrativeInterpreter;
        private RuntimeVariables _narrativeRuntimeVariables;
        
        public NarrativeSequence LastPlayedSequence { get; private set; }
        
        private InputActionMap _uiInputActionMap;
        
        private void Awake()
        {
            _isCurrentlyExecuting = false;
            _currentDialogueIndex = 0;
            _narrativeInterpreter = new();
            _narrativeRuntimeVariables = FindObjectOfType<RuntimeVariables>();

            _dialoguePanel.SetActive(false);
            _uiInputActionMap = InputSystem.actions.FindActionMap("UI");
            if(_uiInputActionMap is not null)
            {
                var interact = _uiInputActionMap["Interact"];
                if(interact is not null)
                {
                    interact.performed += (context) => ExecuteNextDialogueLine();
                }
            }
            //_audioController = FindObjectOfType<AudioController>(); // let the hax continue
        }

        private void Update()
        {
            if (!_isCurrentlyExecuting || !_nextDialogueLineRequested)
            {
                return;
            }

            if (_currentDialogueIndex >= _currentNarrativeSequence.CharacterDialoguePairs.Count && _rollingTextRoutine is null)
            {
                _currentDialogueIndex = 0;

                if (_currentNarrativeSequence.Choices.Count > 0)
                {
                    _choicesButtonView.gameObject.SetActive(true);
                    foreach (Transform child in _choicesButtonView) //explicit variable type here because U N I T Y (TM)
                    {
                        Destroy(child.gameObject);
                    }

                    foreach (var choice in _currentNarrativeSequence.Choices)
                    {
                        var go = Instantiate(_buttonPrefab, _choicesButtonView);
                        go.GetComponentInChildren<TMP_Text>().text = choice.ChoiceText;
                        go.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            _currentNarrativeSequence = choice.NarrativeResponse;
                            _nextDialogueLineRequested = true;
                            _choicesButtonView.gameObject.SetActive(false);
                            _dialoguePanel.SetActive(true);
                            _nextButton.SetActive(true);
                        });
                    }

                    _nextDialogueLineRequested = false;
                    _choicesButtonView.gameObject.SetActive(true);
                    _dialoguePanel.SetActive(false);
                    _nextButton.SetActive(false);
                    return;
                }

                if (_currentNarrativeSequence.NextSequence is null)
                {
                    LastPlayedSequence = _currentNarrativeSequence;
                    listNextEvent.Invoke();
                    _dialoguePanel.SetActive(false);
                    _isCurrentlyExecuting = false;
                    _currentNarrativeSequence = null;
                    //ApplyBackground();
                    return;
                }

                _currentNarrativeSequence = _currentNarrativeSequence.NextSequence;
            }

            _nextDialogueLineRequested = false;

            if (_rollingTextRoutine is not null)
            {
                ResetRollingTextRoutine();
                _narrativeTextBox.text =
                    _currentNarrativeSequence.CharacterDialoguePairs[_currentDialogueIndex - 1].Text;
                return;
            }

            var characterDialogueInfo = _currentNarrativeSequence.CharacterDialoguePairs[_currentDialogueIndex];

            if (characterDialogueInfo.PoseIndex is { } poseIndex)
            {
                _characterRenderer.sprite = characterDialogueInfo.Character.Poses[poseIndex];
                
                if (!_characterRenderer.enabled && _characterRenderer.sprite != null)
                {
                    _characterRenderer.enabled = true;
                }
            }
            
            if(_nameplateRenderer != null &&  characterDialogueInfo.Character.NameplateSprite != null)
                _nameplateRenderer.sprite = characterDialogueInfo.Character.NameplateSprite;

            // if (characterDialogueInfo.PlayMusicIndex is { } playMusicIndex)
            // {
            //     _audioController.PlayMusic(_currentNarrativeSequence.MusicFiles[playMusicIndex]);
            // }

            // if (characterDialogueInfo.PlaySoundEffectIndex is {} soundEffectIndex)
            // {
            //     _audioController.PlayEffect(_currentNarrativeSequence.SoundEffectFiles[soundEffectIndex]);
            // }
            
            _rollingTextRoutine =
                StartCoroutine(
                    PerformRollingText(characterDialogueInfo));
            _currentDialogueIndex++;
            
            ApplyBackground();
        }
        
        private void ApplyBackground()
        {
            if (_currentNarrativeSequence is null || _currentNarrativeSequence.BackgroundSprite is null || _backgroundRenderer is null || _backgroundRenderer.sprite == _currentNarrativeSequence.BackgroundSprite) return;
            
            _backgroundRenderer.sprite = _currentNarrativeSequence.BackgroundSprite;
            _backgroundRenderer.enabled = true;
            _characterRenderer.enabled = false;
        }

        private void ResetRollingTextRoutine()
        {
            StopCoroutine(_rollingTextRoutine);
            _rollingTextRoutine = null;
        }

        private IEnumerator PerformRollingText(NarrativeSequence.CharacterDialogueInfo targetDialogueInfo)
        {
            StringBuilder sb = new();

            if(_characterNameTextBox != null)
                _characterNameTextBox.text = _narrativeInterpreter.ResolveRuntimeVariables(targetDialogueInfo.Character.Name, _narrativeRuntimeVariables.ReadOnlyVariableView);

            var resolvedText = _narrativeInterpreter.ResolveRuntimeVariables(targetDialogueInfo.Text, _narrativeRuntimeVariables != null ? _narrativeRuntimeVariables.ReadOnlyVariableView : null);

            foreach (var character in resolvedText)
            {
                sb.Append(character);
                _narrativeTextBox.text = sb.ToString();
                yield return _rollingCharacterTime;
            }

            _rollingTextRoutine = null;
        }

        public void ExecuteSequence(NarrativeSequence targetSequence)
        {
            if (_rollingTextRoutine is not null)
            {
                ResetRollingTextRoutine();
            }

            if(_preNarrativeEvent is not null)
                _preNarrativeEvent.Invoke();

            _dialoguePanel.SetActive(true);
            _narrativeTextBox.text = string.Empty;
            if(_characterNameTextBox != null)
                _characterNameTextBox.text = string.Empty;
            _currentNarrativeSequence = targetSequence;
            _nextDialogueLineRequested = true;
            _isCurrentlyExecuting = true;
        }

        public void ExecuteNextDialogueLine()
        {
            if (_currentNarrativeSequence is null)
            {
                return;
            }

            _nextDialogueLineRequested = true;
        }
    }
}