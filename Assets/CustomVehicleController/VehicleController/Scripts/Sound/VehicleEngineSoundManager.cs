using UnityEngine;
using UnityEngine.Audio;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Sound/Vehicle Engine Sound Manager")]
    public class VehicleEngineSoundManager : MonoBehaviour
    {
        [SerializeField]
        private CustomVehicleController _vehicleController;

        [SerializeField]
        public CarEngineSoundSO _engineSoundsSO;


        [SerializeField,Space, Header(" Optional fields")]
        private AudioMixerGroup _vehicleSoundAudioMixerGroup;

        private AudioSource[] _engineAudioSources;
        private float _minRPM;

        [SerializeField]
        private float _engineSoundPitch = 1;

        private bool _engineSoundInitialized = false;

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

            _minRPM = _vehicleController.VehicleStats.EngineSO.MinRPM;

            InitializeEngineSound();
        }

        private void Update()
        {
            if (_engineSoundInitialized)
                HandleEngineSound();
        }


        private void InitializeEngineSound()
        {
            if (_engineSoundsSO.EngineRPMRangeArray.Length == 0)
            {
                Debug.LogWarning("No engine sound clips assigned to scriptable object.");
                return;
            }
            GameObject engineAudioHolder = new ("EngineAudioHolder");
            engineAudioHolder.transform.parent = transform;
            engineAudioHolder.transform.localPosition = new (0, 0, 0);
            _engineAudioSources = new AudioSource[_engineSoundsSO.EngineRPMRangeArray.Length];
            int size = _engineAudioSources.Length;
            for (int i = 0; i < size; i++)
            {
                CreateEngineAudioSource(i, engineAudioHolder);
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
        }

        private void HandleEngineSound()
        {
            int size = _engineAudioSources.Length;
            for (int i = 0; i < size; i++)
            {
                float rpmDifference = (i + 1) * _engineSoundsSO.RPMStep - _vehicleController.GetCurrentCarStats().EngineRPM + _minRPM - 5;
                if (rpmDifference <= _engineSoundsSO.RPMStep * 2 && rpmDifference >= -_engineSoundsSO.RPMStep * 2)
                {
                    _engineAudioSources[i].volume = _engineSoundsSO.RPMStep / Mathf.Abs(rpmDifference);
                    _engineAudioSources[i].pitch = (_vehicleController.GetCurrentCarStats().EngineRPM) / 
                        ((i + 2) * _engineSoundsSO.RPMStep) * _engineSoundPitch;
                }
                else
                    _engineAudioSources[i].volume = 0;
            }
        }
    }
}
