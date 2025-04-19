using UnityEngine;
using UnityEngine.Events;

namespace Environment
{
    public class ColliderFunctionTrigger : MonoBehaviour
    {
        [SerializeField] private bool _disableGameObjectAfterTrigger = false;
        [SerializeField] private UnityEvent EventsToFire = new();

        void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.tag == "Player" || collision.tag == "LightFamiliar")
            {
                EventsToFire.Invoke();

                if (_disableGameObjectAfterTrigger)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}