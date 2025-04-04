using UnityEngine;

namespace Environment
{
    public class GenericBeamForceReactor : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private Rigidbody2D _rigidbodyToDrive;

        private int _beamPriority = 0;

        public void RegisterIncomingForce(LightBeamController sender, int priority, Vector2 senderBeamDirection, float force)
        {
            if (_beamPriority >= priority)
            {
                return;
            }

            _beamPriority = priority;
            _rigidbodyToDrive.linearVelocity = senderBeamDirection * force;
        }
    }
}