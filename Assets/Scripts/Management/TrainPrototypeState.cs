using UnityEngine;

namespace FrozenTrain.Management
{
    public enum TrainPace
    {
        Economy,
        Cruise,
        Push
    }

    /// <summary>
    /// Single source of truth for the five-car survival prototype.
    /// Pressure comes from resource flow, travel pace and power allocation.
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
        [SerializeField] private float waterReserve = 45f;
        [SerializeField] private float materials = 60f;
        [SerializeField] private float medicine = 20f;
        [SerializeField, Range(0f, 100f)] private float heatPercent = 72f;
        [SerializeField, Range(0f, 100f)] private float populationHealth = 92f;
        [SerializeField, Range(0f, 100f)] private float trainCondition = 88f;
        [SerializeField] private int population = 10;

        [Header("Travel")]
        [SerializeField] private TrainPace pace = TrainPace.Cruise;
        [SerializeField] private float distanceKm;

        [Header("Car State")]
        [SerializeField] private bool generatorOnline = true;
        [SerializeField] private bool livingCarPowered = true;
        [SerializeField] private bool workshopPowered;
        [SerializeField] private bool greenhousePowered;

        [Header("Manual system effects")]
        [SerializeField] private float greenhouseBoostSeconds;

        [Header("Balance / real second")]
        [SerializeField] private float generatorFuelUse = 0.035f;
        [SerializeField] private float foodUsePerPerson = 0.0015f;
        [SerializeField] private float waterUsePerPerson = 0.0024f;
        [SerializeField] private float greenhouseFoodProduction = 0.055f;
        [SerializeField] private float greenhouseWaterUse = 0.022f;
        [SerializeField] private float poweredHeatGain = 0.10f;
        [SerializeField] private float unpoweredHeatLoss = 0.075f;
        [SerializeField] private float deficitHeatLoss = 0.035f;
        [SerializeField] private float workshopMaterialUse = 0.008f;
        [SerializeField] private float workshopRepairRate = 0.026f;
        [SerializeField] private float snowIntakePerSecondAtCruise = 0.012f;
        [SerializeField] private float waterReserveCapacity = 120f;
        [SerializeField] private float populationLossIntervalSeconds = 45f;

        private const float CruiseSpeedKmh = 60f;
        private float populationLossTimer;
        private const string SaveKey = "FrozenTrain.RunSave.V1";

        [System.Serializable]
        private sealed class RunSnapshot
        {
            public int version = 1;
            public float fuel, food, water, waterReserve, materials, medicine;
            public float heatPercent, populationHealth, trainCondition;
            public float distanceKm, greenhouseBoostSeconds, populationLossTimer;
            public int population, pace;
            public bool generatorOnline, livingCarPowered, workshopPowered, greenhousePowered;
            public Vector3 playerPosition;
            public float playerYaw;
        }

        public bool GeneratorOnline => generatorOnline;
        public bool LivingCarPowered => livingCarPowered;
        public bool WorkshopPowered => workshopPowered;
        public bool GreenhousePowered => greenhousePowered;
        public TrainPace Pace => pace;

        public int EnergyCapacity => generatorOnline && fuel > 0.01f ? generatorCapacity : emergencyCapacity;
        public int EnergyConsumption => locomotiveConsumption
                                        + lifeSupportConsumption
                                        + (livingCarPowered ? livingCarConsumption : 0)
                                        + (workshopPowered ? workshopConsumption : 0)
                                        + (greenhousePowered ? greenhouseConsumption : 0);
        public int EnergyDeficit => Mathf.Max(0, EnergyConsumption - EnergyCapacity);
        public float PowerEfficiency => EnergyConsumption <= 0 ? 1f : Mathf.Clamp01(EnergyCapacity / (float)EnergyConsumption);
        public bool PowerStable => EnergyDeficit == 0;

        public int Fuel => Mathf.CeilToInt(fuel);
        public int HeatPercent => Mathf.RoundToInt(heatPercent);
        public int Food => Mathf.CeilToInt(food);
        public int Water => Mathf.CeilToInt(water);
        public int WaterReserve => Mathf.CeilToInt(waterReserve);
        public int Materials => Mathf.CeilToInt(materials);
        public int Medicine => Mathf.CeilToInt(medicine);
        public int Population => population;
        public int PopulationHealth => Mathf.RoundToInt(populationHealth);
        public int TrainCondition => Mathf.RoundToInt(trainCondition);
        public float DistanceKm => distanceKm;
        public bool GreenhouseBoostActive => greenhouseBoostSeconds > 0.01f;
        public int GreenhouseBoostSecondsRemaining => Mathf.CeilToInt(greenhouseBoostSeconds);

        public int TargetSpeedKmh => pace switch
        {
            TrainPace.Economy => 35,
            TrainPace.Push => 85,
            _ => 60
        };

        public bool IsLost => population <= 0;

        /// <summary>Current speed relative to cruise pace (0 = stopped, ~1.4 = push).</summary>
        public float MotionFactor => SpeedKmh / CruiseSpeedKmh;

        /// <summary>The locomotive plough feeds snow into the melter while moving; melting needs generator heat.</summary>
        public bool SnowIntakeActive => SpeedKmh > 0 && generatorOnline && waterReserve < waterReserveCapacity - 0.01f;

        public bool CanMove => generatorOnline && fuel > 0.01f && trainCondition > 1f;

        public int SpeedKmh
        {
            get
            {
                if (!CanMove) return 0;
                float conditionFactor = Mathf.Lerp(0.55f, 1f, trainCondition / 100f);
                return Mathf.RoundToInt(TargetSpeedKmh * conditionFactor);
            }
        }

        public string PaceLabel => pace switch
        {
            TrainPace.Economy => "EKONOMİ",
            TrainPace.Push => "ZORLA",
            _ => "SEYİR"
        };

        private float LocomotiveFuelUse => pace switch
        {
            TrainPace.Economy => 0.012f,
            TrainPace.Push => 0.034f,
            _ => 0.020f
        };

        private float TravelWearPerSecond => pace switch
        {
            TrainPace.Economy => 0.0025f,
            TrainPace.Push => 0.010f,
            _ => 0.005f
        };

        private float CurrentGreenhouseProduction => greenhouseFoodProduction * (GreenhouseBoostActive ? 1.65f : 1f);
        private float CurrentGreenhouseWaterUse => greenhouseWaterUse * (GreenhouseBoostActive ? 1.30f : 1f);
        // Resource consumption and production both respect actual power delivery.
        private float EffectiveGreenhouseProduction => CurrentGreenhouseProduction * PowerEfficiency;
        private float EffectiveGreenhouseWaterUse => CurrentGreenhouseWaterUse * PowerEfficiency;
        private float EffectiveWorkshopMaterialUse => workshopMaterialUse * PowerEfficiency;

        public float FuelDeltaPerMinute
        {
            get
            {
                float perSecond = generatorOnline ? generatorFuelUse : 0f;
                if (SpeedKmh > 0) perSecond += LocomotiveFuelUse;
                return -perSecond * 60f;
            }
        }

        public float FoodDeltaPerMinute => (-population * foodUsePerPerson + (GreenhouseActuallyRunning ? EffectiveGreenhouseProduction : 0f)) * 60f;
        public float WaterDeltaPerMinute => (-population * waterUsePerPerson - (GreenhouseActuallyRunning ? EffectiveGreenhouseWaterUse : 0f)) * 60f;
        public float WaterReserveDeltaPerMinute => SnowIntakeActive ? snowIntakePerSecondAtCruise * MotionFactor * 60f : 0f;
        public float MaterialsDeltaPerMinute => WorkshopActuallyRepairing ? -EffectiveWorkshopMaterialUse * 60f : 0f;
        public float HeatDeltaPerMinute
        {
            get
            {
                float perSecond = LivingActuallyRunning
                    ? poweredHeatGain * Mathf.Max(0.35f, PowerEfficiency)
                    : -unpoweredHeatLoss;
                if (!PowerStable) perSecond -= deficitHeatLoss;
                return perSecond * 60f;
            }
        }

        public bool LivingActuallyRunning => livingCarPowered && EnergyCapacity > 0 && PowerEfficiency > 0.35f;
        public bool WorkshopActuallyRunning => workshopPowered && EnergyCapacity > 0 && PowerEfficiency > 0.70f;
        public bool WorkshopActuallyRepairing => WorkshopActuallyRunning && materials > 0.01f && trainCondition < 99.9f;
        public bool GreenhouseActuallyRunning => greenhousePowered && water > 0.01f && EnergyCapacity > 0 && PowerEfficiency > 0.75f;

        public int SurvivalWarningCount
        {
            get
            {
                int warnings = 0;
                if (fuel <= 20f) warnings++;
                if (food <= 20f) warnings++;
                if (water <= 20f) warnings++;
                if (heatPercent <= 35f) warnings++;
                if (populationHealth <= 50f) warnings++;
                if (trainCondition <= 35f) warnings++;
                if (!PowerStable) warnings++;
                if (!CanMove) warnings++;
                return warnings;
            }
        }

        public void ToggleGenerator()
        {
            if (IsLost) return;
            if (!generatorOnline && fuel <= 0.01f) return;
            generatorOnline = !generatorOnline;
        }

        public void ToggleLivingCarPower() => livingCarPowered = !livingCarPowered;
        public void ToggleWorkshopPower() => workshopPowered = !workshopPowered;
        public void ToggleGreenhousePower() => greenhousePowered = !greenhousePowered;

        public void CyclePace()
        {
            pace = pace switch
            {
                TrainPace.Economy => TrainPace.Cruise,
                TrainPace.Cruise => TrainPace.Push,
                _ => TrainPace.Economy
            };
        }

        public bool UseMedicine()
        {
            if (medicine < 1f || populationHealth >= 99.5f) return false;
            medicine -= 1f;
            populationHealth = Mathf.Min(100f, populationHealth + 8f);
            return true;
        }

        public bool TransferWaterFromReserve(float requestedAmount)
        {
            if (requestedAmount <= 0f || waterReserve <= 0.01f || water >= 199.9f) return false;
            float transfer = Mathf.Min(requestedAmount, waterReserve, 200f - water);
            waterReserve -= transfer;
            water += transfer;
            return transfer > 0.01f;
        }

        public bool PerformWorkshopMaintenance()
        {
            if (!WorkshopActuallyRunning || materials < 2f || trainCondition >= 99.9f) return false;
            materials -= 2f;
            trainCondition = Mathf.Min(100f, trainCondition + 7f);
            return true;
        }

        public bool PerformGreenhouseFlush()
        {
            if (!GreenhouseActuallyRunning || water < 4f || GreenhouseBoostActive) return false;
            water -= 4f;
            greenhouseBoostSeconds = 120f;
            return true;
        }

        /// <summary>One manual save slot for dynamic run state; balance settings stay in the scene.</summary>
        public bool SaveRun(Transform player)
        {
            if (player == null || IsLost) return false;

            var data = new RunSnapshot
            {
                fuel = fuel, food = food, water = water, waterReserve = waterReserve,
                materials = materials, medicine = medicine, heatPercent = heatPercent,
                populationHealth = populationHealth, trainCondition = trainCondition,
                distanceKm = distanceKm, greenhouseBoostSeconds = greenhouseBoostSeconds,
                populationLossTimer = populationLossTimer, population = population, pace = (int)pace,
                generatorOnline = generatorOnline, livingCarPowered = livingCarPowered,
                workshopPowered = workshopPowered, greenhousePowered = greenhousePowered,
                playerPosition = player.position, playerYaw = player.eulerAngles.y
            };

            try
            {
                PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
                PlayerPrefs.Save();
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[FrozenTrain] Save failed: " + e.Message);
                return false;
            }
        }

        public bool LoadRun(Transform player)
        {
            if (player == null || !PlayerPrefs.HasKey(SaveKey)) return false;
            RunSnapshot data;
            try
            {
                data = JsonUtility.FromJson<RunSnapshot>(PlayerPrefs.GetString(SaveKey));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[FrozenTrain] Invalid save: " + e.Message);
                return false;
            }

            if (data == null || data.version != 1 || data.population < 1 ||
                !Finite(data.fuel) || !Finite(data.food) || !Finite(data.water) ||
                !Finite(data.waterReserve) || !Finite(data.materials) || !Finite(data.medicine) ||
                !Finite(data.heatPercent) || !Finite(data.populationHealth) || !Finite(data.trainCondition) ||
                !Finite(data.distanceKm) || !Finite(data.greenhouseBoostSeconds) ||
                !Finite(data.populationLossTimer) || !Finite(data.playerPosition.x) ||
                !Finite(data.playerPosition.y) || !Finite(data.playerPosition.z) ||
                !Finite(data.playerYaw))
                return false;

            fuel = Mathf.Max(0f, data.fuel);
            food = Mathf.Clamp(data.food, 0f, 200f);
            water = Mathf.Clamp(data.water, 0f, 200f);
            waterReserve = Mathf.Clamp(data.waterReserve, 0f, waterReserveCapacity);
            materials = Mathf.Max(0f, data.materials);
            medicine = Mathf.Max(0f, data.medicine);
            heatPercent = Mathf.Clamp(data.heatPercent, 0f, 100f);
            populationHealth = Mathf.Clamp(data.populationHealth, 0f, 100f);
            trainCondition = Mathf.Clamp(data.trainCondition, 0f, 100f);
            distanceKm = Mathf.Max(0f, data.distanceKm);
            greenhouseBoostSeconds = Mathf.Max(0f, data.greenhouseBoostSeconds);
            populationLossTimer = Mathf.Max(0f, data.populationLossTimer);
            population = data.population;
            pace = (TrainPace)Mathf.Clamp(data.pace, (int)TrainPace.Economy, (int)TrainPace.Push);
            generatorOnline = data.generatorOnline && fuel > 0.01f;
            livingCarPowered = data.livingCarPowered;
            workshopPowered = data.workshopPowered;
            greenhousePowered = data.greenhousePowered;

            CharacterController controller = player.GetComponent<CharacterController>();
            bool wasEnabled = controller != null && controller.enabled;
            if (wasEnabled) controller.enabled = false;
            player.SetPositionAndRotation(data.playerPosition, Quaternion.Euler(0f, data.playerYaw, 0f));
            Physics.SyncTransforms();
            if (wasEnabled) controller.enabled = true;
            return true;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        public void SimulateSecond(float deltaSeconds)
        {
            if (deltaSeconds <= 0f || IsLost) return;

            if (greenhouseBoostSeconds > 0f)
                greenhouseBoostSeconds = Mathf.Max(0f, greenhouseBoostSeconds - deltaSeconds);

            // Sample motion before fuel is spent so this step stays internally consistent.
            float snowIntake = SnowIntakeActive ? snowIntakePerSecondAtCruise * MotionFactor * deltaSeconds : 0f;

            if (SpeedKmh > 0)
            {
                distanceKm += (SpeedKmh / 3600f) * deltaSeconds;
                fuel = Mathf.Max(0f, fuel - LocomotiveFuelUse * deltaSeconds);
                trainCondition = Mathf.Max(0f, trainCondition - TravelWearPerSecond * deltaSeconds);
            }

            if (generatorOnline)
            {
                fuel = Mathf.Max(0f, fuel - generatorFuelUse * deltaSeconds);
                if (fuel <= 0.001f)
                {
                    fuel = 0f;
                    generatorOnline = false;
                }
            }

            waterReserve = Mathf.Min(waterReserveCapacity, waterReserve + snowIntake);

            food = Mathf.Max(0f, food - population * foodUsePerPerson * deltaSeconds);
            water = Mathf.Max(0f, water - population * waterUsePerPerson * deltaSeconds);

            if (GreenhouseActuallyRunning)
            {
                food = Mathf.Min(200f, food + EffectiveGreenhouseProduction * deltaSeconds);
                water = Mathf.Max(0f, water - EffectiveGreenhouseWaterUse * deltaSeconds);
            }

            if (WorkshopActuallyRepairing)
            {
                materials = Mathf.Max(0f, materials - EffectiveWorkshopMaterialUse * deltaSeconds);
                trainCondition = Mathf.Min(100f, trainCondition + workshopRepairRate * PowerEfficiency * deltaSeconds);
            }

            if (LivingActuallyRunning)
                heatPercent += poweredHeatGain * Mathf.Max(0.35f, PowerEfficiency) * deltaSeconds;
            else
                heatPercent -= unpoweredHeatLoss * deltaSeconds;

            if (!PowerStable) heatPercent -= deficitHeatLoss * deltaSeconds;
            heatPercent = Mathf.Clamp(heatPercent, 0f, 100f);

            float healthDelta = 0f;
            if (heatPercent < 35f) healthDelta -= (35f - heatPercent) * 0.0012f;
            if (food <= 0.01f) healthDelta -= 0.025f;
            if (water <= 0.01f) healthDelta -= 0.045f;
            if (heatPercent >= 55f && food > 10f && water > 10f && LivingActuallyRunning) healthDelta += 0.004f;
            populationHealth = Mathf.Clamp(populationHealth + healthDelta * deltaSeconds, 0f, 100f);

            // Collapsed health costs lives; recovering above zero resets the countdown.
            if (populationHealth <= 0.5f)
            {
                populationLossTimer += deltaSeconds;
                if (populationLossTimer >= populationLossIntervalSeconds)
                {
                    populationLossTimer = 0f;
                    population = Mathf.Max(0, population - 1);
                    if (IsLost)
                    {
                        generatorOnline = false;
                    }
                }
            }
            else
            {
                populationLossTimer = 0f;
            }
        }
    }
}
