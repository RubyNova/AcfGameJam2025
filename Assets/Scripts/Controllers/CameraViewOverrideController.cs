using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Controllers 
{
    public class CameraViewOverrideController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CinemachineCamera _cinemachineCameraToOverride;
        [SerializeField] private CinemachineOrbitalFollow _orbitalFollowToOverride;

        [Header("Configuration")]
        [SerializeField] private float _orthographicSizeOverride;
        [SerializeField] private bool _overrideTrackingPosition = false;
        [SerializeField] private Vector3 _trackingPositionOverride = Vector3.zero;

        [Header("Readonly")]
        [SerializeField] private float _originalOrthographicSize = 0.0f;
        [SerializeField] private Vector3 _originalTrackingPosition = Vector3.zero;

        [SerializeField] private bool _switchingToOriginal = false;
        [SerializeField] private bool _switchingToOverride = false;
        [SerializeField] private float _lerpAmount = 0.0f;
        [SerializeField] private Vector3 _vectorLerp = Vector3.zero;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if(_cinemachineCameraToOverride is not null)
            {
                _originalOrthographicSize = _cinemachineCameraToOverride.Lens.OrthographicSize;
                _lerpAmount = (_orthographicSizeOverride - _originalOrthographicSize) * Time.fixedDeltaTime;
            }
            if(_orbitalFollowToOverride is not null)
            {
                _originalTrackingPosition = _orbitalFollowToOverride.TargetOffset;
                _vectorLerp = (_trackingPositionOverride - _originalTrackingPosition) * Time.fixedDeltaTime;
            }
        }

        void FixedUpdate()
        {   
            if(_switchingToOverride)
            {
                LerpToOverride();
            }

            if(_switchingToOriginal)
            {
                LerpFromOverride();
            }
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            print("yaaaasss");
            if(collision.tag == "Player" || collision.tag == "Familiar")
            {
                OverrideCamera();
            }
        }

        void LerpToOverride()
        {
            if(_cinemachineCameraToOverride is not null)
            {
                _cinemachineCameraToOverride.Lens.OrthographicSize += _lerpAmount;
                if(_cinemachineCameraToOverride.Lens.OrthographicSize > _orthographicSizeOverride)
                {
                    _cinemachineCameraToOverride.Lens.OrthographicSize = _orthographicSizeOverride;
                    _switchingToOverride = false;
                }   
            }       
            if(_overrideTrackingPosition && _orbitalFollowToOverride is not null)
            {
                _orbitalFollowToOverride.TargetOffset += _vectorLerp;
                var dist = Vector2.Distance(_orbitalFollowToOverride.TargetOffset, _trackingPositionOverride);
                
                if(dist < 0.1)
                {
                    _orbitalFollowToOverride.TargetOffset = _trackingPositionOverride;
                }
            }
        }

        void LerpFromOverride()
        {
            if(_cinemachineCameraToOverride is not null)
            {
                _cinemachineCameraToOverride.Lens.OrthographicSize -= _lerpAmount;
                if(_cinemachineCameraToOverride.Lens.OrthographicSize < _originalOrthographicSize)
                {
                    _cinemachineCameraToOverride.Lens.OrthographicSize = _originalOrthographicSize;
                    _switchingToOverride = false;
                }   
            }       
            if(_overrideTrackingPosition && _orbitalFollowToOverride is not null)
            {
                _orbitalFollowToOverride.TargetOffset -= _vectorLerp;
                var dist = _orbitalFollowToOverride.TargetOffset - _originalTrackingPosition;
                //hack for time
                if(dist.y < 0.1)
                {
                    _orbitalFollowToOverride.TargetOffset = _originalTrackingPosition;
                }
            }
        }

        public void ResetCamera()
        {
            _switchingToOriginal = true;
            _switchingToOverride = false;
        }

        public void OverrideCamera()
        {
            _switchingToOriginal = false;
            _switchingToOverride = true;
        }
    }
}