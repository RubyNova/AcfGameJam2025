using System;
using Controllers;
using UnityEngine;
using UnityEngine.Events;

namespace Environment.Interactables
{
    public class BasicButton : InteractableBehaviour
    {
        public UnityEvent _actions;

        public override void Interact(PlayerController player) => _actions.Invoke();
    }
}