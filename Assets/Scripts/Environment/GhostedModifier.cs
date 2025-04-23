using System;
using Controllers;
using UnityEngine;

namespace Environment
{
    public class GhostedModifier : LightBeamModifier
    {
        private int _defaultLayer;
        private int _ghostedLayer;
        private int _ignoreBeamsLayer;

        protected void Start()
        {
            _defaultLayer = LayerMask.NameToLayer("Default");
            _ghostedLayer = LayerMask.NameToLayer("Ghosted");
            _ignoreBeamsLayer = LayerMask.NameToLayer("IgnoreBeams");
        }

        public override void Initialise(LightBeamController sender)
        {
            sender.ShouldIgnoreWalls = true;
        }

        public override void Shutdown(LightBeamController sender)
        {
            sender.ShouldIgnoreWalls = false;
        }

        public override void ApplyBeamEffectToPlayer(LightBeamController sender, int beamPriority, PlayerController player, Vector2 senderBeamDirection)
        {
            if (player.gameObject.layer != _ghostedLayer && player.gameObject.layer != _ignoreBeamsLayer)
            {
                player.gameObject.layer = _ghostedLayer;
            }

            base.ApplyBeamEffectToPlayer(sender, beamPriority, player, senderBeamDirection);
        }

        public override void ClearBeamEffectOnPlayer(LightBeamController sender, int beamPriority, PlayerController player)
        {
            if (player.gameObject.layer != _ignoreBeamsLayer)
            {
                player.gameObject.layer = _defaultLayer; 
            }

            base.ClearBeamEffectOnPlayer(sender, beamPriority, player);
        }
    }
}