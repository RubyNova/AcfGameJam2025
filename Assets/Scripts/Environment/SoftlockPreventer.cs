using Controllers;
using Environment.Interactables;
using UnityEngine;
using UnityEngine.Events;

namespace Environment
{
    public class SoftlockPreventer : MonoBehaviour
    {
        [SerializeField]
        private InteractableBehaviour _targetInteractable;

        [SerializeField]
        private UnityEvent _preventSoftlockActions = new();

        void OnTriggerEnter2D(Collider2D collision)
        {
            print(collision.gameObject.name);
            if (collision.gameObject.TryGetComponent<PlayerController>(out var player) && player.HeldObject == null)
            {
                _preventSoftlockActions.Invoke();
            }
        }
    }
}