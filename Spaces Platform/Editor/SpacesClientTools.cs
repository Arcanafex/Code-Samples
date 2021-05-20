using UnityEngine;
using UnityEditor;
using System.IO;

public class SpacesClientTools
{
    [MenuItem("Spaces/Clear Cache")]
    public static void ClearCache()
    {
        PlayerPrefs.DeleteKey(Spaces.Core.Constants.SPACES_LIST);
        PlayerPrefs.DeleteKey(Spaces.Core.Constants.ASSETS_LIST);

        if (Directory.Exists(Spaces.Core.Constants.CACHE))
        {
            Directory.Delete(Spaces.Core.Constants.CACHE, true);
        }

        Caching.CleanCache();

        Debug.Log("Spaces Client Cache cleared.");
    }
}