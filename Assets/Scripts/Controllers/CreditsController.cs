using System.Collections;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Controllers
{
    public class CreditsController : MonoBehaviour
    {
        public Animator LevelTransition;
        public Animator Credits;
        public int LevelToLoad;

        bool creditsRolling = false;
        bool transitionComplete = false;
        bool transitioning = false;

        void Start()
        {
            creditsRolling = true;
        }

        void Update()
        {
            var nt = Credits.GetCurrentAnimatorStateInfo(0).normalizedTime;
            
            if(creditsRolling && nt > 1.0f)
            {
                creditsRolling = false;
                transitioning = true;
            }

            if(transitioning)
            {
                LevelTransition.SetTrigger("Start");
                transitioning = false;
            }
            else if(!creditsRolling && !transitioning && !transitionComplete)
            {
                var lt = LevelTransition.GetCurrentAnimatorStateInfo(0).normalizedTime;
                if(lt > 1.0f)
                {
                    transitionComplete = true;
                }
            }
            if(transitionComplete)
            {
                StartCoroutine(GoToTitle());
            }
        }

        IEnumerator GoToTitle()
        {         
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(LevelToLoad);
            while(!asyncLoad.isDone)
            {
                yield return null;
            }
        }
    }
}