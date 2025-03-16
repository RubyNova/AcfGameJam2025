using Controllers;
using UnityEngine;

namespace Environment
{
    public class GhostedModifier : LightBeamModifier
    {
        private int _defaultLayer;
        private int _ghostedLayer;

        protected void Start()
        {
            _defaultLayer = LayerMask.NameToLayer("Default");
            _ghostedLayer = LayerMask.NameToLayer("Ghosted");
        }

        public override void ApplyBeamEffect(LightBeamController sender, int beamPriority, PlayerController player, Vector2 senderBeamDirection)
        {
            if (player.gameObject.layer != _ghostedLayer)
            {
                player.gameObject.layer = _ghostedLayer;
            }

            base.ApplyBeamEffect(sender, beamPriority, player, senderBeamDirection);
        }

        public override void ClearBeamEffect(LightBeamController sender, int beamPriority, PlayerController player)
        {
            player.gameObject.layer = _defaultLayer; 
            base.ClearBeamEffect(sender, beamPriority, player);
        }
    }
}