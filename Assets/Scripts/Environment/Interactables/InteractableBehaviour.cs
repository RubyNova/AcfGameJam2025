using Controllers;
using UnityEngine;

namespace Environment.Interactables
{
    public abstract class InteractableBehaviour : MonoBehaviour
    {
        [SerializeField]
        private int _priority;

        public int Priority => _priority;

        public abstract void Interact(PlayerController player);
    }
}