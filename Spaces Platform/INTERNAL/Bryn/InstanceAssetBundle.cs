using UnityEngine;
using System.Collections;

/// <summary>
/// This script will load an assetBundle from the path specified, and then instance it as a child of this gameObject.
/// </summary>
public class InstanceAssetBundle : MonoBehaviour
{
    public string[] assetBundlePaths;

    private AssetBundle[] assetBundles;

    void Start()
    {
        assetBundles = new AssetBundle[assetBundlePaths.Length];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("Starting import");

            for(int i = 0; i < assetBundlePaths.Length; i++)
            {
                if (System.IO.File.Exists(assetBundlePaths[i]))
                    StartCoroutine(LoadAndInstanceBundleContent(assetBundlePaths[i], i));
                else
                    Debug.LogWarning("I'm sorry, Dave. I'm afraid I can't find any file at [" + assetBundlePaths + "].");
            }
        }
    }

    IEnumerator LoadAndInstanceBundleContent(string path, int index)
    {

        if (!assetBundles[index])
        {
            var bundleRequest = AssetBundle.LoadFromFileAsync(path);

            yield return bundleRequest;

            Debug.Log("[Attempting Load] " + path);

            assetBundles[index] = bundleRequest.assetBundle;
        }

        if (assetBundles[index])
        {
            foreach (var gameObjectName in assetBundles[index].GetAllAssetNames())
            {
                var gameObjectRef = assetBundles[index].LoadAsset<GameObject>(gameObjectName);
                GameObject theThingItself = (GameObject)Instantiate(gameObjectRef, transform, false);
                theThingItself.tag = "asset_data";
                assetBundles[index].Unload(false);
            }
        }
        else
        {
            Debug.Log("[FAIL] couldn't load " + path);
        }
    }
}
