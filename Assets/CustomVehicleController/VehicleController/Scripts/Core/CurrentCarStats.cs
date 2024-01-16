using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    public class CurrentCarStats : ScriptableObject
    {
        public List<GameObject> ScriptableObjectOwners;
        public void Reset()
        {
            SpeedInMsPerS = 0;
            SpeedPercent = 0;
            EngineRPM = 0;
            EngineRPMPercent = 0;
            ForcedInductionBoostPercent = 0;
            CurrentGear = "N";
            Accelerating = false;
            Braking = false;
            NitroBoosting = false;
            NitroPercentLeft = 1f;
            AccelerationForce = 0;
            SidewaysForce = 0;
            Reversing = false;
            InAir = false;
            FlipperOver = false;
            DriveWheelLostContact = false;
            DriftAngle = 0;
            DriftTime = 0;
            AirTime = 0;
            HandbrakePulled = false;
            IsCarSlipping = false;
            ScriptableObjectOwners.Clear();
        }

        public event Action OnAntiLag;
        public void AntiLagHappened() => OnAntiLag?.Invoke();
        public event Action OnShiftedAntiLag;
        public void ShiftedAntiLagHappened() => OnShiftedAntiLag?.Invoke();

        public float SpeedInMsPerS;
        public float SpeedInKMperH
        {
            get { return SpeedInMsPerS * 3.6f; }
        }
        public float SpeedInMilesPerH
        {
            get { return SpeedInMsPerS * 2.23693629f; }
        }
        public float SpeedPercent;
        public string CurrentGear;
        public float EngineRPM;
        public float EngineRPMPercent;
        public float ForcedInductionBoostPercent;
        public float CurrentEngineTorque;
        public float CurrentEngineHorsepower
        {
            get => CurrentEngineTorque * EngineRPM / 5252;
        }
        public bool Accelerating;
        public bool Braking;
        public bool NitroBoosting;
        public float NitroPercentLeft;
        public float AccelerationForce;
        public float SidewaysForce;
        public bool Reversing;
        public bool InAir;
        public bool FlipperOver;
        public bool DriveWheelLostContact;
        public float DriftAngle;
        public float DriftTime;
        public float AirTime;
        public bool HandbrakePulled;
        public bool IsCarSlipping;
        public bool[] WheelSlipArray;
    }
}
