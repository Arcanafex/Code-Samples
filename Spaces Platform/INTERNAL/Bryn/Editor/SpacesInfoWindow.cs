using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Spaces.Core.RestAPI;

public class SpacesInfoWindow : EditorWindow
{
    List<Spaces.Core.Space> spaces;
    const string SPACES_INFO = "SpacesInfo";
    Vector2 scrollPos;
    Dictionary<string, RestGetSpaceResponseData> spacesInfo;

    //[MenuItem("Spaces/Space Info")]
    static void Init()
    {
        var window = GetWindow<SpacesInfoWindow>("Spaces Info");
        window.minSize = new Vector2(500, 400);
        window.Show();
        window.Focus();

        //PlayerPrefs.DeleteKey(SPACES_INFO);

        //if (PlayerPrefs.HasKey(Spaces.Core.Constants.SPACES_LIST))
        //{
        //    window.spaces = Spaces.Core.JSONTools.LoadFromString<List<RestGetSpaceListResponseData.SpaceInfo>>(PlayerPrefs.GetString(Spaces.Core.Constants.SPACES_LIST));
        //}
        //else
        //{
        //    window.spaces = new List<RestGetSpaceListResponseData.SpaceInfo>();
        //}

        window.spacesInfo = new Dictionary<string, RestGetSpaceResponseData>();

    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Name", GUILayout.Width(200));
        EditorGUILayout.LabelField("ID");
        //EditorGUILayout.LabelField(" ");
        EditorGUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        if (spaces.Count > 0)
        {
            foreach (var space in spaces)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.SelectableLabel(space.name, GUILayout.Width(200));
                EditorGUILayout.SelectableLabel(space.id);

                bool wasEnabled = GUI.enabled;
                GUI.enabled = (wasEnabled && !spacesInfo.ContainsKey(space.id));
                if (GUILayout.Button("More Info", GUILayout.Width(100)))
                {
                    spacesInfo.Add(space.id, null);
                    var getSpace = new RestGetSpace();
                    getSpace.Run(space.id, GetSpaceInfoResponse);
                }
                GUI.enabled = wasEnabled;
                EditorGUILayout.EndHorizontal();

                if (spacesInfo.ContainsKey(space.id) && spacesInfo[space.id] != null)
                {
                    DrawFullSpaceInfo(spacesInfo[space.id]);
                }

            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel("Refresh List");
            EditorGUILayout.SelectableLabel("--");
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Refresh"))
        {
            RefreshSpacesList();
        }

        EditorGUILayout.EndVertical();

        // We force a repaint so we can run the httpmanager while in editor mode. It totally works, sweet.
        BestHTTP.HTTPManager.OnUpdate();
        this.Repaint();
    }

    void DrawFullSpaceInfo(RestGetSpaceResponseData space)
    {
        //owner

        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        
        if (GUILayout.Button("X"))
        {
            spacesInfo.Remove(space.id);
            return;
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.SelectableLabel(" ");
        EditorGUILayout.SelectableLabel("Owner ID");
        EditorGUILayout.SelectableLabel(space.owner_id);
        EditorGUILayout.EndHorizontal();

        foreach (string assetID in space.asset_ids)
        {
            EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.SelectableLabel(" ");
            EditorGUILayout.SelectableLabel("Asset");
            EditorGUILayout.SelectableLabel(assetID);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

    }

    void GetSpaceInfoResponse(bool error, RestGetSpaceResponseData spaceData)
    {
        if (error)
        {
            //ooops
        }
        else
        {
            if (spacesInfo.ContainsKey(spaceData.id))
                spacesInfo[spaceData.id] = spaceData;
            else
                spacesInfo.Add(spaceData.id, spaceData);
        }
    }

    void RefreshSpacesList()
    {
        Spaces.Core.Space.GetSpaceList(RefreshSpacesResponse); //MadeUpFakeSpaces;

        //var getSpaceList = new RestGetSpaceList();
        //getSpaceList.Run(RefreshSpacesResponse);
    }

    void RefreshSpacesResponse(bool error, RestGetSpaceListResponseData responseData)
    {
        if (error)
        {
            Debug.Log("Spaces List request failed.");
        }
        else
        {
            spaces = responseData.spaces;
            //spaces = responseData.spaceInfoList;

            //PlayerPrefs.SetString(Spaces.Core.Constants.SPACES_LIST, Spaces.Core.JSONTools.LoadToString(spaces));
            //PlayerPrefs.Save();
        }
    }

    List<RestGetSpaceListResponseData.SpaceInfo> MadeUpFakeSpaces
    {
        get
        {
            int tunnels = 14;

            var fakeSpaces = new List<RestGetSpaceListResponseData.SpaceInfo>();

            for (int f = 0; f < tunnels; f++)
            {
                var fakeSpace = new RestGetSpaceListResponseData.SpaceInfo();

                fakeSpace.name = "A Maze of " + ((f + 1)) + " Twisty Tunnels";
                fakeSpace.id = System.Guid.NewGuid().ToString();
                fakeSpaces.Add(fakeSpace);
            }

            return fakeSpaces;
        }
    }

}