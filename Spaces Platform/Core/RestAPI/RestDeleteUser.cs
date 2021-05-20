using UnityEngine;
using System.Collections;

//DELETE /user/:username

//ex: curl -X DELETE “http://api.spaces.com:8083/user/jqpublic”

//will return:

//{}
namespace Spaces.Core.RestAPI
{
    public delegate void RestDeleteUserCallback(bool error, DeleteUserResponseData response);

    public class DeleteUserRequestData
    {
        public string userName;
    }

    public class DeleteUserResponseData
    {
        public string userName;
    }

    public class RestDeleteUser : RestEntry
    {
        RestDeleteUserCallback doneCallback;
        string arg;

        public void Run(string userName, RestDeleteUserCallback inCallback)
        {
            arg = userName;

            RestManager.Delete(this, RestManager.Request.DELETE_USER + userName, DeleteUserResponse);
            doneCallback = inCallback;
        }

        public void DeleteUserResponse(bool error, string response)
        {
            if (error)
            {
                Debug.Log("Delete Group " + arg + " Request failed.");
                doneCallback(true, null);
            }
            else
            {
                Debug.Log("Group " + arg + " Deleted.");
                doneCallback(false, new DeleteUserResponseData() { userName = arg });
            }
        }
    }
}