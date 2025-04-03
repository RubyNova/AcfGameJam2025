using UnityEngine;
using UnityEngine.InputSystem;

namespace Controllers
{
    public class InputController : MonoBehaviour
    {
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

        private InputActionMap _playerActionMap;

        private InputActionMap _familiarActionMap;
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
            
        }
    }
}