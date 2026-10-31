using System;
using UnityEngine;

namespace FrozenTrain.Visuals
{
    /// <summary>
    /// Tile the industrial wall preview by *physical metres* instead of
    /// stretching each 96 px preview over an entire 24 m car panel.
    /// Both materials stay separate; colliders and authored geometry are untouched.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrainWallTiling : MonoBehaviour
    {
        private const float InteriorMetresPerTile = 1.20f;
        private const float ExteriorMetresPerTile = 1.45f;
        private static readonly int BaseMapST = Shader.PropertyToID("_BaseMap_ST");
        private static readonly int BumpMapST = Shader.PropertyToID("_BumpMap_ST");

        private void OnEnable() => Apply();
        private void Start() => Apply();

        private void Apply()
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                Material material = renderer.sharedMaterial;
                if (material == null) continue;

                bool interior = material.name == "02_Walls";
                if (!interior && material.name != "01_Body") continue;
                float metresPerTile = interior ? InteriorMetresPerTile : ExteriorMetresPerTile;

                // The generated inner liner is a Quad. On its X-end faces
                // the horizontal world span is Z, but its local scale.x is
                // already the correct width. Other walls are Unity cubes.
                bool innerQuad = renderer.name.StartsWith("__InsideFace_", StringComparison.Ordinal);
                bool endFace = innerQuad &&
                    (renderer.name.Contains("Front") || renderer.name.Contains("Rear"));
                Vector3 bounds = renderer.bounds.size;
                float horizontalSpan;
                float horizontalCentre;

                if (innerQuad)
                {
                    horizontalSpan = Mathf.Abs(renderer.transform.lossyScale.x);
                    horizontalCentre = endFace
                        ? renderer.bounds.center.z : renderer.bounds.center.x;
                }
                else if (bounds.x >= bounds.z)
                {
                    horizontalSpan = bounds.x;
                    horizontalCentre = renderer.bounds.center.x;
                }
                else
                {
                    horizontalSpan = bounds.z;
                    horizontalCentre = renderer.bounds.center.z;
                }

                float verticalSpan = bounds.y;
                if (horizontalSpan < .01f || verticalSpan < .01f) continue;

                // Shared world-origin offsets align the pattern at adjacent
                // seams instead of restarting the image at every wall fragment.
                Vector4 uv = new Vector4(
                    horizontalSpan / metresPerTile,
                    verticalSpan / metresPerTile,
                    (horizontalCentre - horizontalSpan * .5f) / metresPerTile,
                    renderer.bounds.min.y / metresPerTile);

                renderer.GetPropertyBlock(block);
                block.SetVector(BaseMapST, uv);
                block.SetVector(BumpMapST, uv);
                renderer.SetPropertyBlock(block);
                block.Clear();
            }
        }
    }
}
