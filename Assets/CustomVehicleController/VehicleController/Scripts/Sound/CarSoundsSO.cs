using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "CarSoundsSO", menuName = "CustomVehicleController/CarSoundSO")]
    public class CarSoundsSO : ScriptableObject
    {
        [Header("Array of looped engine sounds with 500 rpm step")]
        public AudioClip[] EngineRPMRangeArray;
        public AudioClip ForcedInductionSound;
        public AudioClip TurboFlutterSound;
        public AudioClip AntiLagSound;
        public AudioClip[] AntiLagMildSounds;

        public AudioClip TireSlipSound;
    }
}

