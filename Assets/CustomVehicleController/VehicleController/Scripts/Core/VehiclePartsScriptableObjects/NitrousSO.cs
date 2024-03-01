using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "NitrousSO", menuName = "CustomVehicleController/VehicleParts/Nitrous")]
    public class NitrousSO : ScriptableObject, IVehiclePart
    {
        [Min(0)]
        public float BoostAmount;
        [Min(1)]
        public int BottlesAmount;
        [Min(0)]
        public float BoostIntensity;
        [Min(0)]
        public float BoostWarmUpTime;
        [Min(0)]
        public float RechargeRate;
        [Min(0)]
        public float RechargeDelay;
        [Range(0f, 1f)]
        public float MinAmountPercentToUse;
        public NitroBoostType BoostType;

        public static NitrousSO CreateDefaultNitroSO()
        {
            NitrousSO defaultNitroSO = ScriptableObject.CreateInstance<NitrousSO>();
            defaultNitroSO.BoostAmount = 3000;
            defaultNitroSO.BottlesAmount = 3;
            defaultNitroSO.BoostIntensity = 1500;
            defaultNitroSO.BoostWarmUpTime = 0.5f;
            defaultNitroSO.RechargeRate = 1000;
            defaultNitroSO.RechargeDelay = 2;
            defaultNitroSO.MinAmountPercentToUse = 0;
            defaultNitroSO.BoostType = NitroBoostType.Continuous;

            defaultNitroSO.name = "DefaultNitrosu";

            return defaultNitroSO;
        }
    }
}
