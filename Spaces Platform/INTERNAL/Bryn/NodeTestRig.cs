using UnityEngine;
using Spaces.Core;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class NodeTestRig : MonoBehaviour
{
    public bool bePretty;
    public Transform rootTransform;
    public Node rootNode;

    public Component[] exampleComponent;
    public Widget[] exampleWidget;

    Spaces.Core.Asset asset;

    void Start()
    {
        exampleComponent = rootTransform.GetComponents<Component>();
        exampleWidget = rootTransform.GetComponents<Widget>();

        //var request = new Spaces.Core.RestAPI.RestGetAssetData();
        //request.Run("74CF4C32-130D-4365-BB55-72AAD2CE3931", "win", OnRequestDataResponse);

        asset = new Asset("74CF4C32-130D-4365-BB55-72AAD2CE3931");
        asset.onProcessEnd += OnAssetUpdated;

    }

   void OnAssetUpdated(Spaces.Core.Asset _asset, Spaces.Core.Asset.Process[] last, Spaces.Core.Asset.Process next)
    {
        if (asset == _asset && last.Length > 0)
        {
            Debug.Log(asset.name + " [Switched State] " + last.ToString() + " => " + next.ToString());
        }
       
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("Clicked N");
            StartCoroutine(Node.RenderNodeTree(rootNode));
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("Clicked G");
            rootNode = new Node(rootTransform.gameObject);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Clicked D");
            rootNode = null;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Clicked S");

            Spaces.Core.Space.Save("");//"F2D1BDD0-6DEA-41DD-8285-E7BA5B577C9C");

            //string json = "{\"metadata\": " + JsonUtility.ToJson(rootNode, bePretty) + "}";
            ////string json = "{ \"metadata\": {}}";
            ////json = json.Replace(@"\", "");
            //Debug.Log(json);

            ////foreach (var thingie in exampleWidget)
            ////{
            ////    //string json = TinyJSON.Encoder.Encode(thingie, TinyJSON.EncodeOptions.NoTypeHints | TinyJSON.EncodeOptions.PrettyPrint);
            ////    string json = JsonUtility.ToJson(thingie, true);
            ////    Debug.Log(thingie.GetType().ToString() + " - " + json);
            ////}

            ////PlayerPrefs.SetString("NODE", json);

            ////"F2D1BDD0-6DEA-41DD-8285-E7BA5B577C9C"

            //var restUpdateMetadata = new Spaces.Core.RestAPI.RestUpdateSpaceMetadata();
            //restUpdateMetadata.Run("F2D1BDD0-6DEA-41DD-8285-E7BA5B577C9C", json, OnUpdateMetadataResponse);

            //Debug.Log("Update Metadata Request Sent");
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("Clicked I");
            //string json = PlayerPrefs.GetString("NODE").Replace(@"\\\", @"\");
            //Debug.Log(json);

            //Node reconstitutedNode = JsonUtility.FromJson<Node>(json);
            ////Node reconstitutedNode = JSONTools.LoadFromString<Node>(json);


            //if (reconstitutedNode != null)
            //    StartCoroutine(Node.RenderNodeTree(reconstitutedNode));

            //var restGetMetadata = new Spaces.Core.RestAPI.RestGetSpaceMetadata();
            //restGetMetadata.Run("F2D1BDD0-6DEA-41DD-8285-E7BA5B577C9C", OnGetMetadataResponse);

            //Spaces.Core.Space.LoadGraph("F2D1BDD0-6DEA-41DD-8285-E7BA5B577C9C");

            asset.SpawnAssetInstance();
        }

    }

    void OnUpdateMetadataResponse(bool error, string response)
    {
        if (error)
            Debug.Log("Derp");
        else
            Debug.Log("ZOMG! Metadata update successful!!!!!!1");
    }

    void OnGetMetadataResponse(bool error, Spaces.Core.RestAPI.RestGetSpaceMetadataResponse metadata)
    {
        if (error)
            Debug.Log("Retrieval fail. Blurgh.");
        else
        {
            Debug.Log("Check out this dope Metadata! :\n" + metadata.metadata);

            Debug.Log("Gonna build me a node!");
            Node reconstitutedNode = JsonUtility.FromJson<Node>(metadata.metadata);

            if (reconstitutedNode != null)
                StartCoroutine(Node.RenderNodeTree(reconstitutedNode));

        }
    }
}