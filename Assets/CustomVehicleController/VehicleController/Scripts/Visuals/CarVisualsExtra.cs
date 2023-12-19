using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Visuals/Car Visuals Extra")]
    public class CarVisualsExtra : MonoBehaviour
    {
        [SerializeField]
        private CarVisualsEssentials _carVisualsEssentials;

        [SerializeField, Space, Space]
        private CurrentCarStats _currentCarStats;
        [SerializeField]
        private Rigidbody _rigidbody;

        #region Wheel Meshes
        [SerializeField]
        private Transform[] _wheelMeshes;
        [SerializeField]
        public WheelController[] _wheelControllerArray;
        private int _wheelMeshesSize;
        #endregion

        #region Extra Effects
        [Header("Extra Visual Effects")]
        [SerializeField]
        private CarVisualsBodyWindEffect _bodyWindEffect;
        [SerializeField]
        private CarVisualsBrakeLights _brakeLights;
        [SerializeField]
        private CarVisualsSkidMarks _skidMarks;
        [SerializeField]
        private CarVisualsTireSmoke _tireSmoke;
        #endregion

        private const float DELAY_BEFORE_DISABLING_EFFECTS = 0.15f;
        private float[] _lastStopEmitTimeArray;
        private bool[] _shouldEmitArray;

        private void Awake()
        {
            _wheelMeshesSize = _wheelMeshes.Length;
            _lastStopEmitTimeArray = new float[_wheelMeshesSize];
            _shouldEmitArray = new bool[_wheelMeshesSize];

            TryInstantiateExtraEffects();
        }

        private void Update()
        {
            if (_tireSmoke != null || _skidMarks != null)
                ShouldEmitWheelEffects();

            if (_tireSmoke != null)
                DisplaySmokeEffects();
            if (_skidMarks != null)
                DisplaySkidMarksEffects();


            if (_bodyWindEffect != null)
                _bodyWindEffect.HandleSpeedEffect(_currentCarStats.SpeedInMsPerS, _rigidbody.velocity);
            if (_brakeLights != null)
                _brakeLights.HandleRearLights(_currentCarStats.Braking);
        }


        private void TryInstantiateExtraEffects()
        {
            if (_skidMarks != null)
                _skidMarks.InstantiateTireTrailRenderers(_wheelMeshes);
            if (_tireSmoke != null)
                _tireSmoke.InstantiateSmoke(_wheelMeshes);
        }

        private void ShouldEmitWheelEffects()
        {
            for (int i = 0; i < _wheelMeshesSize; i++)
            {
                if (_currentCarStats.WheelSlipArray[i])
                {
                    _shouldEmitArray[i] = true;
                    _lastStopEmitTimeArray[i] = Time.time;
                }
                else
                {
                    _shouldEmitArray[i] = false;
                }
            }
        }

        private void DisplaySmokeEffects()
        {
            for (int i = 0; i < _wheelMeshesSize; i++)
            {
                if (!_wheelControllerArray[i].HasContactWithGround)
                {
                    _tireSmoke.DisplaySmokeVFX(false, i, _wheelControllerArray[i].GetHitPosition(),
                        _rigidbody.velocity.normalized, _currentCarStats.SpeedInMsPerS);
                    continue;
                }

                if (_shouldEmitArray[i])
                {
                    _tireSmoke.DisplaySmokeVFX(true, i, _wheelControllerArray[i].GetHitPosition(),
                        _rigidbody.velocity.normalized, _currentCarStats.SpeedInMsPerS);
                }
                else
                {
                    bool display = Time.time < _lastStopEmitTimeArray[i] + DELAY_BEFORE_DISABLING_EFFECTS;
                    _tireSmoke.DisplaySmokeVFX(display, i, _wheelControllerArray[i].GetHitPosition(),
                        _rigidbody.velocity.normalized, _currentCarStats.SpeedInMsPerS);
                }
            }
        }

        private void DisplaySkidMarksEffects()
        {
            for (int i = 0; i < _wheelMeshesSize; i++)
            {
                if (!_wheelControllerArray[i].HasContactWithGround)
                {
                    _skidMarks.DisplayTireTrail(false, i, _wheelControllerArray[i].GetHitPosition());
                    continue;
                }

                if (_shouldEmitArray[i])
                {
                    _skidMarks.DisplayTireTrail(true, i, _wheelControllerArray[i].GetHitPosition());

                }
                else
                {
                    bool display = Time.time < _lastStopEmitTimeArray[i] + DELAY_BEFORE_DISABLING_EFFECTS;
                    _skidMarks.DisplayTireTrail(display, i, _wheelControllerArray[i].GetHitPosition());
                }
            }
        }

        public void CopyValuesFromEssentials()
        {
            if(_carVisualsEssentials == null)
            {
                Debug.LogError("CarVisualsEssentials is not assigned");
                return;
            }
            _wheelMeshes = _carVisualsEssentials.GetWheelMeshes();
            _wheelControllerArray = _carVisualsEssentials.GetWheelControllerArray();

            _currentCarStats = _carVisualsEssentials.GetCurrentCarStats();
            _rigidbody = _carVisualsEssentials.GetRigidbody();
        }
    }
}

