using UnityEngine;

namespace Assets.VehicleController
{
    public class NitrousBoost
    {
        private VehiclePartsSetWrapper _partsPresetWrapper;
        private float _boostAmountLeft;
        private float _lastUseTime;

        private int _bottlesLeft = 1;

        private float _effectStrength = 0;

        private CurrentCarStats _currentCarStats;

        private bool _playerAlreadyBoosting = false;

        private bool _playerStartedOneShotBoost = false;

        public NitrousBoost(VehiclePartsSetWrapper partsPresetWrapper, CurrentCarStats currentCarStats)
        {
            _partsPresetWrapper = partsPresetWrapper;
            _currentCarStats = currentCarStats;

            if (_partsPresetWrapper.Nitrous == null)
                return;

            _boostAmountLeft = _partsPresetWrapper.Nitrous.BoostAmount;
            _bottlesLeft = _partsPresetWrapper.Nitrous.BottlesAmount;
        }

        public float GetNitroBoost(bool enabled)
        {
            if (_partsPresetWrapper.Nitrous == null)
                return 0;

            if (_partsPresetWrapper.Nitrous.BoostType == NitroBoostType.Continuous)
                return HandleContinuousBoost(enabled);
            else
                return HandleOneShotBoost(enabled);
        }

        private float HandleContinuousBoost(bool enabled)
        {
            //not one shot boosting
            _playerStartedOneShotBoost = false;

            if (_currentCarStats.Reversing || !_currentCarStats.Accelerating)
                enabled = false;
            else if (enabled && !_playerAlreadyBoosting)
            {
                if (_currentCarStats.NitroPercentLeft < _partsPresetWrapper.Nitrous.MinAmountPercentToUse)
                    enabled = false;
            }

            enabled = ManageContinuousNitroDepletion(enabled);

            UpdateCurrentStats();
            _currentCarStats.NitroBoosting = enabled;

            _playerAlreadyBoosting = enabled;

            RechargeNitro();

            if (_playerAlreadyBoosting)
            {
                _lastUseTime = Time.time;
                return GetBoostAmount();
            }

            _effectStrength = 0;
            return 0;
        }

        private float HandleOneShotBoost(bool enabled)
        {
            //not cont boosting
            _playerAlreadyBoosting = false;

            if (_currentCarStats.Reversing || !_currentCarStats.Accelerating)
                enabled = false;

            UpdateCurrentStats();
            _currentCarStats.NitroBoosting = _playerStartedOneShotBoost;

            if (_playerStartedOneShotBoost)
            {
                if (_currentCarStats.Reversing)
                {
                    _playerStartedOneShotBoost = false;
                    _lastUseTime = Time.time;
                    return 0;
                }

                //in case a switch is made from a nitro with tons of boost to one with low boost
                if (_boostAmountLeft > _partsPresetWrapper.Nitrous.BoostAmount)
                    _boostAmountLeft = _partsPresetWrapper.Nitrous.BoostAmount;

                if (_boostAmountLeft < 0)
                {
                    _boostAmountLeft = 0;
                    _playerStartedOneShotBoost = false;
                    _lastUseTime = Time.time;

                    _bottlesLeft--;
                    if (_bottlesLeft <= 0)
                        _bottlesLeft = 1;
                    else
                        _boostAmountLeft = _partsPresetWrapper.Nitrous.BoostAmount;

                    return 0;
                }

                return GetBoostAmount();
            }

            _playerStartedOneShotBoost = enabled;
            _effectStrength = 0;

            RechargeNitro();

            if (_currentCarStats.NitroPercentLeft < _partsPresetWrapper.Nitrous.MinAmountPercentToUse)
                _playerStartedOneShotBoost = false;

            return 0;
        }

        private void RechargeNitro()
        {
            if (Time.time < _lastUseTime + _partsPresetWrapper.Nitrous.RechargeDelay)
                return;

            _boostAmountLeft += Time.deltaTime * _partsPresetWrapper.Nitrous.RechargeRate;

            if (_boostAmountLeft >= _partsPresetWrapper.Nitrous.BoostAmount)
            {
                _boostAmountLeft = _partsPresetWrapper.Nitrous.BoostAmount;
                if (_bottlesLeft < _partsPresetWrapper.Nitrous.BottlesAmount)
                {
                    _bottlesLeft++;
                    _boostAmountLeft = 0;
                }
                else
                    _bottlesLeft = _partsPresetWrapper.Nitrous.BottlesAmount;
            }
        }

        public void AddNitro(float amount)
        {
            _boostAmountLeft += amount;

            while (_boostAmountLeft >= _partsPresetWrapper.Nitrous.BoostAmount)
            {

                if (_bottlesLeft < _partsPresetWrapper.Nitrous.BottlesAmount)
                    _bottlesLeft++;
                else
                {
                    _boostAmountLeft = _partsPresetWrapper.Nitrous.BoostAmount;
                    return;
                }

                _boostAmountLeft -= _partsPresetWrapper.Nitrous.BoostAmount;
            }
        }

        private float GetBoostAmount()
        {
            float warmUpTime = _partsPresetWrapper.Nitrous.BoostWarmUpTime;
            if (warmUpTime != 0)
                _effectStrength += Time.deltaTime / warmUpTime;
            else
                _effectStrength = 1;
            _effectStrength = Mathf.Clamp01(_effectStrength);

            _boostAmountLeft -= _partsPresetWrapper.Nitrous.BoostIntensity * _effectStrength * Time.deltaTime;
            return _partsPresetWrapper.Nitrous.BoostIntensity * _effectStrength;
        }

        private bool ManageContinuousNitroDepletion(bool enabled)
        {
            //in case a switch is made from a nitro with tons of boost to one with low boost
            if (_boostAmountLeft > _partsPresetWrapper.Nitrous.BoostAmount)
                _boostAmountLeft = _partsPresetWrapper.Nitrous.BoostAmount;

            if (_boostAmountLeft < 0)
            {
                _bottlesLeft--;
                if (_bottlesLeft <= 0)
                {
                    _bottlesLeft = 1;
                    enabled = false;
                }
                else
                    _boostAmountLeft = _partsPresetWrapper.Nitrous.BoostAmount;
            }

            return enabled;
        }

        private void UpdateCurrentStats()
        {
            _currentCarStats.NitroPercentLeft = _boostAmountLeft / _partsPresetWrapper.Nitrous.BoostAmount;

            if (_bottlesLeft > _partsPresetWrapper.Nitrous.BottlesAmount)
                _bottlesLeft = _partsPresetWrapper.Nitrous.BottlesAmount;

            _currentCarStats.NitroBottlesLeft = _bottlesLeft;
            _currentCarStats.NitroIntensity = _effectStrength;
        }
    }
}
