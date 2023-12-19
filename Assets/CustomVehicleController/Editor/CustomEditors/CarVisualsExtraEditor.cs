using UnityEngine;
using UnityEditor;
using Assets.VehicleController;

namespace Assets.VehicleControllerEditor
{
    [CustomEditor(typeof(CarVisualsExtra))]
    public class CarVisualsExtraEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            CarVisualsExtra carVisualsExtra = (CarVisualsExtra)target;

            if (GUILayout.Button("Copy values from CarVisualsEssentials script"))
            {
                carVisualsExtra.CopyValuesFromEssentials();
            }

            DrawDefaultInspector();
        }
    }
}
