using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Spaces.Manager
{
    public class SelectAssetDialog : EditorWindow
    {
        //T1 m_selection;
        Core.Space selectedSpace;

        //List<T2> selectedItems;
        //List<T2> items;
        List<Core.Asset> assets;
        List<Core.Asset> selectedAssets;

        int progCount = 0;
        System.DateTime start;
        string progress = "";

        Vector2 scrollPos;

        //delegate void ProcessSelection<T>(List<T> selection);
        //ProcessSelection<T2> processSelection;

        #region Constants
        private const int ASSET_NAME_WIDTH = 260;
        private const int ASSET_TYPE_WIDTH = 120;
        private const int ASSET_ID_WIDTH = 280;
        private const int ASSET_BUTTON_WIDTH = 100;
        private const int ASSET_WIDE_BUTTON_WIDTH = 140;
        private const int ASSET_EDIT_PANEL_MIN_HEIGHT = 100;

        private static readonly Color BG_COLOR_EVEN = new Color(0.6f, 0.6f, 0.6f);
        private static readonly Color BG_COLOR_ODD = new Color(0.8f, 0.8f, 0.8f);
        #endregion

        public static void Init(Core.Space selectedObject, List<Core.Asset> itemList)//, ProcessSelection<T2> selectionProcess = null)
        {
            var window = GetWindow<SelectAssetDialog>("Select Assets");

            window.position = new Rect(Screen.width / 2, Screen.height / 2, 800, 600);
            window.selectedSpace = selectedObject;
            window.assets = itemList;
            window.selectedAssets = new List<Core.Asset>();
            //window.processSelection = selectionProcess;

            window.ShowPopup();
        }

        void OnGUI()
        {
            DrawAssetList();

            // We force a repaint so we can run the httpmanager while in editor mode. It totally works, sweet.
            BestHTTP.HTTPManager.OnUpdate();

            try
            {
                this.Repaint();
            }
            catch (System.Exception ex)
            {

            }
        }

        void DrawAssetList()
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Asset Name", EditorStyles.label, GUILayout.Width(ASSET_NAME_WIDTH)))
                {
                    if (assets != null && assets.Count > 0)
                    {
                        assets.Sort((a1, a2) => a1.name.CompareTo(a2.name));
                    }
                }

                if (GUILayout.Button("Asset Type", EditorStyles.label, GUILayout.Width(ASSET_TYPE_WIDTH)))
                {
                    if (assets != null && assets.Count > 0)
                    {
                        assets.Sort((a1, a2) => a1.assetType.CompareTo(a2.assetType));
                    }
                }

                if (GUILayout.Button("Asset ID", EditorStyles.label, GUILayout.Width(ASSET_ID_WIDTH)))
                {
                    if (assets != null && assets.Count > 0)
                    {
                        assets.Sort((a1, a2) => a1.id.CompareTo(a2.id));
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox);
            {
                EditorGUILayout.BeginVertical();
                {
                    Color bgColor = GUI.backgroundColor;
                    Color guiColor = GUI.color;

                    if (assets == null)
                        assets = new List<Core.Asset>();

                    for (int i = 0; i < assets.Count; i++)
                    {
                        bool ignore = selectedSpace != null && selectedSpace.assetIDs != null && selectedSpace.assetIDs.Contains(assets[i].id);

                        GUI.backgroundColor = (i % 2 == 0 ? BG_COLOR_EVEN : BG_COLOR_ODD);

                        if (selectedAssets.Contains(assets[i]))
                            GUI.color = Color.green;
                        else if (ignore)
                            GUI.color = Color.grey;
                        else
                            GUI.color = guiColor;

                        var rowRect = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        {
                            if (GUI.Button(rowRect, GUIContent.none, EditorStyles.helpBox))
                            {
                                Debug.Log("Asset clicked in Asset Selection List!!!!!!!");
                                if (!selectedAssets.Contains(assets[i]) && !ignore)
                                    selectedAssets.Add(assets[i]);
                                else
                                    selectedAssets.Remove(assets[i]);
                            }

                            GUILayout.Label(!string.IsNullOrEmpty(assets[i].name) ? assets[i].name : "...", GUILayout.Width(ASSET_NAME_WIDTH));
                            GUILayout.Label(!string.IsNullOrEmpty(assets[i].assetType) ? assets[i].assetType : "...", GUILayout.Width(ASSET_TYPE_WIDTH));
                            EditorGUILayout.SelectableLabel(assets[i].id, GUILayout.Width(ASSET_ID_WIDTH), GUILayout.Height(EditorStyles.label.lineHeight + 1));
                            //DrawStatus(assets[i].GetStatusMessage());

                            GUI.backgroundColor = bgColor;
                            GUI.color = guiColor;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("");

                if (GUILayout.Button("Add", GUILayout.Width(ASSET_BUTTON_WIDTH)))
                {
                    if (selectedAssets != null && selectedAssets.Count > 0)
                    {
                        var addAssets = selectedAssets.ToArray();

                        foreach (var asset in addAssets)
                        {
                            if (selectedAssets.Remove(asset))
                            {
                                selectedSpace.AddAsset(asset);
                            }
                        }
                    }
                }

                if (GUILayout.Button("Close", GUILayout.Width(ASSET_BUTTON_WIDTH)))
                {
                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("");
        }
    
    }
}