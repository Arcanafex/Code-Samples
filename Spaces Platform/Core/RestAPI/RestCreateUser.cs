using System.Collections;
using Spaces.Core;
using System.Collections.Generic;
using UnityEngine;

//POST /user

//ex: curl -X POST "http://api.spaces.com/user/" -d '{
//“username”: “jqpublic”, 
//“first_name”: “John”, 
//“middle_initial”: “Q”, 
//“last_name”: “Public”, 
//“password”: “boogers123”
//"email": "jqp@spaces.com"
//}'

//will return:

//{}
namespace Spaces.Core.RestAPI
{
    public delegate void RestCreateUserCallback(bool error, CreateUserResponseData responseData);

    public class CreateUserData
    {
        public string username;
        public string email;
        public string first_name;
        public string last_name;
        public string middle_initial;
        public string password;
    }

    public class CreateUserResponseData
    {
    }

    public class RestCreateUser : RestEntry
    {
        RestCreateUserCallback doneCallback;

        public void Run(string userName, string eMail, string first, string last, string middle, string password, RestCreateUserCallback inCallback)
        {
            var data = new CreateUserData()
            {
                username = userName,
                first_name = first,
                middle_initial = middle,
                last_name = last,
                password = password,
                email = eMail
            };

            RestManager.Post(this, RestManager.Request.CREATE_USER, TinyJSON.Encoder.Encode(data, TinyJSON.EncodeOptions.NoTypeHints), CreateUserReply);
            doneCallback = inCallback;
        }

        public void CreateUserReply(bool error, string reply)
        {
            if (error)
            {
                doneCallback(true, null);
            }
            else
            {
                Debug.Log("Create User Request succeeded");
            }
        }
    }

}