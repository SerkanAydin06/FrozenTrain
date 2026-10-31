using UnityEngine;

namespace FrozenTrain.Visuals
{
    /// <summary>
    /// Keeps one shared 03_Floors URP material and tiles it on the train's 1 m texture grid.
    /// UVs are computed in car-local space from the inner corner of the car, so every floor slab
    /// (including pieces split around hatches) continues the same pattern and the 24 x 9 m clear
    /// floor holds exactly 24 x 9 whole tiles. Values are written by the authoring pass from
    /// FrozenTrainLayout; this is a visual material-property override only.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrainFloorTiling : MonoBehaviour
    {
        [SerializeField] private float metresPerTile = 1f;
        [SerializeField] private Vector2 gridOriginXZ = new Vector2(-12f, -4.5f);

        private static readonly int BaseMapST = Shader.PropertyToID("_BaseMap_ST");
        private static readonly int BumpMapST = Shader.PropertyToID("_BumpMap_ST");

        public void Configure(float tile, Vector2 originXZ)
        {
            metresPerTile = Mathf.Max(.05f, tile);
            gridOriginXZ = originXZ;
        }

        private void OnEnable() => Apply();
        private void Start() => Apply();

        private void Apply()
        {
            var properties = new MaterialPropertyBlock();
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                Material material = renderer.sharedMaterial;
                if (material == null || material.name != "03_Floors") continue;

                Vector3 size = renderer.bounds.size;
                Vector3 centre = transform.InverseTransformPoint(renderer.bounds.center);
                float minX = centre.x - size.x * .5f, minZ = centre.z - size.z * .5f;
                Vector4 uv = new Vector4(
                    Mathf.Max(.05f, size.x / metresPerTile),
                    Mathf.Max(.05f, size.z / metresPerTile),
                    (minX - gridOriginXZ.x) / metresPerTile,
                    (minZ - gridOriginXZ.y) / metresPerTile);

                renderer.GetPropertyBlock(properties);
                properties.SetVector(BaseMapST, uv);
                properties.SetVector(BumpMapST, uv);
                renderer.SetPropertyBlock(properties);
                properties.Clear();
            }
        }
    }
}
