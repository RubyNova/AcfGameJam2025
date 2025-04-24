using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers 
{
    public class PreloadManager : MonoBehaviour
    {
        [SerializeField]
        private int _levelToLoad;

        public Animator anim;

        void Start()
        {
            var inst = AudioManager.Instance;
            //Fix this later
            //AudioManager.Instance.PlayLayeredTrack(AudioManager.TrackState.FinalLevel);

            //SceneManager.LoadScene(_levelToLoad);
        
        }

        void Update()
        {
            var l = anim.GetCurrentAnimatorStateInfo(0);
            if(l.normalizedTime > 1.0f)
            {
                StartCoroutine(DoThings());   
            }
        }

        private IEnumerator DoThings()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(_levelToLoad);
            while(!load.isDone)
            {
                yield return null;
            }
        }

    }
}