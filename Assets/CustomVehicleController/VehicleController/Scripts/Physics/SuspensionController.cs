using UnityEngine;


namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Physics/Suspension Controller")]
    public class SuspensionController: MonoBehaviour
    {
        private float _wheelRadius;

        private VehicleStats _stats;

        private float _minSpringLength;
        public float MinimumSpringLength
        {
            get => _minSpringLength;
        }
        private float _maxSpringLength;
        public float MaximumSpringLength
        {
            get => _maxSpringLength;
        }
        private float _currentSpringLength;
        public float CurrentSpringLength
        {
            get => _currentSpringLength;
        }
        private float _springRestLength;
        public float SpringRestLength
        {
            get => _springRestLength;
        }

        private float _lastSpringLength;

        private float _springVelocity;

        private bool _isFrontSusp;

        [SerializeField]
        private AntiRollBar _antiRollBar;
        public float _antiroll;

#if UNITY_EDITOR
        private SuspensionSO _frontSuspensionSO;
        private SuspensionSO _rearSuspensionSO;
#endif

        public void Initialize(VehicleStats stats, bool front, float wheelRadius, Transform wheelVisual)
        {
            _stats = stats;
            _isFrontSusp = front;
            _wheelRadius = wheelRadius;
            UpdateSpringStats();
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
            _springRestLength = _isFrontSusp ? _stats.FrontSuspensionSO.SpringRestDistance : _stats.RearSuspensionSO.SpringRestDistance;

            float springTravelDistance = _springRestLength * 0.33f;

            _minSpringLength = _springRestLength - springTravelDistance;
            _maxSpringLength = _springRestLength + springTravelDistance; 
        }

        public (Vector3, Vector3, bool) CalculateSpringForceAndHitPoint(int suspensionSimulationPrecision)
        {
            (bool hit, Vector3 averageHitPoint, Vector3 normal, float len) = FindAverageWheelContactPointAndHighestPoint(suspensionSimulationPrecision);

            if (hit)
            {
                _lastSpringLength = _currentSpringLength;
                _currentSpringLength = Mathf.Clamp(len - _wheelRadius, _minSpringLength, _maxSpringLength);

                _springVelocity = (_lastSpringLength - _currentSpringLength) / Time.fixedDeltaTime;

                return (GetSuspForce(normal), averageHitPoint, true);
            }
            
            return (Vector3.zero, transform.position, false);
        }

        private (bool, Vector3, Vector3, float) FindAverageWheelContactPointAndHighestPoint(int suspensionSimulationPrecision)
        {
            if (suspensionSimulationPrecision <= 1)
            {
                if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, _maxSpringLength + _wheelRadius))
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

                if (Physics.Raycast(ray, out RaycastHit hit, _maxSpringLength + _wheelRadius))
                {
#if UNITY_EDITOR
                    Debug.DrawLine(transform.position + transform.forward * offsetZ, hit.point);
#endif
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

            float springForce = stiffness * (restDistance - _currentSpringLength);
            float damperForce = damper * _springVelocity;

            _antiroll = _antiRollBar.GetAntiRollForce(_currentSpringLength, _maxSpringLength, this);

            return (springForce + damperForce + _antiroll) * normal;
        }
    }
}

