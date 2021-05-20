using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// GET /asset/:id/thumb/:res

// Ex: curl -X GET “http://api.spaces.com/asset/8E2C37F9-77F5-4423-88DB-471CF7330687/thumb/hi/” -H “Authorization: Bearer [token]”

// will return:
// {"url":"https://s3-us-west-2.amazonaws.com/spaces.upload/034441bc/7fd68ba3"}


namespace Spaces.Core.RestAPI
{
    public delegate void RestGetAssetPreviewCallback(bool error, RestGetAssetPreviewResponse response);

    public class RestGetAssetPreviewResponse
    {
        public string url;
    }

    public class RestGetAssetPreview : RestEntry
    {
        RestGetAssetPreviewCallback doneCallback;

        public void Run(string assetID, RestGetAssetPreviewCallback inCallback)
        {
            Run(assetID, "", inCallback);
        }

        public void Run(string assetID, string resolution, RestGetAssetPreviewCallback inCallback)
        {
            RestManager.Get(this, string.Format(RestManager.Request.GET_ASSET_PREVIEW, assetID, resolution), GetSpaceMetadataResponse);
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
                    var getPreviewRespone = TinyJSON.Decoder.Decode(response) as TinyJSON.ProxyObject;

                    var assetUploadPath = new RestGetAssetPreviewResponse()
                    {
                        url = getPreviewRespone.TryGetValue("url", out test) ? WWW.UnEscapeURL(test) : string.Empty
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
