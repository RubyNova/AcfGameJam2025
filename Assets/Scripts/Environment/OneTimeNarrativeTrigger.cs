using System;
using ACHNarrativeDriver;
using ACHNarrativeDriver.ScriptableObjects;
using UnityEngine;

namespace Environment
{
    public class OneTimeNarrativeTrigger : MonoBehaviour
    {
        [SerializeField]
        private NarrativeSequence _targetSequence;

        private NarrativeUIController _controller;

        private bool _alreadyActivated = false;

        protected void Start()
        {
            _controller = FindFirstObjectByType<NarrativeUIController>();

            if (_controller == null)
            {
                throw new NullReferenceException("No NarrativeUIController was found in the scene. Please ensure one exists.");
            }
        }

        public void ExecuteTargetSequence()
        {
            if (_alreadyActivated)
            {
                return;   
            }

            _alreadyActivated = true;

            _controller.ExecuteSequence(_targetSequence);
        } 
    }
}