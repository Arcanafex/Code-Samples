using UnityEngine;
using UnityEditor;

namespace Spaces.Core
{
    [CustomEditor(typeof(MaterialWidget))]
    public class MaterialWidgetInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var materialWidget = target as MaterialWidget;

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Apply Settings"))
                {
                    materialWidget.UpdateMaterial();
                }
                if (GUILayout.Button("Refresh Widget"))
                {
                    materialWidget.UpdateWidgetState();
                }
            }
            EditorGUILayout.EndHorizontal();

            base.OnInspectorGUI();
        }
    }
}