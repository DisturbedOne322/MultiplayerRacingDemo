using UnityEngine;

namespace Assets.VehicleController
{
    public class Handling : IHandling
    {
        private VehicleAxle[] _steerAxle;

        private float _smDampVelocity;
        private float _steerAxlesAmount;

        public void Initialize(VehicleAxle[] wheelColliders)
        {
            _steerAxle = wheelColliders;
            _steerAxlesAmount = _steerAxle.Length;   
        }

        public void SteerWheels(float input, float steerAngle, float steerSpeed)
        {

            float targetAngle = input * steerAngle;
            float angle = Mathf.SmoothDampAngle(_steerAxle[0].LeftHalfShaft.WheelController.SteerAngle, targetAngle, ref _smDampVelocity, steerSpeed);

            for (int i = 0; i < _steerAxlesAmount; i++)
            {
                _steerAxle[i].ApplySteering(angle);
            }
        }
    }
}
