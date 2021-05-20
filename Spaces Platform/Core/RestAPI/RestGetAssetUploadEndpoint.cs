using UnityEngine;
using System.Collections;

// /upload/url/assetbundle/
// ex: curl -X GET "http://api.spaces.com:8082/upload/url/assetbundle/38D9154B-7C2D-4F7D-B1E6-B10BA0EFCF84"
// --data-binary "file.path"
// will return: {"upload_url":"http://api.spaces.com:8082/upload/space/v1/asset?upload_type=resumable%26upload_id=A0hw7Y_a"}

namespace Spaces.Core.RestAPI
{
    public delegate void RestGetAssetUploadEndpointCallback(bool error, GetAssetUploadEndpointResponseData response);

    public class GetAssetUploadEndpointResponseData
    {
        public string uploadUrl;
    }

    public class RestGetAssetUploadEndpoint : RestEntry
    {
        RestGetAssetUploadEndpointCallback doneCallback;

        public void Run(string assetType, string assetID, RestGetAssetUploadEndpointCallback inCallback)
        {
            RestManager.Get(this, RestManager.Request.GET_ASSET_UPLOAD_ENDPOINT + assetType + "/" + assetID, AssetUploadEndpointReply);
            doneCallback = inCallback;
        }

        public void AssetUploadEndpointReply(bool error, string reply)
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

                    GetAssetUploadEndpointResponseData assetUploadPath = new GetAssetUploadEndpointResponseData()
                    {
                        uploadUrl = createAssetRespone.TryGetValue("upload_url", out test) ? WWW.UnEscapeURL(test) : string.Empty
                    };

                    doneCallback(false, assetUploadPath);
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