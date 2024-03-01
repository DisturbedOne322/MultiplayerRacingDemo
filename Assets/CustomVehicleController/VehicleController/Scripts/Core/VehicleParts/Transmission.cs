using System;
using UnityEngine;

namespace Assets.VehicleController
{
    public class Transmission : ITransmission
    {
        public event Action OnShifted;

        private TransmissionType _transmissionType;

        private CurrentCarStats _currentCarStats;
        private VehiclePartsSetWrapper _partsPresetWrapper;
        private IShifter _shifter;

        private float _downShiftRPM;
        private float _upShiftRPM;

        private float _lastShiftTime;

        private bool _redlining = false;

        private bool _inCooldown;

        private float _minRPM;
        private float _maxRPM;

        private float _currentEngineRPM = 0;
        private float smDampVelocity;
        private const float SM_DAMP_SPEED = 0.15f;

        private EngineSO _engineSO;
        private TransmissionSO _transmissionSO;

        public void Initialize(VehiclePartsSetWrapper partsPresetWrapper, CurrentCarStats currentCarStats, IShifter shifter)
        {
            _currentCarStats = currentCarStats;
            _partsPresetWrapper = partsPresetWrapper;
            _shifter = shifter;

            _lastShiftTime = Time.time;

            _minRPM = _partsPresetWrapper.Engine.MinRPM;
            _maxRPM = _partsPresetWrapper.Engine.MaxRPM;

            _upShiftRPM = _maxRPM * _partsPresetWrapper.Transmission.UpShiftRPMPercent;
            _downShiftRPM = _maxRPM * _partsPresetWrapper.Transmission.DownShiftRPMPercent;

            //this adds support to field changes during runtime
            _transmissionSO = _partsPresetWrapper.Transmission;
            _engineSO = _partsPresetWrapper.Engine;
            _transmissionSO.OnTransmissionStatsChanged += OnStatsChanged;
            _engineSO.OnEngineStatsChanged += OnStatsChanged;
            _partsPresetWrapper.OnPartsChanged += _stats_OnPresetChanged;
        }

        private void OnStatsChanged()
        {
            UpdateRPMValues();
        }

        private void UpdateRPMValues()
        {
            _minRPM = _engineSO.MinRPM;
            _maxRPM = _engineSO.MaxRPM;
            _upShiftRPM = _maxRPM * _transmissionSO.UpShiftRPMPercent;
            _downShiftRPM = _maxRPM * _transmissionSO.DownShiftRPMPercent;
        }

        private void _stats_OnPresetChanged()
        {
            _transmissionSO.OnTransmissionStatsChanged -= OnStatsChanged;
            _engineSO.OnEngineStatsChanged -= OnStatsChanged;

            _transmissionSO = _partsPresetWrapper.Transmission;
            _engineSO = _partsPresetWrapper.Engine;

            _transmissionSO.OnTransmissionStatsChanged += OnStatsChanged;
            _engineSO.OnEngineStatsChanged += OnStatsChanged;
            UpdateRPMValues();
        }

        public void HandleGearChanges(TransmissionType transmissionType, VehicleAxle[] axleArray)
        {
            if (_inCooldown)
                return;

            _transmissionType = transmissionType;

            float wheelRPM = 0;

            int size = axleArray.Length;
            for (int i = 0; i < size; i++)
            {
                wheelRPM += axleArray[i].LeftHalfShaft.WheelController.WheelRPM;
                wheelRPM += axleArray[i].RightHalfShaft.WheelController.WheelRPM;
            }


            wheelRPM /= size * 2;

            float temp = Mathf.Abs(wheelRPM) * _partsPresetWrapper.Transmission.FinalDriveRatio * 60 / 6.28f;

            float RPMfromSpeed = CalculateRealRPM(temp);
            (float imaginaryRPM, int gearDown) = CalculateImaginaryRPMAndGearSkip(temp);

            _redlining = RPMfromSpeed == _maxRPM;

            if (transmissionType != TransmissionType.Automatic)
                return;

            if (_currentCarStats.InAir)
                return;

            SwitchGearsAutomatically(RPMfromSpeed, imaginaryRPM, gearDown);
        }

        public float EvaluateRPM(float gasInput, VehicleAxle[] driveAxleArray)
        {
            float highestRPM = 0;
            int size = driveAxleArray.Length;

            for (int i = 0; i < size; i++)
            {
                if (Mathf.Abs(driveAxleArray[i].LeftHalfShaft.WheelController.VisualRPM) > highestRPM)
                    highestRPM = Mathf.Abs(driveAxleArray[i].LeftHalfShaft.WheelController.VisualRPM);

                if (Mathf.Abs(driveAxleArray[i].RightHalfShaft.WheelController.VisualRPM) > highestRPM)
                    highestRPM = Mathf.Abs(driveAxleArray[i].RightHalfShaft.WheelController.VisualRPM);
            }

            float imaginaryEngineRPM = CalculateRealRPM(Mathf.Abs(highestRPM) * _partsPresetWrapper.Transmission.FinalDriveRatio * 60 / 6.28f);

            _currentEngineRPM = Mathf.SmoothDamp(_currentEngineRPM, imaginaryEngineRPM, ref smDampVelocity, SM_DAMP_SPEED);

            _inCooldown = Time.time < _lastShiftTime + _partsPresetWrapper.Transmission.ShiftCooldown;
            PerformRedliningEffect(gasInput);

            return _currentEngineRPM;
        }

        private float CalculateRealRPM(float temp)
        {
            float nextRPM = temp * _partsPresetWrapper.Transmission.GearRatiosList[_shifter.GetCurrentGearID()];
            return Mathf.Clamp(nextRPM, _minRPM, _maxRPM);
        }

        private (float, int) CalculateImaginaryRPMAndGearSkip(float temp)
        {
            int currentGear = _shifter.GetCurrentGearID();

            int gearSkip = -1;

            int bestGear = Mathf.Clamp(currentGear - 1, 0, _shifter.GetGearAmount());
            float bestRPM = Mathf.Clamp(temp * _partsPresetWrapper.Transmission.GearRatiosList[bestGear],
                _minRPM, _maxRPM);

            //find the gear that will give us the highest possible RPM when downshifting.
            //for example, in case of high speed crash, the best one would be the first gear, without the need to downshift multiple times
            for (int i = bestGear - 1; i >= 0; i--)
            {
                float imaginaryRPM = temp * _partsPresetWrapper.Transmission.GearRatiosList[i];

                if (imaginaryRPM > _maxRPM)
                    break;

                if (imaginaryRPM < _downShiftRPM)
                {
                    bestRPM = imaginaryRPM;
                    gearSkip = i - currentGear;
                }
            }
            return (bestRPM, gearSkip);
        }

        private void SwitchGearsAutomatically(float rpmFromSpeed, float imaginaryRPM, int gearDown)
        {
            if (_inCooldown)
                return;

            if (_currentCarStats.Reversing && _currentCarStats.Accelerating)
            {
                ShiftGear(-1);
                return;
            }

            TryUpShift(rpmFromSpeed);
            TryDownShift(imaginaryRPM, gearDown);
        }

        private void PerformRedliningEffect(float gasInput)
        {
            if (_currentEngineRPM < _maxRPM * 0.99f)
                return;

            _currentEngineRPM -= _currentCarStats.CurrentEngineHorsepower * gasInput * UnityEngine.Random.Range(1f, 2f) * Time.deltaTime;
        }

        private void TryUpShift(float currentRPM)
        {
            if (!_currentCarStats.Accelerating)
                return;

            if (_shifter.InReverseGear())
                ShiftGear(+1);

            if (currentRPM > _upShiftRPM)
                ShiftGear(+1);
        }

        private void TryDownShift(float imaginaryRPM, int gearDown)
        {
            if (imaginaryRPM < _downShiftRPM && _shifter.GetCurrentGearID() > 0)
                ShiftGear(gearDown);
        }

        public void ShiftUpManually() => ShiftGear(+1);

        public void ShiftDownManually() => ShiftGear(-1);

        public void ShiftGear(int i)
        {
            if (_inCooldown)
                return;

            if (!_shifter.TryChangeGear(i, _partsPresetWrapper.Transmission.ShiftCooldown))
                return;

            _lastShiftTime = Time.time;
            OnShifted?.Invoke();
        }

        public bool InShiftingCooldown() => _inCooldown;

        public bool Redlining() => _redlining;

        public float DetermineGasInput(float gasInput, float brakeInput)
        {
            if (_transmissionType == TransmissionType.Automatic)
                if (_currentCarStats.SpeedInMsPerS > 1)
                    return gasInput;
                else
                {
                    return brakeInput > gasInput ? -brakeInput : gasInput;
                }


            return _shifter.InReverseGear() ? -gasInput : gasInput;
        }

        public float DetermineBrakeInput(float gasInput, float brakeInput)
        {
            if (_transmissionType == TransmissionType.Automatic)
                return _currentCarStats.Reversing ? gasInput : brakeInput;

            return brakeInput;
        }
    }
}
