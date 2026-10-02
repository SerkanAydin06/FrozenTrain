using UnityEngine;

namespace FrozenTrain.Interaction
{
    /// <summary>
    /// Polished train sliding door for the FPS prototype.
    /// Uses eased movement, procedural mechanical audio and lightweight runtime detailing.
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
            CreateRuntimeDetails();
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
            isOpen = !isOpen;
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

        private void CreateRuntimeDetails()
        {
            Renderer sourceRenderer = GetComponent<Renderer>();
            if (sourceRenderer == null)
            {
                return;
            }

            Material baseMaterial = sourceRenderer.material;
            Material accentMaterial = new Material(baseMaterial);
            Color accent = new Color(0.75f, 0.22f, 0.045f);
            accentMaterial.color = accent;
            if (accentMaterial.HasProperty("_BaseColor")) accentMaterial.SetColor("_BaseColor", accent);
            if (accentMaterial.HasProperty("_EmissionColor"))
            {
                accentMaterial.EnableKeyword("_EMISSION");
                accentMaterial.SetColor("_EmissionColor", accent * 1.8f);
            }

            CreateDetail("CenterSeam", new Vector3(0.066f, 0f, 0f), new Vector3(0.018f, 1.86f, 0.045f), baseMaterial);
            CreateDetail("Handle", new Vector3(0.074f, -0.06f, 0.34f), new Vector3(0.035f, 0.30f, 0.055f), accentMaterial);
            CreateDetail("WarningPlate", new Vector3(0.073f, 0.64f, 0f), new Vector3(0.022f, 0.14f, 0.52f), accentMaterial);
            CreateDetail("LowerKickPlate", new Vector3(0.071f, -0.78f, 0f), new Vector3(0.020f, 0.30f, 1.10f), baseMaterial);
        }

        private void CreateDetail(string detailName, Vector3 worldSizedLocalOffset, Vector3 worldSize, Material material)
        {
            GameObject detail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            detail.name = detailName;
            Destroy(detail.GetComponent<Collider>());
            detail.transform.SetParent(transform, false);

            Vector3 parentScale = transform.localScale;
            detail.transform.localPosition = new Vector3(
                worldSizedLocalOffset.x / Mathf.Max(0.001f, parentScale.x),
                worldSizedLocalOffset.y / Mathf.Max(0.001f, parentScale.y),
                worldSizedLocalOffset.z / Mathf.Max(0.001f, parentScale.z));
            detail.transform.localScale = new Vector3(
                worldSize.x / Mathf.Max(0.001f, parentScale.x),
                worldSize.y / Mathf.Max(0.001f, parentScale.y),
                worldSize.z / Mathf.Max(0.001f, parentScale.z));
            detail.GetComponent<Renderer>().sharedMaterial = material;
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
