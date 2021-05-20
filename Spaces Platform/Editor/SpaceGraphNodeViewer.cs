using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Spaces.Manager
{
    public class SpaceGraphNodeViewer : EditorWindow
    {
        string gameObjectTitle;
        Vector2 scrollPos;

        Core.Asset asset;
        GameObject rootObject;
        Spaces.Core.Node rootNode;

        //bool[] widgetExpandState;
        Dictionary<Core.Node, bool[]> nodeWidgetExpandState;

        Spaces.Core.AssetWidget assetWidget;

        bool isAsset
        {
            get
            {
                return asset != null;
            }
        }

        [MenuItem("GameObject/View Space Graph", priority = 20)]
        private static void OpenGraphViewer()
        {
            if (Selection.activeGameObject)
            {
                OpenGraphViewer(Selection.activeGameObject);
            }
        }

        public static void OpenGraphViewer(GameObject rootGameObject)
        {
            var window = GetWindow<SpaceGraphNodeViewer>();

            window.rootObject = rootGameObject;
            window.gameObjectTitle = window.rootObject.name;
            window.rootNode = new Core.Node(window.rootObject);

            window.assetWidget = rootGameObject.GetComponent<Spaces.Core.AssetWidget>();
            window.asset = window.assetWidget.Asset;
        }

        public static void OpenGraphViewer(Core.Asset asset)
        {
            var window = GetWindow<SpaceGraphNodeViewer>();
            window.asset = asset;
            window.gameObjectTitle = window.asset.name;

            if (asset.metadata != null)
            {
                window.rootNode = asset.metadata.node != null ? asset.metadata.node : new Core.Node(asset.name);
                window.nodeWidgetExpandState = new Dictionary<Core.Node, bool[]>();
            }
            else
            {
                window.SubscribeToAsset();
            }
        }

        void OnGUI()
        {
            using (var nodeViewerPanel = new EditorGUILayout.VerticalScope())
            {
                using (var assetPanel = new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Name: " + gameObjectTitle);

                    bool wasEnabled = GUI.enabled;
                    GUI.enabled = wasEnabled && isAsset;
                    if (GUILayout.Button("Save", GUILayout.Width(100)))
                    {
                        UpdateAssetMetadata();
                    }
                    GUI.enabled = wasEnabled;

                    if (GUILayout.Button("Refresh", GUILayout.Width(100)))
                    {
                        if (rootObject)
                        {
                            gameObjectTitle = rootObject.name;
                            rootNode = new Core.Node(rootObject);

                            assetWidget = rootObject.GetComponent<Spaces.Core.AssetWidget>();
                            asset = assetWidget.Asset;
                        }
                        else if (asset != null)
                        {
                            asset.RefreshInfo();
                        }
                    }
                }

                using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos, EditorStyles.helpBox))
                //scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox);
                {
                    scrollPos = scrollView.scrollPosition;

                    if (rootNode != null)
                        DrawNodeEditor(rootNode, ref nodeWidgetExpandState);
                }
                //EditorGUILayout.EndScrollView();
            }
        }

        public static void DrawNodeEditor(Spaces.Core.Node node, ref Dictionary<Core.Node, bool[]> widgetExpandStates)
        {
            int labelWidth = 80;
            int attributeWidget = 400;
            Color nodeNameColor = Color.green;

            if (widgetExpandStates == null)
            {
                widgetExpandStates = new Dictionary<Core.Node, bool[]>();

            }

            if (widgetExpandStates.ContainsKey(node))
            {
                if (widgetExpandStates[node] == null)
                    widgetExpandStates[node] = new bool[node.widgetMap.Count + 1];
            }
            else
                widgetExpandStates.Add(node, new bool[node.widgetMap.Count + 1]);

            //public int id;

            //public string name;
            var normalColor = GUI.color;
            var normalBGColor = GUI.backgroundColor;
            GUI.color = nodeNameColor;
            GUI.backgroundColor = nodeNameColor;


            var rect = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                // Spacing for Expander Control
                GUILayout.Label("", GUILayout.Width(8));

                GUILayout.Label("Name:", GUILayout.Width(labelWidth));

                var parentNode = node.parent;
                string lineage = "";

                while(parentNode != null)
                {
                    lineage = parentNode.name + " > " + lineage;
                    parentNode = parentNode.parent;
                }

                GUILayout.Label(lineage, GUILayout.ExpandWidth(false));
                node.name = EditorGUILayout.TextField(node.name, GUILayout.ExpandWidth(true));
            }
            EditorGUILayout.EndHorizontal();

            rect.xMin += 3;
            rect.yMin += 3;

            widgetExpandStates[node][0] = EditorGUI.Foldout(rect, widgetExpandStates[node][0], GUIContent.none);

            GUI.color = normalColor;
            GUI.backgroundColor = normalBGColor;

            if (!widgetExpandStates[node][0])
                return;

            //Instance ID
            node.instanceID = EditorGUILayout.TextField("ID: ", node.instanceID);

            // local Position;
            node.localPosition = EditorGUILayout.Vector3Field("Position:", node.localPosition, GUILayout.Width(attributeWidget));

            // local Rotation;
            var rot = new Vector4(node.localRotation.x, node.localRotation.y, node.localRotation.z, node.localRotation.w);
            rot = EditorGUILayout.Vector4Field("Rotation:", rot, GUILayout.Width(attributeWidget));
            node.localRotation.Set(rot.x, rot.y, rot.z, rot.w);

            // local Scale;
            node.localScale = EditorGUILayout.Vector3Field("Scale:", node.localScale, GUILayout.Width(attributeWidget));

            // widget Map;

            List<Core.NodeWidget> toDelete = null;

            for (int i = 1; i <= node.widgetMap.Count; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    var widget = node.widgetMap[i - 1];

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            if (toDelete == null)
                                toDelete = new List<Core.NodeWidget>();

                            toDelete.Add(widget);
                        }
                        GUILayout.Label("Widget Type:", GUILayout.Width(labelWidth));
                        widget.Type = EditorGUILayout.TextField(widget.Type);
                    }
                    EditorGUILayout.EndHorizontal();

                    widgetExpandStates[node][i] = EditorGUILayout.Foldout(widgetExpandStates[node][i], "Metadata:");

                    if (widgetExpandStates[node][i])
                    {
                        string prettyValue = widget.Value.Replace(",", ",\n");
                        widget.Value = EditorGUILayout.TextArea(prettyValue, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)).Replace("\n", "");
                    }

                }
                EditorGUILayout.EndVertical();
            }

            if (toDelete != null)
                toDelete.ForEach(w => node.widgetMap.Remove(w));

            //public List<Node> children;
            if (node.children != null && node.children.Count > 0)
            {
                GUILayout.Label("Children: ");
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    foreach (var child in node.children)
                    {
                        DrawNodeEditor(child, ref widgetExpandStates);
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void UpdateAssetMetadata()
        {
            if (asset.metadata != null)
                asset.metadata.node = rootNode;

            Core.RestAPI.RestManager.Request.UpdateAssetMetadata(asset.id, asset.metadata, OnUpdataMetadataResponse);
        }

        private void OnUpdataMetadataResponse(bool error, string response)
        {
            if (error)
            {
                Debug.LogError(assetWidget.name + " (" + assetWidget.assetID + ") [Error updating metadata]");
            }
            else
            {
                Debug.Log(assetWidget.name + " (" + assetWidget.assetID + ") [Metadata update successful]");
                asset.SaveMetadataToCache();
            }
        }

        void SubscribeToAsset()
        {
            asset.onProcessEnd += AssetUpdated;
        }

        void UnsubscribeFromAsset()
        {
            asset.onProcessEnd -= AssetUpdated;
        }

        void AssetUpdated(Core.Asset sender, Core.Asset.Process[] processSet, Core.Asset.Process endedProcess)
        {
            if (endedProcess == Core.Asset.Process.FetchingMetadata)
            {
                if (asset.metadata != null)
                {
                    rootNode = asset.metadata.node;
                    nodeWidgetExpandState = new Dictionary<Core.Node, bool[]>();
                }
            }
        }
    }
}