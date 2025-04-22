using System;
using UnityEditor.SearchService;
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
        
        [Header("Configuration")]
        [SerializeField]
        private string _defaultActionMapName;

        [SerializeField]
        private string _playerActionMapName;

        [SerializeField]
        private string _familiarActionMapName;

        [Header("Read Only")]
        [SerializeField]
        private string _currentActionMapName = string.Empty;

        private InputActionMap _playerActionMap;

        private InputActionMap _familiarActionMap;

        private ControllerState _previousState = ControllerState.Player;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _playerActionMap = InputSystem.actions.FindActionMap(_playerActionMapName);
            _familiarActionMap = InputSystem.actions.FindActionMap(_familiarActionMapName);
            
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
    }
}