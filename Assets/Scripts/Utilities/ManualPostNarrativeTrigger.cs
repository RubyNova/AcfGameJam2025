using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    public class ManualPostNarrativeTrigger : MonoBehaviour
    {
        public UnityEvent PostNarrativeEvents = new();

        public void InvokeEvents()
        {
            PostNarrativeEvents.Invoke();
        }
    }
}