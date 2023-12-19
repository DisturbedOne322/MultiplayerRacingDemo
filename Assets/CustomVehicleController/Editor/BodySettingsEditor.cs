using Assets.VehicleController;
using System.Threading;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal.VR;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class BodySettingsEditor : EditorWindow
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _mainEditor;

        #region body fields
        private VehicleBodySO _bodySO;

        private ObjectField _bodyObjectField;
        private FloatField _massField;
        private FloatField _forwardDragField;
        private FloatField _midAirDragField;
        private FloatField _downforceField;
        private FloatField _corneringResistanceField;
        private CurveField _corneringResistanceCurve;
        private TextField _bodyNameField;
        #endregion

        #region body field names
        private const string BODY_OBJECT_FIELD = "BodyObjectField";
        private const string BODY_MASS_FIELD = "MassField";
        private const string BODY_FORWARD_DRAG_FIELD = "ForwardDragField";
        private const string BODY_MID_AIR_DRAG_FIELD = "MidAirDragField";
        private const string BODY_DOWNFORCE_FIELD = "DownforceField";
        private const string BODY_CORNERING_RES_FIELD = "CorneringResistanceField";
        private const string BODY_CORNERING_RES_CURVE = "CorneringResistanceCurve";


        private const string BODY_NAME_FIELD = "BodyNameField";
        private const string BODY_CREATE_BUTTON_NAME = "CreateNewBodyButton";
        #endregion

        private const string BODY_FOLDER_NAME = "VehicleBodies";

        public void HandleBodySettings(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _mainEditor = editor;
            FindBodyFields();
            BindBodySOField();
            RebindBodySettings(_bodySO);
            SubscribeToBodySaveButtonClick();
            _mainEditor.OnWindowClosed += Editor_OnWindowClosed;
        }

        private void Editor_OnWindowClosed()
        {
            var button = root.Q<Button>(name: BODY_CREATE_BUTTON_NAME);
            button.clicked -= BodyCreateAssetButton_onClick;
            _mainEditor.OnWindowClosed -= Editor_OnWindowClosed;
        }

        private void FindBodyFields()
        {
            _massField = root.Q<FloatField>(BODY_MASS_FIELD);
            _massField.RegisterValueChangedCallback(evt => { _massField.value = Mathf.Max(1, _massField.value); });

            _forwardDragField = root.Q<FloatField>(BODY_FORWARD_DRAG_FIELD);
            _forwardDragField.RegisterValueChangedCallback(evt => { _forwardDragField.value = Mathf.Max(0, _forwardDragField.value); });

            _midAirDragField = root.Q<FloatField>(BODY_MID_AIR_DRAG_FIELD);
            _midAirDragField.RegisterValueChangedCallback(evt => { _midAirDragField.value = Mathf.Max(0, _midAirDragField.value); });

            _downforceField = root.Q<FloatField>(BODY_DOWNFORCE_FIELD);
            _downforceField.RegisterValueChangedCallback(evt => { _downforceField.value = Mathf.Max(0, _downforceField.value); });

            _corneringResistanceField = root.Q<FloatField>(BODY_CORNERING_RES_FIELD);
            _corneringResistanceField.RegisterValueChangedCallback(evt => { _corneringResistanceField.value = Mathf.Max(0, _corneringResistanceField.value); });

            _corneringResistanceCurve = root.Q<CurveField>(BODY_CORNERING_RES_CURVE);
            _corneringResistanceCurve.RegisterValueChangedCallback(evt => ClampGraph(_corneringResistanceCurve.value));

            _bodyNameField = root.Q<TextField>(BODY_NAME_FIELD);
        }

        private void ClampGraph(AnimationCurve curve)
        {
            Keyframe[] keys = curve.keys;
            int size = keys.Length;
            for (int i = 0; i < size; i++)
            {
                keys[i].time = Mathf.Clamp01(keys[i].time);
            }
        }

        private void BindBodySOField()
        {
            _bodyObjectField = root.Q<ObjectField>(BODY_OBJECT_FIELD);

            _bodyObjectField.RegisterValueChangedCallback(x => RebindBodySettings(_bodyObjectField.value as VehicleBodySO));

            if (_bodyObjectField.value == null)
            {
                _bodySO = CreateDefaultBody();
            }
            else
            {
                _bodySO = _bodyObjectField.value as VehicleBodySO;
            }
        }
        private void RebindBodySettings(VehicleBodySO loadedBodySO)
        {
            _bodySO = loadedBodySO;
            if (_mainEditor.GetSerializedController() != null && _mainEditor.GetController() != null)
            {
                _mainEditor.GetSerializedController().FindProperty(nameof(CustomVehicleController.VehicleStats)).FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.BodySO)).objectReferenceValue = _bodySO;
                _mainEditor.SaveController();
            }
            if (_bodySO == null)
            {
                _bodySO = CreateDefaultBody();
            }

            SerializedObject so = new (_bodySO);
            BindBodyMasFields(so);
            BindBodyDownforceField(so);
            BindBodyCorneringResField(so);
            BindResCurve(so);
            BindForwardDragField(so);
            BindMidAirDragField(so);
        }

        private void BindBodyMasFields(SerializedObject so)
        {
            _massField.bindingPath = nameof(_bodySO.Mass);
            _massField.Bind(so);
        }
        private void BindBodyDownforceField(SerializedObject so)
        {
            _downforceField.bindingPath = nameof(_bodySO.Downforce);
            _downforceField.Bind(so);
        }
        private void BindBodyCorneringResField(SerializedObject so)
        {
            _corneringResistanceField.bindingPath = nameof(_bodySO.CorneringResistanceStrength);
            _corneringResistanceField.Bind(so);
        }
        private void BindForwardDragField(SerializedObject so)
        {
            _forwardDragField.bindingPath = nameof(_bodySO.ForwardDrag);
            _forwardDragField.Bind(so);
        }
        private void BindMidAirDragField(SerializedObject so)
        {
            _midAirDragField.bindingPath = nameof(_bodySO.MidAirDrag);
            _midAirDragField.Bind(so);
        }
        private void BindResCurve(SerializedObject so)
        {
            _corneringResistanceCurve.bindingPath = nameof(_bodySO.CorneringResistanceCurve);
            _corneringResistanceCurve.Bind(so);
        }


        private VehicleBodySO CreateDefaultBody()
        {
            VehicleBodySO defaultBodySO = ScriptableObject.CreateInstance<VehicleBodySO>();
            defaultBodySO.Mass = 1500;
            defaultBodySO.ForwardDrag = 0.07f;
            defaultBodySO.CorneringResistanceStrength = 5f;
            defaultBodySO.Downforce = 5;

            AnimationCurve curve = new ();
            curve.AddKey(0f, 0f);
            curve.AddKey(1f, 1f);
            defaultBodySO.CorneringResistanceCurve = curve;

            return defaultBodySO;
        }

        private void SubscribeToBodySaveButtonClick()
        {
            var button = root.Q<Button>(name: BODY_CREATE_BUTTON_NAME);
            button.clicked += BodyCreateAssetButton_onClick;
        }

        private void BodyCreateAssetButton_onClick()
        {
            if (_bodyNameField.text.ToString() == "")
            {
                Debug.LogWarning("Empty body SO name");
                return;
            }

            string filePath = _mainEditor.GetVehiclePartsFolderPath(BODY_FOLDER_NAME) + "/" + _bodyNameField.text + ".asset";

            VehicleBodySO copy = CreateDefaultBody();

            var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(filePath);
            AssetDatabase.CreateAsset(copy, uniqueFileName);
            AssetDatabase.SaveAssets();

            _bodySO = copy;
            _bodyObjectField.value = _bodySO;
        }

        public void SetVehicleController(SerializedObject so)
        {
            if (so == null)
            {
                _bodyObjectField.value = null;
                return;
            }

            _bodyObjectField.value = so.FindProperty(nameof(CustomVehicleController.VehicleStats)).FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.BodySO)).objectReferenceValue;
            //_bodyObjectField.value = controller == null ? null : controller.VehicleStats.BodySO;
        }
    }

}
