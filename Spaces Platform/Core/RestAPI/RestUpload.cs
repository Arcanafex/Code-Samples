using UnityEngine;
using System.Collections;

// upload file to asset
//
// PUT /upload/space/v1/asset? upload_type = resumable & upload_id =[upload_id]
//
// ex: curl -X PUT  http://api.spaces.com:8082/upload/space/v1/asset?upload_type=resumable&upload_id=7dPdI9_a -d @myAssetBundle.unity3d

namespace Spaces.Core.RestAPI
{
    public delegate void RestUploadCallback(bool error, string reply);

    public class RestUpload : RestEntry
    {
        RestUploadCallback doneCallback;

        public void Run(string uploadUrl, byte[] data, RestUploadCallback inCallback)
        {
            doneCallback = inCallback;
            RestManager.Put(this, uploadUrl, data, UploadToSpacesCloud);
        }

        public void Run(string uploadUrl, string filePath, RestUploadCallback inCallback)
        {
            doneCallback = inCallback;
            RestManager.Put(this, uploadUrl, System.IO.File.ReadAllBytes(filePath), UploadToSpacesCloud);
        }

        public void UploadToSpacesCloud(bool error, string reply)
        {
            if (error)
            {
                doneCallback(true, null);
            }
            else
            {
                doneCallback(false, reply);
            }
        }
    }
}