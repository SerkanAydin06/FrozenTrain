using FrozenTrain.Management;
using UnityEngine;

namespace FrozenTrain.Interaction
{
    public sealed class GeneratorRepairInteractable : InteractableBase
    {
        [SerializeField] private TrainPrototypeState trainState;
        [SerializeField, Range(0, 3)] private int repairStep;

        public override string InteractionText
        {
            get
            {
                if (trainState == null)
                {
                    return "Bakım paneli çevrimdışı";
                }

                switch (trainState.GeneratorFaultPhase)
                {
                    case GeneratorFaultPhase.Alarm:
                        return "Önce yönetim panelinden mühendis görevlendir";
                    case GeneratorFaultPhase.EngineerEnRoute:
                        return "Mühendis jeneratör vagonuna geliyor";
                    case GeneratorFaultPhase.ManualIntervention:
                        if (repairStep == 0) return "E - Ana sigortayı sıfırla  [1/3]";
                        if (repairStep == 1) return "E - Soğutma hattını aç  [2/3]";
                        return "E - Jeneratörü kalibre et  [3/3]";
                    default:
                        return "Jeneratör bakım paneli  //  Sistem normal";
                }
            }
        }

        public void Configure(TrainPrototypeState state)
        {
            trainState = state;
        }

        public override void Interact(GameObject interactor)
        {
            if (trainState == null)
            {
                trainState = Object.FindFirstObjectByType<TrainPrototypeState>();
            }

            if (trainState == null || !trainState.ManualInterventionRequired)
            {
                return;
            }

            repairStep++;
            PlayRepairClick();

            if (repairStep >= 3)
            {
                repairStep = 3;
                trainState.CompleteGeneratorRepair();
            }
        }

        private void PlayRepairClick()
        {
            AudioSource source = GetComponent<AudioSource>();
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0.65f;
                source.minDistance = 0.5f;
                source.maxDistance = 8f;
            }

            const int sampleRate = 16000;
            const float duration = 0.10f;
            int count = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - Mathf.Clamp01(t / duration);
                float low = Mathf.Sin(2f * Mathf.PI * (160f + repairStep * 45f) * t) * 0.42f;
                float click = Mathf.Sin(2f * Mathf.PI * 1150f * t) * 0.16f;
                data[i] = (low + click) * envelope;
            }

            AudioClip clip = AudioClip.Create("GeneratorRepairStep", count, 1, sampleRate, false);
            clip.SetData(data, 0);
            source.PlayOneShot(clip, 0.30f);
            Object.Destroy(clip, 0.5f);
        }
    }
}
