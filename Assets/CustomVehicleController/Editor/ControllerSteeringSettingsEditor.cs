using Assets.VehicleController;
using UnityEditor;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class ControllerSteeringSettingsEditor
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _editor;

        private FloatField _steerAngleField;
        private FloatField _steerSpeedField;

        private const string STEER_ANGLE_FIELD_NAME = "SteerAngleInput";
        private const string STEER_SPEED_FIELD_NAME = "SteerSpeedInput";

        private float _steerAnglePlayMode;
        private float _steerSpeedPlayMode;

        public ControllerSteeringSettingsEditor(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _editor = editor;
            FindSteeringFields();
        }


        public void PasteStats(SerializedObject controller)
        {
            controller.FindProperty(nameof(CustomVehicleController.SteerAngle)).floatValue = _steerAnglePlayMode;
            controller.FindProperty(nameof(CustomVehicleController.SteerSpeed)).floatValue = _steerSpeedPlayMode;
        }

        public void CopyStats(SerializedObject controller)
        {
            _steerAnglePlayMode = controller.FindProperty(nameof(CustomVehicleController.SteerAngle)).floatValue;
            _steerSpeedPlayMode = controller.FindProperty(nameof(CustomVehicleController.SteerSpeed)).floatValue;
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
            if (_editor.GetSerializedController() != null)
            {
                if (newValue < 0)
                    newValue = 0;
                _steerAngleField.value = newValue;
                _editor.GetSerializedController().FindProperty(nameof(CustomVehicleController.SteerAngle)).floatValue = newValue;
                _editor.SaveController();
            }
        }
        private void HandleSpeedValueChanges(float newValue)
        {
            if (_editor.GetSerializedController() != null)
            {
                if (newValue < 0)
                    newValue = 0;
                _steerSpeedField.value = newValue;
                _editor.GetSerializedController().FindProperty(nameof(CustomVehicleController.SteerSpeed)).floatValue = newValue;
                _editor.SaveController();
            }
        }

        public void SetVehicleController(SerializedObject controller)
        {
            if (controller != null)
            {
                _steerAngleField.value = controller.FindProperty(nameof(CustomVehicleController.SteerAngle)).floatValue;
                _steerSpeedField.value = controller.FindProperty(nameof(CustomVehicleController.SteerSpeed)).floatValue;
            }
            else
            {
                _steerAngleField.value = 30;
                _steerSpeedField.value = 0.2f;
            }
        }
    }

}
