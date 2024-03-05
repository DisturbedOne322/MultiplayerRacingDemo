using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Sound/Vehicle Engine Sound Manager"),
    HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/extra/adding-sound-effects/adding-engine-sound")]
    public class VehicleEngineSoundManager : NetworkBehaviour
    {
        [SerializeField]
        private CustomVehicleController _vehicleController;

        [SerializeField]
        public CarEngineSoundSO _engineSoundsSO;

        [SerializeField, Space, Header(" Optional fields")]
        private AudioMixerGroup _vehicleSoundAudioMixerGroup;
        public float EngineSoundPitch = 1;


        private AudioSource[] _engineAudioSources;

        [SerializeField]
        private bool _3DSound;

        [SerializeField, Range(0, 1f)]
        private float _spatialBlend = 0;

        [SerializeField, Range(0, 5f)]
        private float _dopplerLevel = 1;

        [SerializeField, Range(0, 360)]
        private int _spread = 180;

        [SerializeField]
        private AudioRolloffMode _volumeRolloff;

        [SerializeField]
        private float _minDistance = 25;
        [SerializeField]
        private float _maxDistance = 60;

        private float _minRPM;

        private GameObject _engineAudioHolder;
        private bool _engineSoundInitialized = false;

        private NetworkVariable<float> _engineRPM = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private float _lastRPM = 0;

        private void Start()
        {
            if (_engineSoundsSO == null)
            {
                Debug.LogWarning("No car sound SO assigned on " + gameObject.name);
                return;
            }
            if (_vehicleController == null)
            {
                Debug.LogWarning("No Vehicle Controller assigned on " + gameObject.name);
                return;
            }

            if (!IsOwner)
            {
                _3DSound = true;
            }

            _vehicleController.VehiclePartsSetWrapper.OnPartsChanged += VehiclePartsPresetWrapper_OnPartsChanged;
            InitializeEngineSound();
        }

        private void OnDestroy()
        {
            _vehicleController.VehiclePartsSetWrapper.OnPartsChanged -= VehiclePartsPresetWrapper_OnPartsChanged;
        }

        private void VehiclePartsPresetWrapper_OnPartsChanged()
        {
            _minRPM = _vehicleController.VehiclePartsSetWrapper.Engine.MinRPM;
        }

        private void Update()
        {
            if (IsOwner)         
                _engineRPM.Value = _vehicleController.GetCurrentCarStats().EngineRPM;
            
            if (_engineSoundInitialized)
                HandleEngineSound();
        }

        public void SetNewCarEngineSoundSO(CarEngineSoundSO engineSoundSO)
        {
            if (engineSoundSO == null)
                return;

            if (engineSoundSO == _engineSoundsSO)
                return;

            int size = engineSoundSO.EngineRPMRangeArray.Length;

            _engineSoundsSO = engineSoundSO;

            if (_engineAudioSources != null)
            {
                if (_engineAudioSources.Length != size)
                {
                    Destroy(_engineAudioHolder);
                    InitializeEngineSound();
                    return;
                }
            }
            else
            {
                InitializeEngineSound();
                return;
            }
            for (int i = 0; i < size; i++)
                _engineAudioSources[i].clip = _engineSoundsSO.EngineRPMRangeArray[i];
        }

        private void InitializeEngineSound()
        {
            if (_engineSoundsSO == null)
                return;

            if (_engineSoundsSO.EngineRPMRangeArray.Length == 0)
                return;

            _engineAudioHolder = new("EngineAudioHolder");
            _engineAudioHolder.transform.parent = transform;
            _engineAudioHolder.transform.localPosition = new(0, 0, 0);
            _engineAudioSources = new AudioSource[_engineSoundsSO.EngineRPMRangeArray.Length];
            int size = _engineAudioSources.Length;
            for (int i = 0; i < size; i++)
            {
                CreateEngineAudioSource(i, _engineAudioHolder);
            }

            _engineSoundInitialized = true;
        }

        private void CreateEngineAudioSource(int i, GameObject parent)
        {
            _engineAudioSources[i] = parent.AddComponent<AudioSource>();
            _engineAudioSources[i].clip = _engineSoundsSO.EngineRPMRangeArray[i];
            _engineAudioSources[i].outputAudioMixerGroup = _vehicleSoundAudioMixerGroup;
            _engineAudioSources[i].volume = 0;
            _engineAudioSources[i].loop = true;
            _engineAudioSources[i].Play();

            if (_3DSound)
            {
                _engineAudioSources[i].spatialBlend = 1;
            }
        }

        private void HandleEngineSound()
        {
            int size = _engineAudioSources.Length;
            float engineRPM = _engineRPM.Value;

            for (int i = 0; i < size; i++)
            {
                float rpmDifference = (i + 1) * _engineSoundsSO.RPMStep - engineRPM + _minRPM - 5;
                if (rpmDifference <= _engineSoundsSO.RPMStep * 2 && rpmDifference >= -_engineSoundsSO.RPMStep * 2)
                {
                    _engineAudioSources[i].volume = _engineSoundsSO.RPMStep / Mathf.Abs(rpmDifference);
                    _engineAudioSources[i].pitch = (engineRPM) /
                        ((i + 2) * _engineSoundsSO.RPMStep) * EngineSoundPitch;

                    UpdateAudioSourceSettings(_engineAudioSources[i]);
                }
                else
                    _engineAudioSources[i].volume = 0;
            }

            float rpmChangeRate = Mathf.Abs(_lastRPM - engineRPM) / Time.deltaTime;
            _lastRPM = engineRPM;

            //if the rpm changes too quickly, enable all audio sources to avoid audio cracking sound
            if (rpmChangeRate / 2 > size * _engineSoundsSO.RPMStep)
            {
                for (int i = 0; i < size; i++)
                {
                    _engineAudioSources[i].enabled = true;
                }
                return;
            }

            //otherwise, enable audio sources based on their volume + enable 1 audio source before and after all the working audio sources
            //to prepare for audio source change.
            for (int i = 0; i < size; i++)
            {
                if (i - 1 >= 0)
                    if (_engineAudioSources[i].volume == 0 && _engineAudioSources[i - 1].volume != 0)
                    {
                        _engineAudioSources[i].enabled = true;
                        continue;
                    }

                if (i + 1 < size)
                    if (_engineAudioSources[i].volume == 0 && _engineAudioSources[i + 1].volume != 0)
                    {
                        _engineAudioSources[i].enabled = true;
                        continue;
                    }

                _engineAudioSources[i].enabled = _engineAudioSources[i].volume != 0;
            }
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
    }
}
