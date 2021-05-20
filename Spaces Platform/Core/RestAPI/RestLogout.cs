using UnityEngine;
using System.Collections;

//GET /logout

//ex: curl -X GET “http://api.spaces.com/logout/” -H “Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ”

//will return:

//{}
namespace Spaces.Core.RestAPI
{
    public delegate void RestLogoutCallback(bool error, RestLogoutResponseData data);

    public class RestLogoutRequestData
    { }

    public class RestLogoutResponseData
    { }

    public class RestLogout : RestEntry
    {
        private RestLogoutCallback doneCallback;

        public void Run(RestLogoutCallback inCallback)
        {
            RestManager.Post(this, RestManager.Request.USER_LOGOUT, "", LogoutResponse);

            doneCallback = inCallback;
        }

        public void LogoutResponse(bool error, string response)
        {
            if (error)
            {
                doneCallback(true, null);
            }
            else
            {
                doneCallback(false, null);
            }
        }

    }
}