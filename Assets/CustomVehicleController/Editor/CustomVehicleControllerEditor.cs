using Assets.VehicleController;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class CustomVehicleControllerEditor : EditorWindow
    {
        public static CustomVehicleControllerEditor Instance;

        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        public event Action OnWindowClosed;

        private Label _controllerSelectedLabel;
        private Toggle _saveChangedToggle;
        private Toggle _lockWindowToggle;

        private const string CONTROLLER_SELECTED_LABEL = "ControllerSelectionLabel";
        private const string SAVE_CHANGES_TOGGLE_NAME = "SaveChangesToggle";
        private const string LOCK_WINDOW_TOGGLE = "LockWindowToggle";

        private CustomVehicleController _controller;

        private SerializedObject _serializedController;
        private SerializedObject _serializedCarVisuals;

        private VehicleStats _vehicleStatsPlayMode;
        private int _suspensionRaycastNumPlayMode;

        #region Parts Editors
        private ControllerTransmissionSettingsEditor _transmissionSettingsEditor;
        private ControllerEngineSettingsEditor _engineSettingsEditor;
        private ControllerForcedInductionSettingsEditor _fiSettingEditor;
        private ControllerSuspensionSettingsEditor _suspensionSettingsEditor;
        private ControllerBodySettingsEditor _bodySettingsEditor;
        private ControllerTiresSettingsEditor _tiresSettingsEditor;
        private ControllerBrakesSettingsEditor _brakesSettingsEditor;
        private ControllerSteeringSettingsEditor _steeringSettingsEditor;
        private ControllerInitializerEditor _drivetrainSettingsEditor;

        private ControllerExtraVisualsSettingsEditor _extraVisualsSettingsEditor;
        #endregion

        private const string VEHICLE_PARTS_FOLDER_PATH = "\\VehicleController\\VehicleParts\\";

        [MenuItem("Tools/CustomVehicleControllerEditor")]
        public static void ShowTitle()
        {
            CustomVehicleControllerEditor wnd = GetWindow<CustomVehicleControllerEditor>();
            wnd.titleContent = new GUIContent("CustomVehicleControllerEditor");
        }
     
        public void CreateGUI()
        {
            if(Instance == null)
                Initialize();
        }

        private void Initialize()
        {
            Instance = this;

            VisualElement root = rootVisualElement;
            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            _controllerSelectedLabel = root.Q<Label>(CONTROLLER_SELECTED_LABEL);

            _lockWindowToggle = root.Q<Toggle>(LOCK_WINDOW_TOGGLE);
            //_lockWindowToggle.RegisterValueChangedCallback(evt => { Debug.Log(_serializedController.FindProperty("m_Name")); BindController(TryGetVehicleController()); });

            _saveChangedToggle = root.Q<Toggle>(SAVE_CHANGES_TOGGLE_NAME);

            _transmissionSettingsEditor = new ControllerTransmissionSettingsEditor(root, this);
            _fiSettingEditor = new ControllerForcedInductionSettingsEditor(root, this);
            _engineSettingsEditor = new ControllerEngineSettingsEditor(root, this, _fiSettingEditor);
            _suspensionSettingsEditor = new ControllerSuspensionSettingsEditor(root, this);
            _bodySettingsEditor = new ControllerBodySettingsEditor(root, this);
            _tiresSettingsEditor = new ControllerTiresSettingsEditor(root, this);
            _brakesSettingsEditor = new ControllerBrakesSettingsEditor(root, this);
            _steeringSettingsEditor = new ControllerSteeringSettingsEditor(root, this);
            _drivetrainSettingsEditor = new ControllerInitializerEditor(root, this);
            _extraVisualsSettingsEditor = new ControllerExtraVisualsSettingsEditor(root, this);

            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;

            BindController(TryGetVehicleController());

        }

        private void OnDestroy()
        {
            OnWindowClosed?.Invoke();
            EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
        }

        private void EditorApplication_playModeStateChanged(PlayModeStateChange newState)
        {
            if (newState == PlayModeStateChange.ExitingPlayMode)
            {
                if (!_saveChangedToggle.value)
                    return;

                CopyStats();
            }

            if (newState == PlayModeStateChange.EnteredEditMode)
            {
                if (_saveChangedToggle.value)
                {
                    PasteStats();
                }
                SaveController();
                //update field values in the editor after play mode. even though the vehicle stats object gets reset, the fields in editor don't
                SetVehicleControllerToSettingEditors(_serializedController, _controller);
            }
        }

        private void CopyStats()
        {
            if (_serializedController == null)
                return;
            _serializedController.Update();

            _extraVisualsSettingsEditor.CopyStats(_serializedController);
            _bodySettingsEditor.CopyStats(_serializedController);
            _steeringSettingsEditor.CopyStats(_serializedController);

            _suspensionRaycastNumPlayMode = _serializedController.FindProperty(nameof(CustomVehicleController.SuspensionSimulationPrecision)).intValue;
            _vehicleStatsPlayMode = new VehicleStats();
            SerializedProperty vehicleStats = _serializedController.FindProperty(nameof(CustomVehicleController.VehicleStats));

            _vehicleStatsPlayMode.EngineSO = vehicleStats.FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.EngineSO)).objectReferenceValue as EngineSO;
            _vehicleStatsPlayMode.TransmissionSO = vehicleStats.FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.TransmissionSO)).objectReferenceValue as TransmissionSO;
            _vehicleStatsPlayMode.FrontSuspensionSO = vehicleStats.FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.FrontSuspensionSO)).objectReferenceValue as SuspensionSO;
            _vehicleStatsPlayMode.RearSuspensionSO = vehicleStats.FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.RearSuspensionSO)).objectReferenceValue as SuspensionSO;
            _vehicleStatsPlayMode.FrontTiresSO = vehicleStats.FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.FrontTiresSO)).objectReferenceValue as TiresSO;
            _vehicleStatsPlayMode.RearTiresSO = vehicleStats.FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.RearTiresSO)).objectReferenceValue as TiresSO;
            _vehicleStatsPlayMode.BrakesSO = vehicleStats.FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.BrakesSO)).objectReferenceValue as BrakesSO;
            _vehicleStatsPlayMode.BodySO = vehicleStats.FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.BodySO)).objectReferenceValue as VehicleBodySO;

        }

        private void PasteStats()
        {

            if (_serializedController == null)
                return;

            _serializedController.Update();

            SerializedProperty vehicleStats = _serializedController.FindProperty(nameof(CustomVehicleController.VehicleStats));
            vehicleStats.FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.EngineSO)).objectReferenceValue = _vehicleStatsPlayMode.EngineSO;
            vehicleStats.FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.TransmissionSO)).objectReferenceValue = _vehicleStatsPlayMode.TransmissionSO;
            vehicleStats.FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.FrontSuspensionSO)).objectReferenceValue = _vehicleStatsPlayMode.FrontSuspensionSO;
            vehicleStats.FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.RearSuspensionSO)).objectReferenceValue = _vehicleStatsPlayMode.RearSuspensionSO;
            vehicleStats.FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.FrontTiresSO)).objectReferenceValue = _vehicleStatsPlayMode.FrontTiresSO;
            vehicleStats.FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.RearTiresSO)).objectReferenceValue = _vehicleStatsPlayMode.RearTiresSO;
            vehicleStats.FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.BrakesSO)).objectReferenceValue = _vehicleStatsPlayMode.BrakesSO;
            vehicleStats.FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.BodySO)).objectReferenceValue = _vehicleStatsPlayMode.BodySO;

            _extraVisualsSettingsEditor.PasteStats(_serializedController);
            _bodySettingsEditor.PasteStats(_serializedController);
            _steeringSettingsEditor.PasteStats(_serializedController);
            _serializedController.FindProperty(nameof(CustomVehicleController.SuspensionSimulationPrecision)).intValue = _suspensionRaycastNumPlayMode;
        }

        private void OnSelectionChange()
        {
            if (_lockWindowToggle.value)
                return;
                
            BindController(TryGetVehicleController());
        }

        private void OnBecameVisible()
        {
            if (Instance == null)
                Initialize();

            if (_lockWindowToggle.value)
                return;

            BindController(TryGetVehicleController());
        }

        public void RequestUpdate() => BindController(TryGetVehicleController());

        private CustomVehicleController TryGetVehicleController()
        {
            if (Selection.activeGameObject != null)
            {
                if(Selection.activeGameObject.TryGetComponent(out _controller))
                {
                    if (Selection.objects.Length > 1)
                    {
                        Debug.Log("Multiobject editing isn't supported");
                    }

                    _serializedController = new SerializedObject(_controller);
                    _serializedCarVisuals = new SerializedObject(_controller.GetComponent<CarVisualsEssentials>());
                    return _controller;
                }
                else if(Selection.activeGameObject.transform.root.TryGetComponent(out _controller))
                {

                    if (Selection.objects.Length > 1)
                    {
                        Debug.Log("Multiobject editing isn't supported");
                    }

                    _serializedController = new SerializedObject(_controller);
                    _serializedCarVisuals = new SerializedObject(_controller.GetComponent<CarVisualsEssentials>());
                    return _controller;
                }
            }

            if(_serializedController != null)
            {
                _serializedController.Dispose();
                _serializedController = null;
                _serializedCarVisuals.Dispose();
                _serializedCarVisuals=null;
            }

            _controller = null;
            return _controller;
        }

        public SerializedObject GetSerializedController() => _serializedController;
        public SerializedObject GetSerializedCarVisuals() => _serializedCarVisuals;

        public CustomVehicleController GetController() => _controller;

        public void SaveController()
        {
            if (_serializedController == null)
                return;
            _serializedController.ApplyModifiedProperties();
            _serializedController.Update();
        }

        private void BindController(CustomVehicleController controller)
        {
            SetVehicleControllerToSettingEditors(_serializedController, controller);

            if (controller != null)
            {
                _controllerSelectedLabel.text = "CURRENTLY SELECTED VEHICLE CONTROLLER: " + controller.name.ToUpper();
                _controllerSelectedLabel.style.color = Color.green;
                return;
            }

            _controllerSelectedLabel.text = "CURRENTLY SELECTED VEHICLE CONTROLLER: NONE";
            _controllerSelectedLabel.style.color = Color.red;
        }

        private void SetVehicleControllerToSettingEditors(SerializedObject so, CustomVehicleController controller)
        {
            _engineSettingsEditor.SetVehicleController(so);
            _transmissionSettingsEditor.SetVehicleController(so);
            _suspensionSettingsEditor.SetVehicleController(so);
            _bodySettingsEditor.SetVehicleController(so);
            _tiresSettingsEditor.SetVehicleController(so);
            _brakesSettingsEditor.SetVehicleController(so);
            _steeringSettingsEditor.SetVehicleController(so);
            _drivetrainSettingsEditor.SetVehicleController(so);
            _extraVisualsSettingsEditor.SetVehicleController(so);
        }

        private string FindPath()
        {
            MonoScript ms = MonoScript.FromScriptableObject(this);
            string scriptFilePath = AssetDatabase.GetAssetPath(ms);

            FileInfo fi = new FileInfo(scriptFilePath);
            string scriptFolder = fi.Directory.ToString();
            scriptFolder = Path.GetFullPath(Path.Combine(scriptFolder, "..")) + VEHICLE_PARTS_FOLDER_PATH;
            return scriptFolder.Substring(scriptFolder.IndexOf("Assets"));
        }

        public string GetVehiclePartsFolderPath(string folderName)
        {
            string path = FindPath() + folderName;
            if (!AssetDatabase.IsValidFolder(path))
            {
                Debug.LogError("Trying to save asset at path " + path + ", but folder was not found.");
            }
            return path;
        }
    }

}
