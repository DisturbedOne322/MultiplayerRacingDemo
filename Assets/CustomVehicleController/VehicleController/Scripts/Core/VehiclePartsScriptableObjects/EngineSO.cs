using System;
using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "EngineSO", menuName = "CustomVehicleController/VehicleParts/Engine")]
    public class EngineSO : ScriptableObject, IVehiclePart
    {
        public AnimationCurve TorqueCurve;

        public ForcedInductionSO ForcedInductionSO;

        public float MinRPM
        {
            get { return FindMinRpm(); }
        }
        public float MaxRPM
        {
            get { return FindMaxRpm(); }
        }
        [Min(0)]
        public float MaxSpeed;

        private float FindMinRpm()
        {
            Keyframe[] keys = TorqueCurve.keys;
            return keys[0].time;
        }

        private float FindMaxRpm()
        {
            Keyframe[] keys = TorqueCurve.keys;
            return keys[keys.Length - 1].time;
        }

        public float FindMaxTorque()
        {
            float maxEngineRPM = FindMaxRpm();

            float maxTorque = float.MinValue;
            float step = maxEngineRPM / 1000;

            for (float t = FindMinRpm(); t < maxEngineRPM; t += step)
            {
                float value = TorqueCurve.Evaluate(t);
                if (value > maxTorque)
                {
                    maxTorque = value;
                }
            }

            return maxTorque;
        }

        public float FindPeakTorqueRPM()
        {
            float maxEngineRPM = FindMaxRpm();

            float maxTorque = float.MinValue;
            float step = maxEngineRPM / 1000;

            float maxTorqueRPM = 0;

            for (float t = FindMinRpm(); t < maxEngineRPM; t += step)
            {
                float value = TorqueCurve.Evaluate(t);
                if (value > maxTorque)
                {
                    maxTorque = value;
                    maxTorqueRPM = t;
                }
            }

            return maxTorqueRPM;
        }

        public float FindMaxHP()
        {
            // Ensure there are at least two keys in the curve
            if (TorqueCurve.keys.Length < 2)
            {
                Debug.LogWarning("Torque curve must have at least two keys.");
                return float.MinValue;
            }



            float maxTorque = FindMaxTorque();
            float maxTorqueRPM = FindPeakTorqueRPM();
            float maxEngineRPM = FindMaxRpm();

            float boost = 0;
            if (ForcedInductionSO != null)
            {
                switch (ForcedInductionSO.ForcedInductionType)
                {
                    case ForcedInductionType.Centrifugal:
                        boost = ForcedInductionSO.MaxTorqueBoostAmount * (maxTorqueRPM / maxEngineRPM);
                        break;
                    //for simplicity turbo will be considered giving maximum boost
                    case ForcedInductionType.Turbocharger:
                    case ForcedInductionType.Supercharger:
                        boost = ForcedInductionSO.MaxTorqueBoostAmount;
                        break;
                    default:
                        boost = 0;
                        break;
                }
            }


            return maxTorque * maxTorqueRPM / 5252 + boost;
        }

        public static EngineSO CreateDefaultEngineSO()
        {
            EngineSO defaultEngine = ScriptableObject.CreateInstance<EngineSO>();
            defaultEngine.MaxSpeed = 300;
            AnimationCurve torqueCurve = new();
            torqueCurve.AddKey(1000f, 300);
            torqueCurve.AddKey(8000f, 450);
            torqueCurve.AddKey(9000f, 400);
            defaultEngine.TorqueCurve = torqueCurve;
            defaultEngine.name = "DefaultEngine";

            return defaultEngine;
        }

        public event Action OnEngineStatsChanged;

        private void OnValidate()
        {
            OnEngineStatsChanged?.Invoke();
        }
    }
}

