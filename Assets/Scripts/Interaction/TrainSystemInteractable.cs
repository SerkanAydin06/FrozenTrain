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
                        return state.GeneratorOnline ? "E - Jeneratörü Durdur" : "E - Jeneratörü Çalıştır";
                    case TrainSystemControlType.LivingPowerToggle:
                        return state.LivingCarPowered ? "E - Yaşam Gücünü Kes" : "E - Yaşam Gücünü Aç";
                    case TrainSystemControlType.WorkshopPowerToggle:
                        return state.WorkshopPowered ? "E - Atölye Gücünü Kes" : "E - Atölye Gücünü Aç";
                    case TrainSystemControlType.GreenhousePowerToggle:
                        return state.GreenhousePowered ? "E - Sera Gücünü Kes" : "E - Sera Gücünü Aç";
                    case TrainSystemControlType.CyclePace:
                        return "E - Hız Modu: " + state.PaceLabel + " → değiştir";
                    case TrainSystemControlType.UseMedicine:
                        return state.Medicine > 0.05f ? "E - 1 İlaç Kullan" : "İlaç Yok";
                    case TrainSystemControlType.RefillWaterFromReserve:
                        return state.WaterReserve > 0.1f ? "E - Su Rezervinden 8 Birim Aktar" : "Su Rezervi Boş";
                    case TrainSystemControlType.WorkshopMaintenanceBoost:
                        return state.Materials >= 2f ? "E - 2 Malzeme Harca / Hızlı Bakım" : "En Az 2 Malzeme Gerekli";
                    case TrainSystemControlType.GreenhouseNutrientFlush:
                        return state.Water >= 4f ? "E - 4 Su Harca / Sera Verimini Artır" : "En Az 4 Su Gerekli";
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
            {
                state = Object.FindFirstObjectByType<TrainPrototypeState>();
            }
        }
    }
}
