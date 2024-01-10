using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "ForcedInductionSO", menuName = "CustomVehicleController/ForcedInduction")]
    public class ForcedInductionSO : ScriptableObject
    {
        public ForcedInductionType ForcedInductionType;
        [Min(0)]
        public float MaxTorqueBoostAmount;
        [Range(0, 1f)]
        public float TurboRPMPercentDelay;
        [Min(0.1f)]
        public float TurboSpinTime;
    }
}

