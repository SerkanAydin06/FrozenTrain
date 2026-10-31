using System;
using UnityEngine;

namespace FrozenTrain.Visuals
{
    /// <summary>
    /// Tiles the wall materials on the train's texture grid instead of stretching one image per panel.
    /// Interior (02_Walls): exactly 1 m tiles, horizontally from the car's inner corner and vertically
    /// from the floor top of the deck the panel belongs to, so each 3 m deck wall shows exactly 3 whole
    /// tiles and the pattern continues across window piers, lintels and door jambs.
    /// Exterior (01_Body): per-axis tile sizes chosen by the authoring pass so the outer length, width
    /// and full body height each hold a whole number of tiles.
    /// Values are written by the authoring pass from FrozenTrainLayout. Visual property override only.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrainWallTiling : MonoBehaviour
    {
        [SerializeField] private float interiorTile = 1f;
        [SerializeField] private Vector2 interiorOriginXZ = new Vector2(-12f, -4.5f);
        [SerializeField] private float lowestDeckTop = -3.2f;
        [SerializeField] private float deckPitch = 3.2f;
        [SerializeField] private Vector3 exteriorTile = new Vector3(1f, 1f, 1f);
        [SerializeField] private Vector3 exteriorOrigin = new Vector3(-12.1f, -3.3f, -4.6f);

        private static readonly int BaseMapST = Shader.PropertyToID("_BaseMap_ST");
        private static readonly int BumpMapST = Shader.PropertyToID("_BumpMap_ST");

        public void Configure(float tile, Vector2 originXZ, float firstDeckTop, float pitch,
            Vector3 outerTile, Vector3 outerOrigin)
        {
            interiorTile = Mathf.Max(.05f, tile);
            interiorOriginXZ = originXZ;
            lowestDeckTop = firstDeckTop;
            deckPitch = Mathf.Max(.1f, pitch);
            exteriorTile = new Vector3(Mathf.Max(.05f, outerTile.x), Mathf.Max(.05f, outerTile.y), Mathf.Max(.05f, outerTile.z));
            exteriorOrigin = outerOrigin;
        }

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

                Vector4 uv = interior ? InteriorUV(renderer) : ExteriorUV(renderer);
                if (uv.x <= 0f || uv.y <= 0f) continue;

                renderer.GetPropertyBlock(block);
                block.SetVector(BaseMapST, uv);
                block.SetVector(BumpMapST, uv);
                renderer.SetPropertyBlock(block);
                block.Clear();
            }
        }

        private Vector4 InteriorUV(Renderer renderer)
        {
            Vector3 centre = transform.InverseTransformPoint(renderer.bounds.center);
            Vector3 origin = new Vector3(interiorOriginXZ.x, 0f, interiorOriginXZ.y);
            float span, height, uStart;

            if (renderer.name.StartsWith("__InsideFace_", StringComparison.Ordinal))
            {
                // Generated liner quad: follow its own +X axis (may point along +-X or +-Z).
                Vector3 right = transform.InverseTransformDirection(renderer.transform.right).normalized;
                span = Mathf.Abs(renderer.transform.lossyScale.x);
                height = Mathf.Abs(renderer.transform.lossyScale.y);
                Vector3 leftEdge = centre - right * span * .5f;
                uStart = Vector3.Dot(leftEdge - origin, right);
            }
            else
            {
                Vector3 size = renderer.bounds.size;
                height = size.y;
                if (size.x >= size.z)
                {
                    span = size.x;
                    uStart = centre.x - size.x * .5f - origin.x;
                }
                else
                {
                    span = size.z;
                    uStart = centre.z - size.z * .5f - origin.z;
                }
            }

            if (span < .01f || height < .01f) return Vector4.zero;
            float bottom = centre.y - height * .5f;
            float deckTop = lowestDeckTop + Mathf.Floor((centre.y - lowestDeckTop) / deckPitch) * deckPitch;
            return new Vector4(span / interiorTile, height / interiorTile,
                uStart / interiorTile, (bottom - deckTop) / interiorTile);
        }

        private Vector4 ExteriorUV(Renderer renderer)
        {
            Vector3 size = renderer.bounds.size;
            Vector3 centre = transform.InverseTransformPoint(renderer.bounds.center);
            bool alongX = size.x >= size.z;
            float span = alongX ? size.x : size.z;
            float tileH = alongX ? exteriorTile.x : exteriorTile.z;
            float start = alongX ? centre.x - size.x * .5f - exteriorOrigin.x : centre.z - size.z * .5f - exteriorOrigin.z;
            if (span < .01f || size.y < .01f) return Vector4.zero;
            float bottom = centre.y - size.y * .5f;
            return new Vector4(span / tileH, size.y / exteriorTile.y,
                start / tileH, (bottom - exteriorOrigin.y) / exteriorTile.y);
        }
    }
}
