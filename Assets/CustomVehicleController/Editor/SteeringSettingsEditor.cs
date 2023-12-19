using Assets.VehicleController;
using UnityEditor;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class SteeringSettingsEditor : EditorWindow
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _editor;

        private FloatField _steerAngleField;
        private FloatField _steerSpeedField;

        private const string STEER_ANGLE_FIELD_NAME = "SteerAngleInput";
        private const string STEER_SPEED_FIELD_NAME = "SteerSpeedInput";

        private float _steerAnglePlayMode;
        private float _steerSpeedPlayMode;

        public void HandleSteeringSettings(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _editor = editor;
            FindSteeringFields();
        }

        public void UpdateValueFields(PlayModeStateChange playModeState)
        {
            if (playModeState == PlayModeStateChange.EnteredEditMode)
            {
                UpdateEditModeValues(_editor.GetSerializedController());
            }

            if (playModeState == PlayModeStateChange.ExitingPlayMode)
            {
                CopyValuesFromPlayMode();
            }
        }

        private void UpdateEditModeValues(SerializedObject controller)
        {
            controller.FindProperty(nameof(CustomVehicleController.SteerAngle)).floatValue = _steerAnglePlayMode;
            controller.FindProperty(nameof(CustomVehicleController.SteerSpeed)).floatValue = _steerSpeedPlayMode;
        }

        private void CopyValuesFromPlayMode()
        {
            _steerAnglePlayMode = _steerAngleField.value;
            _steerSpeedPlayMode = _steerSpeedField.value;
        }

        private void FindSteeringFields()
        {
            _steerAngleField = root.Q<FloatField>(STEER_ANGLE_FIELD_NAME);
            _steerAngleField.RegisterValueChangedCallback(evt => { HandleAngleValueChanges(evt.newValue); });
            _steerSpeedField = root.Q<FloatField>(STEER_SPEED_FIELD_NAME);
            _steerSpeedField.RegisterValueChangedCallback(evt => { HandleSpeedValueChanges(evt.newValue); });
        }

        private void HandleAngleValueChanges(float newValue)
        {
            if (_editor.ControllerSelected())
            {
                if (newValue < 0)
                    newValue = 0;
                _editor.GetSerializedController().FindProperty(nameof(CustomVehicleController.SteerAngle)).floatValue = newValue;
                _editor.SaveController();
            }
        }
        private void HandleSpeedValueChanges(float newValue)
        {
            if (_editor.ControllerSelected())
            {
                if (newValue < 0)
                    newValue = 0;
                _editor.GetSerializedController().FindProperty(nameof(CustomVehicleController.SteerSpeed)).floatValue = newValue;
                _editor.SaveController();
            }
        }

        public void SetVehicleController(CustomVehicleController controller)
        {
            if (controller != null)
            {
                _steerAngleField.value = controller.SteerAngle;
                _steerSpeedField.value = controller.SteerSpeed;
            }
            else
            {
                _steerAngleField.value = 0;
                _steerSpeedField.value = 0;
            }
        }
    }

}
