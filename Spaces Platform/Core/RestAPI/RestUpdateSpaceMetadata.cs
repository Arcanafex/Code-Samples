using UnityEngine;
using System.Collections;

//add space metadata
//POST /space/:id/metadata/

//curl -X POST “http://api.spaces.com/space/42BA9E71-EA47-4DB7-8BBD-824CEE92FF01/metadata/” 
//-H “Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuc3BhY2VzLmNvbSIsImlhdCI6MTQ3MDg2OTUxNSwiZXhwIjoxNTAyNDA1NTE1LCJhdWQiOiJhcGkuc3BhY2VzLmNvbSIsInN1YiI6InlvdWt1IiwiaWQiOiJENkQ3NTY2RS1ENENGLTRGNjEtQTRGMy0wQTkxOUVCMjZCQjYifQ.TObv9AwcWzVdYW-8fdhfxrtreQVrR0qkoF3HH3_JNnY” 
//-d ‘{“metadata”:{“object1”:{“pos”: {“x”: 10.319231 ...}’

//will return: {} 

namespace Spaces.Core.RestAPI
{
    public delegate void RestUpdateSpaceMetadataCallback(bool error, RestUpdateSpaceMetadataResponse response);

    public class RestUpdateSpaceMetadataRequest
    {
        public string metadata;
    }

    public class RestUpdateSpaceMetadataResponse
    {
        public Space space;
        public SpaceGraph graph;
    }

    public class RestUpdateSpaceMetadata : RestEntry
    {
        RestUpdateSpaceMetadataCallback doneCallback;
        string spaceID;
        string jsonMetadata;

        public void Run(string spaceID, string json, RestUpdateSpaceMetadataCallback inCallback)
        {
            doneCallback = inCallback;
            this.spaceID = spaceID;
            jsonMetadata = json;
            string request = string.Format(RestManager.Request.UPDATE_SPACE_METADATA, spaceID);

            var metadata = new RestUpdateSpaceMetadataRequest()
            {
                metadata = json
            };

            RestManager.Post(this, request, TinyJSON.Encoder.Encode(metadata, TinyJSON.EncodeOptions.NoTypeHints), GetSpaceMetadataResponse);
        }

        public void GetSpaceMetadataResponse(bool error, string response)
        {
            var updateResponse = new RestUpdateSpaceMetadataResponse()
            {
                space = Spaces.Core.Space.SpacesManager.GetSpace(this.spaceID),
                graph = JsonUtility.FromJson<SpaceGraph>(jsonMetadata)
            };

            if (updateResponse.space == null)
                updateResponse.space = new Space(this.spaceID);

            if (updateResponse.graph != null)
                updateResponse.space.Graph = updateResponse.graph;

            if (error)
            {
                doneCallback(true, updateResponse);
            }
            else
            {
                doneCallback(false, updateResponse);
            }
        }
    }
}