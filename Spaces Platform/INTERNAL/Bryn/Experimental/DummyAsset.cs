using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyAsset : MonoBehaviour
{
    public Spaces.Core.Asset m_asset;
    public string assetBundlePath;
    //public AssetBundle assetBundle;


    void Start()
    {
        m_asset = new Spaces.Core.Asset(System.Guid.NewGuid().ToString(), "Dummy Asset");
        m_asset.assetType = "model";
        m_asset.pathToData = assetBundlePath;
        //m_asset.bundle = AssetBundle.LoadFromFile(assetBundlePath);
        m_asset.SpawnAssetInstance();
    }

    void Update()
    {

    }
}
