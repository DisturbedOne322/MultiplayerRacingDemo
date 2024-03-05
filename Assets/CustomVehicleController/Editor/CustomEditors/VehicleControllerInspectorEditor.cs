using Assets.VehicleController;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.VehicleControllerEditor
{
    [CustomEditor(typeof(CustomVehicleController))]
    public class VehicleControllerInspectorEditor : Editor
    {
        private SerializedProperty UsePreset;
        private SerializedProperty _vehiclePartsPreset;
        private SerializedProperty _customizableSet;
        private SerializedProperty Separator;

        private void OnEnable()
        {
            UsePreset = serializedObject.FindProperty("UsePreset");
            _vehiclePartsPreset = serializedObject.FindProperty("_vehiclePartsPreset");
            _customizableSet = serializedObject.FindProperty("_customizableSet");
            Separator = serializedObject.FindProperty("Separator");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(UsePreset);

            if (UsePreset.boolValue)
                EditorGUILayout.PropertyField(_vehiclePartsPreset);
            else
                EditorGUILayout.PropertyField(_customizableSet);

            EditorGUILayout.PropertyField(Separator);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DrivetrainType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TransmissionType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_steerAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_steerSpeed"));


            EditorGUILayout.PropertyField(Separator);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_forwardSlippingThreshold"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_sidewaysSlippingThreshold"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("AerialControlsEnabled"));
            if (serializedObject.FindProperty("AerialControlsEnabled").boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AerialControlsSensitivity"));

            EditorGUILayout.PropertyField(Separator);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_rigidbody"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_centerOfMass"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_suspensionSimulationPrecision"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_ignoreLayers"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_frontAxles"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_rearAxles"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_steerAxles"));

            EditorGUILayout.PropertyField(Separator);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CurrentCarStats"));

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}
