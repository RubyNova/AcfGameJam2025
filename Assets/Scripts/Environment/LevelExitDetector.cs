using System.Collections;
using System.Text;
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
        
        [SerializeField] private bool turnOffMusic = false;
        [SerializeField] private int loadDelay = 0;
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
            if(_switchMusic && !turnOffMusic)
            {
                AudioManager.Instance.TransitionTo(_trackState);
            }

            if(turnOffMusic)
            {
                AudioManager.Instance.RemoveLayerFromMusic(0);
                AudioManager.Instance.RemoveLayerFromMusic(1);
                AudioManager.Instance.RemoveLayerFromMusic(2);
                AudioManager.Instance.RemoveLayerFromMusic(3);
                AudioManager.Instance.RemoveLayerFromMusic(4);
            }
        }

        private IEnumerator SwitchLevels()
        {
            if(loadDelay != 0)
            {
                yield return new WaitForSeconds(loadDelay);
            }

            AsyncOperation ao = SceneManager.LoadSceneAsync(_levelToLoad);
            while(!ao.isDone)
            {
                yield return null;
            }
        }
    }
}