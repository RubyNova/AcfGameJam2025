using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controllers
{
    public class InputController : MonoBehaviour
    {
        public enum ControllerState
        {
            Familiar,
            Player
        }

        [Header("Dependencies")]
        [SerializeField]
        private PlayerInput _playerInputComponent;

        [SerializeField] private GameObject _pausePanel;

        [SerializeField] private OptionsController _options;
        
        [Header("Configuration")]
        [SerializeField]
        private string _defaultActionMapName;

        [SerializeField]
        private string _playerActionMapName;

        [SerializeField]
        private string _familiarActionMapName;

        [SerializeField]
        private InputActionMap _uiInputActionMap;

        [Header("Read Only")]
        [SerializeField]
        private string _currentActionMapName = string.Empty;

        private InputActionMap _playerActionMap;

        private InputActionMap _familiarActionMap;

        private ControllerState _previousState = ControllerState.Player;
        private float _timeScale = 0;

        private bool _paused = false;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _playerActionMap = InputSystem.actions.FindActionMap(_playerActionMapName);
            _familiarActionMap = InputSystem.actions.FindActionMap(_familiarActionMapName);
            _uiInputActionMap = InputSystem.actions.FindActionMap("UI");
            // if(_uiInputActionMap is not null)
            // {
            //     var pause = _uiInputActionMap["Pause"];
            //     if(pause is not null)
            //     {
            //         pause.performed += (context) => PauseGame();
            //     }
            // }
            
            _playerInputComponent.SwitchCurrentActionMap(_defaultActionMapName);
        }

        // Update is called once per frame
        void Update()
        {
            _currentActionMapName = _playerInputComponent.currentActionMap.name;
        }

        public void SwapCharacterMaps(bool player)
        {
            if(player)
            {
                _previousState = ControllerState.Player;
                _playerInputComponent.SwitchCurrentActionMap(_playerActionMapName);
            }
            else
            {
                _previousState = ControllerState.Familiar;
                _playerInputComponent.SwitchCurrentActionMap(_familiarActionMapName);
            }
        }

        public void ResetControls()
        {
            if(_previousState == ControllerState.Player)
            {
                _playerInputComponent.SwitchCurrentActionMap(_playerActionMapName);
                var pc = FindAnyObjectByType<PlayerController>();
                if(pc is not null)
                {
                    pc.EnableCharacter();
                }
            }
            else
            {
                _playerInputComponent.SwitchCurrentActionMap(_familiarActionMapName);
                var pc = FindAnyObjectByType<FamiliarController>();
                if(pc is not null)
                {
                    pc.EnableCharacter();
                }
            }
        }

        public void RevokeControls()
        {
            _playerInputComponent.SwitchCurrentActionMap("UI");
        }

        void OnPause()
        {
            PauseGame();
        }

        private void PauseGame()
        {
            if(!_paused)
            {
                if(_pausePanel is not null)
                {
                    _pausePanel.SetActive(true);
                    RevokeControls();
                    _timeScale = Time.timeScale;
                    Time.timeScale = 0;
                    _paused = true;
                }
            }
            else
            {
                if(_pausePanel is not null)
                {
                    _pausePanel.SetActive(false);
                }
                if(_options is not null)
                {
                    _options.SaveSettings();
                }
                ResetControls();
                Time.timeScale = _timeScale;
                _paused = false;
            }
        }
    }
}
