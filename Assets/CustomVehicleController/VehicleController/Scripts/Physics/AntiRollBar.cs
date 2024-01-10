using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    public class AntiRollBar : MonoBehaviour
    {
        [SerializeField]
        private SuspensionController[] _suspensionArray;

        [SerializeField]
        private float AntiRoll;

        public float _leftTravel = 1;
        public float _rightTravel = 1;

        public float GetAntiRollForce(float suspDistance, float suspMaxDistance, SuspensionController suspensionController)
        {
            if (suspensionController == _suspensionArray[0])
                _leftTravel = suspDistance / suspMaxDistance;
            else
                _rightTravel = suspDistance / suspMaxDistance;

            float antiRollForce = (Mathf.Clamp01(_leftTravel) - Mathf.Clamp01(_rightTravel)) * AntiRoll;

            if (suspensionController == _suspensionArray[0])
                return -antiRollForce;
            else
                return antiRollForce;
        }
    }
}
