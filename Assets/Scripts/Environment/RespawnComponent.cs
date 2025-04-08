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

        public void Respawn() => transform.position = _respawnPosition;
    }
}