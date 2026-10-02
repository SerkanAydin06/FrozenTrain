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
        private GUIStyle headerStyle;
        private GUIStyle bodyStyle;
        private GUIStyle statusStyle;
        private GUIStyle warningStyle;

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
            if (Keyboard.current == null) return;
            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                if (isOpen) ClosePanel(); else OpenPanel();
            }
        }

        public void OpenPanel()
        {
            if (isOpen) return;
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
            isOpen = false;
        }

        private void OnGUI()
        {
            if (!isOpen) return;
            EnsureStyles();

            Color old = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.78f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = old;

            float width = Mathf.Min(1080f, Screen.width - 50f);
            float height = Mathf.Min(820f, Screen.height - 40f);
            Rect rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUI.Box(rect, GUIContent.none);

            GUILayout.BeginArea(new Rect(rect.x + 22f, rect.y + 18f, rect.width - 44f, rect.height - 36f));
            GUILayout.BeginHorizontal();
            GUILayout.Label("FROZENTRAIN // TREN YÖNETİMİ", titleStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Kapat [TAB]", GUILayout.Width(130f), GUILayout.Height(32f))) ClosePanel();
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);

            if (trainState == null)
            {
                GUILayout.Label("Tren durumu bulunamadı.", bodyStyle);
                GUILayout.EndArea();
                return;
            }

            DrawResourceStrip();
            if (trainState.EnergyDeficit > 0)
            {
                GUILayout.Space(8f);
                GUILayout.Label("⚠ ENERJİ AÇIĞI: " + trainState.EnergyDeficit + " birim. Bir sistemi kapat veya jeneratörü devreye al.", warningStyle);
            }

            GUILayout.Space(12f);
            GUILayout.Label("VAGONLAR", headerStyle);
            scroll = GUILayout.BeginScrollView(scroll);
            DrawFixedRow("01  LOKOMOTİF", "Çekiş, rota ve ana kumanda", "ÇALIŞIYOR");
            DrawToggleRow("02  JENERATÖR", trainState.GeneratorOnline ? "Ana enerji üretimi çevrimiçi" : "Ana üretim kapalı — acil güç", trainState.GeneratorOnline, "Jeneratörü Kapat", "Jeneratörü Aç", trainState.ToggleGenerator);
            DrawToggleRow("03  YAŞAM VAGONU", trainState.LivingCarPowered ? "Isıtma ve yaşam sistemleri aktif" : "Güç kesildi — konfor ve ısı riski", trainState.LivingCarPowered, "Gücü Kes", "Gücü Ver", trainState.ToggleLivingCarPower);
            DrawToggleRow("04  ATÖLYE", trainState.WorkshopPowered ? "Üretim ve tamir tezgâhları aktif" : "Atölye beklemede", trainState.WorkshopPowered, "Atölyeyi Kapat", "Atölyeyi Aç", trainState.ToggleWorkshopPower);
            DrawToggleRow("05  SERA", trainState.GreenhousePowered ? "Yetiştirme ışıkları ve pompalar aktif" : "Sera kapalı — yiyecek üretimi durdu", trainState.GreenhousePowered, "Serayı Kapat", "Serayı Aç", trainState.ToggleGreenhousePower);
            GUILayout.EndScrollView();
            GUILayout.Space(6f);
            GUILayout.Label("Panel açıkken oyun %20 hızında devam eder. Tam kapasitede tüm sistemleri aynı anda çalıştırmak mümkün değildir; güç dağıtımı artık gerçek bir karar.", bodyStyle);
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
            DrawMetric("MALZEME", trainState.Materials.ToString());
            DrawMetric("İLAÇ", trainState.Medicine.ToString());
            DrawMetric("NÜFUS", trainState.Population.ToString());
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawMetric(string label, string value)
        {
            GUILayout.BeginVertical(GUILayout.MinWidth(100f));
            GUILayout.Label(label, statusStyle);
            GUILayout.Label(value, headerStyle);
            GUILayout.EndVertical();
        }

        private void DrawFixedRow(string title, string description, string status)
        {
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(76f));
            GUILayout.BeginVertical();
            GUILayout.Label(title, headerStyle);
            GUILayout.Label(description, bodyStyle);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label(status, statusStyle, GUILayout.Width(150f));
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
        }

        private void DrawToggleRow(string title, string description, bool active, string activeText, string inactiveText, System.Action toggle)
        {
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(76f));
            GUILayout.BeginVertical();
            GUILayout.Label(title, headerStyle);
            GUILayout.Label(description, bodyStyle);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label(active ? "AKTİF" : "KAPALI", statusStyle, GUILayout.Width(80f));
            if (GUILayout.Button(active ? activeText : inactiveText, GUILayout.Width(165f), GUILayout.Height(38f))) toggle?.Invoke();
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = Color.white } };
            headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.86f, 0.93f, 1f) } };
            bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = true, normal = { textColor = new Color(0.78f, 0.82f, 0.86f) } };
            statusStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.67f, 0.84f, 1f) } };
            warningStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold, wordWrap = true, normal = { textColor = new Color(1f, 0.62f, 0.28f) } };
        }
    }
}
