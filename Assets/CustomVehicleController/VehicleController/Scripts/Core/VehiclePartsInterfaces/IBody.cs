using UnityEngine;

namespace Assets.VehicleController
{
    public interface IBody
    {
        public void Initialize(Rigidbody rb, VehicleStats bodyStats, CurrentCarStats currentCarStats, Transform transform, Transform centerOfMass);
        public void AddDownforce();
        public void AddCorneringForce();
        public void AutomaticFlipOverRecover(float timer);
        public void PerformAerialControls(float sensitivity, float pitchInput, float yawInput, float rollInput);
    }
}
