using System.Collections.Generic;
using System.Linq;
using Environment;
using Unity.IO.LowLevel.Unsafe;
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

        [SerializeField]
        public Collider2D _collider;

        private bool _jumpRequested = false;
        private bool switchCharacters = false;
        private InputActionMap _playerActions;
        private Vector2 _outsideForces = Vector2.zero;

        private UnityEvent<int> SwitchCamerasEvent = new();
        private Dictionary<int, LightBeamDataGroup> _listOfOutsideForces = new();
        private int _cachedAffectingBeam = 0;

        public Vector2 MinColliderPoint;
        public int BeamCollisionCount;


        [Header("Read-only Values")]

        [SerializeField]
        public bool Grounded = true;

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
            MinColliderPoint = new Vector2 {x = _collider.bounds.min.x, y = _collider.bounds.min.y};
            BeamCollisionCount = _listOfOutsideForces.Count;

            if (switchCharacters && ActiveCharacter)
            {
                SwapCharacters();
            }

            if(ActiveCharacter)
            {
                _outsideForces = Vector2.zero;
                //Sort by priority and only apply the right outside forces if applicable
                if(_listOfOutsideForces.Count > 1)
                {
                    KeyValuePair<int, LightBeamDataGroup> forceWithMaxPriority = _listOfOutsideForces.Aggregate(
                        (left, right) => left.Value.Priority > right.Value.Priority ? left : right);
                    _cachedAffectingBeam = forceWithMaxPriority.Key;
                    _outsideForces += forceWithMaxPriority.Value.DirectionAndForce;
                }
                else if(_listOfOutsideForces.Count > 0)
                {
                    var force = _listOfOutsideForces.ElementAt(0);
                    _cachedAffectingBeam = force.Key;
                    _outsideForces += force.Value.DirectionAndForce;
                    if(transform.rotation.z > 0)
                    {
                        _outsideForces.y = 0.0f;
                    }
                }
            }
        }

        void FixedUpdate()
        {
            if (ActiveCharacter)
            {
                _rigidbody.AddForce(_movementVector * _movementSpeed, ForceMode2D.Force);

                if(_outsideForces != Vector2.zero)
                {
                    _rigidbody.linearVelocity += _outsideForces;
                }
                                
                if (_jumpRequested)
                {
                    if (Grounded)
                    {
                        _rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
                    }
                    _jumpRequested = false;
                }
                //TODO: modify jumping if needed after anims wired up
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
                Grounded = true;
            }
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                Grounded = false;
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

        public void AddLinearVelocity(int cachedAffectingBeamHash, Vector2 velocity)
        {
            if(_cachedAffectingBeam == cachedAffectingBeamHash)
            {
                _rigidbody.linearVelocity += velocity;
            }
        }

        public void AddLinearVelocityRaw(Vector2 velocity)
        {
                _rigidbody.linearVelocity += velocity;
        }

        public void RotateCharacter(Vector3 eulerAngles) 
        {
            // Rotation is absolutely fucked right now because we need to
            // accurately and slowly transition the character back upright after launching them
            // with beams... I cannot figure out the proper way so we'll come back to this soon.
            // - kenny
            //transform.Rotate(eulerAngles);
        }

        public void RegisterIncomingBeamForce(LightBeamController sender, int beamPriority, Vector2 senderBeamDirection, float beamForce)
        {
            if(!_listOfOutsideForces.ContainsKey(sender.gameObject.GetHashCode()))
            {
                _listOfOutsideForces.Add(sender.gameObject.GetHashCode(), new LightBeamDataGroup { 
                    Priority = beamPriority,
                    DirectionAndForce = senderBeamDirection * beamForce
                });
            }
            // TODO: This method is called every tick that the beam detects the player. The beam priority is a value that increments the more controllers this single beam of light
            // has been through. The senderBeamDirection dictates the direction the beam is flowing. The beamForce value is a raw force to be applied in the given direction, the simplest
            // application of this being senderBeamDirection * beamForce, but I didn't want to just "implement that" in this way. I might change the beamForce parameter to a vec2
            // as we implement new beam modifiers and discover we have a need for that, but since im just following my nose for now, this is the best I got for ya. - Matt
        }

        internal void UnregisterIncomingBeamForce(LightBeamController sender, int beamPriority)
        {
            if(_listOfOutsideForces.ContainsKey(sender.gameObject.GetHashCode()))
            {
                _listOfOutsideForces.Remove(sender.gameObject.GetHashCode());
                if(_listOfOutsideForces.Count == 0)
                {
                    _cachedAffectingBeam = 0;
                }
            }   
            // TODO: This method is only called once by the sending beam controller to effectively flag the player is no longer under the control of that particular light beam controller.
            // This method exists to help you clean up any state, or help you track multiple controllers if your implementation requires it, and need a way to figure out which controllers to
            // stop caring about. - Matt
            
        }
    }
}