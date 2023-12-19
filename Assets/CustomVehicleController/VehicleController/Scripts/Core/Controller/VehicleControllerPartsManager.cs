using UnityEngine;

namespace Assets.VehicleController
{
    public class VehicleControllerPartsManager
    {
        private IBody _body;
        private IEngine _engine;
        private ITransmission _transmission;
        private IBrakes _breaks;
        private IHandling _handling;

        private CurrentCarStats _currentCarStats;

        private WheelController[] _wheelControllersArray;
        private WheelController[] _forwardWheelControllersArray;
        private WheelController[] _rearWheelControllersArray;

        private WheelController[] _driveWheelsArray;

        private Transform _centerOfGeometry;
        private Transform _transform;

        public VehicleControllerPartsManager(IBody body, IEngine engine, ITransmission transmission, IBrakes brakes,
            IHandling handling, CurrentCarStats currentCarStats, Transform transform,
            WheelController[] wheelControllers, WheelController[] frontWheels
            , WheelController[] rearWheels, Transform centerOfGeometry)
        {
            _body = body;
            _engine = engine;
            _transmission = transmission;
            _breaks = brakes;
            _handling = handling;
            _currentCarStats = currentCarStats;
            _transform = transform;
            _wheelControllersArray = wheelControllers;
            _forwardWheelControllersArray = frontWheels;
            _rearWheelControllersArray = rearWheels;
            _centerOfGeometry = centerOfGeometry;
        }

        public void ManageCarParts(float gasInput, float breakInput, float horizontalInput,
        bool handbrakeInput, float steerAngle, float steerSpeed, PartTypes.TransmissionType transmissionType, 
        PartTypes.DrivetrainType drivetrainType, int suspensionSimulationPrecision)
        {
            UpdateDriveWheels(drivetrainType);

            _body.AddDownforce();
            _body.AddCorneringForce();
            _body.HandleAirDrag();

            _engine.Accelerate(_driveWheelsArray, gasInput, breakInput, _currentCarStats.EngineRPM);

            _breaks.Break(gasInput, breakInput, handbrakeInput);
            _handling.SteerWheels(horizontalInput, steerAngle, steerSpeed);
            _transmission.HandleGearChanges(transmissionType, _driveWheelsArray);
            ManageWheels(suspensionSimulationPrecision);
        }

        private void UpdateDriveWheels(PartTypes.DrivetrainType drivetrainType)
        {
            switch(drivetrainType)
            {
                case PartTypes.DrivetrainType.RWD:
                    _driveWheelsArray = _rearWheelControllersArray;
                    break;
                case PartTypes.DrivetrainType.FWD:
                    _driveWheelsArray = _forwardWheelControllersArray;
                    break;
                default:
                    _driveWheelsArray = _wheelControllersArray; 
                    break;
            }
        }

        public void ManageTransmissionUpShift(bool shiftUp)
        {
            if (shiftUp)
                _transmission.ShiftUpManually();
        }

        public void ManageTransmissionDownShift(bool shiftDown)
        {
            if (shiftDown)
                _transmission.ShiftDownManually();
        }

        public void ManageWheels(int suspensionSimulationPrecision)
        {
            float dist = GetDistanceToGroundFromCoG();
            int size = _wheelControllersArray.Length;
            for (int i = 0; i < size; i++)
            {
                _wheelControllersArray[i].ControlWheel(_currentCarStats.SpeedInMsPerS, _currentCarStats.SpeedPercent, _currentCarStats.AccelerationForce, dist, suspensionSimulationPrecision);
            }
        }
        private float GetDistanceToGroundFromCoG()
        {
            RaycastHit hit;
            if (Physics.Raycast(_centerOfGeometry.position, -_transform.up, out hit))
            {
                return hit.distance;
            }
            return 0;
        }

        public void PerformAirControls(bool enabled, float aerialControlsSensitivity,
            float horizInput, float verticalInput)
        {
            if (enabled)
            {
                _body.PerformAerialControls(aerialControlsSensitivity, horizInput, verticalInput);
            }
        }

        public void AutomaticFlipOverRecover(bool enabled, float time)
        {
            if (enabled)
                _body.AutomaticFlipOverRecover(time);
        }
    }
}


