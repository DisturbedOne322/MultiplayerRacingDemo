using UnityEngine;

namespace Assets.VehicleController
{
    public interface IBody
    {
        public void Initialize(Rigidbody rb, VehiclePartsSetWrapper vehiclePartsPresetWrapper, CurrentCarStats currentCarStats, Transform transform, Transform centerOfMass);
        public void AddDownforce();
        public void AddCorneringForce();
        public void PerformAerialControls(float sensitivity, float pitchInput, float yawInput, float rollInput);
    }
}
