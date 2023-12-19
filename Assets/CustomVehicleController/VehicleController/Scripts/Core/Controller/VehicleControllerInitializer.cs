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

        public (float, float, float) FindWheelBaseLenAndAxelLengthes(WheelController[] wheelControllers)
        {
            float maxZ = wheelControllers[0].transform.root.GetComponent<CustomVehicleController>().GetCenterOfGeometry().InverseTransformPoint(wheelControllers[0].transform.position).z;
            float minZ = wheelControllers[0].transform.root.GetComponent<CustomVehicleController>().GetCenterOfGeometry().InverseTransformPoint(wheelControllers[0].transform.position).z;
            int size = wheelControllers.Length;
            for (int i = 0; i < size; i++)
            {
                float zPos = wheelControllers[i].transform.root.GetComponent<CustomVehicleController>().GetCenterOfGeometry().InverseTransformPoint(wheelControllers[i].transform.position).z;
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

        private List<WheelController> FindRearWheels(WheelController[] wheelControllersArray, Transform centerOfGeometry)
        {
            List<WheelController> rearWheels = new List<WheelController>();

            int size = wheelControllersArray.Length;
            for (int i = 0; i < size; i++)
            {
                bool front = wheelControllersArray[i].transform.root.GetComponent<CustomVehicleController>().GetCenterOfGeometry().
                    InverseTransformPoint(wheelControllersArray[i].transform.position).z >= centerOfGeometry.localPosition.z;
                if (!front)
                    rearWheels.Add(wheelControllersArray[i]);
            }
            return rearWheels;
        }

        private List<WheelController> FindFrontWheels(WheelController[] wheelControllersArray, Transform centerOfGeometry)
        {
            List<WheelController> frontWheels = new List<WheelController>();

            int size = wheelControllersArray.Length;
            for (int i = 0; i < size; i++)
            {
                bool front = wheelControllersArray[i].transform.root.GetComponent<CustomVehicleController>().GetCenterOfGeometry().
                    InverseTransformPoint(wheelControllersArray[i].transform.position).z >= centerOfGeometry.localPosition.z;
                if (front)
                    frontWheels.Add(wheelControllersArray[i]);
            }
            return frontWheels;
        }

        public (VehicleControllerStatsManager, VehicleControllerPartsManager) InitializeVehicleControllers(
            WheelController[] wheelControllersArray, WheelController[] steerWheelControllersArray,
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

            WheelController[] frontWheels = FindFrontWheels(wheelControllersArray, centerOfGeometry).ToArray();
            WheelController[] rearWheels = FindRearWheels(wheelControllersArray, centerOfGeometry).ToArray();

            _body.Initialize(rb, stats, currentCarStats, transform, centerOfMass);
            _shifter.Initialize(_clutch, stats);
            _transmission.Initialize(stats, currentCarStats, _shifter);
            _engine.Initialize(currentCarStats, stats, _shifter, _transmission);
            _breaks.Initialize(stats, wheelControllersArray, rearWheels,
                currentCarStats, rb, _transmission);
            _handling.Initialize(steerWheelControllersArray);

            rb.centerOfMass = centerOfMass.transform.localPosition;
            (float wheelBase, float frontAxel, float rearAxel) = FindWheelBaseLenAndAxelLengthes(wheelControllersArray);

            InitializeControllers(wheelControllersArray, stats, rb, transform,
                wheelBase, frontAxel, rearAxel);

            VehicleControllerStatsManager statsManager = new(wheelControllersArray, frontWheels, rearWheels,
                currentCarStats, rb, transform, _engine, _transmission, _shifter, stats);
            VehicleControllerPartsManager partsManager = new(_body, _engine, _transmission, _breaks, _handling,
                currentCarStats, transform, wheelControllersArray, frontWheels, rearWheels, centerOfGeometry);

            return (statsManager, partsManager);
        }

        public void InitializeControllers(WheelController[] wheelControllersArray,
                    VehicleStats vehicleStats, Rigidbody _rb, Transform transform,
                    float wheelBase, float frontAxelLen, float rearAxelLen)
        {

            int size = wheelControllersArray.Length;
            for (int i = 0; i < size; i++)
            {
                bool front = Vector3.Dot(transform.forward, wheelControllersArray[i].transform.position - transform.position) > 0;
                float axelLen = front ? frontAxelLen : rearAxelLen;

                wheelControllersArray[i].Initialize(vehicleStats, _rb, wheelBase, axelLen, front);
            }
        }

        public ITransmission GetTransmission() => _transmission;
        public IShifter GetShifter() => _shifter;
    }
}
