using UnityEngine;

namespace FrozenTrain.Atmosphere
{
    public sealed class TrainLightDynamics : MonoBehaviour
    {
        private Light target;
        private float baseIntensity;
        private float amplitude;
        private float speed;
        private float seed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light == null || light.name != "InteriorLight" || light.GetComponent<TrainLightDynamics>() != null)
                {
                    continue;
                }

                light.gameObject.AddComponent<TrainLightDynamics>();
            }
        }

        private void Awake()
        {
            target = GetComponent<Light>();
            if (target == null)
            {
                enabled = false;
                return;
            }

            baseIntensity = target.intensity;
            seed = transform.position.x * 0.173f + transform.position.z * 0.291f + 17.3f;

            float x = transform.position.x;
            if (x < -6.2f)
            {
                amplitude = 0.025f;
                speed = 0.65f;
            }
            else if (x < 6.2f)
            {
                amplitude = 0.085f;
                speed = 1.85f;
            }
            else if (x < 18.6f)
            {
                amplitude = 0.018f;
                speed = 0.52f;
            }
            else if (x < 31.0f)
            {
                amplitude = 0.032f;
                speed = 0.92f;
            }
            else
            {
                amplitude = 0.028f;
                speed = 0.42f;
            }
        }

        private void Update()
        {
            if (target == null)
            {
                return;
            }

            float t = Time.time * speed;
            float noise = Mathf.PerlinNoise(seed, t) * 2f - 1f;
            float mechanicalPulse = Mathf.Sin(t * 2.17f + seed) * 0.22f;
            target.intensity = baseIntensity * (1f + (noise + mechanicalPulse) * amplitude);
        }
    }
}
