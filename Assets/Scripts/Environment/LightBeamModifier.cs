using Controllers;
using Environment.Interactables;
using UnityEngine;
using UnityEngine.Serialization;

namespace Environment
{
    public abstract class LightBeamModifier : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        private Color _colour;
        
        [SerializeField, FormerlySerializedAs("BeamForce")]
        private float _beamForce;

        public Color Colour
        {
            get => _colour;
            set => _colour = value;
        }

        public float BeamForce
        {
            get => _beamForce;
            set => _beamForce = value;
        }

        public virtual void Initialise(LightBeamController sender)
        {

        }
        
        public virtual void Shutdown(LightBeamController sender)
        {

        }

        public virtual void ApplyBeamEffectToPlayer(LightBeamController sender, int beamPriority, PlayerController player, Vector2 senderBeamDirection)
        {
            player.RegisterIncomingBeamForce(sender, beamPriority, senderBeamDirection, BeamForce);
        }

        public virtual void ClearBeamEffectOnPlayer(LightBeamController sender, int beamPriority, PlayerController player)
        {
            player.UnregisterIncomingBeamForce(sender, beamPriority);
        }

        public virtual void ApplyBeamEffectToObject(LightBeamController sender, int beamPriority, GenericBeamForceReactor target, Vector2 senderBeamDirection)
        {

        }

        public virtual void ClearBeamEffectOnObject(LightBeamController sender, int beamPriority, GenericBeamForceReactor target)
        {

        }
    }
}