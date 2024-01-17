using System;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Visuals/Car Visuals Extra")]
    public class CarVisualsExtra : MonoBehaviour
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

        [SerializeField]
        private bool _enableCollisionEffects;
        [SerializeField]
        private CollisionEffectParameters _collisionEffectsParameters;
        private CarVisualsCollisionEffects _collisionEffects;
        #endregion

        private const float DELAY_BEFORE_DISABLING_EFFECTS = 0.33f;
        private float[] _lastStopEmitTimeArray;
        private bool[] _shouldEmitArray;

        //a custom property drawer will draw this as a black line and in the custom editor this property will be drawn after every field
        public Separator Separator;

        private void Awake()
        {
            _lastStopEmitTimeArray = new float[_axleArray.Length * 2];
            _shouldEmitArray = new bool[_axleArray.Length * 2];

            _wheelControllerArray = VehicleAxle.ExtractVehicleWheelControllerArray(_axleArray);
            _wheelMeshesArray = VehicleAxle.ExtractVehicleWheelVisualTransformArray(_axleArray);

            TryInstantiateExtraEffects();
        }


        private void Update()
        {
            if (_enableTireSmoke || _enableTireTrails)
                ShouldEmitWheelEffects();

            if (_enableTireSmoke)
                DisplaySmokeEffects();
            if (_enableTireTrails)
                DisplaySkidMarksEffects();

            if(_enableNitroEffect)
                _nitroEffect.HandleNitroEffect();

            if (_enableBodyAeroEffect)
                _bodyWindEffect.HandleSpeedEffect(_currentCarStats.SpeedInMsPerS, _rigidbody.velocity);

            if (_enableWingAeroEffect)
                _wingAeroEffect.HandleWingAeroEffect();

            if (_enableBrakeLightsEffect)
                _brakeLightsEffect.HandleRearLights(_currentCarStats.Braking);
        }

        private void TryInstantiateExtraEffects()
        {
            if(_enableTireSmoke)
                _tireSmoke = new (_wheelMeshesArray, _wheelControllerArray, transform, _tireSmokeParameters);

            if (_enableTireTrails)
                _tireTrails = new(_wheelMeshesArray, _wheelControllerArray, _tireTrailParameters);

            if (_enableAntiLagEffect)
                _antiLagEffect = new(this, _currentCarStats, _antiLagParameters);

            if (_enableNitroEffect)
                _nitroEffect = new(_nitroParameters, _currentCarStats);

            if (_enableBrakeLightsEffect)
                _brakeLightsEffect = new(_brakeLightsParameters);

            if (_enableBodyAeroEffect)
                _bodyWindEffect = new(_bodyEffectParameters, transform);

            if (_enableWingAeroEffect)
                _wingAeroEffect = new(_wingAeroParameters, _currentCarStats);

            if (_enableCollisionEffects)
                _collisionEffects = new CarVisualsCollisionEffects(_collisionEffectsParameters, transform);
        }

        private void ShouldEmitWheelEffects()
        {
            for (int i = 0; i < _axleArray.Length * 2; i++)
            {
                if (_currentCarStats.WheelSlipArray[i])
                {
                    _shouldEmitArray[i] = true;
                    _lastStopEmitTimeArray[i] = Time.time;
                }
                else
                {
                    _shouldEmitArray[i] = false;
                }
            }
        }

        private void DisplaySmokeEffects()
        {
            Vector3 velocityNormalized = _rigidbody.velocity.normalized;
            for (int i = 0; i < _wheelMeshesArray.Length; i++)
            {
                if (!_wheelControllerArray[i].HasContactWithGround)
                {
                    _tireSmoke.HandleSmokeEffects(false, i,
                        velocityNormalized, _currentCarStats.SpeedInMsPerS);
                    continue;
                }

                if (_shouldEmitArray[i])
                {
                    _tireSmoke.HandleSmokeEffects(true, i,
                        velocityNormalized, _currentCarStats.SpeedInMsPerS);
                }
                else
                {
                    bool display = Time.time < _lastStopEmitTimeArray[i] + DELAY_BEFORE_DISABLING_EFFECTS;
                    _tireSmoke.HandleSmokeEffects(display, i,
                        velocityNormalized, _currentCarStats.SpeedInMsPerS);
                }
            }
        }

        private void DisplaySkidMarksEffects()
        {
            for (int i = 0; i < _wheelMeshesArray.Length; i++)
            {
                if (!_wheelControllerArray[i].HasContactWithGround)
                {
                    _tireTrails.DisplayTireTrail(false, i);
                    continue;
                }

                if (_shouldEmitArray[i])
                {
                    _tireTrails.DisplayTireTrail(true, i);

                }
                else
                {
                    bool display = Time.time < _lastStopEmitTimeArray[i] + DELAY_BEFORE_DISABLING_EFFECTS;
                    _tireTrails.DisplayTireTrail(display, i);
                }
            }
        }

        public void CopyValuesFromEssentials()
        {
            if (_carVisualsEssentials == null)
            {
                Debug.LogError("CarVisualsEssentials is not assigned");
                return;
            }
            _axleArray= _carVisualsEssentials.GetAxleArray();

            _currentCarStats = _carVisualsEssentials.GetCurrentCarStats();
            _rigidbody = _carVisualsEssentials.GetRigidbody();
        }

        private void OnDestroy()
        {
            if(_enableAntiLagEffect)
                _antiLagEffect.OnDestroy();

            if(_enableCollisionEffects)
                _collisionEffects.OnDestroy();
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
        public VisualEffectAsset VFXAsset;
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
        public int MinSpeedMStoDisplay = 20;
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
        public AnimationCurve SizeOverLifeCurve;
        [GradientUsageAttribute(true)]
        public Gradient Gradient;
    }

    [Serializable]
    public class CollisionEffectParameters
    {
        public EffectTypeParameters ContinousVisualEffect;
        public float HorizontalOffset;
        [Min(0.1f)]
        public float SparksSpawnAreaHeight;
        public EffectTypeParameters BurstVisualEffect;
        [Min(0)]
        public float BurstSparkCooldown = 0.5f;
        public Light CollisionLight;
        public CollisionHandler CollisionHandler;
    }

    [Serializable]
    public class Separator { }
}

