using UnityEngine;
using System.Collections.Generic;

//GET /asset/:id/metadata

//ex: curl -X GET “http://api.spaces.com/asset/17C2F5F6-9332-452B-B539-7D4E05A98DF2/metadata” 
//-H “Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuc3BhY2VzLmNvbSIsImlhdCI6MTQ3MDg2OTUxNSwiZXhwIjoxNTAyNDA1NTE1LCJhdWQiOiJhcGkuc3BhY2VzLmNvbSIsInN1YiI6InlvdWt1IiwiaWQiOiJENkQ3NTY2RS1ENENGLTRGNjEtQTRGMy0wQTkxOUVCMjZCQjYifQ.TObv9AwcWzVdYW-8fdhfxrtreQVrR0qkoF3HH3_JNnY"

//will return:

//‘{“metadata”: {“asset”:{“attrs”:{“pos”: {“x”: 10.319231 ...}’

namespace Spaces.Core.RestAPI
{
    public delegate void RestGetAssetMetadataCallback(bool error, RestGetAssetMetadataResponse response);

    public class RestGetAssetMetadataResponse
    {
        public string metadata;
        public Asset.Metadata metadataObject;
    }

    public class RestGetAssetMetadata : RestEntry
    {
        RestGetAssetMetadataCallback doneCallback;

        public void Run(string assetID, RestGetAssetMetadataCallback inCallback)
        {
            RestManager.Get(this, string.Format(RestManager.Request.GET_ASSET_METADATA, assetID), OnGetAssetMetadataResponse);
            doneCallback = inCallback;
        }

        public void OnGetAssetMetadataResponse(bool error, string response)
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
                    var getAssetMetadataResponse = TinyJSON.Decoder.Decode(response) as TinyJSON.ProxyObject;

                    var assetMetadata = new RestGetAssetMetadataResponse();
                    assetMetadata.metadata = getAssetMetadataResponse.TryGetValue("metadata", out test) ? test.ToString().Trim('"').Replace("\\\"", "\"").Replace("\\\\\"", "\\\"") : "";                    
                    assetMetadata.metadataObject = JsonUtility.FromJson<Asset.Metadata>(assetMetadata.metadata);

                    if (assetMetadata.metadataObject != null && assetMetadata.metadataObject.node.id == 0)
                        assetMetadata.metadataObject.node = null;

                    doneCallback(false, assetMetadata);
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
