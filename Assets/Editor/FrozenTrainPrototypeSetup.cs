#if UNITY_EDITOR
using FrozenTrain.CameraSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrozenTrain.EditorTools
{
    public static class FrozenTrainPrototypeSetup
    {
        private const string ScenePath = "Assets/Scenes/MainPrototype.unity";
        private const string MaterialFolder = "Assets/Materials/Prototype";

        [MenuItem("Frozen Train/Create First Prototype Scene")]
        public static void CreateFirstPrototypeScene()
        {
            EnsureFolder("Assets/Materials");
            EnsureFolder(MaterialFolder);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainPrototype";

            Material darkMetal = GetOrCreateMaterial("Prototype_DarkMetal", new Color(0.12f, 0.15f, 0.18f));
            Material generatorMetal = GetOrCreateMaterial("Prototype_Generator", new Color(0.27f, 0.17f, 0.10f));
            Material livingMetal = GetOrCreateMaterial("Prototype_Living", new Color(0.16f, 0.24f, 0.27f));
            Material railMaterial = GetOrCreateMaterial("Prototype_Rail", new Color(0.08f, 0.08f, 0.09f));
            Material snowMaterial = GetOrCreateMaterial("Prototype_Snow", new Color(0.78f, 0.84f, 0.88f));

            CreateLighting();
            CreateEnvironment(snowMaterial, railMaterial);

            GameObject trainRoot = new("TRAIN_PROTOTYPE");
            CreateCar(trainRoot.transform, "01_Locomotive", new Vector3(-14f, 1.8f, 0f), darkMetal, true);
            CreateCar(trainRoot.transform, "02_Generator", new Vector3(0f, 1.8f, 0f), generatorMetal, false);
            CreateCar(trainRoot.transform, "03_Living", new Vector3(14f, 1.8f, 0f), livingMetal, false);

            CreateManagementCamera();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = trainRoot;
            Debug.Log("FrozenTrain prototype created. Press Play and use WASD + middle mouse + wheel.");
        }

        private static void CreateLighting()
        {
            GameObject lightObject = new("Sun");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(0.76f, 0.84f, 1f);
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(42f, -32f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.18f, 0.25f, 0.34f);
            RenderSettings.ambientEquatorColor = new Color(0.11f, 0.14f, 0.18f);
            RenderSettings.ambientGroundColor = new Color(0.06f, 0.07f, 0.08f);
        }

        private static void CreateEnvironment(Material snowMaterial, Material railMaterial)
        {
            GameObject environment = new("ENVIRONMENT");

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "SnowGround";
            ground.transform.SetParent(environment.transform);
            ground.transform.position = new Vector3(0f, 0f, 0f);
            ground.transform.localScale = new Vector3(14f, 1f, 6f);
            ground.GetComponent<Renderer>().sharedMaterial = snowMaterial;

            CreateRail(environment.transform, new Vector3(0f, 0.15f, -2.1f), railMaterial);
            CreateRail(environment.transform, new Vector3(0f, 0.15f, 2.1f), railMaterial);

            for (int x = -55; x <= 55; x += 5)
            {
                GameObject sleeper = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sleeper.name = $"Sleeper_{x}";
                sleeper.transform.SetParent(environment.transform);
                sleeper.transform.position = new Vector3(x, 0.08f, 0f);
                sleeper.transform.localScale = new Vector3(0.35f, 0.15f, 5.2f);
                sleeper.GetComponent<Renderer>().sharedMaterial = railMaterial;
            }
        }

        private static void CreateRail(Transform parent, Vector3 position, Material material)
        {
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Rail";
            rail.transform.SetParent(parent);
            rail.transform.position = position;
            rail.transform.localScale = new Vector3(115f, 0.2f, 0.18f);
            rail.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateCar(Transform parent, string name, Vector3 position, Material material, bool locomotive)
        {
            GameObject car = new(name);
            car.transform.SetParent(parent);
            car.transform.position = position;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(car.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = locomotive
                ? new Vector3(11.5f, 3.6f, 5.4f)
                : new Vector3(12f, 3.4f, 5.4f);
            body.GetComponent<Renderer>().sharedMaterial = material;

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(car.transform);
            roof.transform.localPosition = new Vector3(0f, 2f, 0f);
            roof.transform.localScale = new Vector3(12.2f, 0.35f, 5.8f);
            roof.GetComponent<Renderer>().sharedMaterial = material;

            CreateWheel(car.transform, new Vector3(-3.7f, -2f, -2.6f));
            CreateWheel(car.transform, new Vector3(3.7f, -2f, -2.6f));
            CreateWheel(car.transform, new Vector3(-3.7f, -2f, 2.6f));
            CreateWheel(car.transform, new Vector3(3.7f, -2f, 2.6f));

            if (locomotive)
            {
                GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
                nose.name = "LocomotiveNose";
                nose.transform.SetParent(car.transform);
                nose.transform.localPosition = new Vector3(-6.4f, -0.6f, 0f);
                nose.transform.localScale = new Vector3(1.8f, 2.2f, 5f);
                nose.GetComponent<Renderer>().sharedMaterial = material;
            }
        }

        private static void CreateWheel(Transform parent, Vector3 localPosition)
        {
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = "Wheel";
            wheel.transform.SetParent(parent);
            wheel.transform.localPosition = localPosition;
            wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            wheel.transform.localScale = new Vector3(1.15f, 0.38f, 1.15f);
        }

        private static void CreateManagementCamera()
        {
            GameObject cameraObject = new("ManagementCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.fieldOfView = 55f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 600f;
            cameraObject.AddComponent<AudioListener>();

            TrainManagementCamera controller = cameraObject.AddComponent<TrainManagementCamera>();
            controller.Configure(new Vector3(0f, 1.6f, 0f), 38f, -28f, 38f);
        }

        private static Material GetOrCreateMaterial(string fileName, Color color)
        {
            string path = $"{MaterialFolder}/{fileName}.mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new(shader)
            {
                name = fileName,
                color = color
            };

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.35f);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string name = System.IO.Path.GetFileName(folderPath);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }
    }
}
#endif
