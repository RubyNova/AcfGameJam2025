using UnityEngine;
using UnityEngine.Events;

namespace Environment
{
    public class ColliderFunctionTrigger : MonoBehaviour
    {
        [SerializeField] private bool _disableGameObjectAfterTrigger = false;
        [SerializeField] private bool YeetAfterUse = false;
        [SerializeField] public bool FireOnExit = false;
        [SerializeField] private float FiringDelay = 0.0f;
        [SerializeField] private UnityEvent EventsToFire = new();
        
        void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.tag == "Player" 
                || collision.gameObject.layer == LayerMask.NameToLayer("LightFamiliar")
                || collision.gameObject.layer == LayerMask.NameToLayer("InteractablePhysicsObject"))
            {
                if(!FireOnExit)
                {
                    if(FiringDelay == 0.0f)
                    {
                        InvokeEvents();
                    }
                    else
                    {
                        Invoke("InvokeEvents", FiringDelay);
                    }
                }
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if(collision.tag == "Player" 
                || collision.gameObject.layer == LayerMask.NameToLayer("LightFamiliar")
                || collision.gameObject.layer == LayerMask.NameToLayer("InteractablePhysicsObject"))
            {
                if(FireOnExit)
                {
                    if(FiringDelay == 0.0f)
                    {
                        InvokeEvents();
                    }
                    else
                    {
                        Invoke("InvokeEvents", FiringDelay);
                    }
                }
            }
        }

        private void InvokeEvents()
        {
            EventsToFire.Invoke();

            if(YeetAfterUse)
            {
                Destroy(this);
            }
            else if (_disableGameObjectAfterTrigger)
            {
                gameObject.SetActive(false);
            }
        }
    }
}