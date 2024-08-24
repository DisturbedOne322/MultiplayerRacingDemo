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
    private CinemachineOrbitalTransposer _transposer;

    public float Zoffset = -4f;

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
    private float _vigGain = 0.2f;
    private ChromaticAberration _chromaticAberration;

    [SerializeField]
    private CustomVehicleController _customVehicleController;

    [SerializeField]
    private float _maxEffectSpeed = 100;

    [SerializeField]
    private float _defaultFOV = 50f;
    [SerializeField]
    private float _minFov = 40f;
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
    private float _maxFreq = 2.5f;
    [SerializeField]
    private float _maxAmp = 0.75f;

    private float _fovSmDampVelocity;
    private float _smDampTime = 0.15f;

    private bool _lookingBack = false;

    private float _collisionOffset;

    public void SetOffsetFromCollider(float offset)
    {
        _collisionOffset = offset;
    }

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<CinemachineVirtualCamera>();
        _noise = _camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        _localVolume.profile.TryGet<Vignette>(out  _vignette);
        _localVolume.profile.TryGet<ChromaticAberration>(out _chromaticAberration);

        _transposer = _camera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
    }

    // Update is called once per frame
    void Update()
    {
        ManageCameraOffset();
        CurrentCarStats currentCarStats = _customVehicleController.GetCurrentCarStats();

        if (currentCarStats == null)
            return;

        float speed = currentCarStats.SpeedInMsPerS;
        float nitroIntensity = currentCarStats.NitroIntensity == 1 && currentCarStats.Accelerating ? 1 : 0;
        float speedPercent = Mathf.Clamp01(speed / _maxEffectSpeed);
        bool nitroBoosting = currentCarStats.NitroBoosting;

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

        if (targetFOV < _minFov)
            targetFOV = _minFov;

        _camera.m_Lens.FieldOfView = Mathf.SmoothDamp(_camera.m_Lens.FieldOfView, targetFOV, ref _fovSmDampVelocity, _smDampTime);

        HandleAnimeSpeedEffect(currentCarStats.SpeedInMsPerS, currentCarStats.NitroIntensity == 1);
        HandleCameraUserMovement();
    }

    private float _lastInputTime = 0;

    private void HandleCameraUserMovement()
    {
        float recenterDelayInput = 5;
        float recenterDelayNoInput = 0.3f;

        float input = _transposer.m_XAxis.m_InputAxisValue;

        bool accelerating = _customVehicleController.GetCurrentCarStats().Accelerating;
        bool braking = _customVehicleController.GetCurrentCarStats().Braking;
        bool inputExists = (accelerating || braking) && !(accelerating && braking);

        if (input != 0)
            _lastInputTime = Time.time;

        if (_lastInputTime + 0.5f > Time.time)
            input = 1;

        if (inputExists)
        {
            _transposer.m_RecenterToTargetHeading.m_WaitTime = input == 0 ? recenterDelayNoInput : recenterDelayInput;
        }
        else
        {
            _transposer.m_RecenterToTargetHeading.m_WaitTime = recenterDelayInput;
        }

    }

    private void ManageCameraOffset()
    {
        Vector3 offset = new Vector3(_transposer.m_FollowOffset.x, _transposer.m_FollowOffset.y, 0);


        if(_lookingBack)
            offset.z = -Zoffset;
        else
            offset.z = Zoffset + _collisionOffset;

        _transposer.m_FollowOffset = offset;
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
