using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "BreaksSO", menuName = "CustomVehicleController/Breaks")]
    public class BrakesSO : ScriptableObject
    {
        [Min(0)]
        public float BrakesStrength;
        [Min(0)]
        public float HandbrakeForce;
        [Range(0, 1f)]
        public float HandbrakeTractionPercent;
    }
}

