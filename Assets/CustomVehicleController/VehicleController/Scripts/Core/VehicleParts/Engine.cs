using UnityEngine;

namespace Assets.VehicleController
{
    public class Engine : IEngine
    {
        private VehiclePartsSetWrapper _partsPresetWrapper;

        private ForcedInduction _forcedInduction;
        private NitrousBoost _nitrousBoost;

        private IShifter _shifter;
        private ITransmission _transmission;

        private CurrentCarStats _currentCarStats;

        private float _totalTorque;

        public void Initialize(CurrentCarStats currentCarStats, VehiclePartsSetWrapper partsPresetWrapper, IShifter shifter, ITransmission transmission)
        {
            _partsPresetWrapper = partsPresetWrapper;
            _currentCarStats = currentCarStats;
            _shifter = shifter;
            _transmission = transmission;

            _forcedInduction = new(_partsPresetWrapper, _currentCarStats, _transmission);
            _nitrousBoost = new(_partsPresetWrapper, _currentCarStats);
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

        private float CalculateAccelerationForce(float input, float rpm, float boost)
        {
            if (input == 0 && _currentCarStats.AllWheelsGrounded)
            {
                //engine braking;
                float sign = (_shifter.InReverseGear() && _currentCarStats.SpeedInMsPerS < 0) || _currentCarStats.SpeedInMsPerS < 0 ? -1 : 1;
                float engineBrakingMultiplierFromGear = _partsPresetWrapper.Transmission.GearRatiosList[_shifter.GetCurrentGearID()];
                return -CalculateTorque(rpm, boost) * engineBrakingMultiplierFromGear * sign * 0.1f * _currentCarStats.EngineRPMPercent;
            }

            if (_transmission.Redlining())
            {
                float mult = (_shifter.InReverseGear() && _currentCarStats.SpeedInMsPerS < 0) || _currentCarStats.SpeedInMsPerS < 0 ? -1 : 1;
                return -CalculateTorque(rpm, boost) * mult * Mathf.Abs(input);
            }

            if (_partsPresetWrapper.Engine.MaxSpeed < _currentCarStats.SpeedInKMperH)
                return -CalculateTorque(rpm, boost) * input;

            if (_transmission.InShiftingCooldown())
                return 0;

            return CalculateTorque(rpm, boost) * input;
        }

        private float CalculateTorque(float rpm, float boost)
        {
            return (_partsPresetWrapper.Engine.TorqueCurve.Evaluate(rpm) + boost) * _partsPresetWrapper.Transmission.GearRatiosList[_shifter.GetCurrentGearID()] *
                _partsPresetWrapper.Transmission.FinalDriveRatio;
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

        public void AddNitro(float amount) => _nitrousBoost.AddNitro(amount);
    }
}
