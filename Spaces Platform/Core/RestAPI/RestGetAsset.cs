using UnityEngine;
using System.Collections;
using Spaces.Core;

// GET "http://api.spaces.com/asset/0F2E4A39-6121-4C7E-862E-8F6B4B44189D"

//{"Asset":{
//"id":"0F2E4A39-6121-4C7E-862E-8F6B4B44189D",
//"name":"Thunder Lizard",
//"s3bucket":"spaces.upload",
//"key":"a0fc9263/c0de247c",
//"asset_type":"image",
//"modified_at":"2016-09-19T18:49:15.507784703Z",
//"created_at":"2016-09-19T18:49:15.507784703Z",
//"creator_id":"7B4B384D-DE3F-4A3F-9D8A-0DD640660BCA",
//"s3url":"https://s3-us-west-2.amazonaws.com/spaces.upload/a0fc9263/c0de247c",
//"orig_file_name":"SampleBird_3.jpg",
//"owner_id":"7B4B384D-DE3F-4A3F-9D8A-0DD640660BCA"}}

namespace Spaces.Core.RestAPI
{
    public delegate void RestGetAssetCallback(bool error, RestGetAssetResponseData responseData);

    public class RestGetAssetResponseData
    {
        public string id;
        public string owner_id;
        public string name;
        public string s3bucket;
        public string key;
        public string asset_type;
        public string modified_at;
        public string created_at;
        public string s3url;
        public string orig_file_name;
    }

    public class RestGetAsset : RestEntry
    {
        RestGetAssetCallback doneCallback;

        public void Run(string assetID, RestGetAssetCallback inCallback)
        {
            RestManager.Get(this, RestManager.Request.GET_ASSET + assetID, GetAssetReply);
            doneCallback = inCallback;
        }

        public void GetAssetReply(bool error, string reply)
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

                    var createAssetRespone = TinyJSON.Decoder.Decode(reply) as TinyJSON.ProxyObject;
                    var assetData = createAssetRespone.TryGetValue("Asset", out test) ? test as TinyJSON.ProxyObject : new TinyJSON.ProxyObject();


                    var asset = new RestGetAssetResponseData()
                    {
                        id = assetData.TryGetValue("id", out test) ? test : string.Empty,
                        name = assetData.TryGetValue("name", out test) ? test : string.Empty,
                        owner_id = assetData.TryGetValue("owner_id", out test) ? test : string.Empty,
                        s3bucket = assetData.TryGetValue("s3bucket", out test) ? test : string.Empty,
                        key = assetData.TryGetValue("key", out test) ? test : string.Empty,
                        asset_type = assetData.TryGetValue("asset_type", out test) ? test : string.Empty,
                        created_at = assetData.TryGetValue("created_at", out test) ? test : string.Empty,
                        modified_at = assetData.TryGetValue("modified_at", out test) ? test : string.Empty,
                        s3url = assetData.TryGetValue("s3url", out test) ? test : string.Empty,
                        orig_file_name = assetData.TryGetValue("orig_file_name", out test) ? test : string.Empty
                    };

                    doneCallback(false, asset);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                    doneCallback(true, null);
                }
            }
        }
    }
}