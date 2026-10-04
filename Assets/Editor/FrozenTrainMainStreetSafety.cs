#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    /// <summary>Final physical-safety cleanup for the Main Street architecture.</summary>
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
                    FixPhysicalPartitions(root.transform);
                    if (i == 0) FixLocomotiveEquipmentBalance(root.transform);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void FixPhysicalPartitions(Transform root)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "SafetyGlass" || t.name == "CabBulkhead" || t.name == "CabGlassDoor" || t.name == "ResidentialEndBulkhead")
                {
                    if (t.GetComponent<Collider>() == null)
                        t.gameObject.AddComponent<BoxCollider>();
                }
            }
        }

        private static void FixLocomotiveEquipmentBalance(Transform root)
        {
            Transform[] cabinets = root.GetComponentsInChildren<Transform>(true)
                .Where(t => t.name == "EquipmentCabinet")
                .OrderBy(t => t.localPosition.x)
                .ToArray();

            for (int i = 0; i < cabinets.Length; i++)
            {
                Vector3 p = cabinets[i].localPosition;
                // Keep the two original left-wall cabinets on -Z and the opposite cabinet on +Z.
                p.z = i < 2 ? -3.96f : 3.96f;
                cabinets[i].localPosition = p;
            }
        }
    }
}
#endif
