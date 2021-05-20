using System;
using UnityEngine;
using Spaces.Core.RestAPI;

namespace Spaces.Core
{
    [Serializable]
    public class User
    {
        public string name;
        public string email;
        public string firstName;
        public string lastName;
        public string middleName;
        public DateTime lastLogin;
        //public DateTime created;

        public delegate void infoUpdated(User user);
        public event infoUpdated onInfoUpdated;

        public delegate void tokenUpdated(User user);
        public event tokenUpdated onTokenUpdated;

        public string token { get; private set; }
        public bool authenticated
        {
            get { return !string.IsNullOrEmpty(token); }
        }

        public User() {}

        public User(string userName)
        {
            this.name = userName;
        }

        public void FetchInfo()
        {
            RestManager.Request.GetUser(this.name, OnGetUserResponse);
        }

        private void OnGetUserResponse(bool error, RestGetUserResponse response)
        {
            if (error)
            {
                Debug.Log("[Error] Retrieving user info failed.");
            }
            else
            {
                this.firstName = response.first_name;
                this.middleName = response.middle_initial;
                this.lastName = response.last_name;
                this.email = response.email;

                Debug.Log("[USER]: " + name + " (" + this.firstName + " " + this.middleName + " " + this.lastName + ")");

                if (onInfoUpdated != null)
                    onInfoUpdated(this);
            }
        }

        public static User CreateUser(string userName, string eMail, string first, string last, string middle, string password)
        {
            var user = new User()
            {
                name = userName,
                email = eMail,
                firstName = first,
                middleName = middle,
                lastName = last
            };

            RestManager.Request.CreateUser(user.name, user.email, user.firstName, user.lastName, user.middleName, password, OnCreateUserResponse);
            return user;
        }

        private static void OnCreateUserResponse(bool error, CreateUserResponseData response)
        {
            if (error)
            {
                Debug.Log("[Create User] Request failed.");
            }
            else
            {
                Debug.Log("[Create User] Request Succeeded!");
            }
        }

        public void SetToken(string token)
        {
            this.token = token;
            Debug.Log(this.name + " [Token Updated] (" + token + ")");

            if (onTokenUpdated != null)
                onTokenUpdated(this);
        }

        public void Delete()
        {
            RestManager.Request.DeleteUser(name, OnDeleteUserResponse);
        }

        private void OnDeleteUserResponse(bool error, RestAPI.DeleteUserResponseData response)
        {
            if (error) { }
            else { }
        }
    }

    [Serializable]
    public class Group
    {
        public string id;
        public string name;
    }
}
