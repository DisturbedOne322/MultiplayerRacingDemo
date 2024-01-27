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
        private float _currentSpringLengthPlusGroundOffset;
        public float CurrentSpringLengthPlusGroundOffset
        {
            get => _currentSpringLengthPlusGroundOffset;
        }
        private float _springRestLength;
        public float SpringRestLength
        {
            get => _springRestLength;
        }
        private HitInformation _hitInfo;
        public HitInformation HitInfo
        {
            get => _hitInfo;
        }

        private float _lastSpringLength;

        private float _springVelocity;

        private bool _isFrontSusp;

#if UNITY_EDITOR
        private SuspensionSO _frontSuspensionSO;
        private SuspensionSO _rearSuspensionSO;
#endif

        public void Initialize(VehicleStats stats, bool front, float wheelRadius, Transform wheelVisual)
        {
            _stats = stats;
            _isFrontSusp = front;
            _wheelRadius = wheelRadius;
            _hitInfo = new ();
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

        public void CalculateSpringForceAndHitPoint(int suspensionSimulationPrecision)
        {
            FindAverageWheelContactPointAndHighestPoint(suspensionSimulationPrecision);

            if (_hitInfo.Hit)
            {
                _lastSpringLength = _currentSpringLength;
                _currentSpringLength = Mathf.Clamp(_hitInfo.Distance - _wheelRadius, _minSpringLength, _maxSpringLength);

                _springVelocity = (_lastSpringLength - _currentSpringLength) / Time.fixedDeltaTime;
            }          
        }

        private void FindAverageWheelContactPointAndHighestPoint(int suspensionSimulationPrecision)
        {
            if (suspensionSimulationPrecision <= 1)
            {
                if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, _maxSpringLength + _wheelRadius))
                {
#if UNITY_EDITOR
                    Debug.DrawLine(transform.position, hit.point);
#endif

                    _hitInfo.SetHitInfo(true, hit.point, hit.normal, hit.distance);
                    return;
                }

                _hitInfo.SetHitInfo(false, Vector3.zero, Vector3.zero, 0);
                return;
            }


            int hits = 0;

            float step = _wheelRadius / (suspensionSimulationPrecision - 1) * 2;

            Vector3 averageHitPoint = Vector3.zero;
            Vector3 normalAverage = Vector3.zero;
            float averageDistance = 0;
            float lowestSin = 1;

            for (int i = 0; i < suspensionSimulationPrecision; i++)
            {
                float offsetZ = -_wheelRadius + i * step;

                Ray ray = new Ray(transform.position + transform.forward * offsetZ, -transform.up);

                if (Physics.Raycast(ray, out RaycastHit hit, _maxSpringLength + _wheelRadius))
                {
#if UNITY_EDITOR
                    Debug.DrawLine(transform.position + transform.forward * offsetZ, hit.point);
#endif

                    float distanceMultiplierFromRadius = _wheelRadius * (Mathf.Abs(offsetZ) / _wheelRadius);

                    if(distanceMultiplierFromRadius < lowestSin)
                    {
                        lowestSin = distanceMultiplierFromRadius;
                        _currentSpringLengthPlusGroundOffset = hit.distance + hit.distance * distanceMultiplierFromRadius;
                    }


                    averageHitPoint += hit.point;
                    averageDistance += hit.distance;
                    normalAverage += hit.normal;
                    hits++;
                }
            }

            if (hits == 0)
            {
                _hitInfo.SetHitInfo(false, Vector3.zero, Vector3.zero, 0);
                return;
            }


            averageHitPoint /= hits;
            averageDistance /= hits;
            normalAverage /= hits;

            _hitInfo.SetHitInfo(true, averageHitPoint, normalAverage, averageDistance);
        }

        public float GetSuspForce()
        {
            float stiffness = _isFrontSusp? _stats.FrontSuspensionSO.SpringStiffness: _stats.RearSuspensionSO.SpringStiffness;
            float restDistance = _isFrontSusp ? _stats.FrontSuspensionSO.SpringRestDistance: _stats.RearSuspensionSO.SpringRestDistance;
            float damper = _isFrontSusp ? _stats.FrontSuspensionSO.SpringDampingStiffness : _stats.RearSuspensionSO.SpringDampingStiffness;

            float springForce = stiffness * (restDistance - _currentSpringLength);
            float damperForce = damper * _springVelocity;

            return (springForce + damperForce);
        }

        public class HitInformation
        {
            public bool Hit = false;
            public Vector3 Position = Vector3.zero;
            public Vector3 HitNormal = Vector3.zero;
            public float Distance = 0;

            public void SetHitInfo(bool hit, Vector3 pos, Vector3 normal, float dist)
            {
                Hit = hit;
                Position = pos;
                HitNormal = normal;
                Distance = dist;
            }
        }
    }
}

