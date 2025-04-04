using Controllers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Environment
{
    public class LevelExitDetector : MonoBehaviour
    {
        [SerializeField]
        private int _levelToLoad;

        protected void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.TryGetComponent<PlayerController>(out var _))
            {
                return;
            }

            SceneManager.LoadScene(_levelToLoad);
        }
    }
}