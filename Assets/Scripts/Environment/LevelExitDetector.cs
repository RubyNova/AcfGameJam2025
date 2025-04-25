using System.Collections;
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
        [SerializeField] private Animator _levelTransition;
        [SerializeField] private bool disableLevelTransitionAnim = false;
        private bool transitioning = false;

        void Update()
        {
            if(transitioning && !disableLevelTransitionAnim)
            {
                if(_levelTransition.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f)
                {
                    transitioning = false;
                    StartCoroutine(SwitchLevels());
                }
            }
            else if (transitioning)
            {
                transitioning = false;
                StartCoroutine(SwitchLevels());
            }
        }

        protected void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.TryGetComponent<PlayerController>(out var _))
            {
                return;
            }

            if(!disableLevelTransitionAnim)
                _levelTransition.SetTrigger("Start");
            transitioning = true;
            if(_switchMusic)
            {
                AudioManager.Instance.PlayLayeredTracks(_trackState);
            }
        }

        private IEnumerator SwitchLevels()
        {
            AsyncOperation ao = SceneManager.LoadSceneAsync(_levelToLoad);
            while(!ao.isDone)
            {
                yield return null;
            }
        }
    }
}