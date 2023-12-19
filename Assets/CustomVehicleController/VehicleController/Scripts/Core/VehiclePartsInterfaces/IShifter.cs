namespace Assets.VehicleController
{
    public interface IShifter
    {
        public void Initialize(IClutch clutch, VehicleStats stats);
        public bool TryChangeGear(int i, float delay);
        public bool InNeutralGear();
        public bool InReverseGear();
        public bool CheckIsClutchEngaged();
        public void SetInNeutral();
        public int GetCurrentGearID();
        public int GetGearAmount();
    }
}
