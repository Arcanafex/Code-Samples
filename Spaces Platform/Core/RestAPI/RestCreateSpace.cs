using System.Collections;
using Spaces.Core;
using System.Collections.Generic;
using UnityEngine;

// POST /space

// curl -X POST "http://api.spaces.com/space/"  -H “Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuc3BhY2VzLmNvbSIsImlhdCI6MTQ3MDg2OTUxNSwiZXhwIjoxNTAyNDA1NTE1LCJhdWQiOiJhcGkuc3BhY2VzLmNvbSIsInN1YiI6InlvdWt1IiwiaWQiOiJENkQ3NTY2RS1ENENGLTRGNjEtQTRGMy0wQTkxOUVCMjZCQjYifQ.TObv9AwcWzVdYW-8fdhfxrtreQVrR0qkoF3HH3_JNnY”
// -d '{"name": "Billions & Billions"}'

// will return: {"space_id":"A7E26538-3A64-480C-A96F-EB6AF37A53F4"}
namespace Spaces.Core.RestAPI
{
    public delegate void RestCreateSpaceCallback(bool error, CreateSpaceResponseData responseData);

    public class CreateSpaceData
    {
        public string name;
    }

    public class CreateSpaceResponseData
    {
        public string id;
        public Spaces.Core.Space space;
    }

    public class RestCreateSpace : RestEntry
    {
        RestCreateSpaceCallback doneCallback;
        Spaces.Core.Space space;

        public void Run(ref Spaces.Core.Space createdSpace, string spaceName, RestCreateSpaceCallback inCallback)
        {
            space = createdSpace;
            space.name = spaceName;

            var data = new CreateSpaceData()
            {
                name = space.name
            };

            RestManager.Post(this, RestManager.Request.CREATE_SPACE, TinyJSON.Encoder.Encode(data, TinyJSON.EncodeOptions.NoTypeHints), CreateSpaceReply);
            doneCallback = inCallback;
        }

        public void CreateSpaceReply(bool error, string reply)
        {
            if (error)
            {
                doneCallback(true, new CreateSpaceResponseData() { space = space });
            }
            else
            {
                try
                {
                    TinyJSON.Variant test;
                    var createSpaceRespone = TinyJSON.Decoder.Decode(reply) as TinyJSON.ProxyObject;

                    CreateSpaceResponseData userSpace = new CreateSpaceResponseData()
                    {
                        id = createSpaceRespone.TryGetValue("space_id", out test) ? test : string.Empty
                    };

                    space.id = userSpace.id;
                    userSpace.space = space;

                    doneCallback(false, userSpace);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                    doneCallback(true, new CreateSpaceResponseData() { space = space });
                }
            }
        }
    }

}