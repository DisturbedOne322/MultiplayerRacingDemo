using System;
using UnityEngine;

namespace Assets.VehicleController
{
    [RequireComponent(typeof(CarVisualsEssentials)),
    RequireComponent(typeof(Rigidbody)), DisallowMultipleComponent, AddComponentMenu("CustomVehicleController/Core/Custom Vehicle Controller"),
    HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/")]
    public class CustomVehicleController : MonoBehaviour
    {
        public bool UsePreset = true;
        [SerializeField]
        private VehiclePartsPresetSO _vehiclePartsPreset;
        [SerializeField]
        private VehiclePartsCustomizableSet _customizableSet;

        //Reference type field allows other classes to cache it and use the up-to-date parts scriptable objects.
        //This class holds the parts that the vehicle is using either in the form of VehiclePartsPresetSO of a VehiclePartsCustomizableSet.
        //This class has a static and object specific event when any part field value changes.
        public VehiclePartsSetWrapper VehiclePartsSetWrapper;

        private VehicleControllerStatsManager _statsManager;
        private VehicleControllerPartsManager _partsManager;

        private CarVisualsEssentials _carVisualsEssentials;

        #region Handling Settings
        [Header("   Handling settings")]
        public DrivetrainType DrivetrainType;
        public TransmissionType TransmissionType;

        [SerializeField, Space, Min(0), Tooltip("Maximum steering angle in degrees")]
        private float _steerAngle = 25;
        public float SteerAngle
        {
            get => _steerAngle;
            set { _steerAngle = Mathf.Clamp(value, 0, 90); }
        }

        [SerializeField, Min(0), Tooltip("Time in which wheels will reach maximum steering angle." +
            "\n Since steering uses a smooth damp function, this time is approximate.")]
        private float _steerSpeed = 0.2f;
        public float SteerSpeed
        {
            get =>  _steerAngle;
            set { _steerSpeed = Mathf.Clamp(value, 0, 100); }
        }
        #endregion

        #region Extra options
        [Header("   Extra options")]
        [SerializeField, Range(0f, 100f), Tooltip("Defines how much slipping is allowed until the wheel is considered to be forward slipping. " +
            "\n Forward slipping occurs when acceleration force is higher than the wheel load * tire grip.")]
        private float _forwardSlippingThreshold = 0.1f;
        public float ForwardSlippingThreshold
        {
            get => _forwardSlippingThreshold;
            set
            {
                _forwardSlippingThreshold = Mathf.Clamp(value, 0, 100f);
            }
        }

        [SerializeField, Range(0f, 1f), Tooltip("Defines how much slipping is allowed until the wheel is considered to be sideways slipping.")]
        private float _sidewaysSlippingThreshold = 0.5f;
        public float SidewaysSlippingThreshold
        {
            get => _sidewaysSlippingThreshold;
            set
            {
                _sidewaysSlippingThreshold = Mathf.Clamp(value, 0, 1f);
            }
        }

        //allows you to control the car in air. 
        [Space]
        public bool AerialControlsEnabled = false;
        public float AerialControlsSensitivity = 0;
        #endregion

        [Header("   Physics"), SerializeField, Tooltip("Assign rigidbody component to avoid using the costly GetComponent operation")]
        private Rigidbody _rigidbody;
        [SerializeField, Range(1, 27), Tooltip("The amount of raycasts that go along the forward axis of the wheel with an offset from -radius to +radius. " +
            "\n Recommended values: [3:9]")]
        private int _suspensionSimulationPrecision = 5;
        public int SuspensionSimulationPrecision
        {
            get => _suspensionSimulationPrecision;
            set
            {
                _suspensionSimulationPrecision = Mathf.Clamp(value, 1, 27);
            }
        }

        [SerializeField, Tooltip("In case you are using mesh collider or multiple colliders that the raycast will go through, mark those colliders with specific layer so that the raycast will ignore them. Otherwise you can leave it as it is.")]
        private LayerMask _ignoreLayers;
        #region Wheel Controllers
        [SerializeField, Space]
        private VehicleAxle[] _frontAxles;
        [SerializeField]
        private VehicleAxle[] _rearAxles;
        [SerializeField]
        private VehicleAxle[] _steerAxles;
        #endregion
        [SerializeField, Tooltip("Center Of Mass of the vehicle. " +
    "\nUsually placed in the middle of the vehicle, slightly closer to the engine.")]
        private Transform _centerOfMass;

        [SerializeField, Header("   Current Car Stats Scriptable Object"), Tooltip("In case you want to expose current car stats, " +
    "you can create a scriptable object and assign it here. " +
    "This gives you the ability to access current car stats in your other scripts. This field is optional")]
        private CurrentCarStats CurrentCarStats;

        //Abstract interface for handling input. Create a monobehaviour script that implements this interface,
        //or use the input scripts that come with this package
        private IVehicleControllerInputProvider _inputProvider;

        [SerializeField]
        private Separator Separator;


        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();

            FindInputProvider();

            if (CurrentCarStats == null)
                CurrentCarStats = ScriptableObject.CreateInstance<CurrentCarStats>();
            else
            {
                if (CurrentCarStats.ScriptableObjectOwners.Count > 0)
                    Debug.LogError("Assigning the same instance of CurrentCarStats Scriptable Object to different vehicles can lead to unexpected behaviour.");
                CurrentCarStats.ScriptableObjectOwners.Add(gameObject);
            }

            if (UsePreset && _vehiclePartsPreset == null)
            {
                _vehiclePartsPreset = VehiclePartsPresetSO.CreateDefaultVehiclePartsPresetSO();
#if UNITY_EDITOR
                Debug.Log("VehiclePartsPresetSO wasn't assigned, so default one was created instead.");
#endif
            }

            if (UsePreset)
                VehiclePartsSetWrapper = new(_vehiclePartsPreset, this);
            else
                VehiclePartsSetWrapper = new(_customizableSet, this);

            _carVisualsEssentials = GetComponent<CarVisualsEssentials>();
            _carVisualsEssentials.Initialize(_rigidbody, CurrentCarStats);

            VehicleControllerInitializer initializer = new();
            (_statsManager, _partsManager) = initializer.InitializeVehicleControllers(_frontAxles, _rearAxles,
                _steerAxles, _rigidbody, transform, VehiclePartsSetWrapper, _centerOfMass, CurrentCarStats);
        }

        private void Update()
        {
            if (UsePreset)
                VehiclePartsSetWrapper.UpdateVehiclePartsPresetIfRequired(_vehiclePartsPreset);
            else
                VehiclePartsSetWrapper.UpdateVehiclePartsPresetIfRequired(_customizableSet);

            _statsManager.ManageStats(_inputProvider.GetGasInput(), _inputProvider.GetBrakeInput(), _inputProvider.GetHandbrakeInput(),
                            _sidewaysSlippingThreshold, _forwardSlippingThreshold, DrivetrainType);
            _partsManager.ManageTransmissionUpShift(_inputProvider.GetGearUpInput());
            _partsManager.ManageTransmissionDownShift(_inputProvider.GetGearDownInput());

            _carVisualsEssentials.HandleWheelVisuals(_inputProvider.GetHorizontalInput(), _steerAxles[0].LeftHalfShaft.WheelController.SteerAngle, _steerAngle, _steerSpeed);
        }

        private void FixedUpdate()
        {
            _partsManager.ManageCarParts(_inputProvider.GetGasInput(), _inputProvider.GetBrakeInput(), _inputProvider.GetNitroBoostInput(),
                _inputProvider.GetHorizontalInput(), _inputProvider.GetHandbrakeInput(),
                _steerAngle, _steerSpeed, TransmissionType, DrivetrainType, _suspensionSimulationPrecision, _ignoreLayers);

            _partsManager.PerformAirControls(AerialControlsEnabled, AerialControlsSensitivity,
                _inputProvider.GetPitchInput(), _inputProvider.GetYawInput(), _inputProvider.GetRollInput());
        }

        public Transform GetCenterOfMass() => _centerOfMass;
        public CurrentCarStats GetCurrentCarStats() => CurrentCarStats;
        public Rigidbody GetRigidbody() => _rigidbody;

        public void SetVehiclePresetSO(VehiclePartsPresetSO newPreset)
        {
            _vehiclePartsPreset = newPreset;
            VehiclePartsSetWrapper.UpdateVehiclePartsPresetIfRequired(newPreset);
        }
        public VehiclePartsPresetSO GetVehiclePreset() => _vehiclePartsPreset;
        public void SetNewPartToCustomizableSet(IVehiclePart newPart, bool front = true)
        {
            switch (newPart)
            {
                case EngineSO:
                    _customizableSet.Engine = newPart as EngineSO;
                    break;

                case NitrousSO:
                    _customizableSet.Nitrous = newPart as NitrousSO;
                    break;

                case TransmissionSO:
                    _customizableSet.Transmission = newPart as TransmissionSO;
                    break;

                case SuspensionSO:
                    if (front)
                        _customizableSet.FrontSuspension = newPart as SuspensionSO;
                    else
                        _customizableSet.RearSuspension = newPart as SuspensionSO;
                    break;

                case TiresSO:
                    if (front)
                        _customizableSet.FrontTires = newPart as TiresSO;
                    else
                        _customizableSet.RearTires = newPart as TiresSO;
                    break;

                case BrakesSO:
                    _customizableSet.Brakes = newPart as BrakesSO;
                    break;

                case VehicleBodySO:
                    _customizableSet.Body = newPart as VehicleBodySO;
                    break;
            }
        }
        public VehiclePartsCustomizableSet GetCustomizableSet() => _customizableSet;

        public void AddNitroCharge(float amount) => _partsManager.AddNitro(amount);

        private void FindInputProvider()
        {
            IVehicleControllerInputProvider[] providersFound = GetComponentsInChildren<IVehicleControllerInputProvider>();

            if (providersFound.Length == 1)
            {
                _inputProvider = providersFound[0];
                return;
            }

            if (providersFound.Length == 0)
            {
                Debug.LogWarning($"No input provider script found on {gameObject.name}." +
                    " Input provider implemented using old input system was added." +
                    " Please add a script component that expends IVehicleControllerInputProvider interface.");
                _inputProvider = gameObject.AddComponent<VehicleControllerInputProvider>();
                return;
            }

            Debug.LogWarning($"Multiple Input Providers on {gameObject.name}. Selecting the first one found");
            _inputProvider = providersFound[0];
        }

        private void OnDestroy()
        {
            if (CurrentCarStats != null)
                CurrentCarStats.Reset();
            _statsManager.OnDestroy();
        }

    }

    [Serializable]
    public class VehiclePartsCustomizableSet
    {
        public EngineSO Engine;
        public NitrousSO Nitrous;
        public TransmissionSO Transmission;
        public TiresSO FrontTires;
        public TiresSO RearTires;
        public SuspensionSO FrontSuspension;
        public SuspensionSO RearSuspension;
        public BrakesSO Brakes;
        public VehicleBodySO Body;
    }
}

