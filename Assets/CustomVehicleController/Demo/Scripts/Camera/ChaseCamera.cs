using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Assets.VehicleController
{
    [RequireComponent(typeof(Camera))]
    public class ChaseCamera : MonoBehaviour
    {
        [SerializeField]
        private CustomVehicleController _vehicleController;

        public Separator separator;

        [SerializeField]
        private Vector3 _lookAtOffset = new Vector3(0, 1, 0);
        private float _alignWithDriftAngleMultiplier = 0.0135f;

        [SerializeField]
        private float _verticalOffsetIncreaseFromSpeed = 0.5f;
        [SerializeField]
        private float _distanceIncreaseFromSpeed = 1.2f;
        [SerializeField]
        private float _distanceIncreaseFromBoostMax = 1f;
        private float _distanceIncreaseFromBoost;

        [SerializeField, Range(1f, 20f)]
        private float _distance = 5f;

        [SerializeField, Range(0.1f, 2f)]
        private float _mouseSensitivity = 1;

        [SerializeField, Range(-89f, 89f)]
        private float _minVerticalAngle = -45f, _maxVerticalAngle = 45f;

        [SerializeField, Range(0f, 90f)]
        private float _alignSmoothRange = 45f;

        [SerializeField]
        private float _recenterDelay = 1;
        private float _lastInputTime = 0;

        public Separator separator1;
        [SerializeField, Min(0f)]
        private float _minFOV = 40;
        [SerializeField, Min(0f)]
        private float _maxFOV = 60;
        [SerializeField, Min(0f)]
        private float _fovAddDuringBoost;
        private float _boostFOVeffectTime = 2;
        private float _boostTime = 0;

        private const float FOVChangeSpeed = 10f;

        private Vector3 _focusPoint, _previousFocusPoint;

        private Vector2 _orbitAngles = new Vector2(45f, 0f);
        //////////////////
        public Separator separator2;

        [SerializeField]
        private float _shakeAmount = 0.03f;

        private float _smDampVelocity = 0;
        private float _smDampTime = 0.25f;

        public Separator separator3;

        [SerializeField]
        private Volume _volume;
        private ChromaticAberration _chromaticAberration;
        private Vignette _vignette;


        private const float DEFAULT_VIG_INTENSITY = 0.15f;
        private const float MAX_VIG_INTENSITY = 0.3f;
        private const float NITRO_VIG_INTENSITY_ADD = 0.06f;

        private float MAX_SPEED_IN_MS = 80;

        private float _smDampVelocityFOV;
        private float _smDampVelocityVignette;
        private const float SmDampSpeed = 0.5f;

        void OnValidate()
        {
            if (_maxVerticalAngle < _minVerticalAngle)
            {
                _maxVerticalAngle = _minVerticalAngle;
            }
        }

        void Awake()
        {
            _focusPoint = _vehicleController.transform.position + _vehicleController.transform.TransformDirection(_lookAtOffset);
            transform.localRotation = Quaternion.Euler(_orbitAngles);

            _volume.profile.TryGet(out _chromaticAberration);
            _volume.profile.TryGet(out _vignette);
        }

        void LateUpdate()
        {
            UpdateFocusPoint();
            Quaternion lookRotation;
            if (ManualRotation() || AutomaticRotation())
            {
                ConstrainAngles();
                lookRotation = Quaternion.Euler(_orbitAngles);
            }
            else
            {
                lookRotation = transform.localRotation;
            }

            float target = _vehicleController.GetCurrentCarStats().NitroIntensity == 1 ? _distanceIncreaseFromBoostMax : 0;

            _distanceIncreaseFromBoost = Mathf.SmoothDamp(_distanceIncreaseFromBoost, target, ref _smDampVelocity, _smDampTime);

            Vector3 lookDirection = lookRotation * Vector3.forward;
            Vector3 lookPosition = _focusPoint - lookDirection * 
                (_distance + 
                    _vehicleController.GetCurrentCarStats().SpeedPercent * _distanceIncreaseFromSpeed +
                    _distanceIncreaseFromBoost
                );

            transform.SetPositionAndRotation(lookPosition, lookRotation);
            UpdateFOV();
            ShakeCamera();
            ChangeDistanceFromUserScrollInput();

            float speedPercent = Mathf.Clamp01(_vehicleController.GetCurrentCarStats().SpeedInMsPerS / MAX_SPEED_IN_MS);
            speedPercent *= speedPercent;
            HandleVignette(speedPercent);
            HandleNitroEffect();
        }

        private void HandleVignette(float speedPercent)
        {
            float nitroVig = _vehicleController.GetCurrentCarStats().NitroIntensity == 1 ? NITRO_VIG_INTENSITY_ADD : 0;

            _vignette.intensity.value = Mathf.SmoothDamp(_vignette.intensity.value,
                DEFAULT_VIG_INTENSITY + (MAX_VIG_INTENSITY + nitroVig - DEFAULT_VIG_INTENSITY) * speedPercent, ref _smDampVelocityVignette, SmDampSpeed);
        }

        private void HandleNitroEffect()
        {
            _chromaticAberration.intensity.value = _vehicleController.GetCurrentCarStats().NitroIntensity == 1 ? 1 : 0;
        }

        private void ChangeDistanceFromUserScrollInput()
        {
            _distance = Mathf.Clamp(_distance - Input.mouseScrollDelta.y, 1, 30);
        }

        private void ShakeCamera()
        {
            gameObject.transform.localPosition = transform.localPosition +
                                                    Random.insideUnitSphere * _shakeAmount * _vehicleController.GetCurrentCarStats().SpeedPercent +
                                                    Random.insideUnitSphere * _shakeAmount * _vehicleController.GetCurrentCarStats().SidewaysForce / 80;
        }

        private void UpdateFOV()
        {
            float currentFOV = Mathf.Lerp(_minFOV, _maxFOV, _vehicleController.GetCurrentCarStats().SpeedPercent);
            if (_vehicleController.GetCurrentCarStats().NitroBoosting)
            {
                currentFOV += _fovAddDuringBoost * (Mathf.Clamp01(_boostTime / _boostFOVeffectTime));
                _boostTime += Time.deltaTime;
            }
            else
            {
                _boostTime = 0;
            }

            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, currentFOV, Time.deltaTime * FOVChangeSpeed);
        }

        private void UpdateFocusPoint()
        {
            _previousFocusPoint = _focusPoint;
            Vector3 bodyOffset = _lookAtOffset;
            bodyOffset.y += _verticalOffsetIncreaseFromSpeed * _vehicleController.GetCurrentCarStats().SpeedPercent;
            _focusPoint = _vehicleController.transform.position + _vehicleController.transform.TransformDirection(bodyOffset);
        }

        private bool ManualRotation()
        {
            Vector2 input = new Vector2(
                Input.GetAxis("Mouse Y"),
                -Input.GetAxis("Mouse X")
            );
            const float e = 0.001f;
            if (input.x < -e || input.x > e || input.y < -e || input.y > e)
            {
                _orbitAngles += _mouseSensitivity * 180 * Time.unscaledDeltaTime * input;
                _lastInputTime = Time.time;
                return true;
            }
            return false;
        }

        private bool AutomaticRotation()
        {
            if (_lastInputTime + _recenterDelay > Time.time)
                return false;

            Vector2 movement = new Vector2(
                _focusPoint.x - _previousFocusPoint.x,
                _focusPoint.z - _previousFocusPoint.z
            );

            float movementDeltaSqr = movement.sqrMagnitude;
            if (movementDeltaSqr < 0.0001f)
            {
                return false;
            }

            float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));

            if (!_vehicleController.GetCurrentCarStats().Reversing)
            {
                float driftAngle = Vector3.SignedAngle(_vehicleController.transform.forward, _vehicleController.GetRigidbody().velocity.normalized, Vector3.up);
                float sign = Mathf.Sign(driftAngle);
                headingAngle -= sign * driftAngle * driftAngle * _alignWithDriftAngleMultiplier;

                while (headingAngle >= 360)
                {
                    headingAngle -= 360;
                }
                while (headingAngle <= -360)
                {
                    headingAngle += 360;
                }
            }

            float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(_orbitAngles.y, headingAngle));

            float rotationChange =
                _mouseSensitivity * 180 * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);

            if (deltaAbs < _alignSmoothRange)
                rotationChange *= deltaAbs / _alignSmoothRange;
            else if (180f - deltaAbs < _alignSmoothRange)
                rotationChange *= (180f - deltaAbs) / _alignSmoothRange;

            _orbitAngles.y =
                Mathf.MoveTowardsAngle(_orbitAngles.y, headingAngle, rotationChange);

            if (_vehicleController.GetCurrentCarStats().Accelerating)
                _orbitAngles.x = Mathf.Clamp(_orbitAngles.x - _orbitAngles.x * Time.deltaTime, _minVerticalAngle, _maxVerticalAngle);

            return true;
        }

        private void ConstrainAngles()
        {
            _orbitAngles.x =
                Mathf.Clamp(_orbitAngles.x, _minVerticalAngle, _maxVerticalAngle);

            if (_orbitAngles.y < 0f)
            {
                _orbitAngles.y += 360f;
            }
            else if (_orbitAngles.y >= 360f)
            {
                _orbitAngles.y -= 360f;
            }
        }

        private float GetAngle(Vector2 direction)
        {
            float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
            return direction.x < 0f ? 360f - angle : angle;
        }
    }
}
