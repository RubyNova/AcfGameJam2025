using UnityEngine;
using UnityEngine.Events;

namespace Environment
{
    public class ColliderFunctionTrigger : MonoBehaviour
    {
        [SerializeField] public UnityEvent EventsToFire = new();

        void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.tag == "Player" || collision.tag == "LightFamiliar")
            {
                EventsToFire.Invoke();
            }
        }
    }
}