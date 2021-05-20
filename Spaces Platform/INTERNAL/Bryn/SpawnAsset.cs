using UnityEngine;
using System.Collections;
using Spaces.Extensions;
using System.Linq;

public class SpawnAsset : MonoBehaviour
{
    public string AssetID;
    public GameObject colliderGenTest;
    public bool local;

    private int frame;
    private bool modelInstanced;

    void Start()
    {
        frame = 0;
        StartCoroutine(RealizeThisAsset());
        //GenerateCollider();
    }

    void Update()
    {
        frame++;
    }

    IEnumerator RealizeThisAsset()
    {
        var myAsset = new Spaces.Core.Asset(AssetID);
        myAsset.onProcessBegin += StateChangeHandler;
        myAsset.onProcessEnd += StateChangeHandler;


        while (!myAsset.InProcess(Spaces.Core.Asset.Process.Idle) && !myAsset.InProcess(Spaces.Core.Asset.Process.Error))
        {
            Debug.Log("[Frame " + frame + "] Fetching " + myAsset.id);
            yield return null;
        }

        var GO = myAsset.SpawnAssetInstance();

        if (colliderGenTest)
            GO.transform.SetParent(colliderGenTest.transform, false);

       // GO.GetComponent<Spaces.Core.ModelWidget>().onModelInstanced += ModelInstancedHandler;
        var bounds = local ? GO.transform.CalculateLocalBounds() : GO.transform.CalculateBounds();

        if (!myAsset.InProcess(Spaces.Core.Asset.Process.Error))
        {
            while (myAsset.InProcess(Spaces.Core.Asset.Process.Downloading) && !myAsset.InProcess(Spaces.Core.Asset.Process.Error))
            {
                bounds = local ? GO.transform.CalculateLocalBounds() : GO.transform.CalculateBounds();
                Debug.Log("[Frame " + frame + "] " + myAsset.name + " (" + this.name + ") " + bounds.center.ToString() + " | " + bounds.size.ToString());
                yield return null;
            }

            int timer = 20;

            while (timer-- > 0)
            {
                bounds = local ? GO.transform.CalculateLocalBounds() : GO.transform.CalculateBounds();
                Debug.Log("[Frame " + frame + "] " + myAsset.name + " (" + this.name + ") " + bounds.center.ToString() + " | " + bounds.size.ToString());
            }
        }
    }

    void GenerateCollider()
    {
        //var bounds = new Bounds(colliderGenTest.transform.position, Vector3.zero);

        foreach (var mesh in colliderGenTest.GetComponentsInChildren<Renderer>())
        {
            if (!mesh.gameObject.GetComponent<Collider>())
                mesh.gameObject.AddComponent<BoxCollider>();
        }

        colliderGenTest.AddComponent<Rigidbody>().isKinematic = true;
    }

    //public static void CalculateLocalBounds(GameObject go)
    //{
    //    Quaternion currentRotation = go.transform.rotation;
    //    go.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

    //    Bounds bounds = new Bounds(go.transform.position, Vector3.zero);

    //    foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
    //    {
    //        bounds.Encapsulate(renderer.bounds);
    //    }

    //    Vector3 localCenter = bounds.center - go.transform.position;
    //    bounds.center = localCenter;

    //    //Debug.Log("Bounds center: " + bounds.center + " | Bounds size: " + bounds.size);

    //    go.transform.rotation = currentRotation;
    //}

    void StateChangeHandler(Spaces.Core.Asset asset, Spaces.Core.Asset.Process[] currentStates, Spaces.Core.Asset.Process state)
    {
        Debug.Log("[Frame " + frame + "] " + asset.name + " (" + asset.id + ") [EVENT!!! " + state.ToString() + "] " + string.Join("|", currentStates.Select(s => s.ToString()).ToArray()));
    }

    //void ModelInstancedHandler(Spaces.Core.ModelWidget model)
    //{
    //    modelInstanced = true;
    //    Debug.Log("[Frame " + frame + "] " + model.Asset.name + " (" + this.name + ") [EVENT!!! Instanced] ");

    //    //var bounds = model.transform.CalculateLocalBounds();
    //    var bounds = model.transform.CalculateBounds();
    //    Debug.Log("[Frame " + frame + "] " + model.Asset.name + " (" + this.name + ") " + bounds.center.ToString() + " | " + bounds.size.ToString());
    //}
}
