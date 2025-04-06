using UnityEngine;
using UnityEngine.Events;

namespace Environment
{
    public class WeightSwitchController : MonoBehaviour
    {
        [Header("Button Actions")]
        [SerializeField]
        private UnityEvent _onActivate;

        [SerializeField]
        private UnityEvent _onDeactivate;

        private bool _isActivated = false;

        private int _entityCount = 0;

        protected void OnCollisionEnter2D(Collision2D collision)
        {
            if (_isActivated || (collision.gameObject.layer != LayerMask.NameToLayer("InteractablePhysicsObject") && !collision.gameObject.CompareTag("Player")) || collision.transform.position.y <= transform.position.y)
            {
                return;
            }

            if (_entityCount == 0)
            {
                _onActivate.Invoke();
                _isActivated = true;
            }

            _entityCount++;
        }

        protected void OnCollisionExit2D(Collision2D collision)
        {
            if (!_isActivated || (collision.gameObject.layer != LayerMask.NameToLayer("InteractablePhysicsObject") && !collision.gameObject.CompareTag("Player")) || collision.transform.position.y <= transform.position.y)
            {
                return;
            }
            
            _entityCount--;

            if (_entityCount == 0)
            {
                _onDeactivate.Invoke();
                _isActivated = false;
            }

        }
    }
}