using System.Collections.Generic;
using System.Linq;
using Environment;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Controllers
{

    public class PlayerController : MonoBehaviour
    {
        public const int NO_BEAM_CACHED = 0;
        public const string KEYBOARD_MOUSE_CONTROL_SCHEME = "Keyboard&Mouse";


        [Header("Animation")]

        [SerializeField]
        private Animator _characterAnimator;

        [Header("Character Configuration")]

        [SerializeField]
        private float _movementSpeed;

        [SerializeField]
        private float _jumpForce;

        [SerializeField]
        private float _gravityScale;

        [SerializeField]
        private float _fallingGravityScale;

        [SerializeField]
        [Range(0.1f, 2.0f)]
        private float _fallingMovementSpeedDivider;

        [Header("Character Setup")]

        [SerializeField]
        private FamiliarController _familiarControllerReference;

        [SerializeField]
        public Rigidbody2D _rigidbody;

        [SerializeField]
        public Collider2D _collider;

        [SerializeField]
        private Collider2D _triggerCollider;

        [SerializeField]
        public Transform _spriteRotator;

        [SerializeField]
        private Transform _feetTargetTransform;

        [Header("Read-only Values")]

        [SerializeField]
        public bool Grounded = true;

        [SerializeField]
        private Vector2 _movementVector = Vector2.zero;
        
        [SerializeField]
        private int _cachedAffectingBeam = NO_BEAM_CACHED;
        
        [SerializeField]
        private bool _cachedVelocityUpdate = false;
        
        [SerializeField]
        private Vector2 _cachedVelocity = Vector2.zero;
        
        [SerializeField]
        public bool ActiveCharacter;

        [SerializeField]
        private bool _isRunning = true;

        [SerializeField]
        private Vector2 _outsideForces = Vector2.zero;

        public Vector2 MinColliderPoint;
        public int BeamCollisionCount => _listOfOutsideForces.Count;
        public bool JumpRequested = false;
        private bool switchCharacters = false;
        private InputActionMap _playerActions;
        

        private UnityEvent<int> SwitchCamerasEvent = new();
        private Dictionary<int, LightBeamDataGroup> _listOfOutsideForces = new();
        private InputAction _moveAction;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            var cinemachineController = FindFirstObjectByType<CinemachineController>();
            if(cinemachineController)
            {
                SwitchCamerasEvent.AddListener((int x) => cinemachineController.CinemachineSwapCameras(x));
            }
            _playerActions = InputSystem.actions.FindActionMap("Player");
            _moveAction = _playerActions["Move"];
        }

        // Update is called once per frame
        void Update()
        {
            MinColliderPoint = new Vector2 {x = _collider.bounds.min.x, y = _collider.bounds.min.y};

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

                UpdateAnims();
            }
        }

        void FixedUpdate()
        {
            if (ActiveCharacter)
            {
                if(_rigidbody.linearVelocityY < 0 && !Grounded && _cachedAffectingBeam == NO_BEAM_CACHED)
                {
                    if(_rigidbody.gravityScale != _fallingGravityScale)
                    {
                        _rigidbody.gravityScale = _fallingGravityScale;
                    }

                }
                else
                {
                    if(_rigidbody.gravityScale != _gravityScale)
                    {
                        _rigidbody.gravityScale = _gravityScale;
                    }
                    
                    _rigidbody.AddForce(_movementVector * _movementSpeed * _rigidbody.mass, ForceMode2D.Force);
                }

                if(_outsideForces != Vector2.zero)
                {
                    _rigidbody.linearVelocityX += _outsideForces.x;
                }

                if(_cachedVelocityUpdate)
                {
                    _rigidbody.linearVelocity += _cachedVelocity;
                    _cachedVelocityUpdate = false;
                }
                                
                if (JumpRequested)
                {
                    if (Grounded)
                    {
                        _rigidbody.AddForce(Vector2.up * _jumpForce * _rigidbody.mass, ForceMode2D.Impulse);
                    }
                    JumpRequested = false;
                }
            }
        }

        void OnMove(InputValue value)
        {
            if (!ActiveCharacter)
                return;

            var originalVector = value.Get<Vector2>();

            if(!_isRunning && _moveAction.activeControl?.device is Keyboard)
            {
                originalVector.x *= 0.4f;
            }

            _movementVector = originalVector;
        }

        void OnRun(InputValue _)
        {
            if(!ActiveCharacter) return;

            _isRunning = !_isRunning;
        }

        void OnJump()
        {
            if (!ActiveCharacter)
                return;

            if (!JumpRequested)
            {
                JumpRequested = true;
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                Grounded = true;

                if(_spriteRotator.localEulerAngles.z != 0)
                {
                    _spriteRotator.localEulerAngles = Vector3.zero;
                }
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
                _cachedVelocityUpdate = true;
                _cachedVelocity = velocity;
            }
        }

        public void AddLinearVelocityRaw(Vector2 velocity)
        {
            _cachedVelocity = velocity;
            _cachedVelocityUpdate = true;
        }

        public void RegisterIncomingBeamForce(LightBeamController sender, int beamPriority, Vector2 senderBeamDirection, float beamForce)
        {
            if(!_listOfOutsideForces.ContainsKey(sender.gameObject.GetHashCode()))
            {
                _listOfOutsideForces.Add(sender.gameObject.GetHashCode(), new LightBeamDataGroup { 
                    Priority = beamPriority,
                    DirectionAndForce = senderBeamDirection * beamForce
                });

                _triggerCollider.enabled = true;
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
                    _cachedAffectingBeam = NO_BEAM_CACHED;
                    _triggerCollider.enabled = false;
                }
            }   
            // TODO: This method is only called once by the sending beam controller to effectively flag the player is no longer under the control of that particular light beam controller.
            // This method exists to help you clean up any state, or help you track multiple controllers if your implementation requires it, and need a way to figure out which controllers to
            // stop caring about. - Matt
            
        }
    
        internal void UpdateAnims()
        {
            _characterAnimator.SetFloat("MovementX", _movementVector.x);
            _characterAnimator.SetFloat("MovementY", _rigidbody.linearVelocityY);
            _characterAnimator.SetBool("CollidingWithBeam", _cachedAffectingBeam != NO_BEAM_CACHED);

            if(_movementVector.x != 0)
            {
                FlipCharacterSprite(_movementVector.x > 0);
            }
        }

        public void RotateCharacterToBeam(Vector3 localEulerAngles)
        {
            //Reset rotation first
            if(_spriteRotator.rotation.z > 0)
            {
                _spriteRotator.Rotate(-_spriteRotator.localEulerAngles);
            }
            else if(_spriteRotator.rotation.z < 0)
            {
                _spriteRotator.Rotate(_spriteRotator.localEulerAngles);
            }

            _spriteRotator.Rotate(localEulerAngles);
        }

        public void FlipCharacterSprite(bool normalDirection = true)
        {
            transform.GetPositionAndRotation(out Vector3 pos, out var rot);
            rot.y = normalDirection ? 0 : 180;
            transform.SetPositionAndRotation(pos, rot);
        }

        public LightBeamDataGroup GetCachedBeamData() => _cachedAffectingBeam == NO_BEAM_CACHED ? null : _listOfOutsideForces[_cachedAffectingBeam];

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.gameObject.CompareTag("Wall"))
            {

                var lightBeamController = collision.GetComponentInParent<LightBeamController>();
                if(lightBeamController != null)
                {
                    //Get flipped velocity
                    Vector2 flippedVelocity = FlipVelocity(_rigidbody.linearVelocity, lightBeamController.BeamTransform.right);
                    

                    // Get the distance between feetsies and origin
                    var distance = Vector2.Distance(transform.position, _feetTargetTransform.position);

                    //Get the new position from the LBC's snap point
                    var newPosition = lightBeamController.PlayerSnapPoint;
                    newPosition.y += distance;

                    //identify rotation
                    Transform beamParentTransform = lightBeamController.transform;
                    Vector3 angles = Vector3.zero;

                    if(lightBeamController.BeamTransform.localEulerAngles.z != 0)
                    {
                        angles = lightBeamController.BeamTransform.localEulerAngles.z > 180 ? lightBeamController.BeamTransform.eulerAngles : lightBeamController.BeamTransform.localEulerAngles;
                    }
                    else if(beamParentTransform != null && beamParentTransform.eulerAngles.z != 0)
                    {
                        angles = beamParentTransform.eulerAngles;
                    }

                    print(angles);

                    //Assign stuff to the character
                    FlipCharacterSprite(lightBeamController.BeamTransform.right.x >= 0);
                    RotateCharacterToBeam(angles);
                    transform.position = newPosition;
                    _rigidbody.linearVelocity = flippedVelocity;
                }
            }
        }

        private Vector2 FlipVelocity(Vector2 velocity, Vector2 direction)
        {
            float directionalVelocity = Vector2.Dot(velocity, direction);
            Vector2 newVelocity = new Vector2(directionalVelocity, _rigidbody.linearVelocityY);
            print("Flip: "+newVelocity);
            return newVelocity;
        }
    }
}