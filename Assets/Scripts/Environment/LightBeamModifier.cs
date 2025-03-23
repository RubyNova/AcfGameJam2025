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
        public float BeamForce;

        public Color Colour => _colour;

        public virtual void Initialise(LightBeamController sender)
        {

        }
        
        public virtual void Shutdown(LightBeamController sender)
        {

        }

        public virtual void ApplyBeamEffect(LightBeamController sender, int beamPriority, PlayerController player, Vector2 senderBeamDirection)
        {
            player.RegisterIncomingBeamForce(sender, beamPriority, senderBeamDirection, BeamForce);
        }

        public virtual void ClearBeamEffect(LightBeamController sender, int beamPriority, PlayerController player)
        {
            player.UnregisterIncomingBeamForce(sender, beamPriority);
        }
    }
}