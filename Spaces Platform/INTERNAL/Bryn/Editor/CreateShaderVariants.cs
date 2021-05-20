using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateShaderVariants : EditorWindow
{
    Dictionary<string, bool> Maps;
    List<List<string>> keywordSets;
    Material material;
    string path;
    bool generateSpheres;

    public static string[] ShaderKeywords
    {
        get
        {
            return new string[]
            {
                "_NORMALMAP",
                "_EMISSION",
                "_PARALLAXMAP",
                "_DETAIL_MULX2",
                "_METALLICGLOSSMAP",
                "_SPECGLOSSMAP"
            };
        }
    }

    public static string[] AlphaKeywords
    {
        get
        {
            return new string[]
            {
                "_ALPHATEST_ON",
                "_ALPHABLEND_ON",
                "_ALPHAPREMULTIPLY_ON"
            };
        }
    }


    [MenuItem("Spaces/Generate Shader Variants")]
    private static void OpenGraphEditor()
    {
        var window = EditorWindow.GetWindow<CreateShaderVariants>();
        
        window.material = new Material(Shader.Find("Standard"));
        window.Maps = new Dictionary<string, bool>();

        foreach (string keyword in ShaderKeywords)
        {
            window.Maps.Add(keyword, false);
        }
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        {
            path = EditorGUILayout.TextField("Path", path);
            GUILayout.Label("Shader Keywords to Include:");

            if (GUILayout.Button("Include All"))
            {
                foreach (var keyword in ShaderKeywords)
                    Maps[keyword] = true;
            }

            foreach (var keyword in ShaderKeywords)
            {
                Maps[keyword] = EditorGUILayout.Toggle(keyword, Maps[keyword]);
            }

            generateSpheres = EditorGUILayout.Toggle("Generate Spheres", generateSpheres);

            if (GUILayout.Button("Generate"))
            {
                GenerateMaterials();
            }
        }
        EditorGUILayout.EndVertical();
    }

    void GenerateMaterials()
    {
        keywordSets = new List<List<string>>();

        // first set has no keywords
        keywordSets.Add(new List<string>());

        foreach (var keyword in Maps)
        {
            if (keyword.Value)
            {
                var addedSets = new List<List<string>>();

                for (int i = 0; i < keywordSets.Count; i++)
                {
                    // make copy of existing set
                    var keywordSet = new string[keywordSets[i].Count];
                    keywordSets[i].CopyTo(keywordSet);

                    // create new list and add current keyword
                    var newKeywordSet = new List<string>(keywordSet);
                    newKeywordSet.Add(keyword.Key);
                    addedSets.Add(newKeywordSet);
                }

                keywordSets.AddRange(addedSets);
            }
        }


        var addedAlphaSets = new List<List<string>>();

        // create new list for each alpha
        foreach (var alpha in AlphaKeywords)
        {
            for (int i = 0; i < keywordSets.Count; i++)
            {
                var keywordSet = new string[keywordSets[i].Count];
                keywordSets[i].CopyTo(keywordSet);

                var newAlphaKeywordSet = new List<string>(keywordSet);
                newAlphaKeywordSet.Add(alpha);

                addedAlphaSets.Add(newAlphaKeywordSet);
            }            
        }

        keywordSets.AddRange(addedAlphaSets);

        int count = 0;

        foreach (var set in keywordSets)
        {
            var matVariant = new Material(material);

            string matName = "STANDARD";

            foreach (string keyword in set)
            {
                matVariant.EnableKeyword(keyword);
                matName += keyword;
            }

            AssetDatabase.CreateAsset(matVariant, path + "/" + matName + ".mat");

            if (generateSpheres)
            {
                //Assets/_Spaces SDK/Examples/Materials/Shader Variants
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position += Vector3.left * count++;
                sphere.name = matName;

                var sphereRenderer = sphere.GetComponent<Renderer>();
                sphereRenderer.material = matVariant;
            }
        }
    }
}