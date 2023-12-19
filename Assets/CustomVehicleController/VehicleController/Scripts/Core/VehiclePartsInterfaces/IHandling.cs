namespace Assets.VehicleController
{
    public interface IHandling
    {
        public void Initialize(WheelController[] wheelColliders);
        public void SteerWheels(float input, float steeringAngle, float steerSpeed);
    }
}
