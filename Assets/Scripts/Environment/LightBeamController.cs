using System.Collections.Generic;
using System.Linq;
using Controllers;
using Environment.Interactables;
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
        public BoxCollider2D BoxCollider;

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
        private float _snapPointOffset = 1;

        [SerializeField]
        private GameObject[] _objectsToIgnoreDuringHitChecks;

        [SerializeField]
        private int _beamPriority;

        [SerializeField]
        private bool _forceAlwaysOn = false;

        [Header("Read-only values")]
        [SerializeField]
        private Vector2 _intersectedPosition = Vector2.zero;

        [SerializeField]
        private Vector2 _perceivedMinColliderpoint = Vector2.zero;


        private Quaternion _cachedStartRotation;
        private LightBeamController _targetHit;
        private Vector3? _emissionPoint;
        private bool _isAlreadyUnregistering = false;
        private RaycastHit2D[] _beamRaycastData = new RaycastHit2D[10];
        private ContactFilter2D _beamRaycastFilter;
        private LightBeamController _currentSender;

        private PlayerController _playerControllerForBoundsChecks;
        private Vector2[] _colliderPoints = new Vector2[4];
        private Vector2 _playerSnapPoint;
        private HashSet<GenericBeamForceReactor> _trackedPhyscsInteractables = new();
        private bool _isAlreadyRegistering;

        public LightBeamModifier BeamModifierData => _beamModifierData;

        public int BeamPriority => _beamPriority;

        public bool ShouldIgnoreWalls { get; set; }

        public PlayerController CurrentPlayer { get; set; }

        public Vector2 PlayerSnapPoint => _playerSnapPoint;

        public Transform BeamTransform => _targetTransform;

        public float LightBeamLength { get => _lightBeamLength; set => _lightBeamLength = value; }

        public Vector3 EmissionPoint => _emissionPoint ?? _targetTransform.position;

        public Vector3 UpwardsNormal => _targetTransform.up;

        private Vector3? RegisterPotentialBeamHit()
        {
            var hitCount = Physics2D.Raycast(EmissionPoint, _targetTransform.right, _beamRaycastFilter, _beamRaycastData, LightBeamLength);

            if (hitCount == 0)
            {
                if (_targetHit != null)
                {
                    _targetHit.UnregisterHit();
                    _targetHit = null;
                }

                foreach (var reactorToUntrack in _trackedPhyscsInteractables)
                {
                    _beamModifierData.ClearBeamEffectOnObject(this, BeamPriority, reactorToUntrack);
                }

                _trackedPhyscsInteractables.Clear();

                return null;
            }

            RaycastHit2D? hitInfo = null;

            List<GenericBeamForceReactor> reactorsFoundThisFrame = new();

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = _beamRaycastData[i];
                if (hit.transform != null) // this should also return simple things like walls
                {
                    var beamControllerTest = hit.transform.GetComponentInChildren<LightBeamController>();
                    var beamControllerParentTest = hit.transform.GetComponentInParent<LightBeamController>();

                    if (beamControllerTest == null && beamControllerParentTest == null )
                    {
                        var beamCollisionHandlerTest = hit.transform.GetComponentInChildren<LightBeamCollisionHandler>();

                        if (beamCollisionHandlerTest == null)
                        {
                            beamCollisionHandlerTest = hit.transform.GetComponentInParent<LightBeamCollisionHandler>();   
                        }

                        if (beamCollisionHandlerTest != null)
                        {
                            beamControllerTest = beamCollisionHandlerTest.ParentController;
                        }
                    }

                    bool isSelf = beamControllerTest == this || beamControllerParentTest == this;

                    bool isSendingController = (beamControllerTest != null && beamControllerTest == _currentSender) || (beamControllerParentTest != null && beamControllerParentTest == _currentSender);

                    bool isAWallAndShouldBeIgnored = beamControllerTest == null && beamControllerParentTest == null && ShouldIgnoreWalls && hit.transform.gameObject.layer == LayerMask.NameToLayer("Ghosted");

                    bool anyIgnoredObjects = _objectsToIgnoreDuringHitChecks.Any(x => x.transform.gameObject == hit.transform.gameObject);

                    bool isPlayerOrTaggedForIgnore = hit.transform.CompareTag("IgnoredByBeam") || hit.transform.CompareTag("Player");

                    bool hasGenericForceBeamReactor = hit.transform.TryGetComponent<GenericBeamForceReactor>(out var reactor);

                    bool shouldFilterOut = isSelf
                    || isSendingController
                    || isAWallAndShouldBeIgnored
                    || anyIgnoredObjects
                    || isPlayerOrTaggedForIgnore
                    || hasGenericForceBeamReactor;

                    if (shouldFilterOut)
                    {
                        if (hasGenericForceBeamReactor)
                        {
                            reactorsFoundThisFrame.Add(reactor);
                            _trackedPhyscsInteractables.Add(reactor);
                            _beamModifierData.ApplyBeamEffectToObject(this, _beamPriority, reactor, _targetTransform.right); // objects on the generic object physics layer should still be detected and interacted with.
                        }

                        continue;
                    }
                    else
                    {
                        hitInfo = hit;
                        break;
                    }
                }
            }

            var difference = _trackedPhyscsInteractables.Except(reactorsFoundThisFrame).ToList();

            foreach (var reactorToUntrack in difference)
            {
                _beamModifierData.ClearBeamEffectOnObject(this, BeamPriority, reactorToUntrack);
                _trackedPhyscsInteractables.Remove(reactorToUntrack);
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
                if (beamController == null)
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
                beamController.RegisterHit(this, hitInfoValue.point, hitInfoValue.normal);
                return hitInfoValue.point;
            }

            if (_targetHit != null)
            {
                _targetHit.UnregisterHit();
            }

            beamController.RegisterHit(this, hitInfoValue.point, hitInfoValue.normal);
            _targetHit = beamController;

            return hitInfoValue.point;
        }

        private void CheckForPlayerBelow()
        {
            var targetTransform = BoxCollider.transform;
            var startPoint = targetTransform.InverseTransformPoint(_renderer.GetPosition(0));
            var endPoint = targetTransform.InverseTransformPoint(_renderer.GetPosition(1));
            var halfHeight = _renderer.startWidth * 0.5f;
            _colliderPoints[0] = targetTransform.TransformPoint(new Vector2(startPoint.x, startPoint.y + halfHeight));
            _colliderPoints[1] = targetTransform.TransformPoint(new Vector2(startPoint.x, startPoint.y - halfHeight));
            _colliderPoints[2] = targetTransform.TransformPoint(new Vector2(endPoint.x, endPoint.y + halfHeight));
            _colliderPoints[3] = targetTransform.TransformPoint(new Vector2(endPoint.x, endPoint.y - halfHeight));

            Vector2? firstLowestPoint = null;
            Vector2? secondLowestPoint = null;

            foreach (var point in _colliderPoints)
            {
                if (!firstLowestPoint.HasValue)
                {
                    firstLowestPoint = point;
                    continue;
                }

                if (point.y < firstLowestPoint.Value.y)
                {
                    secondLowestPoint = firstLowestPoint;
                    firstLowestPoint = point;
                }
                else if (!secondLowestPoint.HasValue || point.y < secondLowestPoint.Value.y)
                {
                    secondLowestPoint = point;
                }
            }

            var yValue = _playerControllerForBoundsChecks.MinColliderPoint.y;

            if (yValue < secondLowestPoint.Value.y && !_forceAlwaysOn)
            {
                BoxCollider.enabled = false;
            }
            else
            {
                Vector2? highestPoint = null;

                foreach (var point in _colliderPoints)
                {   
                    if (!highestPoint.HasValue || (highestPoint.Value.y < point.y && point != secondLowestPoint.Value))
                    {
                        highestPoint = point;
                    }
                }

                Vector2 intersectedPosition = new();

                if (Mathf.Approximately(secondLowestPoint.Value.x, highestPoint.Value.x))
                {
                    intersectedPosition = new(secondLowestPoint.Value.x, yValue);
                }
                else
                {
                    float slope = (highestPoint.Value.y - secondLowestPoint.Value.y) / (highestPoint.Value.x - secondLowestPoint.Value.x);
                    float yIntercept = secondLowestPoint.Value.y - slope * secondLowestPoint.Value.x;
                    float finalYPoint = slope * _playerControllerForBoundsChecks.MinColliderPoint.x + yIntercept;

                    intersectedPosition = new(_playerControllerForBoundsChecks.MinColliderPoint.x, finalYPoint);
                }

                _intersectedPosition = intersectedPosition;
                _perceivedMinColliderpoint = _playerControllerForBoundsChecks.MinColliderPoint;

                if (intersectedPosition.y > _playerControllerForBoundsChecks.MinColliderPoint.y && !Mathf.Approximately(intersectedPosition.y, _playerControllerForBoundsChecks.MinColliderPoint.y))
                {
                   BoxCollider.enabled = false; 
                }
                else
                {
                    BoxCollider.enabled = true;
                }

            }
        }

        private void ProduceBeam()
        {
            var potentialHitPoint = RegisterPotentialBeamHit();
            var translatedPosition = EmissionPoint;
            translatedPosition += _targetTransform.right * LightBeamLength;
            var positions = new[] { EmissionPoint,
                potentialHitPoint ?? translatedPosition
            };
            _renderer.SetPositions(positions);

            if (_beamModifierData == null)
            {
                _beamModifierData = GetComponentInChildren<LightBeamModifier>();
            }

            _renderer.startColor = _beamModifierData.Colour;
            _renderer.endColor = _beamModifierData.Colour;

            BoxCollider.size = new Vector2(Vector2.Distance(positions[0], positions[1]), _renderer.startWidth);

            var centrePosition = _targetTransform.InverseTransformPoint((positions[0] + positions[1]) * 0.5f);

            BoxCollider.offset = new Vector2(centrePosition.x, centrePosition.y);

            CheckForPlayerBelow();
            CalculateTopsideSnapPoint();
        }

        private void CalculateTopsideSnapPoint()
        {
            Vector2? firstOriginPoint = null;
            Vector2? secondOriginPoint = null;

            var origin = EmissionPoint;

            foreach (var point in _colliderPoints)
            {
                if (!firstOriginPoint.HasValue)
                {
                    firstOriginPoint = point;
                    continue;
                }

                if (Vector2.Distance(point, origin) <= Vector2.Distance(firstOriginPoint.Value, origin))
                {
                    secondOriginPoint = firstOriginPoint;
                    firstOriginPoint = point;
                }
                else if (!secondOriginPoint.HasValue || Vector2.Distance(point, origin) < Vector2.Distance(secondOriginPoint.Value, origin))
                {
                    secondOriginPoint = point;
                }
            }

            Vector2 startPoint = firstOriginPoint.Value.y > secondOriginPoint.Value.y
                ? firstOriginPoint.Value
                : secondOriginPoint.Value;

            _playerSnapPoint = startPoint + (Vector2)(_targetTransform.right * _snapPointOffset);
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
                BoxCollider.enabled = false;
                return;
            }

            BoxCollider.enabled = true;

            ProduceBeam();
        }

        public void RegisterHit(LightBeamController sender, Vector3 hitPoint, Vector2 normal)
        {
            if (sender == this)
            {
                throw new System.Exception("Sender cannot be self.");
            }

            if (_isAlreadyRegistering || sender == null || sender.BeamModifierData == null || _mode == LightBeamMode.Source || (_currentSender != null && _currentSender.BeamPriority > sender.BeamPriority))
            {
                return;
            }

            _isAlreadyRegistering = true;
            _currentSender = sender;

            _beamPriority = sender.BeamPriority + 1;

            _emissionPoint = hitPoint;
            _renderer.enabled = true;
            var senderDirection = (sender.EmissionPoint - hitPoint).normalized;

            switch (_mode)
            {
                case LightBeamMode.Bounce:

                    if (_beamModifierData != null)
                    {
                        _beamModifierData.Shutdown(this);
                    }

                    _beamModifierData = sender.BeamModifierData;
                    _beamModifierData.Initialise(this);
                    var reflectedDirection = Vector2.Reflect(senderDirection, new Vector2(normal.y, -normal.x));
                    var reflectedAngle = Mathf.Atan2(reflectedDirection.y, reflectedDirection.x) * Mathf.Rad2Deg;
                    _targetTransform.rotation = Quaternion.Euler(0, 0, reflectedAngle);
                    break;
                case LightBeamMode.Transform:
                    var inverseSenderDirection = -senderDirection;
                    _emissionPoint += inverseSenderDirection * _beamPierceDistance;
                    var distanceCheck = Vector2.Distance(EmissionPoint, sender.EmissionPoint);

                    if (distanceCheck < _beamPierceDistance)
                    {
                        _currentSender = null;
                        BoxCollider.enabled = false;
                        _renderer.enabled = false;
                        _isAlreadyRegistering = false;

                        if (_targetHit != null)
                        {
                            _targetHit.UnregisterHit();
                        }

                        _targetHit = null;
                        return;
                    }

                    LightBeamLength = sender.LightBeamLength - distanceCheck;
                    var lookAtAngle = Mathf.Atan2(inverseSenderDirection.y, inverseSenderDirection.x) * Mathf.Rad2Deg;
                    _targetTransform.rotation = Quaternion.Euler(0, 0, lookAtAngle);
                    _beamModifierData.Initialise(this);

                    break;
                default:
                    break;
            }

            _isAlreadyRegistering = false;
        }

        public void UnregisterHit()
        {
            if (_isAlreadyUnregistering || _mode == LightBeamMode.Source)
            {
                return;
            }

            _isAlreadyUnregistering = true;

            if (_targetHit != null)
            {
                _targetHit.UnregisterHit();
                _targetHit = null;
            }
            
            foreach (var trackedReactor in _trackedPhyscsInteractables)
            {
                _beamModifierData.ClearBeamEffectOnObject(this, BeamPriority, trackedReactor);   
            }

            if (_mode != LightBeamMode.Source)
            {
                if (CurrentPlayer != null)
                {
                    _beamModifierData.ClearBeamEffectOnPlayer(this, BeamPriority, CurrentPlayer);
                }

                _beamModifierData.Shutdown(this);

                if (_mode == LightBeamMode.Bounce)
                {
                    _beamModifierData = null;
                }
            }
            


            _currentSender = null;
            _targetTransform.rotation = _cachedStartRotation;
            _renderer.enabled = false;
            _emissionPoint = null;
            _isAlreadyUnregistering = false;
        }

        public void ChangeBeamModifier(LightBeamModifier newModifier)
        {
            if (CurrentPlayer != null)
            {
                _beamModifierData.ClearBeamEffectOnPlayer(this, _beamPriority, CurrentPlayer);
            }

            foreach (var trackedReactor in _trackedPhyscsInteractables)
            {
                _beamModifierData.ClearBeamEffectOnObject(this, BeamPriority, trackedReactor);   
            }
            
            _beamModifierData.Shutdown(this);

            _beamModifierData = newModifier;
            _beamModifierData.Initialise(this);

            if (CurrentPlayer != null)
            {
                _beamModifierData.ApplyBeamEffectToPlayer(this, BeamPriority, CurrentPlayer, _targetTransform.right);
            }

            foreach (var trackedReactor in _trackedPhyscsInteractables)
            {
                _beamModifierData.ApplyBeamEffectToObject(this, BeamPriority, trackedReactor, _targetTransform.right);   
            }
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(EmissionPoint, _targetTransform.right);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(EmissionPoint, -_targetTransform.up);
            Gizmos.DrawSphere(_playerSnapPoint, 0.5f);
        }

        public void OnDrawGizmosSelected()
        {
            var directionPosition = _targetTransform.right * LightBeamLength;
            var emission = EmissionPoint;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(emission, 0.25f);
            Gizmos.color = Color.grey;
            Gizmos.DrawRay(emission, directionPosition);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere((emission + directionPosition), 0.25f);
        }
    }
}