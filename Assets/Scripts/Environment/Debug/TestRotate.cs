using UnityEngine;

namespace Environment.Debug
{
    public class TestRotate : MonoBehaviour
    {
        [SerializeField]
        private Transform _thingToRotate;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            _thingToRotate.Rotate(new Vector3(0, 0, 1) * 5 * Time.deltaTime);
        }
    }
}