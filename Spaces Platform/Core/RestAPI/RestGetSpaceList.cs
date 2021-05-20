using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//GET /spaces/all

//ex: curl -X GET http://api.spaces.com/spaces/all

//Will return: {
//"AllSpaces":{
//  "all":[
//      {
//          "id":"764E0669-390F-4058-A7F1-4612C94AB802",
//          "name":"Test Space"
//      },
//      {
//          "id":"48C9E41A-525F-444C-9304-BC3BA37D94FD",
//          "name":"Test Space"
//      },
//      {"id":"FADE2140-C041-4415-967A-50640285AC12","name":"la la la Space"},{"id":"A4B336FD-BF11-42DE-A4BD-955E20B3DF0C","name":"Test Space 2"},{"id":"7575D142-B126-415E-9D92-ADC094C8484E","name":"Blah Blah Space"}]}}

namespace Spaces.Core.RestAPI
{
    public delegate void RestGetSpaceListCallback(bool error, RestGetSpaceListResponseData responseData);

    public class RestGetSpaceListRequestData
    {

    }

    public class RestGetSpaceListResponseData
    {
        public struct SpaceInfo
        {
            public string id;
            public string name;
        }

        //public List<SpaceInfo> spaceInfoList;
        public List<Space> spaces;

        public RestGetSpaceListResponseData()
        {
            //spaceInfoList = new List<SpaceInfo>();
            spaces = new List<Space>();
        }
    }

    public class RestGetSpaceList : RestEntry
    {
        RestGetSpaceListCallback doneCallback;
        List<Spaces.Core.Space> spaces;

        //public void Run(RestGetSpaceListCallback inCallback)
        //{
        //    spaces = new List<Space>();
        //    RestManager.Get(this, RestManager.Request.GET_SPACE_LIST, GetSpacesListReply);
        //    doneCallback = inCallback;
        //}

        public void Run(ref List<Spaces.Core.Space> spaceList, RestGetSpaceListCallback inCallback)
        {
            spaces = spaceList;
            RestManager.Get(this, RestManager.Request.GET_SPACE_LIST, GetSpacesListReply);
            doneCallback = inCallback;
        }

        public void GetSpacesListReply(bool error, string reply)
        {
            if (error)
            {
                doneCallback(true, null);
            }
            else
            {
                try
                {
                    TinyJSON.Variant test;
                    var getSpaceResponse = TinyJSON.Decoder.Decode(reply) as TinyJSON.ProxyObject;
                    var getSpaceResponseArray = getSpaceResponse.TryGetValue("AllSpaces", out test) ? test as TinyJSON.ProxyObject : new TinyJSON.ProxyObject();

                    if (getSpaceResponseArray.TryGetValue("all", out test))
                    {
                        var array = test as TinyJSON.ProxyArray;
                        List<RestGetSpaceListResponseData.SpaceInfo> spaceInfoList = new List<RestGetSpaceListResponseData.SpaceInfo>();

                        for (int i = 0; i < array.Count; i++)
                        {
                            var spaceID = ((TinyJSON.ProxyObject)array[i]).TryGetValue("id", out test) ? test : string.Empty;
                            var spaceName = ((TinyJSON.ProxyObject)array[i]).TryGetValue("name", out test) ? test : string.Empty;

                            spaceInfoList.Add(new RestGetSpaceListResponseData.SpaceInfo() { id = spaceID, name = spaceName });
                        }

                        spaceInfoList.Sort((s1, s2) => s1.name.CompareTo(s2.name) == 0 ? s1.id.CompareTo(s2.id) : s1.name.CompareTo(s2.name));

                        spaceInfoList.ForEach(info => spaces.Add(new Space(info.id, info.name)));
                    }

                    var spacesList = new RestGetSpaceListResponseData()
                    {
                        spaces = spaces
                    };

                    doneCallback(false, spacesList);
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    doneCallback(true, null);
                }
            }
        }
    }
}