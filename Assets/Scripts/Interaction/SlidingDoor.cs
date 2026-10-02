using UnityEngine;

namespace FrozenTrain.Interaction
{
    /// <summary>
    /// Sliding-door behaviour only. Door mesh, handle, warning plate and trim are authored
    /// in the connector prefab and are never generated at runtime.
    /// </summary>
    public sealed class SlidingDoor : InteractableBase
    {
        [SerializeField] private Vector3 openOffset = new Vector3(0f, 0f, -1.55f);
        [SerializeField] private float travelDuration = 0.55f;
        [SerializeField, Range(0f, 0.4f)] private float soundVolume = 0.16f;

        private Vector3 closedLocalPosition;
        private Vector3 startLocalPosition;
        private Vector3 targetLocalPosition;
        private float animationTime;
        private bool isOpen;
        private bool isAnimating;
        private AudioSource audioSource;
        private AudioClip openClip;
        private AudioClip closeClip;

        public bool IsOpen => isOpen;
        public override string InteractionText => isOpen ? "E - Kapıyı Kapat" : "E - Kapıyı Aç";

        private void Awake()
        {
            closedLocalPosition = transform.localPosition;
            startLocalPosition = closedLocalPosition;
            targetLocalPosition = closedLocalPosition;

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 9f;
            audioSource.volume = soundVolume;

            openClip = CreateDoorClip("DoorOpen", false);
            closeClip = CreateDoorClip("DoorClose", true);
        }

        public void Configure(Vector3 localOpenOffset, float speed)
        {
            openOffset = localOpenOffset;
            travelDuration = Mathf.Clamp(1.55f / Mathf.Max(0.1f, speed), 0.32f, 0.85f);
        }

        private void Update()
        {
            if (!isAnimating)
            {
                return;
            }

            animationTime += Time.deltaTime;
            float t = Mathf.Clamp01(animationTime / Mathf.Max(0.01f, travelDuration));
            float eased = t * t * (3f - 2f * t);
            transform.localPosition = Vector3.Lerp(startLocalPosition, targetLocalPosition, eased);

            if (t >= 1f)
            {
                transform.localPosition = targetLocalPosition;
                isAnimating = false;
            }
        }

        public override void Interact(GameObject interactor)
        {
            SetOpen(!isOpen);
        }

        public void Open()
        {
            if (!isOpen)
            {
                SetOpen(true);
            }
        }

        public void Close()
        {
            if (isOpen)
            {
                SetOpen(false);
            }
        }

        private void SetOpen(bool open)
        {
            isOpen = open;
            startLocalPosition = transform.localPosition;
            targetLocalPosition = closedLocalPosition + (isOpen ? openOffset : Vector3.zero);
            animationTime = 0f;
            isAnimating = true;

            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(isOpen ? openClip : closeClip, 1f);
            }
        }

        private AudioClip CreateDoorClip(string clipName, bool closing)
        {
            const int sampleRate = 22050;
            const float duration = 0.62f;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            System.Random random = new System.Random(closing ? 271 : 137);

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float normalized = t / duration;
                float envelope = Mathf.Sin(Mathf.Clamp01(normalized) * Mathf.PI);
                float motorFrequency = closing ? 78f : 64f;
                float motor = Mathf.Sin(2f * Mathf.PI * motorFrequency * t) * 0.22f;
                float scrape = ((float)random.NextDouble() * 2f - 1f) * 0.16f;
                float hiss = ((float)random.NextDouble() * 2f - 1f) * Mathf.Lerp(0.12f, 0.02f, normalized);
                float thumpWindow = closing ? Mathf.InverseLerp(0.82f, 1f, normalized) : Mathf.InverseLerp(0.0f, 0.12f, normalized);
                float thump = Mathf.Sin(2f * Mathf.PI * 42f * t) * thumpWindow * 0.28f;
                samples[i] = (motor + scrape + hiss) * envelope * 0.45f + thump;
            }

            AudioClip clip = AudioClip.Create("FrozenTrain_" + clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
