using UnityEngine;

namespace Environment
{
    public class MoveableObjectModifier : LightBeamModifier
    {
        public override void ApplyBeamEffectToObject(LightBeamController sender, int beamPriority, GenericBeamForceReactor target, Vector2 senderBeamDirection)
        {
            target.RegisterIncomingForce(sender, beamPriority, senderBeamDirection, BeamForce);
        }
    }
}