using UnityEngine;
using Controllers;

namespace Environment
{
    public class PitfallDetector : MonoBehaviour
    {
        [SerializeField]
        private Transform _resetPositionTransform;

        protected void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.TryGetComponent<PlayerController>(out var component))
            {
                return;
            }

            component.transform.position = _resetPositionTransform.position;
        }
    }
}