using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "NitrousSO", menuName = "CustomVehicleController/Nitrous")]
    public class NitrousSO : ScriptableObject
    {
        [Min(0)]
        public float BoostAmount;
        [Min(0)]
        public float BoostIntensity;
        [Min(0)]
        public float RechargeRate;
        [Min(0)]
        public float RechargeDelay;
        [Range(0f, 1f)]
        public float MinAmountPercentToUse;
        public NitroBoostType BoostType;
    }
    public enum NitroBoostType
    {
        Continuous,
        OneShot,
    }
}
