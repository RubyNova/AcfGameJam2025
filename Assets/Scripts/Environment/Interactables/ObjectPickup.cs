using Controllers;
using UnityEngine;


namespace Environment.Interactables
{
    public class ObjectPickup : InteractableBehaviour 
    {
        public override void Interact(PlayerController player)
        {
            transform.parent = player.transform;
            //GetComponent<Collider2D>().enabled = false;
            foreach (var collider in GetComponentsInChildren<Collider2D>())
            {
                collider.enabled = false;
            }
            GetComponent<Rigidbody2D>().simulated = false;
            player.HeldObject = this;
            transform.localPosition = player.HeldItemPoint.localPosition;
        }
    }
}