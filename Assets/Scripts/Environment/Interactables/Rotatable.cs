using Controllers;
using UnityEngine;

namespace Environment.Interactables
{
    public class Rotatable : InteractableBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        private float _rotationAmount;

        public override void Interact(PlayerController player)
        {
            transform.Rotate(0, 0, _rotationAmount);
        }
    }
}