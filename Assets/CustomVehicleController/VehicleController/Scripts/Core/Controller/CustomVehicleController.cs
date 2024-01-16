using System;
using UnityEngine;

namespace Assets.VehicleController
{
    [RequireComponent(typeof(CarVisualsEssentials)),
    RequireComponent(typeof(Rigidbody)), DisallowMultipleComponent, AddComponentMenu("CustomVehicleController/Core/Custom Vehicle Controller")]
    public class CustomVehicleController : MonoBehaviour
    {
        private VehicleControllerStatsManager _statsManager;
        private VehicleControllerPartsManager _partsManager;

        public VehicleStats VehicleStats;
        private CarVisualsEssentials _carVisualsEssentials;

        #region Handling Settings
        [Header("   Handling settings")]
        public DrivetrainType DrivetrainType;
        public TransmissionType TransmissionType;

        [Space, Tooltip("Maximum steering angle in degrees")]
        public float SteerAngle = 25;
        [Tooltip("Time in which wheels will reach maximum steering angle." +
            "\n Since steering uses a smooth damp function, this time is approximate.")]
        public float SteerSpeed = 0.2f;
        #endregion

        [Space, SerializeField, Tooltip("Center Of Mass of the vehicle. " +
            "\nUsually placed in the middle of the vehicle, slightly closer to the engine.")]
        private Transform _centerOfMass;

        #region Extra options
        [Header("   Extra options")]
        [Min(0f), Tooltip("Defines how much slipping is allowed until the wheel is considered to be forward slipping. " +
            "\n Forward slipping occurs when acceleration force is higher than the wheel load * tire grip.")]
        public float ForwardSlippingThreshold = 0.1f;
        [Min(0f), Tooltip("Defines how much slipping is allowed until the wheel is considered to be sideways slipping. " +
            "\n The slipping amount equals the dot product of the car forward vector and normalizd vehicle velocity.")]
        public float SidewaysSlippingThreshold = 0.5f;
        //allows you to control the car in air. 
        [Space]
        public bool AerialControlsEnabled = false;
        public float AerialControlsSensitivity = 0;
        #endregion



        [Header("   Physics"), SerializeField, Tooltip("Assign rigidbody component to avoid using the costly GetComponent operation")]
        private Rigidbody _rigidbody;
        [Range(1,27), Tooltip("The amount of raycasts that go along the forward axis of the wheel with an offset from -radius to +radius. " +
            "\n Recommended values: [3:9]")]
        public int SuspensionSimulationPrecision = 3;


        [SerializeField, Header("   Current Car Stats Scriptable Object"), Tooltip("In case you want to expose current car stats, " +
    "you can create a scriptable object and assign it here. " +
    "This gives you the ability to access current car stats in your other scripts. This field is optional")]
        private CurrentCarStats CurrentCarStats;


        //Abstract interface for handling input. Create a monobehaviour script that implements this interface,
        //or use the input scripts that come with this package
        private IVehicleControllerInputProvider _inputProvider;

        #region Wheel Controllers
        [SerializeField]
        private VehicleAxle[] _vehicleAxles;
        [SerializeField]
        private VehicleAxle[] _steerAxles;
        #endregion

        private void Awake()
        {
            if(_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();

            FindInputProvider();

            if (CurrentCarStats == null)
                CurrentCarStats = ScriptableObject.CreateInstance<CurrentCarStats>();
            else
                CurrentCarStats.Reset();

            _carVisualsEssentials = GetComponent<CarVisualsEssentials>();
            _carVisualsEssentials.Initialize(_rigidbody, CurrentCarStats);

            VehicleControllerInitializer initializer = new();
            (_statsManager, _partsManager) = initializer.InitializeVehicleControllers(_vehicleAxles,
                _steerAxles, _rigidbody, transform, VehicleStats, _centerOfMass, CurrentCarStats);
        }

        private void Update()
        {
            _statsManager.ManageStats(_inputProvider.GetGasInput(), _inputProvider.GetBrakeInput(), _inputProvider.GetHandbrakeInput(),
                            SidewaysSlippingThreshold, ForwardSlippingThreshold, DrivetrainType);
            _partsManager.ManageTransmissionUpShift(_inputProvider.GetGearUpInput());
            _partsManager.ManageTransmissionDownShift(_inputProvider.GetGearDownInput());

            _carVisualsEssentials.HandleWheelVisuals(_inputProvider.GetHorizontalInput(), _steerAxles[0].LeftHalfShaft.WheelController.SteerAngle, SteerAngle, SteerSpeed);
        }
        private void FixedUpdate()
        {
            _partsManager.ManageCarParts(_inputProvider.GetGasInput(), _inputProvider.GetBrakeInput(), _inputProvider.GetNitroBoostInput(),
                _inputProvider.GetHorizontalInput(), _inputProvider.GetHandbrakeInput(), 
                SteerAngle, SteerSpeed, TransmissionType, DrivetrainType, SuspensionSimulationPrecision);

            _partsManager.PerformAirControls(AerialControlsEnabled, AerialControlsSensitivity,
                _inputProvider.GetPitchInput(), _inputProvider.GetYawInput(), _inputProvider.GetRollInput());
        }

        public Transform GetCenterOfMass() => _centerOfMass;
        public CurrentCarStats GetCurrentCarStats() => CurrentCarStats;
        public Rigidbody GetRigidbody() => _rigidbody;

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
                _inputProvider = gameObject.AddComponent<VehicleControllerInputProviderOld>();
                return;
            }

            Debug.LogWarning("Multiple input provider scripts found on " + gameObject.name);
            for (int i = 0; i < providersFound.Length; i++)
            {
                if (providersFound[i] is VehicleControllerInputProviderPlayerInput || providersFound[i] is VehicleControllerInputProviderNew)
                {
                    _inputProvider = providersFound[i];
                    Debug.LogWarning("The one implenting the new input system was chosen");
                    return;
                }
            }

            _inputProvider = providersFound[0];
        }


#if UNITY_EDITOR
        //tell the scripts to update values when fields are changed
        private void OnValidate()
        {
            if(Application.isPlaying)
                VehicleStats.InvokeFieldChanged();
        }
#endif
    }

    [Serializable]
    public class VehicleStats
    {
#if UNITY_EDITOR
        public event Action OnFieldChanged;
        public void InvokeFieldChanged() => OnFieldChanged?.Invoke();
#endif
        public EngineSO EngineSO;
        public NitrousSO NitrousSO;
        public TransmissionSO TransmissionSO;
        public TiresSO FrontTiresSO;
        public TiresSO RearTiresSO;
        public SuspensionSO FrontSuspensionSO;
        public SuspensionSO RearSuspensionSO;
        public BrakesSO BrakesSO;
        public VehicleBodySO BodySO;
    }
}

