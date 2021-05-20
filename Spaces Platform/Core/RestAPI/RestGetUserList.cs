using UnityEngine;
using System.Collections;

namespace Spaces.Core.RestAPI
{
    public delegate void RestGetUserListCallback(bool error, Spaces.Core.User[] userList);

    public class RestGetUserList : RestEntry
    {

    }
}