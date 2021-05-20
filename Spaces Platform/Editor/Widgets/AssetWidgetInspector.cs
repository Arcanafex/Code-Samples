using UnityEngine;
using UnityEditor;

namespace Spaces.Core
{
    [CustomEditor(typeof(AssetWidget))]
    public class AssetWidgetInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var assetWidget = target as AssetWidget;

            using (var buttonPanel = new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save Settings"))
                {
                    Manager.SpaceGraphNodeViewer.OpenGraphViewer(assetWidget.gameObject);
                }

                if (GUILayout.Button("Reset to Default"))
                {
                    if (assetWidget.Asset != null)
                        assetWidget.Asset.ResetToDefault();
                }

                if (GUILayout.Button("ReLoad"))
                {
                    for (int i = 0; i < assetWidget.transform.childCount; i++)
                    {
                        var child = assetWidget.transform.GetChild(i);

                        if (child.GetComponent<InstancedAssetWidget>())
                            Destroy(child.gameObject);
                    }

                    assetWidget.ReloadAsset();
                }
            }
        }
    }
}