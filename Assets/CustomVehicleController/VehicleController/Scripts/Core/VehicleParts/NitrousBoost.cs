using UnityEngine;

namespace Assets.VehicleController
{
    public class NitrousBoost
    {
        private VehicleStats _stats;
        private float _boostAmountLeft;
        private float _lastUseTime;

        private float _effectStrength = 0;
        private const float EFFECT_STRENGTH_GAIN_SPEED = 0.25f; 

        private CurrentCarStats _currentCarStats;

        private bool _playerAlreadyBoosting = false;

        private bool _playerStartedOneShotBoost = false;

        public NitrousBoost(VehicleStats stats, CurrentCarStats currentCarStats)
        {
            _stats = stats;
            _currentCarStats = currentCarStats;

            if (_stats.NitrousSO == null)
                return;

            _boostAmountLeft = _stats.NitrousSO.BoostAmount;
        }

        public float GetNitroBoost(bool enabled)
        {
            if (_stats.NitrousSO == null)
                return 0;

            if (_stats.NitrousSO.BoostType == NitroBoostType.Continuous)
                return HandleContinuousBoost(enabled);
            else
                return HandleOneShotBoost(enabled);
        }

        private float HandleContinuousBoost(bool enabled)
        {
            if(enabled && !_playerAlreadyBoosting)
            {
                if (_currentCarStats.NitroPercentLeft < _stats.NitrousSO.MinAmountPercentToUse)
                    enabled = false;
            }

            _playerAlreadyBoosting = enabled;

            _currentCarStats.NitroBoosting = _playerAlreadyBoosting;
            _currentCarStats.NitroPercentLeft = _boostAmountLeft / _stats.NitrousSO.BoostAmount;

            if (Time.time >= _lastUseTime + _stats.NitrousSO.RechargeDelay)
                _boostAmountLeft += Time.deltaTime * _stats.NitrousSO.RechargeRate;

            _boostAmountLeft = Mathf.Clamp(_boostAmountLeft, 0, _stats.NitrousSO.BoostAmount);


            if (_playerAlreadyBoosting && _boostAmountLeft > 0)
            {
                _lastUseTime = Time.time;

                _effectStrength += Time.deltaTime / EFFECT_STRENGTH_GAIN_SPEED;
                _effectStrength = Mathf.Clamp01(_effectStrength);

                _boostAmountLeft -= _stats.NitrousSO.BoostIntensity * Time.deltaTime;
                return _stats.NitrousSO.BoostIntensity * _effectStrength;
            }
            else
                _playerAlreadyBoosting = false;

            _effectStrength = 0;
            return 0;
        }

        private float HandleOneShotBoost(bool enabled)
        {
            _currentCarStats.NitroBoosting = _playerStartedOneShotBoost;
            _currentCarStats.NitroPercentLeft = _boostAmountLeft / _stats.NitrousSO.BoostAmount;

            if (_playerStartedOneShotBoost)
            {
                if(_boostAmountLeft < 0)
                {
                    _boostAmountLeft = 0;
                    _playerStartedOneShotBoost = false;
                    _lastUseTime = Time.time;
                    return 0;
                }

                _effectStrength += Time.deltaTime / EFFECT_STRENGTH_GAIN_SPEED;
                _effectStrength = Mathf.Clamp01(_effectStrength);

                _boostAmountLeft -= _stats.NitrousSO.BoostIntensity * Time.deltaTime;
                return _stats.NitrousSO.BoostIntensity * _effectStrength;
            }

            if (Time.time < _lastUseTime + _stats.NitrousSO.RechargeDelay)
                return 0;
        
            _playerStartedOneShotBoost = enabled;
            _effectStrength = 0;

            _boostAmountLeft += Time.deltaTime * _stats.NitrousSO.RechargeRate;
            _boostAmountLeft = Mathf.Clamp(_boostAmountLeft, 0, _stats.NitrousSO.BoostAmount);

            if (_currentCarStats.NitroPercentLeft < _stats.NitrousSO.MinAmountPercentToUse)
                _playerStartedOneShotBoost = false;

            return 0;
        }
    }
}
