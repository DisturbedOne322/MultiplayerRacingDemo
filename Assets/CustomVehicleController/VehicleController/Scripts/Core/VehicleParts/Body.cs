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

        public void Initialize(Rigidbody rb, VehicleStats stats, CurrentCarStats currentCarStats, Transform transform, Transform centerOfMass)
        {
            _rb = rb;
            _rb.centerOfMass = centerOfMass.localPosition;
            _stats = stats;
            _currentCarStats = currentCarStats;
            _coM = centerOfMass;
            this._transform = transform;
        }

        public void AddDownforce()
        {
            _rb.mass = _stats.BodySO.Mass;
            _rb.AddForce(_stats.BodySO.Downforce * Mathf.Abs(_currentCarStats.SpeedInMsPerS) * -Vector3.up);
        }

        public void AddCorneringForce()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                Vector3 fwd = _transform.forward;
                _transform.rotation = Quaternion.identity;
                _transform.forward = fwd;
                _flipOverRecoverTimer = 0;
            }
            if (_currentCarStats.InAir)
                return;

            float slipAngle = Vector3.SignedAngle(_transform.forward, _rb.velocity, Vector3.up);

            ////in case of reversing
            if (slipAngle >= 90)
                slipAngle = -slipAngle + 180;
            else if (slipAngle <= -90)
                slipAngle = -slipAngle - 180;

            _handbrakeEffect = _currentCarStats.HandbrakePulled ? _stats.BrakesSO.HandbrakeTractionPercent : 1;
            //lower effect at higher speed
            _handbrakeEffect += _currentCarStats.SpeedPercent * (1 - _handbrakeEffect);

            //car self centering effect
            _rb.AddTorque(slipAngle * _currentCarStats.SpeedInMsPerS * _stats.BodySO.CorneringResistanceCurve.Evaluate(_currentCarStats.SpeedPercent) *
                              _stats.BodySO.CorneringResistanceStrength * (_rb.angularVelocity.magnitude + 1) * 
                              _handbrakeEffect * _transform.up, ForceMode.Force);
            //car control becoming stiffer and higher speed effect
            _rb.angularDrag = _stats.BodySO.CorneringResistanceCurve.Evaluate(_currentCarStats.SpeedPercent) *
                              _stats.BodySO.CorneringResistanceStrength * _handbrakeEffect;
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

        public void PerformAerialControls(float sensitivity, float pitchInput, float yawInput, float rollInput)
        {
            if (!_currentCarStats.InAir)
                return;
            
            float airTime = Mathf.Clamp(_currentCarStats.AirTime, 0, MAX_SENSITIVITY_AFTER_AERIAL_TIME);

            float sensMultiplier = airTime / MAX_SENSITIVITY_AFTER_AERIAL_TIME;

            Vector3 rotatePitchForce = pitchInput * sensMultiplier * sensitivity * _transform.right;
            Vector3 rotateYawForce = yawInput * sensMultiplier * sensitivity * _transform.up;
            Vector3 rotateRollForce = -rollInput * sensitivity * _transform.forward;
            _rb.AddTorque(rotatePitchForce + rotateYawForce + rotateRollForce);            
        }
    }
}
