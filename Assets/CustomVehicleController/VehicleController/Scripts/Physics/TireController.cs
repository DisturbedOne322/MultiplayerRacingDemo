using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Physics/Tire Controller")]
    public class TireController : MonoBehaviour
    {
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

        private VehicleStats _stats;

        private bool _isFrontTire;
        private Rigidbody _rb;

        private float _minLoad;
        private float _maxLoad;

        public void Initialize(VehicleStats stats, bool front, float axelLen,
            float wheelBaseLen, Rigidbody rb)
        {
            _isFrontTire = front;
            _stats = stats;
            _staticTireLoad = (axelLen / wheelBaseLen) * stats.BodySO.Mass * 9.81f;
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
            float weightTransfer = (distanceToGround / _wheelBaseLen) * _stats.BodySO.Mass * accel / 2;
            float totalLoad = _staticTireLoad - weightTransfer;
            return Mathf.Clamp(totalLoad, _minLoad, _maxLoad);
        }
        public float CalculateRearTireLoad(float accel, float distanceToGround)
        {
            float weightTransfer = (distanceToGround / _wheelBaseLen) * _stats.BodySO.Mass* accel / 2;
            float totalLoad = _staticTireLoad + weightTransfer;
            return Mathf.Clamp(totalLoad, _minLoad, _maxLoad);
        }

        public void CalculateForwardSlip(float accelForce, float wheelLoad, float speed)
        {
            if (_isFrontTire)
                CalculateTireForwardSlip(accelForce, wheelLoad, speed, _stats.FrontTiresSO.ForwardGrip);
            else
                CalculateTireForwardSlip(accelForce, wheelLoad, speed, _stats.RearTiresSO.ForwardGrip);
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
            Vector3 steeringDir = transform.right;
            Vector3 tireWorldVel = _rb.GetPointVelocity(transform.position);

            float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);
            _sidewaysDot = Vector3.Dot(_rb.velocity.normalized, steeringDir);

            if (_isFrontTire)
                return CalculateTiresSidewaysForce(speed, speedPercent, steeringVel, steeringDir,
                                                    _stats.FrontTiresSO.CorneringStiffness,
                                                    _stats.FrontTiresSO.SidewaysGripCurve,
                                                    _stats.FrontTiresSO.SidewaysSlipCurve);
            else
                return CalculateTiresSidewaysForce(speed, speedPercent, steeringVel, steeringDir, 
                                                    _stats.RearTiresSO.CorneringStiffness,
                                                    _stats.RearTiresSO.SidewaysGripCurve,
                                                    _stats.RearTiresSO.SidewaysSlipCurve);
        }

        private Vector3 CalculateTiresSidewaysForce(float speed, float speedPercent, float steeringVel, Vector3 steeringDir, 
            float tireCorneringStiffness, AnimationCurve gripCurve, AnimationCurve slipCurve)
        {
            if (Mathf.Abs(speed) < 1)
                _sidewaysSlip = 0;
            else
                _sidewaysSlip = 1 - slipCurve.Evaluate(Mathf.Abs(_sidewaysDot));

            float desiredVelocityChange = -steeringVel * (1 - _sidewaysSlip) * _handbrakeGripMultiplier;

            float desiredAccel = desiredVelocityChange / Time.fixedDeltaTime;
            return (tireCorneringStiffness * gripCurve.Evaluate(speedPercent)) * desiredAccel * steeringDir;
        }

        public void ApplyHandbrake(bool engaged, float effectStrength, float tractionPercent)
        {
            if (engaged)
                _handbrakeGripMultiplier = Mathf.Clamp(_handbrakeGripMultiplier - Time.deltaTime * 2 * effectStrength, tractionPercent, 1);
            else
                _handbrakeGripMultiplier = Mathf.Clamp(_handbrakeGripMultiplier + Time.deltaTime / (_returnGripTime / _stats.RearTiresSO.ForwardGrip) * (1 - _sidewaysSlip), tractionPercent, 1);
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

