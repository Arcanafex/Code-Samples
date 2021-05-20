using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//GET /assets/all

//curl -X GET “http://api.spaces.com/assets/all” -H "Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuc3BhY2VzLmNvbSIsImlhdCI6MTQ3MDg2OTUxNSwiZXhwIjoxNTAyNDA1NTE1LCJhdWQiOiJhcGkuc3BhY2VzLmNvbSIsInN1YiI6InlvdWt1IiwiaWQiOiJENkQ3NTY2RS1ENENGLTRGNjEtQTRGMy0wQTkxOUVCMjZCQjYifQ.TObv9AwcWzVdYW-8fdhfxrtreQVrR0qkoF3HH3_JNnY"

// will return:
// {"AllAssets":{"all":[
//    {
//	    \"id\":\"EB8E6316-1B0F-497A-A604-C3E90F7C4352\",
//	    \"name\":\"testMusic_AudioType.mp3\",
//	    \"asset_type\":\"audio\"
//    },
//    {
//	    \"id\":\"8E2C37F9-77F5-4423-88DB-471CF7330687\",
//	    \"name\":\"Village_Mushroom_B.fbx\",
//	    \"asset_type\":\"model\"
//    }
// ]}}

namespace Spaces.Core.RestAPI
{
    public delegate void RestGetAssetListCallback(bool error, RestGetAssetListResponseData responseData);

    public class RestGetAssetListRequestData
    { }

    public class RestGetAssetListResponseData
    {
        public class AssetInfo
        {
            public string name;
            public string id;
            public string type;
            //public string url;
        }

        //public List<AssetInfo> assetInfoList;
        public List<Asset> assets;

        public RestGetAssetListResponseData()
        {
            //assetInfoList = new List<AssetInfo>();
            assets = new List<Asset>();
        }
    }

    public class RestGetAssetList : RestEntry
    {
        RestGetAssetListCallback doneCallback;
        List<Spaces.Core.Asset> assets;

        public void Run(RestGetAssetListCallback inCallback)
        {
            RestManager.Get(this, RestManager.Request.GET_ASSET_LIST, GetAssetListReply);
            doneCallback = inCallback;
        }

        public void Run(ref List<Spaces.Core.Asset> assetList, RestGetAssetListCallback inCallback)
        {
            assets = assetList;
            RestManager.Get(this, RestManager.Request.GET_ASSET_LIST, GetAssetListReply);
            doneCallback = inCallback;
        }

        public void GetAssetListReply(bool error, string reply)
        {
            if (error)
            {
                doneCallback(true, null);
            }
            else
            {
                try
                {
                    TinyJSON.Variant test;
                    var GetListReponse = TinyJSON.Decoder.Decode(reply) as TinyJSON.ProxyObject;

                    var allAssets = GetListReponse.TryGetValue("AllAssets", out test) ? test as TinyJSON.ProxyObject : new TinyJSON.ProxyObject();
                    var array = allAssets.TryGetValue("all", out test) ? test as TinyJSON.ProxyArray : new TinyJSON.ProxyArray();

                    List<RestGetAssetListResponseData.AssetInfo> assetListInfo = new List<RestGetAssetListResponseData.AssetInfo>();

                    for (int i = 0; i < array.Count; i++)
                    {
                        var assetID = ((TinyJSON.ProxyObject)array[i]).TryGetValue("id", out test) ? test : string.Empty;
                        var assetName = ((TinyJSON.ProxyObject)array[i]).TryGetValue("name", out test) ? test : string.Empty;
                        var assetType = ((TinyJSON.ProxyObject)array[i]).TryGetValue("asset_type", out test) ? test : string.Empty;
                        //var assetPreview = ((TinyJSON.ProxyObject)array[i]).TryGetValue("url", out test) ? test : string.Empty;

                        assetListInfo.Add(new RestGetAssetListResponseData.AssetInfo() { id = assetID, name = assetName, type = assetType });
                    }

                    assetListInfo.Sort((a1, a2) => a1.name.CompareTo(a2.name) == 0 ? a1.id.CompareTo(a2.id) : a1.name.CompareTo(a2.name));

                    assetListInfo.ForEach(info => assets.Add(new Asset(info.id, info.name, info.type)));

                    var assetsList = new RestGetAssetListResponseData()
                    {
                        assets = assets
                    };

                    doneCallback(false, assetsList);
                }
                catch (System.Exception ex)
                {
                    Debug.Log("Tried but " + ex);
                }
            }
        }
    }
}