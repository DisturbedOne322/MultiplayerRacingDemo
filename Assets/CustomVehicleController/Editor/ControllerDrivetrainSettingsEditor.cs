using Assets.VehicleController;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class ControllerDrivetrainSettingsEditor
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _mainEditor;

        private Foldout _foldout;

        private EnumField _drivetrainTypeEnum;
        private ObjectField _frontLeftWheelObjField;
        private ObjectField _frontRightWheelObjField;
        private ObjectField _rearLeftWheelObjField;
        private ObjectField _rearRightWheelObjField;

        private ObjectField _bodyMeshField;
        private ObjectField _bodyRWDField;
        private ObjectField _bodyFWDField;
        private ObjectField _bodyAWDField;

        private VisualElement _carAWDImage;
        private VisualElement _carRWDImage;
        private VisualElement _carFWDImage;

        private Label _notInitializedMessageLabel;

        private const string FOLDOUT_NAME = "DrivetrainFoldout";

        private const string DRIVETRAIN_TYPE_ENUM_NAME = "DrivetrainTypeEnum";
        private const string FRONT_LEFT_FIELD_NAME = "FrontLeftWheelObjField";
        private const string FRONT_RIGHT_FIELD_NAME = "FrontRightWheelObjField";
        private const string REAR_LEFT_FIELD_NAME = "RearLeftWheelObjField";
        private const string REAR_RIGHT_FIELD_NAME = "RearRightWheelObjField";
        private const string CAR_AWD_IMG_NAME = "CarAWDImage";
        private const string BODY_AWD_FIELD_NAME = "AWDBodyField";
        private const string CAR_RWD_IMG_NAME = "CarRWDImage";
        private const string BODY_RWD_FIELD_NAME = "RWDBodyField";
        private const string CAR_FWD_IMG_NAME = "CarFWDImage";
        private const string BODY_FWD_FIELD_NAME = "FWDBodyField";
        private const string INIT_BUTTON_NAME = "InitializeController";
        private const string NOT_INITIALIZED_LABEL_NAME = "ControllerNotInitializedLabel";

        private PartTypes.DrivetrainType _drivetrainTypePlayMode;

        #region prefab unpack
        private VisualElement _warningWindow;
        private Button _closeWindowsButton;
        private Button _confirmButton;
        private Button _denyButton;   

        private const string WINDOW_NAME = "WarningWindow";
        private const string CONFIRM_BUTTON_NAME = "ConfirmButton";
        private const string DENY_BUTTON_NAME = "DenyButton";
        private const string CLOSE_WINDOWS_BUTTON_NAME = "CloseWindowButton";
        #endregion

        public ControllerDrivetrainSettingsEditor(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _mainEditor = editor;
            FindDrivetrainFields();
            FindWarningWindowFields();
            SubscribeToInitializeButtonClickEvent();

            _mainEditor.OnWindowClosed += _editor_OnWindowClosed;
            SetTooltips();
        }


        private void SetTooltips()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Drag and drop appropriate game objects which represent wheels into transform fields, ideally the ones with MeshRenderer components (this will allow automatic wheel radius and suspension position calculation).");
            sb.AppendLine();
            sb.AppendLine("This button will create a hierarchy of game objects, add needed scripts, populate scripts with appropriate references, and position game objects at correct location if mesh renderer is provided.");
            sb.AppendLine("");
            sb.AppendLine("Since hierarchy will be changed, the root game object can't be a prefab.");
            root.Q<Button>(INIT_BUTTON_NAME).tooltip = sb.ToString();
        }

        private void _editor_OnWindowClosed()
        {
            Button initializeButton = root.Q<Button>(INIT_BUTTON_NAME);
            initializeButton.clicked -= InitializeButton_clicked;

            _mainEditor.OnWindowClosed -= _editor_OnWindowClosed;
        }

        public void PasteStats(SerializedObject serializedObject)
        {
            serializedObject.FindProperty(nameof(CustomVehicleController.DrivetrainType)).intValue = (int)_drivetrainTypePlayMode;
        }

        public void CopyStats(SerializedObject serializedObject)
        {
            _drivetrainTypePlayMode = (PartTypes.DrivetrainType)serializedObject.FindProperty(nameof(CustomVehicleController.DrivetrainType)).intValue;
        }

        private void FindDrivetrainFields()
        {
            _foldout = root.Q<Foldout>(FOLDOUT_NAME);

            _drivetrainTypeEnum = root.Q<EnumField>(DRIVETRAIN_TYPE_ENUM_NAME);
            _drivetrainTypeEnum.RegisterValueChangedCallback(val => { UpdateDrivetrainType((PartTypes.DrivetrainType)_drivetrainTypeEnum.value); });

            _frontLeftWheelObjField = root.Q<ObjectField>(FRONT_LEFT_FIELD_NAME);
            _frontRightWheelObjField = root.Q<ObjectField>(FRONT_RIGHT_FIELD_NAME);
            _rearLeftWheelObjField = root.Q<ObjectField>(REAR_LEFT_FIELD_NAME);
            _rearRightWheelObjField = root.Q<ObjectField>(REAR_RIGHT_FIELD_NAME);

            _carAWDImage = root.Q<VisualElement>(CAR_AWD_IMG_NAME);
            _carRWDImage = root.Q<VisualElement>(CAR_RWD_IMG_NAME);
            _carFWDImage = root.Q<VisualElement>(CAR_FWD_IMG_NAME);

            _bodyRWDField = root.Q<ObjectField>(BODY_RWD_FIELD_NAME);
            _bodyAWDField = root.Q<ObjectField>(BODY_AWD_FIELD_NAME);
            _bodyFWDField = root.Q<ObjectField>(BODY_FWD_FIELD_NAME);

            _notInitializedMessageLabel = root.Q<Label>(NOT_INITIALIZED_LABEL_NAME);
        }

        private void FindWarningWindowFields()
        {
            _warningWindow = root.Q<VisualElement>(WINDOW_NAME);
            _closeWindowsButton = root.Q<Button>(CLOSE_WINDOWS_BUTTON_NAME);
            _confirmButton = root.Q<Button>(CONFIRM_BUTTON_NAME);
            _denyButton = root.Q<Button>(DENY_BUTTON_NAME);


            _closeWindowsButton.clicked += () => { _warningWindow.style.display = DisplayStyle.None; };
            _denyButton.clicked += () => { _warningWindow.style.display = DisplayStyle.None; };
            _confirmButton.clicked += () => { _warningWindow.style.display = DisplayStyle.None; UnpackPrefab(Selection.activeGameObject); InitializeController(Selection.activeGameObject); };
        }
        private void SubscribeToInitializeButtonClickEvent()
        {
            Button initializeButton = root.Q<Button>(INIT_BUTTON_NAME);
            initializeButton.clicked += InitializeButton_clicked;
        }


        private void InitializeButton_clicked()
        {
            GameObject selectedGO = Selection.activeGameObject;
            if (selectedGO == null)
            {
                Debug.LogError("No game object selected");
                return;
            }
            if (_mainEditor.GetController() == null)
            {
                Debug.LogError("CustomVehicleController script is missing");
                return;
            }
            if (_frontLeftWheelObjField.value == null)
            {
                Debug.LogError("Front left transform is missing");
                return;
            }
            if (_frontRightWheelObjField.value == null)
            {
                Debug.LogError("Front right transform is missing");
                return;
            }
            if (_rearLeftWheelObjField.value == null)
            {
                Debug.LogError("Rear left transform is missing");
                return;
            }
            if (_rearRightWheelObjField.value == null)
            {
                Debug.LogError("Rear right transform is missing");
                return;
            }

            if(PrefabUtility.GetPrefabAssetType(selectedGO) != PrefabAssetType.NotAPrefab)
            {
                HandleWarningWindow(selectedGO);
                return;
            }

            InitializeController(selectedGO);
        }

        private void HandleWarningWindow(GameObject selectedGO)
        {
            _warningWindow.style.display = DisplayStyle.Flex;
        }
        private void UnpackPrefab(GameObject selectedGO)
        {
            GameObject rootGO = selectedGO.transform.root.gameObject;
            PrefabUtility.UnpackPrefabInstance(rootGO, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        private void InitializeController(GameObject selectedGO)
        {
            GameObject rootGO = selectedGO.transform.root.gameObject;
            ControllerHierarchyInitializer init = new ();

            Transform[] wheels = new Transform[4];
            wheels[0] = _frontLeftWheelObjField.value as Transform;
            wheels[1] = _frontRightWheelObjField.value as Transform;
            wheels[2] = _rearLeftWheelObjField.value as Transform;
            wheels[3] = _rearRightWheelObjField.value as Transform;

            Transform[] steerWheels = new Transform[2];
            steerWheels[0] = _frontLeftWheelObjField.value as Transform;
            steerWheels[1] = _frontRightWheelObjField.value as Transform;

            init.SetSteerWheelTransforms(steerWheels);
            init.SetWheelTransforms(wheels);
            init.SetDrivetrainType((PartTypes.DrivetrainType)_drivetrainTypeEnum.value);
            SetReferenceToBodyField((PartTypes.DrivetrainType)_drivetrainTypeEnum.value);
            init.CreateHierarchyAndInitializeController(_mainEditor.GetSerializedController(),
                _mainEditor.GetSerializedCarVisuals(), _mainEditor.GetController(), _bodyMeshField.value as MeshRenderer);

            DisplayControllerNotInitializedMessage(false);
        }

        private void UpdateDrivetrainType(PartTypes.DrivetrainType type)
        {
            switch (type)
            {
                case PartTypes.DrivetrainType.FWD:
                    _carFWDImage.style.display = DisplayStyle.Flex;
                    _carAWDImage.style.display = DisplayStyle.None;
                    _carRWDImage.style.display = DisplayStyle.None;
                    break;
                case PartTypes.DrivetrainType.RWD:
                    _carRWDImage.style.display = DisplayStyle.Flex;
                    _carFWDImage.style.display = DisplayStyle.None;
                    _carAWDImage.style.display = DisplayStyle.None;
                    break;
                case PartTypes.DrivetrainType.AWD:
                    _carAWDImage.style.display = DisplayStyle.Flex;
                    _carRWDImage.style.display = DisplayStyle.None;
                    _carFWDImage.style.display = DisplayStyle.None;
                    break;
            }

            SerializedObject serializedObject = _mainEditor.GetSerializedController();

            if (serializedObject != null)
            {
                serializedObject.FindProperty(nameof(CustomVehicleController.DrivetrainType)).intValue = (int)type;
                _mainEditor.SaveController();
            }
        }

        public void SetVehicleController(CustomVehicleController controller)
        {
            if (controller != null)
            {
                _drivetrainTypeEnum.value = controller.DrivetrainType;
                WheelController[] wheels = controller.GetWheelControllers();
                if (wheels == null)
                {
                    DisplayControllerNotInitializedMessage(true);
                }
                else
                {
                    if (wheels.Length == 4)
                    {
                        _frontLeftWheelObjField.value = wheels[0].GetWheelTransform();
                        _frontRightWheelObjField.value = wheels[1].GetWheelTransform();
                        _rearLeftWheelObjField.value = wheels[2].GetWheelTransform();
                        _rearRightWheelObjField.value = wheels[3].GetWheelTransform();
                    }
                    DisplayControllerNotInitializedMessage(wheels.Length == 0);
                }
            }
            else
            {
                _frontLeftWheelObjField.value =
                _frontRightWheelObjField.value =
                _rearLeftWheelObjField.value =
                _rearRightWheelObjField.value = null;
            }
        }

        private void DisplayControllerNotInitializedMessage(bool notInitialized)
        {
            if (notInitialized)
                _foldout.value = true;
            _notInitializedMessageLabel.style.display = notInitialized ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetReferenceToBodyField(PartTypes.DrivetrainType type)
        {
            switch(type)
            {
                case PartTypes.DrivetrainType.AWD:
                    _bodyMeshField = _bodyAWDField;
                    _bodyFWDField.value = _bodyAWDField.value;
                    _bodyRWDField.value = _bodyAWDField.value;
                    break;
                case PartTypes.DrivetrainType.RWD:
                    _bodyMeshField = _bodyRWDField;
                    _bodyFWDField.value = _bodyRWDField.value;
                    _bodyAWDField.value = _bodyRWDField.value;
                    break;
                case PartTypes.DrivetrainType.FWD:
                    _bodyMeshField = _bodyFWDField;
                    _bodyAWDField.value = _bodyFWDField.value;
                    _bodyRWDField.value = _bodyFWDField.value;
                    break;
            }
        }
    }

}
