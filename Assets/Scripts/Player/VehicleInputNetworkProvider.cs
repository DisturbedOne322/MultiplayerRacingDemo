using Assets.VehicleController;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleInputNetworkProvider : NetworkBehaviour, IVehicleControllerInputProvider
{
    private float _brakeInput;
    private float _gasInput;
    private float _horizontalInput;
    private bool _handbrakeInput;
    private bool _nitroInput;

    private PlayerNetworkInputActions _inputActions;

    private NetworkVariable<bool> _inputEnabledNetVar = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


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
    }

    public void EnableInput(bool enable)
    {
        _inputEnabledNetVar.Value = enable;
    }

    private void Update()
    {
        if (!IsOwner)
            return;
        _gasInput = _inputActions.Players.GasInput.ReadValue<float>();

        if (!_inputEnabledNetVar.Value)
        {
            _brakeInput = _inputActions.Players.GasInput.ReadValue<float>() == 0 ? 0 : 1;
            return;
        }

        _brakeInput = _inputActions.Players.BrakeInput.ReadValue<float>();

        _horizontalInput = _inputActions.Players.HorizontalInput.ReadValue<float>();

        _handbrakeInput = _inputActions.Players.HandbrakeInput.IsPressed();
        _nitroInput = _inputActions.Players.NitroInput.IsPressed();
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
