using System;
using System.Collections;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Managers.AudioManager;

namespace UI
{
    public class TitleScreenController : MonoBehaviour
    {
        public Animator LoadingScreenAnimator;
        public GameObject AnyButtonButton;
        public int LevelFromBuildIndexToLoad;
        public int SecondsDelay;
        public TrackState TrackToLoad;
        public AudioSource audioSource;

        private bool initialAnimDone = false;

        private bool navTime = false;

        void Start()
        {
            if(LoadingScreenAnimator is null || AnyButtonButton is null)
                throw new NotSupportedException("Missing Component Assignments in TitleScreenController!");
        }

        void Update()
        {
            var l = LoadingScreenAnimator.GetCurrentAnimatorStateInfo(0);
            if(!initialAnimDone)
            {
                if(l.normalizedTime > 1.0f)
                {
                    //we're finished
                    Invoke("EnableAnyButton", 2.0f);
                    initialAnimDone = true;
                }
            }

            if(navTime)
            {
                if(l.normalizedTime > 1.0f)
                {
                    StartCoroutine(StartGame(SecondsDelay));
                    navTime = false;
                }
            }
            
        }

        private void EnableAnyButton() => AnyButtonButton.SetActive(true);

        public void BeginGame()
        {
            LoadingScreenAnimator.SetTrigger("Start");
            navTime = true;
        }

        private IEnumerator StartGame(int secondsDelay)
        {
            while(audioSource.volume != 0.0f)
            {
                audioSource.volume -= Mathf.Lerp(75, 0, Time.deltaTime);
                if(audioSource.volume > 1.0f)
                    audioSource.volume = 0.0f;
            }
            yield return new WaitForSeconds(secondsDelay);

            AudioManager.Instance.PlayLayeredTrack(TrackToLoad);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(LevelFromBuildIndexToLoad);
            while(!asyncLoad.isDone)
            {
                yield return null;
            }
        }
    }
}