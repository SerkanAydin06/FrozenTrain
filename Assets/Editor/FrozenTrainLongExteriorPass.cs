#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    /// <summary>Replaces 18 m screenshot-era exterior details with geometry positioned for 24 m cars.</summary>
    internal static class FrozenTrainLongExteriorPass
    {
        private const string PrefabRoot = "Assets/Prefabs/Train";
        private const string MaterialRoot = "Assets/Art/Materials/TrainProduction";
        private const string MarkerName = "__LongExteriorV1";

        private sealed class Mats
        {
            public Material Exterior;
            public Material Frame;
            public Material Steel;
            public Material Hazard;
        }

        public static void Apply()
        {
            Mats mats = new Mats
            {
                Exterior = Mat("FT_PRO_Exterior", new Color(.075f, .095f, .115f)),
                Frame = Mat("FT_PRO_Frame", new Color(.04f, .05f, .06f)),
                Steel = Mat("FT_PRO_BrushedSteel", new Color(.62f, .66f, .68f)),
                Hazard = Mat("FT_PRO_Hazard", new Color(.92f, .47f, .055f)),
            };

            for (int i = 0; i < L.Cars.Length; i++)
            {
                string path = PrefabRoot + "/" + L.Cars[i].Name + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Transform qa = root.transform.Find("__ScreenshotQAV1");
                    if (qa != null)
                    {
                        Transform oldExterior = qa.Find("ExteriorQA");
                        if (oldExterior != null) Object.DestroyImmediate(oldExterior.gameObject);
                    }
                    Transform old = root.transform.Find(MarkerName);
                    if (old != null) Object.DestroyImmediate(old.gameObject);
                    Transform marker = Group(root.transform, MarkerName);
                    BuildExterior(marker, i, mats);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
        }

        private static void BuildExterior(Transform p, int index, Mats m)
        {
            GameObject roofL = Cube(p, "RoofChamferL", new Vector3(0f, L.RoofY - .04f, -4.28f),
                new Vector3(L.CarLength - .45f, .22f, .88f), m.Exterior);
            roofL.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
            GameObject roofR = Cube(p, "RoofChamferR", new Vector3(0f, L.RoofY - .04f, 4.28f),
                new Vector3(L.CarLength - .45f, .22f, .88f), m.Exterior);
            roofR.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);

            Material accent = index == 0 ? m.Hazard : m.Steel;
            foreach (float side in new[] { -1f, 1f })
            {
                float z = side * (L.HalfW + .025f);
                Cube(p, "ExteriorRoleBand", new Vector3(0f, .73f, z),
                    new Vector3(L.CarLength - .55f, .18f, .055f), accent);
                Cube(p, "ExteriorLowerSill", new Vector3(0f, -2.78f, z),
                    new Vector3(L.CarLength - .65f, .34f, .08f), m.Frame);

                foreach (float bogieX in new[] { -L.HalfL + 3f, L.HalfL - 3f })
                {
                    foreach (float dx in new[] { -.72f, .72f })
                        WheelDisc(p, new Vector3(bogieX + dx, -3.48f, side * 4.64f), side, m);
                    Cube(p, "BogieSideBeam", new Vector3(bogieX, -3.18f, side * 4.58f),
                        new Vector3(2.55f, .26f, .16f), m.Frame);
                }
            }

            if (index == 0) BuildLocomotiveNose(p, m);
        }

        private static void BuildLocomotiveNose(Transform p, Mats m)
        {
            float x = -L.HalfL;
            Cube(p, "NoseLower", new Vector3(x - .58f, -.05f, 0f), new Vector3(1.16f, .68f, 7.70f), m.Exterior);
            Cube(p, "NoseMid", new Vector3(x - .28f, .50f, 0f), new Vector3(.62f, .42f, 7.92f), m.Frame);
            Cube(p, "NoseAccent", new Vector3(x - .62f, .34f, 0f), new Vector3(.10f, .18f, 7.76f), m.Hazard);
            Cube(p, "WindshieldBrow", new Vector3(x - .08f, 2.82f, 0f), new Vector3(.20f, .16f, 8.38f), m.Frame);
            foreach (float z in new[] { -3.18f, 3.18f })
                Cube(p, "NoseLampBezel", new Vector3(x - .92f, .54f, z), new Vector3(.12f, .48f, .64f), m.Steel);
        }

        private static void WheelDisc(Transform p, Vector3 pos, float side, Mats m)
        {
            GameObject wheel = Cylinder(p, "Wheel", pos, new Vector3(.54f, .18f, .54f), m.Frame);
            wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            GameObject rim = Cylinder(p, "WheelRim", pos + new Vector3(0f, 0f, side * .10f),
                new Vector3(.32f, .04f, .32f), m.Steel);
            rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static Material Mat(string name, Color fallback)
        {
            Material m = AssetDatabase.LoadAssetAtPath<Material>(MaterialRoot + "/" + name + ".mat");
            if (m != null) return m;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            m = new Material(shader) { name = name + "_LongFallback", color = fallback };
            return m;
        }

        private static Transform Group(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static GameObject Cube(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            Collider c = go.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);
            return go;
        }

        private static GameObject Cylinder(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            Collider c = go.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);
            return go;
        }
    }
}
#endif