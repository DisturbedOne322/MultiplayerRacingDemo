using System.Collections.Generic;
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
        private IClutch _clutch;

        public (float, float, float) FindWheelBaseLenAndAxelLengthes(VehicleAxle[] axleArray)
        {
            float maxZ = axleArray[0].transform.root.GetComponent<CustomVehicleController>().GetCenterOfGeometry().
                InverseTransformPoint(axleArray[0].LeftHalfShaft.WheelVisualTransform.transform.position).z;

            float minZ = axleArray[0].transform.root.GetComponent<CustomVehicleController>().GetCenterOfGeometry().
                InverseTransformPoint(axleArray[0].LeftHalfShaft.WheelVisualTransform.transform.position).z;

            int size = axleArray.Length;
            for (int i = 0; i < size; i++)
            {
                float zPos = axleArray[i].transform.root.GetComponent<CustomVehicleController>().GetCenterOfGeometry().
                    InverseTransformPoint(axleArray[i].LeftHalfShaft.WheelVisualTransform.transform.position).z;

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
            return (maxZ + minZ, minZ, maxZ);
        }

        private (List<VehicleAxle>, List<VehicleAxle>) FindFrontAndRearAxles(VehicleAxle[] axleArray, Transform centerOfGeometry)
        {
            List<VehicleAxle> rearAxles = new List<VehicleAxle>();
            List<VehicleAxle> frontAxles = new List<VehicleAxle>();


            int size = axleArray.Length;
            for (int i = 0; i < size; i++)
            {
                bool front = axleArray[i].transform.root.GetComponent<CustomVehicleController>().GetCenterOfGeometry().
                    InverseTransformPoint(axleArray[i].LeftHalfShaft.WheelVisualTransform.transform.position).z >= centerOfGeometry.localPosition.z;

                if (!front)
                    rearAxles.Add(axleArray[i]);
                else
                    frontAxles.Add(axleArray[i]);
            }
            return (frontAxles, rearAxles);
        }

        public (VehicleControllerStatsManager, VehicleControllerPartsManager) InitializeVehicleControllers(
            VehicleAxle[] axleArray, VehicleAxle[] steerAxleArray,
            Rigidbody rb, Transform transform, VehicleStats stats, Transform centerOfMass, Transform centerOfGeometry, 
            CurrentCarStats currentCarStats)
        {
            _body = new Body();
            _engine = new Engine();
            _breaks = new Brakes();
            _handling = new Handling();
            _transmission = new Transmission();
            _shifter = new Shifter();
            _clutch = new Clutch();

            (List<VehicleAxle> frontAxlesList , List<VehicleAxle> rearAxlesList) = FindFrontAndRearAxles(axleArray, centerOfGeometry);
            VehicleAxle[] frontAxles = frontAxlesList.ToArray();
            VehicleAxle[] rearAxles = rearAxlesList.ToArray();

            _body.Initialize(rb, stats, currentCarStats, transform, centerOfMass);
            _shifter.Initialize(_clutch, stats);
            _transmission.Initialize(stats, currentCarStats, _shifter);
            _engine.Initialize(currentCarStats, stats, _shifter, _transmission);
            _breaks.Initialize(stats, axleArray, rearAxles,
                currentCarStats, rb, _transmission);
            _handling.Initialize(steerAxleArray);

            rb.centerOfMass = centerOfMass.transform.localPosition;
            (float wheelBase, float frontAxel, float rearAxel) = FindWheelBaseLenAndAxelLengthes(axleArray);

            InitializeControllers(axleArray, stats, rb, transform,
                wheelBase, frontAxel, rearAxel);

            VehicleControllerStatsManager statsManager = new(axleArray, frontAxles, rearAxles,
                currentCarStats, rb, transform, _engine, _transmission, _shifter, stats);
            VehicleControllerPartsManager partsManager = new(_body, _engine, _transmission, _breaks, _handling,
                currentCarStats, transform, axleArray, frontAxles, rearAxles, centerOfGeometry);

            return (statsManager, partsManager);
        }

        public void InitializeControllers(VehicleAxle[] axleArray,
                    VehicleStats vehicleStats, Rigidbody _rb, Transform transform,
                    float wheelBase, float frontAxelLen, float rearAxelLen)
        {

            int size = axleArray.Length;
            for (int i = 0; i < size; i++)
            {
                bool front = Vector3.Dot(transform.forward, axleArray[i].LeftHalfShaft.WheelVisualTransform.transform.position - transform.position) > 0;
                float axelLen = front ? frontAxelLen : rearAxelLen;

                axleArray[i].InitializeAxle(vehicleStats, _rb, wheelBase, axelLen, front);
            }
        }

        public ITransmission GetTransmission() => _transmission;
        public IShifter GetShifter() => _shifter;
    }
}
