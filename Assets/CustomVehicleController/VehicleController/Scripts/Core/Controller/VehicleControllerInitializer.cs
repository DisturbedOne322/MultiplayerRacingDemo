using UnityEngine;

namespace Assets.VehicleController
{
    public class VehicleControllerInitializer
    {
        private IBody _body;
        private IEngine _engine;
        private IBrakes _breaks;
        private IHandling _handling;
        private ITransmission _transmission;
        private IShifter _shifter;

        public (float, float, float) FindWheelBaseLenAndAxelLengthes(VehicleAxle[] axleArray, Transform coM)
        {
            float maxZ = coM.InverseTransformPoint(axleArray[0].LeftHalfShaft.WheelVisualTransform.transform.position).z;

            float minZ = coM.InverseTransformPoint(axleArray[0].LeftHalfShaft.WheelVisualTransform.transform.position).z;

            int size = axleArray.Length;
            for (int i = 0; i < size; i++)
            {
                float zPos = coM.InverseTransformPoint(axleArray[i].LeftHalfShaft.WheelVisualTransform.transform.position).z;

                if (zPos > maxZ)
                {
                    maxZ = zPos;
                }
                if (zPos < minZ)
                {
                    minZ = zPos;
                }
            }

            minZ = Mathf.Abs(minZ);
            maxZ = Mathf.Abs(maxZ);

            return (maxZ + minZ, minZ, maxZ);
        }

        public (VehicleControllerStatsManager, VehicleControllerPartsManager) InitializeVehicleControllers(
            VehicleAxle[] frontAxleArray, VehicleAxle[] rearAxleArray, VehicleAxle[] steerAxleArray,
            Rigidbody rb, Transform transform, VehiclePartsSetWrapper partsPresetWrapper, Transform centerOfMass, CurrentCarStats currentCarStats)
        {
            if (!CheckIfPartsAreAssigned(partsPresetWrapper))
                return (null, null);

            _body = new Body();
            _engine = new Engine();
            _breaks = new Brakes();
            _handling = new Handling();
            _transmission = new Transmission();
            _shifter = new Shifter();

            VehicleAxle[] axleArray = VehicleAxle.CombineFrontAndRearAxles(frontAxleArray, rearAxleArray);

            _body.Initialize(rb, partsPresetWrapper, currentCarStats, transform, centerOfMass);
            _shifter.Initialize(partsPresetWrapper);
            _transmission.Initialize(partsPresetWrapper, currentCarStats, _shifter);
            _engine.Initialize(currentCarStats, partsPresetWrapper, _shifter, _transmission);
            _breaks.Initialize(partsPresetWrapper, axleArray, rearAxleArray,
                currentCarStats, rb, _transmission);
            _handling.Initialize(steerAxleArray);

            rb.centerOfMass = centerOfMass.transform.localPosition;
            (float wheelBase, float frontAxel, float rearAxel) = FindWheelBaseLenAndAxelLengthes(axleArray, centerOfMass);

            InitializeControllers(axleArray, partsPresetWrapper, rb, transform,
                wheelBase, frontAxel, rearAxel);

            VehicleControllerStatsManager statsManager = new(axleArray, frontAxleArray, rearAxleArray,
                currentCarStats, rb, transform, _engine, _transmission, _shifter, partsPresetWrapper);
            VehicleControllerPartsManager partsManager = new(_body, _engine, _transmission, _breaks, _handling,
                currentCarStats, transform, axleArray, frontAxleArray, rearAxleArray, centerOfMass);

            return (statsManager, partsManager);
        }

        public void InitializeControllers(VehicleAxle[] axleArray,
                    VehiclePartsSetWrapper partsPresetWrapper, Rigidbody _rb, Transform transform,
                    float wheelBase, float frontAxelLen, float rearAxelLen)
        {

            int size = axleArray.Length;
            for (int i = 0; i < size; i++)
            {
                bool front = Vector3.Dot(transform.forward, axleArray[i].LeftHalfShaft.WheelVisualTransform.transform.position - transform.position) > 0;
                float axelLen = front ? frontAxelLen : rearAxelLen;

                axleArray[i].InitializeAxle(partsPresetWrapper, _rb, wheelBase, axelLen, front);
            }
        }

        private bool CheckIfPartsAreAssigned(VehiclePartsSetWrapper partsPresetWrapper)
        {
            bool result = true;

            if (partsPresetWrapper.Engine == null)
            {
                Debug.LogError("No EngineSO assigned.");
                result = false;
            }

            if (partsPresetWrapper.Transmission == null)
            {
                Debug.LogError("No Transmission SO assigned.");
                result = false;
            }
            else if (partsPresetWrapper.Transmission.GearRatiosList.Count == 0)
            {
                Debug.LogError("Car has no gears. \n Add gear to the appropriate scriptable object.");
                result = false;
            }

            if (partsPresetWrapper.FrontTires == null || partsPresetWrapper.RearTires == null)
            {
                Debug.LogError("No TiresSO assigned.");
                result = false;
            }

            if (partsPresetWrapper.FrontSuspension == null || partsPresetWrapper.RearSuspension == null)
            {
                Debug.LogError("No SuspensionSO assigned.");
                result = false;
            }

            if (partsPresetWrapper.Brakes == null)
            {
                Debug.LogError("No BrakesSO assigned.");
                result = false;
            }


            if (partsPresetWrapper.Body == null)
            {
                Debug.LogError("No VehicleBodySO assigned.");
                result = false;
            }

            return result;
        }

        public ITransmission GetTransmission() => _transmission;
        public IShifter GetShifter() => _shifter;
    }
}
