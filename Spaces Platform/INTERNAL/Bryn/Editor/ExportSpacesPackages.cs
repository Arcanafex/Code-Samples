using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ExportSpacesPackages : ScriptableObject
{
    const string UPLOADER_PATH = "SpacesManager.unitypackage";
    const string TOOLS_PATH = "SpacesTools.unitypackage";
    const string WIDGET_EXAMPLES_PATH = "SpacesWidgetsExamples.unitypackage";

    static string[] SpacesCorePaths
    {
        get
        {
            return new string[]{
                "Assets/_Spaces/Scripts/Core/SpacesIO.cs",
                "Assets/_Spaces/Scripts/Core/Space.cs",
                "Assets/_Spaces/Scripts/Core/Asset.cs",
                "Assets/_Spaces/Scripts/Core/Node.cs",
                "Assets/_Spaces/Scripts/Core/JSONStructures.cs",
                "Assets/_Spaces/Scripts/Core/JSONTools.cs",
                "Assets/_Spaces/Scripts/Core/SpacesConstants.cs",

                "Assets/_Spaces/Scripts/Core/RestAPI/RestManager.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestEntry.cs",

                "Assets/_Spaces/Scripts/Core/RestAPI/RestCreateAsset.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestDeleteAsset.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestGetAsset.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestGetAssetData.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestGetAssetList.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestGetAssetUploadEndpoint.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestGetAssetMetadata.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestUpdateAsset.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestUpdateAssetMetadata.cs",

                "Assets/_Spaces/Scripts/Core/RestAPI/RestCreateSpace.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestDeleteSpace.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestGetSpace.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestGetSpaceList.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestGetSpaceMetadata.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestUpdateSpace.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestUpdateSpaceMetadata.cs",

                "Assets/_Spaces/Scripts/Core/RestAPI/RestAddAssetToSpace.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestRemoveAssetFromSpace.cs",

                "Assets/_Spaces/Scripts/Core/RestAPI/RestUpload.cs",

                "Assets/_Spaces/Scripts/Core/RestAPI/RestCreateGroup.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestDeleteGroup.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestUpdateGroup.cs",

                "Assets/_Spaces/Scripts/Core/RestAPI/RestCreateUser.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestDeleteUser.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestUpdateUser.cs",

                "Assets/_Spaces/Scripts/Core/RestAPI/RestGetUserList.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestLogin.cs",
                "Assets/_Spaces/Scripts/Core/RestAPI/RestLogout.cs",

                "Assets/_Spaces/Scripts/Core/TinyJSON/JSON.cs",
                "Assets/_Spaces/Scripts/Core/TinyJSON/Decoder.cs",
                "Assets/_Spaces/Scripts/Core/TinyJSON/EncodeOptions.cs",
                "Assets/_Spaces/Scripts/Core/TinyJSON/Encoder.cs",
                "Assets/_Spaces/Scripts/Core/TinyJSON/Extensions.cs",

                "Assets/_Spaces/Scripts/Core/TinyJSON/Types/ProxyBoolean.cs",
                "Assets/_Spaces/Scripts/Core/TinyJSON/Types/ProxyNumber.cs",
                "Assets/_Spaces/Scripts/Core/TinyJSON/Types/ProxyObject.cs",
                "Assets/_Spaces/Scripts/Core/TinyJSON/Types/ProxyString.cs",
                "Assets/_Spaces/Scripts/Core/TinyJSON/Types/Variant.cs",
                "Assets/_Spaces/Scripts/Core/TinyJSON/Types/ProxyArray.cs"
            };
        }
    }

    static string[] BestHTTPPath { get { return new string[] { "Assets/Best HTTP (Pro)/Plugins/BestHTTP.dll" }; } }

    static string[] UploaderPaths
    {
        get
        {
            return new string[] {
                "Assets/_Spaces/Scripts/Editor/SpacesClientTools.cs",
                "Assets/_Spaces/Scripts/Editor/SpacesManager/AssetManagerWindow.cs",
                "Assets/_Spaces/Scripts/Editor/SpacesManager/AvailableBundle.cs",
                "Assets/_Spaces/Scripts/Editor/SpacesManager/SpaceManagerWindow.cs",
                "Assets/_Spaces/Scripts/Editor/SpacesManager/CreateAssetDialog.cs",
                "Assets/_Spaces/Scripts/Editor/SpacesManager/CreateSpaceDialog.cs",
                "Assets/_Spaces/Scripts/Editor/SpacesManager/SelectAssetDialog.cs"
            };
        }
    }

    static string[] SpacesScenesPaths
    {
        get
        {
            return new string[]
            {
                "Assets/_Spaces/Scenes/void.unity",
                "Assets/_Spaces/Scenes/limbo.unity",
                "Assets/_Spaces/Scenes/lobby.unity",
                "Assets/_Spaces/Scenes/boot.unity",
                "Assets/_Spaces/Scenes/Templates/360VideoTemplate.unity",
                "Assets/_Spaces/Scenes/Templates/ImageTemplate.unity",
                "Assets/_Spaces/Scenes/Templates/360ScreenTemplate.unity",

            };
        }
    }

    static string[] WidgetsPaths
    {
        get
        {
            return new string[]
            {
                "Assets/_Spaces/Scripts/Core/Widgets/Widget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Asset Widgets/AssetWidget.cs",

                "Assets/_Spaces/Scripts/Core/Widgets/Asset Widgets/SpriteWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Asset Widgets/TextWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Asset Widgets/VideoWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Asset Widgets/ImageWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Asset Widgets/ModelWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Asset Widgets/RawImageWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Asset Widgets/SkyboxWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Asset Widgets/SoundWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Asset Widgets/MaterialWidget.cs",

                "Assets/_Spaces/Scripts/Core/Widgets/Behavior Widgets/PopupWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Behavior Widgets/ScalerWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Behavior Widgets/SpinWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Behavior Widgets/FeedDisplayAdapterWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Behavior Widgets/HotspotWidget.cs",
                //"Assets/_Spaces/Scripts/Core/Widgets/Behavior Widgets/MoveWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Behavior Widgets/PhysicsWidget.cs",

                "Assets/_Spaces/Scripts/Core/Widgets/Functionallity Widgets/PlayControlsWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Functionallity Widgets/PlaylistWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Functionallity Widgets/PortalWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Functionallity Widgets/ProgressMeterWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Functionallity Widgets/TeleporterWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Functionallity Widgets/TimelineWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Functionallity Widgets/TimerWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Functionallity Widgets/AvatarWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Functionallity Widgets/DisplayGridWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Functionallity Widgets/FeedWidget.cs",
                "Assets/_Spaces/Scripts/Core/Widgets/Functionallity Widgets/LocationWidget.cs",

                "Assets/_Spaces/Scripts/FeedManager.cs",
                "Assets/_Spaces/Scripts/MediaPlayerInterface.cs",
                "Assets/_Spaces/Scripts/RenderSortingLayer.cs",
                "Assets/_Spaces/Scripts/SceneLoader.cs",
                "Assets/_Spaces/Scripts/EchoToConsole.cs"
            };
        }
    }

    static string[] UserSession
    {
        get
        {
            return new string[]
            {
                "Assets/_Spaces/Scripts/ClientInterface/UserSession.cs"
            };
        }
    }

    static string[] ClientInterfacePaths
    {
        get
        {
            return new string[]
            {
                "Assets/_Spaces/Scripts/ClientInterface/AssetSelectionInterface.cs",
                "Assets/_Spaces/Scripts/ClientInterface/SpacesInputModule.cs",
                "Assets/_Spaces/Scripts/ClientInterface/SuperUserInterface.cs",
                "Assets/_Spaces/Scripts/ClientInterface/Vive_Clicks.cs",
                "Assets/_Spaces/Scripts/ClientInterface/BeamCaster.cs",
                "Assets/_Spaces/Scripts/ClientInterface/LoginInterface.cs",
                "Assets/_Spaces/Scripts/ClientInterface/MouseLook.cs",
                "Assets/_Spaces/Scripts/ClientInterface/NavigationInterface.cs",

                "Assets/_Spaces/Scripts/ClientInterface/Prefabs/SpaceSelectionMenu.prefab",
                "Assets/_Spaces/Scripts/ClientInterface/Prefabs/UserAvatar.prefab",
                "Assets/_Spaces/Scripts/ClientInterface/Prefabs/UserEntry.prefab",
                "Assets/_Spaces/Scripts/ClientInterface/Prefabs/UserItem.prefab",
                "Assets/_Spaces/Scripts/ClientInterface/Prefabs/UserSelectionMenu.prefab",
                "Assets/_Spaces/Scripts/ClientInterface/Prefabs/UserSession.prefab",
                "Assets/_Spaces/Scripts/ClientInterface/Prefabs/EventSystem.prefab",
                "Assets/_Spaces/Scripts/ClientInterface/Prefabs/GazeReticle.prefab",
                "Assets/_Spaces/Scripts/ClientInterface/Prefabs/GazeReticleSprite.prefab",
                "Assets/_Spaces/Scripts/ClientInterface/Prefabs/MasterNavInterface.prefab",
                "Assets/_Spaces/Scripts/ClientInterface/Prefabs/Asset Selector Interface.prefab",
                "Assets/_Spaces/Scripts/ClientInterface/Prefabs/Progress Meter Widget.prefab",
                "Assets/_Spaces/Scripts/ClientInterface/Prefabs/SpaceEntry.prefab",

                "Assets/_Spaces/Scripts/ClientInterface/GazeSystem/GazeInputModule.cs",
                "Assets/_Spaces/Scripts/ClientInterface/GazeSystem/GazeReticle.cs",
                "Assets/_Spaces/Scripts/ClientInterface/GazeSystem/WorldspaceGraphicRaycaster.cs",
                "Assets/_Spaces/Scripts/ClientInterface/GazeSystem/HowTo.txt",
                "Assets/_Spaces/Scripts/ClientInterface/GazeSystem/GazeCaster.cs",
                "Assets/_Spaces/Scripts/ClientInterface/GazeSystem/GazeGizmo.cs",
                "Assets/_Spaces/Scripts/Extensions/TransformExtensions.cs"
            };
        }
    }

    static string[] WidgetsPrefabsPaths
    {
        get
        {
            return new string[]
            {
                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/Asset Widgets/Sound Widget.prefab",
                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/Asset Widgets/Spaces_Intro.prefab",
                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/Asset Widgets/TextWidget.prefab",
                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/Asset Widgets/Video Widget.prefab",
                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/Asset Widgets/360 Video Widget.prefab",
                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/Asset Widgets/360 Screen.prefab",
                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/Asset Widgets/Image Widget.prefab",
                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/Asset Widgets/Model Widget - Capsule.prefab",
                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/Asset Widgets/Model Widget - Cube.prefab",
                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/Asset Widgets/Model Widget - Cylinder.prefab",
                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/Asset Widgets/Model Widget - Sphere.prefab",

                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/Behavior Widgets/Hotspot Widget.prefab",

                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/Functionality Widgets/Video Playlist Widget.prefab",
                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/Functionality Widgets/Progress Meter Widget.prefab",
                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/Functionality Widgets/Sound Playlist Widget.prefab",
                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/Functionality Widgets/Timer Widget.prefab",

                "Assets/_Spaces/Scripts/Widget Examples/ExamplePrefabs/ParticleWidget.prefab"
            };
        }
    }

    static string[] SpacesShaderPaths
    {
        get
        {
            return new string[]
            {
                "Assets/_Spaces/Stuff/Shaders/HUD Draw On Top.shader",
                "Assets/_Spaces/Stuff/Shaders/SpacesPolarCoords.shader",
                "Assets/_Spaces/Stuff/Shaders/SpacesPolarCoordsStereo.shader",
                "Assets/_Spaces/Stuff/Shaders/SpacesScrollY_Unlit_transparent.shader",
                "Assets/_Spaces/Stuff/Shaders/SpacesStereoVideoOU.shader",
                "Assets/_Spaces/Stuff/Shaders/Standard-Doublesided.shader",
                "Assets/_Spaces/Stuff/Shaders/UnlitVideoIntertedV.shader",
                "Assets/_Spaces/Stuff/Shaders/Default Font Draw On Top.shader",
                "Assets/_Spaces/Stuff/Shaders/Double Sided Emissive Diffuse.shader",
                "Assets/_Spaces/Stuff/Shaders/Double Sided Standard Diffuse Bump.shader",
                "Assets/_Spaces/Stuff/Shaders/Double Sided Transparent Diffuse Bump Cutout.shader",
                "Assets/_Spaces/Stuff/Shaders/Double Sided Transparent Diffuse Bump.shader"
            };
        }
    }

    static string[] SpacesStuffPaths
    {
        get
        {
            return new string[]
            {
                "Assets/_Spaces/Stuff/Logo Animator Controller.controller",
                "Assets/_Spaces/Stuff/SPACES_logo_Dodecahedron_Ball.fbx",
                "Assets/_Spaces/Stuff/SpacesLogo.prefab",

                "Assets/_Spaces/Stuff/Audio/ui_menu_hide.wav",
                "Assets/_Spaces/Stuff/Audio/ui_menu_highlight.wav",
                "Assets/_Spaces/Stuff/Audio/ui_menu_show.wav",
                "Assets/_Spaces/Stuff/Audio/ui_menu_click.wav",

                "Assets/_Spaces/Stuff/Materials/DefaultOverlayCanvas.mat",
                "Assets/_Spaces/Stuff/Materials/DefaultStandard.mat",
                "Assets/_Spaces/Stuff/Materials/DefaultUnlit.mat",
                "Assets/_Spaces/Stuff/Materials/DefaultVideo.mat",
                "Assets/_Spaces/Stuff/Materials/Reticle.mat",
                "Assets/_Spaces/Stuff/Materials/SPACES_MAT.mat",
                "Assets/_Spaces/Stuff/Materials/Test.mat",
                "Assets/_Spaces/Stuff/Materials/UIVideoTexture.mat",
                "Assets/_Spaces/Stuff/Materials/360Stereo.mat",
                "Assets/_Spaces/Stuff/Materials/AlphaTransparent.mat",
                "Assets/_Spaces/Stuff/Materials/Hotspot.mat",

                "Assets/_Spaces/Stuff/Models/innerSphere.fbx",
                "Assets/_Spaces/Stuff/Models/innerSphere_hi.fbx",
                "Assets/_Spaces/Stuff/Models/Screen_Mesh.fbx",
                "Assets/_Spaces/Stuff/Models/innerCube.fbx",

                "Assets/_Spaces/Stuff/Models/Materials/stereo360R.mat",
                "Assets/_Spaces/Stuff/Models/Materials/videoQuadMat.mat",
                "Assets/_Spaces/Stuff/Models/Materials/videostereo360L.mat",
                "Assets/_Spaces/Stuff/Models/Materials/videostereo360R.mat",
                "Assets/_Spaces/Stuff/Models/Materials/videostereoL.mat",
                "Assets/_Spaces/Stuff/Models/Materials/videostereoR.mat",
                "Assets/_Spaces/Stuff/Models/Materials/FTWD2_208_RF_0317_0210.mat",
                "Assets/_Spaces/Stuff/Models/Materials/innerCubeMat.mat",
                "Assets/_Spaces/Stuff/Models/Materials/innerSphereMat.mat",
                "Assets/_Spaces/Stuff/Models/Materials/mono360.mat",
                "Assets/_Spaces/Stuff/Models/Materials/monoVideo360.mat",
                "Assets/_Spaces/Stuff/Models/Materials/stereo360L.mat",

                "Assets/_Spaces/Stuff/Prefab/prefab_setup.unity",

                "Assets/_Spaces/Stuff/Prefab/360/mono360.prefab",
                "Assets/_Spaces/Stuff/Prefab/360/stereo360.prefab",

                "Assets/_Spaces/Stuff/Prefab/Camera/StereoCamera.prefab",
                "Assets/_Spaces/Stuff/Prefab/Camera/StereoCameraUI.prefab",

                "Assets/_Spaces/Stuff/Prefab/UI/UI_Transform.prefab",
                "Assets/_Spaces/Stuff/Prefab/UI/FixSpaceMenuZPos.cs",
                "Assets/_Spaces/Stuff/Prefab/UI/Column.prefab",
                "Assets/_Spaces/Stuff/Prefab/UI/SpaceMenu.prefab",

                "Assets/_Spaces/Stuff/Prefab/Video/monoVideoQuad.prefab",
                "Assets/_Spaces/Stuff/Prefab/Video/stereoVideo360.prefab",
                "Assets/_Spaces/Stuff/Prefab/Video/stereoVideoQuad.prefab",
                "Assets/_Spaces/Stuff/Prefab/Video/monoVideo360.prefab",

                "Assets/_Spaces/Stuff/SampleIcons/home.png",
                "Assets/_Spaces/Stuff/SampleIcons/icon-check-inverted.png",
                "Assets/_Spaces/Stuff/SampleIcons/progressCircle.png",
                "Assets/_Spaces/Stuff/SampleIcons/refresh.png",
                "Assets/_Spaces/Stuff/SampleIcons/checkmark.png",
                "Assets/_Spaces/Stuff/SampleIcons/check-mark-in-white-md.png",
                "Assets/_Spaces/Stuff/SampleIcons/cloud check.png",
                "Assets/_Spaces/Stuff/SampleIcons/cloud offline.png",
                "Assets/_Spaces/Stuff/SampleIcons/cloud reload.png",
                "Assets/_Spaces/Stuff/SampleIcons/cloud upload.png",

                "Assets/_Spaces/Stuff/SampleImages/SampleBird_1.jpg",
                "Assets/_Spaces/Stuff/SampleImages/SampleBird_2.jpg",
                "Assets/_Spaces/Stuff/SampleImages/SampleBird_3.jpg",
                "Assets/_Spaces/Stuff/SampleImages/SampleBird_4.jpg",
                "Assets/_Spaces/Stuff/SampleImages/Unlit.mat",
                "Assets/_Spaces/Stuff/SampleImages/Spaces_Loading.png",
                "Assets/_Spaces/Stuff/SampleImages/SpacesLoading.anim",
                "Assets/_Spaces/Stuff/SampleImages/SpacesLoading2.anim",
                "Assets/_Spaces/Stuff/SampleImages/Spaces_Loading_0.controller",
                "Assets/_Spaces/Stuff/SampleImages/Spaces_Loading_1.controller",
                "Assets/_Spaces/Stuff/SampleImages/360_sbs_3_Dtest2_prev.jpg",
                "Assets/_Spaces/Stuff/SampleImages/360Image_UmbrellaonaBridge.JPG",
                "Assets/_Spaces/Stuff/SampleImages/mars_for_vr_spherical_v08_4k-over-under.jpg",
                "Assets/_Spaces/Stuff/SampleImages/publicDomainImage.jpg",

                "Assets/_Spaces/Stuff/UI_Sphere/UI_Sphere.prefab",
                "Assets/_Spaces/Stuff/UI_Sphere/Inverted_sphere.fbx",

                "Assets/_Spaces/Stuff/UI_Sphere/Materials/lambert1.mat",
                "Assets/_Spaces/Stuff/UI_Sphere/Materials/StartLogo.mat",
                "Assets/_Spaces/Stuff/UI_Sphere/Materials/barna_lp_sky1.mat",
                "Assets/_Spaces/Stuff/UI_Sphere/Materials/InvertedSphere_MAT.mat",

                "Assets/StreamingAssets/Spaces_logo_canyon_alpha720.mov"
            };
        }
    }


    static string[] GetUploaderPaths()
    {
        var exportList = new List<string>();

        exportList.AddRange(SpacesCorePaths);
        exportList.AddRange(UserSession);
        exportList.AddRange(UploaderPaths);
        exportList.AddRange(BestHTTPPath);

        return exportList.ToArray();
    }

    static string[] GetToolsPaths()
    {
        var exportList = new List<string>();

        exportList.AddRange(SpacesCorePaths);
        exportList.AddRange(BestHTTPPath);
        exportList.AddRange(SpacesScenesPaths);
        exportList.AddRange(UserSession);
        exportList.AddRange(ClientInterfacePaths);
        exportList.AddRange(WidgetsPaths);
        exportList.AddRange(WidgetsPrefabsPaths);

        exportList.AddRange(SpacesShaderPaths);
        exportList.AddRange(SpacesStuffPaths);

        return exportList.ToArray();
    }

    //[MenuItem("Spaces/Export/Export Uploader Package")]
    static void ExportUploaderPackage()
    {
        AssetDatabase.ExportPackage(GetUploaderPaths(), UPLOADER_PATH, ExportPackageOptions.Interactive);
    }

    //[MenuItem("Spaces/Export/Export Widget Package")]
    static void ExportToolsPackage()
    {
        AssetDatabase.ExportPackage(GetToolsPaths(), TOOLS_PATH, ExportPackageOptions.Interactive);
    }

    //[MenuItem("Spaces/Export/Export Widget Examples Package")]
    static void ExportWidgetExamplesPackage()
    {
        var exportList = new List<string>();
        exportList.Add("Assets/_Spaces/Widget Examples");

        AssetDatabase.ExportPackage(exportList.ToArray(), WIDGET_EXAMPLES_PATH, ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
    }

    [MenuItem("Spaces/Export/Export SDK Package")]
    static void ExportSDKPackage()
    {
        var exportList = new List<string>();//System.IO.File.ReadAllLines(@"AssetTree.txt"));
        exportList.Add("Assets/_Spaces SDK");
        exportList.Add("Assets/StreamingAssets");

        string path = EditorUtility.SaveFilePanel("Spaces SDK", System.IO.Directory.GetCurrentDirectory(), "SpacesSDK_" + System.DateTime.Now.ToString("yyyyMMdd_HHmm") + ".unitypackage", "");

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.ExportPackage(exportList.ToArray(), path, ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);// | ExportPackageOptions.IncludeDependencies);
        }
    }

    [MenuItem("Spaces/Export/Write Asset Tree")]
    static void WriteOutAssetTree()
    {
        int count = 0;
        List<string> assetTree = new List<string>();

        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(@"AssetTree.txt", false))
        {
            var directoryQueue = new Queue<string>();
            directoryQueue.Enqueue(Application.dataPath);

            while (directoryQueue.Count > 0)
            {
                string currentDirectory = directoryQueue.Dequeue();

                foreach (var dir in System.IO.Directory.GetDirectories(currentDirectory))
                {
                    directoryQueue.Enqueue(dir);
                }
                
                foreach (var file in System.IO.Directory.GetFiles(currentDirectory))
                {
                    int index = file.LastIndexOf(@"/");
                    string path = file.Substring(index + 1);

                    if (System.IO.Path.GetExtension(path) != ".meta")
                    {
                        ++count;
                        assetTree.Add(path);
                    }
                }
            }

            assetTree.Sort();
            assetTree.ForEach(path => sw.WriteLine(path));
        }

        Debug.Log("Done. Files: " + count);
    }
}
