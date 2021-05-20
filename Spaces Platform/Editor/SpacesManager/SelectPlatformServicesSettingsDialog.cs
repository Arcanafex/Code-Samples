using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Spaces.Manager
{
    public class SelectPlatformServicesSettingsDialog : EditorWindow
    {
        private const string ServicesSettings = "SERVICES_SETTINGS";
        private Core.RestAPI.SpacesPlatformServicesSettings currentToken;
        private string path;
        private string pastedPath;

        //[MenuItem("Spaces/Editor Services Settings")]
        private static void Init()
        {
            var window = GetWindow<SelectPlatformServicesSettingsDialog>();
            window.currentToken = ScriptableObject.CreateInstance<Core.RestAPI.SpacesPlatformServicesSettings>();

            window.path = "";
            window.pastedPath = "";

            //PlayerPrefs.DeleteAll();
            //EditorPrefs.DeleteAll();

            if (PlayerPrefs.HasKey(Spaces.Core.Constants.PLATFORM_SERVICES_SETTINGS))
            {
                var settings = PlayerPrefs.GetString(Spaces.Core.Constants.PLATFORM_SERVICES_SETTINGS);
                JsonUtility.FromJsonOverwrite(settings, window.currentToken);
            }

            if (EditorPrefs.HasKey(ServicesSettings))
            {
                var settings = EditorPrefs.GetString(ServicesSettings);
                JsonUtility.FromJsonOverwrite(settings, window.currentToken);
            }

        }


        void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            {
                if (GUILayout.Button("Browse"))
                {
                    path = EditorUtility.OpenFilePanel("Platform Services", "Assets", "asset");
                }

                GUILayout.Label("Path: ");
                //GUILayout.Label(path, GUILayout.ExpandWidth(true));
                path = EditorGUILayout.TextField(path, GUILayout.ExpandWidth(true)).Trim('"').Replace(@"\", "/");

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Set"))
                    {
                        currentToken = AssetDatabase.LoadAssetAtPath<Core.RestAPI.SpacesPlatformServicesSettings>(FileUtil.GetProjectRelativePath(path));
                        EditorPrefs.SetString(ServicesSettings, JsonUtility.ToJson(currentToken));
                    }

                    if (GUILayout.Button("Clear"))
                    {
                        EditorPrefs.DeleteKey(ServicesSettings);
                        currentToken = CreateInstance<Core.RestAPI.SpacesPlatformServicesSettings>();
                    }
                }

                GUILayout.Label("");
                GUILayout.Label("Current Setting: " + (!currentToken || string.IsNullOrEmpty(currentToken.TokenDescription) ? "None" : currentToken.TokenDescription));
                GUILayout.Label("");

            }
            EditorGUILayout.EndVertical();
        }

        public static Core.RestAPI.SpacesPlatformServicesSettings GetEditorToken()
        {
            var token = ScriptableObject.CreateInstance<Core.RestAPI.SpacesPlatformServicesSettings>();

            if (PlayerPrefs.HasKey(Spaces.Core.Constants.PLATFORM_SERVICES_SETTINGS))
            {
                var settings = PlayerPrefs.GetString(Spaces.Core.Constants.PLATFORM_SERVICES_SETTINGS);
                JsonUtility.FromJsonOverwrite(settings, token);
            }

            if (EditorPrefs.HasKey(ServicesSettings))
            {
                var settings = EditorPrefs.GetString(ServicesSettings);
                JsonUtility.FromJsonOverwrite(settings, token);
            }

            return token;
        }
    }
}