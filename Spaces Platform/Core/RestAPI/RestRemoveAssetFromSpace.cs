using UnityEngine;
using System.Collections;

// curl -X DELETE “http://api.space.com/association/” 
// -H  "Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuc3BhY2VzLmNvbSIsImlhdCI6MTQ3MDg2OTUxNSwiZXhwIjoxNTAyNDA1NTE1LCJhdWQiOiJhcGkuc3BhY2VzLmNvbSIsInN1YiI6InlvdWt1IiwiaWQiOiJENkQ3NTY2RS1ENENGLTRGNjEtQTRGMy0wQTkxOUVCMjZCQjYifQ.TObv9AwcWzVdYW-8fdhfxrtreQVrR0qkoF3HH3_JNnY" 
// -d ‘{
//      “space_id”: “754C802F-C04C-4EC7-8746-11A483800BC4”, 
//      “asset_id”:”2A1F1B0B-FF55-4D25-899C-7E5DFE539453”
// }’

namespace Spaces.Core.RestAPI
{

    public delegate void RestRemoveAssetFromSpaceCallback(bool error, RestRemoveAssetFromSpaceResponseData response);

    public class RestRemoveAssetFromSpaceRequestData
    {
        public string space_id;
        public string asset_id;
    }

    public class RestRemoveAssetFromSpaceResponseData
    {
        public string spaceID;
        public string assetID;
    }

    public class RestRemoveAssetFromSpace : RestEntry
    {
        RestRemoveAssetFromSpaceCallback doneCallback;
        private string spaceID;
        private string assetID;

        public void Run(string spaceID, string assetID, RestRemoveAssetFromSpaceCallback inCallback)
        {
            var request = new RestRemoveAssetFromSpaceRequestData()
            {
                asset_id = assetID,
                space_id = spaceID
            };

            this.spaceID = spaceID;
            this.assetID = assetID;

            RestManager.Remove(this, RestManager.Request.REMOVE_ASSET_FROM_SPACE, TinyJSON.Encoder.Encode(request, TinyJSON.EncodeOptions.NoTypeHints), RemoveAssetFromSpaceResponse);
            doneCallback = inCallback;

        }

        public void RemoveAssetFromSpaceResponse(bool error, string response)
        {
            var responsedata = new RestRemoveAssetFromSpaceResponseData()
            {
                spaceID = spaceID,
                assetID = assetID
            };

            if (error)
            {
                doneCallback(true, responsedata);
            }
            else
            {
                doneCallback(false, responsedata);
            }
        }
    }
}