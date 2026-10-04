#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Final screenshot-driven correction for the 24 m train.
    /// Converts the vestibule's stacked stair blocks into actual thin treads and improves
    /// upper residential visibility without changing circulation or gameplay bindings.
    /// </summary>
    internal static class FrozenTrainFinalScreenshotFix
    {
        private const string PrefabRoot = "Assets/Prefabs/Train";
        private const string ConnectorPath = "Assets/Prefabs/Train/Shared/CarConnector.prefab";
        private const string MaterialRoot = "Assets/Art/Materials/TrainProduction";
        private const string MarkerName = "__FinalScreenshotFixV1";
        private const float TreadThickness = .14f;

        public static void Apply()
        {
            Material steel = LoadMaterial("FT_PRO_BrushedSteel");
            Material frame = LoadMaterial("FT_PRO_Frame");
            Material warm = LoadMaterial("FT_PRO_WarmLight");
            ConfigureWarmEmission(warm);

            FixVestibuleStairs(steel, frame);
            ImproveUpperResidentialLighting(warm);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void FixVestibuleStairs(Material steel, Material frame)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ConnectorPath) == null) return;
            GameObject root = PrefabUtility.LoadPrefabContents(ConnectorPath);
            try
            {
                Transform old = root.transform.Find(MarkerName);
                if (old != null) Object.DestroyImmediate(old.gameObject);

                // The canonical stair generator used the entire rise as each cube's height. That makes
                // the stair read as one huge serrated wall from the landing. Preserve every tread's top
                // elevation and collider, but make each element a proper thin tread.
                ThinTreads(root.transform, "UpperStairStep", steel);
                ThinTreads(root.transform, "LowerStairStep", steel);

                Transform marker = Group(root.transform, MarkerName);

                // Open industrial stringers make the two flights read as real stairs while keeping the
                // lobby visually porous. These are visual only; tread colliders remain the walk surface.
                AddStringers(marker, -.82f, L.MainTop + .08f, L.UpperTop - .10f, frame, "Upper");
                AddStringers(marker, .82f, L.LowerTop + .08f, L.MainTop - .10f, frame, "Lower");

                // Landing edge cues improve depth perception without obstructing the route.
                Cube(marker, "MainLandingEdge", new Vector3(0f, L.MainTop + .04f, -.53f),
                    new Vector3(3.25f, .055f, .07f), steel, false);
                Cube(marker, "UpperLandingEdge", new Vector3(-.82f, L.UpperTop + .04f, -.50f),
                    new Vector3(1.55f, .055f, .07f), steel, false);
                Cube(marker, "LowerLandingEdge", new Vector3(.82f, L.LowerTop + .04f, -.50f),
                    new Vector3(1.55f, .055f, .07f), steel, false);

                PrefabUtility.SaveAsPrefabAsset(root, ConnectorPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void ThinTreads(Transform root, string name, Material steel)
        {
            foreach (Transform step in FindAll(root, name))
            {
                Vector3 scale = step.localScale;
                Vector3 pos = step.localPosition;
                float topY = pos.y + Mathf.Abs(scale.y) * .5f;
                scale.y = TreadThickness;
                pos.y = topY - TreadThickness * .5f;
                step.localScale = scale;
                step.localPosition = pos;

                Renderer r = step.GetComponent<Renderer>();
                if (r != null && steel != null) r.sharedMaterial = steel;
            }
        }

        private static void AddStringers(Transform parent, float centreX, float lowY, float highY,
            Material frame, string prefix)
        {
            const float zLow = -3.72f;
            const float zHigh = -.68f;
            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 a = new Vector3(centreX + side * .48f, lowY, zLow);
                Vector3 b = new Vector3(centreX + side * .48f, highY, zHigh);
                Beam(parent, prefix + "StairStringer", a, b, .075f, frame);
            }
        }

        private static void ImproveUpperResidentialLighting(Material warm)
        {
            for (int i = 0; i < L.Cars.Length; i++)
            {
                string path = PrefabRoot + "/" + L.Cars[i].Name + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (Transform t in FindAll(root.transform, "UpperHallFill"))
                    {
                        Light l = t.GetComponent<Light>();
                        if (l == null) continue;
                        l.intensity = Mathf.Max(l.intensity, .88f);
                        l.range = Mathf.Max(l.range, 5.35f);
                        l.shadows = LightShadows.None;
                    }

                    foreach (Transform fixture in FindAll(root.transform, "CabinCeilingLight"))
                    {
                        Renderer r = fixture.GetComponent<Renderer>();
                        if (r != null && warm != null) r.sharedMaterial = warm;
                    }

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }

        private static void ConfigureWarmEmission(Material warm)
        {
            if (warm == null || !warm.HasProperty("_EmissionColor")) return;
            Color emission = new Color(1.0f, .54f, .22f) * 1.65f;
            warm.EnableKeyword("_EMISSION");
            warm.SetColor("_EmissionColor", emission);
            EditorUtility.SetDirty(warm);
        }

        private static Material LoadMaterial(string name) =>
            AssetDatabase.LoadAssetAtPath<Material>(MaterialRoot + "/" + name + ".mat");

        private static IEnumerable<Transform> FindAll(Transform root, string name)
        {
            if (root == null) yield break;
            if (root.name == name) yield return root;
            foreach (Transform child in root)
                foreach (Transform found in FindAll(child, name))
                    yield return found;
        }

        private static Transform Group(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static GameObject Cube(Transform parent, string name, Vector3 pos, Vector3 size,
            Material material, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            Renderer r = go.GetComponent<Renderer>();
            if (r != null && material != null) r.sharedMaterial = material;
            Collider c = go.GetComponent<Collider>();
            if (!collider && c != null) Object.DestroyImmediate(c);
            return go;
        }

        private static void Beam(Transform parent, string name, Vector3 a, Vector3 b,
            float thickness, Material material)
        {
            Vector3 dir = b - a;
            float length = dir.magnitude;
            GameObject beam = Cube(parent, name, (a + b) * .5f,
                new Vector3(thickness, thickness, length), material, false);
            if (length > .001f)
                beam.transform.localRotation = Quaternion.FromToRotation(Vector3.forward, dir / length);
        }
    }
}
#endif