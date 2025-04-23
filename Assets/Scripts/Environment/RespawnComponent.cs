using Controllers;
using UnityEngine;

namespace Environment
{
    public class RespawnComponent : MonoBehaviour
    {
        private Vector3 _respawnPosition;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected void Start()
        {
            _respawnPosition = transform.position;
        }

        public void Respawn()
        { 
            var pc = FindAnyObjectByType<PlayerController>();
            if(pc != null && pc.HeldObject != null && pc.HeldObject.gameObject.GetHashCode() == gameObject.GetHashCode())
                return;
                
            transform.position = _respawnPosition;
        }
    }
}