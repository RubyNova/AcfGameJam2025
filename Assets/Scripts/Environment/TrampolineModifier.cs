using UnityEngine;
using Environment.Interactables;

namespace Environment
{
    public class TrampolineModifier : LightBeamModifier
    {
        public override void ApplyBeamEffectToObject(LightBeamController sender, int beamPriority, GenericBeamForceReactor target, Vector2 senderBeamDirection)
        {
            var currentVelocity = target.Rigidbody.linearVelocity;

            if (currentVelocity.y > 0)
            {
                return;   
            }

            

            var reflected = Vector2.Reflect(currentVelocity, sender.UpwardsNormal);
            reflected.y += BeamForce;

            target.RegisterIncomingForce(sender, beamPriority, reflected, 1);
        }

        public override void ClearBeamEffectOnObject(LightBeamController sender, int beamPriority, GenericBeamForceReactor target)
        {
            target.UnregisterIncomingForce(sender, beamPriority);
        }
    }
}