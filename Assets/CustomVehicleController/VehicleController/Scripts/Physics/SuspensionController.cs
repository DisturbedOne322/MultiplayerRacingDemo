using UnityEngine;


namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Physics/Suspension Controller")]
    public class SuspensionController: MonoBehaviour
    {
        private float _wheelRadius;

        private VehicleStats _stats;

        private float _minSpringLen;
        private float _maxSpringLen;
        private float _springLen;
        private float _lastSpringLen;

        private float _springVelocity;

        private Vector3 _restPosition;

        private Vector3 _wheelInitialPosition;

        private Vector3 _wheelPosition;

        private bool _isFrontSusp;

        private const float SM_DAMP_TIME = 0.25f;
        private Vector3 _smDampVelocity;

#if UNITY_EDITOR
        private SuspensionSO _frontSuspensionSO;
        private SuspensionSO _rearSuspensionSO;
#endif

        public void Initialize(VehicleStats stats, bool front, float wheelRadius, Transform wheelVisual)
        {
            _stats = stats;
            _isFrontSusp = front;
            _wheelRadius = wheelRadius;

            _wheelInitialPosition = wheelVisual.transform.localPosition;
            UpdateSpringStats();
            _wheelPosition = _restPosition;

#if UNITY_EDITOR
            _stats.OnFieldChanged += OnFieldChanged;
            _frontSuspensionSO = _stats.FrontSuspensionSO;
            _rearSuspensionSO = _stats.RearSuspensionSO;
            _frontSuspensionSO.OnSuspensionStatsChanged += OnSuspensionStatsChanged;
            _rearSuspensionSO.OnSuspensionStatsChanged += OnSuspensionStatsChanged;
#endif
        }
#if UNITY_EDITOR
        private void OnSuspensionStatsChanged()
        {
            UpdateSpringStats();
        }

        private void OnFieldChanged()
        {
            _frontSuspensionSO.OnSuspensionStatsChanged -= OnSuspensionStatsChanged;
            _rearSuspensionSO.OnSuspensionStatsChanged -= OnSuspensionStatsChanged;

            _frontSuspensionSO = _stats.FrontSuspensionSO;
            _rearSuspensionSO = _stats.RearSuspensionSO;
            _frontSuspensionSO.OnSuspensionStatsChanged += OnSuspensionStatsChanged;
            _rearSuspensionSO.OnSuspensionStatsChanged += OnSuspensionStatsChanged;
            UpdateSpringStats();
        }
#endif

        private void UpdateSpringStats()
        {
            float restDistance = _isFrontSusp ? _stats.FrontSuspensionSO.SpringRestDistance : _stats.RearSuspensionSO.SpringRestDistance;

            float springTravelDistance = restDistance * 0.5f;

            _minSpringLen = restDistance - springTravelDistance;
            _maxSpringLen = restDistance + springTravelDistance;

            _restPosition = _wheelInitialPosition;
            _restPosition.y -= springTravelDistance;
        }

        public (Vector3, Vector3, bool) CalculateSpringForceAndHitPoint(int suspensionSimulationPrecision)
        {

            (bool hit, Vector3 averageHitPoint, Vector3 normal, float len) = FindAverageWheelContactPointAndHighestPoint(suspensionSimulationPrecision);
            if(hit)
            {
                _lastSpringLen = _springLen;
                _springLen = Mathf.Clamp(len - _wheelRadius, _minSpringLen, _maxSpringLen);

                _springVelocity = (_lastSpringLen - _springLen) / Time.fixedDeltaTime;

                UpdateWheelPosition(len);

                return (GetSuspForce(normal), averageHitPoint, true);
            }
            
            UpdateWheelAirPosition();
            return (Vector3.zero, transform.position, false);
        }

        private (bool, Vector3, Vector3, float) FindAverageWheelContactPointAndHighestPoint(int suspensionSimulationPrecision)
        {
            if (suspensionSimulationPrecision <= 1)
            {
                if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, _maxSpringLen + _wheelRadius))
                    return (true, hit.point, hit.normal, hit.distance);

                return (false, Vector3.zero, Vector3.zero, 0);
            }


            int hits = 0;

            float step = _wheelRadius / (suspensionSimulationPrecision - 1) * 2;

            Vector3 averageHitPoint = Vector3.zero;
            Vector3 normalAverage = Vector3.zero;
            float averageDistance = 0;

            for (int i = 0; i < suspensionSimulationPrecision; i++)
            {
                float offsetZ = -_wheelRadius + i * step;

                Ray ray = new Ray(transform.position + transform.forward * offsetZ, -transform.up);

                if (Physics.Raycast(ray, out RaycastHit hit, _maxSpringLen + _wheelRadius))
                {
                    Debug.DrawLine(transform.position + transform.forward * offsetZ, hit.point);
                    averageHitPoint += hit.point;
                    averageDistance += hit.distance;
                    normalAverage += hit.normal;

                    hits++;
                }
            }

            if (hits == 0)
                return (false, Vector3.zero, Vector3.zero, 0);


            averageHitPoint /= hits;
            averageDistance /= hits;
            normalAverage /= hits;

            return (true, averageHitPoint, normalAverage, averageDistance);
        }

        private Vector3 GetSuspForce(Vector3 normal)
        {
            float stiffness = _isFrontSusp? _stats.FrontSuspensionSO.SpringStiffness: _stats.RearSuspensionSO.SpringStiffness;
            float restDistance = _isFrontSusp ? _stats.FrontSuspensionSO.SpringRestDistance: _stats.RearSuspensionSO.SpringRestDistance;
            float damper = _isFrontSusp ? _stats.FrontSuspensionSO.SpringDampingStiffness : _stats.RearSuspensionSO.SpringDampingStiffness;

            float springForce = stiffness * (restDistance - _springLen);
            float damperForce = damper * _springVelocity;

            return (springForce + damperForce) * normal;
        }

        private void UpdateWheelPosition(float distance)
        {
            float targetY = _wheelInitialPosition.y - (distance - _wheelRadius * 2);

            _wheelPosition = new(_wheelInitialPosition.x,
                                 targetY, 
                                 _wheelInitialPosition.z);
        }

        private void UpdateWheelAirPosition()
        {
            Vector3 target = Vector3.Lerp(_wheelInitialPosition, _restPosition, Vector3.Dot(transform.up, Vector3.up) / 2 + 0.5f);
            _wheelPosition = Vector3.SmoothDamp(_wheelPosition, target, ref _smDampVelocity, SM_DAMP_TIME);
        }

        public Vector3 GetWheelPosition() => _wheelPosition;
    }
}

