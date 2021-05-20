using UnityEngine;
using System.Collections;
//    PATCH /user

//    ex: curl -X PATCH “http://api.spaces.com/user/” 
//-H “Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ” 
//-d ‘{
//“first_name”: “Roy”, 
//“middle_initial”: “G”, 
//“last_name”: “Biv”, 
//“password”: “rainbowvomit60”}’

//will return: 

//{}

namespace Spaces.Core.RestAPI
{

    public delegate void RestUpdateUserCallback(bool error, UpdateUserResponseData data);

    public class UpdateUserRequestData
    {
        public string firstName;
        public string lastName;
        public string middleInitial;
        public string password;
    }

    public class UpdateUserResponseData
    { }

    public class RestUpdateUser : RestEntry
    {
        RestUpdateUserCallback doneCallback;

        public void Run(string first, string middle, string last, string password, RestUpdateUserCallback inCallback)
        {
            RestManager.Patch(this, RestManager.Request.UPDATE_USER, "", UpdateUserResponse);
            doneCallback = inCallback;
        }

        public void UpdateUserResponse(bool error, string response)
        {
            if (error)
            {
                Debug.Log("Request failed.");
                doneCallback(true, null);
            }
            else
            {
                Debug.Log(".");
                doneCallback(false, new UpdateUserResponseData());
            }
        }
    }
}