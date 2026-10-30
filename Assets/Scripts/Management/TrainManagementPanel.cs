using FrozenTrain.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace FrozenTrain.Management
{
    public sealed class TrainManagementPanel : MonoBehaviour
    {
        [SerializeField] private TrainPrototypeState trainState;
        [SerializeField] private FpsPlayerController playerController;
        [SerializeField, Range(0.05f, 1f)] private float openTimeScale = 0.2f;

        private bool isOpen;
        private bool gameOverShown;
        private float previousTimeScale = 1f;
        private Vector2 scroll;
        private string saveFeedback;
        private float saveFeedbackExpiry;
        private bool compactLayout;
        private float contentWidth;

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
            if (playerController == null) playerController = GetComponent<FpsPlayerController>();
        }

        private void Update()
        {
            if (trainState == null) trainState = FindFirstObjectByType<TrainPrototypeState>();
            Keyboard keyboard = Keyboard.current;

            if (trainState != null && trainState.IsLost)
            {
                if (keyboard != null && keyboard.f9Key.wasPressedThisFrame) LoadCurrentRun();
                if (trainState.IsLost)
                {
                    if (!gameOverShown) ShowGameOver();
                    if (keyboard != null && keyboard.rKey.wasPressedThisFrame) RestartRun();
                }
                return;
            }

            if (keyboard == null) return;
            if (keyboard.f5Key.wasPressedThisFrame) SaveCurrentRun();
            if (keyboard.f9Key.wasPressedThisFrame) LoadCurrentRun();
            if (keyboard.tabKey.wasPressedThisFrame)
            {
                if (isOpen) ClosePanel();
                else OpenPanel();
            }
            else if (isOpen && keyboard.escapeKey.wasPressedThisFrame)
            {
                ClosePanel();
            }
        }

        private void SaveCurrentRun()
        {
            bool saved = trainState != null && trainState.SaveRun(transform);
            saveFeedback = saved ? "Oyun kaydedildi." : "Oyun kaydedilemedi.";
            saveFeedbackExpiry = Time.unscaledTime + 4f;
        }

        private void LoadCurrentRun()
        {
            bool loaded = trainState != null && trainState.LoadRun(transform);
            saveFeedback = loaded ? "Kayıt yüklendi." : "Geçerli kayıt bulunamadı.";
            saveFeedbackExpiry = Time.unscaledTime + 4f;
            if (loaded && gameOverShown)
            {
                gameOverShown = false;
                Time.timeScale = 1f;
                if (playerController != null)
                {
                    playerController.enabled = true;
                    playerController.SetCursorCaptured(true);
                }
            }
        }

        private void ShowGameOver()
        {
            ClosePanel();
            gameOverShown = true;
            Time.timeScale = 0f;
            if (playerController != null)
            {
                playerController.SetCursorCaptured(false);
                playerController.enabled = false;
            }
        }

        private void RestartRun()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void OpenPanel()
        {
            if (isOpen || gameOverShown) return;
            if (trainState == null) trainState = FindFirstObjectByType<TrainPrototypeState>();
            if (playerController == null) playerController = GetComponent<FpsPlayerController>();

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
            if (!isOpen) return;
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
            if (!isOpen) return;
            Time.timeScale = previousTimeScale;
            if (playerController != null) playerController.enabled = true;
            isOpen = false;
        }

        private void OnDestroy()
        {
            if (gameOverShown) Time.timeScale = 1f;
            Texture2D[] textures = { backdropTexture, panelTexture, sectionTexture, rowTexture, accentTexture, warningTexture, dangerTexture, goodTexture, mutedTexture, buttonTexture, buttonHoverTexture };
            foreach (Texture2D texture in textures)
            {
                if (texture != null) Destroy(texture);
            }
        }

        private void OnGUI()
        {
            if (gameOverShown)
            {
                EnsureStyles();
                DrawGameOver();
                return;
            }

            if (!isOpen) return;
            EnsureStyles();

            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), backdropTexture);
            float width = Mathf.Min(1180f, Screen.width - 24f);
            float height = Mathf.Min(900f, Screen.height - 20f);
            if (width <= 100f || height <= 120f) return;

            Rect panelRect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            compactLayout = width < 900f;
            contentWidth = width - 72f; // account for panel padding and scrollbar

            GUI.Box(panelRect, GUIContent.none, panelStyle);
            GUI.DrawTexture(new Rect(panelRect.x, panelRect.y, 5f, panelRect.height), accentTexture);

            GUILayout.BeginArea(new Rect(panelRect.x + 26f, panelRect.y + 20f, panelRect.width - 52f, panelRect.height - 40f));
            DrawHeader();

            if (trainState == null)
            {
                GUILayout.Space(24f);
                GUILayout.Label("Tren durum sistemi bulunamadı.", rowBodyStyle);
                GUILayout.EndArea();
                return;
            }

            GUILayout.Space(8f);
            // Every section scrolls together, not only the list of cars.
            scroll = GUILayout.BeginScrollView(scroll, false, true);
            GUILayout.Space(8f);
            DrawSaveControls();
            GUILayout.Space(10f);
            DrawEnergySection();
            GUILayout.Space(10f);
            DrawResourceStrip();
            GUILayout.Space(8f);
            DrawTrainStatusStrip();
            GUILayout.Space(10f);
            DrawWarnings();
            GUILayout.Space(10f);
            GUILayout.Label("VAGON SİSTEMLERİ", sectionTitleStyle);
            GUILayout.Space(6f);
            DrawLocomotiveRow();
            DrawToggleRow("02", "JENERATÖR",
                trainState.GeneratorOnline ? (trainState.SnowIntakeActive ? "Ana üretim aktif; atık ısı kar eritip yedek su tankını dolduruyor." : "Ana elektrik üretimi aktif; yakıt tüketiyor.") : "Ana üretim kapalı; yalnızca acil güç mevcut.",
                trainState.GeneratorOnline, "KAPAT", "DEVREYE AL", trainState.ToggleGenerator,
                trainState.GeneratorOnline ? "ÜRETİYOR" : "KAPALI");
            DrawLivingRow();
            DrawToggleRow("04", "ATÖLYE",
                trainState.WorkshopActuallyRepairing ? "Malzeme harcayarak tren kondisyonunu onarıyor." : trainState.WorkshopActuallyRunning ? "Atölye hazır; şu an onarım gerekmiyor." : "Atölye kapalı veya güç yetersiz.",
                trainState.WorkshopPowered, "KAPAT", "ÇALIŞTIR", trainState.ToggleWorkshopPower,
                trainState.WorkshopActuallyRepairing ? "ONARIYOR" : trainState.WorkshopActuallyRunning ? "HAZIR" : "BEKLEME");
            DrawToggleRow("05", "SERA",
                trainState.GreenhouseActuallyRunning ? "Su tüketerek yiyecek üretiyor." : "Üretim yok: güç, su veya sistem kapalı.",
                trainState.GreenhousePowered, "KAPAT", "ÇALIŞTIR", trainState.ToggleGreenhousePower,
                trainState.GreenhouseActuallyRunning ? "ÜRETİYOR" : "DURDU");
            GUILayout.EndScrollView();
            GUILayout.Space(6f);
            GUILayout.Label("TAB / ESC ile kapat  •  Panel açıkken simülasyon %20 hızında devam eder", subtitleStyle);
            GUILayout.EndArea();
        }

        private void DrawGameOver()
        {
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), backdropTexture);
            float width = Mathf.Min(620f, Screen.width - 42f);
            Rect rect = new Rect((Screen.width - width) * 0.5f, Screen.height * 0.5f - 130f, width, 260f);
            GUI.Box(rect, GUIContent.none, panelStyle);
            GUI.DrawTexture(new Rect(rect.x, rect.y, 5f, rect.height), dangerTexture);

            GUILayout.BeginArea(new Rect(rect.x + 28f, rect.y + 22f, rect.width - 56f, rect.height - 44f));
            GUILayout.Label("TREN SESSİZLEŞTİ", titleStyle);
            GUILayout.Space(6f);
            GUILayout.Label("Son yolcu da dondurucu soğuğa yenik düştü.", rowBodyStyle);
            GUILayout.Label("Kat edilen mesafe: " + trainState.DistanceKm.ToString("0.0") + " km", metricValueStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("KAYDI YÜKLE  [F9]", buttonStyle, GUILayout.Height(38f))) LoadCurrentRun();
            if (GUILayout.Button("YENİDEN BAŞLA  [R]", buttonStyle, GUILayout.Height(38f))) RestartRun();
            GUILayout.EndArea();
        }

        private void DrawHeader()
        {
            string travel = trainState == null ? "5 VAGON" : trainState.SpeedKmh + " km/h  •  " + trainState.DistanceKm.ToString("0.0") + " km  •  " + trainState.PaceLabel;

            if (compactLayout)
            {
                GUILayout.Label("FROZENTRAIN // KONTROL", titleStyle);
                GUILayout.Label(travel, subtitleStyle);
                GUILayout.BeginHorizontal();
                if (trainState != null)
                {
                    bool warning = trainState.SurvivalWarningCount > 0;
                    GUILayout.Label(warning ? trainState.SurvivalWarningCount + " UYARI" : "SİSTEM KARARLI",
                        warning ? statusStyle : metricValueStyle);
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("KAPAT [TAB]", smallButtonStyle, GUILayout.Width(112f), GUILayout.Height(34f))) ClosePanel();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label("FROZENTRAIN // SURVIVAL CONTROL", titleStyle);
                GUILayout.Label("GÜÇ DAĞITIMI  •  KAYNAK AKIŞI  •  " + travel, subtitleStyle);
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();
                if (trainState != null)
                {
                    GUILayout.BeginVertical(GUILayout.Width(160f));
                    GUILayout.Label("SİSTEM DURUMU", metricLabelStyle);
                    bool warning = trainState.SurvivalWarningCount > 0;
                    GUILayout.Label(warning ? trainState.SurvivalWarningCount + " UYARI" : "KARARLI",
                        warning ? statusStyle : metricValueStyle);
                    GUILayout.EndVertical();
                }
                if (GUILayout.Button("KAPAT  [TAB]", smallButtonStyle, GUILayout.Width(138f), GUILayout.Height(36f))) ClosePanel();
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(11f);
            Rect line = GUILayoutUtility.GetRect(1f, 2f, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(line, accentTexture);
        }

        private void DrawSaveControls()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("KAYDET  [F5]", buttonStyle, GUILayout.Height(34f))) SaveCurrentRun();
            if (GUILayout.Button("YÜKLE  [F9]", buttonStyle, GUILayout.Height(34f))) LoadCurrentRun();
            GUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(saveFeedback) && Time.unscaledTime < saveFeedbackExpiry)
                GUILayout.Label(saveFeedback, rowBodyStyle);
        }

        private void DrawEnergySection()
        {
            GUILayout.BeginVertical(sectionStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("GÜÇ ŞEBEKESİ", sectionTitleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(trainState.EnergyConsumption + " / " + trainState.EnergyCapacity + " kW", metricValueStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);
            Rect barRect = GUILayoutUtility.GetRect(1f, 16f, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(barRect, mutedTexture);
            float ratio = trainState.EnergyCapacity > 0 ? trainState.EnergyConsumption / (float)trainState.EnergyCapacity : 1f;
            Texture2D fillTexture = ratio > 1f ? dangerTexture : ratio > 0.85f ? warningTexture : goodTexture;
            GUI.DrawTexture(new Rect(barRect.x, barRect.y, barRect.width * Mathf.Clamp01(ratio), barRect.height), fillTexture);

            GUILayout.Space(6f);
            if (trainState.EnergyDeficit > 0)
                GUILayout.Label("GÜÇ AÇIĞI: " + trainState.EnergyDeficit + " kW  •  Alt sistemler verimsizleşiyor. Öncelik belirle.", warningStyle);
            else
                GUILayout.Label("Şebeke kararlı  •  Yedek kapasite: " + Mathf.Max(0, trainState.EnergyCapacity - trainState.EnergyConsumption) + " kW  •  Verim: %" + Mathf.RoundToInt(trainState.PowerEfficiency * 100f), subtitleStyle);
            GUILayout.EndVertical();
        }

        private void DrawResourceStrip()
        {
            int columns = contentWidth < 550f ? 2 : contentWidth < 870f ? 3 : contentWidth < 1090f ? 4 : 7;
            for (int first = 0; first < 7; first += columns)
            {
                GUILayout.BeginHorizontal();
                for (int i = first; i < Mathf.Min(7, first + columns); i++)
                {
                    switch (i)
                    {
                        case 0: DrawMetric("YAKIT", trainState.Fuel.ToString(), trainState.FuelDeltaPerMinute); break;
                        case 1: DrawMetric("ISI", "%" + trainState.HeatPercent, trainState.HeatDeltaPerMinute); break;
                        case 2: DrawMetric("YİYECEK", trainState.Food.ToString(), trainState.FoodDeltaPerMinute); break;
                        case 3: DrawMetric("SU", trainState.Water.ToString(), trainState.WaterDeltaPerMinute); break;
                        case 4: DrawMetric("YEDEK SU", trainState.WaterReserve.ToString(), trainState.WaterReserveDeltaPerMinute); break;
                        case 5: DrawMetric("MALZEME", trainState.Materials.ToString(), trainState.MaterialsDeltaPerMinute); break;
                        case 6: DrawMetric("İLAÇ", trainState.Medicine.ToString(), 0f); break;
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawTrainStatusStrip()
        {
            int columns = contentWidth < 550f ? 2 : contentWidth < 870f ? 3 : 5;
            for (int first = 0; first < 5; first += columns)
            {
                GUILayout.BeginHorizontal();
                for (int i = first; i < Mathf.Min(5, first + columns); i++)
                {
                    switch (i)
                    {
                        case 0: DrawStaticMetric("HIZ", trainState.SpeedKmh + " km/h"); break;
                        case 1: DrawStaticMetric("MESAFE", trainState.DistanceKm.ToString("0.0") + " km"); break;
                        case 2: DrawStaticMetric("TREN KONDİSYONU", "%" + trainState.TrainCondition); break;
                        case 3: DrawStaticMetric("NÜFUS SAĞLIĞI", "%" + trainState.PopulationHealth); break;
                        case 4: DrawStaticMetric("NÜFUS", trainState.Population.ToString()); break;
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawMetric(string label, string value, float deltaPerMinute)
        {
            GUILayout.BeginVertical(sectionStyle, GUILayout.MinWidth(115f), GUILayout.Height(76f));
            GUILayout.Label(label, metricLabelStyle);
            GUILayout.Label(value, metricValueStyle);
            string delta = Mathf.Abs(deltaPerMinute) < 0.005f ? "sabit" : (deltaPerMinute > 0f ? "+" : string.Empty) + deltaPerMinute.ToString("0.0") + "/dk";
            GUILayout.Label(delta, deltaPerMinute < -0.01f ? statusStyle : subtitleStyle);
            GUILayout.EndVertical();
            GUILayout.Space(6f);
        }

        private void DrawStaticMetric(string label, string value)
        {
            GUILayout.BeginVertical(sectionStyle, GUILayout.MinWidth(130f), GUILayout.Height(62f));
            GUILayout.Label(label, metricLabelStyle);
            GUILayout.Label(value, metricValueStyle);
            GUILayout.EndVertical();
            GUILayout.Space(6f);
        }

        private void DrawWarnings()
        {
            if (trainState.SurvivalWarningCount == 0) return;
            GUILayout.BeginVertical(sectionStyle);
            GUILayout.Label("SURVIVAL UYARILARI", warningStyle);
            if (!trainState.PowerStable) GUILayout.Label("• Güç tüketimi kapasitenin üzerinde.", rowBodyStyle);
            if (trainState.Fuel <= 20) GUILayout.Label("• Yakıt kritik seviyeye yaklaşıyor.", rowBodyStyle);
            if (trainState.Food <= 20) GUILayout.Label("• Yiyecek stoğu düşük.", rowBodyStyle);
            if (trainState.Water <= 20)
                GUILayout.Label(trainState.WaterReserve > 0 ? "• Su stoğu düşük; yedek tanktan aktar." : "• Su stoğu düşük ve yedek tank boş.", rowBodyStyle);
            if (trainState.HeatPercent <= 35) GUILayout.Label("• Tren iç sıcaklığı tehlikeli seviyede.", rowBodyStyle);
            if (trainState.PopulationHealth <= 50) GUILayout.Label("• Nüfus sağlığı düşüyor.", rowBodyStyle);
            if (trainState.TrainCondition <= 35) GUILayout.Label("• Tren kondisyonu kritik; atölyeyi çalıştır.", rowBodyStyle);
            if (!trainState.CanMove) GUILayout.Label("• Tren duruyor; kar toplama ve ilerleme durdu.", rowBodyStyle);
            if (trainState.PopulationHealth <= 0) GUILayout.Label("• Nüfus sağlığı çöktü; yolcular kaybediliyor!", warningStyle);
            GUILayout.EndVertical();
        }

        private void BeginSystemRow(string number, string title, string description, float height)
        {
            if (compactLayout)
            {
                GUILayout.BeginVertical(rowStyle);
                GUILayout.BeginHorizontal();
                DrawRowIdentity(number, title, description);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal(rowStyle, GUILayout.Height(height));
                DrawRowIdentity(number, title, description);
            }
            GUILayout.FlexibleSpace();
        }

        private void EndSystemRow()
        {
            GUILayout.EndHorizontal();
            if (compactLayout) GUILayout.EndVertical();
            GUILayout.Space(6f);
        }

        private void DrawLocomotiveRow()
        {
            BeginSystemRow("01", "LOKOMOTİF", "Hız yükseldikçe yol daha hızlı alınır fakat yakıt ve mekanik aşınma artar.", 78f);
            GUILayout.BeginVertical(GUILayout.Width(105f));
            GUILayout.Label("HIZ MODU", metricLabelStyle);
            GUILayout.Label(trainState.PaceLabel, metricValueStyle);
            GUILayout.EndVertical();
            if (GUILayout.Button("MOD DEĞİŞTİR", buttonStyle, GUILayout.Width(142f), GUILayout.Height(38f))) trainState.CyclePace();
            GUILayout.Space(4f);
            EndSystemRow();
        }

        private void DrawLivingRow()
        {
            string description = trainState.LivingActuallyRunning
                ? "Isıtma ve yaşam desteği çalışıyor; nüfus sağlığı korunuyor."
                : "Isıtma verimsiz veya kapalı; ısı ve sağlık düşebilir.";

            if (compactLayout)
            {
                GUILayout.BeginVertical(rowStyle);
                GUILayout.BeginHorizontal();
                DrawRowIdentity("03", "YAŞAM", description);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical(GUILayout.Width(105f));
                GUILayout.Label("DURUM", metricLabelStyle);
                GUILayout.Label(trainState.LivingActuallyRunning ? "ÇALIŞIYOR" : "VERİMSİZ",
                    trainState.LivingActuallyRunning ? metricValueStyle : statusStyle);
                GUILayout.EndVertical();
                if (GUILayout.Button(trainState.LivingCarPowered ? "GÜCÜ KES" : "GÜÇ VER",
                    buttonStyle, GUILayout.Width(128f), GUILayout.Height(38f))) trainState.ToggleLivingCarPower();
                GUILayout.EndHorizontal();
                if (GUILayout.Button("İLAÇ KULLAN", buttonStyle, GUILayout.Height(34f))) trainState.UseMedicine();
                GUILayout.EndVertical();
                GUILayout.Space(6f);
                return;
            }

            BeginSystemRow("03", "YAŞAM", description, 86f);
            GUILayout.BeginVertical(GUILayout.Width(105f));
            GUILayout.Label("DURUM", metricLabelStyle);
            GUILayout.Label(trainState.LivingActuallyRunning ? "ÇALIŞIYOR" : "VERİMSİZ",
                trainState.LivingActuallyRunning ? metricValueStyle : statusStyle);
            GUILayout.EndVertical();
            if (GUILayout.Button(trainState.LivingCarPowered ? "GÜCÜ KES" : "GÜÇ VER",
                buttonStyle, GUILayout.Width(128f), GUILayout.Height(38f))) trainState.ToggleLivingCarPower();
            if (GUILayout.Button("İLAÇ KULLAN", buttonStyle, GUILayout.Width(128f), GUILayout.Height(38f))) trainState.UseMedicine();
            GUILayout.Space(4f);
            EndSystemRow();
        }

        private void DrawToggleRow(string number, string title, string description,
            bool active, string activeText, string inactiveText, System.Action toggle, string actualStatus)
        {
            BeginSystemRow(number, title, description, 78f);
            GUILayout.BeginVertical(GUILayout.Width(105f));
            GUILayout.Label("GERÇEK DURUM", metricLabelStyle);
            bool good = actualStatus == "ÜRETİYOR" || actualStatus == "ONARIYOR" ||
                        actualStatus == "HAZIR" || actualStatus == "ÇALIŞIYOR";
            GUILayout.Label(actualStatus, good ? metricValueStyle : statusStyle);
            GUILayout.EndVertical();
            if (GUILayout.Button(active ? activeText : inactiveText, buttonStyle,
                GUILayout.Width(142f), GUILayout.Height(38f))) toggle?.Invoke();
            GUILayout.Space(4f);
            EndSystemRow();
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
            if (titleStyle != null) return;
            backdropTexture = SolidTexture(new Color(0.006f, 0.010f, 0.014f, 0.88f));
            panelTexture = SolidTexture(new Color(0.024f, 0.032f, 0.040f, 0.99f));
            sectionTexture = SolidTexture(new Color(0.040f, 0.052f, 0.062f, 0.99f));
            rowTexture = SolidTexture(new Color(0.032f, 0.042f, 0.050f, 0.99f));
            accentTexture = SolidTexture(new Color(0.72f, 0.22f, 0.045f, 1f));
            warningTexture = SolidTexture(new Color(0.95f, 0.48f, 0.08f, 1f));
            dangerTexture = SolidTexture(new Color(0.82f, 0.13f, 0.09f, 1f));
            goodTexture = SolidTexture(new Color(0.18f, 0.60f, 0.52f, 1f));
            mutedTexture = SolidTexture(new Color(0.10f, 0.13f, 0.15f, 1f));
            buttonTexture = SolidTexture(new Color(0.18f, 0.22f, 0.25f, 1f));
            buttonHoverTexture = SolidTexture(new Color(0.28f, 0.32f, 0.35f, 1f));

            panelStyle = new GUIStyle(GUI.skin.box) { normal = { background = panelTexture } };
            sectionStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(14, 14, 10, 10), margin = new RectOffset(0, 0, 0, 0), normal = { background = sectionTexture } };
            rowStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(14, 12, 10, 10), normal = { background = rowTexture } };
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 23, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.94f, 0.97f, 1f) } };
            subtitleStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = new Color(0.46f, 0.56f, 0.62f) } };
            sectionTitleStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.70f, 0.78f, 0.82f) } };
            metricLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.46f, 0.56f, 0.62f) } };
            metricValueStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.84f, 0.93f, 0.95f) } };
            rowTitleStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.90f, 0.94f, 0.97f) } };
            rowBodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, wordWrap = true, normal = { textColor = new Color(0.57f, 0.65f, 0.69f) } };
            statusStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.92f, 0.42f, 0.16f) } };
            warningStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, fontStyle = FontStyle.Bold, wordWrap = true, normal = { textColor = new Color(1f, 0.58f, 0.22f) } };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { background = buttonTexture, textColor = new Color(0.90f, 0.94f, 0.96f) },
                hover = { background = buttonHoverTexture, textColor = Color.white },
                active = { background = accentTexture, textColor = Color.white }
            };
            smallButtonStyle = new GUIStyle(buttonStyle) { fontSize = 10 };
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
