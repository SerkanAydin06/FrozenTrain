using UnityEngine;

namespace FrozenTrain.Management
{
    /// <summary>
    /// Continuous survival simulation for the fixed five-car train.
    /// No scripted story events: all pressure comes from resources and player decisions.
    /// </summary>
    public sealed class SurvivalSimulation : MonoBehaviour
    {
        [SerializeField] private TrainPrototypeState state;
        [SerializeField, Min(0.25f)] private float simulationStepSeconds = 1f;

        private float timer;

        private void Awake()
        {
            if (state == null)
            {
                state = GetComponent<TrainPrototypeState>();
            }
        }

        private void Update()
        {
            if (state == null)
            {
                return;
            }

            timer += Time.deltaTime;
            while (timer >= simulationStepSeconds)
            {
                timer -= simulationStepSeconds;
                state.SimulateSecond(simulationStepSeconds);
            }
        }
    }
}
