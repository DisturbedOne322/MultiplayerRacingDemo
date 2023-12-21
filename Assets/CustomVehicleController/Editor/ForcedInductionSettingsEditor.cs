using Assets.VehicleController;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
//

namespace Assets.VehicleControllerEditor
{
    public class ForcedInductionSettingsEditor : EditorWindow
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _mainEditor;

        #region body fields
        private ForcedInductionSO _inductionSO;

        private ObjectField _fiObjectField;
        private EnumField _foTypeEnum;
        private FloatField _maxBoostField;
        private Slider _turboLagSlider;
        private FloatField _turboSpinSpeedField;

        private VisualElement _turboLagHolder;

        private TextField _foNameField;
        #endregion

        #region body field names
        private const string FI_OBJECT_FIELD_NAME = "ForcedInductionSOObjectField";
        private const string FI_TYPE_ENUM = "FITypeEnum";
        private const string FI_MAX_BOOST_FIELD_NAME = "FOMaxBoostField";
        private const string FI_TURBO_LAG_SLIDER_NAME = "TurboLagSlider";
        private const string FI_TURBO_SPEED_FIELD_NAME = "TurboSpinSpeedField";

        private const string FI_TURBO_LAG_HOLDER_NAME = "TurboLagHolder";

        private const string FI_NAME_FIELD_NAME = "FISONameTextField";
        private const string FI_CREATE_BUTTON_NAME = "FOSaveButton";
        #endregion

        private const string FORCED_INDUCTION_FOLDER_NAME = "ForcedInductions";

        public void HandleForcedInductionSettings(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _mainEditor = editor;
            FindFIFields();
            BindFISOField();
            RebindFISettings(_inductionSO);
            SubscribeToFISaveButtonClick();
            _mainEditor.OnWindowClosed += _mainEditor_OnWindowClosed;


            SetTooltips();
        }

        private void SetTooltips()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Forced induction provides an additional boost. The way boost is provided depends on the forced induction type.");
            sb.AppendLine("");
            sb.AppendLine("Turbocharger: isn't working until certain engine rpm. Takes time to spin to full speed. Boost depends on gas input.");
            sb.AppendLine("");
            sb.AppendLine("Supercharger: constant boost.");
            sb.AppendLine("");
            sb.AppendLine("Centrifugal: boost is directly tied to engine rpm.");
            _foTypeEnum.tooltip = sb.ToString();
        }

        private void _mainEditor_OnWindowClosed()
        {
            var button = root.Q<Button>(name: FI_CREATE_BUTTON_NAME);
            button.clicked -= FICreateAssetButton_onClick;
            _mainEditor.OnWindowClosed -= _mainEditor_OnWindowClosed;
        }

        private void FindFIFields()
        {
            _foTypeEnum = root.Q<EnumField>(FI_TYPE_ENUM);
            _foTypeEnum.RegisterValueChangedCallback(evt => { DisplayForcedInductionTypeField((PartTypes.ForcedInductionType)_foTypeEnum.value); });

            _maxBoostField = root.Q<FloatField>(FI_MAX_BOOST_FIELD_NAME);
            _maxBoostField.RegisterValueChangedCallback(evt => { _maxBoostField.value = Mathf.Max(0, _maxBoostField.value); });

            _turboLagSlider = root.Q<Slider>(FI_TURBO_LAG_SLIDER_NAME);

            _turboSpinSpeedField = root.Q<FloatField>(FI_TURBO_SPEED_FIELD_NAME);
            _turboSpinSpeedField.RegisterValueChangedCallback(evt => { _turboSpinSpeedField.value = Mathf.Max(0.1f, _turboSpinSpeedField.value); });

            _foNameField = root.Q<TextField>(FI_NAME_FIELD_NAME);

            _turboLagHolder = root.Q<VisualElement>(FI_TURBO_LAG_HOLDER_NAME);
        }

        private void DisplayForcedInductionTypeField(PartTypes.ForcedInductionType type)
        {
            switch(type)
            {
                case PartTypes.ForcedInductionType.None:
                    _maxBoostField.style.display = DisplayStyle.None;
                    _turboLagHolder.style.display = DisplayStyle.None;
                    _turboSpinSpeedField.style.display = DisplayStyle.None;
                    break;
                case PartTypes.ForcedInductionType.Supercharger:
                case PartTypes.ForcedInductionType.Centrifugal:
                    _maxBoostField.style.display = DisplayStyle.Flex;
                    _turboLagHolder.style.display = DisplayStyle.None;
                    _turboSpinSpeedField.style.display = DisplayStyle.None;
                    break;
                case PartTypes.ForcedInductionType.Turbocharger:
                    _maxBoostField.style.display = DisplayStyle.Flex;
                    _turboLagHolder.style.display = DisplayStyle.Flex;
                    _turboSpinSpeedField.style.display = DisplayStyle.Flex;
                    break;
            }
        }

        private void BindFISOField()
        {
            _fiObjectField = root.Q<ObjectField>(FI_OBJECT_FIELD_NAME);

            _fiObjectField.RegisterValueChangedCallback(x => RebindFISettings(_fiObjectField.value as ForcedInductionSO));

            if (_fiObjectField.value == null)
            {
                _inductionSO = CreateDefaultFI();
            }
            else
            {
                _inductionSO = _fiObjectField.value as ForcedInductionSO;
            }
        }
        private void RebindFISettings(ForcedInductionSO loadedFISO)
        {
            _inductionSO = loadedFISO;
            if (_mainEditor.GetSerializedController() != null)
            {
                _mainEditor.SaveController();
            }
            if (_inductionSO == null)
            {
                _inductionSO = CreateDefaultFI();
            }

            DisplayForcedInductionTypeField(_inductionSO.ForcedInductionType);

            SerializedObject so = new(_inductionSO);
            BindFIType(so);
            BindMaxBoostField(so);
            BindTurboLagField(so);
            BindTurboSpinSpeedField(so);
        }

        public void UpdateForcedInduction(ForcedInductionSO loadedFISO)
        {
            if (loadedFISO == null)
                _fiObjectField.value = null;
            else
            {
                _inductionSO = loadedFISO;
                _fiObjectField.value = _inductionSO;
            }
        }

        private void BindFIType(SerializedObject so)
        {
            _foTypeEnum.bindingPath = nameof(_inductionSO.ForcedInductionType);
            _foTypeEnum.Bind(so);
        }
        private void BindMaxBoostField(SerializedObject so)
        {
            _maxBoostField.bindingPath = nameof(_inductionSO.MaxTorqueBoostAmount);
            _maxBoostField.Bind(so);
        }
        private void BindTurboLagField(SerializedObject so)
        {
            _turboLagSlider.bindingPath = nameof(_inductionSO.TurboRPMPercentDelay);
            _turboLagSlider.Bind(so);
        }
        private void BindTurboSpinSpeedField(SerializedObject so)
        {
            _turboSpinSpeedField.bindingPath = nameof(_inductionSO.TurboSpinTime);
            _turboSpinSpeedField.Bind(so);
        }

        private ForcedInductionSO CreateDefaultFI()
        {
            ForcedInductionSO defaultFISO = ScriptableObject.CreateInstance<ForcedInductionSO>();
            defaultFISO.ForcedInductionType = PartTypes.ForcedInductionType.None;
            defaultFISO.MaxTorqueBoostAmount = 50;
            defaultFISO.TurboRPMPercentDelay = 0.35f;   
            defaultFISO.TurboSpinTime = 2;
            return defaultFISO;
        }

        private void SubscribeToFISaveButtonClick()
        {
            var button = root.Q<Button>(name: FI_CREATE_BUTTON_NAME);
            button.clicked += FICreateAssetButton_onClick;
        }

        private void FICreateAssetButton_onClick()
        {
            Debug.Log("clicked");
            if (_foNameField.text.ToString() == "")
            {
                Debug.LogWarning("Empty forced induction SO name");
                return;
            }

            string filePath = _mainEditor.GetVehiclePartsFolderPath(FORCED_INDUCTION_FOLDER_NAME) + "/" + _foNameField.text + ".asset";

            ForcedInductionSO newFI = CreateDefaultFI();

            var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(filePath);
            AssetDatabase.CreateAsset(newFI, uniqueFileName);
            AssetDatabase.SaveAssets();

            _inductionSO = newFI;
            _fiObjectField.value = _inductionSO;
        }
    }
}

