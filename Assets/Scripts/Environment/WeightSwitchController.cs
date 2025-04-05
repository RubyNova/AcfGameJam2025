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

        protected void OnCollisionEnter2D(Collision2D collision)
        {
            if (_isActivated || (collision.gameObject.layer != LayerMask.NameToLayer("InteractablePhysicsObject") && !collision.gameObject.CompareTag("Player")) || collision.transform.position.y <= transform.position.y)
            {
                return;
            }
            print("Firing!");

            _onActivate.Invoke();
            _isActivated = true;
        }

        protected void OnCollisionExit2D(Collision2D collision)
        {
            if (!_isActivated || (collision.gameObject.layer != LayerMask.NameToLayer("InteractablePhysicsObject") && !collision.gameObject.CompareTag("Player")) || collision.transform.position.y <= transform.position.y)
            {
                return;
            }

            _onDeactivate.Invoke();
            _isActivated = false;
        }
    }
}