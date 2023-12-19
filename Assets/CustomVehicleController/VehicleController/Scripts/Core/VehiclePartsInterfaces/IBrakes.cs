using UnityEngine;

namespace Assets.VehicleController
{
    public interface IBrakes
    {
        public void Initialize(VehicleStats stats, WheelController[] wheelContollers,
            WheelController[] rearWheelControllers, CurrentCarStats currentCarStats, Rigidbody rb, ITransmission transmission);
        public void Break(float gasInput, float breakInput, bool handbrakePulled);
    }
}
