using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Input/Vehicle Controller Input Provider New")]
    public class VehicleControllerInputProviderNew : MonoBehaviour, IVehicleControllerInputProvider
    {
        private PlayerInputActions _playerInput;

        private void Awake()
        {
            _playerInput = new PlayerInputActions();
            _playerInput.Enable();
        }

        public float GetBrakeInput() => _playerInput.Player.BrakeInput.ReadValue<float>();

        public float GetGasInput() => _playerInput.Player.GasInput.ReadValue<float>();

        public bool GetGearDownInput() => _playerInput.Player.GearDownInput.triggered;

        public bool GetGearUpInput() => _playerInput.Player.GearUpInput.triggered;

        public bool GetHandbrakeInput() => _playerInput.Player.HandbrakeInput.IsPressed();

        public float GetHorizontalInput() => _playerInput.Player.HorizontalInput.ReadValue<float>();
    }
}
