using System;
using Controllers;
using UnityEngine;
using UnityEngine.Events;

namespace Environment.Interactables
{
    public class BasicButton : InteractableBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private SpriteRenderer _targetRenderer;

        [SerializeField]
        private Sprite _offState;

        [SerializeField]
        private Sprite _onState;

        [Header("Configuration")]
        [SerializeField]
        private UnityEvent _actions;

        private bool _isOn = false;

        protected void Start() => _targetRenderer.sprite = _offState;


        public override void Interact(PlayerController player)
        {
            _actions.Invoke();

            _isOn = !_isOn;

            _targetRenderer.sprite = _isOn ? _onState : _offState;
        }
    }
}