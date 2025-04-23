using Environment.Interactables;
using UnityEngine;

namespace Environment
{
    public class MoveableObjectModifier : LightBeamModifier
    {
        private GenericBeamForceReactor _cachedTarget = null;
        private LightBeamController _cachedLbc = null;

        public override void ApplyBeamEffectToObject(LightBeamController sender, int beamPriority, GenericBeamForceReactor target, Vector2 senderBeamDirection)
        {
            _cachedTarget = target;
            _cachedLbc = sender;
            target.RegisterIncomingForce(sender, beamPriority, senderBeamDirection, BeamForce);
        }

        public override void ClearBeamEffectOnObject(LightBeamController sender, int beamPriority, GenericBeamForceReactor target)
        {
            _cachedTarget = null;
            _cachedLbc = null;
            target.UnregisterIncomingForce(sender, beamPriority);
        }

        void OnDisable()
        {
            if(_cachedTarget != null && _cachedLbc != null)
            {
                _cachedTarget.UnregisterIncomingForce(_cachedLbc, _cachedLbc.BeamPriority);
            }
        }
    }
}