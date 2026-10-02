#if UNITY_EDITOR
using FrozenTrain.Management;
using FrozenTrain.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrozenTrain.EditorTools
{
    public static class FrozenTrainManagementSetup
    {
        private const string ScenePath = "Assets/Scenes/FPSPrototype.unity";
        private const string TerminalScreenMaterialPath = "Assets/Materials/Prototype/FPS_TerminalScreen.mat";
        private const string FrameMaterialPath = "Assets/Materials/Prototype/FPS_Frame.mat";

        [MenuItem("Frozen Train/Add Portable Management Panel")]
        public static void AddPortableManagementPanel()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject trainRoot = GameObject.Find("TRAIN_INTERIOR_PROTOTYPE");
            GameObject player = GameObject.Find("FPS_Player");

            if (trainRoot == null || player == null)
            {
                Debug.LogError("FPS prototype objects were not found. Run Frozen Train > Rebuild FPS Prototype (URP Safe) first.");
                return;
            }

            TrainPrototypeState state = trainRoot.GetComponent<TrainPrototypeState>();
            if (state == null)
            {
                state = trainRoot.AddComponent<TrainPrototypeState>();
            }

            FpsPlayerController controller = player.GetComponent<FpsPlayerController>();
            TrainManagementPanel panel = player.GetComponent<TrainManagementPanel>();
            if (panel == null)
            {
                panel = player.AddComponent<TrainManagementPanel>();
            }
            panel.Configure(state, controller);

            if (GameObject.Find("ManagementTerminal") == null)
            {
                CreateManagementTerminal(trainRoot.transform);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("FrozenTrain management prototype added. Press Tab anywhere, or look at the locomotive terminal and press E.");
        }

        private static void CreateManagementTerminal(Transform trainRoot)
        {
            Material frameMaterial = AssetDatabase.LoadAssetAtPath<Material>(FrameMaterialPath);
            if (frameMaterial == null)
            {
                Shader lit = Shader.Find("Universal Render Pipeline/Lit");
                frameMaterial = new Material(lit)
                {
                    name = "FPS_TerminalBody",
                    color = new Color(0.06f, 0.08f, 0.10f)
                };
            }

            Material screenMaterial = GetOrCreateScreenMaterial();

            GameObject terminalRoot = new GameObject("ManagementTerminal");
            terminalRoot.transform.SetParent(trainRoot);
            terminalRoot.transform.position = new Vector3(-14.7f, 0f, -1.95f);
            terminalRoot.AddComponent<ManagementTerminal>();

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "TerminalBody";
            body.transform.SetParent(terminalRoot.transform);
            body.transform.localPosition = new Vector3(0f, 0.78f, 0f);
            body.transform.localScale = new Vector3(1.6f, 1.45f, 0.42f);
            body.GetComponent<Renderer>().sharedMaterial = frameMaterial;

            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.name = "TerminalScreen";
            screen.transform.SetParent(terminalRoot.transform);
            screen.transform.localPosition = new Vector3(0f, 0.95f, 0.235f);
            screen.transform.localScale = new Vector3(1.12f, 0.58f, 0.035f);
            screen.GetComponent<Renderer>().sharedMaterial = screenMaterial;

            GameObject console = GameObject.CreatePrimitive(PrimitiveType.Cube);
            console.name = "TerminalConsole";
            console.transform.SetParent(terminalRoot.transform);
            console.transform.localPosition = new Vector3(0f, 0.34f, 0.20f);
            console.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
            console.transform.localScale = new Vector3(1.35f, 0.18f, 0.46f);
            console.GetComponent<Renderer>().sharedMaterial = frameMaterial;
        }

        private static Material GetOrCreateScreenMaterial()
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(TerminalScreenMaterialPath);
            if (existing != null)
            {
                return existing;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Material material = new Material(shader)
            {
                name = "FPS_TerminalScreen",
                color = new Color(0.08f, 0.42f, 0.52f)
            };

            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(0.05f, 0.65f, 0.82f) * 1.8f);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.55f);
            }

            AssetDatabase.CreateAsset(material, TerminalScreenMaterialPath);
            return material;
        }
    }
}
#endif
