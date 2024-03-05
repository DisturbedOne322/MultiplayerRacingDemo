using Assets.VehicleController;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraSpeedEffects : MonoBehaviour
{
    private CinemachineVirtualCamera _camera;
    private CinemachineBasicMultiChannelPerlin _noise;

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

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<CinemachineVirtualCamera>();
        _noise = _camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        _localVolume.profile.TryGet<Vignette>(out  _vignette);
        _localVolume.profile.TryGet<ChromaticAberration>(out _chromaticAberration);
    }

    // Update is called once per frame
    void Update()
    {
        CurrentCarStats currentCarStats = _customVehicleController.GetCurrentCarStats();

        float speedPercent = Mathf.Clamp01(currentCarStats.SpeedInMsPerS / _maxEffectSpeed);

        _chromaticAberration.intensity.value = currentCarStats.NitroIntensity == 1 ? 1 : 0;

        _vignette.intensity.value = _vigMin + _vigGain * speedPercent;

        _noise.m_FrequencyGain = _maxFreq * speedPercent;
        _noise.m_AmplitudeGain = _maxAmp * speedPercent;

        if (currentCarStats.NitroBoosting)
            _nitroFOV = currentCarStats.NitroIntensity * currentCarStats.NitroIntensity;
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
    }
}
