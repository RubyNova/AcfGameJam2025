using UnityEngine;

namespace Controllers
{
    public class ObjectFollow : MonoBehaviour
    {
        [SerializeField]
        private Transform _target;

        // Update is called once per frame
        void Update()
        {
            transform.position = _target.position;
        }
    }
}