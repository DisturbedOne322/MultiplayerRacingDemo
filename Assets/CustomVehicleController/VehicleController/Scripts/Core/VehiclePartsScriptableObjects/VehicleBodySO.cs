using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "VehicleBodySO", menuName = "CustomVehicleController/VehicleBody")]
    public class VehicleBodySO : ScriptableObject
    {
        [Min(0)]
        public float Mass;
        [Min(0)]
        public float ForwardDrag;
        [Min(0)]
        public float Downforce;
        [Min(0)]
        public float CorneringResistanceStrength;
        public AnimationCurve CorneringResistanceCurve;

    }

}
