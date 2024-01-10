using System;


namespace Assets.VehicleController
{
    public interface ITransmission
    {
        public event Action OnShifted;
        public void ShiftUpManually();
        public void ShiftDownManually();
        public void Initialize(VehicleStats stats, CurrentCarStats currentCarStats, IShifter shifter);
        public float EvaluateRPM(float gasInput, WheelController[] wheelControllers);
        public void HandleGearChanges(TransmissionType transmissionType, WheelController[] wheelControllers);
        public bool InShiftingCooldown();
        public bool Redlining();
        public void ShiftGear(int i);
        public float DetermineGasInput(float gasInput, float breakInput);
        public float DetermineBreakInput(float gasInput, float breakInput);
    }
}
