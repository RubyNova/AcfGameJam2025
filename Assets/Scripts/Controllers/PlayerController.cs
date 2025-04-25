using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Environment;
using Environment.Interactables;
using Managers;
using Saveables;
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

        [Header("Configuration")]

        [SerializeField]
        private float _movementSpeed;

        //Temporary til we find a good value
        [SerializeField]
        [Range(0.01f, 0.99f)]
        private float _walkSpeed = 0.5f;

        [SerializeField]
        private float _jumpForce;

        [SerializeField]
        private float _gravityScale;

        [SerializeField]
        private float _fallingGravityScale;

        [SerializeField]
        public float BeamGravityScale = 0.5f;

        [SerializeField]
        [Range(0.01f, 1.0f)]
        private float _triggerVerticalLaunchDamping;

        [SerializeField]
        public float BeamVerticalLaunchDamping = 1.0f;

        [SerializeField]
        [Range(0.1f, 2.0f)]
        private float _fallingMovementSpeedDivider;

        //Controls how much movement on the X axis there needs to be when falling
        //before falling gravity kicks in
        [SerializeField]
        private float _fallingVelocityXThreshold = 0.1f;

        [SerializeField]
        private float _interactionRayLength;

        [SerializeField]
        private float _baseVelocityCap = new Vector2(1, 1).sqrMagnitude;

        [SerializeField]
        private float _outsideForceInfluenceMultiplier = 100;

        [Header("Dependencies")]

        [SerializeField]
        private FamiliarController _familiarControllerReference;

        [SerializeField]
        private PlayerAudioController _audioController;

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
        
        [SerializeField]
        private Transform _heldItemPoint;

        [SerializeField]
        private GameObject _sparksObject;

        [SerializeField] public Transform FamiliarSnapPoint;

        [Header("Read-only Values")]

        [SerializeField]
        public bool ActiveCharacter;

        [SerializeField]
        public bool Grounded = true;

        [SerializeField]
        private bool _toggleSprintEnabled = true;

        private bool _isRunning = true;

        [SerializeField]
        public bool JumpRequested = false;

        [SerializeField]
        private bool _keyboardIsDevice = false;

        [SerializeField]
        private int _cachedAffectingBeam = NO_BEAM_CACHED;

        [SerializeField]
        private int _biggestOutsideForcesCount;
        
        [SerializeField]
        private float _currentVelocityCap;
        
        [SerializeField]
        private Vector2 _currentMovementVector = Vector2.zero;

        [SerializeField]
        private Vector2 _cachedMovementVector = Vector2.zero;

        [SerializeField]
        private Vector2 _outsideForces = Vector2.zero;

        [SerializeField]
        public Vector2 MinColliderPoint;

        [SerializeField]
        private Vector3 _feetsiesGlobalPosition;

        [SerializeField]
        private bool _wasRidingBeam = false;
        
        public int BeamCollisionCount => _listOfOutsideForces.Count;
        public bool Triggered => _triggered;
        public InteractableBehaviour HeldObject { get; set; }
        public Transform HeldItemPoint => _heldItemPoint;

        public bool WasRidingBeam {get => _wasRidingBeam; set => _wasRidingBeam = value; }

        private bool switchCharacters = false;
        private InputActionMap _playerActions;
        private UnityEvent<int> SwitchCamerasEvent = new();
        private Dictionary<int, LightBeamDataGroup> _listOfOutsideForces = new();
        private InputAction _moveAction;
        private InputAction _interactAction;
        private bool _resetMovement = false;
        private bool _triggered = false;
        private float _capMultiplierForOutsideForces;
        private bool _hopDownTriggered = false;
        private Coroutine _resetLayerRoutine;

        public bool DisableFall = false;

        void Start()
        {
            _currentVelocityCap = _baseVelocityCap;
            _toggleSprintEnabled = PreferencesManager.Instance.Settings.ToggleSprint;

            PreferencesManager.Instance.SettingsUpdated.AddListener((controller) => UpdatePlayerSpecificSettings(controller.Settings));

            var cinemachineController = FindFirstObjectByType<CinemachineController>();
            if (cinemachineController)
            {
                SwitchCamerasEvent.AddListener((int x) => cinemachineController.CinemachineSwapCameras(x));
            }

            //Setup Input specific to player
            _playerActions = InputSystem.actions.FindActionMap("Player");
            _interactAction = _playerActions["Interact"];

            _moveAction = _playerActions["Move"];
            if(_moveAction != null)
            {
                _moveAction.performed += (context) => HandleMovement(context);
                _moveAction.canceled += (context) => HandleMovementCanceled(context);
            }
            
            // _runAction = _playerActions["Run"];
            // if(_runAction != null)
            // {
            //     _runAction.performed += (context) => HandleSprinting(context);
            //     _runAction.canceled += (context) => HandleSprintCancel(context);
            // }
        }

        private IEnumerator ResetLayerRoutine()
        {
            float timer = 0;
            float limit = 0.5f;
            
            while (timer < limit)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            gameObject.layer = LayerMask.NameToLayer("Player");
            _resetLayerRoutine = null;
        }

        void Update()
        {
            _feetsiesGlobalPosition = _feetTargetTransform.transform.position;
            if(_feetTargetTransform != null)
            {
                MinColliderPoint = new Vector2(_feetTargetTransform.position.x, _feetTargetTransform.position.y);
            }
            else
            {
                MinColliderPoint = new Vector2(_collider.bounds.min.x, _collider.bounds.min.y);
            }
            

            if (switchCharacters && ActiveCharacter)
            {
                if(_familiarControllerReference != null && _familiarControllerReference.gameObject.activeSelf)
                {
                    SwapCharacters();
                    _familiarControllerReference.CharacterActivated.Invoke();
                }
            }

            if (ActiveCharacter)
            {
                //Interactions
                if (_interactAction != null && _interactAction.WasPressedThisFrame())
                {
                    HandleInteractions();
                }

                //HopDown
                if(_hopDownTriggered)
                {
                    _hopDownTriggered = false;
                    
                    var data = GetCachedBeamData();

                    if(data != null && data.CanBeExited)
                    {
                        //do things
                        if (_resetLayerRoutine == null)
                        {
                            print("Exiting!");
                            gameObject.layer = LayerMask.NameToLayer("IgnoreBeams");
                            _resetLayerRoutine = StartCoroutine(ResetLayerRoutine());
                        }

                        ResetRotation();
                        _rigidbody.linearVelocity = Vector2.zero;
                        _listOfOutsideForces.Remove(_cachedAffectingBeam);
                        if (_listOfOutsideForces.Count == 0)
                        {
                            _cachedAffectingBeam = NO_BEAM_CACHED;
                            _triggerCollider.enabled = false;
                        }
                    }
                }

                //Movement
                if(_resetMovement)
                {
                    _resetMovement = false;
                    _currentMovementVector = _cachedMovementVector = Vector2.zero;
                }
                else
                {
                    if(_isRunning && _cachedMovementVector.x != 0.0f && _keyboardIsDevice)
                    {
                        _cachedMovementVector = new Vector2(_cachedMovementVector.x < 0 ? -1 : 1, 0);
                    }
                    else if (_cachedMovementVector.x != 0.0f && _keyboardIsDevice)
                    {
                        _cachedMovementVector = new Vector2(_cachedMovementVector.x < 0 ? -_walkSpeed : _walkSpeed, 0);
                    }
                    _currentMovementVector = _cachedMovementVector;
                }
                


            }
            else
            {
                if (_currentMovementVector.x != 0 || _currentMovementVector.y != 0)
                {
                    _currentMovementVector = Vector2.zero;
                }
            }
                // Outside Forces
                _outsideForces = Vector2.zero;
                //Sort by priority and only apply the right outside forces if applicable
                int outsideForcesCount = _listOfOutsideForces.Count;

                if (outsideForcesCount > 1)
                {
                    KeyValuePair<int, LightBeamDataGroup> forceWithMaxPriority = _listOfOutsideForces.Aggregate(
                        (left, right) => left.Value.Priority > right.Value.Priority ? left : right);
                    _cachedAffectingBeam = forceWithMaxPriority.Key;
                    _outsideForces += forceWithMaxPriority.Value.DirectionAndForce;
                }
                else if (outsideForcesCount > 0)
                {
                    var force = _listOfOutsideForces.ElementAt(0);
                    _cachedAffectingBeam = force.Key;
                    _outsideForces += force.Value.DirectionAndForce;
                    if (transform.rotation.z > 0)
                    {
                        _outsideForces.y = 0.0f;
                    }
                }
                
                if (_biggestOutsideForcesCount < outsideForcesCount)
                {
                    _biggestOutsideForcesCount = outsideForcesCount;
                }
            UpdateAnims();

            var beamData = GetCachedBeamData();

            if (beamData != null && !_sparksObject.activeInHierarchy)
            {
                _audioController.PlayGrind();
                var sparksControls = _sparksObject.GetComponent<ParticleSystem>().main;
                sparksControls.startColor = beamData.Controller.BeamModifierData.Colour;
                _sparksObject.SetActive(true);
                _sparksObject.GetComponentInChildren<SpriteRenderer>().color = beamData.Controller.BeamModifierData.Colour;
            }
            else if (beamData == null && _sparksObject.activeInHierarchy)
            {
                _sparksObject.SetActive(false);
                _audioController.StopGrind();
            }
        }

        void FixedUpdate()
        {
            //if (ActiveCharacter)
            //{
                if (_wasRidingBeam && !Grounded)
                {
                    _currentMovementVector.x = 0;
                }


                if (_rigidbody.linearVelocityY < 0 &&
                     _rigidbody.linearVelocityX > -_fallingVelocityXThreshold && 
                     _rigidbody.linearVelocityX < _fallingVelocityXThreshold && 
                     !Grounded &&
                     _cachedAffectingBeam == NO_BEAM_CACHED)
                {
                    //Free-falling
                    if (_rigidbody.gravityScale != _fallingGravityScale)
                    {
                        _rigidbody.gravityScale = _fallingGravityScale;
                    }

                    if (_triggered)
                        _triggered = false;

                    _rigidbody.AddForce(_currentMovementVector * _movementSpeed * _fallingMovementSpeedDivider * _rigidbody.mass, ForceMode2D.Force);
                }
                else
                {
                    // if (_rigidbody.gravityScale != _gravityScale)
                    // {
                    //     _rigidbody.gravityScale = _gravityScale;
                    // }
                    
                    _rigidbody.AddForce(_currentMovementVector * _movementSpeed * _rigidbody.mass, ForceMode2D.Force);
                }

                if (_outsideForces != Vector2.zero)
                {
                    _rigidbody.linearVelocity += _outsideForces;
                }

                if (JumpRequested)
                {
                    if (Grounded)
                    {
                        _rigidbody.AddForce(Vector2.up * _jumpForce * _rigidbody.mass, ForceMode2D.Impulse);
                        _audioController.PlayJump();
                    }
                    JumpRequested = false;
                }

                int basicMovementAdjustment = 0;

                basicMovementAdjustment += _isRunning && !_wasRidingBeam ? 1 : 0;

                _currentVelocityCap =  _baseVelocityCap * ((_biggestOutsideForcesCount * _capMultiplierForOutsideForces) + basicMovementAdjustment);
                if (_rigidbody.linearVelocity != Vector2.zero)
                {
                    var startingVelocity = _rigidbody.linearVelocity;
                    float currentSquareMagnitude = startingVelocity.sqrMagnitude;

                    if (currentSquareMagnitude > _currentVelocityCap)
                    {
                        float scaleFactor = Mathf.Sqrt(_currentVelocityCap / currentSquareMagnitude);
                        var cappedVelocity = startingVelocity * scaleFactor;
                        var difference = startingVelocity - cappedVelocity;
                        _rigidbody.AddForce(-difference, ForceMode2D.Impulse);
                    }
                }
            //}

        }

        public void EnableCharacter() => ActiveCharacter = true;

        public void DisableCharacter() => ActiveCharacter = false;

        //Beam-related functions
        public LightBeamDataGroup GetCachedBeamData() => _cachedAffectingBeam == NO_BEAM_CACHED ? null : _listOfOutsideForces[_cachedAffectingBeam];

        public void RegisterIncomingBeamForce(LightBeamController sender, int beamPriority, Vector2 senderBeamDirection, float beamForce)
        {
            if (!_listOfOutsideForces.ContainsKey(sender.gameObject.GetHashCode()))
            {
                _listOfOutsideForces.Add(sender.gameObject.GetHashCode(), new LightBeamDataGroup
                {
                    Controller = sender,
                    Priority = beamPriority,
                    DirectionAndForce = senderBeamDirection * beamForce,
                    CanBeExited = sender.CanBeExited
                });
                _capMultiplierForOutsideForces += beamForce * _outsideForceInfluenceMultiplier;
                _triggerCollider.enabled = true;
            }
            // TODO: This method is called every tick that the beam detects the player. The beam priority is a value that increments the more controllers this single beam of light
            // has been through. The senderBeamDirection dictates the direction the beam is flowing. The beamForce value is a raw force to be applied in the given direction, the simplest
            // application of this being senderBeamDirection * beamForce, but I didn't want to just "implement that" in this way. I might change the beamForce parameter to a vec2
            // as we implement new beam modifiers and discover we have a need for that, but since im just following my nose for now, this is the best I got for ya. - Matt
        }

        internal void UnregisterIncomingBeamForce(LightBeamController sender, int beamPriority)
        {
            if (_listOfOutsideForces.ContainsKey(sender.gameObject.GetHashCode()))
            {
                _listOfOutsideForces.Remove(sender.gameObject.GetHashCode());
                if (_listOfOutsideForces.Count == 0)
                {
                    _cachedAffectingBeam = NO_BEAM_CACHED;
                    _triggerCollider.enabled = false;
                }
            }
            // TODO: This method is only called once by the sending beam controller to effectively flag the player is no longer under the control of that particular light beam controller.
            // This method exists to help you clean up any state, or help you track multiple controllers if your implementation requires it, and need a way to figure out which controllers to
            // stop caring about. - Matt

        }

        //Physics-related (Non-collision) functions

        public void ResetRotation()
        {
            if (_spriteRotator.rotation.z > 0)
            {
                _spriteRotator.Rotate(-_spriteRotator.localEulerAngles);
            }
            else if (_spriteRotator.rotation.z < 0)
            {
                _spriteRotator.Rotate(_spriteRotator.localEulerAngles);
            }
        }

        public void RotateCharacterToBeam(Vector3 localEulerAngles)
        {
            //Reset rotation first
            ResetRotation();

            _spriteRotator.Rotate(localEulerAngles);

            if (_spriteRotator.up.y < 0)
            {
                var rotEuler =_spriteRotator.rotation.eulerAngles;
                rotEuler.z = -rotEuler.z;
                _spriteRotator.rotation = Quaternion.Euler(rotEuler);
                _spriteRotator.Rotate(new Vector3(0, 0, 180));
            }
        }

        public void FlipCharacterSprite(bool normalDirection = true)
        {
            transform.GetPositionAndRotation(out Vector3 pos, out var rot);
            rot.y = normalDirection ? 0 : 180;
            transform.SetPositionAndRotation(pos, rot);
        }

        private Vector2 FlipVelocity(Vector2 velocity, Vector2 direction)
        {
            float directionalVelocity = Vector2.Dot(velocity, direction);
            Vector2 newVelocity = new Vector2(directionalVelocity, _rigidbody.linearVelocityY);
            return newVelocity;
        }

        public void SetGravityScale(float scale) => _rigidbody.gravityScale = scale;

        public void ResetGravityScale() => _rigidbody.gravityScale = _gravityScale;

        public void SetLinearDamping(float dampingValue) => _rigidbody.linearDamping = dampingValue;
        
        public void ResetLinearDamping() => _rigidbody.linearDamping = _gravityScale;

        //Collision-specific functions
        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("InteractablePhysicsObject"))
            {
                Grounded = true;
                _wasRidingBeam = false;
                if(_rigidbody.gravityScale != _gravityScale)
                {
                    _rigidbody.gravityScale = _gravityScale;
                    //Reset linear damping just in case
                    _rigidbody.linearDamping = _gravityScale;
                }
                if(gameObject.layer != LayerMask.NameToLayer("Player"))
                {
                    gameObject.layer = LayerMask.NameToLayer("Player");
                }

                _biggestOutsideForcesCount = 0;

                _currentVelocityCap = _baseVelocityCap;

                if (_spriteRotator.localEulerAngles.z != 0)
                {
                    _spriteRotator.localEulerAngles = Vector3.zero;
                }
                _audioController.PlayLandingClip(_rigidbody.linearVelocityY < -10.0f);
            }
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (!Grounded && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                Grounded = true;
                _wasRidingBeam = false;
                _rigidbody.gravityScale = _gravityScale;
            }
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                collision.gameObject.layer == LayerMask.NameToLayer("LightBeam") ||
                (collision.gameObject.layer == LayerMask.NameToLayer("InteractablePhysicsObject") && this.HeldObject != null))
            {
                Grounded = false;
            }
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.transform.gameObject.layer == LayerMask.NameToLayer("InteractableObject") && !WasRidingBeam)
            {
                return; 
            }

            var lightBeamController = collision.GetComponentInParent<LightBeamController>();
            if (lightBeamController != null && _cachedAffectingBeam != lightBeamController.gameObject.GetHashCode())
            {
                _triggered = true;
                ResetGravityScale();
                ResetLinearDamping();
                
                //Get flipped velocity
                Vector2 flippedVelocity = FlipVelocity(_rigidbody.linearVelocity, lightBeamController.BeamTransform.right);
                flippedVelocity.y *= _triggerVerticalLaunchDamping;

                // Get the distance between feetsies and origin
                var distance = Vector2.Distance(transform.position, _feetTargetTransform.position);

                //Get the new position from the LBC's snap point
                var newPosition = lightBeamController.PlayerSnapPoint;
                newPosition.y += distance;

                //identify rotation
                Transform beamParentTransform = lightBeamController.transform;
                Vector3 angles = Vector3.zero;

                if (lightBeamController.BeamTransform.localEulerAngles.z != 0)
                {
                    angles = lightBeamController.BeamTransform.localEulerAngles.z > 180 ? lightBeamController.BeamTransform.eulerAngles : lightBeamController.BeamTransform.localEulerAngles;

                    if (angles.z > 90)
                    {
                        angles.z -= 180;
                    }
                }
                else if (beamParentTransform != null && beamParentTransform.eulerAngles.z != 0)
                {
                    angles = beamParentTransform.eulerAngles;
                }

                FlipCharacterSprite(lightBeamController.BeamTransform.right.x >= 0);
                RotateCharacterToBeam(angles);
                transform.position = newPosition;
                _rigidbody.linearVelocity = flippedVelocity;

                SetGravityScale(BeamGravityScale);
                SetLinearDamping(BeamVerticalLaunchDamping);
            }

        }

        private void HandleInteractions()
        {
            if (HeldObject != null)
            {
                //HeldObject.GetComponent<Collider2D>().enabled = true;
                foreach (var collider in HeldObject.GetComponentsInChildren<Collider2D>())
                {
                    collider.enabled = true;
                }
                HeldObject.transform.parent = null;
                HeldObject.GetComponent<Rigidbody2D>().simulated = true;
                HeldObject = null;
                return;
            }

           //Physics2D.RaycastAll(_spriteRotator.position, _spriteRotator.right, distance: _interactionRayLength);
            RaycastHit2D[] hitCount = Physics2D.CircleCastAll(_spriteRotator.position, 1, _spriteRotator.right, _interactionRayLength);

            InteractableBehaviour highestPriorityTarget = null;

            foreach (var hit in hitCount)
            {

                Transform testTarget = hit.transform;

                while (testTarget.parent != null)
                {
                    testTarget = testTarget.parent;
                }
                
                InteractableBehaviour interactable = testTarget.transform.gameObject.GetComponentInChildren<InteractableBehaviour>();

                if (interactable != null && (highestPriorityTarget == null || highestPriorityTarget.Priority < interactable.Priority))
                {
                    highestPriorityTarget = interactable;
                }
            }

            if (highestPriorityTarget != null)
            {
                highestPriorityTarget.Interact(this);
            }
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(_spriteRotator.position, _spriteRotator.right * _interactionRayLength);
            Gizmos.DrawSphere(_spriteRotator.right * _interactionRayLength, 0.5f);
        }
    
        //Input-related functions
        private void UpdatePlayerSpecificSettings(Preferences settings)
        {
            _toggleSprintEnabled = settings.ToggleSprint;
        }
    
        public void OnDeviceChanged(InputDevice device, InputDeviceChange change)
        {
            if(change == InputDeviceChange.Added ||
                change == InputDeviceChange.ConfigurationChanged ||
                change == InputDeviceChange.UsageChanged ||
                change == InputDeviceChange.SoftReset)
            {
                if(device is Keyboard)
                {   
                    _keyboardIsDevice = true;
                }
                else
                {
                    _keyboardIsDevice = false;
                }
            }
        }
    
        private void HandleMovement(InputAction.CallbackContext context)
        {
            if (!ActiveCharacter)
            {
                _cachedMovementVector = Vector3.zero;
                return;
            }

            _cachedMovementVector = context.ReadValue<Vector2>();
            if(context.control?.device is Keyboard)
            {
                
                _keyboardIsDevice = true;
            }
            else
            {
                _keyboardIsDevice = false;
            }
            //Disable Up and Down movement for the player - they only move left & right
            //and have separate controls for jumping
            _cachedMovementVector.y = 0f;
            
        }

        private void HandleMovementCanceled(InputAction.CallbackContext _)
        {
            _resetMovement = true;
        }
        
        void OnJump()
        {
            if (!ActiveCharacter)
                return;

            if (!JumpRequested && Grounded)
            {
                JumpRequested = true;
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

        void OnHopDown(InputValue _)
        {
            if(!ActiveCharacter)
                return;
            
            _hopDownTriggered = true;
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
                //Change active status
                ActiveCharacter = false;
                switchCharacters = false;
                _familiarControllerReference.ActiveCharacter = true;
                
                //Shutdown velocity for this character
                //_rigidbody.linearVelocity = Vector2.zero;
                
                //Change Input Action Maps
                var inputComponent = GetComponentInParent<InputController>();
                if(inputComponent != null)
                {
                    inputComponent.SwapCharacterMaps(false);
                }

                //Swap the Camera
                SwitchCamerasEvent.Invoke(2);
            }
        }

        //Animation-related functions
        internal void UpdateAnims()
        {
            _characterAnimator.SetFloat("MovementX", _currentMovementVector.x);
            _characterAnimator.SetFloat("MovementY", _rigidbody.linearVelocityY);
            _characterAnimator.SetBool("DisableFall", DisableFall);
            _characterAnimator.SetBool("CollidingWithBeam", _cachedAffectingBeam != NO_BEAM_CACHED);

            if (_currentMovementVector.x != 0 && _cachedAffectingBeam == NO_BEAM_CACHED)
            {
                FlipCharacterSprite(_currentMovementVector.x > 0);
            }
        }

    }
}