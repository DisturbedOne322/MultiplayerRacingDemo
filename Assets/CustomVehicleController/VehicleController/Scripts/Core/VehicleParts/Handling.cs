using UnityEngine;

namespace Assets.VehicleController
{
    public class Handling : IHandling
    {
        private WheelController[] _steerWheelColliders;

        private float _smDampVelocity;
        private float _steerWheelsAmount;

        public void Initialize(WheelController[] wheelColliders)
        {
            _steerWheelColliders = wheelColliders;
            _steerWheelsAmount = _steerWheelColliders.Length;   
        }

        public void SteerWheels(float input, float steerAngle, float steerSpeed)
        {

            float targetAngle = input * steerAngle;
            float angle = Mathf.SmoothDampAngle(_steerWheelColliders[0].SteerAngle, targetAngle, ref _smDampVelocity, steerSpeed);

            for (int i = 0; i < _steerWheelsAmount; i++)
            {
                _steerWheelColliders[i].SteerAngle = angle;
            }
        }
    }
}
