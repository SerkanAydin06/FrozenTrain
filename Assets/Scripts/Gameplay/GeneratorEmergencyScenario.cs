using System.Collections.Generic;
using FrozenTrain.Interaction;
using FrozenTrain.Management;
using UnityEngine;

namespace FrozenTrain.Gameplay
{
    public sealed class GeneratorEmergencyScenario : MonoBehaviour
    {
        [SerializeField] private float faultDelaySeconds = 12f;
        [SerializeField] private float engineerMoveSpeed = 1.75f;

        private TrainPrototypeState trainState;
        private Transform engineer;
        private Vector3 engineerTarget = new Vector3(-2.55f, 0.10f, -1.05f);
        private readonly HashSet<SlidingDoor> openedDoors = new HashSet<SlidingDoor>();

        private float faultTimer;
        private float alarmCooldown;
        private float repairedBannerTimer;
        private bool faultTriggered;
        private bool repairWasActive;

        private AudioSource alarmSource;
        private AudioClip alarmClip;
        private GUIStyle alertStyle;
        private GUIStyle alertSmallStyle;
        private Texture2D dangerTexture;
        private Texture2D warningTexture;
        private Texture2D goodTexture;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            GameObject train = GameObject.Find("TRAIN_INTERIOR_PROTOTYPE");
            if (train == null || train.GetComponent<GeneratorEmergencyScenario>() != null)
            {
                return;
            }

            train.AddComponent<GeneratorEmergencyScenario>();
        }

        private void Awake()
        {
            trainState = GetComponent<TrainPrototypeState>();
            faultTimer = faultDelaySeconds;

            CreateEngineer();
            CreateManualRepairPanel();
            CreateAlarmAudio();
        }

        private void Update()
        {
            if (trainState == null)
            {
                return;
            }

            if (!faultTriggered)
            {
                faultTimer -= Time.unscaledDeltaTime;
                if (faultTimer <= 0f)
                {
                    faultTriggered = trainState.TriggerGeneratorFault();
                    repairWasActive = faultTriggered;
                    alarmCooldown = 0f;
                }
            }

            if (trainState.CurrentGeneratorFaultPhase == GeneratorFaultPhase.EngineerEnRoute)
            {
                MoveEngineerToGenerator();
            }

            if (trainState.GeneratorFaultActive)
            {
                UpdateAlarm();
            }
            else if (repairWasActive)
            {
                repairWasActive = false;
                repairedBannerTimer = 5f;
            }

            if (repairedBannerTimer > 0f)
            {
                repairedBannerTimer -= Time.unscaledDeltaTime;
            }
        }

        private void MoveEngineerToGenerator()
        {
            if (engineer == null)
            {
                return;
            }

            SlidingDoor[] doors = Object.FindObjectsByType<SlidingDoor>(FindObjectsSortMode.None);
            foreach (SlidingDoor door in doors)
            {
                if (door == null || openedDoors.Contains(door))
                {
                    continue;
                }

                float xDistance = Mathf.Abs(engineer.position.x - door.transform.position.x);
                if (xDistance < 1.55f && door.transform.position.x < engineer.position.x + 0.5f)
                {
                    door.Open();
                    openedDoors.Add(door);
                }
            }

            Vector3 target = engineerTarget;
            if (engineer.position.x > 4.2f)
            {
                target = new Vector3(engineerTarget.x, engineer.position.y, 0f);
            }

            Vector3 before = engineer.position;
            engineer.position = Vector3.MoveTowards(
                engineer.position,
                target,
                engineerMoveSpeed * Time.deltaTime);

            Vector3 delta = engineer.position - before;
            if (delta.sqrMagnitude > 0.00001f)
            {
                Vector3 flat = new Vector3(delta.x, 0f, delta.z);
                if (flat.sqrMagnitude > 0.00001f)
                {
                    engineer.rotation = Quaternion.Slerp(
                        engineer.rotation,
                        Quaternion.LookRotation(flat.normalized, Vector3.up),
                        10f * Time.deltaTime);
                }

                float bob = Mathf.Sin(Time.time * 9f) * 0.018f;
                engineer.localScale = new Vector3(1f, 1f + bob, 1f);
            }

            if (Vector3.Distance(engineer.position, engineerTarget) < 0.18f)
            {
                engineer.position = engineerTarget;
                engineer.localScale = Vector3.one;
                trainState.MarkEngineerOnSite();
                alarmCooldown = 0f;
            }
        }

        private void UpdateAlarm()
        {
            alarmCooldown -= Time.unscaledDeltaTime;
            if (alarmCooldown > 0f || alarmSource == null || alarmClip == null)
            {
                return;
            }

            if (trainState.CurrentGeneratorFaultPhase == GeneratorFaultPhase.EngineerEnRoute)
            {
                alarmCooldown = 4.5f;
                return;
            }

            alarmSource.PlayOneShot(alarmClip, 0.45f);
            alarmCooldown = trainState.ManualInterventionRequired ? 2.2f : 3.0f;
        }

        private void CreateEngineer()
        {
            GameObject root = new GameObject("Engineer_Murat");
            root.transform.SetParent(transform, true);
            root.transform.position = new Vector3(13.4f, 0.10f, 0.55f);
            engineer = root.transform;

            Material suit = RuntimeMaterial("EngineerSuit", new Color(0.11f, 0.17f, 0.20f), 0.22f, 0.20f);
            Material vest = RuntimeMaterial("EngineerVest", new Color(0.42f, 0.18f, 0.035f), 0.25f, 0.10f);
            Material skin = RuntimeMaterial("EngineerSkin", new Color(0.55f, 0.40f, 0.31f), 0.18f, 0f);
            Material helmet = RuntimeMaterial("EngineerHelmet", new Color(0.58f, 0.30f, 0.045f), 0.30f, 0.18f);

            CreatePrimitive(root.transform, PrimitiveType.Capsule, "Body", new Vector3(0f, 0.92f, 0f), new Vector3(0.43f, 0.62f, 0.34f), suit);
            CreatePrimitive(root.transform, PrimitiveType.Cube, "SafetyVest", new Vector3(0f, 1.02f, 0.19f), new Vector3(0.55f, 0.55f, 0.055f), vest);
            CreatePrimitive(root.transform, PrimitiveType.Sphere, "Head", new Vector3(0f, 1.72f, 0f), new Vector3(0.34f, 0.34f, 0.34f), skin);
            CreatePrimitive(root.transform, PrimitiveType.Cylinder, "Helmet", new Vector3(0f, 1.91f, 0f), new Vector3(0.23f, 0.10f, 0.23f), helmet);
            CreatePrimitive(root.transform, PrimitiveType.Cylinder, "LeftLeg", new Vector3(-0.16f, 0.36f, 0f), new Vector3(0.11f, 0.36f, 0.11f), suit);
            CreatePrimitive(root.transform, PrimitiveType.Cylinder, "RightLeg", new Vector3(0.16f, 0.36f, 0f), new Vector3(0.11f, 0.36f, 0.11f), suit);
        }

        private void CreateManualRepairPanel()
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "GeneratorManualRepairPanel";
            panel.transform.SetParent(transform, true);
            panel.transform.position = new Vector3(-3.58f, 1.45f, -1.57f);
            panel.transform.localScale = new Vector3(0.92f, 0.54f, 0.085f);
            panel.GetComponent<Renderer>().sharedMaterial = RuntimeMaterial(
                "RepairPanel",
                new Color(0.18f, 0.035f, 0.025f),
                0.34f,
                0.54f,
                new Color(1.7f, 0.08f, 0.025f));

            GeneratorRepairInteractable repair = panel.AddComponent<GeneratorRepairInteractable>();
            repair.Configure(trainState);
        }

        private void CreateAlarmAudio()
        {
            alarmSource = gameObject.AddComponent<AudioSource>();
            alarmSource.playOnAwake = false;
            alarmSource.loop = false;
            alarmSource.spatialBlend = 0f;
            alarmSource.volume = 0.28f;

            const int sampleRate = 22050;
            const float duration = 0.34f;
            int count = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / duration));
                float pulse = Mathf.Sin(2f * Mathf.PI * 820f * t) * 0.55f;
                float high = Mathf.Sin(2f * Mathf.PI * 1230f * t) * 0.18f;
                data[i] = (pulse + high) * envelope * 0.38f;
            }

            alarmClip = AudioClip.Create("GeneratorEmergencyAlarm", count, 1, sampleRate, false);
            alarmClip.SetData(data, 0);
        }

        private void OnGUI()
        {
            if (trainState == null)
            {
                return;
            }

            EnsureGui();

            string title = string.Empty;
            string detail = string.Empty;
            Texture2D background = dangerTexture;

            switch (trainState.CurrentGeneratorFaultPhase)
            {
                case GeneratorFaultPhase.Alarm:
                    title = "JENERATÖR ARIZASI";
                    detail = "Acil güç devrede  •  TAB ile yönetim panelini aç ve mühendis görevlendir";
                    break;
                case GeneratorFaultPhase.EngineerEnRoute:
                    title = "MÜHENDİS GÖREVLENDİRİLDİ";
                    detail = trainState.AssignedEngineerName + " jeneratör vagonuna ilerliyor";
                    background = warningTexture;
                    break;
                case GeneratorFaultPhase.ManualIntervention:
                    title = "MANUEL MÜDAHALE GEREKLİ";
                    detail = "Jeneratör vagonundaki kırmızı bakım paneline git ve E ile onarımı tamamla";
                    break;
                case GeneratorFaultPhase.Normal:
                    if (repairedBannerTimer <= 0f)
                    {
                        return;
                    }
                    title = "JENERATÖR TEKRAR ÇEVRİMİÇİ";
                    detail = "Ana enerji üretimi kararlı  •  Arıza giderildi";
                    background = goodTexture;
                    break;
            }

            float width = Mathf.Min(620f, Screen.width - 40f);
            Rect box = new Rect((Screen.width - width) * 0.5f, 24f, width, 66f);
            GUI.DrawTexture(box, background);
            GUI.Label(new Rect(box.x + 18f, box.y + 8f, box.width - 36f, 26f), title, alertStyle);
            GUI.Label(new Rect(box.x + 18f, box.y + 34f, box.width - 36f, 22f), detail, alertSmallStyle);
        }

        private void EnsureGui()
        {
            if (alertStyle != null)
            {
                return;
            }

            dangerTexture = SolidTexture(new Color(0.42f, 0.035f, 0.025f, 0.96f));
            warningTexture = SolidTexture(new Color(0.34f, 0.16f, 0.025f, 0.96f));
            goodTexture = SolidTexture(new Color(0.035f, 0.30f, 0.23f, 0.96f));

            alertStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            alertSmallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.92f, 0.94f, 0.96f) }
            };
        }

        private static GameObject CreatePrimitive(Transform parent, PrimitiveType type, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
            obj.transform.localScale = localScale;
            obj.GetComponent<Renderer>().sharedMaterial = material;

            Collider collider = obj.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }

            return obj;
        }

        private static Material RuntimeMaterial(string name, Color color, float smoothness, float metallic, Color? emission = null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Material material = new Material(shader) { name = name };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (emission.HasValue && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
            }
            return material;
        }

        private static Texture2D SolidTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
