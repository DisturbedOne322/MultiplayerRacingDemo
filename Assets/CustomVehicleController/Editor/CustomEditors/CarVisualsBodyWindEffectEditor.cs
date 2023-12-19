using UnityEngine;
using UnityEditor;
using Assets.VehicleController;

namespace Assets.VehicleControllerEditor
{
    [CustomEditor(typeof(CarVisualsBodyWindEffect))]
    public class CarVisualsBodyWindEffectEditor : Editor
    {
        private SerializedProperty _type;
        private SerializedProperty _visualEffect;
        private SerializedProperty _ps;

        private void OnEnable()
        {
            _type = serializedObject.FindProperty("_bodyWindVisualEffectType");
            _visualEffect = serializedObject.FindProperty("_bodyWindVisualEffectAsset");
            _ps = serializedObject.FindProperty("_bodyWindParticleSystem");
        }


        public override void OnInspectorGUI()
        {
            if (_type == null)
                return;
            EditorGUILayout.PropertyField(_type);

            if (_type.enumValueIndex == (int)VisualEffectAssetType.Type.VisualEffect)
            {
                EditorGUILayout.PropertyField(_visualEffect);
            }
            else
            {
                EditorGUILayout.PropertyField(_ps);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
