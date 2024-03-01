using UnityEngine;

namespace Assets.VehicleController
{
    public class Handling : IHandling
    {
        private VehicleAxle[] _steerAxle;

        private float _smDampVelocity;
        private float _steerAxlesAmount;

        private float _targetAngle = 0;

        public void Initialize(VehicleAxle[] steerAxles)
        {
            _steerAxle = steerAxles;
            _steerAxlesAmount = _steerAxle.Length;
        }

        public void SteerWheels(float input, float maxSteerAngle, float steerSpeed)
        {
            _targetAngle = input * maxSteerAngle;
            float angle = Mathf.SmoothDampAngle(_steerAxle[0].LeftHalfShaft.WheelController.SteerAngle, _targetAngle, ref _smDampVelocity, steerSpeed);

            for (int i = 0; i < _steerAxlesAmount; i++)
            {
                _steerAxle[i].ApplySteering(angle);
            }
        }
    }
}
