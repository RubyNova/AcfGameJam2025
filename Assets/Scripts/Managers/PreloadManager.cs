using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers 
{
    public class PreloadManager : MonoBehaviour
    {
        [SerializeField]
        private int _levelToLoad;

        void Start()
        {
            //Fix this later
            AudioManager.Instance.PlayLayeredTrack(AudioManager.TrackState.Track1);

            SceneManager.LoadScene(_levelToLoad);
        }

    }
}