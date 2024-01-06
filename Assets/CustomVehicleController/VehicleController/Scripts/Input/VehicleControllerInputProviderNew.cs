using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Input/Vehicle Controller Input Provider New")]
    public class VehicleControllerInputProviderNew : MonoBehaviour, IVehicleControllerInputProvider
    {
        private PlayerVehicleInputActions _playerInput;

        private void Awake()
        {
            _playerInput = new PlayerVehicleInputActions();
            _playerInput.Enable();
        }

        public float GetBrakeInput() => _playerInput.Player.BrakeInput.ReadValue<float>();

        public float GetGasInput() => _playerInput.Player.GasInput.ReadValue<float>();

        public bool GetGearDownInput() => _playerInput.Player.GearDownInput.triggered;

        public bool GetGearUpInput() => _playerInput.Player.GearUpInput.triggered;

        public bool GetHandbrakeInput() => _playerInput.Player.HandbrakeInput.IsPressed();

        public float GetHorizontalInput() => _playerInput.Player.HorizontalInput.ReadValue<float>();

        public float GetPitchInput() => _playerInput.Player.PitchInput.ReadValue<float>();

        public float GetYawInput() => _playerInput.Player.YawInput.ReadValue<float>();

        public float GetRollInput() => _playerInput.Player.RollInput.ReadValue<float>();

        public bool GetNitroBoostInput() => _playerInput.Player.NitrousBoostInput.IsPressed(); 
    }
}
