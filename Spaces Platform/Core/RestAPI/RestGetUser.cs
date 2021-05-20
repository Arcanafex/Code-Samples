using UnityEngine;
using System.Collections;

//    get user info
//GET /user/:username
//curl -X GET “http://api.spaces.com:8083/user/jqpublic/” -H “Authorization: Bearer [token]”
//will return:
//{
//        “first_name: “John”, 
//        “middle_initial”: “Q”, 
//        “last_name”: ”Public”, 
//        “email”: ”jqp @spaces.com”
//}

namespace Spaces.Core.RestAPI
{
    public delegate void RestGetUserCallback(bool error, RestGetUserResponse response);

    public class RestGetUserResponse
    {
        public string first_name;
        public string middle_initial;
        public string last_name;
        public string email;
    }

    public class RestGetUser : RestEntry
    {
        RestGetUserCallback doneCallback;

        public void Run(string userName, RestGetUserCallback inCallback)
        {
            doneCallback = inCallback;

            RestManager.Get(this, RestManager.Request.GET_USER + userName, GetUserResponse);
        }

        public void GetUserResponse(bool error, string response)
        {
            if (error)
            {
                doneCallback(true, null);
            }
            else
            {
                TinyJSON.Variant test;

                var getUserInfo = TinyJSON.Decoder.Decode(response) as TinyJSON.ProxyObject;
                var userInfo = getUserInfo.TryGetValue("user_info", out test) ? test as TinyJSON.ProxyObject : new TinyJSON.ProxyObject();

                var getUserResponse = new RestGetUserResponse()
                {
                    first_name = userInfo.TryGetValue("first_name", out test) ? test : string.Empty,
                    last_name = userInfo.TryGetValue("last_name", out test) ? test : string.Empty,
                    middle_initial = userInfo.TryGetValue("middle_initial", out test) ? test : string.Empty,
                    email = userInfo.TryGetValue("email", out test) ? test : string.Empty,
                };

                doneCallback(false, getUserResponse);
            }
        }
    }
}
