using System.Threading.Tasks;
using UnityEngine;

namespace Assets.VehicleController
{
    public class Shifter : IShifter
    {
        private int _currentGear = 0;

        private VehicleStats _stats;

        private ShifterStates.ShifterState _shifterState;

        public bool CheckIsClutchEngaged() => false;

        public int GetCurrentGearID() => _currentGear;

        public int GetGearAmount() => _stats.TransmissionSO.GearRatiosList.Count;

        public void Initialize(IClutch clutch, VehicleStats stats)
        {
            _stats = stats;
        }

        public bool InNeutralGear() => _shifterState == ShifterStates.ShifterState.Neutral;

        public bool InReverseGear() => _shifterState == ShifterStates.ShifterState.Reverse;

        public void SetInNeutral() => _shifterState = ShifterStates.ShifterState.Neutral;
        public bool TryChangeGear(int i, float delay)
        {
            (bool success, int newGearID, ShifterStates.ShifterState newState) = WrapGear(_currentGear + i);
            if (success)
            {
                SetInNeutral();
                Task.Delay((int)(delay * 1000)).ContinueWith(t => DelayGearSwitch(newGearID, newState));
            }
            return success;
        }

        private void DelayGearSwitch(int newGearID, ShifterStates.ShifterState newState)
        {
            _currentGear = newGearID;
            _shifterState = newState;
        }

        private (bool, int, ShifterStates.ShifterState) WrapGear(int newGearID)
        {
            //trying to downshift from the lowest gear
            if (newGearID < 0)
            {
                //already in reverse
                if (_shifterState == ShifterStates.ShifterState.Reverse)
                    return (false, 0, _shifterState);

                //get into reverse
                return (true, 0, ShifterStates.ShifterState.Reverse);
            }

            //going from reverse into first gear (id 0)
            if (_shifterState == ShifterStates.ShifterState.Reverse)
                return (true, 0, ShifterStates.ShifterState.Drive);

            if (newGearID >= _stats.TransmissionSO.GearRatiosList.Count)
                return (false, _stats.TransmissionSO.GearRatiosList.Count - 1, ShifterStates.ShifterState.Drive);

            return (true, newGearID, ShifterStates.ShifterState.Drive);
        }
    }
}
