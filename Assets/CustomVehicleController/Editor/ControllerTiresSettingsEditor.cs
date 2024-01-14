using Assets.VehicleController;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class ControllerTiresSettingsEditor
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _mainEditor;

        #region body fields
        private TiresSO _forwardTiresSO;

        private ObjectField _frontTiresObjectField;
        private FloatField _forwardTiresCorneringStiffnessField;
        private CurveField _forwardTiresSideGripCurve;
        private CurveField _forwardTiresSideSlipCurve;
        private FloatField _forwardTiresForwardGripField;
        private TextField _forwardTiresNameField;

        private TiresSO _rearTiresSO;

        private ObjectField _rearTiresObjectField;
        private FloatField _rearTiresCorneringStiffnessField;
        private CurveField _rearTiresSideGripCurve;
        private CurveField _rearTiresSideSlipCurve;
        private FloatField _rearTiresForwardGripField;
        private TextField _rearTiresNameField;
        #endregion

        #region body field names
        private const string FORWARD_TIRES_OBJECT_FIELD = "ForwardTiresObjectField";
        private const string FORWARD_TIRES_CORNERING_STIFFNESS_FIELD = "ForwardTiresCorneringStiffness";
        private const string FORWARD_TIRES_SIDEWAYS_GRIP_CURVE = "ForwardTiresSidewaysGripCurve";
        private const string FORWARD_TIRES_SIDEWAYS_SLIP_CURVE = "ForwardTiresSidewaysSlipCurve";
        private const string FORWARD_TIRES_FORWARD_GRIP_FIELD = "ForwardTiresForwardGripField";

        private const string FORWARD_TIRES_NAME_FIELD = "ForwardTiresAssetNameField";
        private const string FORWARD_TIRES_CREATE_BUTTON_NAME = "ForwardTiresSaveButton";

        private const string REAR_TIRES_OBJECT_FIELD = "RearTiresObjectField";
        private const string REAR_TIRES_CORNERING_STIFFNESS_FIELD = "RearTiresCorneringStiffness";
        private const string REAR_TIRES_SIDEWAYS_GRIP_CURVE = "RearTiresSidewaysGripCurve";
        private const string REAR_TIRES_SIDEWAYS_SLIP_CURVE = "RearTiresSidewaysSlipCurve";
        private const string REAR_TIRES_FORWARD_GRIP_FIELD = "RearTiresForwardGripField";

        private const string REAR_TIRES_NAME_FIELD = "RearTiresAssetNameField";
        private const string REAR_TIRES_CREATE_BUTTON_NAME = "RearTiresSaveButton";
        #endregion

        private const string TIRES_FOLDER_NAME = "Tires";

        public ControllerTiresSettingsEditor(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _mainEditor = editor;
            FindForwardTiresFields();
            FindRearTiresFields();

            BindForwardTiresSOField();
            BindRearTiresSOField();

            RebindForwardTiresSettings(_forwardTiresSO);
            RebindRearTiresSettings(_rearTiresSO);

            SubscribeToForwardTiresSaveButtonClick();
            SubscribeToRearTiresSaveButtonClick();

            _mainEditor.OnWindowClosed += _mainEditor_OnWindowClosed;
            SetTooltips();
        }

        private void SetTooltips()
        {
            SetCorneringStiffnessTooltip();
            SetSidewaysGripTooltip();
            SetSidewaysSlipTooltip();
            SetForwardGripTooltip();
        }

        private void SetCorneringStiffnessTooltip()
        {
            StringBuilder corneringStiffnessSB = new StringBuilder();
            corneringStiffnessSB.AppendLine("Defines the cornering ability of a car.");
            corneringStiffnessSB.AppendLine();
            corneringStiffnessSB.AppendLine("The higher the value, the more the car wants to move in the direction it's facing.");
            corneringStiffnessSB.AppendLine();
            corneringStiffnessSB.AppendLine("Lower values [5:15] suitable for drift, and higher values for grip.");

            _forwardTiresCorneringStiffnessField.tooltip = corneringStiffnessSB.ToString();
            _rearTiresCorneringStiffnessField.tooltip = corneringStiffnessSB.ToString();
        }

        private void SetSidewaysGripTooltip()
        {
            StringBuilder sidewaysGripSB = new StringBuilder();
            sidewaysGripSB.AppendLine("The sideways grip curve is a multiplier to the cornering stiffness according to the current car speed / max speed of the engine.");
            sidewaysGripSB.AppendLine();
            sidewaysGripSB.AppendLine("X-axis - speed percent, Y-axis - multiplier. ");
            sidewaysGripSB.AppendLine();
            sidewaysGripSB.AppendLine("If you want to keep the car at maximum grip all the time, set keys to [0:1], [1:1].");
            sidewaysGripSB.AppendLine();
            sidewaysGripSB.AppendLine("If you want the car to drift, but still handle well at high speeds, make the keys increase in value, for example [0:0.2f], [1,1] and adjust cornering stiffness value.");

            _forwardTiresSideGripCurve.tooltip = sidewaysGripSB.ToString();
            _rearTiresSideGripCurve.tooltip = sidewaysGripSB.ToString();
        }

        private void SetSidewaysSlipTooltip()
        {
            StringBuilder sidewaysSlipSB = new StringBuilder();
            sidewaysSlipSB.AppendLine("The sideways slip curve defines how much grip the tires have depending on the dot product of vehicle movement and forward vector. It is a multiplier to the cornering stiffness.");
            sidewaysSlipSB.AppendLine();
            sidewaysSlipSB.AppendLine("X-axis - dot product (slipping). Y-axis - grip multiplier.");
            sidewaysSlipSB.AppendLine();
            sidewaysSlipSB.AppendLine("It is recommended to increase grip when the dot product is close to 1 because in this case car is moving perpendicular to its forward vector, so the wheels have to have very high resistance to movement.");

            _forwardTiresSideSlipCurve.tooltip = sidewaysSlipSB.ToString();
            _rearTiresSideSlipCurve.tooltip = sidewaysSlipSB.ToString();
        }

        private void SetForwardGripTooltip()
        {
            StringBuilder forwardGripSB = new StringBuilder();
            forwardGripSB.AppendLine("Defines how much force a wheel can take before it starts slipping.");
            forwardGripSB.AppendLine();
            forwardGripSB.AppendLine("If acceleration force is higher than wheel load * forward grip, the wheel starts slipping.");

            _forwardTiresForwardGripField.tooltip = forwardGripSB.ToString();
            _rearTiresForwardGripField.tooltip = forwardGripSB.ToString();
        }

        private void _mainEditor_OnWindowClosed()
        {
            var button1 = root.Q<Button>(name: FORWARD_TIRES_CREATE_BUTTON_NAME);
            button1.clicked -= ForwardTiresCreateAssetButton_onClick;

            var button2 = root.Q<Button>(name: REAR_TIRES_CREATE_BUTTON_NAME);
            button2.clicked -= RearTiresCreateAssetButton_onClick;

            _mainEditor.OnWindowClosed -= _mainEditor_OnWindowClosed;

        }

        private void FindForwardTiresFields()
        {
            _frontTiresObjectField = root.Q<ObjectField>(FORWARD_TIRES_OBJECT_FIELD);

            _forwardTiresCorneringStiffnessField = root.Q<FloatField>(FORWARD_TIRES_CORNERING_STIFFNESS_FIELD);

            _forwardTiresCorneringStiffnessField.RegisterValueChangedCallback(evt =>
            {
                _forwardTiresCorneringStiffnessField.value =
                Mathf.Max(0.1f, _forwardTiresCorneringStiffnessField.value);
            });

            _forwardTiresSideGripCurve = root.Q<CurveField>(FORWARD_TIRES_SIDEWAYS_GRIP_CURVE);

            _forwardTiresSideSlipCurve = root.Q<CurveField>(FORWARD_TIRES_SIDEWAYS_SLIP_CURVE);

            _forwardTiresForwardGripField = root.Q<FloatField>(FORWARD_TIRES_FORWARD_GRIP_FIELD);

            _forwardTiresForwardGripField.RegisterValueChangedCallback(evt =>
            {
                _forwardTiresForwardGripField.value =
                Mathf.Max(0.1f, _forwardTiresForwardGripField.value);
            });
            _forwardTiresNameField = root.Q<TextField>(FORWARD_TIRES_NAME_FIELD);
        }
        private void FindRearTiresFields()
        {
            _rearTiresObjectField = root.Q<ObjectField>(REAR_TIRES_OBJECT_FIELD);

            _rearTiresCorneringStiffnessField = root.Q<FloatField>(REAR_TIRES_CORNERING_STIFFNESS_FIELD);

            _rearTiresCorneringStiffnessField.RegisterValueChangedCallback(evt =>
            {
                _rearTiresCorneringStiffnessField.value =
                Mathf.Max(0.1f, _rearTiresCorneringStiffnessField.value);
            });

            _rearTiresSideGripCurve = root.Q<CurveField>(REAR_TIRES_SIDEWAYS_GRIP_CURVE);

            _rearTiresSideSlipCurve = root.Q<CurveField>(REAR_TIRES_SIDEWAYS_SLIP_CURVE);

            _rearTiresForwardGripField = root.Q<FloatField>(REAR_TIRES_FORWARD_GRIP_FIELD);

            _rearTiresForwardGripField.RegisterValueChangedCallback(evt =>
            {
                _rearTiresForwardGripField.value =
                Mathf.Max(0.1f, _rearTiresForwardGripField.value);
            });

            _rearTiresNameField = root.Q<TextField>(REAR_TIRES_NAME_FIELD);
        }

        private void BindForwardTiresSOField()
        {
            _frontTiresObjectField = root.Q<ObjectField>(FORWARD_TIRES_OBJECT_FIELD);

            _frontTiresObjectField.RegisterValueChangedCallback(x => RebindForwardTiresSettings(_frontTiresObjectField.value as TiresSO));

            if (_frontTiresObjectField.value == null)
            {
                _forwardTiresSO = CreateDefaultTires();
            }
            else
            {
                _forwardTiresSO = _frontTiresObjectField.value as TiresSO;
            }
        }
        private void RebindForwardTiresSettings(TiresSO loadedForwardTiresSO)
        {
            _forwardTiresSO = loadedForwardTiresSO;
            if (_mainEditor.GetSerializedController() != null)
            {
                _mainEditor.GetSerializedController().FindProperty(nameof(CustomVehicleController.VehicleStats)).
                    FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.FrontTiresSO)).objectReferenceValue = _forwardTiresSO;
                _mainEditor.SaveController();
            }
            if (_forwardTiresSO == null)
            {
                _forwardTiresSO = CreateDefaultTires();
            }
            SerializedObject so = new (_forwardTiresSO);
            BindForwardTiresCorneringStiffnessField(so);
            BindForwardTireSidewaysGripCurve(so);
            BindForwardTiresSideSlipCurve(so);
            BindForwardTiresForwardGripField(so);
        }

        private void BindForwardTiresCorneringStiffnessField(SerializedObject so)
        {
            _forwardTiresCorneringStiffnessField.bindingPath = nameof(_forwardTiresSO.SteeringStiffness);
            _forwardTiresCorneringStiffnessField.Bind(so);
        }
        private void BindForwardTireSidewaysGripCurve(SerializedObject so)
        {
            _forwardTiresSideGripCurve.bindingPath = nameof(_forwardTiresSO.SidewaysGripCurve);
            _forwardTiresSideGripCurve.Bind(so);
        }
        private void BindForwardTiresSideSlipCurve(SerializedObject so)
        {
            _forwardTiresSideSlipCurve.bindingPath = nameof(_forwardTiresSO.SidewaysSlipCurve);
            _forwardTiresSideSlipCurve.Bind(so);
        }
        private void BindForwardTiresForwardGripField(SerializedObject so)
        {
            _forwardTiresForwardGripField.bindingPath = nameof(_forwardTiresSO.ForwardGrip);
            _forwardTiresForwardGripField.Bind(so);
        }

        private void BindRearTiresSOField()
        {
            _rearTiresObjectField = root.Q<ObjectField>(REAR_TIRES_OBJECT_FIELD);

            _rearTiresObjectField.RegisterValueChangedCallback(x => RebindRearTiresSettings(_rearTiresObjectField.value as TiresSO));

            if (_rearTiresObjectField.value == null)
            {
                _rearTiresSO = CreateDefaultTires();
            }
            else
            {
                _rearTiresSO = _rearTiresObjectField.value as TiresSO;
            }
        }
        private void RebindRearTiresSettings(TiresSO loadedForwardSuspensionSO)
        {
            _rearTiresSO = loadedForwardSuspensionSO;
            if (_mainEditor.GetSerializedController() != null)
            {
                _mainEditor.GetSerializedController().FindProperty(nameof(CustomVehicleController.VehicleStats)).
                    FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.RearTiresSO)).objectReferenceValue = _rearTiresSO;
                _mainEditor.SaveController();
            }
            if (_rearTiresSO == null)
            {
                _rearTiresSO = CreateDefaultTires();
            }

            SerializedObject so = new (_rearTiresSO);
            BindRearTiresCorneringStiffnessField(so);
            BindRearTiresSideGripCurve(so);
            BindRearTiresSideSlipCurve(so);
            BindRearTiresForwardGripField(so);
        }

        private void BindRearTiresCorneringStiffnessField(SerializedObject so)
        {
            _rearTiresCorneringStiffnessField.bindingPath = nameof(_rearTiresSO.SteeringStiffness);
            _rearTiresCorneringStiffnessField.Bind(so);
        }
        private void BindRearTiresSideGripCurve(SerializedObject so)
        {
            _rearTiresSideGripCurve.bindingPath = nameof(_rearTiresSO.SidewaysGripCurve);
            _rearTiresSideGripCurve.Bind(so);
        }
        private void BindRearTiresSideSlipCurve(SerializedObject so)
        {
            _rearTiresSideSlipCurve.bindingPath = nameof(_rearTiresSO.SidewaysSlipCurve);
            _rearTiresSideSlipCurve.Bind(so);
        }
        private void BindRearTiresForwardGripField(SerializedObject so)
        {
            _rearTiresForwardGripField.bindingPath = nameof(_rearTiresSO.ForwardGrip);
            _rearTiresForwardGripField.Bind(so);
        }

        private TiresSO CreateDefaultTires()
        {
            TiresSO defaultTires = ScriptableObject.CreateInstance<TiresSO>();
            defaultTires.SteeringStiffness = 50;
            defaultTires.ForwardGrip = 1.5f;

            AnimationCurve sidewaysGripCurve = new();
            sidewaysGripCurve.AddKey(0, 0.25f);
            sidewaysGripCurve.AddKey(1, 1);
            defaultTires.SidewaysGripCurve = sidewaysGripCurve;


            AnimationCurve sidewaysSlipCurve = new ();
            Keyframe slip1 = new(0, 1, 0, 0.2f);
            Keyframe slip2 = new(0.95f, 0.5f, 0, 0);
            Keyframe slip3 = new(1, 1, 1, 0);
            sidewaysSlipCurve.AddKey(slip1);
            sidewaysSlipCurve.AddKey(slip2);
            sidewaysSlipCurve.AddKey(slip3);

            defaultTires.SidewaysSlipCurve = sidewaysSlipCurve;
            return defaultTires;
        }

        private void SubscribeToForwardTiresSaveButtonClick()
        {
            var button = root.Q<Button>(name: FORWARD_TIRES_CREATE_BUTTON_NAME);
            button.clicked += ForwardTiresCreateAssetButton_onClick;
        }
        private void ForwardTiresCreateAssetButton_onClick()
        {
            if (_forwardTiresNameField.text.ToString() == "")
            {
                Debug.LogWarning("Empty tires SO name");
                return;
            }

            string filePath = _mainEditor.GetVehiclePartsFolderPath(TIRES_FOLDER_NAME) + "/" + _forwardTiresNameField.text + ".asset";

            TiresSO newTires = CreateDefaultTires();

            var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(filePath);
            AssetDatabase.CreateAsset(newTires, uniqueFileName);
            AssetDatabase.SaveAssets();

            _forwardTiresSO = newTires;
            _frontTiresObjectField.value = _forwardTiresSO;
        }

        private void SubscribeToRearTiresSaveButtonClick()
        {
            var button = root.Q<Button>(name: REAR_TIRES_CREATE_BUTTON_NAME);
            button.clicked += RearTiresCreateAssetButton_onClick;
        }
        private void RearTiresCreateAssetButton_onClick()
        {
            if (_rearTiresNameField.text.ToString() == "")
            {
                Debug.LogWarning("Empty tires SO name");
                return;
            }

            string filePath = _mainEditor.GetVehiclePartsFolderPath(TIRES_FOLDER_NAME) + "/" + _rearTiresNameField.text + ".asset";

            TiresSO newTires = CreateDefaultTires();

            var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(filePath);
            AssetDatabase.CreateAsset(newTires, uniqueFileName);
            AssetDatabase.SaveAssets();

            _rearTiresSO = newTires;
            _rearTiresObjectField.value = _rearTiresSO;
        }
        public void SetVehicleController(SerializedObject so)
        {
            if(so == null)
            {
                _frontTiresObjectField.value = null;
                _rearTiresObjectField.value = null;
                return;
            }
            _frontTiresObjectField.value = so.FindProperty(nameof(CustomVehicleController.VehicleStats)).
                    FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.FrontTiresSO)).objectReferenceValue;

            _rearTiresObjectField.value = so.FindProperty(nameof(CustomVehicleController.VehicleStats)).
                    FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.RearTiresSO)).objectReferenceValue;

        }
    }

}
