using Assets.VehicleController;
using UnityEditor;

namespace Assets.VehicleControllerEditor
{
    [CustomEditor(typeof(CarVisualsAntiLag))]
    public class CarVisualsAntiLagEditor : Editor
    {
        private SerializedProperty _type;
        private SerializedProperty _visualEffect;
        private SerializedProperty _ps;
        private SerializedProperty _exhaustsTransform;
        private SerializedProperty _currentCarStats;
        private SerializedProperty _backfireDelay;

        private SerializedProperty _minBackfireCount;
        private SerializedProperty _maxBackfireCount;

        private void OnEnable()
        {
            _type = serializedObject.FindProperty("_antiLagVisualEffectType");
            _visualEffect = serializedObject.FindProperty("_antiLagVFXAsset");
            _ps = serializedObject.FindProperty("_antiLagParticleSystem");
            _exhaustsTransform = serializedObject.FindProperty("_exhaustsPositionArray");
            _currentCarStats = serializedObject.FindProperty("_currentCarStats");
            _backfireDelay = serializedObject.FindProperty("_backfireDelay");

            _minBackfireCount = serializedObject.FindProperty("_minBackfireCount");
            _maxBackfireCount = serializedObject.FindProperty("_maxBackfireCount");
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
            EditorGUILayout.PropertyField(_backfireDelay);

            EditorGUILayout.PropertyField(_minBackfireCount);
            EditorGUILayout.PropertyField(_maxBackfireCount);

            EditorGUILayout.PropertyField(_exhaustsTransform);
            EditorGUILayout.PropertyField(_currentCarStats);

            if(_maxBackfireCount.intValue < _minBackfireCount.intValue)
            {
                _maxBackfireCount.intValue = _minBackfireCount.intValue;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
