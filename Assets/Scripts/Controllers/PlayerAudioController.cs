using UnityEngine;

namespace Controllers
{

    public class PlayerAudioController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private AudioSource _source;

        [SerializeField]
        private AudioClip _walkingOne;

        [SerializeField]
        private AudioClip _walkingTwo;

        [SerializeField]
        private AudioClip _runningOne;

        [SerializeField]
        private AudioClip _runningTwo;

        [SerializeField]
        private AudioClip _landOne;

        [SerializeField]
        private AudioClip _landTwo;

        public void PlayWalkingClip()
        {
            int i = Random.Range(1,100);
            if(i % 2 == 0)
            {
                _source.clip = _walkingOne;
            }
            else
            {
                _source.clip = _walkingTwo;
            }
            _source.PlayScheduled(AudioSettings.dspTime);
        }

        public void PlayRunningClip()
        {
            int i = Random.Range(1,100);
            if(i % 2 == 0)
            {
                _source.clip = _runningOne;
            }
            else
            {
                _source.clip = _runningTwo;
            }
            _source.PlayScheduled(AudioSettings.dspTime);
        }

        public void PlayLandingClip(bool highFall)
        {
            _source.clip = highFall ? _landTwo : _landOne;
            _source.PlayScheduled(AudioSettings.dspTime);
        }

    }
}