#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Screenshot QA showed magenta mountains/rocks: those scene objects could inherit a temporary or
    /// legacy non-URP material. This pass guarantees persistent URP/Lit assets and reapplies them after
    /// the exterior QA pass has rebuilt the scenery.
    /// </summary>
    internal static class FrozenTrainExteriorMaterialFix
    {
        private const string ScenePath = "Assets/Scenes/FPSPrototype.unity";
        private const string MaterialRoot = "Assets/Art/Materials/Environment";

        public static void Apply()
        {
            EnsureFolder("Assets/Art/Materials");
            EnsureFolder(MaterialRoot);

            Material snow = EnsureUrpMaterial("FT_ENV_Snow", new Color(.88f, .92f, .96f), 0f, .18f);
            Material rock = EnsureUrpMaterial("FT_ENV_Rock", new Color(.115f, .135f, .16f), .04f, .12f);

            Scene scene = File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
                : SceneManager.GetActiveScene();

            GameObject env = GameObject.Find("OUTSIDE_ENVIRONMENT");
            if (env == null) return;

            foreach (Renderer r in env.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null) continue;
                string n = r.gameObject.name;
                if (n == "SnowCap" || n == "SoftSnowDrift")
                    r.sharedMaterial = snow;
                else if (n == "Mountain" || n == "Rock")
                    r.sharedMaterial = rock;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Material EnsureUrpMaterial(string name, Color color, float metallic, float smoothness)
        {
            string path = MaterialRoot + "/" + name + ".mat";
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

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            string name = path.Substring(slash + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif