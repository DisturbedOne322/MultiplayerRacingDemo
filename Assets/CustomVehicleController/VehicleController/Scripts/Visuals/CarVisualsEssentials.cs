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
        [SerializeField, Tooltip("Array of GameObjects that represent wheels")]
        private Transform[] _wheelMeshes;
        [SerializeField, Tooltip("Array of parent GameObjects of the steer wheels")]
        private Transform[] _steerWheelTransformArray;
        private int _steerableWheelsArraySize;
        [SerializeField, Tooltip("Array of wheel controllers")]
        private WheelController[] _wheelControllerArray;
        private int _wheelMeshesSize;
        #endregion

        private float _steerWheelsAngle = 0;

        private float _smDempVelocity;
        private float _smDampTime = 0.1f;

        private bool[] _isOrientationCorrectArray;

        public void Initialize(Rigidbody rb, CurrentCarStats currentCarStats)
        {
            _rigidBody = rb;
            _currentCarStats = currentCarStats; 

            _wheelMeshesSize = _wheelMeshes.Length;
            _steerableWheelsArraySize = _steerWheelTransformArray.Length;

            //if the wheel has wrong orientation
            //(for example it has to be rotated 180 degrees around the Y-axis to have the same forward vector as the car)
            //then this find out those wheels and adds 180 degrees to their rotation in update loop.
            _isOrientationCorrectArray = new bool[_wheelMeshesSize];
            for (int i = 0; i < _wheelMeshesSize; i++)
            {
                float yAngle = _wheelMeshes[i].localEulerAngles.y;

                if (yAngle >= 180)
                    yAngle -= 360;
                if(yAngle <= -180)
                    yAngle += 360;

                _isOrientationCorrectArray[i] = yAngle < 90 && _wheelMeshes[i].localEulerAngles.y > -90;
            }
        }

        public void HandleWheelVisuals(float input, float currentWheelAngle, float maxSteerAngle)
        {
            SpinWheels();
            SteerWheels(input, currentWheelAngle, maxSteerAngle);
            UpdateWheelPosition();
        }

        private void SpinWheels()
        {
            for (int i = 0; i < _wheelMeshesSize; i++)
            {
                float rotationX = _wheelControllerArray[i].VisualRPM;
                if (!_isOrientationCorrectArray[i])
                    rotationX *= -1;

                _wheelMeshes[i].localRotation *= Quaternion.Euler(new Vector3(rotationX, 0, 0));
            }
        }
        private void SteerWheels(float input, float currentWheelAngle, float maxSteerAngle)
        {
            if (input == 0)
            {
                float angle = 0;

                if(_currentCarStats.SpeedInMsPerS > 0.1f)
                    angle = Vector3.SignedAngle(transform.forward, _rigidBody.velocity, Vector3.up);

                _steerWheelsAngle = Mathf.SmoothDampAngle(_steerWheelsAngle, angle, ref _smDempVelocity, _smDampTime);            
            }
            else
                _steerWheelsAngle = Mathf.SmoothDampAngle(_steerWheelsAngle, currentWheelAngle, ref _smDempVelocity, _smDampTime);

            for (int i = 0; i < _steerableWheelsArraySize; i++)
            {
                _steerWheelTransformArray[i].localRotation = Quaternion.Euler(_steerWheelTransformArray[i].localRotation.x,
                    Mathf.Clamp(_steerWheelsAngle, -maxSteerAngle, maxSteerAngle),
                    _steerWheelTransformArray[i].localRotation.z);
            }
        }

        private void UpdateWheelPosition()
        {
            for (int i = 0; i < _wheelMeshesSize; i++)
                _wheelMeshes[i].transform.localPosition = _wheelControllerArray[i].WheelPosition;
        }


        public Transform[] GetWheelMeshes() => _wheelMeshes;
        public WheelController[] GetWheelControllerArray() => _wheelControllerArray;
        public CurrentCarStats GetCurrentCarStats() => GetComponent<CustomVehicleController>().GetCurrentCarStats();
        public Rigidbody GetRigidbody() => GetComponent<CustomVehicleController>().GetRigidbody();
    }
}
