#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    public static class FrozenTrainUrpMaterialRepair
    {
        private const string PrototypeMaterialFolder = "Assets/Materials/Prototype";
        private const string UrpLitShaderName = "Universal Render Pipeline/Lit";

        [MenuItem("Frozen Train/Rebuild FPS Prototype (URP Safe)")]
        public static void RebuildFpsPrototypeUrpSafe()
        {
            // Build the base train first, then add the gameplay interaction layer.
            FrozenTrainFpsPrototypeSetup.CreateFpsTrainPrototype();
            FrozenTrainInteractionSetup.AddDoorsAndInteraction();
            RepairPinkUrpMaterials();

            Debug.Log(
                "FrozenTrain: FPS prototype rebuilt with URP materials, sliding doors and E interaction.");
        }

        [MenuItem("Frozen Train/Repair Pink URP Materials")]
        public static void RepairPinkUrpMaterials()
        {
            Shader urpLit = Shader.Find(UrpLitShaderName);
            if (urpLit == null)
            {
                Debug.LogError(
                    "FrozenTrain: URP/Lit shader could not be found. " +
                    "Make sure Universal Render Pipeline is installed and active before repairing materials.");
                return;
            }

            if (!AssetDatabase.IsValidFolder(PrototypeMaterialFolder))
            {
                Debug.LogWarning("FrozenTrain: Prototype material folder does not exist yet.");
                return;
            }

            string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { PrototypeMaterialFolder });
            int repairedCount = 0;

            foreach (string guid in materialGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    continue;
                }

                if (material.shader != urpLit)
                {
                    material.shader = urpLit;
                    repairedCount++;
                }

                // Keep the prototype opaque and compatible with URP/Lit defaults.
                if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 0f);
                if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
                if (material.HasProperty("_AlphaClip")) material.SetFloat("_AlphaClip", 0f);
                if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 1f);

                EditorUtility.SetDirty(material);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "FrozenTrain: URP material repair complete. Checked " + materialGuids.Length +
                " prototype materials; reassigned " + repairedCount + " shader(s) to URP/Lit.");
        }
    }
}
#endif
