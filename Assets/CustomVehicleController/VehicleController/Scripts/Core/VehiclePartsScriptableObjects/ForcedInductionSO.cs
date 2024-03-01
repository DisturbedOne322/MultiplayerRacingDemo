using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "ForcedInductionSO", menuName = "CustomVehicleController/VehicleParts/ForcedInduction")]
    public class ForcedInductionSO : ScriptableObject, IVehiclePart
    {
        public ForcedInductionType ForcedInductionType;
        [Min(0)]
        public float MaxTorqueBoostAmount;
        [Range(0, 1f)]
        public float TurboRPMPercentDelay;
        [Min(0.1f)]
        public float TurboSpinTime;

        public static ForcedInductionSO CreateDefaultForcedInductionSO()
        {
            ForcedInductionSO defaultFISO = ScriptableObject.CreateInstance<ForcedInductionSO>();
            defaultFISO.ForcedInductionType = ForcedInductionType.Turbocharger;
            defaultFISO.MaxTorqueBoostAmount = 300f;
            defaultFISO.TurboRPMPercentDelay = 0.2f;
            defaultFISO.TurboSpinTime = 0.5f;

            defaultFISO.name = "DefaultForcedInduction";

            return defaultFISO;
        }
    }
}