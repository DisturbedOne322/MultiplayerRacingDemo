using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    public class AntiRollBar : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody _rb;

        [SerializeField]
        private SuspensionController _leftSuspension;
        [SerializeField]
        private SuspensionController _rightSuspension;

        [SerializeField]
        private float AntiRollStrength;

        private void FixedUpdate()
        {
            float leftTravel = 1;
            float rightTravel = 1;

            _leftSuspension.CalculateSpringForceAndHitPoint(5);
            if(_leftSuspension.HitInfo.Hit)
            {
                leftTravel = _leftSuspension.HitInfo.Distance / _leftSuspension.SpringRestLength;
            }

            _rightSuspension.CalculateSpringForceAndHitPoint(5);
            if (_rightSuspension.HitInfo.Hit)
            {
                rightTravel = _rightSuspension.HitInfo.Distance / _rightSuspension.SpringRestLength;
            }

            if(_leftSuspension.HitInfo.Hit && _rightSuspension.HitInfo.Hit)
            {
                float antiRollForce = (leftTravel - rightTravel) * AntiRollStrength;

                ApplySuspension(_leftSuspension.GetSuspForce() - antiRollForce, _leftSuspension.HitInfo.HitNormal, _leftSuspension.HitInfo.Position);
                ApplySuspension(_rightSuspension.GetSuspForce() + antiRollForce, _rightSuspension.HitInfo.HitNormal, _rightSuspension.HitInfo.Position);
            }
        }

        private void ApplySuspension(float force, Vector3 normal, Vector3 pos)
        {
            _rb.AddForceAtPosition(force * normal, pos);
        }
    }
}
