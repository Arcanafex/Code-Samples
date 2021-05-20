using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//GET /space/:id/metadata

//ex: curl -X GET “http://api.spaces.com/space/619D9EA3-FEE0-4426-B57A-38074B249863/metadata” 
//-H “Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuc3BhY2VzLmNvbSIsImlhdCI6MTQ3MDg2OTUxNSwiZXhwIjoxNTAyNDA1NTE1LCJhdWQiOiJhcGkuc3BhY2VzLmNvbSIsInN1YiI6InlvdWt1IiwiaWQiOiJENkQ3NTY2RS1ENENGLTRGNjEtQTRGMy0wQTkxOUVCMjZCQjYifQ.TObv9AwcWzVdYW-8fdhfxrtreQVrR0qkoF3HH3_JNnY"

//will return: 
//‘{“metadata”: {“scene”:{“object1”:{“pos”: {“x”: 10.319231 …}}’

namespace Spaces.Core.RestAPI
{
    public delegate void RestGetSpaceMetadataCallback(bool error, RestGetSpaceMetadataResponse response);

    public class RestGetSpaceMetadataResponse
    {
        public string metadata;
        public SpaceGraph spaceGraph;
    }

    public class RestGetSpaceMetadata : RestEntry
    {
        RestGetSpaceMetadataCallback doneCallback;

        public void Run(string spaceID, RestGetSpaceMetadataCallback inCallback)
        {
            RestManager.Get(this, string.Format(RestManager.Request.GET_SPACE_METADATA, spaceID), GetSpaceMetadataResponse);
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
                    var getSpaceMetadataResponse = TinyJSON.Decoder.Decode(response) as TinyJSON.ProxyObject;
                    var spaceMetadata = new RestGetSpaceMetadataResponse();

                    spaceMetadata.metadata = getSpaceMetadataResponse.TryGetValue("metadata", out test) ? test.ToString().Trim('"').Replace("\\\"", "\"").Replace("\\\\\"", "\\\"") : "";
                    spaceMetadata.spaceGraph = JsonUtility.FromJson<SpaceGraph>(spaceMetadata.metadata);

                    if (spaceMetadata.spaceGraph == null)
                        spaceMetadata.spaceGraph = new SpaceGraph();

                    if (spaceMetadata.spaceGraph.nodes == null)
                        spaceMetadata.spaceGraph.nodes = new List<Node>();

                    doneCallback(false, spaceMetadata);
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