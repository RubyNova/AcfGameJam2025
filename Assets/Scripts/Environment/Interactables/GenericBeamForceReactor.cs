using System;
using Controllers;
using UnityEngine;

namespace Environment.Interactables
{
    public class GenericBeamForceReactor : IInteractable
    {
        [Header("Dependencies")]
        [SerializeField]
        private Rigidbody2D _rigidbodyToDrive;

        private int _beamPriority = 0;

        private float _startingGravityScale;

        protected void Start()
        {
            _startingGravityScale = _rigidbodyToDrive.gravityScale;
        }

        public override void Interact(PlayerController controller)
        {

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