using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "ForcedInductionSoundSO", menuName = "CustomVehicleController/ForcedInductionSoundSO")]
    public class CarForcedInductionSoundSO : ScriptableObject
    {
        public AudioClip ForcedInductionSound;
        public AudioClip[] TurboFlutterSound;
        public AudioClip[] AntiLagSound;
        public AudioClip[] AntiLagMildSounds;
    }
}
