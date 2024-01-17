using UnityEngine;
using UnityEngine.VFX;

namespace Assets.VehicleController
{
    public class CarVisualsCollisionEffects
    {
        private CollisionEffectParameters _parameters;
        private float _lastCollisionTime;
        private Transform _transform;

        private Light _leftCollisionLight;
        private Light _rightCollisionLight;

        private VisualEffect _burstSparksVFX;
        private ParticleSystem _burstSparksPS;

        private VisualEffect _leftSideSparksVFX;
        private VisualEffect _rightSideSparksVFX;
        private ParticleSystem _leftSideSparksPS;
        private ParticleSystem _rightSideSparksPS;

        private GameObject _parentGO;
        private GameObject _burstSparkGO;
        private GameObject _leftContinousSparksGO;
        private GameObject _rightContinousSparksGO;

        public CarVisualsCollisionEffects(CollisionEffectParameters parameters, Transform transform)
        {
            _parameters = parameters;
            _transform = transform;

            _parentGO = new("Collision Effects");
            _parentGO.transform.parent = transform.root;
            _parentGO.transform.localPosition = Vector3.zero;

            _burstSparkGO = new("Burst Sparks");
            _burstSparkGO.transform.parent = _parentGO.transform;
            _burstSparkGO.transform.localPosition = Vector3.zero;

            _leftContinousSparksGO = new("Left Side Sparks");
            Vector3 scale = _leftContinousSparksGO.transform.localScale;
            scale.x *= -1;
            _leftContinousSparksGO.transform.localScale = scale;
            _leftContinousSparksGO.transform.parent = _parentGO.transform;
            _leftContinousSparksGO.transform.localPosition = Vector3.zero;

            _rightContinousSparksGO = new("Right Side Sparks");
            _rightContinousSparksGO.transform.parent = _parentGO.transform;
            _rightContinousSparksGO.transform.localPosition = Vector3.zero;

            if (_parameters.CollisionHandler == null)
            {
                Debug.LogWarning("You have Collision Effects, but Collision Handler is not assigned");
                return;
            }

            if(_parameters.BurstVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
            {
                if (_parameters.BurstVisualEffect.VFXAsset == null)
                {
                    Debug.LogWarning("You have Collision Effects, but Visual Effect Asset is not assigned");
                    return;
                }

                InstantiateBurstVFX();
            }
            else
            {
                if (_parameters.BurstVisualEffect.ParticleSystem == null)
                {
                    Debug.LogWarning("You have Collision Effects, but ParticleSystem is not assigned");
                    return;
                }

                InstantiateBurstPS();
            }

            if(_parameters.ContinousVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
            {
                if (_parameters.ContinousVisualEffect.VFXAsset == null)
                {
                    Debug.LogWarning("You have Collision Effects, but Visual Effect Asset is not assigned");
                    return;
                }

                InstantiateContVFX();
            }
            else
            {
                if (_parameters.ContinousVisualEffect.ParticleSystem == null)
                {
                    Debug.LogWarning("You have Collision Effects, but ParticleSystem is not assigned");
                    return;
                }

                InstantiateContPS();
            }


            if (_parameters.CollisionLight != null)
            {
                _parameters.CollisionLight.enabled = true;
                _leftCollisionLight = GameObject.Instantiate(_parameters.CollisionLight);
                _leftCollisionLight.gameObject.SetActive(false);
                _leftCollisionLight.transform.parent = _leftContinousSparksGO.transform;
                _rightCollisionLight = GameObject.Instantiate(_parameters.CollisionLight);
                _rightCollisionLight.gameObject.SetActive(false);
                _rightCollisionLight.transform.parent = _rightContinousSparksGO.transform;
            }

            _parameters.CollisionHandler.OnLeftSideCollisionStay += _collisionHandler_OnLeftSideCollisionStay;
            _parameters.CollisionHandler.OnLeftSideCollisionExit += CollisionHandler_OnLeftSideCollisionExit;
            _parameters.CollisionHandler.OnRightSideCollisionStay += _collisionHandler_OnRightSideCollisionStay;
            _parameters.CollisionHandler.OnRightSideCollisionExit += CollisionHandler_OnRightSideCollisionExit;
            _parameters.CollisionHandler.OnCollisionImpact += _collisionHandler_OnCollision;
        }

        private void InstantiateContVFX()
        {
            _leftSideSparksVFX = _leftContinousSparksGO.AddComponent<VisualEffect>();
            _leftSideSparksVFX.visualEffectAsset = _parameters.ContinousVisualEffect.VFXAsset;
            _leftSideSparksVFX.Stop();


            _rightSideSparksVFX = _rightContinousSparksGO.AddComponent<VisualEffect>();
            _rightSideSparksVFX.visualEffectAsset = _parameters.ContinousVisualEffect.VFXAsset;
            _rightSideSparksVFX.Stop();
        }
        private void InstantiateBurstVFX()
        {
            _burstSparksVFX = _burstSparkGO.AddComponent<VisualEffect>();
            _burstSparksVFX.visualEffectAsset = _parameters.BurstVisualEffect.VFXAsset;
            _burstSparksVFX.Stop();
        }
        private void InstantiateContPS()
        {
            _leftSideSparksPS = GameObject.Instantiate(_parameters.ContinousVisualEffect.ParticleSystem);
            _leftSideSparksPS.Stop();
            _leftSideSparksPS.transform.parent = _leftContinousSparksGO.transform;
            _leftSideSparksPS.transform.localPosition = new(0, 0, 0);
            _leftSideSparksPS.transform.localRotation = Quaternion.Euler(0, 0, 0);

            _rightSideSparksPS = GameObject.Instantiate(_parameters.ContinousVisualEffect.ParticleSystem);
            _rightSideSparksPS.Stop();
            _rightSideSparksPS.transform.parent = _rightContinousSparksGO.transform;
            _rightSideSparksPS.transform.localPosition = new(0, 0, 0);
            _rightSideSparksPS.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        private void InstantiateBurstPS()
        {
            _burstSparksPS = GameObject.Instantiate(_parameters.BurstVisualEffect.ParticleSystem);
            _burstSparksPS.Stop();
            _burstSparksPS.transform.parent = _burstSparkGO.transform;
            _burstSparksPS.transform.localPosition = new(0, 0, 0);
            _burstSparksPS.transform.localRotation = Quaternion.Euler(0, 0, 0);            
        }

        private void CollisionHandler_OnRightSideCollisionExit()
        {
            if (_parameters.ContinousVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                _rightSideSparksVFX.Stop();
            else
                _rightSideSparksPS.Stop();

            if(_rightCollisionLight != null)
                _rightCollisionLight.gameObject.SetActive(false);
        }

        private void CollisionHandler_OnLeftSideCollisionExit()
        {
            if (_parameters.ContinousVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                _leftSideSparksVFX.Stop();
            else
                _leftSideSparksPS.Stop();

            if (_leftCollisionLight != null)
                _leftCollisionLight.gameObject.SetActive(false);
        }

        private void _collisionHandler_OnCollision(Vector3 pos, Vector3 normal, float collSpeed)
        {
            if (Time.time < _lastCollisionTime + _parameters.BurstSparkCooldown)
                return;

            if(_parameters.BurstVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
            {
                _burstSparksVFX.SetFloat("spawnAmount", collSpeed / 5f);
                _burstSparksVFX.SetFloat("lifetimeMax", collSpeed / 120f + 0.1f);
                _burstSparksVFX.SetFloat("spawnArea", Mathf.Clamp01(collSpeed / 120));
                _burstSparksVFX.SetFloat("scaleMultiplier", Mathf.Clamp01(collSpeed / 120));
                _burstSparksVFX.SetVector3("position", pos);
                _burstSparksVFX.transform.forward = -normal;
                _burstSparksVFX.Play();
            }
            else
            {
                _burstSparksPS.transform.position = pos;
                _burstSparksPS.transform.forward = -normal;
                ParticleSystem.EmissionModule emissionModule = _burstSparksPS.emission;
                emissionModule.SetBurst(0, new ParticleSystem.Burst(0, collSpeed / 5));
                _burstSparksPS.Play();
            }



            _lastCollisionTime = Time.time;
        }

        private void _collisionHandler_OnLeftSideCollisionStay(Vector3 pos, float collMag)
        {
            if(_parameters.ContinousVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
            {
                _leftSideSparksVFX.Play();
                _leftSideSparksVFX.SetVector3("position", pos);
                _leftSideSparksVFX.SetFloat("horizontalOffset", _parameters.HorizontalOffset);
                _leftSideSparksVFX.SetFloat("spawnAmount", Mathf.Clamp(collMag, 0, 100));
                _leftSideSparksVFX.SetFloat("spawnArea", _parameters.SparksSpawnAreaHeight);
            }
            else
            {
                _leftSideSparksPS.transform.position = pos - _transform.right * _parameters.HorizontalOffset;
                _leftSideSparksPS.Play();
            }



            if (_leftCollisionLight == null)
                return;
            _leftCollisionLight.gameObject.SetActive(true);
            _leftCollisionLight.transform.position = pos;
        }

        private void _collisionHandler_OnRightSideCollisionStay(Vector3 pos, float collMag)
        {
            if(_parameters.ContinousVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
            {
                _rightSideSparksVFX.Play();
                _rightSideSparksVFX.SetVector3("position", pos);
                _rightSideSparksVFX.SetFloat("horizontalOffset", _parameters.HorizontalOffset);
                _rightSideSparksVFX.SetFloat("spawnAmount", Mathf.Clamp(collMag, 0, 100));
                _rightSideSparksVFX.SetFloat("spawnArea", _parameters.SparksSpawnAreaHeight);
            }
            else
            {
                _rightSideSparksPS.transform.position = pos + _transform.right * _parameters.HorizontalOffset;
                _rightSideSparksPS.Play();
            }

            if (_rightCollisionLight == null)
                return;

            _rightCollisionLight.gameObject.SetActive(true);
            _rightCollisionLight.transform.position = pos;
        }

        public void OnDestroy()
        {
            if (_parameters.CollisionHandler == null)
                return;

            _parameters.CollisionHandler.OnLeftSideCollisionStay -= _collisionHandler_OnLeftSideCollisionStay;
            _parameters.CollisionHandler.OnLeftSideCollisionExit -= CollisionHandler_OnLeftSideCollisionExit;
            _parameters.CollisionHandler.OnRightSideCollisionStay -= _collisionHandler_OnRightSideCollisionStay;
            _parameters.CollisionHandler.OnRightSideCollisionExit -= CollisionHandler_OnRightSideCollisionExit;
            _parameters.CollisionHandler.OnCollisionImpact -= _collisionHandler_OnCollision;
        }
    }

}
