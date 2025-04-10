using UnityEngine;
using UnityEngine.Audio;

namespace Controllers
{

    public class PlayerAudioController : MonoBehaviour
    {
        [Header("Dependencies")]

        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioSource _source;

        [SerializeField] private AudioSource _sourceTwo;

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
            float pitchValue = Random.Range(0.5f, 1.5f);
            if(i % 2 == 0)
            {
                _source.clip = _walkingOne;
            }
            else
            {
                _source.clip = _walkingTwo;
            }
            _mixer.SetFloat("BasicMovementPitch", pitchValue);
            _source.PlayScheduled(AudioSettings.dspTime);
        }

        public void PlayRunningClip()
        {
            int i = Random.Range(1,100);
            float pitchValue = Random.Range(0.5f, 1.5f);
            if(i % 2 == 0)
            {
                _source.clip = _runningOne;
            }
            else
            {
                _source.clip = _runningTwo;
            }
            _mixer.SetFloat("BasicMovementPitch", pitchValue);
            _source.PlayScheduled(AudioSettings.dspTime);
        }

        public void PlayLandingClip(bool highFall)
        {
            _sourceTwo.clip = highFall ? _landTwo : _landOne;
            _mixer.SetFloat("FallLandPitch", Random.Range(0.5f, 1.5f));
            _sourceTwo.PlayScheduled(AudioSettings.dspTime);
        }

    }
}