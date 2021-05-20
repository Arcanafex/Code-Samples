using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

namespace Spaces.Manager
{
    public class CreateAssetDialog : EditorWindow
    {
        int mode;

        string assetName;
        string assetPath;
        AvailableBundle m_bundle;
        Core.Asset m_asset;

        string[] assetTypeList;
        int assetTypeIndex;

        int progCount = 0;
        System.DateTime start;
        string progress = "";

        bool isCancelRequested;
        bool wasCreateCalled;

        //[MenuItem("Assets/Create Spaces Asset")]
        static void CreateAsset()
        {
            var window = GetWindow<CreateAssetDialog>("Create Asset", true);

            window.position = new Rect(Screen.width / 2, Screen.height / 2, 450, 200);
            window.assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            window.assetName = Selection.activeObject.name;
            window.wasCreateCalled = false;
            window.mode = 0;

            window.assetTypeList = Enum.GetNames(typeof(Spaces.Core.Constants.AssetType));
        }

        //[MenuItem("Assets/Create Spaces Asset", true)]
        static bool CreateAssetValidator()
        {
            // Currently limits Asset to single prefab
            if (Selection.activeObject)
                return PrefabUtility.GetPrefabType(Selection.activeObject) != PrefabType.None;
            else
                return false;

            // Considering expanding allow to multi-select of prefabs, textures, materials, etc.
        }

        //[MenuItem("Spaces/Create/Create Asset", false, 2)]
        public static void CreateAsset(int mode)
        {
            var window = GetWindow<CreateAssetDialog>("Create Asset");

            window.position = new Rect(Screen.width / 2, Screen.height / 2, 450, 200);
            window.assetPath = "";
            window.assetName = "";
            window.mode = mode;

            window.assetTypeList = Enum.GetNames(typeof(Spaces.Core.Constants.AssetType));

            window.ShowPopup();
        }


        void OnGUI()
        {
            if (mode == 0)
                DrawCreateBundlePanel();
            else
                DrawCreateAssetPanel();

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

        void DrawCreateAssetPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Asset Name:", GUILayout.Width(160));
                    assetName = EditorGUILayout.TextField(assetName);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Asset Type:", GUILayout.Width(160));
                    assetTypeIndex = EditorGUILayout.Popup(assetTypeIndex, assetTypeList, GUILayout.Width(160));
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Asset File Path:", GUILayout.Width(160));
                    assetPath = EditorGUILayout.TextField(assetPath);
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Label("", GUILayout.ExpandHeight(true));

                if (m_asset == null)
                    GUILayout.Label("");
                else
                    DrawStatus(m_asset.GetStatusMessage());

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("");

                    bool wasEndabled = GUI.enabled;
                    GUI.enabled = wasEndabled && m_asset == null && !string.IsNullOrEmpty(assetName) && System.IO.File.Exists(assetPath);
                    if (GUILayout.Button("Create Asset", GUILayout.Width(120)))
                    {
                        m_asset = Core.Asset.Create(assetName, assetTypeList[assetTypeIndex], assetPath);
                    }
                    GUI.enabled = wasEndabled;

                    if (GUILayout.Button("Close", GUILayout.Width(120)))
                    {
                        if (wasCreateCalled)
                            GetWindow<AssetManagerWindow>().RefreshAssetList();

                        Close();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        void DrawCreateBundlePanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label("Asset Name");
                assetName = GUILayout.TextField(assetName);
                GUILayout.Label("", GUILayout.ExpandHeight(true));

                if (m_bundle == null)
                    GUILayout.Label("");
                else
                    DrawStatus(m_bundle.GetStatusMessage());

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("");

                    bool wasEndabled = GUI.enabled;
                    GUI.enabled = wasEndabled && m_bundle == null;
                    if (GUILayout.Button("Create Asset", GUILayout.Width(120)))
                    {
                        CreateBundle(assetName);
                    }
                    GUI.enabled = wasEndabled;

                    if (GUILayout.Button("Close", GUILayout.Width(120)))
                    {
                        Close();
                    }
                }
                EditorGUILayout.EndHorizontal();



                // TODO: implement cancellability on Bundles and Assets
                //if (GUILayout.Button("Cancel"))
                //{
                //    Debug.Log("Asset Creation Cancelled");
                //    isCancelRequested = true;
                //}

                //GUILayout.Label("");
            }
            EditorGUILayout.EndVertical();
        }

        void CreateBundle(string bundleName)
        {
            Debug.Log("Building a bundle for " + assetPath);

            string[] assetPaths = new string[1];
            assetPaths[0] = assetPath;

            AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
            buildMap[0].assetBundleName = bundleName;
            buildMap[0].assetNames = assetPaths;

            if (!System.IO.Directory.Exists("Bundles"))
                System.IO.Directory.CreateDirectory("Bundles");

            var manifest = BuildPipeline.BuildAssetBundles(Spaces.Core.Constants.SPACES_BUNDLES, buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            string bundlePath = Spaces.Core.Constants.SPACES_BUNDLES + "\\" + bundleName.ToLower();
            this.Focus();

            if (System.IO.File.Exists(bundlePath))
            {
                m_bundle = new AvailableBundle(bundlePath);
                m_asset = m_bundle.CreateAsset(bundleName);
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
        
    }
}