using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Spaces.Core
{
    [CustomPropertyDrawer(typeof(ShaderInterface.ShaderRangeProperty))]
    public class RangePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            //base.OnGUI(position, property, label);
            EditorGUI.BeginProperty(position, label, property);
            {
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                // Don't make child fields be indented
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                float min = property.FindPropertyRelative("min").floatValue;
                float max = property.FindPropertyRelative("max").floatValue;

                // Calculate rects
                var minRect = new Rect(position.x, position.y, 35, position.height / 2);
                var minMaxRect = new Rect(position.x + minRect.width + 5, position.y, position.width - 80, position.height / 2);
                var maxRect = new Rect(position.x + minRect.width + minMaxRect.width + 10, position.y, 35, position.height / 2);
                var defRect = new Rect(position.x, position.y + (position.height / 2), position.width, position.height / 2);

                min = EditorGUI.FloatField(minRect, min);
                EditorGUI.MinMaxSlider(minMaxRect, ref min, ref max, 0, 10);
                max = EditorGUI.FloatField(maxRect, max);

                property.FindPropertyRelative("min").floatValue = min;
                property.FindPropertyRelative("max").floatValue = max;

                property.FindPropertyRelative("def").floatValue = EditorGUI.Slider(defRect, property.FindPropertyRelative("def").floatValue, min, max);



                // Set indent back to what it was
                EditorGUI.indentLevel = indent;
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2;
        }
    }
}
