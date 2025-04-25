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
        [SerializeField] private AudioSource _sourceThree;
        [SerializeField] private AudioSource _sourceFour;



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
        
        [SerializeField]
        private AudioClip _jump;

        [SerializeField]
        private AudioClip _grind;

        void Start()
        {
         _sourceThree.clip = _jump;
         _sourceFour.clip = _grind;   
        }

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

        public void PlayJump()
        {
            _sourceThree.PlayScheduled(AudioSettings.dspTime);
        }

        public void PlayGrind()
        {
            _sourceFour.PlayScheduled(AudioSettings.dspTime);
        }

        public void StopGrind()
        {
            _sourceFour.Stop();
        }

    }
}