using UnityEngine;

namespace Assets.VehicleController
{
    public class Brakes : IBrakes
    {
        private VehiclePartsSetWrapper _partsPresetWrapper;
        private VehicleAxle[] _axleArray;
        private VehicleAxle[] _rearAxleArray;

        private int _axleCount;
        private int _rearAxleCount;
        private CurrentCarStats _currentCarStats;
        private Rigidbody _rb;

        private ITransmission _transmission;

        public void Initialize(VehiclePartsSetWrapper partsPresetWrapper, VehicleAxle[] axleArray,
            VehicleAxle[] rearAxleArray, CurrentCarStats currentCarStats, Rigidbody rb, ITransmission transmission)
        {
            _partsPresetWrapper = partsPresetWrapper;

            _currentCarStats = currentCarStats;

            _axleArray = axleArray;
            _axleCount = axleArray.Length;

            _rearAxleArray = rearAxleArray;
            _rearAxleCount = rearAxleArray.Length;

            _rb = rb;
            _transmission = transmission;
        }

        public void Break(float gasInput, float brakeInput, bool handbrakePulled)
        {
            if (_currentCarStats.InAir)
                return;

            //depending on whether the transmission is automatic or manual, the input keys get interpreted differently
            brakeInput = _transmission.DetermineBrakeInput(gasInput, brakeInput);

            if (brakeInput != 0)
                _rb.drag = (_partsPresetWrapper.Brakes.BrakesStrength / _partsPresetWrapper.Body.Mass / (Mathf.Abs(_currentCarStats.SpeedInMsPerS) + 1)) * brakeInput;
            else
                _rb.drag = _partsPresetWrapper.Body.ForwardDrag;

            for (int i = 0; i < _axleCount; i++)
            {
                _axleArray[i].LeftHalfShaft.WheelController.DecreaseFriction(gasInput >= brakeInput && gasInput > 0 && brakeInput > 0, gasInput / brakeInput);
                _axleArray[i].RightHalfShaft.WheelController.DecreaseFriction(gasInput >= brakeInput && gasInput > 0 && brakeInput > 0, gasInput / brakeInput);
            }

            if (brakeInput == 0)
                Handbrake(handbrakePulled, gasInput);
        }

        private void Handbrake(bool engaged, float gasInput)
        {
            float speedMultiplier = _currentCarStats.SpeedInMsPerS > 0 ? -1 : 1;

            if (engaged)
                _rb.drag = _partsPresetWrapper.Brakes.BrakesStrength / 2 /
                    (Mathf.Abs(_currentCarStats.SidewaysForce) + 1) /
                    (Mathf.Abs(_currentCarStats.AccelerationForce) + 1) /
                    _partsPresetWrapper.Body.Mass /
                    (Mathf.Abs(_currentCarStats.SpeedInMsPerS) + 1);
            else
                _rb.drag = _partsPresetWrapper.Body.ForwardDrag;

            if (Mathf.Abs(_currentCarStats.SpeedInMsPerS) < 1)
            {
                ApplyHandbrake(engaged ? 1 : 0, gasInput);
                return;
            }

            ApplyHandbrake(engaged ? _partsPresetWrapper.Brakes.HandbrakeForce * speedMultiplier : 0, gasInput);
        }

        private void ApplyHandbrake(float force, float gasInput)
        {
            for (int i = 0; i < _rearAxleCount; i++)
            {
                _rearAxleArray[i].ApplyBraking(force);
                _rearAxleArray[i].ApplyHandbrake(force != 0, gasInput, _partsPresetWrapper.Brakes.HandbrakeTractionPercent);
            }
        }
    }
}
