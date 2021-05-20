using UnityEngine;
using System.Collections;

// PATCH /space/:id

// curl -X PATCH “http://api.spaces.com/space/42BA9E71-EA47-4DB7-8BBD-824CEE92FF01” 
// -H “Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuc3BhY2VzLmNvbSIsImlhdCI6MTQ3MDg2OTUxNSwiZXhwIjoxNTAyNDA1NTE1LCJhdWQiOiJhcGkuc3BhY2VzLmNvbSIsInN1YiI6InlvdWt1IiwiaWQiOiJENkQ3NTY2RS1ENENGLTRGNjEtQTRGMy0wQTkxOUVCMjZCQjYifQ.TObv9AwcWzVdYW-8fdhfxrtreQVrR0qkoF3HH3_JNnY" 
// -d ‘{“name”:”The Best, Around”, “owner_id”:”7B4B384D-DE3F-4A3F-9D8A-0DD640660BCA”}

namespace Spaces.Core.RestAPI
{
    public delegate void RestUpdateSpaceCallback(bool error, string response);

    public class RestUpdateSpaceRequestData
    {
        public string name;
        public string owner_id;
    }

    public class RestUpdateSpace : RestEntry
    {
        RestUpdateSpaceCallback doneCallback;

        public void Run(string spaceID, string spaceName, string ownerID, RestUpdateSpaceCallback inCallback)
        {
            var request = new RestUpdateSpaceRequestData()
            {
                name = spaceName,
                owner_id = ownerID
            };

            RestManager.Patch(this, RestManager.Request.UPDATE_SPACE + spaceID, TinyJSON.Encoder.Encode(request, TinyJSON.EncodeOptions.NoTypeHints), UpdateSpaceResponse);
            doneCallback = inCallback;
        }

        void UpdateSpaceResponse(bool error, string reply)
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