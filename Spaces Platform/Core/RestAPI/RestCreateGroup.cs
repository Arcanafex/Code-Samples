using System.Collections;
using Spaces.Core;
using System.Collections.Generic;
using UnityEngine;

//POST /group

//ex: curl -X POST "http://api.spaces.com/group/" 
//-H “Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ” 
//-d '{
//“name”: “Youku Default”
//}'

//will return:

//{“name”: “Youku Default”, “group_id”: “4E8F9B0D - 7337 - 46D6 - 8304 - E997E9410BD8”}

namespace Spaces.Core.RestAPI
{
    public delegate void RestCreateGroupCallback(bool error, CreateGroupResponseData responseData);

    public class CreateGroupData
    {
        public string name;
    }

    public class CreateGroupResponseData
    {
        public string groupName;
        public string groupID;
    }

    public class RestCreateGroup : RestEntry
    {
        RestCreateGroupCallback doneCallback;

        public void Run(string groupName, RestCreateGroupCallback inCallback)
        {
            var data = new CreateGroupData()
            {
                name = groupName
            };

            RestManager.Post(this, RestManager.Request.CREATE_GROUP, TinyJSON.Encoder.Encode(data, TinyJSON.EncodeOptions.NoTypeHints), CreateGroupReply);
            doneCallback = inCallback;
        }

        public void CreateGroupReply(bool error, string reply)
        {
            // Deserialize from string to the correct object type.
            if (error)
            {
                doneCallback(true, null);
            }
            else
            {
                try
                {
                    TinyJSON.Variant test;
                    var createGroupRespone = TinyJSON.Decoder.Decode(reply) as TinyJSON.ProxyObject;

                    CreateGroupResponseData userGroup = new CreateGroupResponseData()
                    {
                        groupName = createGroupRespone.TryGetValue("name", out test) ? test : string.Empty,
                        groupID = createGroupRespone.TryGetValue("group_id", out test) ? test : string.Empty,
                    };

                    doneCallback(false, userGroup);
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