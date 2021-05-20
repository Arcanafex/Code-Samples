using System.Collections.Generic;
using System.Linq;

// GET  /space/:id
//
// ex: curl -X GET http://api.spaces.com/space/619D9EA3-FEE0-4426-B57A-38074B249863
//
// Will return: {"creator_id":"5EF4AE90-B191-46F6-8729-A9E974A3764D", "name": "Blah Blah Space", "modified_at":"2016-07-06T03:38:30.331227185Z","created_at":"2016-07-06T03:38:30.331227185Z"}
// {"Space":{
//  "id":"C9C4E9AE-967B-4B56-9515-715785C4BFB8",
//  "creator_id":"00000000-0000-0000-0000-000000000000",
//  "name":"The Walking Dead Tour",
//  "base_path":"97a681fc",
//  "modified_at":"2016-07-27T23:33:09.294882678Z",
//  "created_at":"2016-07-27T23:33:09.294882678Z",
//  "owner_id":"00000000-0000-0000-0000-000000000000",
//  "last_accessed_at":"2016-07-27T23:33:09.294882678Z",
//  "asset_ids":["E559F960-AC58-477C-954B-A9B212FB3BAF"]}}

namespace Spaces.Core.RestAPI
{
    public delegate void RestGetSpaceCallback(bool error, RestGetSpaceResponseData spaceData);

    public class RestGetSpaceResponseData
    {
        public string id;
        public string creator_id;
        public string name;
        public string base_path;
        public string modified_at;
        public string created_at;
        public string owner_id;
        public string last_accessed_at;
        public string[] asset_ids;
    }

    public class RestGetSpace : RestEntry
    {
        RestGetSpaceCallback doneCallback;
        private string args;

        public void Run(string spaceID, RestGetSpaceCallback inCallback)
        {
            args = spaceID;
            RestManager.Get(this, RestManager.Request.GET_SPACE + spaceID, GetSpaceReply);
            doneCallback = inCallback;
        }

        public void GetSpaceReply(bool error, string reply)
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
                    var getSpaceResponse = TinyJSON.Decoder.Decode(reply) as TinyJSON.ProxyObject;
                    var space = getSpaceResponse.TryGetValue("Space", out test) ? test as TinyJSON.ProxyObject : new TinyJSON.ProxyObject();

                    var spaceData = new RestGetSpaceResponseData()
                    {
                        //id = space.TryGetValue("id", out test) ? test.ToString() : string.Empty
                        id = args,
                        creator_id = space.TryGetValue("creator_id", out test) ? test.ToString() : string.Empty,
                        owner_id = space.TryGetValue("owner_id", out test) ? test.ToString() : string.Empty,
                        name = space.TryGetValue("name", out test) ? test.ToString() : string.Empty,
                        base_path = space.TryGetValue("base_path", out test) ? test.ToString() : string.Empty,
                        created_at = space.TryGetValue("created_at", out test) ? test.ToString() : string.Empty,
                        modified_at = space.TryGetValue("modified_at", out test) ? test.ToString() : string.Empty,
                        last_accessed_at = space.TryGetValue("last_accessed_at", out test) ? test.ToString() : string.Empty

                        //created = space.TryGetValue("created_at", out test) ? JSONTools.ParseDateTimeFromString(test.ToString()) : new System.DateTime(),
                        //lastAccessed = space.TryGetValue("modified_at", out test) ? JSONTools.ParseDateTimeFromString(test.ToString()) : new System.DateTime()
                    };

                    var assetIDs = space.TryGetValue("asset_ids", out test) ? test as TinyJSON.ProxyArray : new TinyJSON.ProxyArray();
                    spaceData.asset_ids = assetIDs.Make<List<string>>().Distinct().ToArray();

                    doneCallback(false, spaceData);
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