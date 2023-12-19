using System;
using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Physics/Collision Handler")]
    public class CollisionHandler : MonoBehaviour
    {
        public event Action<Vector3, int, float> OnCollision;
        public event Action<Vector3> OnRightSideCollisionStay;
        public event Action OnRightSideCollisionExit;
        public event Action<Vector3> OnLeftSideCollisionStay;
        public event Action OnLeftSideCollisionExit;
        public event Action<Vector3> OnBottomCollision;
        public event Action<Vector3> OnTopCollision;

        [SerializeField]
        private float _minCollisionMagnitude;

        private void OnCollisionEnter(Collision collision)
        {
            float magnitude = collision.relativeVelocity.magnitude;
            if (magnitude < _minCollisionMagnitude)
                return;

            OnCollision?.Invoke(collision.GetContact(0).point, collision.contactCount, magnitude);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.relativeVelocity.sqrMagnitude < _minCollisionMagnitude * _minCollisionMagnitude)
            {
                OnLeftSideCollisionExit?.Invoke();
                OnRightSideCollisionExit?.Invoke();
                return;
            }

            Vector3 averageLeftHitPosition = Vector3.zero;
            int leftColls = 0;
            Vector3 averageRightHitPosition = Vector3.zero;
            int rightColls = 0;

            foreach (var contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, transform.up) > 0.5f)
                {
                    OnBottomCollision?.Invoke(contact.point);
                }
                else if (Vector3.Dot(contact.normal, -transform.up) > 0.5f)
                {
                    OnTopCollision?.Invoke(contact.point);
                }
                Vector3 cross = Vector3.Cross(transform.forward, contact.normal);
                if (Mathf.Abs(cross.y) < 0.25f)
                    continue;

                //left collision
                if (cross.y > 0)
                {
                    averageLeftHitPosition += contact.point;
                    leftColls++;
                }
                //right collision
                else
                {
                    averageRightHitPosition += contact.point;
                    rightColls++;
                }
            }

            if (leftColls > 0)
            {
                averageLeftHitPosition /= leftColls;
                OnLeftSideCollisionStay?.Invoke(averageLeftHitPosition);
            }
            else
                OnLeftSideCollisionExit?.Invoke();

            if (rightColls > 0)
            {
                averageRightHitPosition /= rightColls;
                OnRightSideCollisionStay?.Invoke(averageRightHitPosition);
            }
            else
                OnRightSideCollisionExit?.Invoke();
        }

        private void OnCollisionExit(Collision collision)
        {
            OnLeftSideCollisionExit?.Invoke();
            OnRightSideCollisionExit?.Invoke();
        }
    }
}

