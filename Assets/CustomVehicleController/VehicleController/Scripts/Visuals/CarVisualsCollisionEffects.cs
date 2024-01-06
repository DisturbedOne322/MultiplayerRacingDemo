using UnityEngine;
using UnityEngine.VFX;

namespace Assets.VehicleController
{
    public class CarVisualsCollisionEffects
    {
        private CollisionEffectParameters _parameters;
        private float _lastCollisionTime;

        private Light _leftCollisionLight;
        private Light _rightCollisionLight;

        private VisualEffect _burstSparks;

        private VisualEffect _leftSideSparks;
        private VisualEffect _rightSideSparks;

        public CarVisualsCollisionEffects(CollisionEffectParameters parameters, Transform transform)
        {
            _parameters = parameters;

            if (_parameters.CollisionHandler == null)
            {
                Debug.LogWarning("You have Collision Effects, but Collision Handler is not assigned");
                return;
            }

            if (_parameters.ContinuousSparksVFXAsset == null)
            {
                Debug.LogWarning("You have Collision Effects, but Visual Effect Asset is not assigned");
                return;
            }

            if (_parameters.BurstSparksVFXAsset == null)
            {
                Debug.LogWarning("You have Collision Effects, but Visual Effect Asset is not assigned");
                return;
            }


            GameObject parent = new("Collision Effects");
            parent.transform.parent = transform.root;

            GameObject burstSparks = new("Burst Sparks");
            burstSparks.transform.parent = parent.transform;
            _burstSparks = burstSparks.AddComponent<VisualEffect>();
            _burstSparks.visualEffectAsset = _parameters.BurstSparksVFXAsset;
            _burstSparks.Stop();

            GameObject leftSideSparksGO = new("Left Side Sparks");
            Vector3 scale = leftSideSparksGO.transform.localScale;
            scale.x *= -1;
            leftSideSparksGO.transform.localScale = scale;
            leftSideSparksGO.transform.parent = parent.transform;
            _leftSideSparks = leftSideSparksGO.AddComponent<VisualEffect>();
            _leftSideSparks.visualEffectAsset = _parameters.ContinuousSparksVFXAsset;
            _leftSideSparks.Stop();

            GameObject rightSideSparksGO = new("Right Side Sparks");
            rightSideSparksGO.transform.parent = parent.transform;
            _rightSideSparks = rightSideSparksGO.AddComponent<VisualEffect>();
            _rightSideSparks.visualEffectAsset = _parameters.ContinuousSparksVFXAsset;
            _rightSideSparks.Stop();

            if (_parameters.CollisionLight != null)
            {
                _parameters.CollisionLight.enabled = false;
                _leftCollisionLight = GameObject.Instantiate(_parameters.CollisionLight);
                _leftCollisionLight.transform.parent = leftSideSparksGO.transform;
                _rightCollisionLight = GameObject.Instantiate(_parameters.CollisionLight);
                _rightCollisionLight.transform.parent = rightSideSparksGO.transform;
            }

            _parameters.CollisionHandler.OnLeftSideCollisionStay += _collisionHandler_OnLeftSideCollisionStay;
            _parameters.CollisionHandler.OnLeftSideCollisionExit += CollisionHandler_OnLeftSideCollisionExit;
            _parameters.CollisionHandler.OnRightSideCollisionStay += _collisionHandler_OnRightSideCollisionStay;
            _parameters.CollisionHandler.OnRightSideCollisionExit += CollisionHandler_OnRightSideCollisionExit;
            _parameters.CollisionHandler.OnCollisionImpact += _collisionHandler_OnCollision;
        }

        private void CollisionHandler_OnRightSideCollisionExit()
        {

            _rightSideSparks.Stop();
            _rightCollisionLight.enabled = false;
        }

        private void CollisionHandler_OnLeftSideCollisionExit()
        {
            _leftSideSparks.Stop();
            _leftCollisionLight.enabled = false;
        }

        private void _collisionHandler_OnCollision(Vector3 pos, float collSpeed)
        {
            if (Time.time < _lastCollisionTime + _parameters.BurstSparkCooldown)
                return;

            _burstSparks.SetFloat("spawnAmount", collSpeed / 5f);
            _burstSparks.SetFloat("lifetimeMax", collSpeed / 120f + 0.1f);
            _burstSparks.SetFloat("spawnArea", Mathf.Clamp01(collSpeed / 120));
            _burstSparks.SetFloat("scaleMultiplier", Mathf.Clamp01(collSpeed / 120));
            _burstSparks.SetVector3("position", pos);
            _burstSparks.Play();

            _lastCollisionTime = Time.time;
        }

        private void _collisionHandler_OnLeftSideCollisionStay(Vector3 pos, float collMag)
        {
            _leftSideSparks.Play();
            _leftSideSparks.SetVector3("position", pos);
            _leftSideSparks.SetFloat("horizontalOffset", _parameters.HorizontalOffset);
            _leftSideSparks.SetFloat("spawnAmount", Mathf.Clamp(collMag, 0, 100));
            _leftSideSparks.SetFloat("spawnArea", _parameters.SparksSpawnAreaHeight);

            if (Time.time + 0.1f > _lastCollisionTime)
            {
                _leftCollisionLight.transform.position = pos;
            }
            _leftCollisionLight.enabled = true;
            _leftCollisionLight.transform.position = pos;
        }

        private void _collisionHandler_OnRightSideCollisionStay(Vector3 pos, float collMag)
        {
            _rightSideSparks.Play();
            _rightSideSparks.SetVector3("position", pos);
            _rightSideSparks.SetFloat("horizontalOffset", _parameters.HorizontalOffset);
            _rightSideSparks.SetFloat("spawnAmount", Mathf.Clamp(collMag, 0, 100));
            _rightSideSparks.SetFloat("spawnArea", _parameters.SparksSpawnAreaHeight);

            if (Time.time + 0.1f > _lastCollisionTime)
            {
                _rightCollisionLight.transform.position = pos;
            }
            _rightCollisionLight.enabled = true;
            _rightCollisionLight.transform.position = pos;
        }

        public void OnDestroy()
        {
            _parameters.CollisionHandler.OnLeftSideCollisionStay -= _collisionHandler_OnLeftSideCollisionStay;
            _parameters.CollisionHandler.OnLeftSideCollisionExit -= CollisionHandler_OnLeftSideCollisionExit;
            _parameters.CollisionHandler.OnRightSideCollisionStay -= _collisionHandler_OnRightSideCollisionStay;
            _parameters.CollisionHandler.OnRightSideCollisionExit -= CollisionHandler_OnRightSideCollisionExit;
            _parameters.CollisionHandler.OnCollisionImpact -= _collisionHandler_OnCollision;
        }
    }

}
