#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Small safety cleanup after screenshot-driven dressing: interaction/circulation always wins over decor.
    /// This specifically keeps the Living rear ladder approach and inter-car path unobstructed.
    /// </summary>
    internal static class FrozenTrainScreenshotQASafety
    {
        private const string LivingPath = "Assets/Prefabs/Train/03_Living.prefab";

        public static void Apply()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(LivingPath) == null) return;
            GameObject root = PrefabUtility.LoadPrefabContents(LivingPath);
            try
            {
                Transform med = FindFirst(root.transform, "MedicalCabinet");
                if (med != null)
                {
                    med.localPosition = new Vector3(3.80f, med.localPosition.y, 4.16f);
                    med.localRotation = Quaternion.identity;
                }

                Transform qa = root.transform.Find("__ScreenshotQAV1");
                Transform main = qa != null ? qa.Find("MainDeckQA") : null;
                if (main != null)
                {
                    DestroyChild(main, "LivingServiceBackboard");
                    DestroyChild(main, "LivingServiceHeader");

                    Material frame = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/TrainProduction/FT_PRO_Frame.mat");
                    Material fabric = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/TrainProduction/FT_PRO_Fabric.mat");
                    // Two shallow wall panels replace the cross-car bulkhead decor and leave the centre passage open.
                    CreatePanel(main, "LivingServicePanelL", new Vector3(7.82f, 1.48f, -3.42f), new Vector3(.07f, 2.25f, 1.55f), frame);
                    CreatePanel(main, "LivingServicePanelR", new Vector3(7.82f, 1.48f, 3.42f), new Vector3(.07f, 2.25f, 1.55f), frame);
                    CreatePanel(main, "LivingServiceAccentL", new Vector3(7.77f, 2.38f, -3.42f), new Vector3(.035f, .18f, 1.20f), fabric);
                    CreatePanel(main, "LivingServiceAccentR", new Vector3(7.77f, 2.38f, 3.42f), new Vector3(.035f, .18f, 1.20f), fabric);
                }

                PrefabUtility.SaveAsPrefabAsset(root, LivingPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            AssetDatabase.SaveAssets();
        }

        private static void DestroyChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null) Object.DestroyImmediate(child.gameObject);
        }

        private static void CreatePanel(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            Renderer r = go.GetComponent<Renderer>();
            if (r != null && mat != null) r.sharedMaterial = mat;
            Collider c = go.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);
        }

        private static Transform FindFirst(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform found = FindFirst(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
#endif
