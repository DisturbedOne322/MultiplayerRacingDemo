using Assets.VehicleController;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class TiresSettingsEditor : EditorWindow
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

        public void HandleTiresSettings(VisualElement root, CustomVehicleControllerEditor editor)
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
            if (_mainEditor.GetSerializedController() != null && _mainEditor.GetController() != null)
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
            _forwardTiresCorneringStiffnessField.bindingPath = nameof(_forwardTiresSO.CorneringStiffness);
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
            if (_mainEditor.GetSerializedController() != null && _mainEditor.GetController() != null)
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
            _rearTiresCorneringStiffnessField.bindingPath = nameof(_rearTiresSO.CorneringStiffness);
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
            defaultTires.CorneringStiffness = 15;
            defaultTires.ForwardGrip = 1.5f;

            AnimationCurve sidewaysGripCurve = new();
            sidewaysGripCurve.AddKey(0, 0.25f);
            sidewaysGripCurve.AddKey(1, 1);
            defaultTires.SidewaysGripCurve = sidewaysGripCurve;


            AnimationCurve sidewaysSlipCurve = new ();
            sidewaysSlipCurve.AddKey(0, 1);
            sidewaysSlipCurve.AddKey(0.95f, 0.5f);
            sidewaysSlipCurve.AddKey(1, 1f);
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

            //_frontTiresObjectField.value = controller == null ? null : controller.VehicleStats.FrontTiresSO;
            //_rearTiresObjectField.value = controller == null ? null : controller.VehicleStats.RearTiresSO;

        }
    }

}
