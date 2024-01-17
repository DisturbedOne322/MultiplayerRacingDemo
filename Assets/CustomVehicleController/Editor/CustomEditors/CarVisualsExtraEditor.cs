using UnityEngine;
using UnityEditor;
using Assets.VehicleController;
using System.Runtime.InteropServices;
using UnityEngine.VFX;

namespace Assets.VehicleControllerEditor
{
    [CustomEditor(typeof(CarVisualsExtra))]
    public class CarVisualsExtraEditor : Editor
    {
        #region smoke
        private SerializedProperty _enableTireSmoke;
        private SerializedProperty _tireSmokeParameters;
        #endregion
        #region trails
        private SerializedProperty _enableTireTrails;
        private SerializedProperty _tireTrailParameters;
        #endregion
        #region brake lights
        private SerializedProperty _enableBrakeLightsEffect;
        private SerializedProperty _brakeLightsParameters;
        #endregion
        #region body aero
        private SerializedProperty _enableBodyAeroEffect;
        private SerializedProperty _bodyEffectParameters;
        #endregion
        #region wing aero
        private SerializedProperty _enableWingAeroEffect;
        private SerializedProperty _wingAeroParameters;
        #endregion
        #region anti lag
        private SerializedProperty _enableAntiLagEffect;
        private SerializedProperty _antiLagParameters;
        private SerializedProperty _antiLagMinCount;
        private SerializedProperty _antiLagMaxCount;
        #endregion

        #region nitro
        private SerializedProperty _enableNitroEffect;
        private SerializedProperty _nitroParameters;
        #endregion

        #region coll
        private SerializedProperty _enableCollisionEffects;
        private SerializedProperty _collisionEffectsParameters;
        #endregion

        private SerializedProperty Separator;

        private void OnEnable()
        {
            FindTireSmoke();
            FindTireTrail();
            FindBrakeLights();
            FindBodyAero();
            FindWingAero();
            FindAntiLagProperties();
            FindNitro();
            FindColl();
            FindSeparators();
        }

        private void FindSeparators()
        {
            Separator = serializedObject.FindProperty("Separator");
        }

        private void FindTireSmoke()
        {
            _enableTireSmoke = serializedObject.FindProperty("_enableTireSmoke");
            _tireSmokeParameters = serializedObject.FindProperty("_tireSmokeParameters");
        }

        private void FindTireTrail()
        {
            _enableTireTrails = serializedObject.FindProperty("_enableTireTrails");
            _tireTrailParameters = serializedObject.FindProperty("_tireTrailParameters");
        }

        private void FindBrakeLights()
        {
            _enableBrakeLightsEffect = serializedObject.FindProperty("_enableBrakeLightsEffect");
            _brakeLightsParameters = serializedObject.FindProperty("_brakeLightsParameters");
        }

        private void FindWingAero()
        {
            _enableWingAeroEffect = serializedObject.FindProperty("_enableWingAeroEffect");
            _wingAeroParameters = serializedObject.FindProperty("_wingAeroParameters");
        }

        private void FindBodyAero()
        {
            _enableBodyAeroEffect = serializedObject.FindProperty("_enableBodyAeroEffect");
            _bodyEffectParameters = serializedObject.FindProperty("_bodyEffectParameters");
        }

        private void FindNitro()
        {
            _enableNitroEffect = serializedObject.FindProperty("_enableNitroEffect");
            _nitroParameters = serializedObject.FindProperty("_nitroParameters"); 
        }

        private void FindAntiLagProperties()
        {
            _enableAntiLagEffect = serializedObject.FindProperty("_enableAntiLagEffect");
            _antiLagParameters = serializedObject.FindProperty("_antiLagParameters");
            _antiLagMinCount = _antiLagParameters.FindPropertyRelative(nameof(AntiLagParameters.MinBackfireCount));
            _antiLagMaxCount = _antiLagParameters.FindPropertyRelative(nameof(AntiLagParameters.MaxBackfireCount));
        }

        private void FindColl()
        {
            _enableCollisionEffects = serializedObject.FindProperty("_enableCollisionEffects");
            _collisionEffectsParameters = serializedObject.FindProperty("_collisionEffectsParameters");
        }

        public override void OnInspectorGUI()
        {
            CarVisualsExtra carVisualsExtra = (CarVisualsExtra)target;
            if (carVisualsExtra == null)
                return;

            if (GUILayout.Button("Copy values from CarVisualsEssentials script"))
            {
                carVisualsExtra.CopyValuesFromEssentials();
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_carVisualsEssentials"));
            
            DefaultInspector();

            DrawEffectSettings(_enableTireSmoke, _tireSmokeParameters);
            DrawEffectSettings(_enableTireTrails, _tireTrailParameters);
            DrawEffectSettings(_enableBrakeLightsEffect, _brakeLightsParameters);

            HandleAntiLag();

            DrawEffectSettings(_enableNitroEffect, _nitroParameters);
            DrawEffectSettings(_enableBodyAeroEffect, _bodyEffectParameters);
            DrawEffectSettings(_enableWingAeroEffect, _wingAeroParameters);
            DrawEffectSettings(_enableCollisionEffects, _collisionEffectsParameters);

            serializedObject.ApplyModifiedProperties();
        }

        private void DefaultInspector()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_currentCarStats"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_rigidbody"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_axleArray"));
        }

        private void DrawEffectSettings(SerializedProperty boolProperty, SerializedProperty parametersProperty)
        {
            EditorGUILayout.PropertyField(boolProperty);

            if(boolProperty.boolValue)
            {
                EditorGUILayout.PropertyField(parametersProperty);
                EditorGUILayout.Space();
            }

            EditorGUILayout.PropertyField(Separator);
        }
       
        private void HandleAntiLag()
        {
            EditorGUILayout.PropertyField(_enableAntiLagEffect);

            if (_enableAntiLagEffect.boolValue)
            {
                EditorGUILayout.PropertyField(_antiLagParameters);
                EditorGUILayout.Space();
            }

            if (_antiLagMaxCount.intValue < _antiLagMinCount.intValue)
            {
                _antiLagMaxCount.intValue = _antiLagMinCount.intValue;
            }
            EditorGUILayout.PropertyField(Separator);

        }
    }
}
