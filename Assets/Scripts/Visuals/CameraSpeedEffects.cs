using Assets.VehicleController;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

public class CameraSpeedEffects : MonoBehaviour
{
    private CinemachineVirtualCamera _camera;
    private CinemachineBasicMultiChannelPerlin _noise;
    private CinemachineTransposer _transposer;

    private float _zOffset = -3.2f;

    [SerializeField]
    private VisualEffect _animeSpeedEffect;
    private float _minEffectRadius = 1.8f;
    private float _maxEffectRadius = 2.3f;

    private float _spawnRateAddDuringBoost = 80;
    private float _maxSpawnRateFromSpeed = 80;
    private float _maxEffectAlpha = 0.11f;

    [SerializeField]
    private Volume _localVolume;
    private Vignette _vignette;
    private float _vigMin = 0.2f;
    private float _vigGain = 0.13f;
    private ChromaticAberration _chromaticAberration;

    [SerializeField]
    private CustomVehicleController _customVehicleController;

    [SerializeField]
    private float _maxEffectSpeed = 100;

    [SerializeField]
    private float _defaultFOV = 45f;
    [SerializeField]
    private float _minFov = 37f;
    [SerializeField]
    private float _fovGainFromAccel = 20f;
    [SerializeField]
    private float _maxAccelForce = 50f;
    [SerializeField]
    private float _fovGainFromSpeed = 7f;
    [SerializeField]
    private float _fovGainFromBoost = 25f;
    private float _nitroFOV = 0;

    [SerializeField]
    private float _maxFreq = 3.5f;
    [SerializeField]
    private float _maxAmp = 1.5f;

    private float _smDampVelocity;
    private float _smDampTime = 0.1f;

    private bool _lookingBack = false;

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<CinemachineVirtualCamera>();
        _noise = _camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        _localVolume.profile.TryGet<Vignette>(out  _vignette);
        _localVolume.profile.TryGet<ChromaticAberration>(out _chromaticAberration);

        _transposer = _camera.GetCinemachineComponent<CinemachineTransposer>();
    }

    // Update is called once per frame
    void Update()
    {
        _lookingBack = Input.GetKey(KeyCode.C);

        CurrentCarStats currentCarStats = _customVehicleController.GetCurrentCarStats();

        if (currentCarStats == null)
            return;

        float speed = currentCarStats.SpeedInMsPerS;
        float nitroIntensity = currentCarStats.NitroIntensity == 1 ? 1 : 0;
        float speedPercent = Mathf.Clamp01(speed / _maxEffectSpeed);
        bool nitroBoosting = currentCarStats.NitroBoosting;

        _transposer.m_FollowOffset = new Vector3(_transposer.m_FollowOffset.x, _transposer.m_FollowOffset.y, _lookingBack ? -_zOffset : _zOffset);

        _chromaticAberration.intensity.value = nitroIntensity;

        _vignette.intensity.value = _vigMin + _vigGain * speedPercent;

        _noise.m_FrequencyGain = _maxFreq * speedPercent;
        _noise.m_AmplitudeGain = _maxAmp * speedPercent;

        if (nitroBoosting)
            _nitroFOV = nitroIntensity * nitroIntensity;
        else
            _nitroFOV -= Time.deltaTime;

        _nitroFOV = Mathf.Clamp01(_nitroFOV);

        float accelForce = (currentCarStats.AccelerationForce / _maxAccelForce);
        if (accelForce < 0)
            accelForce /= 4;

        float targetFOV = _defaultFOV + _fovGainFromAccel * accelForce + _fovGainFromSpeed * speedPercent + _nitroFOV * _fovGainFromBoost;

        if(targetFOV < _minFov)
            targetFOV = _minFov;

        _camera.m_Lens.FieldOfView = Mathf.SmoothDamp(_camera.m_Lens.FieldOfView, targetFOV, ref _smDampVelocity, _smDampTime);

        HandleAnimeSpeedEffect(currentCarStats.SpeedInMsPerS, currentCarStats.NitroIntensity == 1);
    }

    private void HandleAnimeSpeedEffect(float speed, bool boosting)
    {
        float spawnRate = Mathf.Clamp01(speed * 1.5f / _maxEffectSpeed) * _maxSpawnRateFromSpeed;
        float alpha = Mathf.Clamp01(speed / _maxEffectSpeed) * _maxEffectAlpha;
        if (boosting)
        {
            spawnRate += _spawnRateAddDuringBoost;
            alpha = _maxEffectAlpha;
        }
        float radius = _maxEffectRadius - (_maxEffectRadius - _minEffectRadius) * Mathf.Clamp01(speed / _maxEffectSpeed);

        _animeSpeedEffect.SetFloat("Radius", radius);
        _animeSpeedEffect.SetFloat("SpawnRate", spawnRate);
        _animeSpeedEffect.SetFloat("MaxAlpha", alpha);
    }
}
