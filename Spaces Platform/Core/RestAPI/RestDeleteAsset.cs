using UnityEngine;
using System.Collections;

//curl -X DELETE “http://api.space.com/asset/A51C6ADA-4A98-4B2E-B20E-58D6A958DA73”
//-H "Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuc3BhY2VzLmNvbSIsImlhdCI6MTQ3MDg2OTUxNSwiZXhwIjoxNTAyNDA1NTE1LCJhdWQiOiJhcGkuc3BhY2VzLmNvbSIsInN1YiI6InlvdWt1IiwiaWQiOiJENkQ3NTY2RS1ENENGLTRGNjEtQTRGMy0wQTkxOUVCMjZCQjYifQ.TObv9AwcWzVdYW-8fdhfxrtreQVrR0qkoF3HH3_JNnY"
namespace Spaces.Core.RestAPI
{
    public delegate void RestDeleteAssetCallback(bool error, RestDeleteAssetResponseData reply);

    public class RestDeleteAssetResponseData
    {
        public string assetID;
        public Spaces.Core.Asset asset;
    }

    public class RestDeleteAsset : RestEntry
    {
        RestDeleteAssetCallback doneCallback;
        Spaces.Core.Asset deletingAsset;

        public void Run(string assetID, RestDeleteAssetCallback inCallback)
        {
            deletingAsset = null;
            RestManager.Delete(this, RestManager.Request.DELETE_ASSET + assetID, OnDeleteAssetResponse);
            doneCallback = inCallback;
        }

        public void Run(Spaces.Core.Asset requesterAsset, RestDeleteAssetCallback inCallback)
        {
            deletingAsset = requesterAsset;
            RestManager.Delete(this, RestManager.Request.DELETE_ASSET + deletingAsset.id, OnDeleteAssetResponse);
            doneCallback = inCallback;
        }

        public void OnDeleteAssetResponse(bool error, string response)
        {
            var responseData = new RestDeleteAssetResponseData()
            {
                assetID = deletingAsset.id,
                asset = deletingAsset
            };

            if (error)
            {
                doneCallback(true, responseData);
            }
            else
            {
                doneCallback(false, responseData);
            }
        }

    }
}