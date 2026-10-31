using UnityEngine;

namespace FrozenTrain.Visuals
{
    /// <summary>
    /// Keeps interior and exterior wall textures consistent across separately
    /// constructed wall panels. No geometry or collision changes at runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrainWallTiling : MonoBehaviour
    {
        private const float MetresPerTile = 3.0f;
        private static readonly int BaseMapST = Shader.PropertyToID("_BaseMap_ST");
        private static readonly int BumpMapST = Shader.PropertyToID("_BumpMap_ST");

        private void OnEnable() { Apply(); }
        private void Start() { Apply(); }

        private void Apply()
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                Material material = renderer.sharedMaterial;
                if (material == null) continue;
                bool wall = material.name == "02_Walls";
                if (!wall && material.name != "01_Body") continue;

                Transform t = renderer.transform;
                Vector3 size = renderer.bounds.size;
                bool insideFace = renderer.name.StartsWith("__InsideFace_", System.StringComparison.Ordinal);
                float span;
                float axisPosition;
                if (insideFace)
                {
                    span = Mathf.Abs(t.lossyScale.x);
                    // End pieces have Z as their horizontal world axis.
                    bool endWall = renderer.name.Contains("Front") || renderer.name.Contains("Rear");
                    axisPosition = endWall ? t.position.z : t.position.x;
                }
                else if (size.x >= size.z)
                {
                    span = size.x;
                    axisPosition = t.position.x;
                }
                else
                {
                    span = size.z;
                    axisPosition = t.position.z;
                }

                float h = Mathf.Max(.05f, size.y / MetresPerTile);
                float w = Mathf.Max(.05f, span / MetresPerTile);
                Vector4 uv = new Vector4(w, h,
                    (axisPosition - span * .5f) / MetresPerTile,
                    (renderer.bounds.center.y - size.y * .5f) / MetresPerTile);
                renderer.GetPropertyBlock(block);
                block.SetVector(BaseMapST, uv);
                block.SetVector(BumpMapST, uv);
                renderer.SetPropertyBlock(block);
                block.Clear();
            }
        }
    }
}
