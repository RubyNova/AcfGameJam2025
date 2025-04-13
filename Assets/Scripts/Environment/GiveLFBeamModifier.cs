using UnityEngine;
using UnityEngine.Events;

namespace Environment
{
    public enum ModifierType
    {
        Ghosted,
        SpeedBoost,
        Trampoline
    }
    public class GiveLFBeamModifier : MonoBehaviour
    {
        [SerializeField]
        private ModifierType _modifierToAttach;

        [SerializeField]
        private GameObject _targetAttachObject;

        [SerializeField]
        private Color[] _beamColours;

        [SerializeField]
        private UnityEvent _additionalTriggers;

        private bool _isAttached = false;

        protected void OnTriggerEnter2D(Collider2D collision)
        {
            if (!_isAttached && !collision.transform.CompareTag("Player"))
            {
                return;
            }

            switch (_modifierToAttach)
            {
                case ModifierType.Ghosted:
                GhostedModifier component = _targetAttachObject.AddComponent<GhostedModifier>();
                component.Colour = _beamColours[0];
                component.BeamForce = 5; 
                break;
                case ModifierType.Trampoline:
                TrampolineModifier trampolineComponent = _targetAttachObject.AddComponent<TrampolineModifier>();
                trampolineComponent.Colour = _beamColours[2];
                break;
            }

            _isAttached = true;

            _additionalTriggers.Invoke();
        }
    }
}