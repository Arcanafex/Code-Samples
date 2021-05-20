using UnityEngine;
using System.Collections;

//PUT /asset/

//curl -X PUT “http://api.spaces.com/asset/” 
//-H “Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuc3BhY2VzLmNvbSIsImlhdCI6MTQ3MDg2OTUxNSwiZXhwIjoxNTAyNDA1NTE1LCJhdWQiOiJhcGkuc3BhY2VzLmNvbSIsInN1YiI6InlvdWt1IiwiaWQiOiJENkQ3NTY2RS1ENENGLTRGNjEtQTRGMy0wQTkxOUVCMjZCQjYifQ.TObv9AwcWzVdYW-8fdhfxrtreQVrR0qkoF3HH3_JNnY" 
//-d ‘{
//“space_id”: "754C802F-C04C-4EC7-8746-11A483800BC4", 
//“asset_id”: "2A1F1B0B-FF55-4D25-899C-7E5DFE539453"
//}’

//will return: {"success":"association successfully created"}

namespace Spaces.Core.RestAPI
{
    public delegate void RestAddAssetToSpaceCallback(bool error, RestAddAssetToSpaceResponseData data);

    public class RestAddAssetToSpaceRequestData
    {
        public string space_id;
        public string asset_id;
    }

    public class RestAddAssetToSpaceResponseData
    {
        public string spaceID;
        public string assetID;
    }


    public class RestAddAssetToSpace : RestEntry
    {
        RestAddAssetToSpaceCallback doneCallback;
        private string spaceID;
        private string assetID;

        public void Run(string spaceID, string assetID, RestAddAssetToSpaceCallback inCallback)
        {
            RestAddAssetToSpaceRequestData args = new RestAddAssetToSpaceRequestData()
            {
                space_id = spaceID,
                asset_id = assetID
            };

            this.spaceID = spaceID;
            this.assetID = assetID;

            RestManager.Put(this, RestManager.Request.ADD_ASSET_TO_SPACE, System.Text.Encoding.UTF8.GetBytes(TinyJSON.Encoder.Encode(args, TinyJSON.EncodeOptions.NoTypeHints)), AddAssetToSpaceResponse);
            doneCallback = inCallback;
        }

        public void AddAssetToSpaceResponse(bool error, string response)
        {
            var responsedata = new RestAddAssetToSpaceResponseData()
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