using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "TiresSO", menuName = "CustomVehicleController/Tires")]
    public class TiresSO : ScriptableObject
    {
        [Min(0)]
        public float SteeringStiffness;
        public AnimationCurve SidewaysGripCurve;
        public AnimationCurve SidewaysSlipCurve;
        [Min(0)]
        public float ForwardGrip;
    }
}

