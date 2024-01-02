using UnityEngine;

namespace Assets.VehicleController
{
    public class ForcedInduction
    {
        private VehicleStats _stats;
        private CurrentCarStats _currentCarStats;

        private float _boostPercent;

        private const float DROP_BOOST_TIME = 0.15f;
        private float _lastShiftTime;

        public ForcedInduction(VehicleStats stats, CurrentCarStats currentCarStats, ITransmission transmission)
        {
            _stats = stats;
            _currentCarStats = currentCarStats;
            transmission.OnShifted += _transmission_OnShifted;
        }

        //simulate the driver leaving the foot off the throttle when shifting, thus decreasing the forced induction boost
        private void _transmission_OnShifted()
        {
            if (_stats.EngineSO.ForcedInductionSO == null)
                return;

            if (_stats.EngineSO.ForcedInductionSO.ForcedInductionType != PartTypes.ForcedInductionType.Turbocharger)
                return;
            
            if (_boostPercent == 1)
                _currentCarStats.ShiftedAntiLagHappened();

            _lastShiftTime = Time.time;
        }

        public float GetForcedInductionBoost(float gasInput)
        {
            if (_stats.EngineSO.ForcedInductionSO == null)
                return 0;

            switch (_stats.EngineSO.ForcedInductionSO.ForcedInductionType)
            {
                case PartTypes.ForcedInductionType.Centrifugal:
                    return GetCentrifugalBoost();
                case PartTypes.ForcedInductionType.Supercharger:
                    return GetSuperchargerBoost();
                case PartTypes.ForcedInductionType.Turbocharger:
                    return GetTurbochargerBoost(gasInput);
                default:
                    return 0;
            }
        }

        public float GetForcedInductionBoostPercent() => _boostPercent;

        //even though supercharger provides boost at all times, set the forced induction boost percent to the engine RPM percent 
        //so that the supercharger sound pitch depends on the engine rpm
        private float GetSuperchargerBoost()
        {
            _boostPercent = _currentCarStats.EngineRPMPercent;
            return _stats.EngineSO.ForcedInductionSO.MaxTorqueBoostAmount;
        }

        //centrifugal supercharger provides boost corresponding to the engine RPM
        private float GetCentrifugalBoost()
        {
            float percent = _currentCarStats.EngineRPMPercent;
            _boostPercent = percent;
            return percent * _stats.EngineSO.ForcedInductionSO.MaxTorqueBoostAmount;
        }

        //turbocharger provides boost based on gas input
        private float GetTurbochargerBoost(float gasInput)
        {
            if(Time.time <= _lastShiftTime + DROP_BOOST_TIME)
            {
                _boostPercent -= Time.deltaTime / DROP_BOOST_TIME;

                if (_boostPercent < 0)
                    _boostPercent = 0;

                return _boostPercent * _stats.EngineSO.ForcedInductionSO.MaxTorqueBoostAmount;
            }

            if (gasInput > 0)
            {
                if (_currentCarStats.EngineRPMPercent > _stats.EngineSO.ForcedInductionSO.TurboRPMPercentDelay)
                {
                    _boostPercent += gasInput * Time.deltaTime / _stats.EngineSO.ForcedInductionSO.TurboSpinTime;
                    _boostPercent = Mathf.Clamp(_boostPercent, 0, gasInput);
                }
                else
                {
                    _boostPercent -= Time.deltaTime / DROP_BOOST_TIME * _boostPercent;
                }
            }
            else
            {
                if (_boostPercent >= 1)
                    _currentCarStats.AntiLagHappened();
                _boostPercent -= Time.deltaTime / DROP_BOOST_TIME * _boostPercent;
            }

            _boostPercent = Mathf.Clamp01(_boostPercent);

            return _boostPercent * _stats.EngineSO.ForcedInductionSO.MaxTorqueBoostAmount;
        }
    }
}

