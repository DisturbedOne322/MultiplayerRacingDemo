namespace Assets.VehicleController
{
    public interface IEngine
    {
        public void Initialize(CurrentCarStats currentCarStats, VehicleStats stats, IShifter shifter, ITransmission transmission);
        public void Accelerate(VehicleAxle[] driveAxleArray, float gasInput, float breakInput, bool nitroBoostInput, float rpm);
        public float CalculateAccelerationForce(float input, float rpm, float boost);
        public float GetCurrentTorque();
        public float GetForcedInductionBoostPercent();
    }
}
