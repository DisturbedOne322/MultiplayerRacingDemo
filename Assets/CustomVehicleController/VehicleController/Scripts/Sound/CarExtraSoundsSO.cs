using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "ExtraSoundsSO", menuName = "CustomVehicleController/ExtraSoundsSO")]
    public class CarExtraSoundsSO : ScriptableObject
    {
        public AudioClip TireSlipSound;
        public AudioClip WindNoise;
        public AudioClip CollisionImpact;
        public AudioClip CollisionContinuous;
        public AudioClip NitroStart;
        public AudioClip NitroContinuous;
    }
}

