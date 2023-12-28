using UnityEngine;

namespace Assets.VehicleController
{
    [RequireComponent(typeof(SuspensionController)), RequireComponent(typeof(TireController)), AddComponentMenu("CustomVehicleController/Physics/Wheel Controller")]
    public class WheelController : MonoBehaviour
    {
        private Rigidbody _rb;
        private SuspensionController _springController;
        private TireController _tireController;

        #region Forces
        public float Torque { get; set; }
        public float BrakeForce { get; set; }
        #endregion

        #region Variables

        [Header("Wheel radius can be calculated automatically from the mesh renderer. \nIn case the radius is wrong, set it manually.\nIf the radius value isn't 0, radius won't be calculated.")]
        [SerializeField]
        private float _wheelRadius;
        public float Radius
        {
            get { return _wheelRadius; }
        }
        private float _speedRpm = 0;
        public float WheelRPM
        {
            get { return _speedRpm; }
        }
        private float _visualRpm = 0;
        public float VisualRPM
        {
            get { return _visualRpm; }
        }
        private float steerAngle;
        public float SteerAngle
        {
            get { return transform.localRotation.eulerAngles.y; }
            set { steerAngle = value; }
        }

        private float _slipWheelRPM = 0;
        private const float MAX_SLIP_WHEEL_RPM = 50000;

        public float SidewaysSlip
        {
            get { return _tireController.SidewaysSlip; }
        }
        public float ForwardSlip
        {
            get { return _tireController.ForwardSlip; }
        }
        private bool _hasContactWithGround;
        public bool HasContactWithGround
        {
            get { return _hasContactWithGround; }
        }
        private Vector3 _hitPosition;
        private Vector3 _springForce;
        #endregion

        #region Visuals
        [SerializeField, Tooltip("Transform component of the wheel this this component represents.")]
        private Transform _wheelMeshTransform;
        public Vector3 WheelPosition
        {
            get { return _springController.GetWheelPosition(); }
        }
        #endregion

        public void Initialize(VehicleStats vehicleStats, Rigidbody rb, float wheelBaseLen, float axelLen, bool front)
        {
            if (_wheelRadius == 0)
            {
                if (_wheelMeshTransform.TryGetComponent<MeshRenderer>(out MeshRenderer mesh))
                {
                    _wheelRadius = mesh.bounds.size.y / 2;
                }
                else
                {
#if UNITY_EDITOR
                    Debug.Log("Wheel Controller couldn't automatically define wheel's radius as mesh renderer was not found " +
                        "and you haven't set the radius value either. Radius has been set to default value (0.25).");
#endif
                    _wheelRadius = 0.25f;
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("You have set the wheel radius on wheel " + gameObject.name + " manually, so it won't calculated from mesh renderer automatically.");
#endif
            }
            _rb = rb;
            _springController = GetComponent<SuspensionController>();
            _springController.Initialize(vehicleStats, front, _wheelRadius, _wheelMeshTransform);
            _tireController = GetComponent<TireController>();
            _tireController.Initialize(vehicleStats, front, axelLen, wheelBaseLen, _rb);
        }

        public void ControlWheel(float speed, float speedPercent, float acceleration, float distanceToGround, int suspensionSimulationPrecision)
        {
            transform.localRotation = Quaternion.Euler(new Vector3(0, steerAngle, 0));
            (_springForce, _hitPosition, _hasContactWithGround) = _springController.CalculateSpringForceAndHitPoint(suspensionSimulationPrecision);

            _tireController.CalculateForwardSlip(Torque, _tireController.CalculateTireLoad(acceleration, distanceToGround), Mathf.Abs(speed));
            CalculateWheelRPM(speed);

            if (!_hasContactWithGround)
                return;

            ApplyBraking(_hitPosition);
            ApplySuspension(_springForce, _hitPosition);
            ApplySteering(_hitPosition, speed, speedPercent);
            ApplyTorque(_hitPosition);
        }
        private void ApplySuspension(Vector3 springForce, Vector3 pos)
        {
            _rb.AddForceAtPosition(springForce, pos);
        }
        private void ApplyTorque(Vector3 pos)
        {
            if (Torque == 0)
                return;

            float slipMultiplier = (_tireController.ForwardSlip / 10) + 1;
            float force = Torque / slipMultiplier;

            _rb.AddForceAtPosition(force * transform.forward, pos);
        }
        private void ApplySteering(Vector3 pos, float speed, float speedPercent)
        {
            _rb.AddForceAtPosition(_tireController.CalculateSidewaysForce(speed, speedPercent), pos);
        }
        private void ApplyBraking(Vector3 pos)
        {
            if (BrakeForce == 0)
                return;
            _rb.AddForceAtPosition(BrakeForce * transform.forward, pos);
        }

        private void CalculateWheelRPM(float speed)
        {
            if (_hasContactWithGround)
            {
                // this rpm is used to shift gears
                _speedRpm = speed / _wheelRadius;
                if(Torque == 0)
                {
                    _visualRpm = speed / _wheelRadius;
                    _slipWheelRPM = 0;
                    return;
                }

                //slip rpm is additive to visual rpm, if the wheel continues slipping, slip rpm increases
                _slipWheelRPM += (Torque / 100 * _tireController.ForwardSlip - speed) * Time.deltaTime;

                if (Torque > 0)
                    _slipWheelRPM = Mathf.Clamp(_slipWheelRPM, 0, MAX_SLIP_WHEEL_RPM);
                else
                    _slipWheelRPM = Mathf.Clamp(_slipWheelRPM, -MAX_SLIP_WHEEL_RPM, 0);

                //this rpm is used for engine rpm calculations and wheel rotation
                _visualRpm = speed / _wheelRadius + _slipWheelRPM;
                if (_tireController.ForwardSlip == 0)
                    _slipWheelRPM = 0;
                
                return;
            }

            _slipWheelRPM = 0;

            if (BrakeForce == 0)
            {
                if (Torque <= 0)
                    _visualRpm -= _visualRpm * Time.fixedDeltaTime * 3;
                else
                    _visualRpm += Torque / 100 * Time.fixedDeltaTime;
            }
            else
                _visualRpm -= _visualRpm * Time.fixedDeltaTime * 10;
        }

        public void SetWheelMeshTransform(Transform transform) => _wheelMeshTransform = transform;

        public Vector3 GetHitPosition() => _hitPosition;

        //for the editor, it uses it to define if the controller has been initialized
        public Transform GetWheelTransform() => _wheelMeshTransform;

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(_wheelMeshTransform.position, _wheelRadius);
        }
    }
}
