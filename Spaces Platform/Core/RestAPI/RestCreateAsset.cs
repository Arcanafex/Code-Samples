using UnityEngine;
using System.Collections;

// POST /space/asset
//
//ex: curl -X POST "http://api.spaces.com/asset/" -H "Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuc3BhY2VzLmNvbSIsImlhdCI6MTQ3MDg2OTUxNSwiZXhwIjoxNTAyNDA1NTE1LCJhdWQiOiJhcGkuc3BhY2VzLmNvbSIsInN1YiI6InlvdWt1IiwiaWQiOiJENkQ3NTY2RS1ENENGLTRGNjEtQTRGMy0wQTkxOUVCMjZCQjYifQ.TObv9AwcWzVdYW-8fdhfxrtreQVrR0qkoF3HH3_JNnY" 
// -d '{
// "name":"Youku Asset Bundle 1",
// "asset_type":"assetbundle",
// "orig_file_name": "YoukuAssetBundle.unity3d"}'
//
// Will return {"asset_id":"38D9154B-7C2D-4F7D-B1E6-B10BA0EFCF84"}

//namespace Spaces.Server    
namespace Spaces.Core.RestAPI
{
    public delegate void RestCreateAssetCallback(bool error, CreateAssetResponseData responseData);

    public class CreateAssetData
    {
        public string name;
        public string asset_type;
        public string orig_file_name;
    }

    public class CreateAssetResponseData
    {
        public string assetID;
        public string assetType;
        public string assetName;
        public string assetOriginalName;
        public Spaces.Core.Asset asset;
    }

    public class RestCreateAsset : RestEntry
    {
        RestCreateAssetCallback doneCallback;
        Spaces.Core.Asset asset = null;
        //bool createdWithRef;

        public void Run(ref Spaces.Core.Asset createdAsset, string assetName, string assetType, string originalFileName, RestCreateAssetCallback inCallback)
        {
            asset = createdAsset;
            asset.name = assetName;
            asset.assetType = assetType;
            asset.originalFileName = originalFileName;
            //createdWithRef = true;

            var args = new CreateAssetData()
            {
                asset_type = asset.assetType,
                name = asset.name,
                orig_file_name = asset.originalFileName
            };

            RestManager.Post(this, RestManager.Request.CREATE_ASSET, TinyJSON.Encoder.Encode(args, TinyJSON.EncodeOptions.NoTypeHints), CreateAssetReply);
            doneCallback = inCallback;
        }

        public void Run(string assetName, string assetType, string originalFileName, RestCreateAssetCallback inCallback)
        {
            asset = new Spaces.Core.Asset();
            asset.name = assetName;
            asset.assetType = assetType;
            asset.originalFileName = originalFileName;
            //createdWithRef = false;

            var args = new CreateAssetData()
            {
                asset_type = asset.assetType,
                name = asset.name,
                orig_file_name = asset.originalFileName
            };

            RestManager.Post(this, RestManager.Request.CREATE_ASSET, TinyJSON.Encoder.Encode(args, TinyJSON.EncodeOptions.NoTypeHints), CreateAssetReply);
            doneCallback = inCallback;
        }

        public void CreateAssetReply(bool error, string reply)
        {
            if (error)
            {
                doneCallback(true, new CreateAssetResponseData() { asset = asset });
            }
            else
            {
                try
                {
                    TinyJSON.Variant test;
                    var createAssetResponse = TinyJSON.Decoder.Decode(reply) as TinyJSON.ProxyObject;

                    asset.id = createAssetResponse.TryGetValue("asset_id", out test) ? test : string.Empty;

                    CreateAssetResponseData assetCreationResponseData = new CreateAssetResponseData()
                    {
                        assetID = string.IsNullOrEmpty(asset.id) ? "" : asset.id,
                        assetType = string.IsNullOrEmpty(asset.assetType) ? "" : asset.assetType,
                        assetName = string.IsNullOrEmpty(asset.assetType) ? "" : asset.name,
                        assetOriginalName = string.IsNullOrEmpty(asset.assetType) ? "" : asset.originalFileName,
                        asset = asset
                    };

                    //if (createdWithRef)
                    //    asset.Refresh();

                    doneCallback(false, assetCreationResponseData);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                    doneCallback(true, new CreateAssetResponseData() { asset = asset });
                }
            }
        }
    }

}