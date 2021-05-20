using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Spaces.Manager
{
    public class AssetManagerWindow : EditorWindow
    {

        public enum State
        {
            Error = -1,
            Idle = 0,
            FetchingAssets = 1,
            BuildingBundles,
            CreatingAsset,
        }

        private class AssetData
        {
            public string updateAssetName;
            public int updateAssetTypeIndex;
            public int updateBundleIndex;
            public string updatedResourcePath;
        }

        private static class AssetIconDictionary
        {
            private static Dictionary<string, Texture> assetTypeIcons;

            private const string IMAGE = "Assets/_Spaces SDK/Editor/Icons/Icons_Image_BW.png";
            private const string VIDEO = "Assets/_Spaces SDK/Editor/Icons/Icons_Video_BW.png";
            private const string AUDIO = "Assets/_Spaces SDK/Editor/Icons/Icons_Sound_BW.png";
            private const string MODEL = "Assets/_Spaces SDK/Editor/Icons/Icons_Model_BW.png";
            private const string MATERIAL = "Assets/_Spaces SDK/Editor/Icons/Icons_Material_BW.png";
            private const string SHADER = "Assets/_Spaces SDK/Editor/Icons/Icons_Shader_BW.png";
            private const string TEMPLATE = "Assets/_Spaces SDK/Editor/Icons/Icons_Template_BW.png";

            public static Texture Icon(string type)
            {
                if (assetTypeIcons == null)
                    assetTypeIcons = new Dictionary<string, Texture>();

                if (assetTypeIcons.ContainsKey(type))
                {
                    return assetTypeIcons[type];
                }
                else
                {
                    Texture icon = EditorGUIUtility.whiteTexture;

                    switch (type)
                    {
                        case "image":
                            icon = LoadIcon(IMAGE);
                            assetTypeIcons.Add(type, icon);
                            return icon;
                        case "video":
                            icon = LoadIcon(VIDEO);
                            assetTypeIcons.Add(type, icon);
                            return icon;
                        case "audio":
                            icon = LoadIcon(AUDIO);
                            assetTypeIcons.Add(type, icon);
                            return icon;
                        case "model":
                            icon = LoadIcon(MODEL);
                            assetTypeIcons.Add(type, icon);
                            return icon;
                        case "material":
                            icon = LoadIcon(MATERIAL);
                            assetTypeIcons.Add(type, icon);
                            return icon;
                        case "shader":
                            icon = LoadIcon(SHADER);
                            assetTypeIcons.Add(type, icon);
                            return icon;
                        case "template":
                            icon = LoadIcon(TEMPLATE);
                            assetTypeIcons.Add(type, icon);
                            return icon;
                        default:
                            return icon;
                    }

                }
            }

            private static Texture LoadIcon(string path)
            {
                var thumbTex = (Texture2D)AssetDatabase.LoadAssetAtPath<Texture>(path);
                return thumbTex;
            }
        }

        Vector2 scrollPosAssets;
        Rect viewport;
        //Vector2 scrollPosBundles;

        bool assetsExpanded;
        //bool bundlesExpanded;
        bool createAsset;
        bool reupload;
        //bool externalBundle;

        //bool veryifyDeletes;
        List<int> verifyDeleteList;

        List<Spaces.Core.Asset> assets;
        //List<AvailableBundle> bundles;

        static bool _fetching;

        private class ThumbFetch
        {
            public WWW www;
            public Core.Asset asset;

            public ThumbFetch(Core.Asset asset)
            {
                this.asset = asset;
                Debug.Log(asset.name + " (" + asset.id + ") " + this.asset.previewUrl);
                www = new WWW(asset.previewUrl);

                Start();
            }

            public static void Start()
            {
                if (!_fetching && thumbs.Count > 0)
                {
                    Debug.Log("Starting");
                    _fetching = true;
                    EditorApplication.update += Tick;
                }
            }

            public static void Tick()
            {
                var finishedJobs = new List<ThumbFetch>();

                foreach (var job in thumbs)
                {
                    if (job.www.isDone)
                    {
                        if (!string.IsNullOrEmpty(job.www.error))
                        {
                            Debug.LogError("[Thumbnail Download Error] " + job.www.error);
                        }
                        else
                        {
                            if (job.asset.assetType == "image")
                            {
                                job.asset.WriteDataToCache(job.www.bytes);
                                job.asset.TryLoadPreviewFromCache();
                            }
                            else
                            {
                                job.asset.thumb = job.www.texture;

                                if (job.asset.thumb)
                                {
                                    job.asset.CachePreview();
                                }
                            }
                        }

                        job.www.Dispose();
                        finishedJobs.Add(job);
                    }
                }

                finishedJobs.ForEach(job => thumbs.Remove(job));

                if (thumbs.Count == 0)
                    Stop();
            }

            public static void Stop()
            {
                if (_fetching)
                {
                    Debug.Log("Stopping");
                    EditorApplication.update -= Tick;
                    _fetching = false;
                }
            }


        }

        static List<ThumbFetch> thumbs;
        Dictionary<Core.Asset, AssetData> assetData;

        string assetName;
        string assetPath;

        int deleteAssetIndex;

        int assetTypeIndex;
        List<string> assetTypeList;
        string[] otherAssetTypes = { "fbx", "jpg", "jpeg" };

        delegate void managerStateChanged(State lastState, State newState);
        event managerStateChanged onStateChanged;

        #region Constants
        private const int BUNDLE_NAME_WIDTH = 260;
        private const int BUNDLE_FILE_SIZE_WIDTH = 100;
        private const int BUNDLE_BUTTON_WIDTH = 140;
        private const int BUNDLE_CREATION_FIELD_WIDTH = 160;

        private const int ASSET_THUMBNAIL = 64;
        private const int ASSET_NAME_WIDTH = 260;
        private const int ASSET_TYPE_WIDTH = 120;
        private const int ASSET_ID_WIDTH = 280;
        private const int ASSET_BUTTON_WIDTH = 100;
        private const int ASSET_WIDE_BUTTON_WIDTH = 140;
        private const int ASSET_EDIT_PANEL_MIN_HEIGHT = 100;

        private static readonly Color BG_COLOR_EVEN = new Color(0.6f, 0.6f, 0.6f);
        private static readonly Color BG_COLOR_ODD = new Color(0.8f, 0.8f, 0.8f);
        #endregion


        // Status messaging stuff
        State state;
        int progCount = 0;
        DateTime start;
        string progress = "";
        bool showProgress;
        string statusMessage;
        //bool bundlesUpdated;

        [MenuItem("Spaces/Asset Manager", false, 1)]
        static void Init()
        {
            var window = GetWindow<AssetManagerWindow>("Manage Assets");
            window.minSize = new Vector2(1080, 400);
            window.Show();
            window.Focus();

            window.assetsExpanded = true;
            //window.bundlesExpanded = false;
            window.state = State.Idle;

            //window.GetAvailableAssetBundles();

            //window.veryifyDeletes = true;
            window.verifyDeleteList = new List<int>();

            //window.selectedBundleIndex = -1;

            window.assetTypeList = Enum.GetNames(typeof(Spaces.Core.Constants.AssetType)).ToList();
            window.assetTypeList.AddRange(window.otherAssetTypes);

            var token = Core.RestAPI.SpacesPlatformServicesSettings.Settings;//SelectPlatformServicesSettingsDialog.GetEditorToken();

            if (token)
            {
                var SessionSettings = CreateInstance<UnityClient.SessionSettings>();
                SessionSettings.spacesPlatformSettiings = token;
                SessionSettings.InitializePlatformServiceSettings();
            }

            window.assets = Core.Asset.AssetsManager.GetAssets();

            if (window.assets.Count == 0)
                window.RefreshAssetList();
            else
                window.assets.ForEach(a => a.onProcessEnd += window.AssetUpdated);
        }



        void OnGUI()
        {
            using (var assetWindow = new EditorGUILayout.VerticalScope())
            {
                using (var assetPanel = new EditorGUILayout.HorizontalScope())
                {
                    assetsExpanded = EditorGUILayout.Foldout(assetsExpanded, "Assets");

                    if (assetsExpanded)
                        if (GUILayout.Button("Refresh", GUILayout.Width(ASSET_BUTTON_WIDTH)))
                        {
                            Core.RestAPI.SpacesPlatformServicesSettings.Refresh();
                            RefreshAssetList();
                        }

                    Color guiColor = GUI.color;
                    GUI.color = Color.green;
                    if (GUILayout.Button("Create Asset", GUILayout.Width(ASSET_WIDE_BUTTON_WIDTH)))
                    {
                        CreateAssetDialog.CreateAsset(1);
                    }
                    GUI.color = guiColor;

                }

                if (assetsExpanded)
                {
                    DrawAssetsPanel();
                }

                DrawStatus();

            }
        }


        /// <summary>
        /// Updates the editor window.
        /// </summary>
        private void OnInspectorUpdate()
        {
            BestHTTP.HTTPManager.OnUpdate();

            try
            {
                this.Repaint();
            }
            catch (Exception ex)
            {

            }

            if (thumbs != null && thumbs.Count > 0)
                ThumbFetch.Start();
        }


        void DrawAssetsPanel()
        {
            using (var assetPanelContentFrame = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("", GUILayout.Width(ASSET_THUMBNAIL + 8));

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

            if (assets != null)
            {
                using (var scrollViewport = new EditorGUILayout.HorizontalScope())
                {
                    using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosAssets, EditorStyles.helpBox))
                    {
                        scrollPosAssets = scrollView.scrollPosition;
                        viewport = new Rect(scrollPosAssets, scrollViewport.rect.size);

                        using (var assetPanelContent = new EditorGUILayout.VerticalScope())
                        {
                            deleteAssetIndex = -1;

                            for (int i = 0; i < assets.Count; i++)
                            {
                                DrawAsset(i);
                            }

                            if (deleteAssetIndex > -1)
                            {
                                var assetToDelete = assets[deleteAssetIndex];

                                if (assets.Remove(assets[deleteAssetIndex]))
                                {
                                    assetToDelete.Delete();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Asset list went null");
                RefreshAssetList();
            }
        }

        void DrawAsset(int index)
        {
            //Color currentColor = GUI.color;
            Color currentBGColor = GUI.backgroundColor;
            GUI.backgroundColor = (index % 2 == 0 ? BG_COLOR_EVEN : BG_COLOR_ODD);

            using (var assetRow = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                Texture refTex = assets[index].thumb ? assets[index].thumb : AssetIconDictionary.Icon(assets[index].assetType);

                if (string.IsNullOrEmpty(assets[index].previewUrl) && !assets[index].InProcess(Core.Asset.Process.FetchingPreviewPath) && viewport.Overlaps(assetRow.rect))
                {
                    if (!assets[index].TryLoadPreviewFromCache())
                        assets[index].FetchPreview();
                }

                var rowRect = assetRow.rect;
                //rowRect.width -= (ASSET_BUTTON_WIDTH);// * 2);

                //Add conditional enable for state of asset
                //if (GUILayout.Button("Update", GUILayout.Width(ASSET_BUTTON_WIDTH)))
                if (GUI.Button(new Rect(rowRect.position.x, rowRect.position.y, rowRect.width - ASSET_BUTTON_WIDTH - 4, rowRect.height), GUIContent.none, EditorStyles.helpBox))
                {
                    if (assetData == null)
                        assetData = new Dictionary<Core.Asset, AssetData>();

                    if (!assetData.ContainsKey(assets[index]))
                    {
                        var updateData = new AssetData()
                        {
                            updateAssetName = assets[index].name,
                            updateAssetTypeIndex = assetTypeList.IndexOf(assets[index].assetType.ToLower()),
                            //updateBundleIndex = bundles.IndexOf(bundles.FirstOrDefault(b => b.BundleName + ".unity3d" == assets[index].originalFileName))
                        };

                        //if (!assets[index].isLoaded)
                        assets[index].RefreshInfo();

                        assetData.Add(assets[index], updateData);
                    }
                    else
                    {
                        assetData.Remove(assets[index]);
                    }

                }

                GUILayout.Label("", GUILayout.Height(ASSET_THUMBNAIL + 2), GUILayout.Width(ASSET_THUMBNAIL + 2), GUILayout.ExpandWidth(false));

                GUI.DrawTexture(new Rect(rowRect.position.x + 4, rowRect.position.y + 4, ASSET_THUMBNAIL, ASSET_THUMBNAIL), refTex);

                if (string.IsNullOrEmpty(assets[index].name))
                    DrawStatus(new Core.StatusMessage() { statusMessage = "", progressing = true });
                else
                    GUILayout.Label(assets[index].name, GUILayout.Width(ASSET_NAME_WIDTH));

                GUILayout.Label(!string.IsNullOrEmpty(assets[index].assetType) ? assets[index].assetType : "...", GUILayout.Width(ASSET_TYPE_WIDTH));
                EditorGUILayout.SelectableLabel(assets[index].id, GUILayout.Width(ASSET_ID_WIDTH), GUILayout.Height(EditorStyles.label.lineHeight + 1));

                DrawStatus(assets[index].GetStatusMessage());

                GUI.backgroundColor = currentBGColor;


                if (GUI.Button(new Rect(rowRect.position.x + rowRect.width - ASSET_BUTTON_WIDTH - 2, rowRect.position.y + 2, ASSET_BUTTON_WIDTH, 24), "Add To Scene"))
                {
                    //EditorApplication.isPlaying = true;
                    var gameObject = new GameObject(assets[index].name);
                    var assetWidget = gameObject.AddComponent<Core.AssetWidget>();
                    assetWidget.assetID = assets[index].id;
                    //assets[index].SpawnAssetInstance();
                }

                bool wasEnabled = GUI.enabled;
                GUI.enabled = wasEnabled && !verifyDeleteList.Contains(index);
                if (GUI.Button(new Rect(rowRect.position.x + rowRect.width - ASSET_BUTTON_WIDTH - 2, rowRect.position.y + 28, ASSET_BUTTON_WIDTH, 24), "Delete"))
                {
                    if (!verifyDeleteList.Contains(index))
                        verifyDeleteList.Add(index);
                }
                GUI.enabled = wasEnabled;


            }

            if (verifyDeleteList.Contains(index))
            {
                using (var verifyDelete = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("");
                    if (GUILayout.Button("Verify Delete", GUILayout.Width(ASSET_WIDE_BUTTON_WIDTH)))
                    {
                        deleteAssetIndex = index;
                        verifyDeleteList.Remove(index);
                    }
                    if (GUILayout.Button("Cancel", GUILayout.Width(ASSET_WIDE_BUTTON_WIDTH)))
                    {
                        verifyDeleteList.Remove(index);
                    }
                }
            }

            if (assetData != null && assetData.ContainsKey(assets[index]) && !assets[index].InProcess(Core.Asset.Process.Updating))
            {
                using (var assetDetailPanel = new EditorGUILayout.HorizontalScope())
                {
                    DrawEditAssetPanel(assets[index]);
                }
            }
        }

        void DrawEditAssetPanel(Core.Asset asset)
        {
            Color bgColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;//BG_COLOR_EVEN;

            using (var assetDetailPanel = new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinHeight(ASSET_EDIT_PANEL_MIN_HEIGHT)))
            {
                using (var nameField = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Asset Name:", GUILayout.Width(160));
                    assetData[asset].updateAssetName = GUILayout.TextField(assetData[asset].updateAssetName);
                }

                using (var idField = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Asset ID:", GUILayout.Width(160));
                    EditorGUILayout.SelectableLabel(asset.id, GUILayout.Width(ASSET_ID_WIDTH), GUILayout.Height(EditorStyles.label.lineHeight + 1));
                }

                using (var typeField = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Asset Type:", GUILayout.Width(160));
                    assetData[asset].updateAssetTypeIndex = EditorGUILayout.Popup(assetData[asset].updateAssetTypeIndex, assetTypeList.ToArray(), GUILayout.Width(160));
                }

                using (var pathField = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Path to Data:", GUILayout.Width(160));
                    EditorGUILayout.SelectableLabel(asset.pathToData, GUILayout.ExpandWidth(true), GUILayout.Height(EditorStyles.label.lineHeight + 1));
                }

                EditorGUILayout.LabelField("");

                //if (assetData[asset].updateAssetTypeIndex > -1 && assetData[asset].updateAssetTypeIndex < assetTypeList.Count && asset.assetType.ToLower() == assetTypeList[assetData[asset].updateAssetTypeIndex] && !externalBundle)
                //{
                //    EditorGUILayout.BeginHorizontal();
                //    {
                //        EditorGUILayout.LabelField("Bundle:", GUILayout.Width(160));
                //        assetData[asset].updateBundleIndex = EditorGUILayout.Popup(assetData[asset].updateBundleIndex, bundles.Select(b => b.BundleName).ToArray(), GUILayout.Width(BUNDLE_NAME_WIDTH));

                //        bool wasEnabled = GUI.enabled;
                //        GUI.enabled = wasEnabled && (!asset.InProcess(Core.Asset.Process.Uploading) || !asset.InProcess(Core.Asset.Process.RequestingEndpoint));
                //        if (GUILayout.Button("Re-Upload", GUILayout.Width(ASSET_WIDE_BUTTON_WIDTH)))
                //        {
                //            asset.AddData(bundles[assetData[asset].updateBundleIndex].PathToBundle);
                //        }
                //        GUI.enabled = wasEnabled;

                //        if (assetData[asset].updateBundleIndex > -1 && assetData[asset].updateBundleIndex < bundles.Count)
                //            DrawStatus(bundles[assetData[asset].updateBundleIndex].GetStatusMessage());
                //    }
                //    EditorGUILayout.EndHorizontal();
                //}
                //else
                //{
                using (var filePathField = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Asset File Path:", GUILayout.Width(160));
                    assetData[asset].updatedResourcePath = EditorGUILayout.TextField(assetData[asset].updatedResourcePath);

                    bool wasEnabled = GUI.enabled;
                    GUI.enabled = wasEnabled && !asset.InProcess(Core.Asset.Process.Uploading);
                    if (GUILayout.Button("Re-Upload", GUILayout.Width(ASSET_WIDE_BUTTON_WIDTH)))
                    {
                        asset.AddData(assetData[asset].updatedResourcePath);
                    }
                    GUI.enabled = wasEnabled;
                }
                //}

                //if (asset.assetType == assetTypeList[assetData[asset].updateAssetTypeIndex])
                //    externalBundle = EditorGUILayout.Toggle("external bundle file", externalBundle, GUILayout.Width(160));

                EditorGUILayout.LabelField("", GUILayout.ExpandHeight(true));

                using (var graphButtonField = new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Open Graph", GUILayout.Width(ASSET_WIDE_BUTTON_WIDTH)))
                    {
                        Manager.SpaceGraphNodeViewer.OpenGraphViewer(asset);
                    }

                    if (GUILayout.Button("Reset to Default", GUILayout.Width(ASSET_WIDE_BUTTON_WIDTH)))
                    {
                        if (asset != null)
                        {
                            asset.ResetToDefault();
                        }
                    }

                    EditorGUILayout.LabelField("");
                    bool wasEnabled = GUI.enabled;
                    GUI.enabled = wasEnabled && assetData[asset].updateAssetName != asset.name;
                    if (GUILayout.Button("Update", GUILayout.Width(ASSET_WIDE_BUTTON_WIDTH)))
                    {
                        asset.Update(assetData[asset].updateAssetName, assetTypeList[assetData[asset].updateAssetTypeIndex], "", "");
                    }
                    GUI.enabled = wasEnabled;

                    if (GUILayout.Button("Cancel", GUILayout.Width(ASSET_WIDE_BUTTON_WIDTH)))
                    {
                        assetData.Remove(asset);
                        //externalBundle = false;
                    }
                }
            }
            GUI.backgroundColor = bgColor;
        }

        //void DrawBundlesPanel()
        //{
        //    EditorGUILayout.BeginHorizontal();
        //    {
        //        if (GUILayout.Button("Bundle Name", EditorStyles.label, GUILayout.Width(BUNDLE_NAME_WIDTH)))
        //        {
        //            if (bundles != null && bundles.Count > 0)
        //            {
        //                bundles.Sort((b1, b2) => b1.BundleName.CompareTo(b2.BundleName));
        //            }
        //        }

        //        if (GUILayout.Button("File Size", EditorStyles.label, GUILayout.Width(BUNDLE_FILE_SIZE_WIDTH)))
        //        {
        //            if (bundles != null && bundles.Count > 0)
        //            {
        //                bundles.Sort((b1, b2) => b1.Size.CompareTo(b2.Size));
        //            }
        //        }
        //    }
        //    EditorGUILayout.EndHorizontal();

        //    if (bundles != null)
        //    {
        //        scrollPosBundles = EditorGUILayout.BeginScrollView(scrollPosBundles);
        //        {
        //            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinHeight(Screen.height / 3), GUILayout.MaxHeight(Screen.height));
        //            {

        //                for (int i = 0; i < bundles.Count; i++)
        //                {
        //                    DrawAvailableAssetBundle(bundles[i], i);
        //                }
        //            }
        //            EditorGUILayout.EndVertical();

        //        }
        //        EditorGUILayout.EndScrollView();
        //    }

        //}

        /// <summary>
        /// Draws an assetbundle.
        /// </summary>
        /// <param name="bundle">Bundle to draw.</param>
        /// <param name="index">Index of the bundle.</param>
        //private void DrawAvailableAssetBundle(AvailableBundle bundle, int index)
        //{
        //    Color currentColor = GUI.color;
        //    Color currentBGColor = GUI.backgroundColor;

        //    if (selectedBundleIndex == index)
        //    {
        //        //GUI.backgroundColor = Color.green;
        //        GUI.color = Color.green;
        //    }

        //    GUI.backgroundColor = (index % 2 == 0 ? BG_COLOR_EVEN : BG_COLOR_ODD);
        //    //GUI.color = (index % 2 == 0 ? BG_COLOR_EVEN : BG_COLOR_ODD);

        //    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        //    {
        //        GUILayout.Label(bundle.BundleName, GUILayout.Width(BUNDLE_NAME_WIDTH));
        //        GUILayout.Label(string.Concat(bundle.Size.ToString("F4"), " MB"), GUILayout.Width(BUNDLE_FILE_SIZE_WIDTH));

        //        if (bundle.BundleState == AvailableBundle.State.Error)
        //        {
        //            showProgress = false;

        //            if (GUILayout.Button("Retry"))
        //            {
        //                bundle.RetryFromLastState();
        //            }
        //        }
        //        else if (bundle.BundleState == AvailableBundle.State.Idle || bundle.BundleState == AvailableBundle.State.Complete)
        //        {
        //            showProgress = false;
        //        }
        //        else
        //        {
        //            showProgress = true;
        //        }

        //        DrawStatus(bundle.GetStatusMessage());
        //        //GUI.color = currentColor;
        //        GUI.backgroundColor = currentBGColor;

        //        //Update to allow for re-upload of asset bundle.

        //        if (GUILayout.Button("Select", GUILayout.Width(ASSET_BUTTON_WIDTH)))
        //        {
        //            if (selectedBundleIndex == index)
        //                selectedBundleIndex = -1;
        //            else
        //                selectedBundleIndex = index;
        //        }

        //        GUI.color = currentColor;
        //        //GUI.backgroundColor = currentBGColor;
        //    }
        //    EditorGUILayout.EndHorizontal();

        //    if (selectedBundleIndex == index)
        //    {
        //        DrawCreateAssetPanel();
        //    }
        //}

        //void DrawCreateAssetPanel()
        //{
        //    Color guiColor = GUI.color;
        //    GUI.color = Color.green;

        //    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        //    {
        //        EditorGUILayout.BeginHorizontal();
        //        {
        //            EditorGUILayout.LabelField("Asset Name:", GUILayout.Width(160));
        //            assetName = EditorGUILayout.TextField(assetName);
        //        }
        //        EditorGUILayout.EndHorizontal();

        //        if (createAsset)
        //        {
        //            EditorGUILayout.BeginHorizontal();
        //            {
        //                EditorGUILayout.LabelField("Asset Type:", GUILayout.Width(160));
        //                assetTypeIndex = EditorGUILayout.Popup(assetTypeIndex, assetTypeList, GUILayout.Width(160));
        //            }
        //            EditorGUILayout.EndHorizontal();

        //            EditorGUILayout.BeginHorizontal();
        //            {
        //                EditorGUILayout.LabelField("Asset File Path:", GUILayout.Width(160));
        //                assetPath = EditorGUILayout.TextField(assetPath);
        //            }
        //            EditorGUILayout.EndHorizontal();
        //        }

        //        EditorGUILayout.BeginHorizontal();
        //        {
        //            bool wasEndabled = GUI.enabled;
        //            GUI.enabled = wasEndabled && !string.IsNullOrEmpty(assetName) && (!createAsset || System.IO.File.Exists(assetPath));
        //            if (GUILayout.Button("Create Asset"))
        //            {
        //                Debug.Log("Create me an asset, fast as you can!\n\nFaster.\n\nFASTER!");

        //                if (createAsset)
        //                {
        //                    CreateAsset();
        //                    createAsset = false;
        //                }
        //                else
        //                {
        //                    bundles[selectedBundleIndex].CreateAsset(assetName);
        //                    selectedBundleIndex = -1;
        //                }
        //            }
        //            GUI.enabled = wasEndabled;

        //            if (GUILayout.Button("Cancel"))
        //            {
        //                Debug.Log("Cancelling asset creation.");
        //                assetName = "";
        //                assetPath = "";

        //                if (createAsset)
        //                    createAsset = false;
        //                else
        //                    selectedBundleIndex = -1;
        //            }
        //        }
        //        EditorGUILayout.EndHorizontal();
        //        GUI.color = guiColor;
        //    }
        //    EditorGUILayout.EndVertical();
        //}

        public void RefreshAssetList()
        {
            assets.Clear();

            //assets = Core.Asset.GetAssetList(OnGetAssetListResponse);
            Core.Asset.GetAssetList(OnGetAssetListResponse);
            UpdateState(State.FetchingAssets);

            if (verifyDeleteList == null)
                verifyDeleteList = new List<int>();
            else
                verifyDeleteList.Clear();

            if (assetData == null)
                assetData = new Dictionary<Core.Asset, AssetData>();
            else
                assetData.Clear();
        }

        private void OnGetAssetListResponse(bool error, Core.RestAPI.RestGetAssetListResponseData response)
        {
            if (error)
            {
                Debug.Log("[Error] Retrieving Asset list failed.");
                UpdateState(State.Error);
            }
            else
            {
                assets.ForEach(a => a.onProcessEnd += AssetUpdated);
                UpdateState(State.Idle);
                Core.Asset.AssetsManager.SaveToCache();
                Debug.Log("[Completed Process: Fetch Asset List]");
            }
        }

        //[MenuItem("Spaces/Create/(Re)Build Bundles")]
        //static void BuildAllAssetBundles(BuildTarget platform = BuildTarget.StandaloneWindows64)
        //{
        //    //Create the folder if it does not exist
        //    if (!System.IO.Directory.Exists(Spaces.Core.Constants.SPACES_BUNDLES))
        //        System.IO.Directory.CreateDirectory(Spaces.Core.Constants.SPACES_BUNDLES);

        //    //Create the bundles
        //    var manifest = BuildPipeline.BuildAssetBundles(Spaces.Core.Constants.SPACES_BUNDLES, BuildAssetBundleOptions.None, platform);
        //    Debug.Log("Bundle Count: " + manifest.GetAllAssetBundles().Length);
        //}

        //[MenuItem("Spaces/Create/Make Test Bundles")]
        //static void BuildAssetBundles()
        //{
        //    BuildTarget platform = BuildTarget.StandaloneWindows64;

        //    //Create the folder if it does not exist
        //    if (!System.IO.Directory.Exists("Bundles"))
        //        System.IO.Directory.CreateDirectory("Bundles");

        //    //Create the bundles
        //    var manifest = BuildPipeline.BuildAssetBundles("Bundles", BuildAssetBundleOptions.None, platform);
        //    Debug.Log("Bundle Count: " + manifest.GetAllAssetBundles().Length);
        //}

        //private void GetAvailableAssetBundles()
        //{
        //    if (bundles == null)
        //        bundles = new List<AvailableBundle>();
        //    else
        //        bundles.Clear();

        //    //Get all of the asset bundles in the asset bundle folder
        //    if (System.IO.Directory.Exists(Spaces.Core.Constants.SPACES_BUNDLES))
        //    {
        //        //Get all of the local asset bundles
        //        if (System.IO.File.Exists(Spaces.Core.Constants.SPACES_BUNDLES + "/AvailableBundles.json"))
        //        {
        //            try
        //            {
        //                var availableBundles = Spaces.Core.JSONTools.Load<List<AvailableBundle>>(Spaces.Core.Constants.SPACES_BUNDLES + "/AvailableBundles.json");

        //                foreach (var bundle in availableBundles)
        //                {
        //                    if (System.IO.File.Exists(bundle.PathToBundle))
        //                    {
        //                        bundle.onStateChanged += BundleUpdated;
        //                        bundles.Add(bundle);
        //                    }
        //                }
        //            }
        //            catch (System.Exception ex)
        //            {
        //                Debug.Log(ex);
        //            }
        //        }

        //        //Get all of the local asset bundles
        //        foreach (string file in
        //            System.IO.Directory.GetFiles(Spaces.Core.Constants.SPACES_BUNDLES)
        //            .Where(file => string.IsNullOrEmpty(System.IO.Path.GetExtension(file)) && System.IO.File.Exists(file + ".manifest")))
        //        {
        //            if (!bundles.Any(b => b.PathToBundle == file.Replace("\\", "/")))
        //            {
        //                var bundle = new AvailableBundle(file.Replace("\\", "/"));
        //                bundle.onStateChanged += BundleUpdated;
        //                bundles.Add(bundle);
        //            }
        //        }

        //        bundles.Sort((b1, b2) => b1.BundleName.CompareTo(b2.BundleName));

        //        bundlesUpdated = true;
        //    }
        //}


        private void AssetUpdated(Core.Asset sender, Core.Asset.Process[] currentState, Core.Asset.Process endingState)
        {
            if (endingState == Core.Asset.Process.Deleting)
                sender.onProcessEnd -= AssetUpdated;

            // Asset creation steps
            if (endingState == Core.Asset.Process.Creating)
            {
                if (assetData.ContainsKey(sender) && !string.IsNullOrEmpty(assetData[sender].updatedResourcePath))
                    sender.AddData(assetData[sender].updatedResourcePath);
            }
            else if (endingState == Core.Asset.Process.Fetching && !string.IsNullOrEmpty(sender.assetType))
            {
                if (!assetTypeList.Contains(sender.assetType.ToLower()))
                    assetTypeList.Add(sender.assetType.ToLower());

                if (sender.assetType == Core.Constants.AssetType.image.ToString() && sender.previewUrl == "NONE")
                {
                    if (thumbs == null)
                        thumbs = new List<ThumbFetch>();

                    sender.previewUrl = sender.pathToData;
                    thumbs.Add(new ThumbFetch(sender));
                }
            }
            else if (endingState == Core.Asset.Process.FetchingPreviewPath)
            {
                if (thumbs == null)
                    thumbs = new List<ThumbFetch>();

                if (sender.previewUrl != "NONE")
                    thumbs.Add(new ThumbFetch(sender));
            }

            //if (state == State.FetchingAssets)
            //{
            //    if (assets.All(a => !a.InProcess(Core.Asset.Process.Fetching)))
            //    {
            //        UpdateState(State.Idle);
            //    }
            //}
            //else 
            if (state == State.CreatingAsset)
            {
                if (assets.All(a => !a.InProcess(Core.Asset.Process.Creating)))
                {
                    UpdateState(State.Idle);
                }
            }

        }

        //private void BundleUpdated(AvailableBundle.State lastState, AvailableBundle.State newState)
        //{
        //    bundlesUpdated = true;

        //    if (lastState == AvailableBundle.State.Creating && newState != AvailableBundle.State.Error)
        //    {
        //        RefreshAssetList();
        //    }
        //    else if (lastState == AvailableBundle.State.Uploading && newState == AvailableBundle.State.Complete)
        //    {
        //        UpdateState(State.Idle);
        //    }
        //}

        //public void SerializeAvailableBundles()
        //{
        //    if (bundles.Count > 0)
        //    {
        //        if (System.IO.Directory.Exists(Spaces.Core.Constants.SPACES_BUNDLES))
        //            System.IO.Directory.CreateDirectory(Spaces.Core.Constants.SPACES_BUNDLES);

        //        // serialize updated space attributes to CACHE.
        //        using (var writer = new System.IO.StreamWriter(Spaces.Core.Constants.SPACES_BUNDLES + "/AvailableBundles.json"))
        //        {
        //            string spaceJSON = Spaces.Core.JSONTools.LoadToString(bundles);
        //            writer.Write(spaceJSON);
        //        }
        //    }
        //}

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


        void DrawStatus()
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

                Debug.Log(statusMessage + progress);
            }

            if (showProgress)
                EditorGUILayout.LabelField(statusMessage + progress);
            else
                EditorGUILayout.LabelField(statusMessage);
        }

        void UpdateState(State newState)
        {
            if (state != newState && onStateChanged != null)
                onStateChanged(state, newState);

            state = newState;
            progress = "";
            progCount = 0;

            switch (state)
            {
                case State.Error:
                    statusMessage = "Something has gone wrong. :(";
                    showProgress = false;
                    break;
                case State.Idle:
                    statusMessage = "";
                    showProgress = false;
                    break;
                case State.FetchingAssets:
                    statusMessage = "Updating Assets list";
                    showProgress = true;
                    break;
                case State.CreatingAsset:
                    statusMessage = "Creating a new Asset";
                    showProgress = true;
                    break;
                default:
                    break;
            }
        }

    }
}