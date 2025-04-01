using UnityEngine;

namespace Environment.VFX
{


    public class HelixRotator : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private ParticleSystem _particleSystem;

        private float _zValue = 0f;
        private float _resolution = 20;

        private void CreateCircle()
        {
            var velocity = _particleSystem.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            ParticleSystem.MainModule mainModuleSettings = _particleSystem.main;
            mainModuleSettings.startSpeed = 0f;
            var fuck = velocity.z;
            fuck.mode = ParticleSystemCurveMode.Curve;
            velocity.z = fuck;
            AnimationCurve xCurve = new();

            for (int i = 0; i < _resolution; i++)
            {
                float newTime = i/(_resolution-i);
                float newValue = Mathf.Sin(newTime * 2 * Mathf.PI);
                xCurve.AddKey(newTime, newValue);
            }

            velocity.x = new ParticleSystem.MinMaxCurve(10.0f, xCurve);

            AnimationCurve yCurve = new();

            for (int i = 0; i < _resolution; i++)
            {
                float newTime = i/(_resolution-i);
                float newValue = Mathf.Cos(newTime * 2 * Mathf.PI);
                yCurve.AddKey(newTime, newValue);
            }

            velocity.y = new ParticleSystem.MinMaxCurve(10.0f, yCurve);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            CreateCircle();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}