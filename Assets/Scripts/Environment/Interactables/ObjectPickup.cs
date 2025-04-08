using Controllers;
using UnityEngine;


namespace Environment.Interactables
{
    public class ObjectPickup : InteractableBehaviour 
    {
        public override void Interact(PlayerController player)
        {
            transform.parent = player.transform;
            GetComponent<Collider2D>().enabled = false;
            GetComponent<Rigidbody2D>().simulated = false;
            player.HeldObject = this;
            transform.localPosition = player.HeldItemPoint.localPosition;
        }
    }
}