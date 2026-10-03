using UnityEngine;

namespace FrozenTrain.Atmosphere
{
    public sealed class MovingScenery : MonoBehaviour
    {
        [SerializeField] private float speed = 13f;
        [SerializeField] private float minX = -35f;
        [SerializeField] private float maxX = 62f;

        public void Configure(float movementSpeed, float minimumX, float maximumX)
        {
            speed = Mathf.Max(0f, movementSpeed);
            minX = minimumX;
            maxX = Mathf.Max(minX + 1f, maximumX);
        }

        private void Update()
        {
            float span = maxX - minX;
            if (span <= 0.1f || speed <= 0f)
            {
                return;
            }

            // The locomotive front faces -X, so scenery must travel toward +X
            // to create the correct forward-motion illusion.
            foreach (Transform child in transform)
            {
                Vector3 position = child.localPosition;
                position.x += speed * Time.deltaTime;

                while (position.x > maxX)
                {
                    position.x -= span;
                }

                child.localPosition = position;
            }
        }
    }
}
