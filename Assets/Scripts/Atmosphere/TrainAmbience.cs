using UnityEngine;

namespace FrozenTrain.Atmosphere
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class TrainAmbience : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.25f)] private float volume = 0.065f;
        [SerializeField] private float baseFrequency = 44f;
        [SerializeField] private float secondaryFrequency = 88f;

        private void Awake()
        {
            AudioSource source = GetComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = volume;
            source.clip = CreateHumClip();
            source.Play();
        }

        private AudioClip CreateHumClip()
        {
            const int sampleRate = 22050;
            const float duration = 2f;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float low = Mathf.Sin(2f * Mathf.PI * baseFrequency * t) * 0.32f;
                float upper = Mathf.Sin(2f * Mathf.PI * secondaryFrequency * t) * 0.11f;
                float pulse = Mathf.Sin(2f * Mathf.PI * 2.2f * t) * 0.025f;
                data[i] = (low + upper + pulse) * 0.35f;
            }

            AudioClip clip = AudioClip.Create("FrozenTrain_ProceduralHum", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
