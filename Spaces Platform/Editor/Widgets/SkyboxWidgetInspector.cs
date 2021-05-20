
using UnityEngine;
using UnityEditor;

namespace Spaces.Core
{
    [CustomEditor(typeof(SkyboxWidget))]
    public class SkyboxWidgetInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var widget = target as SkyboxWidget;

            if (GUILayout.Button("Apply Settings"))
            {
                widget.ApplySkybox();
            }

            base.OnInspectorGUI();
        }
    }
}