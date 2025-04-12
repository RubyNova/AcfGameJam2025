using UnityEngine;
using Controllers;

namespace Environment
{
    public class PitfallDetector : MonoBehaviour
    {
        [SerializeField]
        private Transform _resetPositionTransform;

        [SerializeField]
        private bool _forceObjectsToResetPosition = false;

        protected void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.TryGetComponent<PlayerController>(out var playerComponent))
            {
                playerComponent.transform.position = _resetPositionTransform.position;
            }
            else if (collision.gameObject.TryGetComponent<RespawnComponent>(out var respawnComponent))
            {
                if (_forceObjectsToResetPosition)
                {
                    respawnComponent.transform.position = _resetPositionTransform.position;   
                }
                else
                {
                    respawnComponent.Respawn();
                }
            }
        }
    }
}