using FrozenTrain.Management;
using UnityEngine;

namespace FrozenTrain.Interaction
{
    public enum TrainSystemControlType
    {
        GeneratorToggle,
        LivingPowerToggle,
        WorkshopPowerToggle,
        GreenhousePowerToggle,
        CyclePace,
        UseMedicine,
        RefillWaterFromReserve,
        WorkshopMaintenanceBoost,
        GreenhouseNutrientFlush
    }

    public sealed class TrainSystemInteractable : InteractableBase
    {
        [SerializeField] private TrainSystemControlType controlType;
        [SerializeField] private TrainPrototypeState state;

        public void Configure(TrainPrototypeState trainState, TrainSystemControlType type)
        {
            state = trainState;
            controlType = type;
        }

        public override string InteractionText
        {
            get
            {
                ResolveState();
                if (state == null) return "Sistem çevrimdışı";

                switch (controlType)
                {
                    case TrainSystemControlType.GeneratorToggle:
                        return (state.GeneratorOnline ? "E - Jeneratörü Durdur" : "E - Jeneratörü Çalıştır") +
                               $"  // Yakıt {state.Fuel}";
                    case TrainSystemControlType.LivingPowerToggle:
                        return (state.LivingCarPowered ? "E - Yaşam Gücünü Kes" : "E - Yaşam Gücünü Aç") +
                               $"  // Isı %{state.HeatPercent}";
                    case TrainSystemControlType.WorkshopPowerToggle:
                        return (state.WorkshopPowered ? "E - Atölye Gücünü Kes" : "E - Atölye Gücünü Aç") +
                               $"  // Kondisyon %{state.TrainCondition}";
                    case TrainSystemControlType.GreenhousePowerToggle:
                        return (state.GreenhousePowered ? "E - Sera Gücünü Kes" : "E - Sera Gücünü Aç") +
                               $"  // Su {state.Water}";
                    case TrainSystemControlType.CyclePace:
                        return state.CanMove
                            ? $"E - Hız Modu: {state.PaceLabel} → değiştir  // {state.SpeedKmh} km/h"
                            : $"E - Hız Modu: {state.PaceLabel} → değiştir  // TREN DURUYOR";
                    case TrainSystemControlType.UseMedicine:
                        return state.Medicine > 0
                            ? $"E - 1 İlaç Kullan  // İlaç {state.Medicine} • Sağlık %{state.PopulationHealth}"
                            : $"İlaç Yok  // Sağlık %{state.PopulationHealth}";
                    case TrainSystemControlType.RefillWaterFromReserve:
                        return state.WaterReserve > 0
                            ? $"E - Yedek Sudan 8 Birim Aktar  // Ana {state.Water} • Rezerv {state.WaterReserve}"
                            : $"Yedek Su Boş  // Ana {state.Water}";
                    case TrainSystemControlType.WorkshopMaintenanceBoost:
                        if (!state.WorkshopActuallyRunning)
                            return $"Hızlı Bakım İçin Atölyeyi Çalıştır  // Kondisyon %{state.TrainCondition}";
                        return state.Materials >= 2
                            ? $"E - 2 Malzeme Harca / Hızlı Bakım  // Kondisyon %{state.TrainCondition} • Malzeme {state.Materials}"
                            : $"En Az 2 Malzeme Gerekli  // Kondisyon %{state.TrainCondition}";
                    case TrainSystemControlType.GreenhouseNutrientFlush:
                        if (state.GreenhouseBoostActive)
                            return $"Sera Verim Takviyesi Aktif  // {state.GreenhouseBoostSecondsRemaining} sn";
                        if (!state.GreenhouseActuallyRunning)
                            return $"Verim Takviyesi İçin Serayı Çalıştır  // Su {state.Water}";
                        return state.Water >= 4
                            ? $"E - 4 Su Harca / 120 sn Verim Takviyesi  // Su {state.Water}"
                            : $"En Az 4 Su Gerekli  // Su {state.Water}";
                    default:
                        return "E - Kullan";
                }
            }
        }

        public override void Interact(GameObject interactor)
        {
            ResolveState();
            if (state == null) return;

            switch (controlType)
            {
                case TrainSystemControlType.GeneratorToggle:
                    state.ToggleGenerator();
                    break;
                case TrainSystemControlType.LivingPowerToggle:
                    state.ToggleLivingCarPower();
                    break;
                case TrainSystemControlType.WorkshopPowerToggle:
                    state.ToggleWorkshopPower();
                    break;
                case TrainSystemControlType.GreenhousePowerToggle:
                    state.ToggleGreenhousePower();
                    break;
                case TrainSystemControlType.CyclePace:
                    state.CyclePace();
                    break;
                case TrainSystemControlType.UseMedicine:
                    state.UseMedicine();
                    break;
                case TrainSystemControlType.RefillWaterFromReserve:
                    state.TransferWaterFromReserve(8f);
                    break;
                case TrainSystemControlType.WorkshopMaintenanceBoost:
                    state.PerformWorkshopMaintenance();
                    break;
                case TrainSystemControlType.GreenhouseNutrientFlush:
                    state.PerformGreenhouseFlush();
                    break;
            }
        }

        private void ResolveState()
        {
            if (state == null)
                state = Object.FindFirstObjectByType<TrainPrototypeState>();
        }
    }
}
