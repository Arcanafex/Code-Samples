using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//GET /asset/:id/data/:rep
//or
//GET /asset/:id/data/


//ex: curl -X GET "http://api.spaces.com/asset/F248BA17-62FD-4622-AD67-C8EE33083878/data/win"  
//-H "Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuc3BhY2VzLmNvbSIsImlhdCI6MTQ3MDg2OTUxNSwiZXhwIjoxNTAyNDA1NTE1LCJhdWQiOiJhcGkuc3BhY2VzLmNvbSIsInN1YiI6InlvdWt1IiwiaWQiOiJENkQ3NTY2RS1ENENGLTRGNjEtQTRGMy0wQTkxOUVCMjZCQjYifQ.TObv9AwcWzVdYW-8fdhfxrtreQVrR0qkoF3HH3_JNnY"




//will return:
//  {"data":
//      {
//          "VitraChair.manifest":{"filename":"133bb8ca","url":"https://s3-us-west-2.amazonaws.com/spaces.upload/906709f8/133bb8ca"},
//          "VitraChair.unity3d":{"filename":"e00bef3e","url":"https://s3-us-west-2.amazonaws.com/spaces.upload/906709f8/e00bef3e"},
//          "win":{"filename":"b8d8ef48","url":"https://s3-us-west-2.amazonaws.com/spaces.upload/906709f8/b8d8ef48"},
//          "win.manifest":{"filename":"17f1c72e","url":"https://s3-us-west-2.amazonaws.com/spaces.upload/906709f8/17f1c72e"}
//      }
//  }


namespace Spaces.Core.RestAPI
{
    public delegate void RestGetAssetDataCallback(bool error, RestGetAssetDataResponse response);

    public class RestGetAssetDataResponse
    {
        public struct AssetDataItem
        {
            public string filename;
            public string url;
        }

        public Dictionary<string, AssetDataItem> data;
    }

    public class RestGetAssetData : RestEntry
    {
        RestGetAssetDataCallback doneCallback;

        public void Run(string assetID, RestGetAssetDataCallback inCallback)
        {
            Run(assetID, "", inCallback);
        }

        public void Run(string assetID, string platform, RestGetAssetDataCallback inCallback)
        {
            RestManager.Get(this, string.Format(RestManager.Request.GET_ASSET_DATA, assetID, platform), GetSpaceMetadataResponse);
            doneCallback = inCallback;
        }

        public void GetSpaceMetadataResponse(bool error, string response)
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
                    var getAssetDataResponse = TinyJSON.Decoder.Decode(response) as TinyJSON.ProxyObject;
                    var data = getAssetDataResponse.TryGetValue("data", out test) ? test as TinyJSON.ProxyObject : new TinyJSON.ProxyObject();

                    var assetData = new RestGetAssetDataResponse()
                    {
                        //data = new Dictionary<string, RestGetAssetDataResponse.AssetDataItem>()
                        data = data.Make<Dictionary<string, RestGetAssetDataResponse.AssetDataItem>>()
                        //metadata = getSpaceMetadataResponse.TryGetValue("metadata", out test) ? test.ToString().Trim('"').Replace("\\\"", "\"").Replace("\\\\\"", "\\\"") : ""
                    };

                    doneCallback(false, assetData);
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    doneCallback(true, null);
                }
            }
        }
    }
}
