using UnityEngine;
using System.Collections;


//curl -X DELETE “http://api.spaces.com/space/42BA9E71-EA47-4DB7-8BBD-824CEE92FF0” 
//-H "Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuc3BhY2VzLmNvbSIsImlhdCI6MTQ3MDg2OTUxNSwiZXhwIjoxNTAyNDA1NTE1LCJhdWQiOiJhcGkuc3BhY2VzLmNvbSIsInN1YiI6InlvdWt1IiwiaWQiOiJENkQ3NTY2RS1ENENGLTRGNjEtQTRGMy0wQTkxOUVCMjZCQjYifQ.TObv9AwcWzVdYW-8fdhfxrtreQVrR0qkoF3HH3_JNnY"

//will return:
//{}
namespace Spaces.Core.RestAPI
{
    public delegate void RestDeleteSpaceCallback(bool error, RestDeleteSpaceResponse reply);

    public class RestDeleteSpaceResponse
    {
        public Spaces.Core.Space space;
    }

    public class RestDeleteSpace : RestEntry
    {
        RestDeleteSpaceCallback doneCallback;
        string spaceID;

        public void Run(string spaceID, RestDeleteSpaceCallback inCallback)
        {
            this.spaceID = spaceID;
            RestManager.Delete(this, RestManager.Request.DELETE_SPACE + spaceID, OnDeleteSpaceResponse);
            doneCallback = inCallback;
        }

        public void OnDeleteSpaceResponse(bool error, string response)
        {
            var deleteResponse = new RestDeleteSpaceResponse()
            {
                space = Spaces.Core.Space.SpacesManager.GetSpace(this.spaceID)
            };

            if (deleteResponse.space == null)
                deleteResponse.space = new Space(this.spaceID);

            if (error)
            {
                doneCallback(true, deleteResponse);
            }
            else
            {
                doneCallback(false, deleteResponse);
            }
        }

    }
}