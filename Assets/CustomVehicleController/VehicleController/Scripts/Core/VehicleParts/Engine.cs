using UnityEngine;

namespace Assets.VehicleController
{
    public class Engine : IEngine
    {
        private VehicleStats _stats;

        private ForcedInduction _forcedInduction;
        private NitrousBoost _nitrousBoost;

        private IShifter _shifter;
        private ITransmission _transmission;

        private CurrentCarStats _currentCarStats;

        private float _totalTorque;

        public void Initialize(CurrentCarStats currentCarStats, VehicleStats stats, IShifter shifter, ITransmission transmission)
        {
            _stats = stats;
            _currentCarStats = currentCarStats;
            _shifter = shifter;
            _transmission = transmission;

            _forcedInduction = new(_stats, _currentCarStats, _transmission);
            _nitrousBoost = new(_stats, _currentCarStats);
        }

        public void Accelerate(VehicleAxle[] driveAxleArray, float gasInput, float breakInput, bool nitroBoostInput, float rpm)
        {
            float input = _transmission.DetermineGasInput(gasInput, breakInput);
            float forcedInductionBoost = _forcedInduction.GetForcedInductionBoost(_transmission.InShiftingCooldown() ? 0 : Mathf.Abs(input));
            float nitroBoost = _nitrousBoost.GetNitroBoost(nitroBoostInput);
            float boost = nitroBoost + forcedInductionBoost;

            _totalTorque = CalculateAccelerationForce(input, rpm, boost);
            SetTorque(_totalTorque, driveAxleArray);
        }

        public float CalculateAccelerationForce(float input, float rpm, float boost)
        {
            if (_transmission.Redlining())
            {
                float mult = (_shifter.InReverseGear() && _currentCarStats.SpeedInMsPerS < 0) || _currentCarStats.SpeedInMsPerS < 0 ? -1 : 1;
                return -CalculateTorque(rpm, boost) * mult;
            }

            if (_stats.EngineSO.MaxSpeed < _currentCarStats.SpeedInKMperH)
                return -CalculateTorque(rpm, boost);

            if (_transmission.InShiftingCooldown())
                return 0;

            return CalculateTorque(rpm, boost) * input;
        }

        private float CalculateTorque(float rpm, float boost)
        {
            return (_stats.EngineSO.TorqueCurve.Evaluate(rpm) + boost) * _stats.TransmissionSO.GearRatiosList[_shifter.GetCurrentGearID()] *
                _stats.TransmissionSO.FinalDriveRatio;
        }

        private void SetTorque(float torque, VehicleAxle[] driveAxleArray)
        {
            int size = driveAxleArray.Length;
            float torqueToApply = torque / (size * 2);
            for (int i = 0; i < size; i++)
            {
                driveAxleArray[i].ApplyTorque(torqueToApply / driveAxleArray[i].LeftHalfShaft.WheelController.WheelRadius);
            }
        }

        public float GetCurrentTorque() => Mathf.Abs(_totalTorque);
        public float GetForcedInductionBoostPercent() => _forcedInduction.GetForcedInductionBoostPercent();
    }
}
