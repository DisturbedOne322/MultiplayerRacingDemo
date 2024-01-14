using System;
using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "SuspensionSO", menuName = "CustomVehicleController/Suspension")]
    public class SuspensionSO : ScriptableObject
    {
        [Range(0, 2f)]
        public float SpringRestDistance;
        [Min(0)]
        public float SpringStiffness;
        [Min(0)]
        public float SpringDampingStiffness;
        [Min(0)]
        public float AntiRollForce;

#if UNITY_EDITOR
        public event Action OnSuspensionStatsChanged;

        private void OnValidate()
        {
            OnSuspensionStatsChanged?.Invoke();
        }
#endif
    }
}

