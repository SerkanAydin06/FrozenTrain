using FrozenTrain.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FrozenTrain.Management
{
    /// <summary>
    /// First portable train-management UI. Tab toggles it from anywhere in FPS mode.
    /// The game slows down while the panel is open and FPS input is temporarily disabled.
    /// </summary>
    public sealed class TrainManagementPanel : MonoBehaviour
    {
        [SerializeField] private TrainPrototypeState trainState;
        [SerializeField] private FpsPlayerController playerController;
        [SerializeField, Range(0.05f, 1f)] private float openTimeScale = 0.2f;

        private bool isOpen;
        private float previousTimeScale = 1f;
        private GUIStyle titleStyle;
        private GUIStyle headerStyle;
        private GUIStyle bodyStyle;
        private GUIStyle statusStyle;

        public bool IsOpen => isOpen;

        public void Configure(TrainPrototypeState state, FpsPlayerController controller)
        {
            trainState = state;
            playerController = controller;
        }

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = GetComponent<FpsPlayerController>();
            }
        }

        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                if (isOpen)
                {
                    ClosePanel();
                }
                else
                {
                    OpenPanel();
                }
            }
        }

        public void OpenPanel()
        {
            if (isOpen)
            {
                return;
            }

            if (trainState == null)
            {
                trainState = FindFirstObjectByType<TrainPrototypeState>();
            }

            if (playerController == null)
            {
                playerController = GetComponent<FpsPlayerController>();
            }

            previousTimeScale = Time.timeScale;
            Time.timeScale = openTimeScale;

            if (playerController != null)
            {
                playerController.SetCursorCaptured(false);
                playerController.enabled = false;
            }

            isOpen = true;
        }

        public void ClosePanel()
        {
            if (!isOpen)
            {
                return;
            }

            Time.timeScale = previousTimeScale;

            if (playerController != null)
            {
                playerController.enabled = true;
                playerController.SetCursorCaptured(true);
            }

            isOpen = false;
        }

        private void OnDisable()
        {
            if (isOpen)
            {
                Time.timeScale = previousTimeScale;
                isOpen = false;
            }
        }

        private void OnGUI()
        {
            if (!isOpen)
            {
                return;
            }

            EnsureStyles();

            Color oldColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = oldColor;

            float panelWidth = Mathf.Min(980f, Screen.width - 80f);
            float panelHeight = Mathf.Min(650f, Screen.height - 80f);
            Rect panelRect = new Rect(
                (Screen.width - panelWidth) * 0.5f,
                (Screen.height - panelHeight) * 0.5f,
                panelWidth,
                panelHeight);

            GUI.Box(panelRect, GUIContent.none);

            GUILayout.BeginArea(new Rect(panelRect.x + 24f, panelRect.y + 20f, panelRect.width - 48f, panelRect.height - 40f));

            GUILayout.BeginHorizontal();
            GUILayout.Label("FROZENTRAIN // TAŞINABİLİR YÖNETİM", titleStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Kapat  [TAB]", GUILayout.Width(130f), GUILayout.Height(32f)))
            {
                ClosePanel();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(12f);

            if (trainState == null)
            {
                GUILayout.Label("Tren durumu bulunamadı.", bodyStyle);
                GUILayout.EndArea();
                return;
            }

            DrawResourceStrip();
            GUILayout.Space(18f);

            GUILayout.Label("VAGONLAR", headerStyle);
            GUILayout.Space(6f);

            DrawLocomotiveRow();
            GUILayout.Space(8f);
            DrawGeneratorRow();
            GUILayout.Space(8f);
            DrawLivingRow();

            GUILayout.FlexibleSpace();
            GUILayout.Label("Panel açıkken oyun %20 hızında devam eder. Bu ekran daha sonra rota, personel, üretim ve acil durum yönetimini de içerecek.", bodyStyle);

            GUILayout.EndArea();
        }

        private void DrawResourceStrip()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("SİSTEM ÖZETİ", headerStyle);

            GUILayout.BeginHorizontal();
            DrawMetric("ENERJİ", trainState.EnergyConsumption + " / " + trainState.EnergyCapacity);
            DrawMetric("ISI", "%" + trainState.HeatPercent);
            DrawMetric("YİYECEK", trainState.Food.ToString());
            DrawMetric("SU", trainState.Water.ToString());
            DrawMetric("NÜFUS", trainState.Population.ToString());
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawMetric(string label, string value)
        {
            GUILayout.BeginVertical(GUILayout.MinWidth(120f));
            GUILayout.Label(label, statusStyle);
            GUILayout.Label(value, headerStyle);
            GUILayout.EndVertical();
        }

        private void DrawLocomotiveRow()
        {
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(78f));
            GUILayout.BeginVertical();
            GUILayout.Label("01  LOKOMOTİF", headerStyle);
            GUILayout.Label("Çekiş, yön ve ana kumanda", bodyStyle);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("ZORUNLU // ÇALIŞIYOR", statusStyle, GUILayout.Width(190f));
            GUILayout.EndHorizontal();
        }

        private void DrawGeneratorRow()
        {
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(78f));
            GUILayout.BeginVertical();
            GUILayout.Label("02  JENERATÖR", headerStyle);
            GUILayout.Label(trainState.GeneratorOnline ? "Ana enerji üretimi çevrimiçi" : "Ana üretim kapalı — acil güç aktif", bodyStyle);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            string buttonText = trainState.GeneratorOnline ? "Jeneratörü Kapat" : "Jeneratörü Aç";
            if (GUILayout.Button(buttonText, GUILayout.Width(170f), GUILayout.Height(38f)))
            {
                trainState.ToggleGenerator();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawLivingRow()
        {
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(78f));
            GUILayout.BeginVertical();
            GUILayout.Label("03  YAŞAM VAGONU", headerStyle);
            GUILayout.Label(trainState.LivingCarPowered ? "Isıtma ve yaşam sistemleri besleniyor" : "Güç kesildi — tüketim azaltıldı", bodyStyle);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            string buttonText = trainState.LivingCarPowered ? "Gücü Kes" : "Gücü Ver";
            if (GUILayout.Button(buttonText, GUILayout.Width(170f), GUILayout.Height(38f)))
            {
                trainState.ToggleLivingCarPower();
            }

            GUILayout.EndHorizontal();
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.86f, 0.93f, 1f) }
            };

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                wordWrap = true,
                normal = { textColor = new Color(0.78f, 0.82f, 0.86f) }
            };

            statusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.67f, 0.84f, 1f) }
            };
        }
    }
}
