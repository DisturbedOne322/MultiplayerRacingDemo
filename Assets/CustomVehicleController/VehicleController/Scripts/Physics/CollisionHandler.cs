using System;
using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Physics/Collision Handler")]
    public class CollisionHandler : MonoBehaviour
    {
        public event Action<Vector3, Vector3, float> OnCollisionImpact;

        public event Action<Vector3, float> OnLeftSideCollisionStay;
        public event Action OnLeftSideCollisionExit;

        public event Action<Vector3, float> OnRightSideCollisionStay;
        public event Action OnRightSideCollisionExit;

        [SerializeField]
        private float _minCollisionMagnitude;

        [SerializeField] 
        private CustomVehicleController _vehicleController;

        public BoxCollider _boxCollider;
        private Collider[] _colliders;

        private Vector3 _colliderHalfExtent;

        private void Awake()
        {
            _colliders = new Collider[6];
            _colliderHalfExtent = _boxCollider.bounds.size / 1.99f;
        }

        private void Update()
        {
            int leftColls = 0;
            int rightColls = 0;

            for (int i = 0; i < Physics.OverlapBoxNonAlloc(_boxCollider.bounds.center, _colliderHalfExtent, _colliders, transform.rotation); i++)
            {
                if (_colliders[i].transform == _boxCollider.transform)
                    continue;

                float magnitude = Mathf.Abs(_vehicleController.GetCurrentCarStats().SpeedInMsPerS * 2);
                if (magnitude < _minCollisionMagnitude)
                {
                    continue;
                }

                Vector3 closest = _colliders[i].ClosestPoint(_boxCollider.bounds.center);
                Vector3 direction = closest - _boxCollider.bounds.center;

                bool right = Vector3.Dot(transform.right, direction) > 0;

                if (right)
                {
                    OnRightSideCollisionStay?.Invoke(_colliders[i].ClosestPoint(_boxCollider.bounds.center), magnitude);
                    rightColls++;   
                }
                else
                {
                    OnLeftSideCollisionStay?.Invoke(_colliders[i].ClosestPoint(_boxCollider.bounds.center), magnitude);
                    leftColls++;
                }
            }

            if(leftColls == 0)
                OnLeftSideCollisionExit?.Invoke();

            if(rightColls == 0) 
                OnRightSideCollisionExit?.Invoke();
        }

        private void OnCollisionEnter(Collision collision)
        {
            float magnitude = collision.relativeVelocity.magnitude;
            if (magnitude < _minCollisionMagnitude)
                return;

            OnCollisionImpact?.Invoke(collision.GetContact(0).point, collision.GetContact(0).normal, magnitude);
        }

        private void OnCollisionStay(Collision collision)
        {
            //float magnitude = collision.relativeVelocity.magnitude;
            //OnCollisionContinuous?.Invoke(magnitude);

            //if (magnitude < _minCollisionMagnitude)
            //{
            //    OnLeftSideCollisionExit?.Invoke();
            //    OnRightSideCollisionExit?.Invoke();
            //    return;
            //}

            //Vector3 averageLeftHitPosition = Vector3.zero;
            //int leftColls = 0;
            //Vector3 averageRightHitPosition = Vector3.zero;
            //int rightColls = 0;

            //foreach (var contact in collision.contacts)
            //{
            //    if (Vector3.Dot(contact.normal, transform.up) > 0.5f)
            //    {
            //        OnBottomCollision?.Invoke(contact.point);
            //    }
            //    else if (Vector3.Dot(contact.normal, -transform.up) > 0.5f)
            //    {
            //        OnTopCollision?.Invoke(contact.point);
            //    }
            //    Vector3 cross = Vector3.Cross(transform.forward, contact.normal);
            //    if (Mathf.Abs(cross.y) < 0.25f)
            //        continue;

            //    //left collision
            //    if (cross.y > 0)
            //    {
            //        averageLeftHitPosition += contact.point;
            //        leftColls++;
            //    }
            //    //right collision
            //    else
            //    {
            //        averageRightHitPosition += contact.point;
            //        rightColls++;
            //    }
            //}

            //if (leftColls > 0)
            //{
            //    averageLeftHitPosition /= leftColls;
            //    OnLeftSideCollisionStay?.Invoke(averageLeftHitPosition, magnitude);
            //}
            //else
            //    OnLeftSideCollisionExit?.Invoke();

            //if (rightColls > 0)
            //{
            //    averageRightHitPosition /= rightColls;
            //    OnRightSideCollisionStay?.Invoke(averageRightHitPosition, magnitude);
            //}
            //else
            //    OnRightSideCollisionExit?.Invoke();
        }

        //private void OnCollisionExit(Collision collision)
        //{
        //    OnLeftSideCollisionExit?.Invoke();
        //    OnRightSideCollisionExit?.Invoke();
        //}
    }
}

