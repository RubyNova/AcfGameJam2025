using System;
using Controllers;
using UnityEngine;

namespace Environment.Interactables
{
    public class GenericBeamForceReactor : MonoBehaviour 
    {
        [Header("Dependencies")]
        [SerializeField]
        private Rigidbody2D _rigidbodyToDrive;

        [Header("Configuration")]
        [SerializeField]
        public Vector2 VelocityCap;

        private int _beamPriority = 0;

        private float _startingGravityScale;

        public Rigidbody2D Rigidbody => _rigidbodyToDrive;

        

        protected void Start()
        {
            _startingGravityScale = _rigidbodyToDrive.gravityScale;
        }

        protected void Update()
        {
            // _rigidbodyToDrive.linearVelocityX = Mathf.Clamp(_rigidbodyToDrive.linearVelocityX, -VelocityCap.x, VelocityCap.x);
            // _rigidbodyToDrive.linearVelocityY = Mathf.Clamp(_rigidbodyToDrive.linearVelocityY, -VelocityCap.y, VelocityCap.y);
        }

        public void RegisterIncomingForce(LightBeamController sender, int priority, Vector2 senderBeamDirection, float force)
        {
            if (_beamPriority > priority)
            {
                return;
            }

            _beamPriority = priority;
            _rigidbodyToDrive.linearVelocity = senderBeamDirection * force;
            _rigidbodyToDrive.gravityScale = 0;
        }

        public void UnregisterIncomingForce(LightBeamController sender, int priority)
        {
            if (_beamPriority > priority)
            {
                return;   
            }

            _rigidbodyToDrive.gravityScale = _startingGravityScale;
            _beamPriority = 0;
        }
    }
}