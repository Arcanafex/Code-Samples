using UnityEngine;
using UnityEditor;

namespace Spaces.Manager
{
    public class CreateSpaceDialog : EditorWindow
    {
        string spaceName;
        Core.Space m_space;

        //Template options?

        int progCount = 0;
        System.DateTime start;
        string progress = "";

        bool isCancelRequested;
        bool wasCreateCalled;

        [MenuItem("Spaces/Create/Create Space", false, 1)]
        public static void CreateSpace()
        {
            var window = GetWindow<CreateSpaceDialog>("Create Space", true);

            window.position = new Rect(Screen.width / 2, Screen.height / 2, 450, 200);
            window.spaceName = "";
            window.wasCreateCalled = false;
        }

        void OnGUI()
        {
            DrawCreateSpacePanel();

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

        void DrawCreateSpacePanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Space Name:", GUILayout.Width(160));
                    spaceName = EditorGUILayout.TextField(spaceName);
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Label("", GUILayout.ExpandHeight(true));

                if (m_space == null)
                    GUILayout.Label("");
                else
                    DrawStatus(m_space.GetStatusMessage());

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("");

                    bool wasEndabled = GUI.enabled;
                    GUI.enabled = wasEndabled && m_space == null && !string.IsNullOrEmpty(spaceName);
                    if (GUILayout.Button("Create Space", GUILayout.Width(120)))
                    {
                        wasCreateCalled = true;
                        m_space = Core.Space.Create(spaceName);
                    }
                    GUI.enabled = wasEndabled;

                    if (GUILayout.Button("Close", GUILayout.Width(120)))
                    {
                        if (wasCreateCalled)
                            GetWindow<SpaceManagerWindow>().RefreshSpacesList();

                        Close();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
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