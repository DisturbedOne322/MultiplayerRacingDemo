using UnityEngine;
using UnityEngine.VFX;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Visuals/Body Aero Effect")]
    public class CarVisualsBodyWindEffect : MonoBehaviour
    {
        [SerializeField]
        private VisualEffectAssetType.Type _bodyWindVisualEffectType;
        #region VFX
        [SerializeField]
        private VisualEffectAsset _bodyWindVisualEffectAsset;
        private VisualEffect _bodySpeedEffect;

        private const string SPEED_FIELD_NAME = "Speed";
        private const string SIDE_VELOCITY_FIELD_NAME = "Velocity";
        #endregion

        #region PS
        [SerializeField]
        private ParticleSystem _bodyWindParticleSystem;
        private ParticleSystem _bodyWindPSInstance;
        #endregion

        private void Awake()
        {
            if (_bodyWindVisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                InitializeVFX();
            else
                InitializePS();
        }


        public void HandleSpeedEffect(float speed, Vector3 rbVelocity)
        {
            if(_bodyWindVisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                DisplayVFX(speed, rbVelocity);
            else
                DisplayPS(speed, rbVelocity);
        }

        private void InitializeVFX()
        {
            GameObject parent = new("Body Speed Effect");
            parent.transform.parent = this.transform.root;
            parent.transform.localPosition = Vector3.zero;
            parent.transform.localRotation = Quaternion.identity;
            _bodySpeedEffect = parent.AddComponent<VisualEffect>();
            _bodySpeedEffect.visualEffectAsset = _bodyWindVisualEffectAsset;
        }

        private void InitializePS()
        {
            GameObject parent = new("Body Speed Effect");
            parent.transform.parent = this.transform.root;
            parent.transform.localPosition = Vector3.zero;
            parent.transform.localRotation = Quaternion.identity;
            _bodyWindPSInstance = Instantiate(_bodyWindParticleSystem, parent.transform);
        }

        private void DisplayVFX(float speed, Vector3 rbVelocity)
        {
            if (_bodyWindVisualEffectAsset == null)
            {
                Debug.LogWarning("You have Body Wind Effect, but Visual Effect Asset is not assigned");
                return;
            }

            _bodySpeedEffect.SetFloat(SPEED_FIELD_NAME, speed);
            _bodySpeedEffect.SetFloat(SIDE_VELOCITY_FIELD_NAME, transform.InverseTransformDirection(rbVelocity).x);
        }

        private void DisplayPS(float speed, Vector3 rbVelocity)
        {
            _bodyWindPSInstance.transform.forward = rbVelocity.normalized;
            _bodyWindPSInstance.startLifetime = Mathf.Clamp(1 / (speed / 120), 0.2f, 1);
            _bodyWindPSInstance.emissionRate = Mathf.Clamp(speed, 0 , 100);
            _bodyWindPSInstance.startSpeed = -speed / 2;

            Color targetColor = Color.white;
            targetColor.a = Mathf.Clamp(1 / (150 / (speed + 1)), 0.1f, 0.5f);
            _bodyWindPSInstance.startColor = targetColor;
        }
    }

}
