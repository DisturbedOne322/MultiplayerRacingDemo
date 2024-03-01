using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Physics/Tire Controller")]
    public class TireController
    {
        private Transform _transform;

        private float _handbrakeGripMultiplier = 1;
        private float _lockedGripMultiplier = 1;

        private float _staticTireLoad;
        public float StaticTireLoad
        {
            get { return _staticTireLoad; }
        }

        private float _wheelBaseLen;

        private float _forwardSlip = 0;
        public float ForwardSlip
        {
            get { return _forwardSlip; }
        }

        private float _sidewaysSlip;
        public float SidewaysSlip
        {
            get { return _sidewaysSlip; }
        }

        private float _sidewaysDot;
        public float SidewaysDot
        {
            get { return _sidewaysDot; }
        }

        private float _returnGripTime = 2;

        private VehiclePartsSetWrapper _partsPresetWrapper;

        private bool _isFrontTire;
        private Rigidbody _rb;

        private float _currentLoad;

        private float _minLoad;
        private float _maxLoad;

        public TireController(Transform transform, VehiclePartsSetWrapper partsPresetWrapper, bool front, float axelLen,
            float wheelBaseLen, Rigidbody rb)
        {
            _transform = transform;
            _isFrontTire = front;
            _partsPresetWrapper = partsPresetWrapper;
            _staticTireLoad = (axelLen / wheelBaseLen) * partsPresetWrapper.Body.Mass * 9.81f;
            _minLoad = _staticTireLoad - _staticTireLoad / 2;
            _maxLoad = _staticTireLoad * 3;
            _wheelBaseLen = wheelBaseLen;
            _rb = rb;
        }

        public float CalculateTireLoad(float accel, float distanceToGround)
        {
            if (_isFrontTire)
                return CalculateFrontTireLoad(accel, distanceToGround);
            else
                return CalculateRearTireLoad(accel, distanceToGround);
        }
        public float CalculateFrontTireLoad(float accel, float distanceToGround)
        {
            float weightTransfer = (distanceToGround / _wheelBaseLen) * _partsPresetWrapper.Body.Mass * accel / 2;
            float totalLoad = _staticTireLoad - weightTransfer;
            _currentLoad = (_staticTireLoad - weightTransfer) / 3000;
            return Mathf.Clamp(totalLoad, _minLoad, _maxLoad);
        }
        public float CalculateRearTireLoad(float accel, float distanceToGround)
        {
            float weightTransfer = (distanceToGround / _wheelBaseLen) * _partsPresetWrapper.Body.Mass * accel / 2;
            float totalLoad = _staticTireLoad + weightTransfer;
            _currentLoad = (_staticTireLoad + weightTransfer) / 3000;
            return Mathf.Clamp(totalLoad, _minLoad, _maxLoad);
        }

        public void CalculateForwardSlip(float accelForce, float wheelLoad, float speed)
        {
            if (_isFrontTire)
                CalculateTireForwardSlip(accelForce, wheelLoad, speed, _partsPresetWrapper.FrontTires.ForwardGrip);
            else
                CalculateTireForwardSlip(accelForce, wheelLoad, speed, _partsPresetWrapper.RearTires.ForwardGrip);
        }

        private void CalculateTireForwardSlip(float accelForce, float wheelLoad, float speed, float tireGrip)
        {
            float maxGrip = wheelLoad * tireGrip * _lockedGripMultiplier;

            accelForce = Mathf.Abs(accelForce);
            speed = Mathf.Abs(speed);

            if (accelForce / _rb.mass > speed * tireGrip * _lockedGripMultiplier)
            {
                if (speed < 1)
                    speed = 1;
                _forwardSlip = (accelForce / _rb.mass) / (speed * tireGrip * _lockedGripMultiplier);
                return;
            }

            if (accelForce < maxGrip)
                _forwardSlip = 0;
            else
                _forwardSlip = accelForce / maxGrip;
        }

        public Vector3 CalculateSidewaysForce(float speed, float speedPercent)
        {
            Vector3 steeringDir = _transform.right;
            Vector3 tireWorldVel = _rb.GetPointVelocity(_transform.position);

            float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);
            _sidewaysDot = Vector3.Dot(_rb.velocity.normalized, steeringDir);

            if (_isFrontTire)
                return CalculateTiresSidewaysForce(speed, speedPercent, steeringVel, steeringDir,
                                                    _partsPresetWrapper.FrontTires.SteeringStiffness,
                                                    _partsPresetWrapper.FrontTires.SidewaysGripCurve,
                                                    _partsPresetWrapper.FrontTires.SidewaysSlipCurve);
            else
                return CalculateTiresSidewaysForce(speed, speedPercent, steeringVel, steeringDir,
                                                    _partsPresetWrapper.RearTires.SteeringStiffness,
                                                    _partsPresetWrapper.RearTires.SidewaysGripCurve,
                                                    _partsPresetWrapper.RearTires.SidewaysSlipCurve);
        }

        private Vector3 CalculateTiresSidewaysForce(float speed, float speedPercent, float steeringVel, Vector3 steeringDir,
            float tireCorneringStiffnessMax, AnimationCurve gripCurve, AnimationCurve slipCurve)
        {
            if (speed < 0.1f)
                _sidewaysSlip = 0;
            else
                _sidewaysSlip = 1 - slipCurve.Evaluate(Mathf.Abs(_sidewaysDot));

            float desiredVelocityChange = -steeringVel * (1 - _sidewaysSlip) * _handbrakeGripMultiplier;

            float desiredAccel = (desiredVelocityChange) / 0.02f;

            float tireCorneringStiffness = tireCorneringStiffnessMax * gripCurve.Evaluate(speedPercent);

            return (tireCorneringStiffness + GetResistanceToOrthogonalMotion(speed, tireCorneringStiffness)) * desiredAccel * steeringDir;
        }

        private float GetResistanceToOrthogonalMotion(float speed, float tireStiffness)
        {
            speed = Mathf.Clamp(Mathf.Abs(speed), 0, tireStiffness);
            return Mathf.Abs(_sidewaysDot) * (tireStiffness - speed);
        }

        public void ApplyHandbrake(bool engaged, float effectStrength, float tractionPercent)
        {
            if (engaged)
                _handbrakeGripMultiplier = Mathf.Clamp(_handbrakeGripMultiplier - Time.deltaTime * 2 * effectStrength, tractionPercent, 1);
            else
                _handbrakeGripMultiplier = Mathf.Clamp(_handbrakeGripMultiplier + Time.deltaTime / (_returnGripTime / _partsPresetWrapper.RearTires.ForwardGrip) * (1 - _sidewaysSlip), tractionPercent, 1);
        }

        public void DecreaseFriction(bool locked, float effectStrength)
        {
            if (locked)
                _lockedGripMultiplier = Mathf.Clamp(_lockedGripMultiplier - Time.deltaTime * effectStrength, 0.1f, 1);
            else
                _lockedGripMultiplier = Mathf.Clamp(_lockedGripMultiplier + Time.deltaTime / _returnGripTime * (1 - _sidewaysSlip), 0.1f, 1);
        }
    }
}

