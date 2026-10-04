#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    /// <summary>Small final cleanup for the shared upper vestibule; never covers the physical hatch openings.</summary>
    internal static class FrozenTrainUpperDeckFinalPolish
    {
        public static void Apply()
        {
            Material floor = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/TrainProduction/FT_PRO_Floor.mat");
            for (int i = 0; i < L.Cars.Length; i++)
            {
                string path = "Assets/Prefabs/Train/" + L.Cars[i].Name + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;

                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Transform upper = FindFirst(root.transform, "UpperSleepingDeck");
                    if (upper == null) continue;

                    Transform old = upper.Find("SharedVestibuleFloorMark");
                    if (old != null) Object.DestroyImmediate(old.gameObject);

                    Transform existing = upper.Find("VestibuleCentreRunner");
                    if (existing != null) Object.DestroyImmediate(existing.gameObject);

                    float s = L.Cars[i].UpperLaddersAtFront ? -1f : 1f;
                    GameObject runner = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    runner.name = "VestibuleCentreRunner";
                    runner.transform.SetParent(upper, false);
                    runner.transform.localPosition = new Vector3(s * 7.12f, L.UpperTop + .018f, 0f);
                    runner.transform.localScale = new Vector3(3.05f, .028f, 1.92f);
                    Collider c = runner.GetComponent<Collider>();
                    if (c != null) Object.DestroyImmediate(c);
                    Renderer r = runner.GetComponent<Renderer>();
                    if (r != null && floor != null) r.sharedMaterial = floor;

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
            AssetDatabase.SaveAssets();
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
