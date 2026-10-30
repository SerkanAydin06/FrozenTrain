#if UNITY_EDITOR
using System.Linq;
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

            for (int i = 0; i < L.Cars.Length; i++)
            {
                string path = PrefabRoot + "/" + L.Cars[i].Name + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    // Keep the QA aisle markings, but remove its old 18 m three-light pattern.
                    Transform qa = root.transform.Find("__ScreenshotQAV1");
                    Transform lowerQa = qa != null ? qa.Find("LowerDeckQA") : null;
                    if (lowerQa != null)
                    {
                        foreach (Transform t in lowerQa.GetComponentsInChildren<Transform>(true)
                                     .Where(t => t != lowerQa && (t.name == "LowerCeilingLuminaire" || t.name == "LowerServiceLight"))
                                     .ToArray())
                            if (t != null) Object.DestroyImmediate(t.gameObject);
                    }

                    Transform old = root.transform.Find(MarkerName);
                    if (old != null) Object.DestroyImmediate(old.gameObject);
                    Transform marker = Group(root.transform, MarkerName);
                    foreach (float x in new[] { -9.2f, -4.6f, 0f, 4.6f, 9.2f })
                    {
                        Cube(marker, "TechnicalLuminaire", new Vector3(x, L.MainY - L.MainFloorT * .5f - .0275f, 0f),
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
            EnsureFolder(MaterialRoot);
            string path = MaterialRoot + "/" + name + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (m == null)
            {
                m = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(m, path);
            }
            m.shader = shader;
            m.color = fallback;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", fallback);
            EditorUtility.SetDirty(m);
            return m;
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

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;
            string parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string name = System.IO.Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }
    }
}
#endif