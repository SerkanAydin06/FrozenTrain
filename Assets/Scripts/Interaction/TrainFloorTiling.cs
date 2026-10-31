using UnityEngine;

namespace FrozenTrain.Visuals
{
    /// <summary>
    /// Keeps one shared 03_Floors URP material while tiling its generated
    /// industrial floor image at a stable size, regardless of floor mesh size.
    /// All five cars and both gangway decks use exactly the same scale.
    /// This is a visual material-property override, not a collider or mesh edit.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrainFloorTiling : MonoBehaviour
    {
        private const float MetersPerTile = 2.50f;
        private static readonly int BaseMapST = Shader.PropertyToID("_BaseMap_ST");
        private static readonly int BumpMapST = Shader.PropertyToID("_BumpMap_ST");

        private void OnEnable() => Apply();
        private void Start() => Apply();

        private void Apply()
        {
            var properties = new MaterialPropertyBlock();
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                Material material = renderer.sharedMaterial;
                if (material == null || material.name != "03_Floors") continue;

                Vector3 size = renderer.transform.lossyScale;
                Vector3 centre = renderer.transform.position;
                float tileX = Mathf.Max(.05f, Mathf.Abs(size.x) / MetersPerTile);
                float tileZ = Mathf.Max(.05f, Mathf.Abs(size.z) / MetersPerTile);
                float offsetX = (centre.x - Mathf.Abs(size.x) * .5f) / MetersPerTile;
                float offsetZ = (centre.z - Mathf.Abs(size.z) * .5f) / MetersPerTile;
                Vector4 uv = new Vector4(tileX, tileZ, offsetX, offsetZ);

                renderer.GetPropertyBlock(properties);
                properties.SetVector(BaseMapST, uv);
                properties.SetVector(BumpMapST, uv);
                renderer.SetPropertyBlock(properties);
                properties.Clear();
            }
        }
    }
}
