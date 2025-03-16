using Controllers;
using UnityEngine;

namespace Environment
{
    public abstract class LightBeamModifier : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        private Color _colour;
        
        [SerializeField]
        private float _beamForce;

        public Color Colour => _colour;

        public virtual void ApplyBeamEffect(LightBeamController sender, int beamPriority, PlayerController player, Vector2 senderBeamDirection)
        {
            player.RegisterIncomingBeamForce(sender, beamPriority, senderBeamDirection, _beamForce);
        }

        public virtual void ClearBeamEffect(LightBeamController sender, int beamPriority, PlayerController player)
        {
            player.UnregisterIncomingBeamForce(sender, beamPriority);
        }
    }
}