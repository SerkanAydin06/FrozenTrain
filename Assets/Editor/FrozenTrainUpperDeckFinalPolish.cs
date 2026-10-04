#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    /// <summary>Final cleanup for shared vestibules; keeps both physical hatches and their circulation clear.</summary>
    internal static class FrozenTrainUpperDeckFinalPolish
    {
        public static void Apply()
        {
            Material floor = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/TrainProduction/FT_PRO_Floor.mat");
            Material hazard = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/TrainProduction/FT_PRO_Hazard.mat");

            for (int i = 0; i < L.Cars.Length; i++)
            {
                string path = "Assets/Prefabs/Train/" + L.Cars[i].Name + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;

                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Transform production = root.transform.Find("__ProductionArtV2");
                    if (production != null)
                    {
                        Transform obsolete = production.Find("AccessSafetyDetails");
                        if (obsolete != null) Object.DestroyImmediate(obsolete.gameObject);
                    }

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

                    if (L.Cars[i].TechnicalLadder)
                        AddTechnicalHatchMark(root.transform, hazard);

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
            AssetDatabase.SaveAssets();
        }

        private static void AddTechnicalHatchMark(Transform root, Material hazard)
        {
            Transform old = root.Find("__TechnicalHatchMark");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            GameObject group = new GameObject("__TechnicalHatchMark");
            group.transform.SetParent(root, false);

            float x0 = L.TechnicalHoleX0, x1 = L.TechnicalHoleX1;
            float z0 = L.TechnicalHoleZ0, z1 = L.TechnicalHoleZ1;
            float xc = (x0 + x1) * .5f, zc = (z0 + z1) * .5f;
            Strip(group.transform, new Vector3(x0, L.MainTop + .025f, zc), new Vector3(.08f, .04f, z1 - z0), hazard);
            Strip(group.transform, new Vector3(x1, L.MainTop + .025f, zc), new Vector3(.08f, .04f, z1 - z0), hazard);
            Strip(group.transform, new Vector3(xc, L.MainTop + .025f, z0), new Vector3(x1 - x0, .04f, .08f), hazard);
            Strip(group.transform, new Vector3(xc, L.MainTop + .025f, z1), new Vector3(x1 - x0, .04f, .08f), hazard);
        }

        private static void Strip(Transform parent, Vector3 pos, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "HatchHazardMark";
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            Collider c = go.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);
            Renderer r = go.GetComponent<Renderer>();
            if (r != null && material != null) r.sharedMaterial = material;
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
