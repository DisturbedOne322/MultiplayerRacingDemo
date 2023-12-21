using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Input/Vehicle Controller Input Provider Player Input")]
    public class VehicleControllerInputProviderPlayerInput : MonoBehaviour, IVehicleControllerInputProvider
    {
        #region Control field
        private float _gasInput;
        private float _brakeInput;
        private bool _handbrakeInput;
        private float _horizontalInput;
        private float _pitchInput;
        private float _yawInput;
        private float _rollInput;
        #endregion
        [SerializeField]
        private PlayerInput _playerInput;
        private InputAction _gearUpAction;
        private InputAction _gearDownAction;

        private void Awake()
        {
            if (_playerInput == null)
                _playerInput = GetComponent<PlayerInput>();
            if (_playerInput != null)
            {
                _gearUpAction = _playerInput.actions["GearUpInput"];
                _gearDownAction = _playerInput.actions["GearDownInput"];
            }
        }

        public float OnGasInput(InputValue value) => _gasInput = value.Get<float>();

        public float OnBrakeInput(InputValue value) => _brakeInput = value.Get<float>();

        public bool OnHandbrakeInput(InputValue value) => _handbrakeInput = value.Get<float>() == 1;

        public float OnHorizontalInput(InputValue value) => _horizontalInput = value.Get<float>();

        public float OnPitchInput(InputValue value) => _pitchInput = value.Get<float>();

        public float OnYawInput(InputValue value) => _yawInput = value.Get<float>();

        public float OnRollInput(InputValue value) => _rollInput = value.Get<float>();

        public float GetGasInput() => _gasInput;

        public float GetBrakeInput() => _brakeInput;

        public bool GetHandbrakeInput() => _handbrakeInput;

        public float GetHorizontalInput() => _horizontalInput;

        public bool GetGearUpInput() => _gearUpAction.triggered;

        public bool GetGearDownInput() => _gearDownAction.triggered;

        public float GetPitchInput() => _pitchInput;

        public float GetYawInput() => _yawInput;

        public float GetRollInput() => _rollInput;
    }
}
