using UnityEngine;

namespace Assets.VehicleController
{
    [RequireComponent(typeof(SuspensionController)), AddComponentMenu("CustomVehicleController/Physics/Wheel Controller")]
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
        public bool HasContactWithGround
        {
            get { return _springController.HitInfo.Hit; }
        }
        #endregion

        #region Visuals
        [SerializeField]
        private Transform _wheelMeshTransform;
        private Vector3 _wheelPosition;
        public Vector3 WheelPosition
        {
            get => _wheelPosition;
        }
        private Vector3 _wheelInitialPosition;
        private float _distanceFromSuspensionTopPointToWheelTopPoint;
        private const float SM_DAMP_TIME = 0.25f;
        private Vector3 _smDampVelocity;

        #endregion

        public void Initialize(SuspensionController suspensionController, Transform wheelMeshTransform, VehicleStats vehicleStats, Rigidbody rb, float wheelBaseLen, float axelLen, bool front)
        {
            _rb = rb;
            _springController = suspensionController;
            _wheelMeshTransform = wheelMeshTransform;
            _tireController = new(transform, vehicleStats, front, axelLen, wheelBaseLen, _rb);
            _wheelInitialPosition = _wheelMeshTransform.transform.localPosition;
            _wheelPosition = _wheelInitialPosition;

            if (_wheelRadius == 0)
            {
                if (_wheelMeshTransform.TryGetComponent<MeshRenderer>(out MeshRenderer mesh))
                {
                    _wheelRadius = mesh.bounds.size.y / 2;
                }
                else
                {
#if UNITY_EDITOR
                    Debug.Log("Wheel Controller couldn't automatically define wheel's radius as mesh renderer was not found. " +
                        "\n Radius has been set to default value (0.25). Set the radius to correct value manually in edit mode.");
#endif
                    _wheelRadius = 0.25f;
                }
            }
            else
            {
#if UNITY_EDITOR
                if (_wheelMeshTransform.TryGetComponent<MeshRenderer>(out MeshRenderer mesh))
                {
                    Debug.Log("You have set the wheel radius on wheel " + gameObject.name + " manually, so it won't calculated from mesh renderer automatically.");
                }
#endif
            }
            _distanceFromSuspensionTopPointToWheelTopPoint = transform.position.y - (_wheelMeshTransform.position.y + _wheelRadius);
        }

        public void ControlWheel(float speed, float speedPercent, float acceleration, float distanceToGround)
        {
            transform.localRotation = Quaternion.Euler(new Vector3(0, steerAngle, 0));

            _tireController.CalculateForwardSlip(Torque, _tireController.CalculateTireLoad(acceleration, distanceToGround), Mathf.Abs(speed));
            CalculateWheelRPM(speed);

            if (!HasContactWithGround)
            {
                UpdateWheelAirPosition();
                return;
            }
            ApplyBraking(_wheelMeshTransform.position);
            ApplySteering(_wheelMeshTransform.position, speed, speedPercent);
            ApplyTorque(_wheelMeshTransform.position);

            UpdateWheelPosition();
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

        public void ApplyHandbrake(bool engaged, float effectStrength, float tractionPercent)
        {
            _tireController.ApplyHandbrake(engaged, effectStrength, tractionPercent);
        }

        public void DecreaseFriction(bool locked, float effectStrength)
        {
            _tireController.DecreaseFriction(locked, effectStrength);
        }

        private void CalculateWheelRPM(float speed)
        {
            _speedRpm = speed / _wheelRadius;

            if (HasContactWithGround)
            {
                // this rpm is used to shift gears
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

        private void UpdateWheelPosition()
        {
            float targetY = _wheelInitialPosition.y + _distanceFromSuspensionTopPointToWheelTopPoint - (_springController.CurrentSpringLengthPlusGroundOffset - _wheelRadius * 2);
            _wheelPosition = new(_wheelInitialPosition.x,
                                 targetY,
                                 _wheelInitialPosition.z);
        }

        private void UpdateWheelAirPosition()
        {
            Vector3 target = Vector3.Lerp(
                _wheelInitialPosition - new Vector3(0, _springController.MinimumSpringLength - _distanceFromSuspensionTopPointToWheelTopPoint - _wheelRadius, 0), 
                _wheelInitialPosition - new Vector3(0, _springController.MaximumSpringLength - _distanceFromSuspensionTopPointToWheelTopPoint - _wheelRadius, 0),
                Vector3.Dot(transform.up, Vector3.up) / 2 + 0.5f);

            _wheelPosition = Vector3.SmoothDamp(_wheelPosition, target, ref _smDampVelocity, SM_DAMP_TIME);
        }

        public void SetWheelMeshTransform(Transform transform) => _wheelMeshTransform = transform;

        public bool IsExceedingSlipThreshold(float forwardSlip, float sidewaysSlip)
        {
            if (!HasContactWithGround)
                return false;


            if (SidewaysSlip > sidewaysSlip || ForwardSlip > forwardSlip)
                return true;

            return false;
        }

        //for the editor, it uses it to define if the controller has been initialized
        public Transform GetWheelTransform() => _wheelMeshTransform;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(_wheelMeshTransform.position, _wheelRadius);
        }
#endif
    }
}
