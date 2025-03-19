using System;
using System.Collections.Generic;
using Environment;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Controllers
{

    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private Animator _characterAnimator;

        [SerializeField]
        private FamiliarController _familiarControllerReference;

        [SerializeField]
        private Rigidbody2D _rigidbody;

        [SerializeField]
        private float _movementSpeed;

        [SerializeField]
        private float _jumpForce;

        [SerializeField]
        private float _gravityScale;

        [SerializeField]
        private float _fallingGravityScale;

        [SerializeField]
        public bool ActiveCharacter;

        private bool _jumpRequested = false;
        private bool switchCharacters = false;
        private InputActionMap _playerActions;
        private Vector2 _outsideForces = Vector2.zero;

        private UnityEvent<int> SwitchCamerasEvent = new();
        private Dictionary<int, Vector2> _listOfOutsideForces = new();


        [Header("Read-only Values")]

        [SerializeField]
        private bool _grounded = true;

        [SerializeField]
        private Vector2 _movementVector = Vector2.zero;

        

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            var cinemachineController = FindFirstObjectByType<CinemachineController>();
            if(cinemachineController)
            {
                SwitchCamerasEvent.AddListener((int x) => cinemachineController.CinemachineSwapCameras(x));
            }
            _playerActions = InputSystem.actions.FindActionMap("Player");
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
                _rigidbody.linearVelocityX = _movementVector.x * _movementSpeed;
                if(_outsideForces != Vector2.zero)
                {
                    _rigidbody.Slide(_outsideForces, 1f, new Rigidbody2D.SlideMovement {});
                    _rigidbody.linearDamping = 5; 
                }
                else
                    _rigidbody.linearDamping = 0;
                    

                if (_jumpRequested)
                {
                    if (_grounded)
                    {
                        _rigidbody.linearVelocity += Vector2.up * _jumpForce;
                    }
                    _jumpRequested = false;
                }

                if (_rigidbody.linearVelocityY >= 0 && _grounded)
                {
                    _rigidbody.gravityScale = _gravityScale;
                }
                else if(_rigidbody.linearVelocityY < 0)
                {
                    _rigidbody.gravityScale = _fallingGravityScale;
                }
                else if(_outsideForces != Vector2.zero) //outside forces are at play
                {
                    _rigidbody.gravityScale = 0.2f;
                }
            }
        }

        void OnMove(InputValue value)
        {
            if (!ActiveCharacter)
                return;

            _movementVector = value.Get<Vector2>();
        }

        void OnJump()
        {
            if (!ActiveCharacter)
                return;

            if (!_jumpRequested)
            {
                _jumpRequested = true;
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                _grounded = true;
            }
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                _grounded = false;
            }
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
            if (_familiarControllerReference)
            {
                ActiveCharacter = false;
                switchCharacters = false;
                _familiarControllerReference.ActiveCharacter = true;
                _rigidbody.linearVelocity = Vector2.zero;
                SwitchCamerasEvent.Invoke(2);
            }
        }

        public void RegisterIncomingBeamForce(LightBeamController sender, int beamPriority, Vector2 senderBeamDirection, float beamForce)
        {
            if(!_listOfOutsideForces.ContainsKey(sender.gameObject.GetHashCode()))
            {
                var beamVelocity =senderBeamDirection * beamForce; 
                _outsideForces += beamVelocity;
                _listOfOutsideForces.Add(sender.gameObject.GetHashCode(), beamVelocity);
            }
            // TODO: This method is called every tick that the beam detects the player. The beam priority is a value that increments the more controllers this single beam of light
            // has been through. The senderBeamDirection dictates the direction the beam is flowing. The beamForce value is a raw force to be applied in the given direction, the simplest
            // application of this being senderBeamDirection * beamForce, but I didn't want to just "implement that" in this way. I might change the beamForce parameter to a vec2
            // as we implement new beam modifiers and discover we have a need for that, but since im just following my nose for now, this is the best I got for ya. - Matt
        }

        internal void UnregisterIncomingBeamForce(LightBeamController sender, int beamPriority)
        {
            var beamVelocity = _listOfOutsideForces[sender.gameObject.GetHashCode()];
            _outsideForces -= beamVelocity;
            _listOfOutsideForces.Remove(sender.gameObject.GetHashCode());
                
            // TODO: This method is only called once by the sending beam controller to effectively flag the player is no longer under the control of that particular light beam controller.
            // This method exists to help you clean up any state, or help you track multiple controllers if your implementation requires it, and need a way to figure out which controllers to
            // stop caring about. - Matt
            print("Player unregisters force!");
        }
    }
}