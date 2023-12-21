using Assets.VehicleController;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace Assets.VehicleControllerEditor
{
    public class SuspensionSettingsEditor : EditorWindow
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _mainEditor;

        #region Suspension Fields
        private SuspensionSO _forwardSuspensionSO;

        private ObjectField _forwardSuspensionObjectField;
        private FloatField _forwardSuspensionStiffnessField;
        private FloatField _forwardSuspensionDamperField;
        private Slider _forwardSuspensionHeightSlider;
        private TextField _forwardSuspensionNameField;

        private SuspensionSO _rearSuspensionSO;

        private ObjectField _rearSuspensionObjectField;
        private FloatField _rearSuspensionStiffnessField;
        private FloatField _rearSuspensionDamperField;
        private Slider _rearSuspensionHeightSlider;
        private TextField _rearSuspensionNameField;
        #endregion

        #region SuspensionFieldNames
        private const string FORWARD_SUSPENSION_OBJECT_FIELD = "ForwardSuspensionObjectField";
        private const string FORWARD_SUSPENSION_STIFFNESS_FIELD = "ForwardSuspensionStiffnessField";
        private const string FORWARD_SUSPENSION_DAMPER_FIELD = "ForwardSuspensionDamperField";
        private const string FORWARD_SUSPENSION_HEIGHT_SLIDER_FIELD = "ForwardSuspensionHeightSliderField";
        private const string FORWARD_SUSPENSION_NAME_FIELD = "ForwardSuspensionNameField";
        private const string FORWARD_SUSPENSION_SAVE_BUTTON_NAME = "ForwardSuspensionSaveButton";

        private const string REAR_SUSPENSION_OBJECT_FIELD = "RearSuspensionObjectField";
        private const string REAR_SUSPENSION_STIFFNESS_FIELD = "RearSuspensionStiffnessField";
        private const string REAR_SUSPENSION_DAMPER_FIELD = "RearSuspensionDamperField";
        private const string REAR_SUSPENSION_HEIGHT_SLIDER_FIELD = "RearSuspensionHeightSliderField";
        private const string REAR_SUSPENSION_NAME_FIELD = "RearSuspensionNameField";
        private const string REAR_SUSPENSION_SAVE_BUTTON_NAME = "RearSuspensionSaveButton";
        #endregion
        private const string SUSPENSION_FOLDER_NAME = "Suspensions";

        public void HandleSuspensionSetting(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _mainEditor = editor;
            FindForwardSuspensionFields();
            FindRearSuspensionFields();

            BindForwardSuspensionSOField();
            BindRearSuspensionSOField();
            RebindFrontSuspensionSettings(_forwardSuspensionSO);
            RebindRearSuspensionSettings(_rearSuspensionSO);

            SubscribeToForwardSuspensionSaveButtonClick();
            SubscribeToRearSuspensionSaveButtonClick();

            _mainEditor.OnWindowClosed += _mainEditor_OnWindowClosed;
            SetTooltips();
        }

        private void SetTooltips()
        {
            StringBuilder sb1 = new StringBuilder();
            sb1.AppendLine("Defines the amount of force that is applied to the suspension.");
            sb1.AppendLine("");
            sb1.AppendLine("If the value is too low, the suspension won't be able to keep the car at a desired height above the ground.");
            sb1.AppendLine("");
            sb1.AppendLine("Vehicle mass has an influence on the amount of force required.");
            sb1.AppendLine("");
            sb1.AppendLine("Recommended values: [50000: 200000].");

            _forwardSuspensionStiffnessField.tooltip = sb1.ToString();
            _rearSuspensionStiffnessField.tooltip = sb1.ToString();

            StringBuilder sb2 = new StringBuilder();
            sb2.AppendLine("Damper stiffness defines how much the suspension wants to travel.");
            sb2.AppendLine("");
            sb2.AppendLine("The lower the value, the less resistance the suspension has to movement.");
            sb2.AppendLine("");
            sb2.AppendLine("Recommended values: [500: 3000].");

            _forwardSuspensionDamperField.tooltip = sb2.ToString();
            _rearSuspensionDamperField.tooltip = sb2.ToString();
        }

        private void _mainEditor_OnWindowClosed()
        {
            var button1 = root.Q<Button>(name: FORWARD_SUSPENSION_SAVE_BUTTON_NAME);
            button1.clicked -= ForwardSuspensionCreateAssetButton_onClick;

            var button2 = root.Q<Button>(name: REAR_SUSPENSION_SAVE_BUTTON_NAME);
            button2.clicked -= RearSuspensionCreateAssetButton_onClick;
            _mainEditor.OnWindowClosed -= _mainEditor_OnWindowClosed;
        }

        private void FindForwardSuspensionFields()
        {
            _forwardSuspensionStiffnessField = root.Q<FloatField>(FORWARD_SUSPENSION_STIFFNESS_FIELD);
            _forwardSuspensionStiffnessField.RegisterValueChangedCallback(evt =>
            {
                _forwardSuspensionStiffnessField.value =
                Mathf.Max(0, _forwardSuspensionStiffnessField.value);
            });

            _forwardSuspensionDamperField = root.Q<FloatField>(FORWARD_SUSPENSION_DAMPER_FIELD);
            _forwardSuspensionDamperField.RegisterValueChangedCallback(evt =>
            {
                _forwardSuspensionDamperField.value =
                Mathf.Max(0, _forwardSuspensionDamperField.value);
            });

            _forwardSuspensionHeightSlider = root.Q<Slider>(FORWARD_SUSPENSION_HEIGHT_SLIDER_FIELD);
            _forwardSuspensionNameField = root.Q<TextField>(FORWARD_SUSPENSION_NAME_FIELD);
        }
        private void FindRearSuspensionFields()
        {
            _rearSuspensionStiffnessField = root.Q<FloatField>(REAR_SUSPENSION_STIFFNESS_FIELD);
            _rearSuspensionStiffnessField.RegisterValueChangedCallback(evt =>
            {
                _rearSuspensionStiffnessField.value =
                Mathf.Max(0, _rearSuspensionStiffnessField.value);
            });
            _rearSuspensionDamperField = root.Q<FloatField>(REAR_SUSPENSION_DAMPER_FIELD);
            _rearSuspensionDamperField.RegisterValueChangedCallback(evt =>
            {
                _rearSuspensionDamperField.value =
                Mathf.Max(0, _rearSuspensionDamperField.value);
            });
            _rearSuspensionHeightSlider = root.Q<Slider>(REAR_SUSPENSION_HEIGHT_SLIDER_FIELD);
            _rearSuspensionNameField = root.Q<TextField>(REAR_SUSPENSION_NAME_FIELD);
        }

        private void BindForwardSuspensionSOField()
        {
            _forwardSuspensionObjectField = root.Q<ObjectField>(FORWARD_SUSPENSION_OBJECT_FIELD);

            _forwardSuspensionObjectField.RegisterValueChangedCallback(x => RebindFrontSuspensionSettings(_forwardSuspensionObjectField.value as SuspensionSO));

            if (_forwardSuspensionObjectField.value == null)
            {
                _forwardSuspensionSO = CreateDefaultSuspension();
            }
            else
            {
                _forwardSuspensionSO = _forwardSuspensionObjectField.value as SuspensionSO;
            }
        }

        private void RebindFrontSuspensionSettings(SuspensionSO loadedFrontSuspensionSO)
        {
            _forwardSuspensionSO = loadedFrontSuspensionSO;

            if (_mainEditor.GetSerializedController() != null)
            {
                _mainEditor.GetSerializedController().FindProperty(nameof(CustomVehicleController.VehicleStats)).
                    FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.FrontSuspensionSO)).objectReferenceValue = _forwardSuspensionSO;
                _mainEditor.SaveController();
            }

            if (_forwardSuspensionSO == null)
            {
                _forwardSuspensionSO = CreateDefaultSuspension();
            }

            SerializedObject so = new (_forwardSuspensionSO);
            BindForwardSuspensionStiffnessField(so);
            BindForwardSuspensionDamperField(so);
            BindForwardSuspensionHeightField(so);
        }

        private void BindForwardSuspensionStiffnessField(SerializedObject so)
        {
            _forwardSuspensionStiffnessField.bindingPath = nameof(_forwardSuspensionSO.SpringStiffness);
            _forwardSuspensionStiffnessField.Bind(so);
        }
        private void BindForwardSuspensionDamperField(SerializedObject so)
        {
            _forwardSuspensionDamperField.bindingPath = nameof(_forwardSuspensionSO.SpringDampingStiffness);
            _forwardSuspensionDamperField.Bind(so);
        }
        private void BindForwardSuspensionHeightField(SerializedObject so)
        {
            _forwardSuspensionHeightSlider.bindingPath = nameof(_forwardSuspensionSO.SpringRestDistance);
            _forwardSuspensionHeightSlider.Bind(so);
        }

        private void BindRearSuspensionSOField()
        {
            _rearSuspensionObjectField = root.Q<ObjectField>(REAR_SUSPENSION_OBJECT_FIELD);

            _rearSuspensionObjectField.RegisterValueChangedCallback(x => RebindRearSuspensionSettings(_rearSuspensionObjectField.value as SuspensionSO));

            if (_rearSuspensionObjectField.value == null)
            {
                _rearSuspensionSO = CreateDefaultSuspension();
            }
            else
            {
                _rearSuspensionSO = _rearSuspensionObjectField.value as SuspensionSO;
            }
        }

        private void RebindRearSuspensionSettings(SuspensionSO loadedForwardSuspensionSO)
        {
            _rearSuspensionSO = loadedForwardSuspensionSO;

            if (_mainEditor.GetSerializedController() != null)
            {
                _mainEditor.GetSerializedController().FindProperty(nameof(CustomVehicleController.VehicleStats)).
                    FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.RearSuspensionSO)).objectReferenceValue = _rearSuspensionSO;
                _mainEditor.SaveController();
            }

            if (_rearSuspensionSO == null)
            {
                _rearSuspensionSO = CreateDefaultSuspension();
            }

            SerializedObject so = new (_rearSuspensionSO);
            BindRearSuspensionStiffnessField(so);
            BindRearSuspensionDamperField(so);
            BindRearSuspensionHeightField(so);
        }

        private void BindRearSuspensionStiffnessField(SerializedObject so)
        {
            _rearSuspensionStiffnessField.bindingPath = nameof(_rearSuspensionSO.SpringStiffness);
            _rearSuspensionStiffnessField.Bind(so);
        }
        private void BindRearSuspensionDamperField(SerializedObject so)
        {
            _rearSuspensionDamperField.bindingPath = nameof(_rearSuspensionSO.SpringDampingStiffness);
            _rearSuspensionDamperField.Bind(so);
        }
        private void BindRearSuspensionHeightField(SerializedObject so)
        {
            _rearSuspensionHeightSlider.bindingPath = nameof(_rearSuspensionSO.SpringRestDistance);
            _rearSuspensionHeightSlider.Bind(so);
        }

        private SuspensionSO CreateDefaultSuspension()
        {
            SuspensionSO defaultSuspension = ScriptableObject.CreateInstance<SuspensionSO>();
            defaultSuspension.SpringStiffness = 60000f;
            defaultSuspension.SpringDampingStiffness = 3500f;
            defaultSuspension.SpringRestDistance = 0.3f;

            return defaultSuspension;
        }
        private void SubscribeToForwardSuspensionSaveButtonClick()
        {
            var button = root.Q<Button>(name: FORWARD_SUSPENSION_SAVE_BUTTON_NAME);
            button.clicked += ForwardSuspensionCreateAssetButton_onClick;
        }
        private void ForwardSuspensionCreateAssetButton_onClick()
        {
            if (_forwardSuspensionNameField.text.ToString() == "")
            {
                Debug.LogWarning("Empty suspension SO name");
                return;
            }

            string filePath = _mainEditor.GetVehiclePartsFolderPath(SUSPENSION_FOLDER_NAME) + "/" + _forwardSuspensionNameField.text + ".asset";

            SuspensionSO newSusp = CreateDefaultSuspension();

            var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(filePath);
            AssetDatabase.CreateAsset(newSusp, uniqueFileName);
            AssetDatabase.SaveAssets();

            _forwardSuspensionSO = newSusp;
            _forwardSuspensionObjectField.value = _forwardSuspensionSO;
        }
        private void SubscribeToRearSuspensionSaveButtonClick()
        {
            var button = root.Q<Button>(name: REAR_SUSPENSION_SAVE_BUTTON_NAME);
            button.clicked += RearSuspensionCreateAssetButton_onClick;
        }
        private void RearSuspensionCreateAssetButton_onClick()
        {
            if (_rearSuspensionNameField.text.ToString() == "")
            {
                Debug.LogWarning("Empty suspension SO name");
                return;
            }

            string filePath = _mainEditor.GetVehiclePartsFolderPath(SUSPENSION_FOLDER_NAME) + "/" + _rearSuspensionNameField.text + ".asset";

            SuspensionSO newSusp = CreateDefaultSuspension();

            var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(filePath);
            AssetDatabase.CreateAsset(newSusp, uniqueFileName);
            AssetDatabase.SaveAssets();

            _rearSuspensionSO = newSusp;
            _rearSuspensionObjectField.value = _rearSuspensionSO;
        }

        public void SetVehicleController(SerializedObject so)
        {
            if (so == null)
            {
                _forwardSuspensionObjectField.value = null;
                _rearSuspensionObjectField.value = null;
                return;
            }

            _forwardSuspensionObjectField.value = so.FindProperty(nameof(CustomVehicleController.VehicleStats)).
                    FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.FrontSuspensionSO)).objectReferenceValue;

            _rearSuspensionObjectField.value = so.FindProperty(nameof(CustomVehicleController.VehicleStats)).
                    FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.RearSuspensionSO)).objectReferenceValue;
        }
    }

}
