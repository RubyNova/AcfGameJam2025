using UnityEngine;

namespace Environment
{
    public class CustomToggle : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] _objectsToToggle;

        public void ToggleAllObjects()
        {
            foreach (var item in _objectsToToggle)
            {
                item.SetActive(!item.activeSelf);
            }
        }
    }
}