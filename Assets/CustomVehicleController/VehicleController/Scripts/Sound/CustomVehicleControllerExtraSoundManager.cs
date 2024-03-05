using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Sound/Vehicle Extra Sound Manager"),
    HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/extra/adding-sound-effects/adding-extra-sound-effects")]
    public class CustomVehicleControllerExtraSoundManager : NetworkBehaviour
    {
        public Separator Separator;

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
        private AudioSource _windNoiseAudioSource;
        private AudioSource _nitroStartAudioSource;
        private AudioSource _nitroContinuousAudioSource;

        [SerializeField, Header("   Optional")]
        private AudioMixerGroup _vehicleSoundAudioMixerGroup;

        [SerializeField, Min(0), Space]
        private float _antiLagSoundCooldown = 1f;
        private float lastAntiLag;

        [SerializeField, Min(1)]
        private float _forcedInductionMaxPitch = 1.5f;
        [SerializeField, Range(0, 1)]
        private float _forcedInductionMaxVolume = 0.7f;

        [SerializeField, Range(0, 1)]
        private float _turboFlutterVolumeMultiplier = 0.55f;
        [SerializeField, Range(0, 1)]
        private float _antiLagVolumeMultiplier = 0.55f;

        [SerializeField, Min(0), Space]
        private float _tireVolumeIncreaseTime = 0.75f;
        [SerializeField, Range(0, 1f)]
        private float _maxTireSlipVolume = 0.5f;


        [SerializeField, Range(0, 1f), Space]
        private float _maxWindVolume = 0.3f;
        [SerializeField, Min(0)]
        private float _speedForMaxWindVolume = 100f;

        [SerializeField, Min(0), Space]
        private float _nitroVolumeGainSpeedInSeconds = 0.4f;
        [SerializeField, Range(0, 1)]
        private float _nitroMaxVolume = 0.5f;
        private float _boostingTime = 0;
        [SerializeField, Tooltip("Optional")]
        private AudioReverbZone _reverbZone;
        [SerializeField]
        private AudioReverbPreset _reverbDuringNitroPreset = AudioReverbPreset.Off;

        [SerializeField]
        private bool _3DSound;

        [SerializeField, Range(0, 1f)]
        private float _spatialBlend = 0;

        [SerializeField, Range(0, 5f)]
        private float _dopplerLevel = 1;

        [SerializeField, Range(0, 360)]
        private int _spread = 0;

        [SerializeField]
        private AudioRolloffMode _volumeRolloff;

        [SerializeField]
        private float _minDistance = 1;
        [SerializeField]
        private float _maxDistance = 500;

        private bool _forcedInductionSoundInitialized = false;
        private bool _tireSlipSoundInitialized = false;
        private bool _windNoiseSoundInitialized = false;
        private bool _nitroEffectsInitialized = false;

        private bool _nitroStartSoundExists = false;

        private GameObject _effectAudioSourceHolder;

        private void Start()
        {
            if (!IsOwner)
            {
                _3DSound = true;
                _spatialBlend = 1;
            }

            _effectAudioSourceHolder = new GameObject("Car Sound Effects");
            _effectAudioSourceHolder.transform.parent = this.transform;
            _effectAudioSourceHolder.transform.localPosition = Vector3.zero;

            InitializeForcedInductionSound();
            InitializeTireSlipSound();
            InitializeCarEffectSound();
            InitializedWindNoise();
            InitializeNitroSound();
        }

        private void OnDestroy()
        {
            if (_vehicleController == null)
                return;

            if (_currentCarStats == null)
                return;
            _currentCarStats.OnAntiLag -= _currentCarStats_OnAntiLag;
            _currentCarStats.OnShiftedAntiLag -= _currentCarStats_OnShiftedAntiLag;
        }


        private void InitializeForcedInductionSound()
        {
            if (_forcedInductionSoundSO == null)
                return;

            if (_forcedInductionSoundSO.ForcedInductionSound == null)
                return;

            _forcedInductionAudioSource = _effectAudioSourceHolder.AddComponent<AudioSource>();
            _forcedInductionAudioSource.clip = _forcedInductionSoundSO.ForcedInductionSound;
            _forcedInductionAudioSource.volume = 0;
            SetupAudioSource(_forcedInductionAudioSource, true, true);

            _forcedInductionSoundInitialized = true;
        }

        private void InitializeTireSlipSound()
        {
            if (_extraSoundSO.TireSlipSound == null)
                return;

            _tireSlipAudioSource = _effectAudioSourceHolder.AddComponent<AudioSource>();
            _tireSlipAudioSource.clip = _extraSoundSO.TireSlipSound;
            _tireSlipAudioSource.volume = 0;
            SetupAudioSource(_tireSlipAudioSource, true, false);

            _tireSlipSoundInitialized = true;
        }

        private void InitializeCarEffectSound()
        {
            _carEffectsAudioSource = _effectAudioSourceHolder.AddComponent<AudioSource>();
            SetupAudioSource(_carEffectsAudioSource, false, false);
            _currentCarStats = _vehicleController.GetCurrentCarStats();

            if (!IsOwner)
                return;

            if (_currentCarStats == null)
                return;

            _currentCarStats.OnAntiLag += _currentCarStats_OnAntiLag;
            _currentCarStats.OnShiftedAntiLag += _currentCarStats_OnShiftedAntiLag;
        }

        private void InitializedWindNoise()
        {
            if (_extraSoundSO.WindNoise == null)
                return;

            _windNoiseAudioSource = _effectAudioSourceHolder.AddComponent<AudioSource>();
            _windNoiseAudioSource.clip = _extraSoundSO.WindNoise;
            _windNoiseAudioSource.volume = 0;
            SetupAudioSource(_windNoiseAudioSource, true, false);

            _windNoiseSoundInitialized = true;
        }

        private void InitializeNitroSound()
        {
            if (_extraSoundSO.NitroContinuous == null)
                return;
            _nitroStartSoundExists = _extraSoundSO.NitroStart != null;

            if (_nitroStartSoundExists)
            {
                _nitroStartAudioSource = _effectAudioSourceHolder.AddComponent<AudioSource>();
                SetupAudioSource(_nitroStartAudioSource, false, false);
                _nitroStartAudioSource.clip = _extraSoundSO.NitroStart;
            }

            _nitroContinuousAudioSource = _effectAudioSourceHolder.AddComponent<AudioSource>();
            SetupAudioSource(_nitroContinuousAudioSource, true, false);
            _nitroContinuousAudioSource.clip = _extraSoundSO.NitroContinuous;

            _nitroEffectsInitialized = true;
        }

        private void SetupAudioSource(AudioSource source, bool loop, bool playOnAwake)
        {
            source.loop = loop;
            source.playOnAwake = playOnAwake;

            if (_vehicleSoundAudioMixerGroup != null)
                source.outputAudioMixerGroup = _vehicleSoundAudioMixerGroup;
        }

        private void _currentCarStats_OnShiftedAntiLag()
        {
            RandomizeAntiLagPitchAndVolume();

            if (_forcedInductionSoundSO.AntiLagMildSounds.Length > 0 && Time.time > lastAntiLag + _antiLagSoundCooldown)
            {
                PlayEffectOnServer(EffectType.AntiLagMild);
                lastAntiLag = Time.time;
            }

            if (_forcedInductionSoundSO.TurboFlutterMildSound.Length > 0)
                PlayEffectOnServer(EffectType.Flutter);
        }

        private void _currentCarStats_OnAntiLag()
        {
            RandomizeAntiLagPitchAndVolume();
            if (_forcedInductionSoundSO.AntiLagSound.Length > 0 && Time.time > lastAntiLag + _antiLagSoundCooldown)
            {
                PlayEffectOnServer(EffectType.AntiLag);
                lastAntiLag = Time.time;
            }

            if (_forcedInductionSoundSO.TurboFlutterSound.Length > 0)
                PlayEffectOnServer(EffectType.Flutter);
        }


        private void RandomizeAntiLagPitchAndVolume()
        {
            _carEffectsAudioSource.volume = Random.Range(0.8f, 1.2f);
            _carEffectsAudioSource.pitch = Random.Range(0.8f, 1.2f);
        }

        private enum EffectType
        {
            AntiLag,
            AntiLagMild,
            Flutter
        }

        private void PlayEffectOnServer(EffectType type)
        {
            PlayEffect(type);
            PlayEffectServerRpc(type);
        }

        private void PlayEffect(EffectType type)
        {
            switch (type)
            {
                case EffectType.AntiLag:
                    _carEffectsAudioSource.PlayOneShot(
                        _forcedInductionSoundSO.AntiLagSound[
                            Random.Range(0, _forcedInductionSoundSO.AntiLagSound.Length)], 
                        _antiLagVolumeMultiplier);
                    break;
                case EffectType.AntiLagMild:
                    _carEffectsAudioSource.PlayOneShot(_forcedInductionSoundSO.AntiLagMildSounds
                    [Random.Range(0, _forcedInductionSoundSO.AntiLagMildSounds.Length)], _antiLagVolumeMultiplier);
                    break;
                case EffectType.Flutter:
                    _carEffectsAudioSource.PlayOneShot(
                        _forcedInductionSoundSO.TurboFlutterSound[
                            Random.Range(0, _forcedInductionSoundSO.TurboFlutterSound.Length)], 
                        _turboFlutterVolumeMultiplier);
                    break;
            }
        }

        [ServerRpc]
        private void PlayEffectServerRpc(EffectType type)
        {
            PlayEffectClientRpc(type);
        }

        [ClientRpc]
        private void PlayEffectClientRpc(EffectType type)
        {
            PlayEffect(type);
        }

        private void Update()
        {
            if (_forcedInductionSoundInitialized)
                HandleForcedInductionSound();
            if (_tireSlipSoundInitialized)
                HandleTireSlipSound();
            if (_nitroEffectsInitialized)
                HandleNitroSound();

            UpdateAudioSourceSettings(_carEffectsAudioSource);

            if (_windNoiseSoundInitialized)
                HandleWindNoise();
        }

        private void UpdateAudioSourceSettings(AudioSource audioSource)
        {
            if (!_3DSound)
            {
                audioSource.spatialBlend = 0;
                return;
            }

            audioSource.spatialBlend = _spatialBlend;
            audioSource.dopplerLevel = _dopplerLevel;
            audioSource.spread = _spread;
            audioSource.rolloffMode = _volumeRolloff;
            audioSource.minDistance = _minDistance;
            audioSource.maxDistance = _maxDistance;
        }

        private void HandleForcedInductionSound()
        {
            if (_vehicleController.GetCurrentCarStats().ForcedInductionBoostPercent == 0 && _forcedInductionAudioSource.isPlaying)
            {
                _forcedInductionAudioSource.Stop();
                return;
            }

            if (!_forcedInductionAudioSource.isPlaying)
                _forcedInductionAudioSource.Play();

            UpdateAudioSourceSettings(_forcedInductionAudioSource);

            _forcedInductionAudioSource.volume = _vehicleController.GetCurrentCarStats().ForcedInductionBoostPercent * 2 * _forcedInductionMaxVolume;
            _forcedInductionAudioSource.pitch = _vehicleController.GetCurrentCarStats().ForcedInductionBoostPercent * _forcedInductionMaxPitch;
        }

        private void HandleTireSlipSound()
        {
            if (!_vehicleController.GetCurrentCarStats().DriveWheelsGrounded)
            {
                _tireSlipAudioSource.volume = 0;
                if (_tireSlipAudioSource.isPlaying)
                    _tireSlipAudioSource.Stop();

                return;
            }

            UpdateAudioSourceSettings(_tireSlipAudioSource);

            if (_vehicleController.GetCurrentCarStats().IsCarSlipping)
            {
                if (!_tireSlipAudioSource.isPlaying)
                {
                    _tireSlipAudioSource.Play();
                }
                _tireSlipAudioSource.volume += Time.deltaTime * _maxTireSlipVolume / (_tireVolumeIncreaseTime);
            }
            else
            {
                if (!_tireSlipAudioSource.isPlaying)
                    return;

                _tireSlipAudioSource.volume -= Time.deltaTime / (_tireVolumeIncreaseTime / 3);

                if (_tireSlipAudioSource.volume < 0.01f)
                    _tireSlipAudioSource.Stop();
            }

            _tireSlipAudioSource.volume = Mathf.Clamp(_tireSlipAudioSource.volume, 0, _maxTireSlipVolume);
        }

        private void HandleWindNoise()
        {
            UpdateAudioSourceSettings(_windNoiseAudioSource);

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

        private void HandleNitroSound()
        {
            UpdateAudioSourceSettings(_nitroContinuousAudioSource);
            UpdateAudioSourceSettings(_nitroStartAudioSource);

            if (_currentCarStats.NitroBoosting)
            {
                if (_boostingTime == 0)
                {
                    _nitroContinuousAudioSource.volume = 0;
                    _nitroContinuousAudioSource.Play();

                    if (_nitroStartSoundExists)
                    {
                        _nitroStartAudioSource.volume = _nitroMaxVolume;
                        _nitroStartAudioSource.Play();
                    }
                }

                _boostingTime += Time.deltaTime;

                if (_reverbZone != null && _boostingTime > _nitroVolumeGainSpeedInSeconds)
                    _reverbZone.reverbPreset = _reverbDuringNitroPreset;

                if (_nitroVolumeGainSpeedInSeconds == 0)
                    _nitroContinuousAudioSource.volume = _nitroMaxVolume;
                else if (_nitroContinuousAudioSource.volume < _nitroMaxVolume)
                    _nitroContinuousAudioSource.volume += Time.deltaTime * _nitroMaxVolume / _nitroVolumeGainSpeedInSeconds;
            }
            else
            {
                if (_reverbZone != null)
                    _reverbZone.reverbPreset = AudioReverbPreset.Off;

                _boostingTime = 0;

                _nitroContinuousAudioSource.volume -= Time.deltaTime * 4;

                if (_nitroStartSoundExists)
                    _nitroStartAudioSource.volume -= Time.deltaTime * 4;

                if (_nitroStartSoundExists && _nitroStartAudioSource.volume == 0)
                    if (_nitroStartAudioSource.isPlaying)
                        _nitroStartAudioSource.Stop();

                if (_nitroContinuousAudioSource.volume == 0)
                    if (_nitroContinuousAudioSource.isPlaying)
                        _nitroContinuousAudioSource.Stop();
            }
        }
    }
}
