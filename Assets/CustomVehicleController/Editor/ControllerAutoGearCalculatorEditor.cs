using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Linq;
using System.Text;
using Assets.VehicleController;
using UnityEngine;
using System.Buffers.Text;

namespace Assets.VehicleControllerEditor
{
    public class ControllerAutoGearCalculatorEditor
    {
        private VisualElement root;

        #region Auto Gear Ratio Calculator
        private Toggle _autoCalculateToggle;
        private Slider _gearRatioChangeRateSlider;
        private FloatField _firstGearRatioField;
        private IntegerField _gearAmountField;
        private VisualElement _calculatorParentElement;

        private const string TOGGLE_NAME = "AutoCalculateGearsToggle";
        private const string GEAR_RATIO_CHANGE_SLIDER_NAME = "GearRatioChangeRateSlider";
        private const string FIRST_GEAR_FIELD_NAME = "GearRatioChangeRateSlider";
        private const string GEAR_AMOUNT_FIELD_NAME = "GearAmountField";
        private const string PARENT_NAME = "CalculateGearsMenu";
        #endregion

        public ControllerAutoGearCalculatorEditor(VisualElement _root)
        {
            root = _root;
            FindFields();
        }


        private void FindFields()
        {
            _autoCalculateToggle = root.Q<Toggle>(TOGGLE_NAME);
            _autoCalculateToggle.RegisterValueChangedCallback(evt => _calculatorParentElement.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);

            _gearRatioChangeRateSlider = root.Q<Slider>(GEAR_RATIO_CHANGE_SLIDER_NAME);
            _firstGearRatioField = root.Q<FloatField>(FIRST_GEAR_FIELD_NAME);
            _firstGearRatioField.RegisterValueChangedCallback(evt => {
                _firstGearRatioField.value =
                evt.newValue < 1 ? 1 : evt.newValue; });
            _gearAmountField = root.Q<IntegerField>(GEAR_AMOUNT_FIELD_NAME);
            _gearAmountField.RegisterValueChangedCallback(evt =>
            {
                _gearAmountField.value = evt.newValue < 1? 1 : evt.newValue;
            });
            _calculatorParentElement = root.Q<VisualElement>(PARENT_NAME);

            SetTooltip();
        }

        private void SetTooltip()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Controls the rate of change in gear ratios.");
            sb.AppendLine("");
            sb.AppendLine("A more negative value results in a faster decrease in gear ratios as the index increases.");
            sb.AppendLine("");
            sb.AppendLine("Recommended value [-0.25: -0.3]");

            _gearRatioChangeRateSlider.tooltip = sb.ToString();
        }

        public float[] CalculateGearArray()
        {
            int size = _gearAmountField.value;
            float[] array = new float[size];

            float changeRate = _gearRatioChangeRateSlider.value;

            float firstGearRatio = _firstGearRatioField.value;

            array[0] = _firstGearRatioField.value;

            for(int i = 1; i < size; i++)
            {
                array[i] = firstGearRatio * Mathf.Exp(changeRate * i);
            }

            return array;
        }

        public float GetChangeRate() => _autoCalculateToggle.value ? _gearRatioChangeRateSlider.value : 0;
    }
}
