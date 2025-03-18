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

        [Header("Configuration")]
        [SerializeField]
        private float _lightBeamLength;

        [SerializeField]
        private LightBeamMode _mode;

        [SerializeField]
        private float _beamPierceDistance;

        private Quaternion _cachedStartRotation;
        private LightBeamController _targetHit;
        private Vector3? _emissionPoint;
        private RaycastHit2D[] _beamRaycastData = new RaycastHit2D[3];
        private ContactFilter2D _beamRaycastFilter;
        private int _beamPriority;
        private PlayerController _player;

        public LightBeamModifier BeamModifierData => _beamModifierData;
        public int BeamPriority => _beamPriority;

        public bool ShouldIgnoreWalls { get; set; }

        private Vector3? RegisterPotentialBeamHit()
        {
            var hitCount = Physics2D.Raycast(_emissionPoint ?? transform.position, transform.right, _beamRaycastFilter, _beamRaycastData, _lightBeamLength);

            if (hitCount == 0)
            {
                if (_targetHit != null)
                {
                    _targetHit.UnregisterHit();
                    _targetHit = null;
                }

                if (_player != null)
                {
                    _beamModifierData.ClearBeamEffect(this, _beamPriority, _player);
                    _player = null;
                }

                return null;
            }

            RaycastHit2D? hitInfo = null;

            bool appliedForceToPlayerThisFrame = false;
            
            foreach (var hit in _beamRaycastData)
            {
                if (hit.transform != null) // this should also return simple things like walls
                {
                    _player = hit.transform.GetComponentInChildren<PlayerController>();
                    
                    if (_player != null && !appliedForceToPlayerThisFrame)
                    {
                        appliedForceToPlayerThisFrame = true;
                        _beamModifierData.ApplyBeamEffect(this, _beamPriority, _player, transform.right);
                        continue;
                    }

                    var beamControllerTest = hit.transform.GetComponentInChildren<LightBeamController>();

                    if (beamControllerTest == this || (beamControllerTest == null && ShouldIgnoreWalls))
                    {
                        continue;
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
                if (_targetHit != null)
                {
                    _targetHit.UnregisterHit();
                    _targetHit = null;
                }
                
                return hitInfoValue.point;
            }

            if (beamController == null)
            {
                if (_targetHit != null)
                {
                    _targetHit.UnregisterHit();
                    _targetHit = null;
                }

                return hitInfoValue.point; 
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

        private void ProduceBeam()
        {
            var potentialHitPoint = RegisterPotentialBeamHit();
            var translatedPosition = transform.position;
            translatedPosition += transform.right * _lightBeamLength;
            _renderer.SetPositions(new[] { _emissionPoint ?? transform.position, potentialHitPoint ?? translatedPosition});
            _renderer.startColor = _beamModifierData.Colour; 
            _renderer.endColor = _beamModifierData.Colour; 
        }

        protected void Start()
        {
            _beamRaycastFilter = new ContactFilter2D().NoFilter();
            _cachedStartRotation = transform.rotation;

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
                    transform.rotation = Quaternion.Euler(0, 0, reflectedAngle);
                    break;
                case LightBeamMode.Transform:
                    senderDirection = -senderDirection;
                    _emissionPoint += senderDirection * _beamPierceDistance;
                    var lookAtAngle = Mathf.Atan2(senderDirection.y, senderDirection.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, 0, lookAtAngle);
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

            transform.rotation = _cachedStartRotation;
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
                _beamModifierData.ApplyBeamEffect(this, BeamPriority, _player, transform.right);
            }
        }
    }
}