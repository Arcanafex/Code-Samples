using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using System.Linq;

namespace Spaces.Core.RestAPI
{
    public delegate void RestManagerCallback(bool error, string reply);

    public class RestManager
    {
        #region Platform Services Settings
        public static SpacesPlatformServicesSettings PlatformSettings
        {
            get
            {
                return SpacesPlatformServicesSettings.Settings;
            }
            internal set
            {
                value.SetDefault();
            }
        }

        public static string AuthToken
        {
            get
            {
                return SpacesPlatformServicesSettings.Settings.Token;
            }
        }

        private static string ServicesDomain
        {
            get
            {
                return SpacesPlatformServicesSettings.Settings.ServiceDomain;
            }
        }

        #endregion

        #region Rest Commands

        public static class Request
        {
            //POST /space/
            public const string CREATE_SPACE = "/space/";
            public static Space CreateSpace(string spaceName, RestCreateSpaceCallback callback)
            {
                var createdSpace = new Space();

                var restCreateSpace = new RestCreateSpace();
                restCreateSpace.Run(ref createdSpace, spaceName, callback);

                return createdSpace;
            }

            //POST /asset/
            public const string CREATE_ASSET = "/asset/";
            public static Asset CreateAsset(string assetName, string assetType, RestCreateAssetCallback callback)
            {
                return CreateAsset(assetName, assetType, "", callback);
            }

            public static Asset CreateAsset(string assetName, string assetType, string sourcePath, RestCreateAssetCallback callback)
            {
                var createdAsset = new Asset();
                createdAsset.SetDataSourcePath(sourcePath);

                string fileName = System.IO.Path.GetFileName(sourcePath);

                if (string.IsNullOrEmpty(System.IO.Path.GetExtension(sourcePath)) && assetType == Core.Constants.AssetType.assetbundle.ToString())
                    fileName += ".unity3d";

                var createAsset = new RestCreateAsset();
                createAsset.Run(ref createdAsset, assetName, assetType, fileName, callback);

                return createdAsset;
            }

            //POST /user/
            public const string CREATE_USER = ":8083/user/";
            public static void CreateUser(string userName, string eMail, string first, string last, string middle, string password, RestCreateUserCallback callback)
            {
                var restCreateUser = new RestCreateUser();
                restCreateUser.Run(userName, eMail, first, last, middle, password, callback);
            }


            //POST /group/
            public const string CREATE_GROUP = ":8083/group/";
            public static void CreateGroup(string groupName, RestCreateGroupCallback callback)
            {
                var restCreateGroup = new RestCreateGroup();
                restCreateGroup.Run(groupName, callback);
            }

            //GET  /space/:id
            public const string GET_SPACE = "/space/";
            public static void GetSpace(string spaceID, RestGetSpaceCallback callback)
            {
                var restGetSpace = new RestGetSpace();
                restGetSpace.Run(spaceID, callback);
            }

            //GET /asset/:id
            public const string GET_ASSET = "/asset/";
            public static void GetAsset(string assetID, RestGetAssetCallback callback)
            {
                var assetRequest = new RestGetAsset();
                assetRequest.Run(assetID, callback);
            }

            //GET /upload/url/:assettype/:assetid
            public const string GET_ASSET_UPLOAD_ENDPOINT = ":8082/upload/url/";
            public static void GetAssetUploadEndpoint(string assetType, string assetID, RestGetAssetUploadEndpointCallback callback)
            {
                var restGetUploadEndpoint = new RestGetAssetUploadEndpoint();
                restGetUploadEndpoint.Run(assetType, assetID, callback);
            }

            //GET /space/:id/metadata
            public const string GET_SPACE_METADATA = "/space/{0}/metadata";
            public static void GetSpaceMetadata(string spaceID, RestGetSpaceMetadataCallback callback)
            {
                var restGetMetadata = new Spaces.Core.RestAPI.RestGetSpaceMetadata();
                restGetMetadata.Run(spaceID, callback);
            }

            //GET /asset/:id/metadata
            public const string GET_ASSET_METADATA = "/asset/{0}/metadata";
            public static void GetAssetMetadata(string assetID, RestGetAssetMetadataCallback callback)
            {
                var restGetAssetMetadata = new Spaces.Core.RestAPI.RestGetAssetMetadata();
                restGetAssetMetadata.Run(assetID, callback);
            }

            //GET /asset/:id/data/(:platform)
            public const string GET_ASSET_DATA = "/asset/{0}/data/{1}";
            public static void GetAssetData(string assetID, string platform, RestGetAssetDataCallback callback)
            {
                var request = new Spaces.Core.RestAPI.RestGetAssetData();
                request.Run(assetID, platform, callback);
            }

            //GET /asset/:id/thumb/:res
            public const string GET_ASSET_PREVIEW = "/asset/{0}/thumb/{1}";
            public static void GetAssetPreview(string assetID, string resolution, RestGetAssetPreviewCallback callback)
            {
                var request = new Spaces.Core.RestAPI.RestGetAssetPreview();
                request.Run(assetID, resolution, callback);
            }

            public static void GetAssetPreview(string assetID, RestGetAssetPreviewCallback callback)
            {
                var request = new Spaces.Core.RestAPI.RestGetAssetPreview();
                request.Run(assetID, callback);
            }

            //GET /user/:username
            public const string GET_USER = ":8083/user/";
            public static void GetUser(string userName, RestGetUserCallback callback)
            {
                var restGetUser = new RestGetUser();
                restGetUser.Run(userName, callback);
            }

            //POST /space/:id/metadata/            
            public const string UPDATE_SPACE_METADATA = "/space/{0}/metadata/";
            public static void UpdateSpaceMetadata(string spaceID, string json, RestUpdateSpaceMetadataCallback callback)
            {
                var restUpdateMetadata = new Spaces.Core.RestAPI.RestUpdateSpaceMetadata();
                restUpdateMetadata.Run(spaceID, json, callback);
            }

            //POST /asset/:id/metadata/
            public const string UPDATE_ASSET_METADATA = "/asset/{0}/metadata/";
            public static void UpdateAssetMetadata(string assetID, Spaces.Core.Asset.Metadata json, RestUpdateAssetMetadataCallback callback)
            {
                var restUpdateAssetMetadata = new Spaces.Core.RestAPI.RestUpdateAssetMetadata();
                restUpdateAssetMetadata.Run(assetID, json, callback);
            }

            //GET /spaces/all
            public const string GET_SPACE_LIST = "/spaces/all";
            public static List<Space> GetSpaceList(RestGetSpaceListCallback callback)
            {
                var spaceList = new List<Space>();
                var getSpaceList = new RestGetSpaceList();
                getSpaceList.Run(ref spaceList, callback);
                return spaceList;
            }

            //GET /assets/all
            public const string GET_ASSET_LIST = "/assets/all";
            public static List<Asset> GetAssetList(RestGetAssetListCallback callback)
            {
                var assetList = new List<Asset>();
                var getAssetList = new RestGetAssetList();
                getAssetList.Run(ref assetList, callback);
                return assetList;
            }

            //PATCH /space/:id
            public const string UPDATE_SPACE = "/space/";
            public static void UpdateSpace(string spaceID, string spaceName, string spaceOwnerID, RestUpdateSpaceCallback callback)
            {
                var restUpdateSpace = new RestUpdateSpace();
                restUpdateSpace.Run(spaceID, spaceName, spaceOwnerID, callback);
            }

            //PATCH /asset/:id
            public const string UPDATE_ASSET = "/asset/";
            public static void UpdateAsset(string assetID, string assetName, string assetType, string assetOwnerID, string originalFilename, RestUpdateAssetCallback callback)
            {
                var restUpdateAsset = new RestUpdateAsset();
                restUpdateAsset.Run(assetID, assetName, assetType, assetOwnerID, originalFilename, callback);
            }

            //PATCH /user
            public const string UPDATE_USER = ":8083/user/";
            public static void UpdateUser(string first, string middle, string last, string password, RestUpdateUserCallback callback)
            {
                var updateUser = new RestUpdateUser();
                updateUser.Run(first, middle, last, password, callback);
            }

            //PATCH /group/:id
            public const string UPDATE_GROUP = ":8083/group/";
            public static void UpdateGroup(string groupID, string updatedName, RestUpdateGroupCallback callback)
            {
                var updateGroup = new RestUpdateGroup();
                updateGroup.Run(groupID, updatedName, callback);
            }

            //DELETE /space/:id
            public const string DELETE_SPACE = "/space/";
            public static void DeleteSpace(string spaceID, RestDeleteSpaceCallback callback)
            {
                var restDeleteSpace = new RestDeleteSpace();
                restDeleteSpace.Run(spaceID, callback);
            }

            //DELETE /asset/:id
            public const string DELETE_ASSET = "/asset/";
            public static void DeleteAsset(string assetID, RestDeleteAssetCallback callback)
            {
                var deleteAsset = new RestDeleteAsset();
                deleteAsset.Run(assetID, callback);
            }
            public static void DeleteAsset(Asset asset, RestDeleteAssetCallback callback)
            {
                var deleteAsset = new RestDeleteAsset();
                deleteAsset.Run(asset, callback);
            }


            //DELETE /user/:id
            public const string DELETE_USER = ":8083/user/";
            public static void DeleteUser(string userName, RestDeleteUserCallback callback)
            {
                var deleteUser = new RestDeleteUser();
                deleteUser.Run(userName, callback);
            }

            //DELETE /group/:id
            public const string DELETE_GROUP = ":8083/group/";
            public static void DeleteGroup(string groupID, RestDeleteGroupCallback callback)
            {
                var deleteGroup = new RestDeleteGroup();
                deleteGroup.Run(groupID, callback);
            }

            //POST /login
            public const string USER_LOGIN = ":8083/login/";
            public static void Login(string userName, string password, RestLoginCallback callback)
            {
                var login = new RestLogin();
                login.Run(userName, password, callback);
            }

            //GET /logout
            public const string USER_LOGOUT = ":8083/logout/";
            public static void Logout(RestLogoutCallback callback)
            {
                var logout = new RestLogout();
                logout.Run(callback);
            }

            //public const string GET_USER_LIST = "/getallusers";

            //PUT /asset/
            public const string ADD_ASSET_TO_SPACE = "/asset/";
            public static void AddAssetToSpace(string spaceID, string assetID, RestAddAssetToSpaceCallback callback)
            {
                var RestAddAssetToSpace = new RestAddAssetToSpace();
                RestAddAssetToSpace.Run(spaceID, assetID, callback);
            }

            //DELETE /association/
            public const string REMOVE_ASSET_FROM_SPACE = "/association/";
            public static void RemoveAssetFromSpace(string spaceID, string assetID, RestRemoveAssetFromSpaceCallback callback)
            {
                var restRemoveAssetFromSpace = new RestRemoveAssetFromSpace();
                restRemoveAssetFromSpace.Run(spaceID, assetID, callback);
            }

            //PUT /upload/space/v1/asset? upload_type = resumable & upload_id =[upload_id]
            public static void UploadFile(string uploadUrl, string filePath, RestUploadCallback callback)
            {
                var restUploadFile = new RestUpload();
                restUploadFile.Run(uploadUrl, filePath, callback);
            }

        }
        #endregion


        private static RestManager _instance;
        public static RestManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RestManager();
                }

                return _instance;
            }
        }

        private List<RestEntry> entries = new List<RestEntry>();

        private void StartRequest(RestEntry entry)
        {
            entries.Add(entry);

            entry.request = new HTTPRequest(new System.Uri(entry.fullUrl), entry.entryType, OnRequestFinished);
            entry.request.Tag = entry;
            entry.request.AddHeader("Authorization", "Bearer " + AuthToken);

            if (entry.data != null)
                entry.request.RawData = entry.data;

            entry.request.Send();
        }

        private void OnRequestFinished(HTTPRequest request, HTTPResponse response)
        {
            RestEntry restEntry = request.Tag as RestEntry;

            // check for errors
            if (response != null && response.IsSuccess == true)
            {
                // Success
                restEntry.managerCallback(false, response.DataAsText);
            }
            else
            {
                restEntry.managerCallback(true, null);
            }

            Instance.entries.Remove(restEntry);
        }

        //public static RestEntry GetNewEntry()
        //{
        //    RestEntry entry = new RestEntry();
        //    Instance.entries.Add(entry);


        //    return entry;
        //}

        public static RestEntry Get(RestEntry entry, string url, RestManagerCallback managerCallback)
        {
            entry.entryType = HTTPMethods.Get;
            entry.managerCallback = managerCallback;
            entry.fullUrl = url.StartsWith(ServicesDomain) ? url : string.Concat(ServicesDomain, url);
            Instance.StartRequest(entry);

            return entry;
        }

        public static RestEntry Put(RestEntry entry, string url, byte[] body, RestManagerCallback managerCallback)
        {
            entry.entryType = HTTPMethods.Put;
            entry.managerCallback = managerCallback;
            entry.data = body;
            entry.fullUrl = url.StartsWith(ServicesDomain) ? url : string.Concat(ServicesDomain, url);
            Instance.StartRequest(entry);

            return entry;
        }

        public static RestEntry Post(RestEntry entry, string url, string jsonStringData, RestManagerCallback managerCallback)
        {
            entry.entryType = HTTPMethods.Post;
            entry.managerCallback = managerCallback;
            entry.data = System.Text.Encoding.UTF8.GetBytes(jsonStringData);
            entry.fullUrl = url.StartsWith(ServicesDomain) ? url : string.Concat(ServicesDomain, url);
            Instance.StartRequest(entry);

            return entry;
        }

        public static RestEntry Patch(RestEntry entry, string url, string jsonStringData, RestManagerCallback managerCallback)
        {
            entry.entryType = HTTPMethods.Patch;
            entry.managerCallback = managerCallback;
            entry.data = System.Text.Encoding.UTF8.GetBytes(jsonStringData);
            entry.fullUrl = url.StartsWith(ServicesDomain) ? url : string.Concat(ServicesDomain, url);
            Instance.StartRequest(entry);

            return entry;
        }

        public static RestEntry Delete(RestEntry entry, string url, RestManagerCallback managerCallback)
        {
            entry.entryType = HTTPMethods.Delete;
            entry.managerCallback = managerCallback;
            entry.fullUrl = url.StartsWith(ServicesDomain) ? url : string.Concat(ServicesDomain, url);
            Instance.StartRequest(entry);

            return entry;
        }

        public static RestEntry Remove(RestEntry entry, string url, string jsonStringData, RestManagerCallback managerCallback)
        {
            entry.entryType = HTTPMethods.Delete;
            entry.managerCallback = managerCallback;
            entry.data = System.Text.Encoding.UTF8.GetBytes(jsonStringData);
            entry.fullUrl = url.StartsWith(ServicesDomain) ? url : string.Concat(ServicesDomain, url);
            Instance.StartRequest(entry);

            return entry;
        }
    }
}
