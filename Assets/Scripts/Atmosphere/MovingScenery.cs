using UnityEngine;

namespace FrozenTrain.Atmosphere
{
    public sealed class MovingScenery : MonoBehaviour
    {
        [SerializeField] private float speed = 13f;
        [SerializeField] private float minX = -35f;
        [SerializeField] private float maxX = 62f;

        private void Update()
        {
            float span = maxX - minX;
            if (span <= 0.1f) return;

            foreach (Transform child in transform)
            {
                Vector3 p = child.localPosition;
                p.x -= speed * Time.deltaTime;
                while (p.x < minX) p.x += span;
                child.localPosition = p;
            }
        }
    }
}
