using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers 
{
    public class PreloadManager : MonoBehaviour
    {
        [SerializeField]
        private int _levelToLoad;

        public Animation anim;

        void Start()
        {
            var inst = AudioManager.Instance;
            //Fix this later
            //AudioManager.Instance.PlayLayeredTrack(AudioManager.TrackState.FinalLevel);

            //SceneManager.LoadScene(_levelToLoad);
            StartCoroutine(DoThings());
        }

        private IEnumerator DoThings()
        {
            while(anim.isPlaying)
            {
            }

            AsyncOperation load = SceneManager.LoadSceneAsync(_levelToLoad);
            while(!load.isDone)
            {
                yield return null;
            }
        }

    }
}