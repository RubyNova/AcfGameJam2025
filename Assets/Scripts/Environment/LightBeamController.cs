using System;
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

        private Quaternion _cachedStartRotation;
        private LightBeamController _targetHit;
        private Vector3? _contactPoint;
        private Vector3 _senderDirection;
        private RaycastHit2D[] _beamRaycastData = new RaycastHit2D[3];
        private ContactFilter2D _beamRaycastFilter;
        private int _beamPriority;
        private PlayerController _player;

        public LightBeamModifier BeamModifierData => _beamModifierData;

        private Vector3? RegisterPotentialBeamHit()
        {
            var hitCount = Physics2D.Raycast(_contactPoint ?? transform.position, transform.right, _beamRaycastFilter, _beamRaycastData, _lightBeamLength);

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
                if (hit.transform != null && hit.transform.GetComponentInChildren<LightBeamController>() != this)
                {
                    _player = hit.transform.GetComponentInChildren<PlayerController>();

                    if (_player != null && !appliedForceToPlayerThisFrame)
                    {
                        appliedForceToPlayerThisFrame = true;
                        _beamModifierData.ApplyBeamEffect(this, _beamPriority, _player, transform.right);
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
            _renderer.SetPositions(new[] { _contactPoint ?? transform.position, potentialHitPoint ?? translatedPosition});
            _renderer.startColor = _beamModifierData.Colour; 
            _renderer.endColor = _beamModifierData.Colour; 
        }

        protected void Start()
        {
            _beamRaycastFilter = new ContactFilter2D().NoFilter();
            _cachedStartRotation = transform.rotation;

            if (_mode != LightBeamMode.Source)
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

            _contactPoint = hitPoint;
            _renderer.enabled = true;
            _senderDirection = sender.transform.position - hitPoint;

            switch (_mode)
            {
                case LightBeamMode.Bounce:
                    _beamModifierData = sender.BeamModifierData;
                    var reflectedDirection = Vector2.Reflect(_senderDirection, _axisHelper.up);
                    var reflectedAngle = Mathf.Atan2(reflectedDirection.y, reflectedDirection.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, 0, reflectedAngle);
                    break;
                case LightBeamMode.Transform:

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
            _contactPoint = null;
        }
    }
}