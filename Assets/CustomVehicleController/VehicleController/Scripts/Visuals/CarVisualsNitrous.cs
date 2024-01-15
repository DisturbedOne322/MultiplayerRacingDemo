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

        private ParticleSystem[] _nitroPSArray;

        private float _maxBoostTime = 0.5f;

        private const float SPAWN_AMOUNT_MAX_VFX = 30;
        private const float SPAWN_AMOUNT_MAX_PS = 300;
        private float _boostingTime = 0;

        public CarVisualsNitrous(NitrousParameters nitrousParameters, CurrentCarStats currentCarStats)
        {
            _parameters = nitrousParameters;
            _currentCarStats = currentCarStats;

            if(_parameters.VisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                InitializeVFX();
            else
                InitializePS();
        }

        private void InitializePS()
        {
            if (_parameters.VisualEffect.ParticleSystem == null)
            {
                Debug.LogWarning("You have Nitrous Visual Effect, but Particle System is not assigned");
                return;
            }

            int size = _parameters.ExhaustsPositionArray.Length;

            _nitroPSArray = new ParticleSystem[size];
            for (int i = 0; i < size; i++)
            {
                _nitroPSArray[i] = GameObject.Instantiate(_parameters.VisualEffect.ParticleSystem);
                _nitroPSArray[i].Play();
                _nitroPSArray[i].transform.parent = _parameters.ExhaustsPositionArray[i].transform;
                _nitroPSArray[i].transform.localPosition = new(0, 0, 0);
                _nitroPSArray[i].transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        private void InitializeVFX()
        {
            if (_parameters.VisualEffect.VFXAsset == null)
            {
                Debug.LogWarning("You have Nitrous Visual Effect, but Visual Effect Asset is not assigned");
                return;
            }

            int size = _parameters.ExhaustsPositionArray.Length;

            _nitroVFXArray = new VisualEffect[size];

            for (int i = 0; i < size; i++)
            {
                GameObject holder = new GameObject("Nitro Position");
                holder.transform.parent = _parameters.ExhaustsPositionArray[i];
                holder.transform.localPosition = Vector3.zero;
                _nitroVFXArray[i] = holder.gameObject.AddComponent<VisualEffect>();
                _nitroVFXArray[i].visualEffectAsset = _parameters.VisualEffect.VFXAsset;
                _nitroVFXArray[i].Play();
            }
        }

        public void HandleNitroEffect()
        {
            if (_parameters.VisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                HandleNitroVFX();
            else
                HandleNitroPS();
        }

        private void HandleNitroVFX()
        {
            if (_currentCarStats.NitroBoosting && _currentCarStats.Accelerating)
                _boostingTime += Time.deltaTime;
            else
                _boostingTime = 0;

            for (int i = 0; i < _nitroVFXArray.Length; i++)
            {
                if (_boostingTime == 0 || !_currentCarStats.Accelerating)
                    _nitroVFXArray[i].SetFloat("spawnRate", 0);
                else
                {
                    _nitroVFXArray[i].SetGradient("color", _parameters.Gradient);
                    _nitroVFXArray[i].SetAnimationCurve("sizeCurve", _parameters.SizeOverLifeCurve);
                    _nitroVFXArray[i].SetFloat("spawnRate", _boostingTime < _maxBoostTime ? Random.Range(0, SPAWN_AMOUNT_MAX_VFX / 4) : SPAWN_AMOUNT_MAX_VFX);
                    _nitroVFXArray[i].SetFloat("sideVelocity", _currentCarStats.SidewaysForce / -10);
                }
            }
        }

        private void HandleNitroPS()
        {
            if (_currentCarStats.NitroBoosting && _currentCarStats.Accelerating)
                _boostingTime += Time.deltaTime;
            else
            {
                _boostingTime = 0;
            }

            for (int i = 0; i < _nitroPSArray.Length; i++)
            {
                if (_boostingTime == 0 || !_currentCarStats.Accelerating)
                    _nitroPSArray[i].emissionRate = 0;
                else
                {
                    if (_nitroPSArray[i].isStopped)
                        _nitroPSArray[i].Play();
                    
                    ParticleSystem.SizeOverLifetimeModule sizeOverLife = _nitroPSArray[i].sizeOverLifetime;
                    sizeOverLife.size = new ParticleSystem.MinMaxCurve(1, _parameters.SizeOverLifeCurve);

                    ParticleSystem.ForceOverLifetimeModule forceOverLife = _nitroPSArray[i].forceOverLifetime;
                    forceOverLife.x = -_currentCarStats.SidewaysForce * 3;

                    forceOverLife.z = _boostingTime < _maxBoostTime ? _boostingTime * _boostingTime * -160 : -80;

                    _nitroPSArray[i].emissionRate = _boostingTime < _maxBoostTime ? Random.Range(0, SPAWN_AMOUNT_MAX_PS / 4) : SPAWN_AMOUNT_MAX_PS;
                }
            }
        }
    }
}
