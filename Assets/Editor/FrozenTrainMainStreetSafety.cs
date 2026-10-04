#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    /// <summary>Final physical-safety cleanup for the right-side public corridor architecture.</summary>
    internal static class FrozenTrainMainStreetSafety
    {
        private const string PrefabRoot = "Assets/Prefabs/Train";

        public static void Apply()
        {
            for (int i = 0; i < L.Cars.Length; i++)
            {
                string path = PrefabRoot + "/" + L.Cars[i].Name + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;

                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    EnsurePartitionColliders(root.transform);
                    KeepFunctionalPropsOutOfCorridor(root.transform);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsurePartitionColliders(Transform root)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "CompartmentWallFront" || t.name == "CompartmentWallRear" ||
                    t.name == "CompartmentDoorHeader" || t.name == "ResidentialEndBulkhead")
                {
                    if (t.GetComponent<Collider>() == null) t.gameObject.AddComponent<BoxCollider>();
                }
            }
        }

        private static void KeepFunctionalPropsOutOfCorridor(Transform root)
        {
            // Final guard against older production/QA passes leaving movable props on the +Z passenger route.
            string[] movable =
            {
                "EquipmentCabinet", "ManagementTerminal", "MonitoringRack", "ToolCabinet",
                "MachineStation", "MedicalCabinet", "HydroponicBed", "SeatingBay", "Workbench"
            };

            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                bool tracked = false;
                for (int i = 0; i < movable.Length; i++)
                    if (t.name == movable[i]) { tracked = true; break; }
                if (!tracked) continue;

                Vector3 p = t.localPosition;
                if (p.z >= FrozenTrainMainStreetArchitecture.CorridorMinZ - .10f)
                {
                    p.z = Mathf.Min(.85f, FrozenTrainMainStreetArchitecture.CorridorMinZ - .55f);
                    t.localPosition = p;
                }
            }
        }
    }
}
#endif
