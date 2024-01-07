using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Assets.VehicleController
{
    public class CarVisualsNitrous
    {
        private CurrentCarStats _currentCarStats;
        private NitrousParameters _parameters;

        private VisualEffect[] _nitroVFXArray;

        private float _spawnAmount = 0;
        private const float SPAWN_AMOUNT_MAX = 30;
        private float _boostingTime = 0;

        public CarVisualsNitrous(NitrousParameters nitrousParameters, CurrentCarStats currentCarStats)
        {
            _parameters = nitrousParameters;
            _currentCarStats = currentCarStats;

            InitializeVFX();
        }

        private void InitializeVFX()
        {
            //if (_antiLagParameters.VisualEffect.VFXAsset == null)
            //{
            //    Debug.LogWarning("You have Anti Lag Effect, but Visual Effect Asset is not assigned");
            //    return;
            //}

            int size = _parameters.ExhaustsPositionArray.Length;

            _nitroVFXArray = new VisualEffect[size];

            for (int i = 0; i < size; i++)
            {
                GameObject holder = new GameObject("Nitro Position");
                holder.transform.parent = _parameters.ExhaustsPositionArray[i];
                holder.transform.localPosition = Vector3.zero;
                _nitroVFXArray[i] = holder.gameObject.AddComponent<VisualEffect>();
                _nitroVFXArray[i].visualEffectAsset = _parameters.VFXAsset;
                _nitroVFXArray[i].Play();
            }
        }

        public void HandleNitroEffect()
        {
            if(_currentCarStats.NitroBoosting && _currentCarStats.Accelerating)
                _boostingTime += Time.deltaTime;
            else
                _boostingTime = 0;


            for (int i = 0; i < _nitroVFXArray.Length; i++)
            {
                if(_boostingTime == 0 || !_currentCarStats.Accelerating)
                    _nitroVFXArray[i].SetFloat("spawnRate", 0);
                else
                {
                    _nitroVFXArray[i].SetGradient("color", _parameters.Gradient);
                    _nitroVFXArray[i].SetAnimationCurve("sizeCurve", _parameters.SizeOverLifeCurve);
                    _nitroVFXArray[i].SetFloat("spawnRate", _boostingTime < 0.5f ? Random.Range(0, SPAWN_AMOUNT_MAX / 4) : SPAWN_AMOUNT_MAX);
                }
            }
        }
    }
}
