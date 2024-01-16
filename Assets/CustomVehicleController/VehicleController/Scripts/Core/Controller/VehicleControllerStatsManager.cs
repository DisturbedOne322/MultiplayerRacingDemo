using System.Linq;
using UnityEngine;

namespace Assets.VehicleController
{
    public class VehicleControllerStatsManager
    {
        private float _lastDriftTime;
        private float _lastSpeed;

        private Rigidbody _rb;
        private Transform _transform;
        private CurrentCarStats _currentCarStats;
        private VehicleAxle[] _axleArray;
        private WheelController[] _wheelControllerArray;
        private VehicleAxle[] _frontAxleArray;
        private VehicleAxle[] _rearAxleArray;

        private VehicleAxle[] _driveAxleArray;

        private ITransmission _transmission;
        private IShifter _shifter;
        private IEngine _engine;

        private float _maxRPM;

        private VehicleStats _stats;
        private int _axleAmount;
        private int _wheelAmount;

        private const string REVERSE_GEAR_NAME = "R";
        private const string NEUTRAL_GEAR_NAME = "N";

        private string[] _gearsArray;

#if UNITY_EDITOR
        private EngineSO _engineSO;
#endif

        public VehicleControllerStatsManager(VehicleAxle[] axleArray, VehicleAxle[] frontAxleArray, VehicleAxle[] rearAxleArray,
            CurrentCarStats currentCarStats, Rigidbody rb, Transform transform, IEngine engine, ITransmission transmission,
            IShifter shifter, VehicleStats stats)
        {
            _currentCarStats = currentCarStats;
            _axleArray = axleArray;
            _frontAxleArray = frontAxleArray;
            _rearAxleArray = rearAxleArray;
            _rb = rb;
            this._transform = transform;

            _wheelControllerArray = VehicleAxle.ExtractVehicleWheelControllerArray(_axleArray);

            _engine = engine;
            _transmission = transmission;
            _shifter = shifter;

            _axleAmount = axleArray.Length;
            _wheelAmount = _axleAmount * 2;
            _currentCarStats.WheelSlipArray = new bool[_axleAmount * 2];

            _stats = stats;
            _maxRPM = _stats.EngineSO.MaxRPM;

            CreateGearCharArray();

#if UNITY_EDITOR
            _engineSO = _stats.EngineSO;
            _engineSO.OnEngineStatsChanged += OnStatsChanged;
            _stats.OnFieldChanged += _stats_OnFieldChanged;
#endif
        }

#if UNITY_EDITOR

        private void OnStatsChanged() => _maxRPM = _engineSO.MaxRPM;

        private void _stats_OnFieldChanged()
        {
            _engineSO.OnEngineStatsChanged -= OnStatsChanged;

            _engineSO = _stats.EngineSO;

            _engineSO.OnEngineStatsChanged += OnStatsChanged;
        }
#endif

        private void CreateGearCharArray()
        {
            _gearsArray = new string[_stats.TransmissionSO.GearRatiosList.Count];

            for (int i = 1; i <= _gearsArray.Length; i++)
                _gearsArray[i - 1] = i.ToString();
        }

        public void ManageStats(float gasInput, float brakeInput, bool handbrakeInput, float sideSlipThreshold, float fwdSlipThreshold, DrivetrainType drivetrainType)
        {
            UpdateDriveWheels(drivetrainType);
            float speedMS = Vector3.Dot(_rb.velocity, _transform.forward);

            _currentCarStats.SpeedInMsPerS = speedMS;
            _currentCarStats.SpeedPercent = Mathf.Clamp01(Mathf.Abs(_currentCarStats.SpeedInKMperH) / _stats.EngineSO.MaxSpeed);
            _currentCarStats.EngineRPM = _transmission.EvaluateRPM(gasInput, _driveAxleArray);
            _currentCarStats.EngineRPMPercent = _currentCarStats.EngineRPM / _maxRPM;

            _currentCarStats.CurrentEngineTorque = _engine.GetCurrentTorque();
            _currentCarStats.ForcedInductionBoostPercent = _engine.GetForcedInductionBoostPercent();

            _currentCarStats.Accelerating = gasInput != 0;
            _currentCarStats.Braking = _transmission.DetermineBreakInput(gasInput, brakeInput) != 0;
            _currentCarStats.HandbrakePulled = handbrakeInput;

            _currentCarStats.AccelerationForce = (_currentCarStats.SpeedInMsPerS - _lastSpeed) / Time.deltaTime;
            _lastSpeed = speedMS;

            _currentCarStats.SidewaysForce = _rb.velocity.x * _transform.right.x + _rb.velocity.z * _transform.right.z;

            _currentCarStats.Reversing = gasInput == 0 && brakeInput > 0 && speedMS <= 1;

            CalculateDriftAngle();
            CalculateDriftTime(sideSlipThreshold);
            IsCarInAir();
            HaveDriveWheelNoGroundContact();
            HasCarLostTraction(sideSlipThreshold, fwdSlipThreshold);
            UpdateCurrentGear();
        }

        private void UpdateDriveWheels(DrivetrainType drivetrainType)
        {
            switch (drivetrainType)
            {
                case DrivetrainType.FWD:
                    _driveAxleArray = _frontAxleArray;
                    break;
                case DrivetrainType.RWD:
                    _driveAxleArray = _rearAxleArray;
                    break;
                default:
                    _driveAxleArray = _axleArray;
                    break;
            }
        }

        private void CalculateDriftAngle()
        {
            if (_currentCarStats.SpeedInMsPerS > 0.1f)
                _currentCarStats.DriftAngle = Vector3.Angle(_transform.forward, _rb.velocity);
            else
                _currentCarStats.DriftAngle = 0;
        }

        private void CalculateDriftTime(float sideSlipThreshold)
        {
            if (Mathf.Abs(Vector3.Dot(_rb.velocity.normalized, _transform.right)) > sideSlipThreshold)
            {
                _lastDriftTime = Time.time;
            }            
                   
            _currentCarStats.DriftTime = Time.time < _lastDriftTime + 1 ? _currentCarStats.DriftTime + Time.deltaTime : 0;
        }

        private void UpdateCurrentGear()
        {
            if (_shifter.InReverseGear())
            {
                _currentCarStats.CurrentGear = REVERSE_GEAR_NAME;
                return;
            }

            if (_shifter.InNeutralGear())
            {
                _currentCarStats.CurrentGear = NEUTRAL_GEAR_NAME;
                return;
            }

            int gearID = _shifter.GetCurrentGearID();

            if (gearID >= _gearsArray.Length)
                CreateGearCharArray();

            _currentCarStats.CurrentGear = _gearsArray[gearID];
        }

        private void HasCarLostTraction(float sideSlipThres, float fwdSlipThres)
        {
            bool slip = false;

            for (int i = 0; i < _wheelAmount; i++)
            {
                if (!_wheelControllerArray[i].IsExceedingSlipThreshold(fwdSlipThres, sideSlipThres))
                {
                    _currentCarStats.WheelSlipArray[i] = false;
                }
                else
                {
                    slip = true;
                    _currentCarStats.WheelSlipArray[i] = true;
                }
            }
            _currentCarStats.IsCarSlipping = slip;
        }

        private void IsCarInAir()
        {
            int wheelsInAir = 0;
            for (int i = 0; i < _wheelAmount; i++)
            {
                if (!_wheelControllerArray[i].HasContactWithGround)
                    wheelsInAir++;
            }
            _currentCarStats.InAir = wheelsInAir == _wheelAmount;
            _currentCarStats.AirTime = _currentCarStats.InAir ? _currentCarStats.AirTime + Time.deltaTime : 0;
        }

        private void HaveDriveWheelNoGroundContact()
        {
            int wheelsInAir = 0;
            int size = _driveAxleArray.Length;
            for (int i = 0; i < size; i++)
            {
                if (!_driveAxleArray[i].LeftHalfShaft.WheelController.HasContactWithGround)
                    wheelsInAir++;

                if (!_driveAxleArray[i].RightHalfShaft.WheelController.HasContactWithGround)
                    wheelsInAir++;
            }

            _currentCarStats.DriveWheelLostContact = wheelsInAir == size * 2;
        }
    }

}
