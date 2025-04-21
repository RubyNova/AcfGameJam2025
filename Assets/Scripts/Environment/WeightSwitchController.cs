using UnityEngine;
using UnityEngine.Events;

namespace Environment
{
    public class WeightSwitchController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private Animator _buttonAnimator;

        [Header("Button Actions")]
        [SerializeField]
        private UnityEvent _onActivate;

        [SerializeField]
        private UnityEvent _onDeactivate;

        private int _entityCount = 0;

        protected void OnCollisionEnter2D(Collision2D collision)
        {
            if ((collision.gameObject.layer != LayerMask.NameToLayer("InteractablePhysicsObject") && !collision.gameObject.CompareTag("Player")) /*|| collision.transform.position.y <= transform.position.y*/)
            {
                return;
            }

            if (_entityCount == 0)
            {
                _buttonAnimator.SetBool("IsPressed", true);
                _onActivate.Invoke();
            }

            _entityCount++; 
        }

        protected void OnCollisionExit2D(Collision2D collision)
        {
            if ((collision.gameObject.layer != LayerMask.NameToLayer("InteractablePhysicsObject") && !collision.gameObject.CompareTag("Player")) /*|| collision.transform.position.y <= transform.position.y*/)
            {
                return;
            }
            
            _entityCount--;

            if (_entityCount == 0)
            {
                _buttonAnimator.SetBool("IsPressed", false);
                _onDeactivate.Invoke();
            }

        }
    }
}