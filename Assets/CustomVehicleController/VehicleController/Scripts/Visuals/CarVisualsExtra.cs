using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Visuals/Car Visuals Extra"),
    HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/extra/adding-visual-effects")]
    public class CarVisualsExtra : NetworkBehaviour
    {
        [SerializeField]
        private CarVisualsEssentials _carVisualsEssentials;

        [SerializeField, Space, Space]
        private CurrentCarStats _currentCarStats;
        [SerializeField]
        private Rigidbody _rigidbody;

        #region Wheel Meshes
        [SerializeField]
        public VehicleAxle[] _axleArray;
        private WheelController[] _wheelControllerArray;
        private Transform[] _wheelMeshesArray;
        #endregion

        #region Extra Effects
        [Header("Extra Visual Effects")]
        [SerializeField]
        private bool _enableTireSmoke;
        [SerializeField]
        private TireSmokeParameters _tireSmokeParameters;
        private CarVisualsTireSmoke _tireSmoke;

        [SerializeField]
        private bool _enableTireTrails;
        [SerializeField]
        private TireTrailParameters _tireTrailParameters;
        private CarVisualsTireTrails _tireTrails;

        [SerializeField]
        private bool _enableBrakeLightsEffect;
        [SerializeField]
        private BrakeLightsParameters _brakeLightsParameters;
        private CarVisualsBrakeLights _brakeLightsEffect;


        [SerializeField]
        private bool _enableBodyAeroEffect;
        [SerializeField]
        private EffectTypeParameters _bodyEffectParameters;
        private CarVisualsBodyWindEffect _bodyWindEffect;


        [SerializeField]
        private bool _enableWingAeroEffect;
        [SerializeField]
        private WingAeroParameters _wingAeroParameters;
        private CarVisualsWingAeroEffect _wingAeroEffect;


        [SerializeField]
        private bool _enableAntiLagEffect;
        [SerializeField]
        private AntiLagParameters _antiLagParameters;
        private CarVisualsAntiLag _antiLagEffect;

        [SerializeField]
        private bool _enableNitroEffect;
        [SerializeField]
        private NitrousParameters _nitroParameters;
        private CarVisualsNitrous _nitroEffect;
        #endregion

        private const float DELAY_BEFORE_DISABLING_EFFECTS = 0.33f;
        private float[] _lastStopEmitTimeArray;

        //a custom property drawer will draw this as a black line and in the custom editor this property will be drawn after every field
        public Separator Separator;

        private NetworkVariable<float> _nitroIntensityNetVar = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> _sidewaysForceNetVar = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<bool> _accelNetVar = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<bool> _brakingNetVar = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private NetworkVariable<float> _speedNetVar = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private NetworkVariable<Vector3> _rbVelocityNetVar = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private NetworkVariable<NetworkShouldWheelsEmit> _networkShouldWheelsEmit = new NetworkVariable<NetworkShouldWheelsEmit>(
            new NetworkShouldWheelsEmit {
                FrontLeft = false, FrontRight = false, RearLeft = false, RearRight = false}, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Owner);

        private struct NetworkShouldWheelsEmit : INetworkSerializable
        {
            public bool FrontLeft;
            public bool FrontRight;
            public bool RearLeft;
            public bool RearRight;


            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref FrontLeft);
                serializer.SerializeValue(ref FrontRight);
                serializer.SerializeValue(ref RearLeft);
                serializer.SerializeValue(ref RearRight);
            }
        }

        private void Start()
        {
            if (_currentCarStats == null)
            {
                if (_carVisualsEssentials == null)
                    Debug.LogError("CurrentCarStats wasn't assigned and couldn't be found");
                else
                    _currentCarStats = _carVisualsEssentials.GetCurrentCarStats();
            }

            _lastStopEmitTimeArray = new float[_axleArray.Length * 2];

            _wheelControllerArray = VehicleAxle.ExtractVehicleWheelControllerArray(_axleArray);
            _wheelMeshesArray = VehicleAxle.ExtractVehicleWheelVisualTransformArray(_axleArray);

            TryInstantiateExtraEffects();
        }

        private void Reset()
        {
#if !VISUAL_EFFECT_GRAPH_INSTALLED
            _tireSmokeParameters.VisualEffect.VisualEffectType = VisualEffectAssetType.Type.ParticleSystem;
            _bodyEffectParameters.VisualEffectType = VisualEffectAssetType.Type.ParticleSystem;
            _antiLagParameters.VisualEffect.VisualEffectType = VisualEffectAssetType.Type.ParticleSystem;
            _nitroParameters.VisualEffect.VisualEffectType = VisualEffectAssetType.Type.ParticleSystem;
#endif
        }

        private void LateUpdate()
        {
            UpdateNetworkVariables();

            if (_enableTireSmoke || _enableTireTrails)
                ShouldEmitWheelEffects();

            if (_enableNitroEffect)
                _nitroEffect.HandleNitroEffect(_nitroIntensityNetVar.Value, _sidewaysForceNetVar.Value, _accelNetVar.Value);

            if (_enableTireSmoke)
                DisplaySmokeEffects();

            if (_enableTireTrails)
                DisplaySkidMarksEffects();

            if (_enableBrakeLightsEffect)
                _brakeLightsEffect.HandleRearLights(_brakingNetVar.Value);

            if (!IsOwner)
                return;

            if (_enableBodyAeroEffect)
                _bodyWindEffect.HandleSpeedEffect(_speedNetVar.Value, _rbVelocityNetVar.Value);
            
            if (_enableWingAeroEffect)
                _wingAeroEffect.HandleWingAeroEffect();
        }

        private void UpdateNetworkVariables()
        {
            if (!IsOwner)
                return;

            _nitroIntensityNetVar.Value = _currentCarStats.NitroIntensity;
            _sidewaysForceNetVar.Value = _currentCarStats.SidewaysForce;
            _accelNetVar.Value = _currentCarStats.Accelerating;
            _speedNetVar.Value = _currentCarStats.SpeedInMsPerS;
            _rbVelocityNetVar.Value = _rigidbody.velocity;
            _brakingNetVar.Value = _currentCarStats.Braking;
        }

        private void TryInstantiateExtraEffects()
        {
            if (_enableTireSmoke)
                _tireSmoke = new(_wheelMeshesArray, _wheelControllerArray, transform, _tireSmokeParameters);

            if (_enableTireTrails)
                _tireTrails = new(_wheelMeshesArray, _wheelControllerArray, _tireTrailParameters);

            if (_enableAntiLagEffect)
                _antiLagEffect = new(this, _currentCarStats, _antiLagParameters);

            if (_enableNitroEffect)
                _nitroEffect = new(_nitroParameters, _currentCarStats);

            if (_enableBrakeLightsEffect)
                _brakeLightsEffect = new(_brakeLightsParameters);

            if (!IsOwner)
                return;

            if (_enableBodyAeroEffect)
                _bodyWindEffect = new(_bodyEffectParameters, transform);

            if (_enableWingAeroEffect)
                _wingAeroEffect = new(_wingAeroParameters, _currentCarStats);
        }

        private void ShouldEmitWheelEffects()
        {
            if (!IsOwner)
                return;

            bool frontLeft = _wheelControllerArray[0].HasContactWithGround && _currentCarStats.WheelSlipArray[0];
            if (frontLeft)
                _lastStopEmitTimeArray[0] = Time.time;
            else
                frontLeft = Time.time < _lastStopEmitTimeArray[0] + DELAY_BEFORE_DISABLING_EFFECTS;

            bool frontRight = _wheelControllerArray[1].HasContactWithGround && _currentCarStats.WheelSlipArray[1];
            if (frontRight)
                _lastStopEmitTimeArray[1] = Time.time;
            else
                frontRight = Time.time < _lastStopEmitTimeArray[1] + DELAY_BEFORE_DISABLING_EFFECTS;

            bool rearLeft = _wheelControllerArray[2].HasContactWithGround && _currentCarStats.WheelSlipArray[2];
            if (rearLeft)
                _lastStopEmitTimeArray[2] = Time.time;
            else
                rearLeft = Time.time < _lastStopEmitTimeArray[2] + DELAY_BEFORE_DISABLING_EFFECTS;

            bool rearRight = _wheelControllerArray[3].HasContactWithGround && _currentCarStats.WheelSlipArray[3];
            if (rearRight)
                _lastStopEmitTimeArray[3] = Time.time;
            else
                rearRight = Time.time < _lastStopEmitTimeArray[3] + DELAY_BEFORE_DISABLING_EFFECTS;

            _networkShouldWheelsEmit.Value = new NetworkShouldWheelsEmit {
                FrontLeft = frontLeft,
                FrontRight = frontRight,
                RearLeft = rearLeft,
                RearRight = rearRight,
            };
        }

        private void DisplaySmokeEffects()
        {
            _tireSmoke.HandleSmokeEffects(_networkShouldWheelsEmit.Value.FrontLeft, 0, _rbVelocityNetVar.Value, _speedNetVar.Value);
            _tireSmoke.HandleSmokeEffects(_networkShouldWheelsEmit.Value.FrontRight, 1, _rbVelocityNetVar.Value, _speedNetVar.Value);
            _tireSmoke.HandleSmokeEffects(_networkShouldWheelsEmit.Value.RearLeft, 2, _rbVelocityNetVar.Value, _speedNetVar.Value);
            _tireSmoke.HandleSmokeEffects(_networkShouldWheelsEmit.Value.RearRight, 3, _rbVelocityNetVar.Value, _speedNetVar.Value);
        }

        private void DisplaySkidMarksEffects()
        {
            _tireTrails.DisplayTireTrail(_networkShouldWheelsEmit.Value.FrontLeft, 0);
            _tireTrails.DisplayTireTrail(_networkShouldWheelsEmit.Value.FrontRight, 1);
            _tireTrails.DisplayTireTrail(_networkShouldWheelsEmit.Value.RearLeft, 2);
            _tireTrails.DisplayTireTrail(_networkShouldWheelsEmit.Value.RearRight, 3);
        }

        public void CopyValuesFromEssentials()
        {
            if (_carVisualsEssentials == null)
            {
                Debug.LogError("CarVisualsEssentials is not assigned");
                return;
            }
            _axleArray = _carVisualsEssentials.GetAxleArray();

            _currentCarStats = _carVisualsEssentials.GetCurrentCarStats();
            _rigidbody = _carVisualsEssentials.GetRigidbody();
        }

        [ServerRpc]
        public void RequestAntiLagServerRpc()
        {
            PlayAntiLagClientRpc();
        }

        [ClientRpc]
        private void PlayAntiLagClientRpc()
        {
            _antiLagEffect.PlayAntiLag();
        }

        [ServerRpc]
        public void RequestShiftedAntiLagServerRpc()
        {
            PlayShiftedAntiLagClientRpc();
        }

        [ClientRpc]
        private void PlayShiftedAntiLagClientRpc()
        {
            _antiLagEffect.PlayShiftedAntiLag();
        }

        private void OnDestroy()
        {
            if (!IsOwner)
                return;

            if (_enableAntiLagEffect)
                _antiLagEffect.OnDestroy();
        }
    }

    [Serializable]
    public class AntiLagParameters
    {
        public EffectTypeParameters VisualEffect;
        public Transform[] ExhaustsPositionArray;
        public float BackfireDelay = 0.25f;
        [Min(1)]
        public int MinBackfireCount = 2;
        [Min(1)]
        public int MaxBackfireCount = 5;
    }

    [Serializable]
    public class TireSmokeParameters
    {
        public EffectTypeParameters VisualEffect;
        public float VerticalOffset;
    }

    [Serializable]
    public class EffectTypeParameters
    {
        public VisualEffectAssetType.Type VisualEffectType;
        public ParticleSystem ParticleSystem;
#if VISUAL_EFFECT_GRAPH_INSTALLED
        public VisualEffectAsset VFXAsset;
#endif
    }

    [Serializable]
    public class BrakeLightsParameters
    {
        public MeshRenderer[] RearLightMeshes;
        [ColorUsageAttribute(true, true)]
        public Color BrakeColor;
    }

    [Serializable]
    public class WingAeroParameters
    {
        public TrailRenderer[] TrailRendererArray;
        public int MinSpeedToDisplay = 20;
        [Range(0, 1f)]
        public float MaxAlpha = 0.5f;
    }

    [Serializable]
    public class TireTrailParameters
    {
        public TrailRenderer TrailRenderer;
        public float VerticalOffset;
    }

    [Serializable]
    public class NitrousParameters
    {
        public EffectTypeParameters VisualEffect;
        public Transform[] ExhaustsPositionArray;
        [GradientUsageAttribute(true), Header("For ParticleSystem, change material properties instead")]
        public Gradient Gradient;
    }

    [Serializable]
    public class Separator { }
}

