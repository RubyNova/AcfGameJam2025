using Environment;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Controllers
{


    public class FamiliarController : MonoBehaviour
    {
        [SerializeField]
        private LightBeamController _beamChanger;

        [SerializeField]
        private PlayerController _playerControllerReference;

        [SerializeField]
        private Rigidbody2D _rigidbody;

        [SerializeField]
        private float _movementSpeed;

        [SerializeField]
        public bool ActiveCharacter;

        [Header("Read-only Values")]

        [SerializeField]
        private Vector2 _movementVector = Vector2.zero;

        private bool switchCharacters = false;
        private UnityEvent<int> SwitchCamerasEvent = new();
        private InputActionMap _familiarActions;
        private int _modifierIndex = 0;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            var cinemachineController = FindFirstObjectByType<CinemachineController>();
            if(cinemachineController)
            {
                SwitchCamerasEvent.AddListener((int x) => cinemachineController.CinemachineSwapCameras(x));
            }
            _familiarActions = InputSystem.actions.FindActionMap("Familiar");
        }

        // Update is called once per frame
        void Update()
        {
            if (switchCharacters && ActiveCharacter)
            {
                SwapCharacters();
            }
        }

        void FixedUpdate()
        {
            if (ActiveCharacter)
            {
                _rigidbody.linearVelocity = _movementVector * _movementSpeed;
            }
        }

        void OnMove(InputValue value)
        {
            if (!ActiveCharacter)
                return;

            _movementVector = value.Get<Vector2>();
        }

        void OnCollisionEnter2D(Collision2D collision)
        {

        }

        void OnCollisionExit2D(Collision2D collision)
        {

        }

        void OnSwap(InputValue value)
        {
            if (value.isPressed && !switchCharacters)
            {
                if (!ActiveCharacter)
                    return;

                switchCharacters = true;
            }
        }

        void OnZoomOut(InputValue value)
        {
            if (value.isPressed && !switchCharacters)
            {
                if (!ActiveCharacter)
                    return;

                SwitchCamerasEvent.Invoke(3);
            }
        }

        private void SwapCharacters()
        {
            if (_playerControllerReference)
            {
                ActiveCharacter = false;
                switchCharacters = false;
                _playerControllerReference.ActiveCharacter = true;
                _rigidbody.linearVelocity = Vector2.zero;
                SwitchCamerasEvent.Invoke(1);
            }
        }

        protected void OnCycleBeamModifier()
        {
            CycleModifiers();
        }

        private void CycleModifiers()
        {
            var modifiers = GetComponentsInChildren<LightBeamModifier>();
            _beamChanger.ChangeBeamModifier(modifiers[_modifierIndex]);

            if (++_modifierIndex >= modifiers.Length)
            {
                _modifierIndex = 0;
            }
        }

    }
}