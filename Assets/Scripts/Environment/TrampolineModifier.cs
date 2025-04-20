using UnityEngine;
using Environment.Interactables;
using System;

namespace Environment
{
    public class TrampolineModifier : LightBeamModifier
    {
        [SerializeField]
        private float _toleranceValue = 20;

        public override void ApplyBeamEffectToObject(LightBeamController sender, int beamPriority, GenericBeamForceReactor target, Vector2 senderBeamDirection)
        {
            var currentVelocity = target.Rigidbody.linearVelocity;

            bool isUpsideDown = sender.UpwardsNormal.y < 0;

            var comparisonDirection = isUpsideDown ? -sender.UpwardsNormal : sender.UpwardsNormal;

            if (Vector2.Angle(currentVelocity.normalized, comparisonDirection) < _toleranceValue)
            {
                return;   
            }

            

            var reflected = Vector2.Reflect(currentVelocity, sender.UpwardsNormal);
            reflected.y += BeamForce;

            target.RegisterIncomingForce(sender, beamPriority, reflected, BeamForce);
        }

        public override void ClearBeamEffectOnObject(LightBeamController sender, int beamPriority, GenericBeamForceReactor target)
        {
            target.UnregisterIncomingForce(sender, beamPriority);
        }
    }
}