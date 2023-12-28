using Assets.VehicleController;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    //[CustomPropertyDrawer(typeof(AntiLagParameters))]
    public class CarVisualsAntiLagEditor //:PropertyDrawer
    {
        private SerializedProperty _type;
        private SerializedProperty _visualEffect;
        private SerializedProperty _ps;
        private SerializedProperty _exhaustsTransform;
        private SerializedProperty _currentCarStats;
        private SerializedProperty _backfireDelay;

        private SerializedProperty _minBackfireCount;
        private SerializedProperty _maxBackfireCount;

        //private void OnEnable()
        //{
        //    _type = serializedObject.FindProperty("_antiLagVisualEffectType");
        //    _visualEffect = serializedObject.FindProperty("_antiLagVFXAsset");
        //    _ps = serializedObject.FindProperty("_antiLagParticleSystem");
        //    _exhaustsTransform = serializedObject.FindProperty("_exhaustsPositionArray");
        //    _currentCarStats = serializedObject.FindProperty("_currentCarStats");
        //    _backfireDelay = serializedObject.FindProperty("_backfireDelay");

        //    _minBackfireCount = serializedObject.FindProperty("_minBackfireCount");
        //    _maxBackfireCount = serializedObject.FindProperty("_maxBackfireCount");
        //}
        //private Rect GetRect(Rect position, int number)
        //{
        //    return new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + 3) * number, 200, EditorGUIUtility.singleLineHeight);
        //}

        //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        //{
        //    var style = new GUIStyle(EditorStyles.foldoutHeader);
        //    style.fontStyle = FontStyle.Normal;

        //    if (property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(position, property.isExpanded, "AntiLag Parameters", style))
        //    { 
        //        EditorGUI.indentLevel++;
        //        EditorGUI.PropertyField(GetRect(position, 1), property.FindPropertyRelative("AntiLagVisualEffectType"), GUIContent.none);
        //        if (property.FindPropertyRelative("AntiLagVisualEffectType").enumValueIndex == (int)VisualEffectAssetType.Type.VisualEffect)

        //            EditorGUI.PropertyField(GetRect(position, 2), property.FindPropertyRelative("AntiLagVFXAsset"), GUIContent.none);
        //        else
        //            EditorGUI.PropertyField(GetRect(position, 2), property.FindPropertyRelative("AntiLagParticleSystem"), GUIContent.none);

        //        EditorGUI.indentLevel--;
        //    }

        //    EditorGUI.EndFoldoutHeaderGroup();

        //}

        //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        //{
        //    return EditorGUI.GetPropertyHeight(property, label);
        //}

        //public override void OnInspectorGUI()
        //{
        //    if (_type == null)
        //        return;
        //    EditorGUILayout.PropertyField(_type);

        //    if (_type.enumValueIndex == (int)VisualEffectAssetType.Type.VisualEffect)
        //    {
        //        EditorGUILayout.PropertyField(_visualEffect);
        //    }
        //    else
        //    {
        //        EditorGUILayout.PropertyField(_ps);
        //    }
        //    EditorGUILayout.PropertyField(_backfireDelay);

        //    EditorGUILayout.PropertyField(_minBackfireCount);
        //    EditorGUILayout.PropertyField(_maxBackfireCount);

        //    EditorGUILayout.PropertyField(_exhaustsTransform);
        //    EditorGUILayout.PropertyField(_currentCarStats);

        //    if(_maxBackfireCount.intValue < _minBackfireCount.intValue)
        //    {
        //        _maxBackfireCount.intValue = _minBackfireCount.intValue;
        //    }

        //    serializedObject.ApplyModifiedProperties();
        //}
    }
}
