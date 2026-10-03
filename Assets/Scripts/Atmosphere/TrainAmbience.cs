using FrozenTrain.Management;
using UnityEngine;

namespace FrozenTrain.Atmosphere
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class TrainAmbience : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.25f)] private float volume = 0.075f;
        [SerializeField] private float baseFrequency = 42f;
        [SerializeField] private float secondaryFrequency = 84f;
        [SerializeField] private float wheelRhythmHz = 1.85f;
        [SerializeField, Range(0f, 1f)] private float stoppedVolumeScale = 0.25f;

        private AudioSource source;
        private TrainPrototypeState trainState;
        private float motion = 1f;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            trainState = GetComponent<TrainPrototypeState>();
            if (trainState == null) trainState = FindFirstObjectByType<TrainPrototypeState>();
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = volume;
            source.clip = CreateAmbienceClip();
            source.Play();
        }

        private void Update()
        {
            if (source == null || trainState == null) return;

            // Wheel rhythm and rumble follow train speed; a stopped train only keeps a faint hum.
            motion = Mathf.MoveTowards(motion, trainState.MotionFactor, 0.35f * Time.deltaTime);
            float moving = Mathf.Clamp01(motion);
            source.volume = volume * Mathf.Lerp(stoppedVolumeScale, 1f, moving);
            source.pitch = Mathf.Lerp(0.55f, 1f, moving) + Mathf.Max(0f, motion - 1f) * 0.25f;
        }

        private AudioClip CreateAmbienceClip()
        {
            const int sampleRate = 22050;
            const float duration = 4f;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[sampleCount];
            System.Random random = new System.Random(9041);

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;

                float low = Mathf.Sin(2f * Mathf.PI * baseFrequency * t) * 0.23f;
                float upper = Mathf.Sin(2f * Mathf.PI * secondaryFrequency * t + 0.4f) * 0.075f;
                float body = Mathf.Sin(2f * Mathf.PI * 18f * t) * 0.055f;
                float mechanicalPulse = Mathf.Sin(2f * Mathf.PI * 2.1f * t) * 0.025f;

                float rhythmPhase = Mathf.Repeat(t * wheelRhythmHz, 1f);
                float clackEnvelope = Mathf.Exp(-rhythmPhase * 27f);
                float clack = Mathf.Sin(2f * Mathf.PI * 118f * t) * clackEnvelope * 0.16f;

                float secondaryPhase = Mathf.Repeat(t * wheelRhythmHz + 0.52f, 1f);
                float secondaryEnvelope = Mathf.Exp(-secondaryPhase * 31f);
                clack += Mathf.Sin(2f * Mathf.PI * 92f * t) * secondaryEnvelope * 0.10f;

                float noise = ((float)random.NextDouble() * 2f - 1f) * 0.018f;
                float slowRumble = Mathf.PerlinNoise(t * 0.42f, 0.31f) - 0.5f;
                slowRumble *= 0.055f;

                data[i] = Mathf.Clamp((low + upper + body + mechanicalPulse + clack + noise + slowRumble) * 0.42f, -0.9f, 0.9f);
            }

            AudioClip clip = AudioClip.Create("FrozenTrain_ProceduralTrainAmbience", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
