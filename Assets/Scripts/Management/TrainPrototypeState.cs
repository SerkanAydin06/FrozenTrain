using UnityEngine;

namespace FrozenTrain.Management
{
    /// <summary>
    /// Small runtime state container used by the first management-panel prototype.
    /// This will later be replaced/expanded by the real train simulation systems.
    /// </summary>
    public sealed class TrainPrototypeState : MonoBehaviour
    {
        [Header("Energy")]
        [SerializeField] private int generatorCapacity = 100;
        [SerializeField] private int emergencyCapacity = 18;
        [SerializeField] private int baseConsumption = 38;
        [SerializeField] private int livingCarConsumption = 22;

        [Header("Resources")]
        [SerializeField] private int heatPercent = 74;
        [SerializeField] private int food = 86;
        [SerializeField] private int water = 79;
        [SerializeField] private int population = 10;

        [Header("Car State")]
        [SerializeField] private bool generatorOnline = true;
        [SerializeField] private bool livingCarPowered = true;

        public bool GeneratorOnline => generatorOnline;
        public bool LivingCarPowered => livingCarPowered;
        public int EnergyCapacity => generatorOnline ? generatorCapacity : emergencyCapacity;
        public int EnergyConsumption => baseConsumption + (livingCarPowered ? livingCarConsumption : 0);
        public int HeatPercent => heatPercent;
        public int Food => food;
        public int Water => water;
        public int Population => population;

        public void ToggleGenerator()
        {
            generatorOnline = !generatorOnline;
        }

        public void ToggleLivingCarPower()
        {
            livingCarPowered = !livingCarPowered;
        }
    }
}
