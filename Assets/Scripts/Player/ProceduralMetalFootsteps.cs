using UnityEngine;

namespace FrozenTrain.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ProceduralMetalFootsteps : MonoBehaviour
    {
        [SerializeField] private float walkInterval = 0.46f;
        [SerializeField] private float sprintInterval = 0.31f;
        [SerializeField, Range(0f, 0.35f)] private float volume = 0.12f;

        private CharacterController controller;
        private AudioSource source;
        private AudioClip[] clips;
        private float timer;
        private int clipIndex;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            GameObject player = GameObject.Find("FPS_Player");
            if (player == null || player.GetComponent<ProceduralMetalFootsteps>() != null)
            {
                return;
            }

            player.AddComponent<ProceduralMetalFootsteps>();
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = volume;
            clips = new[]
            {
                BuildClip("MetalStep_A", 112f, 167f, 0.86f),
                BuildClip("MetalStep_B", 128f, 184f, 0.78f),
                BuildClip("MetalStep_C", 101f, 153f, 0.92f)
            };
        }

        private void Update()
        {
            if (controller == null || !controller.isGrounded)
            {
                timer = 0f;
                return;
            }

            Vector3 horizontal = controller.velocity;
            horizontal.y = 0f;
            float speed = horizontal.magnitude;
            if (speed < 0.35f)
            {
                timer = Mathf.Min(timer, 0.10f);
                return;
            }

            float interval = speed > 5.0f ? sprintInterval : walkInterval;
            interval *= Mathf.Lerp(1.08f, 0.92f, Mathf.InverseLerp(1.5f, 7f, speed));

            timer -= Time.deltaTime;
            if (timer > 0f)
            {
                return;
            }

            PlayStep(speed);
            timer = interval;
        }

        private void PlayStep(float speed)
        {
            if (clips == null || clips.Length == 0)
            {
                return;
            }

            clipIndex = (clipIndex + 1) % clips.Length;
            source.pitch = Random.Range(0.94f, 1.06f);
            source.volume = volume * Mathf.Lerp(0.82f, 1.08f, Mathf.InverseLerp(2f, 7f, speed));
            source.PlayOneShot(clips[clipIndex]);
        }

        private static AudioClip BuildClip(string name, float lowFrequency, float highFrequency, float damping)
        {
            const int sampleRate = 22050;
            const float duration = 0.115f;
            int count = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[count];

            uint noiseState = (uint)(lowFrequency * 973f + highFrequency * 271f);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sampleRate;
                float normalized = i / (float)count;
                float envelope = Mathf.Exp(-normalized * (8.2f + damping * 3.5f));

                noiseState = noiseState * 1664525u + 1013904223u;
                float noise = ((noiseState >> 8) & 0xFFFF) / 32768f - 1f;

                float body = Mathf.Sin(2f * Mathf.PI * lowFrequency * t) * 0.32f;
                float ring = Mathf.Sin(2f * Mathf.PI * highFrequency * t) * Mathf.Exp(-normalized * 12f) * 0.22f;
                float transient = noise * Mathf.Exp(-normalized * 34f) * 0.26f;
                data[i] = (body + ring + transient) * envelope * 0.62f;
            }

            AudioClip clip = AudioClip.Create(name, count, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (clips == null)
            {
                return;
            }

            foreach (AudioClip clip in clips)
            {
                if (clip != null)
                {
                    Destroy(clip);
                }
            }
        }
    }
}
