namespace Assets.VehicleController
{
    public interface IEngine
    {
        public void Initialize(CurrentCarStats currentCarStats, VehicleStats stats, IShifter shifter, ITransmission transmission);
        public void Accelerate(WheelController[] driveWheelsArray, float gasInput, float breakInput, float rpm);
        public float CalculateAccelerationForce(float input, float rpm, float boost);
        public float GetCurrentTorque();
        public float GetForcedInductionBoostPercent();
    }
}
