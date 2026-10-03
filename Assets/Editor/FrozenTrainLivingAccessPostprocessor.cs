#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FrozenTrain.EditorTools
{
    /// <summary>
    /// Keeps the living-car stair flights physically usable by replacing the solid
    /// main/upper slabs with segmented floors around the two stairwell openings.
    /// Runs automatically whenever the authored living prefab is rebuilt.
    /// </summary>
    public sealed class FrozenTrainLivingAccessPostprocessor : AssetPostprocessor
    {
        private const string LivingPrefabPath = "Assets/Prefabs/Train/03_Living.prefab";
        private static bool applying;

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (applying || !importedAssets.Contains(LivingPrefabPath))
            {
                return;
            }

            ApplyOpenings();
        }

        private static void ApplyOpenings()
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(LivingPrefabPath);
            if (asset == null) return;

            applying = true;
            GameObject root = PrefabUtility.LoadPrefabContents(LivingPrefabPath);
            try
            {
                Transform architecture = FindDirect(root.transform, "MultiDeckArchitecture");
                if (architecture == null || FindDirect(architecture, "LivingFloorOpeningsApplied") != null)
                {
                    return;
                }

                Material floor = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Prototype/Train_Floor.mat");

                Transform oldMainFloor = FindDirect(root.transform, "Floor");
                if (oldMainFloor != null) Object.DestroyImmediate(oldMainFloor.gameObject);

                Transform oldUpperFloor = FindDirect(architecture, "UpperDeckFloor");
                if (oldUpperFloor != null) Object.DestroyImmediate(oldUpperFloor.gameObject);

                // Main-to-basement opening: X -6.70..-3.35, Z -1.95..-0.30.
                BuildSegmentedFloor(
                    architecture,
                    "MainFloor",
                    0f,
                    0.22f,
                    -6.70f,
                    -3.35f,
                    -1.95f,
                    -0.30f,
                    floor);

                // Main-to-upper opening: X -6.70..-2.30, Z 0.30..1.95.
                BuildSegmentedFloor(
                    architecture,
                    "UpperDeckFloor",
                    3.10f,
                    0.18f,
                    -6.70f,
                    -2.30f,
                    0.30f,
                    1.95f,
                    floor);

                GameObject marker = new GameObject("LivingFloorOpeningsApplied");
                marker.transform.SetParent(architecture, false);

                PrefabUtility.SaveAsPrefabAsset(root, LivingPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
                applying = false;
            }
        }

        private static void BuildSegmentedFloor(
            Transform parent,
            string prefix,
            float y,
            float thickness,
            float holeMinX,
            float holeMaxX,
            float holeMinZ,
            float holeMaxZ,
            Material material)
        {
            const float halfLength = 8.25f;
            const float halfWidth = 4.20f;

            float leftWidth = holeMinX + halfLength;
            if (leftWidth > 0.01f)
                Cube(parent, prefix + "_Left", new Vector3(-halfLength + leftWidth * 0.5f, y, 0f), new Vector3(leftWidth, thickness, halfWidth * 2f), material);

            float rightWidth = halfLength - holeMaxX;
            if (rightWidth > 0.01f)
                Cube(parent, prefix + "_Right", new Vector3(holeMaxX + rightWidth * 0.5f, y, 0f), new Vector3(rightWidth, thickness, halfWidth * 2f), material);

            float middleWidth = holeMaxX - holeMinX;
            float lowerZWidth = holeMinZ + halfWidth;
            if (lowerZWidth > 0.01f)
                Cube(parent, prefix + "_LowerStrip", new Vector3((holeMinX + holeMaxX) * 0.5f, y, -halfWidth + lowerZWidth * 0.5f), new Vector3(middleWidth, thickness, lowerZWidth), material);

            float upperZWidth = halfWidth - holeMaxZ;
            if (upperZWidth > 0.01f)
                Cube(parent, prefix + "_UpperStrip", new Vector3((holeMinX + holeMaxX) * 0.5f, y, holeMaxZ + upperZWidth * 0.5f), new Vector3(middleWidth, thickness, upperZWidth), material);
        }

        private static GameObject Cube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            if (material != null) go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static Transform FindDirect(Transform root, string name)
        {
            for (int i = 0; i < root.childCount; i++)
                if (root.GetChild(i).name == name) return root.GetChild(i);
            return null;
        }
    }
}
#endif
