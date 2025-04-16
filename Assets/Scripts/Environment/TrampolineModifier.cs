using UnityEngine;
using Environment.Interactables;

namespace Environment
{
    public class TrampolineModifier : LightBeamModifier
    {
        public override void ApplyBeamEffectToObject(LightBeamController sender, int beamPriority, GenericBeamForceReactor target, Vector2 senderBeamDirection)
        {
            var currentVelocity = target.Rigidbody.linearVelocity;
            // currentVelocity.x += currentVelocity.x < 0 ? -1 : 1;
            // currentVelocity.y += currentVelocity.y < 0 ? -1 : 1;

            var reversed = currentVelocity * -1;

            target.RegisterIncomingForce(sender, beamPriority, reversed, 1);
            print("IM RICK JAMES BITCH");
        }

        public override void ClearBeamEffectOnObject(LightBeamController sender, int beamPriority, GenericBeamForceReactor target)
        {
            target.UnregisterIncomingForce(sender, beamPriority);
        }
    }
}