using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    public class AntiRollBar : MonoBehaviour
    {
        //[SerializeField]
        //private Rigidbody _rb;



        //[SerializeField]
        //private float AntiRollStrength;

        //public void HandleSuspension(int suspensionSimulationPrecision)
        //{
        //    float leftTravel = 1;
        //    float rightTravel = 1;

        //    _leftSuspension.CalculateSpringForceAndHitPoint(suspensionSimulationPrecision);
        //    if (_leftSuspension.HitInfo.Hit)
        //    {
        //        leftTravel = _leftSuspension.HitInfo.Distance / _leftSuspension.SpringRestLength;
        //        ApplySuspension(_leftSuspension.GetSuspForce(), _leftSuspension.HitInfo.HitNormal, _leftSuspension.HitInfo.Position);
        //    }

        //    _rightSuspension.CalculateSpringForceAndHitPoint(suspensionSimulationPrecision);
        //    if (_rightSuspension.HitInfo.Hit)
        //    {
        //        rightTravel = _rightSuspension.HitInfo.Distance / _rightSuspension.SpringRestLength;
        //        ApplySuspension(_rightSuspension.GetSuspForce(), _rightSuspension.HitInfo.HitNormal, _rightSuspension.HitInfo.Position);
        //    }

        //    if (_leftSuspension.HitInfo.Hit && _rightSuspension.HitInfo.Hit)
        //    {
        //        float antiRollForce = (leftTravel - rightTravel) * AntiRollStrength;

        //        ApplySuspension(-antiRollForce, _leftSuspension.HitInfo.HitNormal, _leftSuspension.HitInfo.Position);
        //        ApplySuspension(+antiRollForce, _rightSuspension.HitInfo.HitNormal, _rightSuspension.HitInfo.Position);
        //    }
        //}

        //private void ApplySuspension(float force, Vector3 normal, Vector3 pos)
        //{
        //    _rb.AddForceAtPosition(force * normal, pos);
        //}
    }
}
