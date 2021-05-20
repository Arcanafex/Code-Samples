using UnityEngine;
using System.Collections;

// PATCH /asset/:id

// curl -X PATCH “http://api.spaces.com/asset/BDE55D40-CBBD-417A-BC0D-367AA6B88D39” “Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuc3BhY2VzLmNvbSIsImlhdCI6MTQ3MDg2OTUxNSwiZXhwIjoxNTAyNDA1NTE1LCJhdWQiOiJhcGkuc3BhY2VzLmNvbSIsInN1YiI6InlvdWt1IiwiaWQiOiJENkQ3NTY2RS1ENENGLTRGNjEtQTRGMy0wQTkxOUVCMjZCQjYifQ.TObv9AwcWzVdYW-8fdhfxrtreQVrR0qkoF3HH3_JNnY" 
// -d ‘{
//      “name”:”Awesome Asset 1”, 
//      “asset_type”: “movfile”, 
//      “owner_id”:”7B4B384D-DE3F-4A3F-9D8A-0DD640660BCA”, 
//      “orig_file_name”: “dopey.mov” 
// }

namespace Spaces.Core.RestAPI
{
    public delegate void RestUpdateAssetCallback(bool error, string response);

    public class RestUpdateAssetRequestData
    {
        public string name;
        public string asset_type;
        public string owner_id;
        public string orig_file_name;
    }

    public class RestUpdateAsset : RestEntry
    {
        RestUpdateAssetCallback doneCallback;

        public void Run(string assetID, string assetName, string assetType, string ownerID, string originalFilename, RestUpdateAssetCallback inCallback)
        {
            var request = new RestUpdateAssetRequestData()
            {
                name = assetName,
                asset_type = assetType,
                owner_id = ownerID,
                orig_file_name = originalFilename
            };

            RestManager.Patch(this, RestManager.Request.UPDATE_ASSET + assetID, TinyJSON.Encoder.Encode(request, TinyJSON.EncodeOptions.NoTypeHints), UpdateAssetResponse);
            doneCallback = inCallback;
        }

        void UpdateAssetResponse(bool error, string reply)
        {
            if (error)
            {
                doneCallback(true, null);
            }
            else
            {
                doneCallback(false, reply);
            }
        }

    }
}