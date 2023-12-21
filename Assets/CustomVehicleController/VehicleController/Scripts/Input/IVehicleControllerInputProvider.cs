public interface IVehicleControllerInputProvider
{
    public float GetGasInput();

    public float GetBrakeInput();

    public bool GetHandbrakeInput();

    public float GetHorizontalInput();

    public bool GetGearUpInput();

    public bool GetGearDownInput();

    public float GetPitchInput();

    public float GetYawInput();

    public float GetRollInput();
}
