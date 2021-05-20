using UnityEngine;
using UnityEditor;
using System.Collections;

public class PostProcessingTest : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        var importer = assetImporter as ModelImporter;

        string bundleName = System.IO.Path.GetFileNameWithoutExtension(importer.assetPath).ToLower();
        Debug.Log("Setting bundleName to " + bundleName);
        importer.assetBundleName = bundleName;
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string asset in importedAssets)
        {
            Debug.Log("Reimported Asset: " + asset);

            //if (System.IO.Path.GetExtension(asset) == ".fbx")
            //{
            //    Debug.Log("Building a bundle for " + asset);
            //    // Create the array of bundle build details.
            //    AssetBundleBuild[] buildMap = new AssetBundleBuild[1];

            //    buildMap[0].assetBundleName = System.IO.Path.GetFileNameWithoutExtension(asset);//"spaceball";

            //    string[] ballAssets = new string[1];
            //    ballAssets[0] = asset;//"Assets/_Spaces/Stuff/SpacesLogo.prefab";

            //    buildMap[0].assetNames = ballAssets;

            //    if (!System.IO.Directory.Exists("Bundle"))
            //        System.IO.Directory.CreateDirectory("Bundle");

            //    BuildPipeline.BuildAssetBundles("Bundle", buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            //}

            if (System.IO.Path.GetExtension(asset) == ".shader")
            {
                var myShader = AssetDatabase.LoadAssetAtPath<Shader>(asset);
                var widget = Spaces.Core.ShaderInterface.CreateInterface(myShader);
                AssetDatabase.CreateAsset(widget, asset.Replace(".shader", ".asset"));

                int count = ShaderUtil.GetPropertyCount(myShader);
                for (int i = 0; i < count; i++)
                {
                    var prop = new Spaces.Core.ShaderInterface.ShaderProperty()
                    {
                        name = ShaderUtil.GetPropertyName(myShader, i),
                        type = (Spaces.Core.ShaderInterface.ShaderPropertyType)ShaderUtil.GetPropertyType(myShader, i),
                        hidden = ShaderUtil.IsShaderPropertyHidden(myShader, i),
                        texDim = ShaderUtil.GetTexDim(myShader, i),
                        range = new Spaces.Core.ShaderInterface.ShaderRangeProperty()
                        {
                            def = ShaderUtil.GetRangeLimits(myShader, i, 0),
                            min = ShaderUtil.GetRangeLimits(myShader, i, 1),
                            max = ShaderUtil.GetRangeLimits(myShader, i, 2)
                        }
                    };

                    widget.properties.Add(prop);
                }
            }
        }

        foreach (string str in deletedAssets)
        {
            Debug.Log("Deleted Asset: " + str);
        }

        for (int i = 0; i < movedAssets.Length; i++)
        {
            Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
        }
    }
}