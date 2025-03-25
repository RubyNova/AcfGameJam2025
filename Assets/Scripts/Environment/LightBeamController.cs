using System.Linq;
using Controllers;
using UnityEngine;

namespace Environment
{
    public class LightBeamController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private LineRenderer _renderer;

        [SerializeField]
        private Transform _axisHelper;

        [SerializeField]
        private LightBeamModifier _beamModifierData;

        [SerializeField]
        private Transform _targetTransform;
        
        [SerializeField]
        private BoxCollider2D _boxCollider;

        [Header("Configuration")]
        [SerializeField]
        private float _lightBeamLength;

        [SerializeField]
        private LightBeamMode _mode;

        [SerializeField]
        private float _beamPierceDistance;

        [SerializeField]
        private float _beamExitVelocityMultiplier;

        [SerializeField]
        private GameObject[] _objectsToIgnoreDuringHitChecks;

        private Quaternion _cachedStartRotation;
        private LightBeamController _targetHit;
        private Vector3? _emissionPoint;
        private RaycastHit2D[] _beamRaycastData = new RaycastHit2D[5];
        private RaycastHit2D[] _beamLowerRaycastData = new RaycastHit2D[5];
        private ContactFilter2D _beamRaycastFilter;
        private LightBeamController _currentSender;
        private int _beamPriority;
        private PlayerController _player; 
        private PlayerController _playerControllerForBoundsChecks;

        public LightBeamModifier BeamModifierData => _beamModifierData;
        
        public int BeamPriority => _beamPriority;

        public bool ShouldIgnoreWalls { get; set; }

        private Vector3? RegisterPotentialBeamHit()
        {
            var hitCount = Physics2D.Raycast(_emissionPoint ?? _targetTransform.position, _targetTransform.right, _beamRaycastFilter, _beamRaycastData, _lightBeamLength);

            if (hitCount == 0)
            {
                if (_targetHit != null)
                {
                    _targetHit.UnregisterHit();
                    _targetHit = null;
                }

                if (_player != null)
                {
                    //_beamModifierData.ClearBeamEffect(this, _beamPriority, _player);
                    _player = null;
                }

                return null;
            }

            RaycastHit2D? hitInfo = null;

            bool appliedForceToPlayerThisFrame = false;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = _beamRaycastData[i];
                if (hit.transform != null) // this should also return simple things like walls
                {
                    _player = hit.transform.GetComponentInChildren<PlayerController>();
                    
                    if(_player != null && !appliedForceToPlayerThisFrame)
                    {    
                        appliedForceToPlayerThisFrame = true;
                        //_beamModifierData.ApplyBeamEffect(this, _beamPriority, _player, _targetTransform.right);
                        continue;
                    }
                    
                    var beamControllerTest = hit.transform.GetComponentInChildren<LightBeamController>();
                    var beamControllerParentTest = hit.transform.GetComponentInParent<LightBeamController>();

                    if (beamControllerTest == this || beamControllerTest == _currentSender || hit.transform.CompareTag("IgnoredByBeam") || (beamControllerTest == null && ShouldIgnoreWalls) || _objectsToIgnoreDuringHitChecks.Any(o => o == hit.transform.gameObject))
                    {
                        if (beamControllerParentTest == this || beamControllerParentTest == _currentSender || (beamControllerParentTest == null && ShouldIgnoreWalls))
                        {
                            continue;
                        }
                        else
                        {
                            hitInfo = hit;
                            break;
                        }
                    }
                    else
                    {
                        hitInfo = hit;
                        break;
                    }
                }
            }

            if (!hitInfo.HasValue)
            {
                if (_targetHit != null)
                {
                    _targetHit.UnregisterHit();
                    _targetHit = null;
                }

                return null;
            }

            var hitInfoValue = hitInfo.Value;

            var beamController = hitInfoValue.transform.GetComponentInChildren<LightBeamController>();

            if (beamController == null)
            {
                beamController = hitInfoValue.transform.GetComponentInParent<LightBeamController>();
                if(beamController == null)
                {
                    if (_targetHit != null)
                    {
                        _targetHit.UnregisterHit();
                        _targetHit = null;
                    }
                    
                    return hitInfoValue.point;
                }
            }

            if (_targetHit == beamController)
            {
                beamController.RegisterHit(this, hitInfoValue.point);
                return hitInfoValue.point;
            }

            if (_targetHit != null)
            {
                _targetHit.UnregisterHit();
            }

            beamController.RegisterHit(this, hitInfoValue.point);
            _targetHit = beamController;
            
            return hitInfoValue.point;
        }

        private void CheckForPlayerBelow()
        {
            if(_playerControllerForBoundsChecks != null)
            {        
                if(_playerControllerForBoundsChecks.MinColliderPoint.y < _boxCollider.bounds.min.y)
                {
                    _boxCollider.enabled = false;
                }
                else
                {
                    _boxCollider.enabled = true;
                }
            }
        }

        private void ProduceBeam()
        {
            var potentialHitPoint = RegisterPotentialBeamHit();
            var translatedPosition = _targetTransform.position;
            translatedPosition += _targetTransform.right * _lightBeamLength;
            var positions = new[] { _emissionPoint ?? _targetTransform.position, 
                potentialHitPoint ?? translatedPosition
            };
            _renderer.SetPositions(positions);
            _renderer.startColor = _beamModifierData.Colour; 
            _renderer.endColor = _beamModifierData.Colour;

            _boxCollider.size = new Vector2(Vector2.Distance(positions[0], positions[1]), _renderer.startWidth);

            var centrePosition = _targetTransform.InverseTransformPoint((positions[0] + positions[1]) * 0.5f);

            _boxCollider.offset = new Vector2(centrePosition.x, centrePosition.y);

            CheckForPlayerBelow();
        }

        protected void Start()
        {
            _beamRaycastFilter = new ContactFilter2D().NoFilter();
            _cachedStartRotation = _targetTransform.rotation;
            _playerControllerForBoundsChecks = FindFirstObjectByType<PlayerController>();

            if (_mode == LightBeamMode.Source)
            {
                _beamModifierData.Initialise(this);
            }
            else
            {
                _renderer.enabled = false;   
            }
        }

        protected void Update()
        {
            if (_mode != LightBeamMode.Source && !_renderer.enabled)
            {
                return;
            }

            ProduceBeam();
        }

        public void RegisterHit(LightBeamController sender, Vector3 hitPoint)
        {
            if (sender == this)
            {
                throw new System.Exception("Sender cannot be self.");
            }

            if (_mode == LightBeamMode.Source)
            {
                return;
            }

            _currentSender = sender;

            _beamPriority = sender.BeamPriority + 1;

            _emissionPoint = hitPoint;
            _renderer.enabled = true;
            var senderDirection = (sender.transform.position - hitPoint).normalized;

            switch (_mode)
            {
                case LightBeamMode.Bounce:

                    if (_beamModifierData != null)
                    {
                        _beamModifierData.Shutdown(this);
                    }

                    _beamModifierData = sender.BeamModifierData;
                    _beamModifierData.Initialise(this);
                    var reflectedDirection = Vector2.Reflect(senderDirection, _axisHelper.up);
                    var reflectedAngle = Mathf.Atan2(reflectedDirection.y, reflectedDirection.x) * Mathf.Rad2Deg;
                    _targetTransform.rotation = Quaternion.Euler(0, 0, reflectedAngle);
                    break;
                case LightBeamMode.Transform:
                    senderDirection = -senderDirection;
                    _emissionPoint += senderDirection * _beamPierceDistance;
                    var lookAtAngle = Mathf.Atan2(senderDirection.y, senderDirection.x) * Mathf.Rad2Deg;
                    _targetTransform.rotation = Quaternion.Euler(0, 0, lookAtAngle);
                    break;
                default:
                    break;
            }
        }

        public void UnregisterHit()
        {
            if (_mode == LightBeamMode.Source)
            {
                return;
            }

            if (_targetHit != null)
            {
                _targetHit.UnregisterHit();
                _targetHit = null;
            }

            if (_mode == LightBeamMode.Bounce)
            {
                _beamModifierData = null;   
            }

            _currentSender = null;
            _targetTransform.rotation = _cachedStartRotation;
            _renderer.enabled = false;
            _emissionPoint = null;
        }

        public void ChangeBeamModifier(LightBeamModifier newModifier)
        {
            if (_player != null)
            {
                _beamModifierData.ClearBeamEffect(this, _beamPriority, _player);
            }

            if (_beamModifierData != null)
            {
                _beamModifierData.Shutdown(this);
            }

            _beamModifierData = newModifier;
            _beamModifierData.Initialise(this);

            if (_player != null)
            {
                _beamModifierData.ApplyBeamEffect(this, BeamPriority, _player, _targetTransform.right);
            }
        }

        // public void OnCollisionEnter2D(Collision2D collision)
        // {
        //     if(collision.gameObject.CompareTag("Player"))
        //     {
        //         print("player col");
        //         var playerComponent = collision.gameObject.GetComponent<PlayerController>();
        //         if(!_isColliding)
        //         {
        //             _isColliding = true;
        //             if(transform.rotation.z != 0)
        //             {
        //                 //playerComponent._rigidbody.freezeRotation = false;
        //                 playerComponent.RotateCharacter(transform.localEulerAngles);
        //                 // if(collision.contactCount > 0)
        //                 // {
        //                 //     playerComponent._rigidbody.MovePosition(collision.GetContact(0).point);
        //                 // }
                        
        //             }

        //             playerComponent._rigidbody.MovePosition(collision.contacts[0].point);

        //             _beamModifierData.ApplyBeamEffect(this, 
        //                 BeamPriority, 
        //                 playerComponent, 
        //                 _targetTransform.right);
        //         }

        //         playerComponent.Grounded = true;
        //     }
        // }

        // public void OnCollisionExit2D(Collision2D collision)
        // {
        //     if(collision.gameObject.CompareTag("Player"))
        //     {
        //         var playerComponent = collision.gameObject.GetComponent<PlayerController>();
        //         if(playerComponent.JumpRequested)
        //         {
        //             if(_isColliding)
        //             {
        //                 _isColliding = false;
        //                 _beamModifierData.ClearBeamEffect(this, 
        //                     BeamPriority, 
        //                     playerComponent
        //                     );
        //             }

        //             if(playerComponent.Grounded)
        //             {
        //                 playerComponent.Grounded = false;
        //             //     //playerComponent._rigidbody.freezeRotation = true;
                      

        //             //    var vel = _beamModifierData.BeamForce * _targetTransform.right * _beamExitVelocityMultiplier;
        //             //     if(playerComponent.BeamCollisionCount > 1)
        //             //     {
        //             //         playerComponent.AddLinearVelocity(gameObject.GetHashCode(), 
        //             //             vel);
        //             //     }
        //             //     else
        //             //     {
        //             //         playerComponent.AddLinearVelocityRaw(vel);
        //             //     }
        //             }

        //             playerComponent.RotateCharacter(-transform.localEulerAngles);
        //         }
        //         else
        //         {
        //            if(playerComponent.transform.position.x > _boxCollider.bounds.max.x ||
        //             playerComponent.transform.position.x < _boxCollider.bounds.min.x)
        //             {
        //                 if(_isColliding)
        //             {
        //                 _isColliding = false;
        //                 _beamModifierData.ClearBeamEffect(this, 
        //                     BeamPriority, 
        //                     playerComponent
        //                     );
        //             }

        //             if(playerComponent.Grounded)
        //             {
        //                 playerComponent.Grounded = false;
        //             //     //playerComponent._rigidbody.freezeRotation = true;
                      

        //             //    var vel = _beamModifierData.BeamForce * _targetTransform.right * _beamExitVelocityMultiplier;
        //             //     if(playerComponent.BeamCollisionCount > 1)
        //             //     {
        //             //         playerComponent.AddLinearVelocity(gameObject.GetHashCode(), 
        //             //             vel);
        //             //     }
        //             //     else
        //             //     {
        //             //         playerComponent.AddLinearVelocityRaw(vel);
        //             //     }
        //             }

        //             playerComponent.RotateCharacter(-transform.localEulerAngles);
        //             if(playerComponent.BeamCollisionCount > 1)
        //             {
        //                 playerComponent.AddLinearVelocity(gameObject.GetHashCode(), 
        //                     _beamModifierData.BeamForce * _targetTransform.right );
        //             }
        //             else
        //             {
        //                 playerComponent.AddLinearVelocityRaw(_beamModifierData.BeamForce * _targetTransform.right );
        //             }
        //             } 
        //         }
        //     }
            
        // }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(_emissionPoint ?? _targetTransform.position, _targetTransform.right);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_emissionPoint ?? _targetTransform.position, -_targetTransform.up);
        }
    }
}