using UnityEngine;
using UnityEngine.Audio;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Sound/Vehicle Extra Sound Manager")]
    public class CustomVehicleControllerExtraSoundManager : MonoBehaviour
    {
        private const float MAX_VOLUME_COLLISION_VELOCITY = 100;

        [SerializeField]
        private CustomVehicleController _vehicleController;
        private CurrentCarStats _currentCarStats;

        [SerializeField]
        private CarExtraSoundsSO _extraSoundSO;
        [SerializeField]
        private CarForcedInductionSoundSO _forcedInductionSoundSO;

        private AudioSource _forcedInductionAudioSource;
        private AudioSource _tireSlipAudioSource;
        private AudioSource _carEffectsAudioSource;
        private AudioSource _collisionAudioSource;
        private AudioSource _windNoiseAudioSource;

        [SerializeField, Header("   Optional")]
        private AudioMixerGroup _vehicleSoundAudioMixerGroup;

        [SerializeField, Min(0), Space]
        private float _antiLagSoundCooldown = 1f;
        private float lastAntiLag;

        [SerializeField, Min(1)]
        private float _forcedInductionMaxPitch = 1.5f;
        [SerializeField, Min(0)]
        private float _forcedInductionMaxVolume = 0.7f;

        [SerializeField, Min(0)]
        private float _tireVolumeIncreaseTime = 0.75f;

        [SerializeField, Range(0,1f)]
        private float _maxWindVolume = 0.3f;
        [SerializeField, Min(0)]
        private float _speedForMaxWindVolume = 100f;

        [SerializeField]
        private CollisionHandler _collisionHandler;

        private bool _forcedInductionSoundInitialized = false;
        private bool _tireSlipSoundInitialized = false;
        private bool _windNoiseSoundInitialized = false;
        private bool _collisionEffectInitialized = false;

        private bool _flutterSoundExists = false;
        private bool _antiLagSoundExists = false;
        private bool _antiLagMildSoundExists = false;

        private bool _collisionImpactSoundExists = false;
        private bool _collisionContinuousSoundExists = false;

        private GameObject _effectAudioSourceHolder;

        private bool _rightCollisionStay = false;
        private bool _leftCollisionStay = false;

        private void Awake()
        {
            _effectAudioSourceHolder = new GameObject("Car Sound Effects");
            _effectAudioSourceHolder.transform.parent = this.transform;
            _effectAudioSourceHolder.transform.localPosition = Vector3.zero;

            InitializeForcedInductionSound();
            InitializeTireSlipSound();
            InitializeCarEffectSound();
            InitializeCollisionSound();
            InitializedWindNoise();
        }

        private void OnDestroy()
        {
            if (_vehicleController == null)
                return;

            if (_collisionEffectInitialized)
            {
                _collisionHandler.OnCollisionImpact -= _collisionHandler_OnCollisionImpact;
                _collisionHandler.OnRightSideCollisionStay -= _collisionHandler_OnSideCollisionStay;
                _collisionHandler.OnRightSideCollisionExit -= _collisionHandler_OnRightSideCollisionExit;
                _collisionHandler.OnLeftSideCollisionStay -= _collisionHandler_OnSideCollisionStay;
                _collisionHandler.OnLeftSideCollisionExit -= _collisionHandler_OnLeftSideCollisionExit;
            }

            if (!_flutterSoundExists && !_antiLagSoundExists && !_antiLagMildSoundExists)
                return;

            if (_currentCarStats == null)
                return;

            _currentCarStats.OnAntiLag -= _currentCarStats_OnAntiLag;
            _currentCarStats.OnShiftedAntiLag -= _currentCarStats_OnShiftedAntiLag;           
        }

        private void _currentCarStats_OnShiftedAntiLag()
        {
            RandomizeAntiLagPitchAndVolume();

            if (_antiLagMildSoundExists && Time.time > lastAntiLag + _antiLagSoundCooldown)
            {
                _carEffectsAudioSource.PlayOneShot(_forcedInductionSoundSO.AntiLagMildSounds
                    [Random.Range(0, _forcedInductionSoundSO.AntiLagMildSounds.Length)]);
                lastAntiLag = Time.time;
            }

            if(_flutterSoundExists)
                _carEffectsAudioSource.PlayOneShot(_forcedInductionSoundSO.TurboFlutterSound[Random.Range(0, _forcedInductionSoundSO.TurboFlutterSound.Length)]);
        }

        private void _currentCarStats_OnAntiLag()
        {
            RandomizeAntiLagPitchAndVolume();
            if (_antiLagSoundExists && Time.time > lastAntiLag + _antiLagSoundCooldown)
            {
                _carEffectsAudioSource.PlayOneShot(_forcedInductionSoundSO.AntiLagSound[Random.Range(0, _forcedInductionSoundSO.AntiLagSound.Length)]);
                lastAntiLag = Time.time;
            }

            if (_flutterSoundExists)
                _carEffectsAudioSource.PlayOneShot(_forcedInductionSoundSO.TurboFlutterSound[Random.Range(0, _forcedInductionSoundSO.TurboFlutterSound.Length)]);
        }

        private void RandomizeAntiLagPitchAndVolume()
        {
            _carEffectsAudioSource.volume = Random.Range(0.8f, 1.2f);
            _carEffectsAudioSource.pitch = Random.Range(0.8f, 1.2f);
        }

        private void InitializeForcedInductionSound()
        {
            if (_forcedInductionSoundSO.ForcedInductionSound == null)
                return;

            _forcedInductionAudioSource = _effectAudioSourceHolder.AddComponent<AudioSource>();
            _forcedInductionAudioSource.clip = _forcedInductionSoundSO.ForcedInductionSound;
            _forcedInductionAudioSource.volume = 0;
            SetupAudioSource(_forcedInductionAudioSource, true);

            _forcedInductionSoundInitialized = true;
        }

        private void InitializeTireSlipSound()
        {
            if (_extraSoundSO.TireSlipSound == null)
                return;

            _tireSlipAudioSource = _effectAudioSourceHolder.AddComponent<AudioSource>();
            _tireSlipAudioSource.clip = _extraSoundSO.TireSlipSound;
            _tireSlipAudioSource.volume = 0;
            SetupAudioSource(_tireSlipAudioSource, true);

            _tireSlipSoundInitialized = true;
        }

        private void InitializeCarEffectSound()
        {
            _carEffectsAudioSource = _effectAudioSourceHolder.AddComponent<AudioSource>();
            SetupAudioSource(_carEffectsAudioSource, false);

            _flutterSoundExists = _forcedInductionSoundSO.TurboFlutterSound != null;
            _antiLagSoundExists = _forcedInductionSoundSO.AntiLagSound != null;
            _antiLagMildSoundExists = _forcedInductionSoundSO.AntiLagMildSounds.Length > 0;

            if (!_flutterSoundExists && !_antiLagSoundExists && !_antiLagMildSoundExists)
                return;

            _currentCarStats = _vehicleController.GetCurrentCarStats();

            if (_currentCarStats == null)
                return;

            _currentCarStats.OnAntiLag += _currentCarStats_OnAntiLag;
            _currentCarStats.OnShiftedAntiLag += _currentCarStats_OnShiftedAntiLag;
        }

        private void InitializeCollisionSound()
        {
            _collisionImpactSoundExists = _extraSoundSO.CollisionImpact != null;
            _collisionContinuousSoundExists = _extraSoundSO.CollisionContinuous != null;

            if(_collisionHandler == null)
            {
                Debug.LogWarning("You have collision effects sound effect, but no CollisionHandler component assigned");
                return;
            }

            if (!_collisionImpactSoundExists && !_collisionContinuousSoundExists)
                return;

            _collisionAudioSource = _effectAudioSourceHolder.AddComponent<AudioSource>();

            if(_extraSoundSO.CollisionContinuous != null)
                _collisionAudioSource.clip = _extraSoundSO.CollisionContinuous;

            SetupAudioSource (_collisionAudioSource, true);
            _collisionAudioSource.Stop();

            _collisionHandler.OnCollisionImpact += _collisionHandler_OnCollisionImpact;
            _collisionHandler.OnRightSideCollisionStay += _collisionHandler_OnSideCollisionStay; 
            _collisionHandler.OnRightSideCollisionExit += _collisionHandler_OnRightSideCollisionExit;
            _collisionHandler.OnLeftSideCollisionStay += _collisionHandler_OnSideCollisionStay;
            _collisionHandler.OnLeftSideCollisionExit += _collisionHandler_OnLeftSideCollisionExit;

            _collisionEffectInitialized = true;
        }

        private void _collisionHandler_OnLeftSideCollisionExit()
        {
            _leftCollisionStay = false;
            if (_rightCollisionStay)
                return;

            if (!_collisionContinuousSoundExists)
                return;

            if (_collisionAudioSource.isPlaying)
                _collisionAudioSource.Stop();
        }

        private void _collisionHandler_OnRightSideCollisionExit()
        {
            _rightCollisionStay = false;
            if (_leftCollisionStay)
                return;


            if (!_collisionContinuousSoundExists)
                return;

            if (_collisionAudioSource.isPlaying)
                _collisionAudioSource.Stop();
        }

        private void _collisionHandler_OnSideCollisionStay(Vector3 pos, float magnitude)
        {
            if (!_collisionContinuousSoundExists)
                return;

            // dont care which side
            _leftCollisionStay = _rightCollisionStay = true;

            _collisionAudioSource.volume = Mathf.Abs(magnitude / MAX_VOLUME_COLLISION_VELOCITY);
            if (!_collisionAudioSource.isPlaying)
                _collisionAudioSource.Play();
        }

        private void _collisionHandler_OnCollisionImpact(Vector3 arg, float collMagnitude)
        {
            RandomizeAntiLagPitchAndVolume();

            if (_collisionImpactSoundExists)
                _carEffectsAudioSource.PlayOneShot(_extraSoundSO.CollisionImpact, Mathf.Clamp01(collMagnitude / MAX_VOLUME_COLLISION_VELOCITY));
        }

        private void InitializedWindNoise()
        {
            if (_extraSoundSO.WindNoise == null)
                return;

            _windNoiseAudioSource = _effectAudioSourceHolder.AddComponent<AudioSource>();
            _windNoiseAudioSource.clip = _extraSoundSO.WindNoise;
            _windNoiseAudioSource.volume = 0;
            SetupAudioSource(_windNoiseAudioSource, true);

            _windNoiseSoundInitialized = true;
        }

        private void SetupAudioSource(AudioSource source, bool loop)
        {
            source.loop = loop;
            source.Play();

            if (_vehicleSoundAudioMixerGroup != null)
                source.outputAudioMixerGroup = _vehicleSoundAudioMixerGroup;
        }

        private void Update()
        {
            if (_forcedInductionSoundInitialized)
                HandleForcedInductionSound();
            if (_tireSlipSoundInitialized)
                HandleTireSlipSound();
            if (_windNoiseSoundInitialized)
                HandleWindNoise();
        }

        private void HandleForcedInductionSound()
        {
            _forcedInductionAudioSource.volume = _vehicleController.GetCurrentCarStats().ForcedInductionBoostPercent * _forcedInductionMaxVolume;
            _forcedInductionAudioSource.pitch = _vehicleController.GetCurrentCarStats().ForcedInductionBoostPercent * _forcedInductionMaxPitch;
        }

        private void HandleTireSlipSound()
        {
            if (_vehicleController.GetCurrentCarStats().DriveWheelLostContact)
            {
                _tireSlipAudioSource.volume = 0;
                if (_tireSlipAudioSource.isPlaying)
                    _tireSlipAudioSource.Stop();

                return;
            }

            if (_vehicleController.GetCurrentCarStats().IsCarSlipping)
            {
                if (!_tireSlipAudioSource.isPlaying)
                {
                    _tireSlipAudioSource.Play();
                }
                _tireSlipAudioSource.volume += Time.deltaTime / (_tireVolumeIncreaseTime);
            }
            else
            {
                if (!_tireSlipAudioSource.isPlaying)
                    return;

                _tireSlipAudioSource.volume -= Time.deltaTime / (_tireVolumeIncreaseTime / 3);

                if (_tireSlipAudioSource.volume < 0.01f)
                    _tireSlipAudioSource.Stop();
            }

            _tireSlipAudioSource.volume = Mathf.Clamp01(_tireSlipAudioSource.volume);
        }

        private void HandleWindNoise()
        {
            float volume = Mathf.Clamp01(_vehicleController.GetCurrentCarStats().SpeedInMsPerS / _speedForMaxWindVolume);
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
