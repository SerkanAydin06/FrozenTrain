using FrozenTrain.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FrozenTrain.Management
{
    public sealed class TrainManagementPanel : MonoBehaviour
    {
        [SerializeField] private TrainPrototypeState trainState;
        [SerializeField] private FpsPlayerController playerController;
        [SerializeField, Range(0.05f, 1f)] private float openTimeScale = 0.2f;

        private bool isOpen;
        private float previousTimeScale = 1f;
        private Vector2 scroll;

        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle sectionTitleStyle;
        private GUIStyle metricLabelStyle;
        private GUIStyle metricValueStyle;
        private GUIStyle rowTitleStyle;
        private GUIStyle rowBodyStyle;
        private GUIStyle statusStyle;
        private GUIStyle buttonStyle;
        private GUIStyle smallButtonStyle;
        private GUIStyle panelStyle;
        private GUIStyle sectionStyle;
        private GUIStyle rowStyle;
        private GUIStyle warningStyle;

        private Texture2D backdropTexture;
        private Texture2D panelTexture;
        private Texture2D sectionTexture;
        private Texture2D rowTexture;
        private Texture2D accentTexture;
        private Texture2D warningTexture;
        private Texture2D dangerTexture;
        private Texture2D goodTexture;
        private Texture2D mutedTexture;
        private Texture2D buttonTexture;
        private Texture2D buttonHoverTexture;

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
                if (isOpen) ClosePanel();
                else OpenPanel();
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
            if (!isOpen)
            {
                return;
            }

            Time.timeScale = previousTimeScale;
            isOpen = false;
        }

        private void OnGUI()
        {
            if (!isOpen)
            {
                return;
            }

            EnsureStyles();

            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), backdropTexture);

            float width = Mathf.Min(1120f, Screen.width - 48f);
            float height = Mathf.Min(820f, Screen.height - 36f);
            Rect panelRect = new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f,
                width,
                height);

            GUI.Box(panelRect, GUIContent.none, panelStyle);
            GUI.DrawTexture(new Rect(panelRect.x, panelRect.y, 5f, panelRect.height), accentTexture);

            GUILayout.BeginArea(new Rect(panelRect.x + 26f, panelRect.y + 20f, panelRect.width - 52f, panelRect.height - 40f));

            DrawHeader();

            if (trainState == null)
            {
                GUILayout.Space(28f);
                GUILayout.Label("Tren durum sistemi bulunamadı.", rowBodyStyle);
                GUILayout.EndArea();
                return;
            }

            GUILayout.Space(14f);
            DrawEnergySection();
            GUILayout.Space(12f);
            DrawResourceStrip();
            GUILayout.Space(14f);

            GUILayout.Label("VAGON KONTROLÜ", sectionTitleStyle);
            GUILayout.Space(7f);

            scroll = GUILayout.BeginScrollView(scroll, false, true);
            DrawFixedRow("01", "LOKOMOTİF", "Çekiş, rota ve ana kumanda", "ÇALIŞIYOR", true);
            DrawToggleRow("02", "JENERATÖR", trainState.GeneratorOnline ? "Ana enerji üretimi çevrimiçi" : "Ana üretim kapalı — acil güç kullanılıyor", trainState.GeneratorOnline, "KAPAT", "DEVREYE AL", trainState.ToggleGenerator);
            DrawToggleRow("03", "YAŞAM", trainState.LivingCarPowered ? "Isıtma ve yaşam sistemleri besleniyor" : "Güç kesildi — sıcaklık ve moral riski", trainState.LivingCarPowered, "GÜCÜ KES", "GÜÇ VER", trainState.ToggleLivingCarPower);
            DrawToggleRow("04", "ATÖLYE", trainState.WorkshopPowered ? "Üretim ve tamir tezgâhları çalışıyor" : "Atölye bekleme modunda", trainState.WorkshopPowered, "KAPAT", "ÇALIŞTIR", trainState.ToggleWorkshopPower);
            DrawToggleRow("05", "SERA", trainState.GreenhousePowered ? "Yetiştirme ışıkları ve pompalar aktif" : "Yiyecek üretimi durduruldu", trainState.GreenhousePowered, "KAPAT", "ÇALIŞTIR", trainState.ToggleGreenhousePower);
            GUILayout.EndScrollView();

            GUILayout.Space(8f);
            GUILayout.Label("TAB ile kapat  •  Yönetim açıkken simülasyon %20 hızında çalışır", subtitleStyle);
            GUILayout.EndArea();
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("FROZENTRAIN // TREN YÖNETİMİ", titleStyle);
            GUILayout.Label("TAŞINABİLİR KOMUTA ARAYÜZÜ  •  YEREL BAĞLANTI", subtitleStyle);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(GUILayout.Width(150f));
            GUILayout.Label("SİSTEM", metricLabelStyle);
            GUILayout.Label(trainState != null && trainState.EnergyDeficit > 0 ? "UYARI" : "ONLINE", trainState != null && trainState.EnergyDeficit > 0 ? statusStyle : metricValueStyle);
            GUILayout.EndVertical();

            if (GUILayout.Button("KAPAT  [TAB]", smallButtonStyle, GUILayout.Width(138f), GUILayout.Height(36f)))
            {
                ClosePanel();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(12f);
            Rect line = GUILayoutUtility.GetRect(1f, 2f, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(line, accentTexture);
        }

        private void DrawEnergySection()
        {
            GUILayout.BeginVertical(sectionStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("GÜÇ ŞEBEKESİ", sectionTitleStyle);
            GUILayout.FlexibleSpace();
            string energyText = trainState.EnergyConsumption + " / " + trainState.EnergyCapacity + " BİRİM";
            GUILayout.Label(energyText, metricValueStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);
            Rect barRect = GUILayoutUtility.GetRect(1f, 16f, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(barRect, mutedTexture);

            float ratio = trainState.EnergyCapacity > 0
                ? trainState.EnergyConsumption / (float)trainState.EnergyCapacity
                : 1f;
            float fill = Mathf.Clamp01(ratio);
            Texture2D fillTexture = ratio > 1f ? dangerTexture : ratio > 0.82f ? warningTexture : goodTexture;
            GUI.DrawTexture(new Rect(barRect.x, barRect.y, barRect.width * fill, barRect.height), fillTexture);

            if (trainState.EnergyDeficit > 0)
            {
                GUILayout.Space(7f);
                GUILayout.Label(
                    "ENERJİ AÇIĞI  //  " + trainState.EnergyDeficit + " birim fazla tüketim. Bir alt sistemi kapat veya ana jeneratörü devreye al.",
                    warningStyle);
            }
            else
            {
                int reserve = Mathf.Max(0, trainState.EnergyCapacity - trainState.EnergyConsumption);
                GUILayout.Space(5f);
                GUILayout.Label("Şebeke kararlı  •  Yedek kapasite: " + reserve + " birim", subtitleStyle);
            }

            GUILayout.EndVertical();
        }

        private void DrawResourceStrip()
        {
            GUILayout.BeginHorizontal();
            DrawMetric("ISI", "%" + trainState.HeatPercent);
            DrawMetric("YİYECEK", trainState.Food.ToString());
            DrawMetric("SU", trainState.Water.ToString());
            DrawMetric("MALZEME", trainState.Materials.ToString());
            DrawMetric("İLAÇ", trainState.Medicine.ToString());
            DrawMetric("NÜFUS", trainState.Population.ToString());
            GUILayout.EndHorizontal();
        }

        private void DrawMetric(string label, string value)
        {
            GUILayout.BeginVertical(sectionStyle, GUILayout.MinWidth(105f), GUILayout.Height(66f));
            GUILayout.Label(label, metricLabelStyle);
            GUILayout.Label(value, metricValueStyle);
            GUILayout.EndVertical();
            GUILayout.Space(6f);
        }

        private void DrawFixedRow(string number, string title, string description, string status, bool active)
        {
            GUILayout.BeginHorizontal(rowStyle, GUILayout.Height(76f));
            DrawRowIdentity(number, title, description);
            GUILayout.FlexibleSpace();
            GUILayout.Label(status, active ? metricValueStyle : statusStyle, GUILayout.Width(120f));
            GUILayout.Space(8f);
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
        }

        private void DrawToggleRow(
            string number,
            string title,
            string description,
            bool active,
            string activeText,
            string inactiveText,
            System.Action toggle)
        {
            GUILayout.BeginHorizontal(rowStyle, GUILayout.Height(76f));
            DrawRowIdentity(number, title, description);
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical(GUILayout.Width(88f));
            GUILayout.Label("DURUM", metricLabelStyle);
            GUILayout.Label(active ? "AKTİF" : "KAPALI", active ? metricValueStyle : statusStyle);
            GUILayout.EndVertical();

            if (GUILayout.Button(active ? activeText : inactiveText, buttonStyle, GUILayout.Width(142f), GUILayout.Height(38f)))
            {
                toggle?.Invoke();
            }

            GUILayout.Space(4f);
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
        }

        private void DrawRowIdentity(string number, string title, string description)
        {
            GUILayout.Label(number, statusStyle, GUILayout.Width(38f));
            GUILayout.BeginVertical();
            GUILayout.Label(title, rowTitleStyle);
            GUILayout.Label(description, rowBodyStyle);
            GUILayout.EndVertical();
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            backdropTexture = SolidTexture(new Color(0.006f, 0.010f, 0.014f, 0.86f));
            panelTexture = SolidTexture(new Color(0.026f, 0.035f, 0.044f, 0.98f));
            sectionTexture = SolidTexture(new Color(0.042f, 0.054f, 0.064f, 0.98f));
            rowTexture = SolidTexture(new Color(0.034f, 0.044f, 0.052f, 0.98f));
            accentTexture = SolidTexture(new Color(0.78f, 0.25f, 0.055f, 1f));
            warningTexture = SolidTexture(new Color(0.95f, 0.48f, 0.08f, 1f));
            dangerTexture = SolidTexture(new Color(0.82f, 0.13f, 0.09f, 1f));
            goodTexture = SolidTexture(new Color(0.18f, 0.60f, 0.52f, 1f));
            mutedTexture = SolidTexture(new Color(0.10f, 0.13f, 0.15f, 1f));
            buttonTexture = SolidTexture(new Color(0.18f, 0.22f, 0.25f, 1f));
            buttonHoverTexture = SolidTexture(new Color(0.28f, 0.32f, 0.35f, 1f));

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = panelTexture }
            };

            sectionStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(14, 14, 10, 10),
                margin = new RectOffset(0, 0, 0, 0),
                normal = { background = sectionTexture }
            };

            rowStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(14, 12, 10, 10),
                normal = { background = rowTexture }
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 23,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.94f, 0.97f, 1f) }
            };

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.46f, 0.56f, 0.62f) }
            };

            sectionTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.70f, 0.78f, 0.82f) }
            };

            metricLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.46f, 0.56f, 0.62f) }
            };

            metricValueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.84f, 0.93f, 0.95f) }
            };

            rowTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.90f, 0.94f, 0.97f) }
            };

            rowBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                normal = { textColor = new Color(0.57f, 0.65f, 0.69f) }
            };

            statusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.92f, 0.42f, 0.16f) }
            };

            warningStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(1f, 0.58f, 0.22f) }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { background = buttonTexture, textColor = new Color(0.90f, 0.94f, 0.96f) },
                hover = { background = buttonHoverTexture, textColor = Color.white },
                active = { background = accentTexture, textColor = Color.white }
            };

            smallButtonStyle = new GUIStyle(buttonStyle)
            {
                fontSize = 10
            };
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
