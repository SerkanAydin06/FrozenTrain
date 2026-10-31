#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using L = FrozenTrain.EditorTools.FrozenTrainLayout;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Outside-the-train scenery pass: low-poly mountain range, rock field, soft snow drifts and
    /// the widened rail gauge for the wide three-deck body. All scenery uses persistent URP/Lit
    /// materials (FT_ENV_Snow / FT_ENV_Rock) so no magenta fallback can appear.
    /// Replaces the scene half of the former ScreenshotQARefinement pass and ExteriorMaterialFix;
    /// their car-prefab dressing was always discarded by FrozenTrainMinimalInteriorPass.
    /// </summary>
    internal static class FrozenTrainExteriorScenery
    {
        private const string EnvMaterialRoot = "Assets/Art/Materials/Environment";
        private const string MeshRoot = "Assets/Art/Meshes/Generated";
        private const string ScenePath = "Assets/Scenes/FPSPrototype.unity";

        public static void Apply()
        {
            EnsureFolder(EnvMaterialRoot);
            Material snow = EnsureUrpMaterial("FT_ENV_Snow", new Color(.88f, .92f, .96f), 0f, .18f);
            Material rock = EnsureUrpMaterial("FT_ENV_Rock", new Color(.115f, .135f, .16f), .04f, .12f);
            BuildScenery(snow, rock);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void BuildScenery(Material snow, Material rock)
        {
            Scene scene = File.Exists(ScenePath) ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single) : SceneManager.GetActiveScene();
            GameObject env = GameObject.Find("OUTSIDE_ENVIRONMENT");
            if (env == null) return;

            Transform near = FindFirst(env.transform, "NEAR_SCENERY");
            Transform far = FindFirst(env.transform, "DISTANT_SCENERY");
            if (near != null)
            {
                RemoveNamedChildrenRecursive(near, "PassingRock", "SnowDrift");
                Transform old = near.Find("QA_RockField");
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                BuildRockField(near, snow, rock);
            }
            if (far != null)
            {
                RemoveNamedChildrenRecursive(far, "DistantMountain");
                Transform old = far.Find("QA_MountainRange");
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                BuildMountainRange(far, snow, rock);
            }

            // Widen the visual rail gauge a little for the unusually wide three-deck body and extend sleepers.
            Transform[] rails = env.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Rail").ToArray();
            for (int i = 0; i < rails.Length; i++)
            {
                Vector3 p = rails[i].localPosition;
                p.z = (i % 2 == 0 ? -1f : 1f) * 3.15f;
                rails[i].localPosition = p;
            }
            foreach (Transform sleeper in env.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Sleeper"))
            {
                Vector3 s = sleeper.localScale;
                s.z = 7.65f;
                sleeper.localScale = s;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void BuildMountainRange(Transform far, Material snow, Material rock)
        {
            Mesh mesh = GetOrCreateMountainMesh();
            System.Random rng = new System.Random(41017);
            Transform group = Group(far, "QA_MountainRange", Vector3.zero);
            for (int i = 0; i < 14; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                float x = -82f + i * 13.2f;
                float z = side * (31f + (float)rng.NextDouble() * 13f);
                float sx = 11f + (float)rng.NextDouble() * 12f;
                float sy = 10f + (float)rng.NextDouble() * 12f;
                float sz = 9f + (float)rng.NextDouble() * 10f;

                Transform mountain = MeshObject(group, "Mountain", mesh, new Vector3(x, L.GroundY - .2f, z), new Vector3(sx, sy, sz), rock);
                mountain.localRotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 55f - 27f, 0f);
                Transform cap = MeshObject(mountain, "SnowCap", mesh, new Vector3(0f, .58f, 0f), new Vector3(.76f, .38f, .76f), snow);
                cap.localRotation = Quaternion.Euler(0f, 14f, 0f);
            }
        }

        private static void BuildRockField(Transform near, Material snow, Material rock)
        {
            Mesh mesh = GetOrCreateMountainMesh();
            System.Random rng = new System.Random(7721);
            Transform group = Group(near, "QA_RockField", Vector3.zero);
            for (int i = 0; i < 18; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                float x = -58f + i * 7.2f;
                float z = side * (11f + (float)rng.NextDouble() * 7f);
                float sx = 1.8f + (float)rng.NextDouble() * 3.2f;
                float sy = .8f + (float)rng.NextDouble() * 1.6f;
                float sz = 1.6f + (float)rng.NextDouble() * 2.8f;
                Transform rockObject = MeshObject(group, "Rock", mesh, new Vector3(x, L.GroundY, z), new Vector3(sx, sy, sz), rock);
                rockObject.localRotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 180f, 0f);
                MeshObject(rockObject, "SnowCap", mesh, new Vector3(0f, .50f, 0f), new Vector3(.82f, .34f, .82f), snow);
            }

            for (int i = 0; i < 18; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                GameObject drift = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                drift.name = "SoftSnowDrift";
                drift.transform.SetParent(group, false);
                drift.transform.localPosition = new Vector3(-56f + i * 7f, L.GroundY + .18f, side * (6.3f + (i % 3) * .65f));
                drift.transform.localScale = new Vector3(4.2f + (i % 4), .42f + (i % 2) * .12f, 1.5f + (i % 3) * .32f);
                Renderer r = drift.GetComponent<Renderer>();
                if (r != null) r.sharedMaterial = snow;
                Collider c = drift.GetComponent<Collider>();
                if (c != null) UnityEngine.Object.DestroyImmediate(c);
            }
        }

        private static Mesh GetOrCreateMountainMesh()
        {
            EnsureFolder("Assets/Art/Meshes");
            EnsureFolder(MeshRoot);
            string path = MeshRoot + "/FT_LowPolyMountain.asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) return existing;

            const int sides = 8;
            var vertices = new List<Vector3>();
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2f / sides;
                float radius = 1f + Mathf.Sin(i * 2.31f) * .08f;
                vertices.Add(new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius));
            }
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2f / sides + .12f;
                float radius = .52f + Mathf.Cos(i * 1.77f) * .06f;
                vertices.Add(new Vector3(Mathf.Cos(a) * radius, .48f, Mathf.Sin(a) * radius));
            }
            vertices.Add(new Vector3(.08f, 1f, -.04f));
            int apex = vertices.Count - 1;

            var triangles = new List<int>();
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                int lowerA = i, lowerB = next, midA = sides + i, midB = sides + next;
                triangles.Add(lowerA); triangles.Add(midA); triangles.Add(lowerB);
                triangles.Add(lowerB); triangles.Add(midA); triangles.Add(midB);
                triangles.Add(midA); triangles.Add(apex); triangles.Add(midB);
            }

            Mesh mesh = new Mesh { name = "FT_LowPolyMountain" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        // ---- Helpers ------------------------------------------------------------------------

        // ---- Helpers ------------------------------------------------------------------------

        private static Material EnsureUrpMaterial(string name, Color color, float metallic, float smoothness)
        {
            string path = EnvMaterialRoot + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else if (shader != null && material.shader != shader)
            {
                material.shader = shader;
            }

            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Transform MeshObject(Transform parent, string name, Mesh mesh, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            return go.transform;
        }

        private static void RemoveNamedChildrenRecursive(Transform root, params string[] names)
        {
            HashSet<string> set = new HashSet<string>(names);
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = all.Length - 1; i >= 0; i--)
            {
                Transform t = all[i];
                if (t != null && t != root && set.Contains(t.name)) UnityEngine.Object.DestroyImmediate(t.gameObject);
            }
        }

        private static Transform Group(Transform parent, string name, Vector3 pos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            return go.transform;
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

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            string leaf = Path.GetFileName(folder);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            if (!string.IsNullOrEmpty(parent)) AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
