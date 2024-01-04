using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "ExtraSoundsSO", menuName = "CustomVehicleController/ExtraSoundsSO")]
    public class CarExtraSoundsSO : ScriptableObject
    {
        public AudioClip ForcedInductionSound;
        public AudioClip[] TurboFlutterSound;
        public AudioClip[] AntiLagSound;
        public AudioClip[] AntiLagMildSounds;

        public AudioClip TireSlipSound;
        public AudioClip WindNoise;
    }
}

