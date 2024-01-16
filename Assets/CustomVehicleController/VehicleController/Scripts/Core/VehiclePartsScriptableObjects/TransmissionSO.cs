using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "TransmissionSO", menuName = "CustomVehicleController/Transmission")]
    public class TransmissionSO : ScriptableObject
    {
        public List<float> GearRatiosList;
        [Min(0.1f)]
        public float FinalDriveRatio;
        [Min(0f)]
        public float ShiftCooldown;
        [Range(0f, 0.99f)]
        public float UpShiftRPMPercent;
        [Range(0f, 0.94f)]
        public float DownShiftRPMPercent;

#if UNITY_EDITOR
        public event Action OnTransmissionStatsChanged;

        private void OnValidate()
        {
            OnTransmissionStatsChanged?.Invoke();
        }
#endif
    }
}

