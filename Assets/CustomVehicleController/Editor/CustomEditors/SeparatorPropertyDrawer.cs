using Assets.VehicleController;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    [CustomPropertyDrawer(typeof(Separator))]
    public class SeparatorPropertyDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            Handles.color = Color.gray;
            Handles.DrawAAPolyLine(
                Texture2D.whiteTexture,
                2,
                new Vector3(position.x, position.y, 0),
                new Vector3(position.size.x, position.y, 0));
        }
    }
}
