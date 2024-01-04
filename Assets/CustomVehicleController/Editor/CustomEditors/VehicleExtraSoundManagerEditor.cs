using Assets.VehicleController;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

namespace Assets.VehicleControllerEditor
{
    [CustomEditor(typeof(CustomVehicleControllerExtraSoundManager))]
    public class VehicleExtraSoundManagerEditor : Editor
    {
        private SerializedProperty _vehicleController;

        private SerializedProperty _extraSoundSO;
        private CarExtraSoundsSO _extraSound;

        private SerializedProperty _vehicleSoundAudioMixerGroup;

        private SerializedProperty _tireVolumeIncreaseTime;

        private SerializedProperty _antiLagSoundCooldown;

        private SerializedProperty _forcedInductionMaxPitch;
        private SerializedProperty _forcedInductionMaxVolume;

        private SerializedProperty _maxWindVolume;

        private void OnEnable()
        {
            _vehicleController = serializedObject.FindProperty("_vehicleController");

            _extraSoundSO = serializedObject.FindProperty("_extraSoundSO");
            _extraSound = _extraSoundSO.objectReferenceValue as CarExtraSoundsSO;

            _vehicleSoundAudioMixerGroup = serializedObject.FindProperty("_vehicleSoundAudioMixerGroup");

            _tireVolumeIncreaseTime = serializedObject.FindProperty("_tireVolumeIncreaseTime");
            _antiLagSoundCooldown = serializedObject.FindProperty("_antiLagSoundCooldown");
            _forcedInductionMaxPitch = serializedObject.FindProperty("_forcedInductionMaxPitch");
            _forcedInductionMaxVolume = serializedObject.FindProperty("_forcedInductionMaxVolume");

            _maxWindVolume = serializedObject.FindProperty("_maxWindVolume");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_vehicleController);
            EditorGUILayout.PropertyField(_extraSoundSO);
            EditorGUILayout.PropertyField(_vehicleSoundAudioMixerGroup);

            if(_extraSound != null)
            {
                if(_extraSound.AntiLagMildSounds.Length != 0 || _extraSound.AntiLagSound.Length != 0)
                    EditorGUILayout.PropertyField(_antiLagSoundCooldown);

                if(_extraSound.ForcedInductionSound.length != 0)
                {
                    EditorGUILayout.PropertyField(_forcedInductionMaxPitch);
                    EditorGUILayout.PropertyField(_forcedInductionMaxVolume);
                }

                if(_extraSound.TireSlipSound != null)
                    EditorGUILayout.PropertyField(_tireVolumeIncreaseTime);

                if(_extraSound.WindNoise != null)
                    EditorGUILayout.PropertyField(_maxWindVolume);
            }
            _extraSound = _extraSoundSO.objectReferenceValue as CarExtraSoundsSO;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
