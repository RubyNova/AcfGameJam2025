using UnityEngine;
using Controllers;
using Managers;

namespace Audio 
{
    public class AudioDistanceTrigger : AudioTrigger
    {
        public bool GizmosEnabled = false;
        public float DistanceValue = 0;
        public float TriggerThresholdValue = 0;
        public float CutoffValue = 0;
		private float distance = 0;
		private float threshold = 0;

        private PlayerController _playerReference;

        void Start()
        {
            _playerReference = FindFirstObjectByType<PlayerController>();
        }

        void Update()
        {
            if(_playerReference != null)
            {
                distance = Vector3.Distance(transform.position, _playerReference.transform.position);
                if(distance < DistanceValue)
                {
                    //TriggerThresholdValue / Distance == Percentage Volume we should be at
                    if(distance != 0f)
                    {
                        threshold = TriggerThresholdValue / distance;
                        if(threshold > CutoffValue)
                        {
                            AudioManager.Instance.SetLayerVolume(GetHashCode(), LayerNumber, (TriggerThresholdValue / distance) * 100);
                        }
                        else
                        {
                            AudioManager.Instance.SetLayerVolume(GetHashCode(), LayerNumber, 0);
                        }
                    }
                    else
                    {
                        AudioManager.Instance.SetLayerVolume(GetHashCode(), LayerNumber, 100);
                    }
                    
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            if(GizmosEnabled)
            {
                var direction = transform.right * DistanceValue;
                var thresholdDirection = transform.right * (TriggerThresholdValue / DistanceValue) * DistanceValue;
                var endpoint = transform.position + direction;
                var threshold = transform.position + thresholdDirection;

                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, direction);
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(endpoint, 0.25f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(threshold, 0.25f);
            }
        }
    }
}