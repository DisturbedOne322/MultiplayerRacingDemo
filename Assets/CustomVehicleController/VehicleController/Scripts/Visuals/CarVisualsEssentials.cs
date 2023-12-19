using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Visuals/Car Visuals Essentials")]
    public class CarVisualsEssentials : MonoBehaviour
    {
        #region Wheel Meshes
        [SerializeField, Tooltip("Array of GameObjects that represent wheels")]
        private Transform[] _wheelMeshes;
        [SerializeField, Tooltip("Referce to the wheel controller that represents a steer wheel")]
        private WheelController _steerWheel;
        [SerializeField, Tooltip("Array of parent GameObjects of the steer wheels")]
        private Transform[] _steerWheelTransformArray;
        private int _steerableWheelsArraySize;
        [SerializeField, Tooltip("Array of wheel controllers")]
        private WheelController[] _wheelControllerArray;
        private int _wheelMeshesSize;
        #endregion

        private void Awake()
        {
            _wheelMeshesSize = _wheelMeshes.Length;
            _steerableWheelsArraySize = _steerWheelTransformArray.Length;
        }

        private void Update()
        {
            SpinWheels();
            SteerWheels();
            UpdateWheelPosition();
        }

        private void SpinWheels()
        {
            for (int i = 0; i < _wheelMeshesSize; i++)
                _wheelMeshes[i].localRotation *= Quaternion.Euler(
                    new Vector3(_wheelControllerArray[i].VisualRPM, 0, 0));
        }
        private void SteerWheels()
        {
            for (int i = 0; i < _steerableWheelsArraySize; i++)
                _steerWheelTransformArray[i].localRotation = Quaternion.Euler(_steerWheelTransformArray[i].localRotation.x, 
                    _steerWheel.SteerAngle, _steerWheelTransformArray[i].localRotation.z);
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
