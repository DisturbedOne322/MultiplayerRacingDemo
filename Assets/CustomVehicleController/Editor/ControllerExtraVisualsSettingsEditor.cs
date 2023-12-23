using Assets.VehicleController;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class ControllerExtraVisualsSettingsEditor
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _editor;

        #region fields
        private FloatField _forwardSlipField;
        private FloatField _sidewaysSlipField;

        private Toggle _autoFlipToggle;
        private FloatField _flipDelayField;

        private Toggle _aerialControlsToggle;
        private FloatField _aerialSensitivityField;
        #endregion

        #region field names
        private const string PARTICLE_SYSTEM_FIELD_NAME = "SmokeParticlesObjectField";
        private const string TRAIL_RENDERER_FIELD_NAME = "TireTrailRendererObjectField";
        private const string FORWARD_SLIP_FIELD_NAME = "ForwardSlipThresholdField";
        private const string SIDEWAYS_SLIP_FIELD_NAME = "SidewaysSlipThresholdField";
        private const string AUTO_FLIP_TOGGLE_NAME = "AutoFlipOverToggle";
        private const string FLIP_DELAY_FIELD_NAME = "AutoFlipDelayField";
        private const string AERIAL_CONTROLS_TOGGLE_NAME = "AerialControlsToggle";
        private const string AERIAL_CONTROLS_SENSITIVITY_NAME = "AerialControlsSensitivityField";
        #endregion

        #region values changed during play mode

        private ParticleSystem _smokeParticleSystemPlayMode;
        private TrailRenderer _trailRendererPlayMode;
        private float _forwardSlipPlayMode;
        private float _sidewaysSlipPlayMode;

        private bool _autoFlipPlayMode;
        private float _flipDelayPlayMode;

        private bool _aerialControlsPlayMode;
        private float _aerialSensitivityPlayMode;
        #endregion

        public ControllerExtraVisualsSettingsEditor(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _editor = editor;
            FindFields();
        }
        public void PasteStats(SerializedObject controller)
        {
            controller.FindProperty(nameof(CustomVehicleController.ForwardSlippingThreshold)).floatValue = _forwardSlipPlayMode;
            controller.FindProperty(nameof(CustomVehicleController.SidewaysSlippingThreshold)).floatValue = _sidewaysSlipPlayMode;
            controller.FindProperty(nameof(CustomVehicleController.AutomaticFlipOverRecoverEnabled)).boolValue = _autoFlipPlayMode;
            controller.FindProperty(nameof(CustomVehicleController.AutomaticFlipOverRecoverDelay)).floatValue = _flipDelayPlayMode;
            controller.FindProperty(nameof(CustomVehicleController.AutomaticFlipOverRecoverEnabled)).boolValue = _aerialControlsPlayMode;
            controller.FindProperty(nameof(CustomVehicleController.AerialControlsSensitivity)).floatValue = _aerialSensitivityPlayMode;
        }

        public void CopyStats()
        {
            _forwardSlipPlayMode = _forwardSlipField.value;
            _sidewaysSlipPlayMode = _sidewaysSlipField.value;

            _autoFlipPlayMode = _autoFlipToggle.value;
            _flipDelayPlayMode = _flipDelayField.value;

            _aerialControlsPlayMode = _aerialControlsToggle.value;
            _aerialSensitivityPlayMode = _aerialSensitivityField.value;
        }

        private void FindFields()
        {
            _forwardSlipField = root.Q<FloatField>(FORWARD_SLIP_FIELD_NAME);
            _forwardSlipField.RegisterValueChangedCallback(evt =>
            {
                _forwardSlipField.value = Mathf.Clamp(_forwardSlipField.value, 0, 100);
                SerializedObject serializedObject = _editor.GetSerializedController();
                if (serializedObject != null)
                {
                    serializedObject.FindProperty(nameof(CustomVehicleController.ForwardSlippingThreshold)).floatValue = _forwardSlipField.value;
                    _editor.SaveController();
                }
            });

            _sidewaysSlipField = root.Q<FloatField>(SIDEWAYS_SLIP_FIELD_NAME);
            _sidewaysSlipField.RegisterValueChangedCallback(evt =>
            {
                _sidewaysSlipField.value = Mathf.Clamp(_sidewaysSlipField.value, 0, 1);
                SerializedObject serializedObject = _editor.GetSerializedController();
                if (serializedObject != null)
                {
                    serializedObject.FindProperty(nameof(CustomVehicleController.SidewaysSlippingThreshold)).floatValue = _sidewaysSlipField.value;
                    _editor.SaveController();
                }
            });

            _autoFlipToggle = root.Q<Toggle>(AUTO_FLIP_TOGGLE_NAME);
            _autoFlipToggle.RegisterValueChangedCallback(evt =>
            {
                _flipDelayField.style.display = _autoFlipToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
                SerializedObject serializedObject = _editor.GetSerializedController();
                if (serializedObject != null)
                {
                    serializedObject.FindProperty(nameof(CustomVehicleController.AutomaticFlipOverRecoverEnabled)).boolValue = _autoFlipToggle.value;
                    _editor.SaveController();
                }
            });

            _flipDelayField = root.Q<FloatField>(FLIP_DELAY_FIELD_NAME);
            _flipDelayField.RegisterValueChangedCallback(evt =>
            {
                _flipDelayField.value = Mathf.Clamp(_flipDelayField.value, 0.1f, 100);
                SerializedObject serializedObject = _editor.GetSerializedController();
                if (serializedObject != null)
                {
                    serializedObject.FindProperty(nameof(CustomVehicleController.AutomaticFlipOverRecoverDelay)).floatValue = _flipDelayField.value;
                    _editor.SaveController();
                }
            });

            _aerialControlsToggle = root.Q<Toggle>(AERIAL_CONTROLS_TOGGLE_NAME);
            _aerialControlsToggle.RegisterValueChangedCallback(evt =>
            {
                _aerialSensitivityField.style.display = _aerialControlsToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
                SerializedObject serializedObject = _editor.GetSerializedController();
                if (serializedObject != null)
                {
                    serializedObject.FindProperty(nameof(CustomVehicleController.AerialControlsEnabled)).boolValue = _aerialControlsToggle.value;
                    _editor.SaveController();
                }
            });

            _aerialSensitivityField = root.Q<FloatField>(AERIAL_CONTROLS_SENSITIVITY_NAME);
            _aerialSensitivityField.RegisterValueChangedCallback(evt =>
            {
                SerializedObject serializedObject = _editor.GetSerializedController();
                if (serializedObject != null)
                {
                    serializedObject.FindProperty(nameof(CustomVehicleController.AerialControlsSensitivity)).floatValue = _aerialSensitivityField.value;
                    _editor.SaveController();
                }
            });
        }

        public void SetVehicleController(SerializedObject vehicleController)
        {
            _autoFlipToggle.value = vehicleController == null ? true : vehicleController.FindProperty(nameof(CustomVehicleController.AutomaticFlipOverRecoverEnabled)).boolValue;
            _flipDelayField.value = vehicleController == null ? 2 : vehicleController.FindProperty(nameof(CustomVehicleController.AutomaticFlipOverRecoverDelay)).floatValue;

            _aerialControlsToggle.value = vehicleController == null ? true : vehicleController.FindProperty(nameof(CustomVehicleController.AerialControlsEnabled)).boolValue;
            _aerialSensitivityField.value = vehicleController == null ? 7500 : vehicleController.FindProperty(nameof(CustomVehicleController.AerialControlsSensitivity)).floatValue;

            _forwardSlipField.value = vehicleController == null ? 0.1f : vehicleController.FindProperty(nameof(CustomVehicleController.ForwardSlippingThreshold)).floatValue;
            _sidewaysSlipField.value = vehicleController == null ? 0.3f : vehicleController.FindProperty(nameof(CustomVehicleController.SidewaysSlippingThreshold)).floatValue;
        }
    }

}
