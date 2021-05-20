using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpacesShaderTool
{
    [MenuItem("Assets/Generate Shader Widgets")]
    public static void GenerateShaderWidgets()
    {
        if (Selection.activeObject && Selection.activeObject is Spaces.UnityClient.SessionSettings)
        {
            Spaces.UnityClient.SessionSettings settings = Selection.activeObject as Spaces.UnityClient.SessionSettings;

            foreach (var item in settings.shaders)
            {
                if (item.shader)
                {
                    item.shaderInterface = GenerateShaderInterface(item.shader);
                }
            }
        }
    }

    public static Spaces.Core.ShaderInterface GenerateShaderInterface(Shader shader)
    {
        var shaderInterface = Spaces.Core.ShaderInterface.CreateInterface(shader);

        int count = ShaderUtil.GetPropertyCount(shader);
        for (int i = 0; i < count; i++)
        {
            var prop = new Spaces.Core.ShaderInterface.ShaderProperty()
            {
                name = ShaderUtil.GetPropertyName(shader, i),
                type = (Spaces.Core.ShaderInterface.ShaderPropertyType)ShaderUtil.GetPropertyType(shader, i),
                hidden = ShaderUtil.IsShaderPropertyHidden(shader, i),
                texDim = ShaderUtil.GetTexDim(shader, i),
                range = new Spaces.Core.ShaderInterface.ShaderRangeProperty()
                {
                    def = ShaderUtil.GetRangeLimits(shader, i, 0),
                    min = ShaderUtil.GetRangeLimits(shader, i, 1),
                    max = ShaderUtil.GetRangeLimits(shader, i, 2)
                }
            };

            shaderInterface.properties.Add(prop);
        }

        string path = AssetDatabase.GetAssetPath(shader);
        path = path.EndsWith(".shader") ? path.Replace(".shader", ".asset") : "Assets/_Spaces SDK/Client/Interface/" + shader.name.Replace("/", "_") + ".asset";
        AssetDatabase.CreateAsset(shaderInterface, path);

        return shaderInterface;
    }

    //[MenuItem("Assets/Generate Shader Widget", true)]
    //public static bool AmIAShader()
    //{
    //    if (Selection.activeObject && Selection.activeObject is Shader)
    //        return true;
    //    else
    //        return false;
    //}
}
