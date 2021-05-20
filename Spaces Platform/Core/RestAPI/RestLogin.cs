using System.Collections;
using Spaces.Core;

//POST /login

//ex: curl -X POST “http://api.spaces.com/login/”  -d ‘{“username”: “jqpublic”, “password”: “rainbowvomit60”}’

//will return:

//{“token”: “eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ”}

namespace Spaces.Core.RestAPI
{

    public delegate void RestLoginCallback(bool error, RestLoginResponseData userToken);

    public class RestLoginRequestData
    {
        public string username;
        public string password;
    }

    public class RestLoginResponseData
    {
        public string token;
    }

    public class RestLogin : RestEntry
    {
        RestLoginCallback doneCallback;

        public void Run(string username, string password, RestLoginCallback inCallback)
        {
            var userData = new RestLoginRequestData()
            {
                username = username,
                password = password
            };

            RestManager.Post(this, RestManager.Request.USER_LOGIN, TinyJSON.Encoder.Encode(userData), LoginReply);
            doneCallback = inCallback;
        }

        public void LoginReply(bool error, string reply)
        {
            if (error)
            {
                doneCallback(true, null);
            }
            else
            {
                TinyJSON.Variant test;
                var loginRespone = TinyJSON.Decoder.Decode(reply) as TinyJSON.ProxyObject;

                RestLoginResponseData replyData = new RestLoginResponseData()
                {
                    token = loginRespone.TryGetValue("token", out test) ? test : string.Empty
                };

                doneCallback(false, replyData);
            }
        }
    }

}