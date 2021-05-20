using UnityEngine;
using System.Collections.Generic;

//POST /asset/:id/metadata/

//curl -X POST “http://api.spaces.com/asset/BDE55D40-CBBD-417A-BC0D-367AA6B88D39/metadata/” 
//-H “Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuc3BhY2VzLmNvbSIsImlhdCI6MTQ3MDg2OTUxNSwiZXhwIjoxNTAyNDA1NTE1LCJhdWQiOiJhcGkuc3BhY2VzLmNvbSIsInN1YiI6InlvdWt1IiwiaWQiOiJENkQ3NTY2RS1ENENGLTRGNjEtQTRGMy0wQTkxOUVCMjZCQjYifQ.TObv9AwcWzVdYW-8fdhfxrtreQVrR0qkoF3HH3_JNnY” 
//-d ‘{“metadata”:{“attrs”:{“pos”: {“x”: 10.319231 ...}’

//will return: {} 

namespace Spaces.Core.RestAPI
{
    public delegate void RestUpdateAssetMetadataCallback(bool error, string response);

    public class RestUpdateAssetMetadataRequest
    {
        //public Spaces.Core.Asset.Metadata metadata;
        public string metadata;
    }

    public class RestUpdateAssetMetadata : RestEntry
    {
        RestUpdateAssetMetadataCallback doneCallback;

        //public void Run(string assetID, string json, RestUpdateAssetMetadataCallback inCallback)
        //{
        //    doneCallback = inCallback;

        //    var metadata = new RestUpdateAssetMetadataRequest()
        //    {
        //        metadata = json
        //    };

        //    RestManager.Post(this, string.Format(RestManager.Request.UPDATE_ASSET_METADATA, assetID), TinyJSON.Encoder.Encode(metadata, TinyJSON.EncodeOptions.NoTypeHints), OnUpdateAssetMetadataResponse);
        //}

        public void Run(string assetID, Spaces.Core.Asset.Metadata json, RestUpdateAssetMetadataCallback inCallback)
        {
            doneCallback = inCallback;

            var metadata = new RestUpdateAssetMetadataRequest()
            {
                metadata = JsonUtility.ToJson(json)
            };

            RestManager.Post(this, string.Format(RestManager.Request.UPDATE_ASSET_METADATA, assetID), TinyJSON.Encoder.Encode(metadata, TinyJSON.EncodeOptions.NoTypeHints), OnUpdateAssetMetadataResponse);
        }

        public void OnUpdateAssetMetadataResponse(bool error, string response)
        {
            if (error)
            {
                doneCallback(true, null);
            }
            else
            {
                doneCallback(false, "Success");
            }
        }
    }
}
