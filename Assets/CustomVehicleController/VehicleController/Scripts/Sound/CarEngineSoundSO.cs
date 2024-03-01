using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "EngineSoundSO", menuName = "CustomVehicleController/Sound/EngineSoundSO")]
    public class CarEngineSoundSO : ScriptableObject
    {
        [Min(0), Header("A difference between engine rpm of the recorded clips.")]
        public float RPMStep;
        [Header("Array of looped engine sounds with some rpm step.")]
        public AudioClip[] EngineRPMRangeArray;
    }
}
