using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "CarSoundsSO", menuName = "CustomVehicleController/CarSoundSO")]
    public class CarExtraSoundsSO : ScriptableObject
    {
        public AudioClip ForcedInductionSound;
        public AudioClip TurboFlutterSound;
        public AudioClip AntiLagSound;
        public AudioClip[] AntiLagMildSounds;

        public AudioClip TireSlipSound;
    }
}

