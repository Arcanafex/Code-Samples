using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Spaces.Core;
using Spaces.Extensions;

public class AssetsSpawningRig : MonoBehaviour
{
    public int assetIndex;
    private int displayIndex;
    public Asset[] displayAssets;
    private GameObject[] platforms;
    public List<Asset> assetList;

    public Vector3 platformOffset;
    public Vector3 platformSize;

    public UnityEvent OnAssetListPopulated;
    public UnityEvent OnLoadAssetStarted;
    public UnityEvent OnLoadAssetDone;
    public UnityEvent OnAssetGameObjectInstanced;

    void Start()
    {
        displayAssets = new Asset[4];
        platforms = new GameObject[4];
        assetList = Asset.GetAssetList(OnAssetListResponse);

        InitializePlatforms();
    }

    private void InitializePlatforms()
    {
        for (int i = 0; i < platforms.Length; i++)
        {
            var platformLocation = new GameObject("Asset " + i);
            platforms[i] = platformLocation;
            platformLocation.transform.SetParent(transform);
            platformLocation.transform.position = platformOffset;

            var platformModel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platformModel.transform.SetParent(platformLocation.transform);
            platformModel.transform.localPosition = Vector3.zero;
            platformModel.transform.localScale = platformSize;

            var platformModelBounds = platformModel.transform.CalculateLocalBounds();
            platformLocation.transform.localPosition = platformLocation.transform.localPosition + Vector3.up * platformModelBounds.extents.y;
            platformModel.transform.localPosition = Vector3.down * platformModelBounds.extents.y;

            transform.Rotate(Vector3.up, 90, UnityEngine.Space.World);
        }
    }

    public void OnAssetListResponse(bool error, Spaces.Core.RestAPI.RestGetAssetListResponseData resposne)
    {
        if (error)
        {
            Debug.LogError("There was an error getting the Asset List.");
        }
        else
        {
            Debug.Log("Finished getting the Asset List.");
            OnAssetListPopulated.Invoke();
        }
    }

    public void Next()
    {
        if (assetIndex < assetList.Count)
            assetIndex++;
        else
            assetIndex = 0;

        Spawn();
    }

    public void Spawn()
    {
        if (assetIndex < assetList.Count)
        {
            // Clean up the existing isntanced asset, if any.
            var assetWidget = platforms[displayIndex].GetComponentInChildren<AssetWidget>();

            if (assetWidget)
                Destroy(assetWidget.gameObject);

            // Set the next asset.
            displayAssets[displayIndex] = assetList[assetIndex];

            var assetInstance = displayAssets[displayIndex].SpawnAssetInstance();
            assetInstance.transform.SetParent(platforms[displayIndex].transform, false);
            assetWidget = assetInstance.GetComponent<AssetWidget>();

            // Immediate Adjustment will affect just the Progress Meter unless the Asset was preloaded.
            AdjustInstancedAsset(assetInstance.transform, platforms[displayIndex].transform);

            // Actions for when AssetWidget signals the model has been instanced.
            assetWidget.OnAssetInstanced.AddListener(
                delegate
                {
                    AdjustInstancedAsset(assetInstance.transform, platforms[displayIndex].transform);
                    WriteNode(assetInstance.transform);
                });

            displayIndex = ++displayIndex % 4;
            Debug.Log("Display index (" + displayIndex + ") " + displayAssets[displayIndex].name);
        }
    }

    public void Skip()
    {
        assetIndex++;
    }

    public void AssetLoadStart(Asset asset)
    {
        OnLoadAssetStarted.Invoke();
    }

    public void AssetLoadDone(Asset asset)
    {
        OnLoadAssetDone.Invoke();
    }

    public void GetGreenSphere(Transform parent)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(parent);
        var renderer = sphere.GetComponent<Renderer>();

        if (renderer)
            renderer.material.color = Color.green;
    }

    public void AdjustInstancedAsset(Transform adjustee, Transform reference)
    {
        var bounds = adjustee.CalculateBounds();
        //Vector3 posAdjustment = new Vector3(-bounds.center.x, bounds.extents.y, -bounds.center.z);

        adjustee.localPosition = Vector3.up * bounds.extents.y;
    }

    public void WriteNode(Transform node)
    {
        var myNode = new Node(node.gameObject);

        Debug.Log(JsonUtility.ToJson(myNode));
    }
}
