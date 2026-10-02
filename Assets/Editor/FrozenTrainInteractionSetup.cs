#if UNITY_EDITOR
using FrozenTrain.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrozenTrain.EditorTools
{
    public static class FrozenTrainInteractionSetup
    {
        private const string ScenePath = "Assets/Scenes/FPSPrototype.unity";
        private const string DoorMaterialPath = "Assets/Materials/Prototype/FPS_Door.mat";

        [MenuItem("Frozen Train/Add FPS Doors & Interaction")]
        public static void AddDoorsAndInteraction()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject trainRoot = GameObject.Find("TRAIN_INTERIOR_PROTOTYPE");
            GameObject player = GameObject.Find("FPS_Player");

            if (trainRoot == null || player == null)
            {
                Debug.LogError("FPS prototype objects were not found. Run Frozen Train > Create FPS Train Prototype first.");
                return;
            }

            Camera playerCamera = player.GetComponentInChildren<Camera>(true);
            if (playerCamera == null)
            {
                Debug.LogError("FPS Player camera was not found.");
                return;
            }

            PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
            if (interaction == null)
            {
                interaction = player.AddComponent<PlayerInteraction>();
            }
            interaction.Configure(playerCamera, 2.8f);

            Material doorMaterial = GetOrCreateDoorMaterial();
            int createdDoors = 0;

            foreach (Transform child in trainRoot.transform)
            {
                if (child.name != "CarConnector")
                {
                    continue;
                }

                Transform existingDoor = child.Find("SlidingDoor");
                if (existingDoor != null)
                {
                    continue;
                }

                GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
                door.name = "SlidingDoor";
                door.transform.SetParent(child);
                door.transform.localPosition = new Vector3(0f, 1.18f, 0f);
                door.transform.localRotation = Quaternion.identity;
                door.transform.localScale = new Vector3(0.12f, 2.25f, 1.48f);
                door.GetComponent<Renderer>().sharedMaterial = doorMaterial;

                SlidingDoor slidingDoor = door.AddComponent<SlidingDoor>();
                slidingDoor.Configure(new Vector3(0f, 0f, -1.55f), 2.8f);
                createdDoors++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("FPS interaction setup complete. Created " + createdDoors + " sliding doors. Press Play, look at a door, then press E.");
        }

        private static Material GetOrCreateDoorMaterial()
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(DoorMaterialPath);
            if (existing != null)
            {
                return existing;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader)
            {
                name = "FPS_Door",
                color = new Color(0.16f, 0.22f, 0.25f)
            };

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0.65f);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.42f);
            }

            AssetDatabase.CreateAsset(material, DoorMaterialPath);
            return material;
        }
    }
}
#endif
