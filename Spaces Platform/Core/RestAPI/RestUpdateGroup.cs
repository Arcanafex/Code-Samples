using UnityEngine;
using System.Collections;

//PATCH /group/:id

//ex: curl -X PATCH “http://api.spaces.com/group/4E8F9B0D-7337-46D6-8304-E997E9410BD8” -H “Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ” 
//-d ‘{“name”: “Youku Content Creator”}’

//will return:

//{}

namespace Spaces.Core.RestAPI
{
    public delegate void RestUpdateGroupCallback(bool error, string reply);

    public class RestUpdateGroupRequestData
    {
        public string name;
    }

    public class RestUpdateGroup : RestEntry
    {
        RestUpdateGroupCallback doneCallback;

        public void Run(string groupID, string updatedName, RestUpdateGroupCallback inCallback)
        {
            var reqData = new RestUpdateGroupRequestData() { name = updatedName };

            RestManager.Patch(this, RestManager.Request.UPDATE_GROUP + groupID, TinyJSON.Encoder.Encode(reqData, TinyJSON.EncodeOptions.NoTypeHints), OnUpdateGroupResponse);
            doneCallback = inCallback;
        }

        public void OnUpdateGroupResponse(bool error, string reply)
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