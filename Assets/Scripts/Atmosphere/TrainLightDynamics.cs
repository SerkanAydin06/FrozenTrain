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
            seed = transform.position.x * 0.173f + 17.3f;

            float x = transform.position.x;
            if (x < -23.7f)
            {
                amplitude = 0.018f;
                speed = 0.55f;
            }
            else if (x < -7.9f)
            {
                amplitude = 0.055f;
                speed = 1.35f;
            }
            else if (x < 7.9f)
            {
                amplitude = 0.012f;
                speed = 0.45f;
            }
            else if (x < 23.7f)
            {
                amplitude = 0.022f;
                speed = 0.72f;
            }
            else
            {
                amplitude = 0.018f;
                speed = 0.38f;
            }
        }

        private void Update()
        {
            if (target == null) return;
            float t = Time.time * speed;
            float noise = Mathf.PerlinNoise(seed, t) * 2f - 1f;
            float pulse = Mathf.Sin(t * 2.17f + seed) * 0.18f;
            target.intensity = baseIntensity * (1f + (noise + pulse) * amplitude);
        }
    }
}
