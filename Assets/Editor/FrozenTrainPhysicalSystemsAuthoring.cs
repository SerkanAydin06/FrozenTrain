#if UNITY_EDITOR
using FrozenTrain.Interaction;
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Binds survival mechanics to already-authored prefab objects.
    /// It never creates train geometry at runtime.
    /// </summary>
    public static class FrozenTrainPhysicalSystemsAuthoring
    {
        private const string TrainPrefabFolder = "Assets/Prefabs/Train";

        [MenuItem("Frozen Train/Refresh Physical Survival Systems", priority = 3)]
        public static void RefreshFromMenu()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(TrainPrefabFolder + "/01_Locomotive.prefab") == null)
            {
                FrozenTrainAuthoredLayoutResize.ResizeAuthoredTrain();
            }

            InstallOrRefresh();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("FrozenTrain physical survival controls refreshed on all five authored car prefabs.");
        }

        public static void InstallOrRefresh()
        {
            ConfigurePrefab("01_Locomotive", root =>
            {
                Bind(root, "MainConsole", TrainSystemControlType.CyclePace);
            });

            ConfigurePrefab("02_Generator", root =>
            {
                Bind(root, "GeneratorConsole", TrainSystemControlType.GeneratorToggle);
            });

            ConfigurePrefab("03_Living", root =>
            {
                Bind(root, "WallHeater", TrainSystemControlType.LivingPowerToggle);
                Bind(root, "GalleyCounter", TrainSystemControlType.UseMedicine);
            });

            ConfigurePrefab("04_Workshop", root =>
            {
                Bind(root, "MachineStation", TrainSystemControlType.WorkshopPowerToggle);
                Bind(root, "WorkbenchBase", TrainSystemControlType.WorkshopMaintenanceBoost);
            });

            ConfigurePrefab("05_Greenhouse", root =>
            {
                Bind(root, "PumpUnit", TrainSystemControlType.GreenhousePowerToggle);
                Bind(root, "WaterTank", TrainSystemControlType.RefillWaterFromReserve);
                Bind(root, "HydroponicBed", TrainSystemControlType.GreenhouseNutrientFlush);
            });
        }

        private static void ConfigurePrefab(string prefabName, System.Action<GameObject> configure)
        {
            string path = TrainPrefabFolder + "/" + prefabName + ".prefab";
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                Debug.LogWarning("FrozenTrain: prefab missing, cannot bind survival controls: " + path);
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                configure?.Invoke(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void Bind(GameObject root, string objectName, TrainSystemControlType type)
        {
            Transform target = FindDeepChild(root.transform, objectName);
            if (target == null)
            {
                Debug.LogWarning($"FrozenTrain: {objectName} not found under {root.name}.");
                return;
            }

            TrainSystemInteractable[] existing = target.GetComponents<TrainSystemInteractable>();
            TrainSystemInteractable control = existing.Length > 0 ? existing[0] : target.gameObject.AddComponent<TrainSystemInteractable>();
            control.Configure(null, type);

            for (int i = 1; i < existing.Length; i++)
                Object.DestroyImmediate(existing[i]);
        }

        private static Transform FindDeepChild(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeepChild(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
#endif
