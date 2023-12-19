using UnityEngine;
using UnityEngine.VFX;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Visuals/Collision Effect")]
    public class CarVisualsCollisionEffects : MonoBehaviour
    {
        [SerializeField]
        private VisualEffectAsset _continuousSparksVFXAsset;
        [SerializeField]
        private VisualEffectAsset _burstSparksVFXAsset;
        [SerializeField]
        private float _burstSparkCooldown = 0.5f;
        private float _lastCollisionTime;
        [SerializeField]
        private Light _collisionLight;

        private Light _leftCollisionLight;
        private Light _rightCollisionLight;
        private const float SM_DAMP_TIME = 0.5f;
        private Vector3 _leftLightSmDampVelocity;
        private Vector3 _rightLightSmDampVelocity;

        private VisualEffect _burstSparks;

        private VisualEffect _leftSideSparks;
        private VisualEffect _rightSideSparks;

        [SerializeField]
        private CollisionHandler _collisionHandler;

        // Start is called before the first frame update
        void Start()
        {
            if (_collisionHandler == null)
            {
                Debug.LogWarning("You have Collision Effects, but Collision Handler is not assigned");
                return;
            }

            if (_continuousSparksVFXAsset == null)
            {
                Debug.LogWarning("You have Collision Effects, but Visual Effect Asset is not assigned");
                return;
            }

            if (_burstSparksVFXAsset == null)
            {
                Debug.LogWarning("You have Collision Effects, but Visual Effect Asset is not assigned");
                return;
            }


            GameObject parent = new ("Collision Effects");
            parent.transform.parent = transform.root;

            GameObject burstSparks = new ("Burst Sparks");
            burstSparks.transform.parent = parent.transform;
            _burstSparks = burstSparks.AddComponent<VisualEffect>();
            _burstSparks.visualEffectAsset = _burstSparksVFXAsset;
            _burstSparks.Stop();

            GameObject leftSideSparksGO = new ("Left Side Sparks");
            Vector3 scale = leftSideSparksGO.transform.localScale;
            scale.x *= -1;
            leftSideSparksGO.transform.localScale = scale;
            leftSideSparksGO.transform.parent = parent.transform;
            _leftSideSparks = leftSideSparksGO.AddComponent<VisualEffect>();
            _leftSideSparks.visualEffectAsset = _continuousSparksVFXAsset;
            _leftSideSparks.Stop();

            GameObject rightSideSparksGO = new ("Right Side Sparks");
            rightSideSparksGO.transform.parent = parent.transform;
            _rightSideSparks = rightSideSparksGO.AddComponent<VisualEffect>();
            _rightSideSparks.visualEffectAsset = _continuousSparksVFXAsset;
            _rightSideSparks.Stop();

            if (_collisionLight != null)
            {
                _collisionLight.enabled = false;
                _leftCollisionLight = Instantiate(_collisionLight);
                _leftCollisionLight.transform.parent = leftSideSparksGO.transform;
                _rightCollisionLight = Instantiate(_collisionLight);
                _rightCollisionLight.transform.parent = rightSideSparksGO.transform;
            }

            _collisionHandler.OnLeftSideCollisionStay += _collisionHandler_OnLeftSideCollisionStay;
            _collisionHandler.OnLeftSideCollisionExit += _collisionHandler_OnLeftSideCollisionExit;

            _collisionHandler.OnRightSideCollisionStay += _collisionHandler_OnRightSideCollisionStay;
            _collisionHandler.OnRightSideCollisionExit += _collisionHandler_OnRightSideCollisionExit;

            _collisionHandler.OnCollision += _collisionHandler_OnCollision;
        }

        private void _collisionHandler_OnCollision(Vector3 pos, int collAmount, float collSpeed)
        {
            if (Time.time < _lastCollisionTime + _burstSparkCooldown)
                return;

            _burstSparks.SetFloat("spawnAmount", collSpeed / 5f);
            _burstSparks.SetFloat("lifetimeMax", collSpeed / 120f + 0.1f);
            _burstSparks.SetFloat("spawnArea", 1 - 1f / collAmount);
            _burstSparks.SetFloat("scaleMultiplier", 1 - 1 / (collAmount * 4));
            _burstSparks.SetVector3("position", pos);
            _burstSparks.Play();

            _lastCollisionTime = Time.time;
        }

        private void _collisionHandler_OnLeftSideCollisionExit()
        {
            _leftSideSparks.Stop();
            _leftCollisionLight.enabled = false;
        }

        private void _collisionHandler_OnLeftSideCollisionStay(Vector3 pos)
        {
            _leftSideSparks.Play();
            _leftSideSparks.SetVector3("position", pos);

            if(Time.time + 0.1f > _lastCollisionTime)
            {
                _leftCollisionLight.transform.position = pos;
            }
            _leftCollisionLight.enabled = true;
            _leftCollisionLight.transform.position = Vector3.SmoothDamp(_leftCollisionLight.transform.position, 
                pos, ref _leftLightSmDampVelocity, SM_DAMP_TIME);
        }

        private void _collisionHandler_OnRightSideCollisionExit()
        {
            _rightSideSparks.Stop();
            _rightCollisionLight.enabled = false;
        }

        private void _collisionHandler_OnRightSideCollisionStay(Vector3 pos)
        {
            _rightSideSparks.Play();
            _rightSideSparks.SetVector3("position", pos);
            if (Time.time + 0.1f > _lastCollisionTime)
            {
                _rightCollisionLight.transform.position = pos;
            }
            _rightCollisionLight.enabled = true;
            _rightCollisionLight.transform.position = Vector3.SmoothDamp(_rightCollisionLight.transform.position, 
                pos, ref _rightLightSmDampVelocity, SM_DAMP_TIME);
        }

        private void OnDestroy()
        {
            _collisionHandler.OnLeftSideCollisionStay -= _collisionHandler_OnLeftSideCollisionStay;
            _collisionHandler.OnLeftSideCollisionExit -= _collisionHandler_OnLeftSideCollisionExit;

            _collisionHandler.OnRightSideCollisionStay -= _collisionHandler_OnRightSideCollisionStay;
            _collisionHandler.OnRightSideCollisionExit -= _collisionHandler_OnRightSideCollisionExit;

            _collisionHandler.OnCollision -= _collisionHandler_OnCollision;
        }
    }

}
