using UnityEditor;
using UnityEngine;

namespace Assets.VehicleController
{
#if UNITY_EDITOR
    [RequireComponent(typeof(CustomVehicleController))]
    public class ControllerHierarchyInitializer
    {
        [SerializeField]
        private Transform[] _wheelTransforms;
        [SerializeField]
        private Transform[] _steerWheelTransforms;

        [SerializeField]
        private PartTypes.DrivetrainType _drivetrainType;

        [SerializeField]
        private Transform _centerOfGeometry;
        [SerializeField]
        private Transform _centerOfMass;

        private Transform[] _steerParentTransforms;
        private WheelController[] _wheelControllers;
        private WheelController[] _steerWheelControllers;

        public void CreateHierarchyAndInitializeController(SerializedObject serializedController,
            SerializedObject serializedCarVisuals, CustomVehicleController controller)
        {
            GameObject wheelsParent = new ("Wheels");
            wheelsParent.transform.parent = controller.transform.root;
            wheelsParent.transform.localPosition = new (0, 0, 0);
            wheelsParent.transform.localRotation = Quaternion.identity;

            GameObject wheelsMeshesParent = new ("WheelsMeshes");
            wheelsMeshesParent.transform.parent = wheelsParent.transform;
            wheelsMeshesParent.transform.localPosition = new (0, 0, 0);
            wheelsMeshesParent.transform.localRotation = Quaternion.identity;

            GameObject wheelControllersParent = new ("WheelControllers");
            wheelControllersParent.transform.parent = wheelsParent.transform;
            wheelControllersParent.transform.localPosition = new (0, 0, 0);
            wheelControllersParent.transform.localRotation = Quaternion.identity;

            CreateWheelsHierarcy(wheelsMeshesParent.transform, wheelControllersParent.transform);
            TryMoveUpControllers();
            CreateSteerWheelsHierarcy(wheelsMeshesParent.transform);

            CreateCoG(controller.transform);
            CreateCoM(controller.transform);

            InjectCustomVehicleFields(serializedController);
            InjectCarVisualsFields(serializedCarVisuals);
        }

        private void CreateWheelsHierarcy(Transform meshesParent, Transform controllerParent)
        {
            int size = _wheelTransforms.Length;
            _wheelControllers = new WheelController[size];

            for (int i = 0; i < size; i++)
            {
                _wheelTransforms[i].transform.parent = meshesParent.transform;
                GameObject wheelObj = new (_wheelTransforms[i].name + "Controller");
                wheelObj.transform.parent = controllerParent.transform;
                wheelObj.transform.localPosition = _wheelTransforms[i].transform.localPosition;
                wheelObj.transform.localRotation = Quaternion.identity;
                wheelObj.AddComponent<WheelController>();
                _wheelControllers[i] = wheelObj.GetComponent<WheelController>();
                _wheelControllers[i].SetWheelMeshTransform(_wheelTransforms[i]);
            }
        }
        private void CreateSteerWheelsHierarcy(Transform meshesParent)
        {
            int size = _steerWheelTransforms.Length;
            _steerWheelControllers = new WheelController[size];
            _steerParentTransforms = new Transform[size];
            for (int i = 0; i < size; i++)
            {
                GameObject steerWheelParent = new ("SteerWheel");
                steerWheelParent.transform.parent = meshesParent.transform;
                steerWheelParent.transform.localPosition = _steerWheelTransforms[i].localPosition;
                steerWheelParent.transform.localRotation = Quaternion.identity;
                _steerWheelControllers[i] = _wheelControllers[i];
                _steerWheelTransforms[i].transform.parent = steerWheelParent.transform;
                _steerWheelTransforms[i].transform.localPosition = new (0, 0, 0);
                _steerWheelTransforms[i].transform.localRotation = Quaternion.identity;
                _steerParentTransforms[i] = steerWheelParent.transform;
            }
        }

        private void TryMoveUpControllers()
        {
            int size = _wheelTransforms.Length;
            for (int i = 0; i < size; i++)
            {
                if (_wheelTransforms[i].TryGetComponent<MeshRenderer>(out MeshRenderer mesh))
                {
                    _wheelControllers[i].transform.position = new (_wheelControllers[i].transform.position.x,
                        _wheelControllers[i].transform.position.y + mesh.bounds.size.y / 2,
                        _wheelControllers[i].transform.position.z);
                }
                else
                {
                    Debug.LogError($"Wheel {_wheelTransforms[i].name} has no mesh renderer, " +
                        $"but you need to move the game object with wheel controller script up " +
                        $"to simulate suspension top point");
                }
            }
        }

        private void CreateCoM(Transform transform)
        {
            _centerOfMass = new GameObject("CenterOfMass").transform;
            _centerOfMass.transform.parent = transform.root;
            _centerOfMass.transform.localPosition = new (0, 0, 0);
            _centerOfMass.transform.localRotation = Quaternion.identity;
        }

        private void CreateCoG(Transform transform)
        {
            _centerOfGeometry = new GameObject("CenterOfGeometry").transform;
            _centerOfGeometry.transform.parent = transform.root;
            _centerOfGeometry.transform.localPosition = new (0, 0, 0);
            _centerOfGeometry.transform.localRotation = Quaternion.identity;
        }

        private void InjectCarVisualsFields(SerializedObject serializedCarVisuals)
        {
            //set meshes
            var wheelMeshesArray = serializedCarVisuals.FindProperty("_wheelMeshes");
            wheelMeshesArray.ClearArray();
            int size1 = _wheelTransforms.Length;
            for (int i = 0; i < size1; i++)
            {
                wheelMeshesArray.InsertArrayElementAtIndex(i);
                wheelMeshesArray.GetArrayElementAtIndex(i).objectReferenceValue = _wheelTransforms[i];
            }

            //set steer wheels parents
            var steerWheelsArray = serializedCarVisuals.FindProperty("_steerableWheels");
            steerWheelsArray.ClearArray();
            int size2 = _steerParentTransforms.Length;
            for (int i = 0; i < size2; i++)
            {
                steerWheelsArray.InsertArrayElementAtIndex(i);
                steerWheelsArray.GetArrayElementAtIndex(i).objectReferenceValue = _steerParentTransforms[i];
            }

            //set wheel controllers
            var wheelControllerArray = serializedCarVisuals.FindProperty("_wheelControllerArray");
            wheelControllerArray.ClearArray();
            int size3 = _wheelControllers.Length;
            for (int i = 0; i < size3; i++)
            {
                wheelControllerArray.InsertArrayElementAtIndex(i);
                wheelControllerArray.GetArrayElementAtIndex(i).objectReferenceValue = _wheelControllers[i];
            }

            serializedCarVisuals.FindProperty("_steerWheel").objectReferenceValue = _steerWheelControllers[0];

            serializedCarVisuals.ApplyModifiedProperties();
            serializedCarVisuals.Update();
        }

        private void InjectCustomVehicleFields(SerializedObject serializedController)
        {
            var wheelControllersArray = serializedController.FindProperty("_wheelControllersArray");
            wheelControllersArray.ClearArray();
            int size1 = _wheelControllers.Length;
            for (var i = 0; i < size1; i++)
            {
                wheelControllersArray.InsertArrayElementAtIndex(i);
                wheelControllersArray.GetArrayElementAtIndex(i).objectReferenceValue = _wheelControllers[i];
                //do something with the property
            }

            var steerControllersArray = serializedController.FindProperty("_steerWheelControllersArray");
            steerControllersArray.ClearArray();
            int size2 = _steerWheelControllers.Length;
            for (var i = 0; i < size2; i++)
            {
                steerControllersArray.InsertArrayElementAtIndex(i);
                steerControllersArray.GetArrayElementAtIndex(i).objectReferenceValue = _steerWheelControllers[i];
                //do something with the property
            }

            serializedController.FindProperty("_centerOfGeometry").objectReferenceValue = _centerOfGeometry;
            serializedController.FindProperty("_centerOfMass").objectReferenceValue = _centerOfMass;
            serializedController.FindProperty("DrivetrainType").intValue = (int)_drivetrainType;

            serializedController.ApplyModifiedProperties();
            serializedController.Update();
        }

        public void SetWheelTransforms(Transform[] wheelTransforms)
        {
            _wheelTransforms = wheelTransforms;
        }
        public void SetSteerWheelTransforms(Transform[] steerTransforms)
        {
            _steerWheelTransforms = steerTransforms;
        }
        public void SetDrivetrainType(PartTypes.DrivetrainType type)
        {
            _drivetrainType = type;
        }
    }
#endif
}

