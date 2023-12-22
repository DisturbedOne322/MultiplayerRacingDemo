using System.Collections;
using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Sound/Custom Vehicle Sound Manager")]
    public class CustomVehicleSoundManager : MonoBehaviour
    {
        [SerializeField]
        public CarSoundsSO _carSoundsSO;
        [SerializeField]
        private CurrentCarStats _currentCarStats;

        private AudioSource[] _engineAudioSources;
        private AudioSource _forcedInductionAudioSource;
        private AudioSource _tireSlipAudioSource;
        private AudioSource _carEffectsSource;


        public float ForcedInductionMaxPitch = 1.5f;

        private const float TIRE_VOLUME_INREASE_TIME = 0.5f;

        private float _engineSmoothChangeTime = 1f;

        [SerializeField]
        private AudioClip windNoise;

        [SerializeField]
        private float _antiLagSoundCooldown = 1f;
        private float lastAntiLag;

        private bool _engineSoundInitialized = false;
        private bool _forcedInductionSoundInitialized = false;
        private bool _tireSlipSoundInitialized = false;

        private bool _flutterSoundExists = false;
        private bool _antiLagSoundExists = false;
        private bool _antiLagMildSoundExists = false;

        private void Awake()
        {
            if (_carSoundsSO == null)
            {
                Debug.LogWarning("No car sound SO assigned on " + gameObject.name);
                return;
            }
            if(_currentCarStats == null)
            {
                Debug.LogWarning("No current car stats assigned on " + gameObject.name);
                return;
            }

            InitializeEngineSound();
            InitializeForcedInductionSound();
            InitializeTireSlipSound();
            InitializeCarEffectSound();

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

        private void Update()
        {
            HandleCarSounds();
        }

        private void _currentCarStats_OnShiftedAntiLag()
        {
            if (_antiLagMildSoundExists && Time.time > lastAntiLag + _antiLagSoundCooldown)
            {
                _carEffectsSource.PlayOneShot(_carSoundsSO.AntiLagMildSounds[UnityEngine.Random.Range(0, _carSoundsSO.AntiLagMildSounds.Length)], 1 / _carEffectsSource.volume);
                lastAntiLag = Time.time;
            }
        }

        private void _currentCarStats_OnAntiLag()
        {
            if (_flutterSoundExists)
            {
                _carEffectsSource.PlayOneShot(_carSoundsSO.TurboFlutterSound, 1 / _carEffectsSource.volume);
            }
            if (_antiLagSoundExists)
            {
                _carEffectsSource.PlayOneShot(_carSoundsSO.AntiLagSound, 1 / _carEffectsSource.volume);
            }
        }

        private void InitializeForcedInductionSound()
        {
            if (_carSoundsSO.ForcedInductionSound == null)
            {
                Debug.LogWarning("No forced induction sound is assigned to scriptable object.");
                return;
            }
            GameObject forcedInductionAudioHolder = new ("ForcedInductionAudioHolder");
            forcedInductionAudioHolder.transform.parent = transform;
            forcedInductionAudioHolder.transform.localPosition = new (0, 0, 0);
            _forcedInductionAudioSource = forcedInductionAudioHolder.AddComponent<AudioSource>();
            _forcedInductionAudioSource.clip = _carSoundsSO.ForcedInductionSound;
            _forcedInductionAudioSource.volume = 0;
            SetupLoopingAudioSource(_forcedInductionAudioSource);

            _forcedInductionSoundInitialized = true;
        }

        private void InitializeTireSlipSound()
        {
            if (_carSoundsSO.TireSlipSound == null)
            {
                Debug.LogWarning("No tire slip sound is assigned to scriptable object.");
                return;
            }
            GameObject tireSlipAudioHolder = new ("TireSlipAudioHolder");
            tireSlipAudioHolder.transform.parent = transform;
            tireSlipAudioHolder.transform.localPosition = new (0, 0, 0);
            _tireSlipAudioSource = tireSlipAudioHolder.AddComponent<AudioSource>();
            _tireSlipAudioSource.clip = _carSoundsSO.TireSlipSound;
            SetupAudioSource(_tireSlipAudioSource);

            _tireSlipSoundInitialized = true;
        }

        private void InitializeCarEffectSound()
        {
            GameObject effectsAudioHolder = new ("CarEffectsAudioHolder");
            effectsAudioHolder.transform.parent = transform;
            effectsAudioHolder.transform.localPosition = new (0, 0, 0);
            _carEffectsSource = effectsAudioHolder.AddComponent<AudioSource>();
            //_carEffectsSource.clip = windNoise;
            SetupLoopingAudioSource(_carEffectsSource);
            _flutterSoundExists = _carSoundsSO.TurboFlutterSound != null;
            _antiLagSoundExists = _carSoundsSO.AntiLagSound != null;
            _antiLagMildSoundExists = _carSoundsSO.AntiLagMildSounds.Length > 0;
        }

        private void InitializeEngineSound()
        {
            if (_carSoundsSO.EngineRPMRangeArray.Length == 0)
            {
                Debug.LogWarning("No engine sound clips assigned to scriptable object.");
                return;
            }
            GameObject engineAudioHolder = new ("EngineAudioHolder");
            engineAudioHolder.transform.parent = transform;
            engineAudioHolder.transform.localPosition = new (0, 0, 0);
            _engineAudioSources = new AudioSource[_carSoundsSO.EngineRPMRangeArray.Length];
            int size = _engineAudioSources.Length;
            for (int i = 0; i < size; i++)
            {
                _engineAudioSources[i] = engineAudioHolder.AddComponent<AudioSource>();
                _engineAudioSources[i].clip = _carSoundsSO.EngineRPMRangeArray[i];
                SetupEngineSources(_engineAudioSources[i]);
            }

            _engineSoundInitialized = true;
        }

        private void SetupAudioSource(AudioSource source)
        {
            //source.rolloffMode = AudioRolloffMode.Logarithmic;
            //source.maxDistance = _maxSoundDistance;
            //source.spatialBlend = 0.7f;
        }

        private void SetupLoopingAudioSource(AudioSource source)
        {
            source.loop = true;
            source.Play();
            SetupAudioSource(source);
        }

        private void SetupEngineSources(AudioSource source)
        {
            source.loop = true;
            SetupAudioSource(source);
        }

        private void HandleCarSounds()
        {
            if (_engineSoundInitialized)
                HandleEngineSound();
            if (_forcedInductionSoundInitialized)
                HandleForcedInductionSound();
            if (_tireSlipSoundInitialized)
                HandleTireSlipSound();

            HandleWindNoise();
        }

        private void HandleEngineSound()
        {
            int size = _engineAudioSources.Length;
            for (int i = 0; i < size; i++)
            {
                float rpmDifference = (i + 3) * 500 - _currentCarStats.EngineRPM;
                if (rpmDifference <= 1000 && rpmDifference >= -500)
                {
                    if (!_engineAudioSources[i].isPlaying)
                        _engineAudioSources[i].Play();
                    _engineAudioSources[i].volume = Mathf.Clamp01(500 / rpmDifference);
                    _engineAudioSources[i].pitch = _currentCarStats.EngineRPM / ((i + 2) * 500);
                }
                else
                {
                    _engineAudioSources[i].volume -= _engineSmoothChangeTime * Time.deltaTime;
                    if (_engineAudioSources[i].isPlaying && _engineAudioSources[i].volume < 0.01f)
                        _engineAudioSources[i].Stop();
                }
            }
        }

        private void HandleForcedInductionSound()
        {
            _forcedInductionAudioSource.volume = _currentCarStats.ForcedInductionBoostPercent;
            _forcedInductionAudioSource.pitch = _currentCarStats.ForcedInductionBoostPercent * ForcedInductionMaxPitch;
        }

        private void HandleTireSlipSound()
        {
            if (_currentCarStats.DriveWheelLostContact)
            {
                if (_tireSlipAudioSource.isPlaying)
                    _tireSlipAudioSource.Stop();

                return;
            }

            if (_currentCarStats.IsCarSlipping)
            {
                if (!_tireSlipAudioSource.isPlaying)
                {
                    _tireSlipAudioSource.Play();
                    StartCoroutine(SmoothlyAddVolumeOnSlippingStart());
                }
            }
            else
            {
                if (!_tireSlipAudioSource.isPlaying)
                    return;

                _tireSlipAudioSource.volume -= Time.deltaTime / (TIRE_VOLUME_INREASE_TIME / 3);

                if (_tireSlipAudioSource.volume < 0.1f)
                    _tireSlipAudioSource.Stop();
            }
        }

        private void HandleWindNoise()
        {
            //_carEffectsSource.volume = (Mathf.Clamp01(_currentCarStats.SpeedInMsPerS / 100f));
        }

        private IEnumerator SmoothlyAddVolumeOnSlippingStart()
        {
            _tireSlipAudioSource.volume = 0;
            while (_tireSlipAudioSource.volume < 1)
            {
                _tireSlipAudioSource.volume += Time.deltaTime / TIRE_VOLUME_INREASE_TIME;
                yield return null;
            }
        }
    }

}
