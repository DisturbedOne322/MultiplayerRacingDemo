using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

namespace Assets.VehicleController
{
    public class CameraHighSpeedEffect : MonoBehaviour
    {
        [SerializeField]
        private CinemachineVirtualCamera _virtualCamera;
        private CinemachineBasicMultiChannelPerlin _multiChannelPerlin;

        [SerializeField]
        private VisualEffect _animeSpeedEffect;

        private const string SPAWN_RATE_FIELD_NAME = "SpawnRate";

        [SerializeField]
        private Volume _volume;
        private ChromaticAberration _chromaticAberration;
        private Vignette _vignette;

        [SerializeField]
        private CurrentCarStats _currentCarStats;

        [SerializeField]
        private float _maxShakeAmp = 0.8f;
        [SerializeField]
        private float _maxShakeFreq = 12;

        [SerializeField]
        private float DefaultFOV = 60;

        [SerializeField]
        private float MAX_ACCELERATION_FORCE = 10;
        [SerializeField]
        private float MAX_FOV_INCREASE_FOR_ACCEL = 10;
        [SerializeField]
        private float MAX_FOV_INCREASE_FOR_SPEED = 10;
        [SerializeField]
        private float MAX_SPEED_IN_MS = 80;
        [SerializeField]
        private float MAX_SIDEWAYS_FORCE = 40;

        [SerializeField]
        private float _nitroBoostFOV_Max = 20f;
        private float _nitroFOV = 0;
        private float _nitroBoostSmDampVelocity;
        private float _nitroSmDampTime = 0.5f;


        private const float DEFAULT_VIG_INTENSITY = 0.15f;
        private const float MAX_VIG_INTENSITY = 0.3f;

        private float _smDampVelocityFOV;
        private float _smDampVelocityVignette;
        private const float SmDampSpeed = 0.5f;


        private void Awake()
        {
            _multiChannelPerlin = _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            _volume.profile.TryGet(out _chromaticAberration);
            _volume.profile.TryGet(out _vignette);
        }

        // Update is called once per frame
        void Update()
        {
            float speedPercent = Mathf.Clamp01(_currentCarStats.SpeedInMsPerS / MAX_SPEED_IN_MS);
            speedPercent *= speedPercent;

            float acceleration = _currentCarStats.AccelerationForce;
            if (acceleration < 0)
                acceleration /= 4;

            HandleCameraShakeEffect(speedPercent);
            HandleFOV(speedPercent, acceleration);
            HandleVignette(speedPercent);
            HandleAnimeSpeedEffect();
        }

        private void HandleCameraShakeEffect(float speedPercent)
        {
            float value = Mathf.Clamp01(speedPercent + Mathf.Abs(_currentCarStats.SidewaysForce / MAX_SIDEWAYS_FORCE));
            _multiChannelPerlin.m_FrequencyGain = value * _maxShakeFreq;
            _multiChannelPerlin.m_AmplitudeGain = value * _maxShakeAmp;
        }

        private void HandleFOV(float speedPercent, float accel)
        {
            float speedFOV = speedPercent * MAX_FOV_INCREASE_FOR_SPEED;
            float accelFOV = Mathf.Clamp(accel / MAX_ACCELERATION_FORCE, -1, 1) * MAX_FOV_INCREASE_FOR_ACCEL;

            float target = 0;
            if (_currentCarStats.Accelerating && _currentCarStats.NitroBoosting)
                target = _nitroBoostFOV_Max;
            _chromaticAberration.intensity.value = _nitroFOV / _nitroBoostFOV_Max;

            _nitroFOV = Mathf.SmoothDamp(_nitroFOV, target, ref _nitroBoostSmDampVelocity, _nitroSmDampTime);
            _virtualCamera.m_Lens.FieldOfView = Mathf.SmoothDamp(_virtualCamera.m_Lens.FieldOfView,
                DefaultFOV + speedFOV + accelFOV + _nitroFOV, ref _smDampVelocityFOV, SmDampSpeed);
        }

        private void HandleVignette(float speedPercent)
        {
            _vignette.intensity.value = Mathf.SmoothDamp(_vignette.intensity.value,
                DEFAULT_VIG_INTENSITY + (MAX_VIG_INTENSITY - DEFAULT_VIG_INTENSITY) * speedPercent, ref _smDampVelocityVignette, SmDampSpeed);
        }

        private void HandleAnimeSpeedEffect()
        {
            _animeSpeedEffect.SetFloat(SPAWN_RATE_FIELD_NAME, _currentCarStats.SpeedInMsPerS);
        }
    }
}
