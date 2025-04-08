using Controllers;
using UnityEngine;

namespace Environment.Interactables
{
    public abstract class InteractableBehaviour : MonoBehaviour
    {
        public abstract void Interact(PlayerController player);
    }
}