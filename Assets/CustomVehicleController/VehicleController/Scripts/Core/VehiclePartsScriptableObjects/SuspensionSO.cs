using System;
using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "SuspensionSO", menuName = "CustomVehicleController/VehicleParts/Suspension")]
    public class SuspensionSO : ScriptableObject, IVehiclePart
    {
        [Range(0, 2f)]
        public float SpringRestDistance;
        [Min(0)]
        public float SpringTravelLength;
        [Min(0)]
        public float SpringStiffness;
        [Min(0)]
        public float SpringDampingStiffness;
        [Min(0)]
        public float AntiRollForce;

        public static SuspensionSO CreateDefaultSuspensionSO()
        {
            SuspensionSO defaultSuspension = ScriptableObject.CreateInstance<SuspensionSO>();
            defaultSuspension.SpringStiffness = 90000f;
            defaultSuspension.SpringDampingStiffness = 2000;
            defaultSuspension.SpringRestDistance = 0.37f;
            defaultSuspension.AntiRollForce = 45000f;
            defaultSuspension.SpringTravelLength = 0.2f;
            defaultSuspension.name = "DefaultSuspension";

            return defaultSuspension;
        }

        public event Action OnSuspensionStatsChanged;

        private void OnValidate()
        {
            OnSuspensionStatsChanged?.Invoke();
        }
    }
}