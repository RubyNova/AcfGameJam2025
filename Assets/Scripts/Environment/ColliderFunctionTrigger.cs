using UnityEngine;
using UnityEngine.Events;

namespace Environment
{
    public class ColliderFunctionTrigger : MonoBehaviour
    {
        [SerializeField] private bool _disableGameObjectAfterTrigger = false;
        [SerializeField] private UnityEvent EventsToFire = new();
        [SerializeField] private bool YeetAfterUse = false;

        void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.tag == "Player" || collision.gameObject.layer == LayerMask.NameToLayer("LightFamiliar"))
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
}