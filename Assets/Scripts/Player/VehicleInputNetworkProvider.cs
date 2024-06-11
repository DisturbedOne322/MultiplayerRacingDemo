using Assets.VehicleController;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class VehicleInputNetworkProvider : NetworkBehaviour, IVehicleControllerInputProvider
{
    private float _brakeInput;
    private float _gasInput;
    private float _horizontalInput;
    private bool _handbrakeInput;
    private bool _nitroInput;

    private PlayerNetworkInputActions _inputActions;

    private NetworkVariable<bool> _inputEnabledNetVar = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private bool _lockVehicleInput = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        _inputActions = new PlayerNetworkInputActions();
        _inputActions.Enable();

        _inputEnabledNetVar.OnValueChanged += (bool oldValue, bool newValue) =>
        {
            if (!newValue)
            {
                _horizontalInput = 0;
                _handbrakeInput = _nitroInput = false;
                GetComponent<CustomVehicleController>().TransmissionType = TransmissionType.Manual;
            }
            else
            {
                GetComponent<CustomVehicleController>().TransmissionType = TransmissionType.Automatic;
            }
        };

        _inputActions.Chat.OpenChat.performed += _ => {_lockVehicleInput = !_lockVehicleInput;};
    }

    public void EnableInput(bool enable)
    {
        _inputEnabledNetVar.Value = enable;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (_lockVehicleInput)
        {
            _gasInput = 0;
            _brakeInput = 0;
            _handbrakeInput = false;
            _nitroInput = false;
            _horizontalInput = 0;
            return;
        }

        _gasInput = _inputActions.Vehicle.GasInput.ReadValue<float>();

        if (!_inputEnabledNetVar.Value)
        {
            _brakeInput = _inputActions.Vehicle.GasInput.ReadValue<float>() == 0 ? 0 : 1;
            return;
        }

        _brakeInput = _inputActions.Vehicle.BrakeInput.ReadValue<float>();

        _horizontalInput = _inputActions.Vehicle.HorizontalInput.ReadValue<float>();

        _handbrakeInput = _inputActions.Vehicle.HandbrakeInput.IsPressed();
        _nitroInput = _inputActions.Vehicle.NitroInput.IsPressed();
    }

    public float GetBrakeInput() => IsOwner ? _brakeInput : 0;

    public float GetGasInput() => IsOwner ? _gasInput : 0;

    public bool GetHandbrakeInput() => IsOwner ? _handbrakeInput : false;

    public float GetHorizontalInput() => IsOwner ? _horizontalInput : 0;

    public bool GetNitroBoostInput() => IsOwner ? _nitroInput : false;

    public bool GetGearDownInput() => false;

    public bool GetGearUpInput() => false;

    public float GetPitchInput() => 0;

    public float GetRollInput() => 0;

    public float GetYawInput() => 0;
}
