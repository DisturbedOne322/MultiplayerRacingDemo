using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Sound/Vehicle Extra Sound Manager")]
    public class CustomVehicleControllerExtraSoundManager : MonoBehaviour
    {
        [SerializeField]
        private CarExtraSoundsSO _extraSoundSO;
        [SerializeField]
        private CurrentCarStats _currentCarStats;

        private AudioSource _forcedInductionAudioSource;
        private AudioSource _tireSlipAudioSource;
        private AudioSource _carEffectsAudioSource;
        private AudioSource _windNoiseAudioSource;

        [SerializeField]
        private AudioMixerGroup _vehicleSoundAudioMixerGroup;

        [SerializeField, Min(0)]
        private float _antiLagSoundCooldown = 1f;
        private float lastAntiLag;

        [SerializeField, Min(1)]
        private float _forcedInductionMaxPitch = 1.5f;
        [SerializeField, Min(0)]
        private float _forcedInductionMaxVolume = 0.7f;

        private const float TIRE_VOLUME_INREASE_TIME = 0.75f;

        [SerializeField]
        private AudioClip _windNoise;
        [SerializeField, Range(0,1f)]
        private float _maxWindVolume = 0.3f;

        private bool _forcedInductionSoundInitialized = false;
        private bool _tireSlipSoundInitialized = false;

        private bool _flutterSoundExists = false;
        private bool _antiLagSoundExists = false;
        private bool _antiLagMildSoundExists = false;

        private GameObject _effectAudioSourceHolder;

        private void Awake()
        {
            _effectAudioSourceHolder = new GameObject("Car Sound Effects");
            _effectAudioSourceHolder.transform.parent = this.transform;
            _effectAudioSourceHolder.transform.localPosition = Vector3.zero;

            InitializeForcedInductionSound();
            InitializeTireSlipSound();
            InitializeCarEffectSound();
            InitializedWindNoise();
            _currentCarStats.OnAntiLag += _currentCarStats_OnAntiLag;
            _currentCarStats.OnShiftedAntiLag += _currentCarStats_OnShiftedAntiLag;
        }

        private void OnDestroy()
        {
            if (_currentCarStats == null)
                return;
            _currentCarStats.OnAntiLag -= _currentCarStats_OnAntiLag;
            _currentCarStats.OnShiftedAntiLag -= _currentCarStats_OnShiftedAntiLag;
        }

        private void _currentCarStats_OnShiftedAntiLag()
        {
            if (_antiLagMildSoundExists && Time.time > lastAntiLag + _antiLagSoundCooldown)
            {
                _carEffectsAudioSource.PlayOneShot(_extraSoundSO.AntiLagMildSounds[UnityEngine.Random.Range(0, _extraSoundSO.AntiLagMildSounds.Length)], 1 / _carEffectsAudioSource.volume);
                lastAntiLag = Time.time;
            }

            if(_flutterSoundExists)
                _carEffectsAudioSource.PlayOneShot(_extraSoundSO.TurboFlutterSound);
        }

        private void _currentCarStats_OnAntiLag()
        {
            if (_flutterSoundExists)
                _carEffectsAudioSource.PlayOneShot(_extraSoundSO.TurboFlutterSound);

            if (_antiLagSoundExists)
                _carEffectsAudioSource.PlayOneShot(_extraSoundSO.AntiLagSound);
        }

        private void InitializeForcedInductionSound()
        {
            if (_extraSoundSO.ForcedInductionSound == null)
            {
                Debug.LogWarning("No forced induction sound is assigned to scriptable object.");
                return;
            }
            _forcedInductionAudioSource = _effectAudioSourceHolder.AddComponent<AudioSource>();
            _forcedInductionAudioSource.clip = _extraSoundSO.ForcedInductionSound;
            _forcedInductionAudioSource.volume = 0;
            SetupLoopingAudioSource(_forcedInductionAudioSource);

            if (_vehicleSoundAudioMixerGroup != null)
                _forcedInductionAudioSource.outputAudioMixerGroup = _vehicleSoundAudioMixerGroup;

            _forcedInductionSoundInitialized = true;
        }

        private void InitializeTireSlipSound()
        {
            if (_extraSoundSO.TireSlipSound == null)
            {
                Debug.LogWarning("No tire slip sound is assigned to scriptable object.");
                return;
            }
            _tireSlipAudioSource = _effectAudioSourceHolder.AddComponent<AudioSource>();
            _tireSlipAudioSource.clip = _extraSoundSO.TireSlipSound;
            _tireSlipAudioSource.volume = 0;
            SetupLoopingAudioSource(_tireSlipAudioSource);

            if (_vehicleSoundAudioMixerGroup != null)
                _tireSlipAudioSource.outputAudioMixerGroup = _vehicleSoundAudioMixerGroup;

            _tireSlipSoundInitialized = true;
        }

        private void InitializeCarEffectSound()
        {
            _carEffectsAudioSource = _effectAudioSourceHolder.AddComponent<AudioSource>();
            SetupLoopingAudioSource(_carEffectsAudioSource);
            _flutterSoundExists = _extraSoundSO.TurboFlutterSound != null;
            _antiLagSoundExists = _extraSoundSO.AntiLagSound != null;
            _antiLagMildSoundExists = _extraSoundSO.AntiLagMildSounds.Length > 0;

            if (_vehicleSoundAudioMixerGroup != null)
                _carEffectsAudioSource.outputAudioMixerGroup = _vehicleSoundAudioMixerGroup;
        }

        private void InitializedWindNoise()
        {
            _windNoiseAudioSource = _effectAudioSourceHolder.AddComponent<AudioSource>();
            _windNoiseAudioSource.clip = _windNoise;
            _windNoiseAudioSource.volume = 0;
            SetupLoopingAudioSource(_windNoiseAudioSource);

            if (_vehicleSoundAudioMixerGroup != null)
                _windNoiseAudioSource.outputAudioMixerGroup = _vehicleSoundAudioMixerGroup;
        }

        private void SetupLoopingAudioSource(AudioSource source)
        {
            source.loop = true;
            source.Play();
        }

        private void Update()
        {
            if (_forcedInductionSoundInitialized)
                HandleForcedInductionSound();
            if (_tireSlipSoundInitialized)
                HandleTireSlipSound();
            if (_windNoise != null)
                HandleWindNoise();
        }

        private void HandleForcedInductionSound()
        {
            _forcedInductionAudioSource.volume = _currentCarStats.ForcedInductionBoostPercent * _forcedInductionMaxVolume;
            _forcedInductionAudioSource.pitch = _currentCarStats.ForcedInductionBoostPercent * _forcedInductionMaxPitch;
        }

        private void HandleTireSlipSound()
        {
            if (_currentCarStats.DriveWheelLostContact)
            {
                _tireSlipAudioSource.volume = 0;
                if (_tireSlipAudioSource.isPlaying)
                    _tireSlipAudioSource.Stop();

                return;
            }

            if (_currentCarStats.IsCarSlipping)
            {
                if (!_tireSlipAudioSource.isPlaying)
                {
                    _tireSlipAudioSource.Play();
                }
                _tireSlipAudioSource.volume += Time.deltaTime / (TIRE_VOLUME_INREASE_TIME);
            }
            else
            {
                if (!_tireSlipAudioSource.isPlaying)
                    return;

                _tireSlipAudioSource.volume -= Time.deltaTime / (TIRE_VOLUME_INREASE_TIME / 3);

                if (_tireSlipAudioSource.volume < 0.01f)
                    _tireSlipAudioSource.Stop();
            }

            _tireSlipAudioSource.volume = Mathf.Clamp01(_tireSlipAudioSource.volume);
        }

        private void HandleWindNoise()
        {
            float volume = Mathf.Clamp01(_currentCarStats.SpeedInMsPerS / 100);
            //more gradual volume increase
            volume *= volume;
            volume *= _maxWindVolume;
            if (volume <= 0)
            {
                if (_windNoiseAudioSource.isPlaying)
                    _windNoiseAudioSource.Stop();
            }
            else
            {
                if (!_windNoiseAudioSource.isPlaying)
                    _windNoiseAudioSource.Play();
                _windNoiseAudioSource.volume = volume;

            }
        }
    }
}
