using Controllers;
using UnityEngine;

namespace Environment.Interactables
{
    public abstract class IInteractable : MonoBehaviour
    {
        public abstract void Interact(PlayerController player);
    }
}