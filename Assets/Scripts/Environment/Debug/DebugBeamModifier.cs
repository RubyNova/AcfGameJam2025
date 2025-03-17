namespace Environment.Debug
{
    public class DebugBeamModifier : LightBeamModifier 
    {
        private void Start()
        {
            UnityEngine.Debug.LogWarning("You have a debug beam modifier in your scene! This modifier will do nothing besides change the beam colour and apply default behaviour for testing! GameObject: " + gameObject.name);
        }
    }
}