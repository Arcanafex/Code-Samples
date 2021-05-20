using UnityEngine;
using UnityEditor;

namespace Spaces.Core
{
    [CustomEditor(typeof(ParticleWidget))]
    public class ParticleWidgetInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var widget = target as ParticleWidget;


            using (var buttons = new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Apply Settings"))
                {
                    widget.UpdateParticleSystem();
                }

                if (GUILayout.Button("Refresh Widget"))
                {
                    widget.UpdateWidget();
                }
            }

            base.OnInspectorGUI();
        }
    }
}
