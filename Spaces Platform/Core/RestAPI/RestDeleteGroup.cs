using UnityEngine;
using System.Collections;

//DELETE /group/:id

//ex: curl -X DELETE “http://api.spaces.com/group/4E8F9B0D-7337-46D6-8304-E997E9410BD8” -H “Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ”

//will return:

//{}
namespace Spaces.Core.RestAPI
{
    public delegate void RestDeleteGroupCallback(bool error, DeleteGroupResponseData response);

    public class DeleteGroupRequestData
    {
        public string groupID;
    }

    public class DeleteGroupResponseData
    {
    }

    public class RestDeleteGroup : RestEntry
    {
        RestDeleteGroupCallback doneCallback;
        string arg;

        public void Run(string groupID, RestDeleteGroupCallback inCallback)
        {
            arg = groupID;
            var data = new DeleteGroupRequestData() { groupID = groupID };

            RestManager.Delete(this, RestManager.Request.DELETE_GROUP + groupID, DeleteGroupResponse);
            doneCallback = inCallback;
        }

        public void DeleteGroupResponse(bool error, string response)
        {
            if (error)
            {
                Debug.Log("Delete Group " + arg + " Request failed.");
                doneCallback(true, null);
            }
            else
            {
                Debug.Log("Delete Group Request failed.");
                doneCallback(false, new DeleteGroupResponseData());
            }
        }


    }


}