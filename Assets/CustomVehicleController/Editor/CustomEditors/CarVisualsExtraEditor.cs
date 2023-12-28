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
        #region coll
        private SerializedProperty _enableCollisionEffects;
        private SerializedProperty _collisionEffectsParameters;
        #endregion

        private void OnEnable()
        {
            FindTireSmoke();
            FindTireTrail();
            FindBrakeLights();
            FindBodyAero();
            FindWingAero();
            FindAntiLagProperties();
            FindColl();
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

            HandleSmoke();
            HandleTireTrails();
            HandleBrakeLights();
            HandleAntiLag();
            HandleBodyAero();
            HandleWingAero();
            HandleCollisions();

            serializedObject.ApplyModifiedProperties();
        }

        private void DefaultInspector()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_currentCarStats"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_rigidbody"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_wheelMeshes"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_wheelControllerArray"));
        }

        private void HandleSmoke()
        {
            EditorGUILayout.PropertyField(_enableTireSmoke);

            if (_enableTireSmoke.boolValue)
            {
                EditorGUILayout.PropertyField(_tireSmokeParameters);
                EditorGUILayout.Space();
            }
        }

        private void HandleTireTrails()
        {
            EditorGUILayout.PropertyField(_enableTireTrails);

            if (_enableTireTrails.boolValue)
            {
                EditorGUILayout.PropertyField(_tireTrailParameters);
                EditorGUILayout.Space();
            }
        }

        private void HandleBrakeLights()
        {
            EditorGUILayout.PropertyField(_enableBrakeLightsEffect);

            if (_enableBrakeLightsEffect.boolValue)
            {
                EditorGUILayout.PropertyField(_brakeLightsParameters);
                EditorGUILayout.Space();
            }
        }

        private void HandleBodyAero()
        {
            EditorGUILayout.PropertyField(_enableBodyAeroEffect);

            if (_enableBodyAeroEffect.boolValue)
            {
                EditorGUILayout.PropertyField(_bodyEffectParameters);
                EditorGUILayout.Space();
            }
        }

        private void HandleWingAero()
        {
            EditorGUILayout.PropertyField(_enableWingAeroEffect);

            if (_enableWingAeroEffect.boolValue)
            {
                EditorGUILayout.PropertyField(_wingAeroParameters);
                EditorGUILayout.Space();
            }
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
        }

        private void HandleCollisions()
        {
            EditorGUILayout.PropertyField(_enableCollisionEffects);

            if (_enableCollisionEffects.boolValue)
            {
                EditorGUILayout.PropertyField(_collisionEffectsParameters);
            }
        }
    }
}
