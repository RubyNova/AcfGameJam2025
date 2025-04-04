using System;
using UnityEngine;
using UnityEngine.Events;

namespace Environment.Interactables
{
    public class BasicButton : IInteractable
    {
        public UnityEvent _actions;
        public int fuck;

        public override void Interact() => _actions.Invoke();
    }
}