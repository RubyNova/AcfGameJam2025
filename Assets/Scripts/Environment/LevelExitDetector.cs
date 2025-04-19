using Controllers;
using Managers;
using ScriptableObjects.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Environment
{
    public class LevelExitDetector : MonoBehaviour
    {
        [SerializeField]
        private int _levelToLoad;

        [SerializeField] private bool _switchMusic = false;
        [SerializeField] private AudioManager.TrackState _trackState;

        protected void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.TryGetComponent<PlayerController>(out var _))
            {
                return;
            }

            if(_switchMusic)
            {
                AudioManager.Instance.TransitionTo(_trackState);
            }

            SceneManager.LoadScene(_levelToLoad);
        }
    }
}