//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/CustomVehicleController/VehicleController/Scripts/Input/PlayerVehicleInputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @PlayerVehicleInputActions: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerVehicleInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerVehicleInputActions"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""33b1eead-38c3-4a0f-a642-375d9376985e"",
            ""actions"": [
                {
                    ""name"": ""HandbrakeInput"",
                    ""type"": ""Value"",
                    ""id"": ""d504abf3-a63b-46e9-a693-c73c336ed7a0"",
                    ""expectedControlType"": ""Integer"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""GearDownInput"",
                    ""type"": ""Button"",
                    ""id"": ""47ba21d9-77be-4ef5-9016-5348c15c857d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""GearUpInput"",
                    ""type"": ""Button"",
                    ""id"": ""5205feed-b2fb-4ad7-872d-49ee69fe958e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""GasInput"",
                    ""type"": ""Value"",
                    ""id"": ""66287a17-7380-45be-b17c-3c0df11daad6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""BrakeInput"",
                    ""type"": ""Value"",
                    ""id"": ""0d022bf9-3a00-45ad-ac49-359aaec6c6e1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""HorizontalInput"",
                    ""type"": ""Value"",
                    ""id"": ""69c2a3fd-8915-4880-85e6-3c0b3ec9f3b6"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""PitchInput"",
                    ""type"": ""Value"",
                    ""id"": ""fabcfe13-f9ce-429d-aa6f-f9ec617c59f6"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""YawInput"",
                    ""type"": ""Value"",
                    ""id"": ""e9676fbf-1234-493b-8419-3f52dbb31d5c"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""RollInput"",
                    ""type"": ""Value"",
                    ""id"": ""f77ffe61-aafc-4c3a-a665-508e47777276"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""NitrousBoostInput"",
                    ""type"": ""Button"",
                    ""id"": ""61e59642-ae52-4f8d-b533-6d609a1de107"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""a8dd42f3-054d-4fe7-bb4f-3be6ae565e3b"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HandbrakeInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7959fa8c-a01a-45ae-b7a1-56c0098e32fe"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HandbrakeInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d521bd77-11a7-4441-8e60-2cf5ed1cf991"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""GearDownInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""68187b6f-de17-4162-89b8-6b62e9c92a85"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""GearUpInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""02038b5f-e70d-4916-a1d4-e4cd45f85275"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""GasInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ba342673-5183-46ab-955d-88ffd061c105"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""GasInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3ef52019-39df-4787-abe3-da9644e2cc52"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""BrakeInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""573c1dd7-e811-40fe-b4a1-1d7b9d38a113"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""BrakeInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""2155e1ab-3d1d-4ae4-9dda-07a6ee323332"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HorizontalInput"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""beb2d8fe-48c9-48a8-a3b2-29615f10ab2b"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HorizontalInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""62c3fb5c-9dfd-4dc9-93c5-98e8a9ea2939"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HorizontalInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""64425705-57a9-468a-9207-321dcd1f1e55"",
                    ""path"": ""<Gamepad>/leftStick/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""HorizontalInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""1f8419b8-b600-4b36-af68-737d0468b779"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""YawInput"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""5d40ed95-4679-4bd7-a4f1-47e827389607"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""YawInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""d7186a49-5dde-4b3f-aa08-08d111561dc3"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""YawInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""5a2e798d-2b7d-4fb5-8d04-c9c22f7cb254"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PitchInput"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""398f0f64-7aa9-4131-882b-d8022256d14a"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PitchInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""b5f64dad-d075-4a67-be1a-4361bf51fcb1"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PitchInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""ab3160a5-c737-4d2d-a688-51a8cdd9970b"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RollInput"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""7025b310-3ef6-4362-aff0-89b5bffa2361"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RollInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""afd73e57-2fd1-43d9-9901-91d7db5abc31"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RollInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""d3d45795-f899-49b2-bd43-9bfb63483ea3"",
                    ""path"": ""<Keyboard>/leftAlt"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""NitrousBoostInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""18676485-207d-4c7c-8390-613c0b184ee4"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""NitrousBoostInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_HandbrakeInput = m_Player.FindAction("HandbrakeInput", throwIfNotFound: true);
        m_Player_GearDownInput = m_Player.FindAction("GearDownInput", throwIfNotFound: true);
        m_Player_GearUpInput = m_Player.FindAction("GearUpInput", throwIfNotFound: true);
        m_Player_GasInput = m_Player.FindAction("GasInput", throwIfNotFound: true);
        m_Player_BrakeInput = m_Player.FindAction("BrakeInput", throwIfNotFound: true);
        m_Player_HorizontalInput = m_Player.FindAction("HorizontalInput", throwIfNotFound: true);
        m_Player_PitchInput = m_Player.FindAction("PitchInput", throwIfNotFound: true);
        m_Player_YawInput = m_Player.FindAction("YawInput", throwIfNotFound: true);
        m_Player_RollInput = m_Player.FindAction("RollInput", throwIfNotFound: true);
        m_Player_NitrousBoostInput = m_Player.FindAction("NitrousBoostInput", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Player
    private readonly InputActionMap m_Player;
    private List<IPlayerActions> m_PlayerActionsCallbackInterfaces = new List<IPlayerActions>();
    private readonly InputAction m_Player_HandbrakeInput;
    private readonly InputAction m_Player_GearDownInput;
    private readonly InputAction m_Player_GearUpInput;
    private readonly InputAction m_Player_GasInput;
    private readonly InputAction m_Player_BrakeInput;
    private readonly InputAction m_Player_HorizontalInput;
    private readonly InputAction m_Player_PitchInput;
    private readonly InputAction m_Player_YawInput;
    private readonly InputAction m_Player_RollInput;
    private readonly InputAction m_Player_NitrousBoostInput;
    public struct PlayerActions
    {
        private @PlayerVehicleInputActions m_Wrapper;
        public PlayerActions(@PlayerVehicleInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @HandbrakeInput => m_Wrapper.m_Player_HandbrakeInput;
        public InputAction @GearDownInput => m_Wrapper.m_Player_GearDownInput;
        public InputAction @GearUpInput => m_Wrapper.m_Player_GearUpInput;
        public InputAction @GasInput => m_Wrapper.m_Player_GasInput;
        public InputAction @BrakeInput => m_Wrapper.m_Player_BrakeInput;
        public InputAction @HorizontalInput => m_Wrapper.m_Player_HorizontalInput;
        public InputAction @PitchInput => m_Wrapper.m_Player_PitchInput;
        public InputAction @YawInput => m_Wrapper.m_Player_YawInput;
        public InputAction @RollInput => m_Wrapper.m_Player_RollInput;
        public InputAction @NitrousBoostInput => m_Wrapper.m_Player_NitrousBoostInput;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void AddCallbacks(IPlayerActions instance)
        {
            if (instance == null || m_Wrapper.m_PlayerActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_PlayerActionsCallbackInterfaces.Add(instance);
            @HandbrakeInput.started += instance.OnHandbrakeInput;
            @HandbrakeInput.performed += instance.OnHandbrakeInput;
            @HandbrakeInput.canceled += instance.OnHandbrakeInput;
            @GearDownInput.started += instance.OnGearDownInput;
            @GearDownInput.performed += instance.OnGearDownInput;
            @GearDownInput.canceled += instance.OnGearDownInput;
            @GearUpInput.started += instance.OnGearUpInput;
            @GearUpInput.performed += instance.OnGearUpInput;
            @GearUpInput.canceled += instance.OnGearUpInput;
            @GasInput.started += instance.OnGasInput;
            @GasInput.performed += instance.OnGasInput;
            @GasInput.canceled += instance.OnGasInput;
            @BrakeInput.started += instance.OnBrakeInput;
            @BrakeInput.performed += instance.OnBrakeInput;
            @BrakeInput.canceled += instance.OnBrakeInput;
            @HorizontalInput.started += instance.OnHorizontalInput;
            @HorizontalInput.performed += instance.OnHorizontalInput;
            @HorizontalInput.canceled += instance.OnHorizontalInput;
            @PitchInput.started += instance.OnPitchInput;
            @PitchInput.performed += instance.OnPitchInput;
            @PitchInput.canceled += instance.OnPitchInput;
            @YawInput.started += instance.OnYawInput;
            @YawInput.performed += instance.OnYawInput;
            @YawInput.canceled += instance.OnYawInput;
            @RollInput.started += instance.OnRollInput;
            @RollInput.performed += instance.OnRollInput;
            @RollInput.canceled += instance.OnRollInput;
            @NitrousBoostInput.started += instance.OnNitrousBoostInput;
            @NitrousBoostInput.performed += instance.OnNitrousBoostInput;
            @NitrousBoostInput.canceled += instance.OnNitrousBoostInput;
        }

        private void UnregisterCallbacks(IPlayerActions instance)
        {
            @HandbrakeInput.started -= instance.OnHandbrakeInput;
            @HandbrakeInput.performed -= instance.OnHandbrakeInput;
            @HandbrakeInput.canceled -= instance.OnHandbrakeInput;
            @GearDownInput.started -= instance.OnGearDownInput;
            @GearDownInput.performed -= instance.OnGearDownInput;
            @GearDownInput.canceled -= instance.OnGearDownInput;
            @GearUpInput.started -= instance.OnGearUpInput;
            @GearUpInput.performed -= instance.OnGearUpInput;
            @GearUpInput.canceled -= instance.OnGearUpInput;
            @GasInput.started -= instance.OnGasInput;
            @GasInput.performed -= instance.OnGasInput;
            @GasInput.canceled -= instance.OnGasInput;
            @BrakeInput.started -= instance.OnBrakeInput;
            @BrakeInput.performed -= instance.OnBrakeInput;
            @BrakeInput.canceled -= instance.OnBrakeInput;
            @HorizontalInput.started -= instance.OnHorizontalInput;
            @HorizontalInput.performed -= instance.OnHorizontalInput;
            @HorizontalInput.canceled -= instance.OnHorizontalInput;
            @PitchInput.started -= instance.OnPitchInput;
            @PitchInput.performed -= instance.OnPitchInput;
            @PitchInput.canceled -= instance.OnPitchInput;
            @YawInput.started -= instance.OnYawInput;
            @YawInput.performed -= instance.OnYawInput;
            @YawInput.canceled -= instance.OnYawInput;
            @RollInput.started -= instance.OnRollInput;
            @RollInput.performed -= instance.OnRollInput;
            @RollInput.canceled -= instance.OnRollInput;
            @NitrousBoostInput.started -= instance.OnNitrousBoostInput;
            @NitrousBoostInput.performed -= instance.OnNitrousBoostInput;
            @NitrousBoostInput.canceled -= instance.OnNitrousBoostInput;
        }

        public void RemoveCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IPlayerActions instance)
        {
            foreach (var item in m_Wrapper.m_PlayerActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_PlayerActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public PlayerActions @Player => new PlayerActions(this);
    public interface IPlayerActions
    {
        void OnHandbrakeInput(InputAction.CallbackContext context);
        void OnGearDownInput(InputAction.CallbackContext context);
        void OnGearUpInput(InputAction.CallbackContext context);
        void OnGasInput(InputAction.CallbackContext context);
        void OnBrakeInput(InputAction.CallbackContext context);
        void OnHorizontalInput(InputAction.CallbackContext context);
        void OnPitchInput(InputAction.CallbackContext context);
        void OnYawInput(InputAction.CallbackContext context);
        void OnRollInput(InputAction.CallbackContext context);
        void OnNitrousBoostInput(InputAction.CallbackContext context);
    }
}
