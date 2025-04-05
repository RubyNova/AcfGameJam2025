using System;
using Controllers;
using UnityEngine;
using UnityEngine.Events;

namespace Environment.Interactables
{
    public class BasicButton : IInteractable
    {
        public UnityEvent _actions;
        public int fuck;

        public override void Interact(PlayerController player) => _actions.Invoke();
    }
}