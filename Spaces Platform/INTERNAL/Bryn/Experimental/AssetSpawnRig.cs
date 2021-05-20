using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AssetSpawnRig : MonoBehaviour
{
    public Vector3 startPoint;
    public Vector3 cameraOffset;
    public float spacing;
    public float lineHeight;
    public int lineLength;

    public int index;
    public int linePosition;
    public int copies;

    public GameObject placardPrefab;

    public List<Spaces.Core.Asset> assetList;
    private List<GameObject> assetInstances;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Initialize();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SpawnAsset(index);

        if (Input.GetKeyDown(KeyCode.N))
        {
            SpawnAsset(index++);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++index;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --index;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
        }

        if (Input.GetKey(KeyCode.W))
        {
            cameraOffset += Vector3.forward * 0.02f;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            linePosition = Mathf.Clamp(linePosition - 1, 0, assetInstances.Count);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            cameraOffset += Vector3.back * 0.02f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            linePosition = Mathf.Clamp(linePosition + 1, 0, assetInstances.Count);
        }

        Camera.main.transform.position = startPoint + ((Vector3.left * spacing) * linePosition) + cameraOffset;
    }

    void Initialize()
    {
        index = 0;
        linePosition = 0;
        assetList = Spaces.Core.Asset.GetAssetList();
        assetInstances = new List<GameObject>();
    }

    void OnGetAssetListDone(bool error, Spaces.Core.RestAPI.RestGetAssetListResponseData data)
    {
        if (error)
        {
            Debug.LogError("[Get Asset List] Completed with errors");
        }
        else
        {
            Debug.Log("[Get Asset List] Completed successfully with " + data.assets.Count + " items.");
        }
    }

    void SpawnAsset(int assetIndex)
    {
        if (assetList.Count > assetIndex)
        {
            for (int c = 0; c < copies; c++)
            {
                var asset = assetList[assetIndex].SpawnAssetInstance();
                asset.transform.position = startPoint + ((Vector3.left * spacing) * assetInstances.Count);
                assetInstances.Add(asset);

                var widget = asset.GetComponent<Spaces.Core.AssetWidget>();
                widget.OnAssetInstanced.AddListener(Scream);

                linePosition++;
            }
        }
    }

    void Scream()
    {
        Debug.Log("AAAAahhhhh!");
    }

}
