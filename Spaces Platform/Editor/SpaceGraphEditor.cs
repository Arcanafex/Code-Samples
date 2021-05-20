using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Spaces.Manager
{
    public class SpaceGraphEditor : EditorWindow
    {
        string spaceTitle;
        Vector2 scrollPos;
        bool assetsExpanded = false;

        Core.Space space;
        Core.SpaceGraph graph;
        //string metadata;
        List<string> toDelete;
        Dictionary<Core.Node, bool[]> nodeWidgetExpandState;

        bool metadataFormatError;
        bool wasCreateCalled;

        Color regularColor;
        Color errorColor = Color.red;

        [MenuItem("Spaces/Edit Space Graph")]
        private static void OpenGraphEditor()
        {
            OpenGraphEditor(Core.SpaceGraph.Generate());
        }

        public static void OpenGraphEditor(Core.SpaceGraph spaceGraph)
        {
            var window = GetWindow<SpaceGraphEditor>();
            window.spaceTitle = "";
            window.graph = spaceGraph;

            if (EditorApplication.isPlayingOrWillChangePlaymode && UnityClient.UserSession.Instance != null)
                window.space = UnityClient.UserSession.Instance.CurrentSpace;

            window.InitializeGraph();            
        }

        public static void OpenGraphEditor(Core.Space space)
        {
            var window = GetWindow<SpaceGraphEditor>();

            if (space != null)
            {
                window.space = space;
                window.SubscribeToSpace();

                if (space.isGraphFetched)
                {
                    window.graph = space.Graph;
                    window.InitializeGraph();
                }
                else
                {
                    window.space.FetchGraph();
                }                
            }
        }

        void InitializeGraph()
        {
            nodeWidgetExpandState = new Dictionary<Core.Node, bool[]>();

            foreach (var node in graph.nodes)
            {
                nodeWidgetExpandState.Add(node, new bool[node.widgetMap.Count + 1]);
            }
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            {
                GUILayout.Label("");

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Space Name", GUILayout.Width(100));
                    if (space == null)
                    {
                        spaceTitle = EditorGUILayout.TextField(spaceTitle, GUILayout.ExpandWidth(true));

                        bool wasEndabled = GUI.enabled;
                        GUI.enabled = wasEndabled && !wasCreateCalled && !string.IsNullOrEmpty(spaceTitle);
                        if (GUILayout.Button("Create Space", GUILayout.Width(140)))
                        {
                            wasCreateCalled = true;
                            space = Core.Space.Create(spaceTitle);
                        }
                        GUI.enabled = wasEndabled;
                    }
                    else
                    {
                        EditorGUILayout.SelectableLabel(space.name);
                        GUILayout.Label("", GUILayout.ExpandWidth(true));
                    }

                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Space ID", GUILayout.Width(100));
                    EditorGUILayout.SelectableLabel(space == null ? "" : space.id, GUILayout.Height(EditorStyles.label.lineHeight + 1));
                    GUILayout.Label("", GUILayout.ExpandWidth(true));


                }
                EditorGUILayout.EndHorizontal();


                GUILayout.Label("");

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox);
                {
                    if (graph != null)
                    {
                        assetsExpanded = EditorGUILayout.Foldout(assetsExpanded, "Assets:");

                        if (graph.assetIDs != null & assetsExpanded)
                        {
                            for (int i = 0; i < graph.assetIDs.Count; i++)
                            {
                                using (var assetListPanel = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                                {
                                    using (var assetPanel = new EditorGUILayout.HorizontalScope())
                                    {
                                        if (GUILayout.Button("X", GUILayout.Width(20)))
                                        {
                                            if (toDelete == null)
                                                toDelete = new List<string>();

                                            toDelete.Add(graph.assetIDs[i]);
                                        }
                                        //GUILayout.Label("Asset ID:", GUILayout.Width(80));
                                        EditorGUILayout.SelectableLabel(graph.assetIDs[i], GUILayout.Height(EditorStyles.label.lineHeight + 1));
                                    }
                                }
                            }

                            if (toDelete != null)
                            {
                                toDelete.ForEach(id => graph.assetIDs.Remove(id));
                            }
                        }

                        if (graph.nodes != null)
                        {
                            foreach (Core.Node node in graph.nodes)
                            {
                                SpaceGraphNodeViewer.DrawNodeEditor(node, ref nodeWidgetExpandState);
                            }
                        }
                    }

                    //if (nodes != null)
                    //{
                    //    //metadata = EditorGUILayout.TextArea(metadata, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                    //    for (int i = 0; i < nodes.Count; i++)
                    //    {
                    //        nodes[i] = EditorGUILayout.TextArea(nodes[i]);
                    //    }
                    //}
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Refresh", GUILayout.Width(140)))
                    {
                        graph = Core.SpaceGraph.Generate();
                    }

                    GUILayout.Label("", GUILayout.ExpandWidth(true));
                    regularColor = GUI.color;

                    if (metadataFormatError)
                        GUI.color = errorColor;

                    bool wasEnabled = GUI.enabled;
                    GUI.enabled = wasEnabled && space != null && !metadataFormatError;
                    if (GUILayout.Button("Update Space Graph", GUILayout.Width(140)))
                    {
                        if (UpdateSpaceGraph())
                        {
                            metadataFormatError = false;
                        }
                        else
                        {
                            metadataFormatError = true;
                        }
                    }
                    GUI.enabled = wasEnabled;
                    GUI.color = regularColor;

                    if (GUILayout.Button("Close", GUILayout.Width(140)))
                    {
                        Close();
                    }

                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Label("");

            }
            EditorGUILayout.EndVertical();

            // We force a repaint so we can run the httpmanager while in editor mode.
            BestHTTP.HTTPManager.OnUpdate();

            try
            {
                this.Repaint();
            }
            catch (System.Exception ex)
            {
            }
        }

        void SubscribeToSpace()
        {
            space.onProcessEnd += SpaceUpdated;
        }

        void SpaceUpdated(Core.Space sender, Core.Space.Process[] processSet, Core.Space.Process endedProcess)
        {
            if (endedProcess == Core.Space.Process.FetchingGraph)
            {
                graph = sender.Graph;
                //metadata = JsonUtility.ToJson(graph, true);

                InitializeGraph();
            }
        }

        bool UpdateSpaceGraph()
        {
            //bool nodeError = false;

            //graph = JsonUtility.FromJson<Core.SpaceGraph>(metadata);
            //for (int i = 0; i < nodes.Count; i++)
            //{
            //    try
            //    {
            //        JsonUtility.FromJsonOverwrite(nodes[i], graph.nodes[i]);
            //    }
            //    catch (System.Exception ex)
            //    {
            //        nodeError = true;
            //    }
            //}

            //if (nodeError)
            //    return false;

            try
            {
                Core.RestAPI.RestManager.Request.UpdateSpaceMetadata(space.id, JsonUtility.ToJson(graph), OnUpdateMetadataResponse);
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        void OnUpdateMetadataResponse(bool error, Core.RestAPI.RestUpdateSpaceMetadataResponse response)
        {
            if (error)
            {
                Debug.LogError(space.name + " (" + space.id + ") [Error Updating Space]");
            }
            else
            {
                Debug.Log(space.name + " (" + space.id + ") [Space Graph Updated]");
                space.Graph = graph;
                space.SaveGraphToCache();
            }
        }
    }
}