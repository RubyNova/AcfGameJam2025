using UnityEngine;

namespace Environment
{
    public class CustomObjectSpawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject _objectToSpawn;

        [SerializeField]
        private Transform _spawnPosition;


        public void Spawn() => Instantiate(_objectToSpawn, _spawnPosition.position, _spawnPosition.rotation);
    }
}