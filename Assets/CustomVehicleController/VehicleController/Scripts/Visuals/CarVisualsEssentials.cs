using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Visuals/Car Visuals Essentials")]
    public class CarVisualsEssentials : MonoBehaviour
    {
        private Rigidbody _rigidBody;
        private CurrentCarStats _currentCarStats;

        #region Wheel Meshes
        [SerializeField, Tooltip("Array of wheel controllers")]
        private VehicleAxle[] _axleArray;
        [SerializeField]
        private VehicleAxle[] _steerAxleArray;
        #endregion

        private float _steerWheelsAngle = 0;

        private float _smDempVelocity;

        private bool[] _isOrientationCorrectArray;

        public void Initialize(Rigidbody rb, CurrentCarStats currentCarStats)
        {
            _rigidBody = rb;
            _currentCarStats = currentCarStats; 

            //if the wheel has wrong orientation
            //(for example it has to be rotated 180 degrees around the Y-axis to have the same forward vector as the car)
            //then this find out those wheels and adds 180 degrees to their rotation in update loop.
            _isOrientationCorrectArray = new bool[_axleArray.Length * 2];

            int wheelId = 0;

            for (int i = 0; i < _axleArray.Length; i++)
            {
                _isOrientationCorrectArray[wheelId] = IsOrientationCorrect(_axleArray[i].LeftHalfShaft.WheelVisualTransform.localEulerAngles.y);
                wheelId++;

                _isOrientationCorrectArray[wheelId] = IsOrientationCorrect(_axleArray[i].RightHalfShaft.WheelVisualTransform.localEulerAngles.y);
                wheelId++;
            }
        }

        private bool IsOrientationCorrect(float angle )
        {
            if (angle >= 180)
                angle -= 360;
            if (angle <= -180)
                angle += 360;

            return angle < 90 && angle > -90;
        }

        public void HandleWheelVisuals(float input, float currentWheelAngle, float maxSteerAngle, float steerSpeed)
        {
            SpinWheels();
            SteerWheels(input, currentWheelAngle, maxSteerAngle, steerSpeed);
            UpdateWheelPosition();
        }

        private void SpinWheels()
        {
            int wheelId = 0;
            for (int i = 0; i < _axleArray.Length; i++)
            {
                float rotationX = _axleArray[i].LeftHalfShaft.WheelController.VisualRPM;

                if (!_isOrientationCorrectArray[wheelId])
                    _axleArray[i].LeftHalfShaft.WheelVisualTransform.localRotation *= Quaternion.Euler(new Vector3(-rotationX, 0, 0));
                else
                    _axleArray[i].LeftHalfShaft.WheelVisualTransform.localRotation *= Quaternion.Euler(new Vector3(rotationX, 0, 0));

                wheelId++;


                if (!_isOrientationCorrectArray[wheelId])
                    _axleArray[i].RightHalfShaft.WheelVisualTransform.localRotation *= Quaternion.Euler(new Vector3(-rotationX, 0, 0));
                else
                    _axleArray[i].RightHalfShaft.WheelVisualTransform.localRotation *= Quaternion.Euler(new Vector3(rotationX, 0, 0));

                wheelId++;
            }
        }


        private void SteerWheels(float input, float currentWheelAngle, float maxSteerAngle, float steerSpeed)
        {
            if (input == 0)
            {
                float angle = 0;

                Debug.Log(_currentCarStats.SpeedInMsPerS);
                if(!_currentCarStats.InAir)
                    if (_currentCarStats.SpeedInMsPerS > 0.1f)
                        angle = Vector3.SignedAngle(transform.forward, _rigidBody.velocity, Vector3.up);
                    else if (_currentCarStats.SpeedInMsPerS < -0.1f)
                        angle = Vector3.SignedAngle(-transform.forward, _rigidBody.velocity, Vector3.up);
                    else
                        angle = _steerWheelsAngle;

                _steerWheelsAngle = Mathf.SmoothDampAngle(_steerWheelsAngle, angle, ref _smDempVelocity, steerSpeed);            
            }
            else
                _steerWheelsAngle = Mathf.SmoothDampAngle(_steerWheelsAngle, currentWheelAngle, ref _smDempVelocity, steerSpeed);

            for (int i = 0; i < _steerAxleArray.Length; i++)
            {
                if (_axleArray[i].LeftHalfShaft.SteerParentTransform == null ||
                    _axleArray[i].RightHalfShaft.SteerParentTransform == null)
                    continue;

                _axleArray[i].LeftHalfShaft.SteerParentTransform.localRotation = Quaternion.Euler(_axleArray[i].LeftHalfShaft.SteerParentTransform.localRotation.x,
                    Mathf.Clamp(_steerWheelsAngle, -maxSteerAngle, maxSteerAngle),
                    _axleArray[i].LeftHalfShaft.SteerParentTransform.localRotation.z);

                _axleArray[i].RightHalfShaft.SteerParentTransform.localRotation = Quaternion.Euler(_axleArray[i].RightHalfShaft.SteerParentTransform.localRotation.x,
                    Mathf.Clamp(_steerWheelsAngle, -maxSteerAngle, maxSteerAngle),
                    _axleArray[i].RightHalfShaft.SteerParentTransform.localRotation.z);
            }
        }

        private void UpdateWheelPosition()
        {
            for (int i = 0; i < _axleArray.Length; i++)
            {
                _axleArray[i].LeftHalfShaft.WheelVisualTransform.transform.localPosition = _axleArray[i].LeftHalfShaft.WheelController.WheelPosition;
                _axleArray[i].RightHalfShaft.WheelVisualTransform.transform.localPosition = _axleArray[i].RightHalfShaft.WheelController.WheelPosition;
            }
        }


        public VehicleAxle[] GetAxleArray() => _axleArray;
        public CurrentCarStats GetCurrentCarStats() => GetComponent<CustomVehicleController>().GetCurrentCarStats();
        public Rigidbody GetRigidbody() => GetComponent<CustomVehicleController>().GetRigidbody();
    }
}
