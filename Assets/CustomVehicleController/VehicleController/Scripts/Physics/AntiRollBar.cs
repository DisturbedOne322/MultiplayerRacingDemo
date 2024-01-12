using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    public class AntiRollBar : MonoBehaviour
    {
        [SerializeField]
        private SuspensionController _leftSuspension;
        [SerializeField]
        private SuspensionController _rightSuspension;

        [SerializeField]
        private float AntiRollStrength;

        public float _leftTravel = 1;
        public float _rightTravel = 1;

        public float GetAntiRollForce(float suspDistance, float suspMaxDistance, SuspensionController suspensionController)
        {
            if (suspensionController == _leftSuspension)
                _leftTravel = suspDistance / suspMaxDistance;
            else
                _rightTravel = suspDistance / suspMaxDistance;

            float antiRollForce = (Mathf.Clamp01(_leftTravel) - Mathf.Clamp01(_rightTravel)) * AntiRollStrength;

            if (suspensionController == _leftSuspension)
                return -antiRollForce;
            else
                return antiRollForce;
        }
    }
}
