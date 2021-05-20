using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace Spaces.Manager
{
    public struct StatusMessage
    {
        public string statusMessage;
        public bool progressing;
    }

    public class SpaceManagerWindow : EditorWindow
    {
        public enum State
        {
            Error = -1,
            Idle = 0,
            FetchingLists = 1,
            CreatingSpace,
            DeletingSpace,
            AddingAsset,
            RemovingAsset
        }

        Vector2 scrollPosSpaces;
        Vector2 scrollPosSpaceAssets;
        Vector2 scrollPosAssets;

        bool spacesExpanded;
        bool assetsExpanded;
        bool createSpace;
        string createSpaceName;
        string updatedSpaceName;

        //bool veryifyDeletes;
        List<int> verifyDeleteList;

        int selectedSpaceIndex;

        Spaces.Core.Space selectedSpace;
        List<Core.Space> spaces;
        List<Spaces.Core.Asset> assets;

        State state;
        Core.StatusMessage statusMessage;
        int progCount = 0;
        System.DateTime start;
        string progress = "";

        bool spacesUpdated;
        bool assetsUpdated;

        #region Constants
        private const int SPACE_NAME_WIDTH = 260;
        private const int SPACE_ID_WIDTH = 280;
        private const int SPACE_BUTTON_WIDTH = 100;
        private const int SPACE_WIDE_BUTTON_WIDTH = 140;

        private const int SPACE_ASSET_PANEL_MIN_HEIGHT = 200;

        private const int ASSET_NAME_WIDTH = 260;
        private const int ASSET_TYPE_WIDTH = 120;
        private const int ASSET_ID_WIDTH = 280;
        private const int ASSET_BUTTON_WIDTH = 100;

        private static readonly Color BG_COLOR_EVEN = new Color(0.6f, 0.6f, 0.6f);
        private static readonly Color BG_COLOR_ODD = new Color(0.8f, 0.8f, 0.8f);
        #endregion

        [MenuItem("Spaces/Space Manager", false, 1)]
        static void Init()
        {
            var window = GetWindow<SpaceManagerWindow>("Manage Spaces");
            window.minSize = new Vector2(1080, 400);
            window.Show();
            window.Focus();

            window.spacesExpanded = true;
            window.verifyDeleteList = new List<int>();

            window.selectedSpaceIndex = -1;

            var token = Core.RestAPI.SpacesPlatformServicesSettings.Settings;//SelectPlatformServicesSettingsDialog.GetEditorToken();

            if (token)
            {
                var SessionSettings = CreateInstance<UnityClient.SessionSettings>();
                SessionSettings.spacesPlatformSettiings = token;
                SessionSettings.InitializePlatformServiceSettings();
            }

            window.RefreshSpacesList();

            window.spacesUpdated = false;
            window.assetsUpdated = false;
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    spacesExpanded = EditorGUILayout.Foldout(spacesExpanded, "Spaces");

                    if (spacesExpanded)
                    {
                        if (GUILayout.Button("Refresh", GUILayout.Width(SPACE_BUTTON_WIDTH)))
                        {
                            Core.RestAPI.SpacesPlatformServicesSettings.Refresh();
                            RefreshSpacesList();
                        }

                        Color guiColor = GUI.color;
                        GUI.color = Color.green;

                        if (GUILayout.Button("Create Space", GUILayout.Width(SPACE_WIDE_BUTTON_WIDTH)))
                        {
                            CreateSpaceDialog.CreateSpace();
                        }

                        GUI.color = guiColor;
                    }

                }
                EditorGUILayout.EndHorizontal();

                if (spacesExpanded)
                {
                    DrawSpacesPanel();
                }

                DrawStatus(statusMessage);
            }
            EditorGUILayout.EndVertical();

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

        private void OnInspectorUpdate()
        {
            if (state == State.FetchingLists)
            {
                if (selectedSpace != null)
                {
                    if (spacesUpdated && selectedSpace.Assets.Count > 0)
                    {
                        spacesUpdated = false;
                        spaces.ForEach(s => s.onProcessEnd += SpaceUpdated);
                    }

                    if (assetsUpdated && selectedSpace.Assets.Count > 0)
                    {
                        assetsUpdated = false;
                        if (selectedSpace != null)
                        {
                            foreach (var asset in selectedSpace.Assets)
                            {
                                asset.RefreshInfo();
                                asset.onProcessEnd += AssetUpdated;
                            }
                        }
                    }
                }

                if ((spaces != null && spaces.Count > 0 && !spaces.Any(s => s.InProcess(Core.Space.Process.Fetching))) && (assets != null && assets.Count > 0 && assets.All(a => a.InProcess(Core.Asset.Process.Idle))))
                {
                    UpdateState(State.Idle);
                }
            }
            else
            if (state == State.CreatingSpace)
            {
                if (spaces != null && !spaces.Any(s => s.InProcess(Core.Space.Process.Creating)))
                {
                    UpdateState(State.Idle);
                }
            }

        }


        void DrawSpacesPanel()
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Space Name", EditorStyles.label, GUILayout.Width(SPACE_NAME_WIDTH)))
                {
                    if (spaces != null && spaces.Count > 0)
                    {
                        spaces.Sort((s1, s2) => s1.name.CompareTo(s2.name));
                    }
                }

                if (GUILayout.Button("Space ID", EditorStyles.label, GUILayout.Width(SPACE_ID_WIDTH)))
                {
                    if (spaces != null && spaces.Count > 0)
                    {
                        spaces.Sort((s1, s2) => s1.id.CompareTo(s2.id));
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            scrollPosSpaces = EditorGUILayout.BeginScrollView(scrollPosSpaces);
            {
                if (spaces != null && spaces.Count > 0)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        int deleteSpace = -1;
                        Color currentBGColor = GUI.backgroundColor;
                        Color currentColor = GUI.color;

                        for (int i = 0; i < spaces.Count; i++)
                        {
                            GUI.backgroundColor = (i % 2 == 0 ? BG_COLOR_EVEN : BG_COLOR_ODD);

                            if (selectedSpaceIndex == i)
                            {
                                GUI.color = Color.green;
                            }

                            var rowRect = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                            {
                                rowRect.width -= (SPACE_BUTTON_WIDTH * 2);

                                if (GUI.Button(rowRect, GUIContent.none, EditorStyles.helpBox))
                                {
                                    if (selectedSpaceIndex == i)
                                    {
                                        UnselectSpace(i);
                                    }
                                    else
                                    {
                                        SelectSpace(i);
                                    }
                                }

                                GUILayout.Label(spaces[i].name, GUILayout.Width(SPACE_NAME_WIDTH));
                                EditorGUILayout.SelectableLabel(spaces[i].id, GUILayout.Width(SPACE_ID_WIDTH), GUILayout.Height(EditorStyles.label.lineHeight + 1));

                                DrawStatus(spaces[i].GetStatusMessage());

                                GUI.backgroundColor = currentBGColor;

                                if (GUILayout.Button("Load Space", GUILayout.Width(SPACE_BUTTON_WIDTH)))
                                {
                                    LoadSpaceInEditor(spaces[i]);
                                }

                                bool wasEnabled = GUI.enabled;
                                GUI.enabled = wasEnabled && !verifyDeleteList.Contains(i);
                                if (GUILayout.Button("Delete", GUILayout.Width(SPACE_BUTTON_WIDTH)))
                                {
                                    if (!verifyDeleteList.Contains(i))
                                        verifyDeleteList.Add(i);
                                }
                                GUI.enabled = wasEnabled;
                            }
                            EditorGUILayout.EndHorizontal();

                            if (verifyDeleteList.Contains(i))
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.LabelField("");
                                    if (GUILayout.Button("Verify Delete", GUILayout.Width(SPACE_WIDE_BUTTON_WIDTH)))
                                    {
                                        deleteSpace = i;
                                        verifyDeleteList.Remove(i);
                                        UnselectSpace(i);
                                    }
                                    if (GUILayout.Button("Cancel", GUILayout.Width(SPACE_WIDE_BUTTON_WIDTH)))
                                    {
                                        verifyDeleteList.Remove(i);
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                            }

                            // Draws assets associated with this Space
                            if (selectedSpaceIndex == i)
                            {
                                DrawAssociatedAssetsPanel();
                            }

                            GUI.color = currentColor;
                        }


                        if (deleteSpace > -1 && deleteSpace < spaces.Count)
                        {
                            var spaceToDelete = spaces[deleteSpace];

                            if (spaces.Remove(spaces[deleteSpace]))
                            {
                                spaceToDelete.Delete();
                            }
                        }

                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        void SelectSpace(int index)
        {
            if (selectedSpaceIndex == index)
            {
                UnselectSpace(index);
            }
            else
            {
                selectedSpaceIndex = index;
                updatedSpaceName = spaces[selectedSpaceIndex].name;

                Debug.Log("Selected Index: " + selectedSpaceIndex);
                selectedSpace = spaces[selectedSpaceIndex];
                selectedSpace.Refresh();
                //selectedSpace.Assets.ForEach(asset => asset.Refresh());
            }
        }

        void UnselectSpace(int index)
        {
            if (selectedSpaceIndex == index)
            {
                selectedSpaceIndex = -1;
                selectedSpace = null;
                updatedSpaceName = "";
            }
        }

        void UnselectSpace()
        {
            selectedSpaceIndex = -1;
            selectedSpace = null;
            updatedSpaceName = "";
        }

        void LoadSpaceInEditor(Core.Space space)
        {
            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
                EditorApplication.isPlaying = true;
            }

            if (!FindObjectOfType<UnityClient.UserSession>())
                Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Spaces SDK/Examples/Prefabs/User Session [Debug].prefab"));

            //space.Enter();
            CreatePortal(space);
        }

        void CreatePortal(Core.Space space)
        {
            var portal = new GameObject("Portal (" + space.id + ")");
            var portalWidget = portal.AddComponent<Core.PortalWidget>();
            portalWidget.generateHotspot = false;
            portalWidget.Initialize(space);
            portalWidget.LoadOnStart = true;
        }

        void DrawAssociatedAssetsPanel()
        {
            if (selectedSpace != null && selectedSpace.assetIDs != null)
            {
                Color bgColor = GUI.backgroundColor;

                GUI.backgroundColor = BG_COLOR_EVEN;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinHeight(SPACE_ASSET_PANEL_MIN_HEIGHT));
                {
                    int removeAsset = -1;

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Space Name", GUILayout.Width(SPACE_BUTTON_WIDTH));
                        updatedSpaceName = GUILayout.TextField(updatedSpaceName);

                        bool wasEnabled = GUI.enabled;
                        GUI.enabled = wasEnabled && updatedSpaceName != selectedSpace.name;
                        if (GUILayout.Button("Update Name", GUILayout.Width(SPACE_WIDE_BUTTON_WIDTH)))
                        {
                            selectedSpace.Update(updatedSpaceName, "");
                        }
                        GUI.enabled = wasEnabled;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Space ID", GUILayout.Width(SPACE_BUTTON_WIDTH));
                        EditorGUILayout.SelectableLabel(selectedSpace.id, GUILayout.Width(ASSET_ID_WIDTH), GUILayout.Height(EditorStyles.label.lineHeight + 1));
                        GUILayout.Label("", GUILayout.ExpandWidth(true));

                        if (GUILayout.Button("Edit Space Graph", GUILayout.Width(SPACE_WIDE_BUTTON_WIDTH)))
                        {
                            SpaceGraphEditor.OpenGraphEditor(selectedSpace);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Asset Name", GUILayout.Width(ASSET_NAME_WIDTH));
                        GUILayout.Label("Asset Type", GUILayout.Width(ASSET_TYPE_WIDTH));
                        GUILayout.Label("Asset ID", GUILayout.Width(ASSET_ID_WIDTH));
                        GUILayout.Label("");

                        if (GUILayout.Button("Add Assets", GUILayout.Width(SPACE_WIDE_BUTTON_WIDTH)))
                        {
                            SelectAssetDialog.Init(selectedSpace, assets);
                        }

                        //TODO: Add a Remove All button.
                    }
                    EditorGUILayout.EndHorizontal();

                    scrollPosSpaceAssets = EditorGUILayout.BeginScrollView(scrollPosSpaceAssets, EditorStyles.helpBox);
                    {
                        if (selectedSpace.Assets != null && selectedSpace.Assets.Count > 0)
                        {
                            for (int j = 0; j < selectedSpace.Assets.Count; j++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    if (string.IsNullOrEmpty(selectedSpace.Assets[j].name))
                                        DrawStatus(new Core.StatusMessage() { statusMessage = "", progressing = true });//selectedSpace.Assets[j].GetStatusMessage());
                                    else
                                        GUILayout.Label(selectedSpace.Assets[j].name, GUILayout.Width(ASSET_NAME_WIDTH));

                                    GUILayout.Label(selectedSpace.Assets[j].assetType, GUILayout.Width(ASSET_TYPE_WIDTH));
                                    EditorGUILayout.SelectableLabel(selectedSpace.assetIDs[j], GUILayout.Width(ASSET_ID_WIDTH), GUILayout.Height(EditorStyles.label.lineHeight + 1));

                                    if (GUILayout.Button("Remove"))
                                    {
                                        removeAsset = j;
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        else
                        {
                            for (int j = 0; j < selectedSpace.assetIDs.Count; j++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label(!string.IsNullOrEmpty(assets[j].name) ? assets[j].name : "...", GUILayout.Width(ASSET_NAME_WIDTH));
                                    GUILayout.Label(!string.IsNullOrEmpty(assets[j].assetType) ? assets[j].assetType : "...", GUILayout.Width(ASSET_TYPE_WIDTH));
                                    EditorGUILayout.SelectableLabel(selectedSpace.assetIDs[j], GUILayout.Width(ASSET_ID_WIDTH), GUILayout.Height(EditorStyles.label.lineHeight + 1));

                                    if (GUILayout.Button("Remove"))
                                    {
                                        removeAsset = j;
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }

                        if (removeAsset > -1)
                        {
                            var assetToRemove = selectedSpace.Assets[removeAsset];

                            if (selectedSpace.Assets.Remove(selectedSpace.Assets[removeAsset]))
                            {
                                selectedSpace.RemoveAsset(assetToRemove);
                            }
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = bgColor;
            }

        }
        
        private void AssetUpdated(Core.Asset sender, Core.Asset.Process[] lastState, Core.Asset.Process newState)
        {
            if (newState == Core.Asset.Process.Deleting)
                sender.onProcessEnd -= AssetUpdated;
        }

        private void SpaceUpdated(Core.Space sender, Core.Space.Process[] processSet, Core.Space.Process endedProcess)
        {
            if (endedProcess == Core.Space.Process.Fetching)
            {
                assetsUpdated = true;                
            }

            if (endedProcess == Core.Space.Process.Deleting)
                sender.onProcessEnd -= SpaceUpdated;

            if (endedProcess == Core.Space.Process.FetchingGraph)
            {
                if (selectedSpace != null && selectedSpace.isGraphFetched)
                {
                    SpaceGraphEditor.OpenGraphEditor(selectedSpace.Graph);
                }
            }
        }

        void UpdateState(State newState)
        {
            state = newState;
            progress = "";
            progCount = 0;

            switch (state)
            {
                case State.Error:
                    statusMessage = new Core.StatusMessage()
                    {
                        statusMessage = "Something has gone wrong. :(",
                        progressing = false
                    };
                    break;
                case State.Idle:
                    statusMessage = new Core.StatusMessage()
                    {
                        statusMessage = "",
                        progressing = false
                    };
                    break;
                case State.FetchingLists:
                    statusMessage = new Core.StatusMessage()
                    {
                        statusMessage = "Updating Spaces/Assets lists",
                        progressing = true
                    };
                    break;
                case State.CreatingSpace:
                    statusMessage = new Core.StatusMessage()
                    {
                        statusMessage = "Creating new Space",
                        progressing = true
                    };
                    break;
                case State.AddingAsset:
                    statusMessage = new Core.StatusMessage()
                    {
                        statusMessage = "Adding Asset to selected Space",
                        progressing = true
                    };
                    break;
                case State.DeletingSpace:
                    statusMessage = new Core.StatusMessage()
                    {
                        statusMessage = "Deleting the selected Space",
                        progressing = true
                    };
                    break;
            }
        }

        void DrawStatus(Core.StatusMessage status)
        {
            if (System.DateTime.Now.Subtract(start).TotalMilliseconds > 300)
            {
                start = System.DateTime.Now;
                progCount = (++progCount) % 4;
                progress = "";

                for (int i = 0; i < progCount; i++)
                {
                    progress += " .";
                }
            }

            if (status.progressing)
                EditorGUILayout.LabelField(status.statusMessage + progress);
            else
                EditorGUILayout.LabelField(status.statusMessage);
        }

        public void RefreshSpacesList()
        {
            spacesUpdated = true;
            UpdateState(State.FetchingLists);
            selectedSpaceIndex = -1;
            selectedSpace = null;

            if (verifyDeleteList == null)
                verifyDeleteList = new List<int>();
            else
                verifyDeleteList.Clear();

            spaces = Spaces.Core.Space.GetSpaceList(OnSpacesListReceived);
        }

        void OnSpacesListReceived(bool error, Core.RestAPI.RestGetSpaceListResponseData response)
        {
            if (error)
            {

            }
            else
            {
                spaces.ForEach(s => s.onProcessEnd += SpaceUpdated);
                Core.Space.SpacesManager.SaveToCache();
            }
        }
    }
}