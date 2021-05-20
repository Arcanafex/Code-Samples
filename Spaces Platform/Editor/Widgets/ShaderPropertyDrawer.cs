using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Spaces.Core
{
    [CustomPropertyDrawer(typeof(ShaderInterface.ShaderProperty))]
    public class ShaderPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            EditorGUI.BeginProperty(position, label, property);
            {
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(property.FindPropertyRelative("name").stringValue));

                // Don't make child fields be indented
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                string propertyName = "";
                string propertyType = "";

                switch ((ShaderInterface.ShaderPropertyType)property.FindPropertyRelative("type").enumValueIndex)
                {
                    case ShaderInterface.ShaderPropertyType.Float:
                        propertyType = ShaderInterface.ShaderPropertyType.Float.ToString();
                        break;
                    case ShaderInterface.ShaderPropertyType.Color:
                        propertyType = ShaderInterface.ShaderPropertyType.Color.ToString();
                        break;
                    case ShaderInterface.ShaderPropertyType.Vector:
                        propertyType = ShaderInterface.ShaderPropertyType.Vector.ToString();
                        break;
                    case ShaderInterface.ShaderPropertyType.Range:
                        propertyName = "range";
                        propertyType = ShaderInterface.ShaderPropertyType.Range.ToString();
                        break;
                    case ShaderInterface.ShaderPropertyType.TexEnv:
                        propertyName = "texDim";
                        propertyType = ShaderInterface.ShaderPropertyType.TexEnv.ToString();
                        break;

                }

                // Calculate rects
                var labelRect = new Rect(position.x, position.y, 60, position.height);
                var dataRect = new Rect(position.x + labelRect.width + 5, position.y, position.width - 65, position.height);

                if (!string.IsNullOrEmpty(propertyType))
                    EditorGUI.LabelField(labelRect, new GUIContent(propertyType));

                if (!string.IsNullOrEmpty(propertyName))
                    EditorGUI.PropertyField(dataRect, property.FindPropertyRelative(propertyName), GUIContent.none);


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

    [CustomPropertyDrawer(typeof(MaterialWidget.MaterialFloatProperty))]
    [CustomPropertyDrawer(typeof(MaterialWidget.MaterialRangeProperty))]
    [CustomPropertyDrawer(typeof(MaterialWidget.MaterialColorProperty))]
    [CustomPropertyDrawer(typeof(MaterialWidget.MaterialVectorProperty))]
    public class MaterialPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            {
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(property.FindPropertyRelative("name").stringValue));

                // Don't make child fields be indented
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                // Calculate rects
                var labelRect = new Rect(position.x, position.y, 40, position.height);
                var dataRect = new Rect(position.x + 40, position.y, position.width - 40, position.height);

                switch ((ShaderInterface.ShaderPropertyType)property.FindPropertyRelative("type").enumValueIndex)
                {
                    case ShaderInterface.ShaderPropertyType.Float:
                        EditorGUI.PropertyField(dataRect, property.FindPropertyRelative("value"), GUIContent.none);
                        break;
                    case ShaderInterface.ShaderPropertyType.Color:
                        EditorGUI.PropertyField(dataRect, property.FindPropertyRelative("value"), GUIContent.none);
                        break;
                    case ShaderInterface.ShaderPropertyType.Vector:
                        EditorGUI.PropertyField(dataRect, property.FindPropertyRelative("value"), GUIContent.none);
                        break;
                    case ShaderInterface.ShaderPropertyType.Range:
                        property.FindPropertyRelative("value").floatValue = EditorGUI.Slider(dataRect, property.FindPropertyRelative("value").floatValue, property.FindPropertyRelative("range.min").floatValue, property.FindPropertyRelative("range.max").floatValue);
                        break;
                    case ShaderInterface.ShaderPropertyType.TexEnv:
                    default:
                        break;

                }

                // Set indent back to what it was
                EditorGUI.indentLevel = indent;
            }
            EditorGUI.EndProperty();

        }
    }

    [CustomPropertyDrawer(typeof(MaterialWidget.MaterialTextureProperty))]
    public class MaterialTexturePropertyDrawer : PropertyDrawer
    {
        private const int ROWCOUNT = 5; 

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            {
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(property.FindPropertyRelative("name").stringValue));

                // Don't make child fields be indented
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;


                float row = position.height / ROWCOUNT;

                // Calculate rects
                var assetLabelRect = new Rect(position.x, position.y, 55, row);
                var assetRect = new Rect(position.x + 60, position.y, position.width - 60, row);

                var wrapLabelRect = new Rect(position.x, position.y + row, 35, row);
                var wrapRect = new Rect(position.x + 40, position.y + row, (position.width / 2) - 40, row);
                var anisoLabelRect = new Rect(position.x + (position.width / 2), position.y + row, 35, row);
                var anisoRect = new Rect(position.x + 40 + (position.width / 2), position.y + row, position.width - 40, row);


                var tilingLabelRect = new Rect(position.x, position.y + (row * 2), 35, row);
                var tilingRect = new Rect(position.x + 40, position.y + (row * 2), position.width - 40, row);

                var offsetLabelRect = new Rect(position.x, position.y + (row * 3), 35, row);
                var offsetRect = new Rect(position.x + 40, position.y + (row * 3), position.width - 40, row);

                MaterialWidget.MaterialTextureProperty.SourceMode modeIndex = (MaterialWidget.MaterialTextureProperty.SourceMode)property.FindPropertyRelative("mode").enumValueIndex;
                modeIndex = (MaterialWidget.MaterialTextureProperty.SourceMode)EditorGUI.EnumPopup(assetLabelRect, modeIndex);
                property.FindPropertyRelative("mode").enumValueIndex = (int)modeIndex;

                if (modeIndex == MaterialWidget.MaterialTextureProperty.SourceMode.Asset)
                {
                    EditorGUI.PropertyField(assetRect, property.FindPropertyRelative("assetId"), GUIContent.none);
                }
                else if (modeIndex == MaterialWidget.MaterialTextureProperty.SourceMode.URL)
                {
                    EditorGUI.PropertyField(assetRect, property.FindPropertyRelative("url"), GUIContent.none);
                }

                EditorGUI.LabelField(wrapLabelRect, new GUIContent("Wrap Mode"));
                TextureWrapMode wrapMode = (TextureWrapMode)property.FindPropertyRelative("wrapMode").enumValueIndex;
                wrapMode = (TextureWrapMode)EditorGUI.EnumPopup(wrapRect, wrapMode);
                property.FindPropertyRelative("wrapMode").enumValueIndex = (int)wrapMode;

                //EditorGUI.LabelField(anisoLabelRect, new GUIContent("Aniso"));
                //property.FindPropertyRelative("anisoLevel").intValue = EditorGUI.Slider(anisoRect, property.FindPropertyRelative("anisoLevel").intValue, 1, 9);

                EditorGUI.LabelField(tilingLabelRect, new GUIContent("Tiling"));
                EditorGUI.PropertyField(tilingRect, property.FindPropertyRelative("tiling"), GUIContent.none);

                EditorGUI.LabelField(offsetLabelRect, new GUIContent("Offset"));
                EditorGUI.PropertyField(offsetRect, property.FindPropertyRelative("offset"), GUIContent.none);

                // Set indent back to what it was
                EditorGUI.indentLevel = indent;
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * ROWCOUNT;
        }
    }
}
