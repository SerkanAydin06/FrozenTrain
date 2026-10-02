using UnityEngine;

namespace FrozenTrain.Management
{
    public enum GeneratorFaultPhase
    {
        Normal,
        Alarm,
        EngineerEnRoute,
        ManualIntervention
    }

    public sealed class TrainPrototypeState : MonoBehaviour
    {
        [Header("Energy")]
        [SerializeField] private int generatorCapacity = 100;
        [SerializeField] private int emergencyCapacity = 18;
        [SerializeField] private int baseConsumption = 38;
        [SerializeField] private int livingCarConsumption = 22;
        [SerializeField] private int workshopConsumption = 24;
        [SerializeField] private int greenhouseConsumption = 28;

        [Header("Resources")]
        [SerializeField] private int heatPercent = 74;
        [SerializeField] private int food = 86;
        [SerializeField] private int water = 79;
        [SerializeField] private int materials = 42;
        [SerializeField] private int medicine = 14;
        [SerializeField] private int population = 10;

        [Header("Car State")]
        [SerializeField] private bool generatorOnline = true;
        [SerializeField] private bool livingCarPowered = true;
        [SerializeField] private bool workshopPowered = false;
        [SerializeField] private bool greenhousePowered = false;

        [Header("Emergency State")]
        [SerializeField] private GeneratorFaultPhase generatorFaultPhase = GeneratorFaultPhase.Normal;
        [SerializeField] private string assignedEngineerName = string.Empty;

        public bool GeneratorOnline => generatorOnline;
        public bool LivingCarPowered => livingCarPowered;
        public bool WorkshopPowered => workshopPowered;
        public bool GreenhousePowered => greenhousePowered;
        public GeneratorFaultPhase CurrentGeneratorFaultPhase => generatorFaultPhase;
        public bool GeneratorFaultActive => generatorFaultPhase != GeneratorFaultPhase.Normal;
        public bool EngineerAssigned => generatorFaultPhase == GeneratorFaultPhase.EngineerEnRoute || generatorFaultPhase == GeneratorFaultPhase.ManualIntervention;
        public bool ManualInterventionRequired => generatorFaultPhase == GeneratorFaultPhase.ManualIntervention;
        public string AssignedEngineerName => assignedEngineerName;

        public int EnergyCapacity => generatorOnline ? generatorCapacity : emergencyCapacity;
        public int EnergyConsumption => baseConsumption
                                        + (livingCarPowered ? livingCarConsumption : 0)
                                        + (workshopPowered ? workshopConsumption : 0)
                                        + (greenhousePowered ? greenhouseConsumption : 0);
        public int EnergyDeficit => Mathf.Max(0, EnergyConsumption - EnergyCapacity);
        public int HeatPercent => heatPercent;
        public int Food => food;
        public int Water => water;
        public int Materials => materials;
        public int Medicine => medicine;
        public int Population => population;

        public void ToggleGenerator()
        {
            if (GeneratorFaultActive)
            {
                return;
            }

            generatorOnline = !generatorOnline;
        }

        public void ToggleLivingCarPower() => livingCarPowered = !livingCarPowered;
        public void ToggleWorkshopPower() => workshopPowered = !workshopPowered;
        public void ToggleGreenhousePower() => greenhousePowered = !greenhousePowered;

        public bool TriggerGeneratorFault()
        {
            if (GeneratorFaultActive)
            {
                return false;
            }

            generatorOnline = false;
            generatorFaultPhase = GeneratorFaultPhase.Alarm;
            assignedEngineerName = string.Empty;
            return true;
        }

        public bool AssignEngineerToGenerator(string engineerName)
        {
            if (generatorFaultPhase != GeneratorFaultPhase.Alarm)
            {
                return false;
            }

            assignedEngineerName = string.IsNullOrWhiteSpace(engineerName) ? "Mühendis" : engineerName;
            generatorFaultPhase = GeneratorFaultPhase.EngineerEnRoute;
            return true;
        }

        public bool MarkEngineerOnSite()
        {
            if (generatorFaultPhase != GeneratorFaultPhase.EngineerEnRoute)
            {
                return false;
            }

            generatorFaultPhase = GeneratorFaultPhase.ManualIntervention;
            return true;
        }

        public bool CompleteGeneratorRepair()
        {
            if (generatorFaultPhase != GeneratorFaultPhase.ManualIntervention)
            {
                return false;
            }

            generatorOnline = true;
            generatorFaultPhase = GeneratorFaultPhase.Normal;
            assignedEngineerName = string.Empty;
            return true;
        }
    }
}
