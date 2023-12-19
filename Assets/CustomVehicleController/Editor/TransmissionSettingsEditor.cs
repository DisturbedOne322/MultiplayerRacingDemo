using Assets.VehicleController;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class TransmissionSettingsEditor : EditorWindow
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _mainEditor;

        #region Transmission
        private TransmissionSO _transmissionSO;
        private TextField _transmissionTextField;
        private FloatField _transmissionFinalDriveField;
        private FloatField _transmissionShiftCDField;

        private Slider _upshiftSlider;
        private Slider _downshiftSlider;

        private ObjectField _transmissionSOObjectField;

        private ListView _gearRatiosListView;

        private const string TRANSMISSION_GEAR_RATIOS_LIST_NAME = "GearRatiosList";
        private const string TRANSMISSION_TEXT_FIELD_NAME = "TransmissionAssetName";
        private const string TRANSMISSION_FINAL_DRIVE_FIELD_NAME = "FinalDriveInput";
        private const string TRANSMISSION_SHIFT_CD_NAME = "ShiftCooldownInput";
        private const string TRANSMISSION_CREATE_BUTTON_NAME = "CreateTransmissionAssetButton";
        private const string TRANSMISSION_SO_NAME = "TransmissionSOField";

        private const string TRANSMISSION_UPSHIFT_SLIDER_NAME = "UpshiftRPMSlider";
        private const string TRANSMISSION_DONWSHIFT_SLIDER_NAME = "DownshiftRPMSlider";
        #endregion

        private const string TRANSMISSION_FOLDER_NAME = "Transmissions";

        private const float MIN_RPM_DIFFERENCE = 0.15f;

        public void HandleTransmissionSettings(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _mainEditor = editor;

            FindTransmissionFields();

            BindTransmissionSOField();
            RebindTransmissionSettings(_transmissionSO);
            SubscribeToTransmissionSaveButtonClick();

            _mainEditor.OnWindowClosed += _mainEditor_OnWindowClosed;
        }

        private void _mainEditor_OnWindowClosed()
        {
            var button = root.Q<Button>(name: TRANSMISSION_CREATE_BUTTON_NAME);
            button.clicked -= TransmissionCreateAssetButton_onClick;
            _mainEditor.OnWindowClosed -= _mainEditor_OnWindowClosed;
        }

        private void FindTransmissionFields()
        {
            _transmissionTextField = root.Q<TextField>(name: TRANSMISSION_TEXT_FIELD_NAME);
            _transmissionFinalDriveField = root.Q<FloatField>(name: TRANSMISSION_FINAL_DRIVE_FIELD_NAME);
            _transmissionFinalDriveField.RegisterValueChangedCallback(evt => { _transmissionFinalDriveField.value = Mathf.Max(0.1f, _transmissionFinalDriveField.value); });
            _transmissionShiftCDField = root.Q<FloatField>(name: TRANSMISSION_SHIFT_CD_NAME);
            _transmissionShiftCDField.RegisterValueChangedCallback(evt => { _transmissionShiftCDField.value = Mathf.Max(0f, _transmissionShiftCDField.value); });
            _gearRatiosListView = root.Q<ListView>(TRANSMISSION_GEAR_RATIOS_LIST_NAME);

            _upshiftSlider = root.Q<Slider>(TRANSMISSION_UPSHIFT_SLIDER_NAME);
            _upshiftSlider.RegisterValueChangedCallback(evt =>
            {
                if (_downshiftSlider.value > _upshiftSlider.value - MIN_RPM_DIFFERENCE)
                {
                    _downshiftSlider.value = _upshiftSlider.value - MIN_RPM_DIFFERENCE;
                }
            });
            _downshiftSlider = root.Q<Slider>(TRANSMISSION_DONWSHIFT_SLIDER_NAME);
            _downshiftSlider.RegisterValueChangedCallback(evt => { _downshiftSlider.value = Mathf.Clamp(_downshiftSlider.value, 0, _upshiftSlider.value - MIN_RPM_DIFFERENCE); });
        }


        private void BindTransmissionSOField()
        {
            _transmissionSOObjectField = root.Q<ObjectField>(TRANSMISSION_SO_NAME);

            _transmissionSOObjectField.RegisterValueChangedCallback(x => RebindTransmissionSettings(_transmissionSOObjectField.value as TransmissionSO));

            if (_transmissionSOObjectField.value == null)
            {
                _transmissionSO = CreateDefaultTransmission();
            }
            else
            {
                _transmissionSO = _transmissionSOObjectField.value as TransmissionSO;
            }
        }

        private TransmissionSO CreateDefaultTransmission()
        {
            TransmissionSO defaultTransmissionSO = ScriptableObject.CreateInstance<TransmissionSO>();
            List<float> gearList = new List<float>
            {
                3.45f,
                2.12f,
                1.44f,
                1.13f,
                0.91f
            };
            defaultTransmissionSO.GearRatiosList = gearList;
            defaultTransmissionSO.FinalDriveRatio = 1;
            defaultTransmissionSO.ShiftCooldown = 0.2f;
            return defaultTransmissionSO;
        }

        private void RebindTransmissionSettings(TransmissionSO loadedTransmissionSO)
        {
            _transmissionSO = loadedTransmissionSO;
            if (_mainEditor.GetSerializedController() != null && _mainEditor.GetController() != null)
            {
                _mainEditor.GetSerializedController().FindProperty(nameof(CustomVehicleController.VehicleStats)).FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.TransmissionSO)).objectReferenceValue = _transmissionSO;
                _mainEditor.SaveController();
            }
            if (_transmissionSO == null)
            {
                _transmissionSO = CreateDefaultTransmission();
            }

            SerializedObject so = new (_transmissionSO);
            BindGearList(so);
            BindFinalDrive(so);
            BindShiftCD(so);
            BindUpshiftSlider(so);
            BindDownshiftSlider(so);
        }

        private void BindGearList(SerializedObject so)
        {
            _gearRatiosListView.bindingPath = nameof(_transmissionSO.GearRatiosList);
            _gearRatiosListView.Bind(so);
        }
        private void BindFinalDrive(SerializedObject so)
        {
            _transmissionFinalDriveField.bindingPath = nameof(_transmissionSO.FinalDriveRatio);
            _transmissionFinalDriveField.Bind(so);
        }
        private void BindShiftCD(SerializedObject so)
        {
            _transmissionShiftCDField.bindingPath = nameof(_transmissionSO.ShiftCooldown);
            _transmissionShiftCDField.Bind(so);
        }

        private void BindUpshiftSlider(SerializedObject so)
        {
            _upshiftSlider.bindingPath = nameof(_transmissionSO.UpShiftRPMPercent);
            _upshiftSlider.Bind(so);
        }
        private void BindDownshiftSlider(SerializedObject so)
        {
            _downshiftSlider.bindingPath = nameof(_transmissionSO.DownShiftRPMPercent);
            _downshiftSlider.Bind(so);
        }


        private void SubscribeToTransmissionSaveButtonClick()
        {
            var button = root.Q<Button>(name: TRANSMISSION_CREATE_BUTTON_NAME);
            button.clicked += TransmissionCreateAssetButton_onClick;
        }

        private void TransmissionCreateAssetButton_onClick()
        {
            if (_transmissionTextField.text.ToString() == "")
            {
                Debug.LogWarning("Empty transmission SO name");
                return;
            }

            string filePath = _mainEditor.GetVehiclePartsFolderPath(TRANSMISSION_FOLDER_NAME) + "/" + _transmissionTextField.text + ".asset";

            TransmissionSO newTransmission = CreateDefaultTransmission();

            var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(filePath);
            AssetDatabase.CreateAsset(newTransmission, uniqueFileName);
            AssetDatabase.SaveAssets();

            _transmissionSO = newTransmission;
            _transmissionSOObjectField.value = _transmissionSO;
        }

        public void SetVehicleController(SerializedObject so)
        {
            if(so == null)
            {
                _transmissionSOObjectField.value = null;
                return;
            }
            _transmissionSOObjectField.value = so.FindProperty(nameof(CustomVehicleController.VehicleStats)).
                    FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.TransmissionSO)).objectReferenceValue;
            //_transmissionSOObjectField.value = controller == null ? null : controller.VehicleStats.TransmissionSO;
        }
    }

}
