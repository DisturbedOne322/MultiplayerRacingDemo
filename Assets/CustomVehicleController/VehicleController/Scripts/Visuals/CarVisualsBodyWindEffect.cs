using UnityEngine;
using UnityEngine.VFX;

namespace Assets.VehicleController
{
    public class CarVisualsBodyWindEffect
    {
        private VisualEffect _bodySpeedEffect;

        private const string SPEED_FIELD_NAME = "Speed";
        private const string SIDE_VELOCITY_FIELD_NAME = "Velocity";

        private ParticleSystem _bodyWindPSInstance;

        private EffectTypeParameters _parameters;

        private Transform _transform;

        public CarVisualsBodyWindEffect(EffectTypeParameters parameters, Transform transform)
        {
            _parameters = parameters;
            _transform = transform;

            if (parameters.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                InitializeVFX();
            else
                InitializePS();

        }

        private void InitializeVFX()
        {
            if (_parameters.VFXAsset == null)
                return;

            GameObject parent = new("Body Speed Effect");
            parent.transform.parent = this._transform.root;
            parent.transform.localPosition = Vector3.zero;
            parent.transform.localRotation = Quaternion.identity;
            _bodySpeedEffect = parent.AddComponent<VisualEffect>();
            _bodySpeedEffect.visualEffectAsset = _parameters.VFXAsset;
        }

        private void InitializePS()
        {
            if (_parameters.ParticleSystem == null)
                return;

            GameObject parent = new("Body Speed Effect");
            parent.transform.parent = this._transform.root;
            parent.transform.localPosition = Vector3.zero;
            parent.transform.localRotation = Quaternion.identity;
            _bodyWindPSInstance = GameObject.Instantiate(_parameters.ParticleSystem, parent.transform);
        }

        public void HandleSpeedEffect(float speed, Vector3 rbVelocity)
        {
            if(_parameters.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                DisplayVFX(speed, rbVelocity);
            else
                DisplayPS(speed, rbVelocity);
        }

        private void DisplayVFX(float speed, Vector3 rbVelocity)
        {
            if (_parameters.VFXAsset == null)
            {
                Debug.LogWarning("You have Body Wind Effect, but Visual Effect Asset is not assigned");
                return;
            }

            _bodySpeedEffect.SetFloat(SPEED_FIELD_NAME, speed);
            _bodySpeedEffect.SetFloat(SIDE_VELOCITY_FIELD_NAME, _transform.InverseTransformDirection(rbVelocity).x);
        }

        private void DisplayPS(float speed, Vector3 rbVelocity)
        {
            _bodyWindPSInstance.transform.forward = rbVelocity.normalized;

            var main = _bodyWindPSInstance.main;
            var emission = _bodyWindPSInstance.emission;
            main.startLifetime = Mathf.Clamp(1 / (speed / 120), 0.2f, 1);
            emission.rateOverTime = Mathf.Clamp(speed, 0 , 100);
            main.startSpeed = -speed / 2;

            Color targetColor = Color.white;
            targetColor.a = Mathf.Clamp(1 / (150 / (speed + 1)), 0.1f, 0.5f);
            main.startColor = targetColor;
        }
    }

}
