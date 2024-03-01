
namespace Assets.VehicleController
{
    public interface IEngine
    {
        public void Initialize(CurrentCarStats currentCarStats, VehiclePartsSetWrapper partsPresetWrapper, IShifter shifter, ITransmission transmission);
        public void Accelerate(VehicleAxle[] driveAxleArray, float gasInput, float breakInput, bool nitroBoostInput, float rpm);
        public float GetCurrentTorque();
        public float GetForcedInductionBoostPercent();
        public void AddNitro(float amount);
    }
}
