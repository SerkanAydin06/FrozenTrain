#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    internal static class FrozenTrainLongLightingPass
    {
        private const string PrefabRoot = "Assets/Prefabs/Train";
        private const string MaterialRoot = "Assets/Art/Materials/TrainProduction";
        private const string MarkerName = "__LongLightingV1";

        public static void Apply()
        {
            Material frame = Mat("FT_PRO_Frame", new Color(.04f, .05f, .06f));
            Material warm = Mat("FT_PRO_WarmLight", new Color(.90f, .55f, .18f));

            for (int i = 0; i < L.Cars.Length; i++)
            {
                string path = PrefabRoot + "/" + L.Cars[i].Name + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Transform old = root.transform.Find(MarkerName);
                    if (old != null) Object.DestroyImmediate(old.gameObject);
                    Transform marker = Group(root.transform, MarkerName);
                    foreach (float x in new[] { -9.2f, -4.6f, 0f, 4.6f, 9.2f })
                    {
                        Cube(marker, "TechnicalLuminaire", new Vector3(x, L.MainY - .28f, 0f),
                            new Vector3(2.35f, .055f, .24f), frame);
                        Transform t = Group(marker, "TechnicalServiceLight");
                        t.localPosition = new Vector3(x, L.MainY - .42f, 0f);
                        Light l = t.gameObject.AddComponent<Light>();
                        l.type = LightType.Point;
                        l.intensity = .68f;
                        l.range = 5.4f;
                        l.color = new Color(.64f, .76f, .88f);
                        l.shadows = LightShadows.None;
                    }
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
        }

        private static Material Mat(string name, Color fallback)
        {
            Material m = AssetDatabase.LoadAssetAtPath<Material>(MaterialRoot + "/" + name + ".mat");
            if (m != null) return m;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            return new Material(shader) { name = name + "_LongLightingFallback", color = fallback };
        }

        private static Transform Group(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void Cube(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            Collider c = go.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);
        }
    }
}
#endif