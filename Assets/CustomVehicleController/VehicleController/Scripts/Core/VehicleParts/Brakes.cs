using UnityEngine;

namespace Assets.VehicleController
{
    public class Brakes : IBrakes
    {
        private VehicleStats _stats;
        private WheelController[] _rearWheelControllers;
        private int _rearWheelControllersCount;
        private TireController[] _tireControllers;
        private TireController[] _rearTireControllers;
        private CurrentCarStats _currentCarStats;
        private Rigidbody _rb;

        private ITransmission _transmission;

        public void Initialize(VehicleStats stats, WheelController[] wheelContollers,
            WheelController[] rearWheelControllers, CurrentCarStats currentCarStats, Rigidbody rb, ITransmission transmission)
        {
            _stats = stats;

            _currentCarStats = currentCarStats;
            _rearWheelControllers = rearWheelControllers;

            int size1 = wheelContollers.Length;
            _tireControllers = new TireController[size1];
            for (int i = 0; i < size1; i++)
                _tireControllers[i] = wheelContollers[i].GetComponent<TireController>();

            _rearWheelControllersCount = rearWheelControllers.Length;
            _rearTireControllers = new TireController[_rearWheelControllersCount];
            for (int i = 0; i < _rearWheelControllersCount; i++)
                _rearTireControllers[i] = _rearWheelControllers[i].GetComponent<TireController>();

            _rb = rb;
            _transmission = transmission;
        }

        public void Break(float gasInput, float brakeInput, bool handbrakePulled)
        {
            if (_currentCarStats.InAir)
                return;
            
            //depending on whether the transmission is automatic or manual, the input keys get interpreted differently
            brakeInput = _transmission.DetermineBreakInput(gasInput, brakeInput);

            if (brakeInput != 0)
                _rb.drag = _stats.BrakesSO.BrakesStrength / (Mathf.Abs(_currentCarStats.SpeedInMsPerS) + 1);
            else
                _rb.drag = _stats.BodySO.ForwardDrag;

            int size = _tireControllers.Length;
            for (int i = 0; i < size; i++)
                _tireControllers[i].DecreaseFriction(gasInput >= brakeInput && gasInput > 0 && brakeInput > 0, gasInput / brakeInput);

            if(brakeInput == 0)
                Handbrake(handbrakePulled, gasInput);
        }

        private void Handbrake(bool engaged, float gasInput)
        {
            _currentCarStats.HandbrakePulled = engaged;

            float speedMultiplier = _currentCarStats.SpeedInMsPerS > 0 ? -1 : 1;

            if (engaged)
                _rb.drag = _stats.BrakesSO.BrakesStrength / 2 / 
                    (Mathf.Abs(_currentCarStats.SidewaysForce) + 1) / 
                    (Mathf.Abs(_currentCarStats.AccelerationForce) + 1) / 
                    (Mathf.Abs(_currentCarStats.SpeedInMsPerS) + 1);
            else
                _rb.drag = _stats.BodySO.ForwardDrag;   

            if (Mathf.Abs(_currentCarStats.SpeedInMsPerS) < 1)
            {
                ApplyHandbrake(engaged ? 1 : 0, gasInput);
                return;
            }

            ApplyHandbrake(engaged ? _stats.BrakesSO.HandbrakeForce * speedMultiplier : 0, gasInput);            
        }

        private void ApplyHandbrake(float force, float gasInput)
        {
            for (int i = 0; i < _rearWheelControllersCount; i++)
            {
                _rearWheelControllers[i].BrakeForce = force;
                _rearTireControllers[i].ApplyHandbrake(force != 0, gasInput, _stats.BrakesSO.HandbrakeTractionPercent);
            }
        }
    }
}
