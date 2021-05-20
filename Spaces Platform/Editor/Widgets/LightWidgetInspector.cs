using UnityEngine;
using UnityEditor;

namespace Spaces.Core
{
    [CustomEditor(typeof(LightWidget))]
    public class LightWidgetInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var widget = target as LightWidget;

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Apply Settings"))
                {
                    widget.UpdateLight();
                }
                if (GUILayout.Button("Refresh Widget"))
                {
                    widget.UpdateWidget();
                }
            }
            EditorGUILayout.EndHorizontal();

            base.OnInspectorGUI();
        }
    }
}