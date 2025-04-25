using UnityEngine;

namespace Environment
{
    public class LightBeamDataGroup
    {
        public LightBeamController Controller { get; internal set; }
        public int Priority { get; set; }
        public Vector2 DirectionAndForce { get; set; }

        public bool CanBeExited { get; set; } = true;
    }
}