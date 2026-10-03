using FrozenTrain.Management;
using UnityEngine;

namespace FrozenTrain.Atmosphere
{
    public sealed class MovingScenery : MonoBehaviour
    {
        [SerializeField] private float speed = 13f;
        [SerializeField] private float minX = -35f;
        [SerializeField] private float maxX = 62f;
        [SerializeField] private float speedResponse = 0.35f;

        private TrainPrototypeState trainState;
        private bool searchedForState;
        private float motion = 1f;

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

            // Scenery speed follows the real train speed so stopping/accelerating is visible outside.
            if (!searchedForState)
            {
                trainState = FindFirstObjectByType<TrainPrototypeState>();
                searchedForState = true;
            }

            float targetMotion = trainState != null ? trainState.MotionFactor : 1f;
            motion = Mathf.MoveTowards(motion, targetMotion, speedResponse * Time.deltaTime);
            if (motion <= 0.0001f)
            {
                return;
            }

            // The locomotive front faces -X, so scenery must travel toward +X
            // to create the correct forward-motion illusion.
            foreach (Transform child in transform)
            {
                Vector3 position = child.localPosition;
                position.x += speed * motion * Time.deltaTime;

                while (position.x > maxX)
                {
                    position.x -= span;
                }

                child.localPosition = position;
            }
        }
    }
}
