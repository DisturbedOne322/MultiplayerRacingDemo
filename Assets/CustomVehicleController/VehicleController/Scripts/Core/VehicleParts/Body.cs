using UnityEngine;

namespace Assets.VehicleController
{

    public class Body : IBody
    {
        private Rigidbody _rb;
        private Transform _transform;
        private Transform _coM; 

        private VehicleStats _stats;
        private CurrentCarStats _currentCarStats;


        private float _flipOverRecoverTimer = 0;

        private const float MAX_SENSITIVITY_AFTER_AERIAL_TIME = 2f;

        private float _handbrakeEffect = 0;
        private const float HANDBRAKE_EFFECT_SPEED = 1;

        public void Initialize(Rigidbody rb, VehicleStats stats, CurrentCarStats currentCarStats, Transform transform, Transform centerOfMass)
        {
            _rb = rb;
            _rb.centerOfMass = centerOfMass.localPosition;
            _stats = stats;
            _currentCarStats = currentCarStats;
            _coM = centerOfMass;
            this._transform = transform;
        }

        public void AddDownforce() => _rb.AddForce(_stats.BodySO.Downforce * Mathf.Abs(_currentCarStats.SpeedInMsPerS) * -Vector3.up);

        public void AddCorneringForce()
        {
            if (_currentCarStats.InAir)
                return;

            float slipAngle = Vector3.SignedAngle(_transform.forward, _rb.velocity, Vector3.up);

            //in case of reversing
            if (slipAngle > 90)
                slipAngle -= 180;
            else if (slipAngle < -90)
                slipAngle += 180;

            _handbrakeEffect += _currentCarStats.HandbrakePulled? Time.deltaTime * HANDBRAKE_EFFECT_SPEED : -Time.deltaTime * HANDBRAKE_EFFECT_SPEED;
            _handbrakeEffect = Mathf.Clamp(_handbrakeEffect, 0, _stats.BrakesSO.HandbrakeTractionPercent);
            
            //car self centering effect
            _rb.AddRelativeTorque(slipAngle * _currentCarStats.SpeedInMsPerS * _stats.BodySO.CorneringResistanceStrength / (_rb.angularVelocity.magnitude + 1) * (1 - _handbrakeEffect) * _transform.up, ForceMode.Force);
            //car control becoming stiffer and higher speed effect
            _rb.angularDrag = 1 + _stats.BodySO.CorneringResistanceCurve.Evaluate(_currentCarStats.SpeedPercent) *
                              _stats.BodySO.CorneringResistanceStrength * (1 - _handbrakeEffect);
        }

        public void HandleAirDrag()
        {
            //since braking is performed by changing drag, refrain from changing it here
            if (_currentCarStats.Braking)
                return;

            _rb.mass = _stats.BodySO.Mass;
            _rb.drag = _currentCarStats.InAir ? _stats.BodySO.MidAirDrag : _stats.BodySO.ForwardDrag;
        }

        public void AutomaticFlipOverRecover(float flipOverRecoverTimerTotal)
        {
            if (_currentCarStats.FlipperOver && _currentCarStats.InAir)
                _flipOverRecoverTimer += Time.deltaTime;
            else
                _flipOverRecoverTimer = 0;

            if (_flipOverRecoverTimer > flipOverRecoverTimerTotal)
            {
                Vector3 fwd = _transform.forward;
                _transform.rotation = Quaternion.identity;
                _transform.forward = fwd;
                _flipOverRecoverTimer = 0;
            }
        }

        public void PerformAerialControls(float sensitivity, float xInput, float yInput)
        {
            if (!_currentCarStats.InAir)
                return;
            
            float airTime = Mathf.Clamp(_currentCarStats.AirTime, 0, MAX_SENSITIVITY_AFTER_AERIAL_TIME);

            float sensMultiplier = airTime / MAX_SENSITIVITY_AFTER_AERIAL_TIME;

            Vector3 rotateRightForce = -xInput * sensMultiplier * sensitivity * _transform.forward;
            Vector3 rotateForwardForce = yInput * sensMultiplier * sensitivity * _transform.right;
            _rb.AddTorque(rotateForwardForce + rotateRightForce);            
        }
    }
}
