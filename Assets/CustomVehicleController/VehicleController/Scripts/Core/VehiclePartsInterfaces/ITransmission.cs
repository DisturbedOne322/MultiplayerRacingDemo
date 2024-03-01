using System;


namespace Assets.VehicleController
{
    public interface ITransmission
    {
        public event Action OnShifted;
        public void ShiftUpManually();
        public void ShiftDownManually();
        public void Initialize(VehiclePartsSetWrapper partsPresetWrapper, CurrentCarStats currentCarStats, IShifter shifter);
        public float EvaluateRPM(float gasInput, VehicleAxle[] axleArray);
        public void HandleGearChanges(TransmissionType transmissionType, VehicleAxle[] axleArray);
        public bool InShiftingCooldown();
        public bool Redlining();
        public void ShiftGear(int i);
        public float DetermineGasInput(float gasInput, float breakInput);
        public float DetermineBrakeInput(float gasInput, float breakInput);
    }
}
