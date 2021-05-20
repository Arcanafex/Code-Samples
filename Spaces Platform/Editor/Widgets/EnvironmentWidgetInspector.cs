using UnityEngine;
using UnityEditor;

namespace Spaces.Core
{
    [CustomEditor(typeof(EnvironmentWidget))]
    public class EnvironmentWidgetInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var widget = target as EnvironmentWidget;

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Apply Settings"))
                {
                    widget.UpdateEnvironment();
                }

                if (GUILayout.Button("Reset To Defaults"))
                {
                    widget.ResetToDefaults();
                }
            }
            EditorGUILayout.EndHorizontal();

            base.OnInspectorGUI();
        }
    }
}