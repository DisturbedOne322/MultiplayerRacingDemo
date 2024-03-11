using Assets.VehicleController;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VehicleInputNetworkProvider : NetworkBehaviour, IVehicleControllerInputProvider
{
    private float _brakeInput;
    private float _gasInput;
    private float _horizontalInput;
    private bool _handbrakeInput;
    private bool _nitroInput;

    private NetworkVariable<bool> _inputEnabledNetVar = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        _inputEnabledNetVar.OnValueChanged += (bool oldValue, bool newValue) => 
        { 
            if(!newValue)
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
        _gasInput = Input.GetKey(KeyCode.W) ? 1 : 0;

        if (!_inputEnabledNetVar.Value)
        {
            _brakeInput = _gasInput == 0 ? 0 : 1;
            return;
        }

        _brakeInput = Input.GetKey(KeyCode.S) ? 1 : 0;
        float horizInput = 0;
        if (Input.GetKey(KeyCode.A))
            horizInput -= 1;
        if(Input.GetKey(KeyCode.D))
            horizInput += 1;
        _horizontalInput = horizInput;

        _handbrakeInput = Input.GetKey(KeyCode.Space);
        _nitroInput = Input.GetKey(KeyCode.N);
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
