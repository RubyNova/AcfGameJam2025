using Environment;
using Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

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

        [SerializeField]
        private GameObject _spriteRoot;

        [SerializeField]
        private Light2D _ambientLight;

        public UnityEvent CharacterActivated;

        public UnityEvent CharacterDeactivated;

        public float AudioThresholdDistanceValue = 5f;

        [Header("Read-only Values")]

        [SerializeField]
        private Vector2 _movementVector = Vector2.zero;

        private bool switchCharacters = false;
        private UnityEvent<int> SwitchCamerasEvent = new();
        private InputActionMap _familiarActions;
        private int _modifierIndex = 0;
        private bool _snapToPlayer = false;


        public void EnableCharacter() => ActiveCharacter = true;
        public void DisableCharacter() => ActiveCharacter = false;

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
            
            if(ActiveCharacter)
            {
                AudioManager.Instance.SetLayerVolume(GetHashCode(), 2, 100);
            }
            else
            {
                if(_playerControllerReference is not null)
                {
                    var distance = Vector3.Distance(transform.position, _playerControllerReference.transform.position);
                    var percentage = AudioThresholdDistanceValue / distance * 100;

                    AudioManager.Instance.SetLayerVolume(GetHashCode(), 2, percentage);
                }
            }
        }

        void FixedUpdate()
        {
            if (ActiveCharacter)
            {
                if(_snapToPlayer && _playerControllerReference is not null)
                {
                    _rigidbody.linearVelocity = Vector2.zero;
                    transform.position = _playerControllerReference.FamiliarSnapPoint.position;
                    _snapToPlayer = false;
                }
                else
                    _rigidbody.linearVelocity = _movementVector * _movementSpeed;
            }
            else
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }

        void OnMove(InputValue value)
        {
            if (!ActiveCharacter)
                return;

            _movementVector = value.Get<Vector2>();
        }

        void OnSnapToPlayer(InputValue _)
        {
            if (!ActiveCharacter)
                return;

            _snapToPlayer = true;
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
                //Set flags
                ActiveCharacter = false;
                switchCharacters = false;
                _playerControllerReference.ActiveCharacter = true;
                
                //Drop velocity
                _rigidbody.linearVelocity = Vector2.zero;

                //Change Input Action Maps
                var inputComponent = GetComponentInParent<InputController>();
                if(inputComponent != null)
                {
                    inputComponent.SwapCharacterMaps(true);
                }

                CharacterDeactivated.Invoke();

                //Swap Cameras
                SwitchCamerasEvent.Invoke(1);
            }
        }

        protected void OnCycleBeamModifier()
        {
            if(!ActiveCharacter)
                return;

            CycleModifiers();
        }

        private void CycleModifiers()
        {
            var modifiers = GetComponentsInChildren<LightBeamModifier>();
            
            if (++_modifierIndex >= modifiers.Length)
            {
                _modifierIndex = 0;
            }

            _beamChanger.ChangeBeamModifier(modifiers[_modifierIndex]);

            var renderers = _spriteRoot.GetComponentsInChildren<SpriteRenderer>();

            foreach (var renderer in renderers)
            {
                renderer.color = _beamChanger.BeamModifierData.Colour;  
            }

            _ambientLight.color = _beamChanger.BeamModifierData.Colour;
        }

    }
}