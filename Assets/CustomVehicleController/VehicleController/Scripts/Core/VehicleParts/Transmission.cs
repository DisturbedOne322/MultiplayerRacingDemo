using System;
using UnityEngine;

namespace Assets.VehicleController
{
    public class Transmission : ITransmission
    {
        public event Action OnShifted;

        private TransmissionType _transmissionType;

        private CurrentCarStats _currentCarStats;
        private VehicleStats _stats;
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

#if UNITY_EDITOR
        private EngineSO _engineSO;
        private TransmissionSO _transmissionSO;
#endif

        public void Initialize(VehicleStats stats, CurrentCarStats currentCarStats, IShifter shifter)
        {
            if (stats.TransmissionSO.GearRatiosList.Count == 0)
                Debug.LogError("Car has no gears. \n Add gear to the appropriate scriptable object");

            _currentCarStats = currentCarStats;
            _stats = stats;
            _shifter = shifter;

            _lastShiftTime = Time.time;

            _minRPM = _stats.EngineSO.MinRPM;
            _maxRPM = _stats.EngineSO.MaxRPM;

            _upShiftRPM = _maxRPM * _stats.TransmissionSO.UpShiftRPMPercent;
            _downShiftRPM = _maxRPM * _stats.TransmissionSO.DownShiftRPMPercent;

            //this adds support to field changes during runtime
#if UNITY_EDITOR
            _transmissionSO = _stats.TransmissionSO;
            _engineSO = _stats.EngineSO;
            _transmissionSO.OnTransmissionStatsChanged += OnStatsChanged;
            _engineSO.OnEngineStatsChanged += OnStatsChanged;
            _stats.OnFieldChanged += _stats_OnFieldChanged;
#endif
        }

#if UNITY_EDITOR

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

        private void _stats_OnFieldChanged()
        {
            _transmissionSO.OnTransmissionStatsChanged -= OnStatsChanged;
            _engineSO.OnEngineStatsChanged -= OnStatsChanged;

            _transmissionSO = _stats.TransmissionSO;
            _engineSO = _stats.EngineSO;

            _transmissionSO.OnTransmissionStatsChanged += OnStatsChanged;
            _engineSO.OnEngineStatsChanged += OnStatsChanged;
            UpdateRPMValues();
        }
#endif
        public void HandleGearChanges(TransmissionType transmissionType, WheelController[] wheelControllers)
        {
            _transmissionType = transmissionType;

            float wheelRPM = 0;

            int size = wheelControllers.Length;
            for (int i = 0; i < size; i++)
                wheelRPM += wheelControllers[i].WheelRPM;

            wheelRPM /= size;

            float temp = Mathf.Abs(wheelRPM) * _stats.TransmissionSO.FinalDriveRatio * 60 / 6.28f;

            float RPMfromSpeed = CalculateRealRPM(temp);
            (float imaginaryRPM, int gearDown) = CalculateImaginaryRPMAndGearSkip(temp);

            _redlining = RPMfromSpeed == _maxRPM;

            if (transmissionType != TransmissionType.Automatic)
                return;

            if (_currentCarStats.InAir)
                return;
            
            SwitchGearsAutomatically(RPMfromSpeed, imaginaryRPM, gearDown);
        }

        public float EvaluateRPM(float gasInput, WheelController[] wheelControllers)
        {
            float highestRPM = 0;
            int size = wheelControllers.Length;

            for (int i = 0; i < size; i++)
            {
                if(Mathf.Abs(wheelControllers[i].VisualRPM) > highestRPM)
                    highestRPM = Mathf.Abs(wheelControllers[i].VisualRPM);
            }

            float imaginaryEngineRPM = CalculateRealRPM(Mathf.Abs(highestRPM) * _stats.TransmissionSO.FinalDriveRatio * 60 / 6.28f);

            _currentEngineRPM = Mathf.SmoothDamp(_currentEngineRPM, imaginaryEngineRPM, ref smDampVelocity, SM_DAMP_SPEED);

            _inCooldown = Time.time < _lastShiftTime + _stats.TransmissionSO.ShiftCooldown;
            PerformRedliningEffect(gasInput);

            return _currentEngineRPM;
        }

        private float CalculateRealRPM(float temp)
        {
            float nextRPM = temp * _stats.TransmissionSO.GearRatiosList[_shifter.GetCurrentGearID()];
            return Mathf.Clamp(nextRPM, _minRPM, _maxRPM);
        }

        private (float, int) CalculateImaginaryRPMAndGearSkip(float temp)
        {
            int currentGear = _shifter.GetCurrentGearID();

            int gearSkip = -1;

            int bestGear = Mathf.Clamp(currentGear - 1, 0, _shifter.GetGearAmount());
            float bestRPM = Mathf.Clamp(temp * _stats.TransmissionSO.GearRatiosList[bestGear],
                _minRPM, _maxRPM);

            //find the gear that will give us the highest possible RPM when downshifting.
            //for example, in case of high speed crash, the best one would be the first gear, without the need to downshift multiple times
            for (int i = bestGear - 1; i >= 0; i--)
            {
                float imaginaryRPM = temp * _stats.TransmissionSO.GearRatiosList[i];

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

            if (_currentCarStats.Reversing && !_currentCarStats.Accelerating)
                ShiftGear(-1);

            TryUpShift(rpmFromSpeed);
            TryDownShift(imaginaryRPM, gearDown);
        }

        private void PerformRedliningEffect(float gasInput)
        {
            if (_currentEngineRPM < _maxRPM * 0.99f)
                return;

            _currentEngineRPM -= _currentCarStats.CurrentEngineHorsepower * gasInput * UnityEngine.Random.Range(1f,2f) * Time.deltaTime;
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

            if (!_shifter.TryChangeGear(i, _stats.TransmissionSO.ShiftCooldown))
                return;

            _lastShiftTime = Time.time;
            OnShifted?.Invoke();
        }

        public bool InShiftingCooldown() => _inCooldown;

        public bool Redlining() => _redlining;

        public float DetermineGasInput(float gasInput, float breakInput)
        {
            if(_transmissionType == TransmissionType.Automatic)
                return _currentCarStats.Reversing ? -breakInput : gasInput;

            return _shifter.InReverseGear() ? -gasInput : gasInput;
        }

        public float DetermineBreakInput(float gasInput, float breakInput)
        {
            if (_transmissionType == TransmissionType.Automatic)
                return _currentCarStats.Reversing ? gasInput : breakInput;

            return breakInput;
        }
    }
}
