using Assets.VehicleController;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class EngineSettingsEditor : EditorWindow
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _mainEditor;

        #region fields
        private Foldout _engineSettingsFoldout;

        private ObjectField _engineSOObjectField;
        private ObjectField _fiSOObjectField;

        private EngineSO _engineSO;
        private ForcedInductionSettingsEditor _forcedInductionSettingsEditor;

        private CurveField _engineTorqueCurveField;
        private FloatField _engineMaxSpeedField;

        private Label _engineHPLabel;

        private TextField _engineNameTextField;
        #endregion


        #region FieldNames
        private const string ENGINE_FOLDOUT_NAME = "Engine";
        private const string ENGINE_SO_OBJECT_FIELD_NAME = "EngineSOObjectField";
        private const string ENGINE_FI_SO_OBJECT_FIELD_NAME = "EngineForcedInductionSOField";
        private const string ENGINE_TORQUE_CURVE_FIELD_NAME = "EngineTorqueCurveField";
        private const string ENGINE_MAX_SPEED_FIELD_NAME = "EngineMaxSpeedField";
        private const string ENGINE_NAME_FIELD_NAME = "EngineSONameTextField";
        private const string ENGINE_SAVE_BUTTON_NAME = "EngineSaveButton";

        private const string ENGINE_HP_LABEL_NAME = "HorsepowerLabel";
        #endregion

        private const string ENGINE_FOLDER_NAME = "Engines";

        private int _ticks = 0;
        private const int TICKS_TO_UPDATE = 500;

        public void HandleEngineSettings(VisualElement root, CustomVehicleControllerEditor editor, ForcedInductionSettingsEditor forcedInductionSettingsEditor)
        {
            this.root = root;
            _mainEditor = editor;
            _forcedInductionSettingsEditor = forcedInductionSettingsEditor;
            FindEngineFields();

            BindEngineSOField();
            RebindEngineSettings(_engineSO);
            SubscribeToEngineSaveButtonClick();

            RecalculateHorsepower();

            _mainEditor.OnWindowClosed += _mainEditor_OnWindowClosed;
            EditorApplication.update += RecalculateHorsepowerRoutine;

            SetTooltips();
        }

        private void SetTooltips()
        {
            StringBuilder sb1 = new StringBuilder();
            sb1.AppendLine("Engine torque graph.");
            sb1.AppendLine("");
            sb1.AppendLine("X-axis - rpm. Y-axis - torque.");
            sb1.AppendLine("");
            sb1.AppendLine("The leftmost key is the idle rpm, and the rightmost - the max rpm.");
            _engineTorqueCurveField.tooltip = sb1.ToString();

            StringBuilder sb2 = new StringBuilder();

            sb2.AppendLine("The maximum speed a vehicle can reach (in km/h).");
            sb2.AppendLine();
            sb2.AppendLine("In case the car is trying to accelerate beyond this value, the opposite force would be applied.");
            sb2.AppendLine();
            sb2.AppendLine("Keep the value at a reasonable amount because other components may depend on it for calculations.");

            _engineMaxSpeedField.tooltip = sb2.ToString();
        }

        private void _mainEditor_OnWindowClosed()
        {
            var button = root.Q<Button>(name: ENGINE_SAVE_BUTTON_NAME);
            button.clicked -= EngineCreateAssetButton_onClick;
            EditorApplication.update -= RecalculateHorsepowerRoutine;
            _mainEditor.OnWindowClosed -= _mainEditor_OnWindowClosed;
        }

        private void RecalculateHorsepowerRoutine()
        {
            if (!_engineSettingsFoldout.value)
                return;

            _ticks++;
            if (_ticks < TICKS_TO_UPDATE)
                return;

            _ticks = 0;

            RecalculateHorsepower();
        }

        private void RecalculateHorsepower()
        {
            if (_engineSOObjectField.value == null)
            {
                _engineHPLabel.text = "0";
                return;
            }

            _engineHPLabel.text = ((int)_engineSO.FindMaxHP()).ToString();
        }


        private void FindEngineFields()
        {
            _engineSettingsFoldout = root.Q<Foldout>(ENGINE_FOLDOUT_NAME);
            _fiSOObjectField = root.Q<ObjectField>(ENGINE_FI_SO_OBJECT_FIELD_NAME);
            _fiSOObjectField.RegisterValueChangedCallback(evt => _forcedInductionSettingsEditor.UpdateForcedInduction(_fiSOObjectField.value as ForcedInductionSO));
            _engineTorqueCurveField = root.Q<CurveField>(ENGINE_TORQUE_CURVE_FIELD_NAME);
            _engineMaxSpeedField = root.Q<FloatField>(ENGINE_MAX_SPEED_FIELD_NAME);
            _engineMaxSpeedField.RegisterValueChangedCallback(evt => { _engineMaxSpeedField.value = Mathf.Max(0, _engineMaxSpeedField.value); });
            _engineNameTextField = root.Q<TextField>(ENGINE_NAME_FIELD_NAME);
            _engineHPLabel = root.Q<Label>(ENGINE_HP_LABEL_NAME);
        }


        private void BindEngineSOField()
        {
            _engineSOObjectField = root.Q<ObjectField>(ENGINE_SO_OBJECT_FIELD_NAME);

            _engineSOObjectField.RegisterValueChangedCallback(x => RebindEngineSettings(_engineSOObjectField.value as EngineSO));

            if (_engineSOObjectField.value == null)
            {
                _engineSO = CreateDefaultEngine();
            }
            else
            {
                _engineSO = _engineSOObjectField.value as EngineSO;
            }
        }
        private void RebindEngineSettings(EngineSO loadedEngineSO)
        {
            _engineSO = loadedEngineSO;

            if (_mainEditor.GetSerializedController() != null)
            {
                _mainEditor.GetSerializedController().FindProperty(nameof(CustomVehicleController.VehicleStats)).FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.EngineSO)).objectReferenceValue = _engineSO;
                _mainEditor.SaveController();
            }

            if (_engineSO == null)
            {
                _engineSO = CreateDefaultEngine();
            }

            SerializedObject so = new (_engineSO);
            BindForcedInduction(so);
            BindEngineTorqueCurve(so);
            BindMaxSpeedField(so);
            RecalculateHorsepower();
        }
        private void BindForcedInduction(SerializedObject so)
        {
            _fiSOObjectField.bindingPath = nameof(_engineSO.ForcedInductionSO);
            _fiSOObjectField.Bind(so);
            _forcedInductionSettingsEditor.UpdateForcedInduction(_engineSO.ForcedInductionSO);
        }
        private void BindEngineTorqueCurve(SerializedObject so)
        {
            _engineTorqueCurveField.bindingPath = nameof(_engineSO.TorqueCurve);
            _engineTorqueCurveField.Bind(so);
        }
        private void BindMaxSpeedField(SerializedObject so)
        {
            _engineMaxSpeedField.bindingPath = nameof(_engineSO.MaxSpeed);
            _engineMaxSpeedField.Bind(so);
        }

        private EngineSO CreateDefaultEngine()
        {
            EngineSO defaultEngine = ScriptableObject.CreateInstance<EngineSO>();
            defaultEngine.MaxSpeed = 300;
            AnimationCurve torqueCurve = new ();
            torqueCurve.AddKey(1000f, 100f);
            torqueCurve.AddKey(8000f, 160f);
            torqueCurve.AddKey(9000f, 130f);
            int size = torqueCurve.keys.Length;
            for (int i = 0; i < size; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(torqueCurve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(torqueCurve, i, AnimationUtility.TangentMode.Auto);
            }
            defaultEngine.TorqueCurve = torqueCurve;
            return defaultEngine;
        }

        private void SubscribeToEngineSaveButtonClick()
        {
            var button = root.Q<Button>(name: ENGINE_SAVE_BUTTON_NAME);
            button.clicked += EngineCreateAssetButton_onClick;
        }

        private void EngineCreateAssetButton_onClick()
        {
            if (_engineNameTextField.text.ToString() == "")
            {
                Debug.LogWarning("Empty engine SO name");
                return;
            }

            string filePath = _mainEditor.GetVehiclePartsFolderPath(ENGINE_FOLDER_NAME) + "/" + _engineNameTextField.text + ".asset";

            EngineSO newEngine = CreateDefaultEngine();

            var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(filePath);
            AssetDatabase.CreateAsset(newEngine, uniqueFileName);
            AssetDatabase.SaveAssets();

            _engineSO = newEngine;
            _engineSOObjectField.value = _engineSO;
        }

        public void SetVehicleController(SerializedObject so)
        {
            if(so == null)
            {
                _engineSOObjectField.value = null;
                return;
            }

            _engineSOObjectField.value = so.FindProperty(nameof(CustomVehicleController.VehicleStats)).
                    FindPropertyRelative(nameof(CustomVehicleController.VehicleStats.EngineSO)).objectReferenceValue;
        }
    }

}
