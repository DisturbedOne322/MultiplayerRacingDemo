using Assets.VehicleController;
using UnityEditor;
using UnityEngine;

namespace Assets.VehicleControllerEditor
{
    [CustomEditor(typeof(CustomVehicleControllerExtraSoundManager))]
    public class VehicleExtraSoundManagerEditor : Editor
    {
        private SerializedProperty _vehicleController;

        private SerializedProperty _extraSoundSO;
        private CarExtraSoundsSO _extraSound;

        private SerializedProperty _forcedInductionSoundSO;
        private CarForcedInductionSoundSO _forcedInductionSound;

        private SerializedProperty _vehicleSoundAudioMixerGroup;

        private SerializedProperty _tireVolumeIncreaseTime;

        private SerializedProperty _antiLagSoundCooldown;

        private SerializedProperty _forcedInductionMaxPitch;
        private SerializedProperty _forcedInductionMaxVolume;

        private SerializedProperty _maxWindVolume;
        private SerializedProperty _speedForMaxWindVolume;

        private SerializedProperty _nitroVolumeGainSpeedInSeconds;
        private SerializedProperty _reverbZone;
        private SerializedProperty _reverbDuringNitroPreset;

        private SerializedProperty _collisionHandler;

        private void OnEnable()
        {
            _vehicleController = serializedObject.FindProperty("_vehicleController");

            _extraSoundSO = serializedObject.FindProperty("_extraSoundSO");
            _extraSound = _extraSoundSO.objectReferenceValue as CarExtraSoundsSO;

            _forcedInductionSoundSO = serializedObject.FindProperty("_forcedInductionSoundSO");
            _forcedInductionSound = _forcedInductionSoundSO.objectReferenceValue as CarForcedInductionSoundSO;

            _vehicleSoundAudioMixerGroup = serializedObject.FindProperty("_vehicleSoundAudioMixerGroup");

            _tireVolumeIncreaseTime = serializedObject.FindProperty("_tireVolumeIncreaseTime");
            _antiLagSoundCooldown = serializedObject.FindProperty("_antiLagSoundCooldown");
            _forcedInductionMaxPitch = serializedObject.FindProperty("_forcedInductionMaxPitch");
            _forcedInductionMaxVolume = serializedObject.FindProperty("_forcedInductionMaxVolume");

            _maxWindVolume = serializedObject.FindProperty("_maxWindVolume");
            _speedForMaxWindVolume = serializedObject.FindProperty("_speedForMaxWindVolume");

            _nitroVolumeGainSpeedInSeconds = serializedObject.FindProperty("_nitroVolumeGainSpeedInSeconds");

            _reverbZone = serializedObject.FindProperty("_reverbZone");
            _reverbDuringNitroPreset = serializedObject.FindProperty("_reverbDuringNitroPreset");

            _collisionHandler = serializedObject.FindProperty("_collisionHandler");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_vehicleController);
            EditorGUILayout.PropertyField(_forcedInductionSoundSO);
            EditorGUILayout.PropertyField(_extraSoundSO);
            EditorGUILayout.PropertyField(_vehicleSoundAudioMixerGroup);

            if(_forcedInductionSound != null)
            {
                if (_forcedInductionSound.AntiLagMildSounds.Length != 0 || _forcedInductionSound.AntiLagSound.Length != 0)
                    EditorGUILayout.PropertyField(_antiLagSoundCooldown);

                if (_forcedInductionSound.ForcedInductionSound.length != 0)
                {
                    EditorGUILayout.PropertyField(_forcedInductionMaxPitch);
                    EditorGUILayout.PropertyField(_forcedInductionMaxVolume);
                }
            }

            if(_extraSound != null)
            {
                if(_extraSound.TireSlipSound != null)
                    EditorGUILayout.PropertyField(_tireVolumeIncreaseTime);

                if(_extraSound.WindNoise != null)
                {
                    EditorGUILayout.PropertyField(_maxWindVolume);
                    EditorGUILayout.PropertyField(_speedForMaxWindVolume);
                }

                if (_extraSound.NitroContinuous != null)
                {
                    EditorGUILayout.PropertyField(_nitroVolumeGainSpeedInSeconds);
                    EditorGUILayout.PropertyField(_reverbZone);
                }

                if (_reverbZone.objectReferenceValue != null)
                    EditorGUILayout.PropertyField(_reverbDuringNitroPreset);

                if (_extraSound.CollisionImpact != null || _extraSound.CollisionContinuous != null)
                    EditorGUILayout.PropertyField(_collisionHandler);
            }

            _forcedInductionSound = _forcedInductionSoundSO.objectReferenceValue as CarForcedInductionSoundSO;
            _extraSound = _extraSoundSO.objectReferenceValue as CarExtraSoundsSO;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
