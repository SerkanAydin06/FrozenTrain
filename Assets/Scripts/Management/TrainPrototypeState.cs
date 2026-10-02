using UnityEngine;

namespace FrozenTrain.Management
{
    /// <summary>
    /// Single source of truth for the five-car survival prototype.
    /// The game pressure comes from resource flow and power allocation, not scripted events.
    /// </summary>
    public sealed class TrainPrototypeState : MonoBehaviour
    {
        [Header("Power")]
        [SerializeField] private int generatorCapacity = 115;
        [SerializeField] private int emergencyCapacity = 18;
        [SerializeField] private int locomotiveConsumption = 34;
        [SerializeField] private int lifeSupportConsumption = 12;
        [SerializeField] private int livingCarConsumption = 24;
        [SerializeField] private int workshopConsumption = 26;
        [SerializeField] private int greenhouseConsumption = 30;

        [Header("Resources")]
        [SerializeField] private float fuel = 100f;
        [SerializeField] private float food = 100f;
        [SerializeField] private float water = 100f;
        [SerializeField] private float materials = 60f;
        [SerializeField] private float medicine = 20f;
        [SerializeField, Range(0f, 100f)] private float heatPercent = 72f;
        [SerializeField] private int population = 10;

        [Header("Car State")]
        [SerializeField] private bool generatorOnline = true;
        [SerializeField] private bool livingCarPowered = true;
        [SerializeField] private bool workshopPowered;
        [SerializeField] private bool greenhousePowered;

        [Header("Balance / real second")]
        [SerializeField] private float locomotiveFuelUse = 0.018f;
        [SerializeField] private float generatorFuelUse = 0.035f;
        [SerializeField] private float foodUsePerPerson = 0.0015f;
        [SerializeField] private float waterUsePerPerson = 0.0024f;
        [SerializeField] private float greenhouseFoodProduction = 0.055f;
        [SerializeField] private float greenhouseWaterUse = 0.022f;
        [SerializeField] private float poweredHeatGain = 0.10f;
        [SerializeField] private float unpoweredHeatLoss = 0.075f;
        [SerializeField] private float deficitHeatLoss = 0.035f;

        public bool GeneratorOnline => generatorOnline;
        public bool LivingCarPowered => livingCarPowered;
        public bool WorkshopPowered => workshopPowered;
        public bool GreenhousePowered => greenhousePowered;

        public int EnergyCapacity => generatorOnline && fuel > 0.01f ? generatorCapacity : emergencyCapacity;
        public int EnergyConsumption => locomotiveConsumption
                                        + lifeSupportConsumption
                                        + (livingCarPowered ? livingCarConsumption : 0)
                                        + (workshopPowered ? workshopConsumption : 0)
                                        + (greenhousePowered ? greenhouseConsumption : 0);
        public int EnergyDeficit => Mathf.Max(0, EnergyConsumption - EnergyCapacity);
        public float PowerEfficiency => EnergyConsumption <= 0
            ? 1f
            : Mathf.Clamp01(EnergyCapacity / (float)EnergyConsumption);
        public bool PowerStable => EnergyDeficit == 0;

        public int Fuel => Mathf.CeilToInt(fuel);
        public int HeatPercent => Mathf.RoundToInt(heatPercent);
        public int Food => Mathf.CeilToInt(food);
        public int Water => Mathf.CeilToInt(water);
        public int Materials => Mathf.CeilToInt(materials);
        public int Medicine => Mathf.CeilToInt(medicine);
        public int Population => population;

        public float FuelDeltaPerMinute => -(locomotiveFuelUse + (generatorOnline ? generatorFuelUse : 0f)) * 60f;
        public float FoodDeltaPerMinute => (-population * foodUsePerPerson
                                            + (GreenhouseActuallyRunning ? greenhouseFoodProduction : 0f)) * 60f;
        public float WaterDeltaPerMinute => (-population * waterUsePerPerson
                                             - (GreenhouseActuallyRunning ? greenhouseWaterUse : 0f)) * 60f;
        public float HeatDeltaPerMinute
        {
            get
            {
                float perSecond;
                if (LivingActuallyRunning)
                {
                    perSecond = poweredHeatGain * Mathf.Max(0.35f, PowerEfficiency);
                }
                else
                {
                    perSecond = -unpoweredHeatLoss;
                }

                if (!PowerStable)
                {
                    perSecond -= deficitHeatLoss;
                }

                return perSecond * 60f;
            }
        }

        public bool LivingActuallyRunning => livingCarPowered && EnergyCapacity > 0 && PowerEfficiency > 0.35f;
        public bool WorkshopActuallyRunning => workshopPowered && EnergyCapacity > 0 && PowerEfficiency > 0.70f;
        public bool GreenhouseActuallyRunning => greenhousePowered
                                                 && water > 0.01f
                                                 && EnergyCapacity > 0
                                                 && PowerEfficiency > 0.75f;

        public int SurvivalWarningCount
        {
            get
            {
                int warnings = 0;
                if (fuel <= 20f) warnings++;
                if (food <= 20f) warnings++;
                if (water <= 20f) warnings++;
                if (heatPercent <= 35f) warnings++;
                if (!PowerStable) warnings++;
                return warnings;
            }
        }

        public void ToggleGenerator()
        {
            if (!generatorOnline && fuel <= 0.01f)
            {
                return;
            }

            generatorOnline = !generatorOnline;
        }

        public void ToggleLivingCarPower() => livingCarPowered = !livingCarPowered;
        public void ToggleWorkshopPower() => workshopPowered = !workshopPowered;
        public void ToggleGreenhousePower() => greenhousePowered = !greenhousePowered;

        public void SimulateSecond(float deltaSeconds)
        {
            if (deltaSeconds <= 0f)
            {
                return;
            }

            fuel = Mathf.Max(0f, fuel - locomotiveFuelUse * deltaSeconds);

            if (generatorOnline)
            {
                fuel = Mathf.Max(0f, fuel - generatorFuelUse * deltaSeconds);
                if (fuel <= 0.001f)
                {
                    fuel = 0f;
                    generatorOnline = false;
                }
            }

            food = Mathf.Max(0f, food - population * foodUsePerPerson * deltaSeconds);
            water = Mathf.Max(0f, water - population * waterUsePerPerson * deltaSeconds);

            if (GreenhouseActuallyRunning)
            {
                food = Mathf.Min(200f, food + greenhouseFoodProduction * deltaSeconds);
                water = Mathf.Max(0f, water - greenhouseWaterUse * deltaSeconds);
            }

            if (LivingActuallyRunning)
            {
                heatPercent += poweredHeatGain * Mathf.Max(0.35f, PowerEfficiency) * deltaSeconds;
            }
            else
            {
                heatPercent -= unpoweredHeatLoss * deltaSeconds;
            }

            if (!PowerStable)
            {
                heatPercent -= deficitHeatLoss * deltaSeconds;
            }

            if (food <= 0.01f || water <= 0.01f)
            {
                heatPercent -= 0.015f * deltaSeconds;
            }

            heatPercent = Mathf.Clamp(heatPercent, 0f, 100f);
        }
    }
}
