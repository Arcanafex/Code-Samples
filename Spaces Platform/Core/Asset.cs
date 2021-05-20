using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spaces.UnityClient;
using Spaces.Core.RestAPI;
using Spaces.Extensions;
using UnityEngine.Networking;
using System;

namespace Spaces.Core
{
    [Serializable]
    public class Asset : IProgressive
    {
        //protected internal static class AssetsManager
        public static class AssetsManager
        {
            private static readonly List<Asset> s_assets = new List<Asset>();

            public static Asset AddAsset(Asset asset)
            {
                var existingAsset = GetAsset(asset.id);

                if (existingAsset == null)
                {
                    existingAsset = asset;
                    s_assets.Add(asset);
                }

                return existingAsset;
            }

            public static Asset AddAsset(string assetID)
            {
                if (s_assets.Count == 0)
                    LoadFromCache();

                var existingAsset = GetAsset(assetID);

                if (existingAsset == null)
                {
                    existingAsset = new Asset(assetID, "", "", false);
                    s_assets.Add(existingAsset);
                }

                return existingAsset;
            }

            public static List<Asset> GetAssets()
            {
                if (s_assets.Count == 0)
                    LoadFromCache();

                if (s_assets.Count > 1)
                    s_assets.Sort((a1, a2) => a1.name.CompareTo(a2.name) == 0 ? a1.id.CompareTo(a2.id) : a1.name.CompareTo(a2.name));                    

                return s_assets;
            }

            public static List<Asset> GetAssets(string[] assetIDs)
            {
                var assetList = new List<Asset>();

                foreach (string id in assetIDs)
                {
                    assetList.Add(AddAsset(id));
                }

                return assetList;
            }

            public static List<Asset> GetAssetsWithTag(string tag)
            {
                return s_assets.Where(asset => asset.tags.Contains(tag)).ToList();
            }

            public static List<Asset> GetAssetsWithoutTag(string tag)
            {
                return s_assets.Where(asset => !asset.tags.Contains(tag)).ToList();
            }

            public static Asset GetAsset(string assetID)
            {
                return s_assets.FirstOrDefault(asset => asset.id == assetID);
            }

            public static bool RemoveAsset(Asset asset)
            {
                bool removed = s_assets.Remove(asset);

                foreach (var item in s_assets.Where(space => space.id == asset.id))
                {
                    if (s_assets.Remove(item))
                        removed = true;
                }

                return removed;
            }

            public static bool RemoveAsset(string assetID)
            {
                bool removed = false;

                var assetRemoveList = new List<Asset>(s_assets.Where(space => space.id == assetID));
                foreach (var asset in assetRemoveList)
                {
                    if (s_assets.Remove(asset))
                        removed = true;
                }

                return removed;
            }

            public static void Clear(bool evenLoaded = false)
            {
                if (evenLoaded)
                    s_assets.Clear();
                else
                {
                    var idList = new List<string>(GetAssets().Where(a => !a.isLoaded).Select(a => a.id));
                    idList.ForEach(i => RemoveAsset(i));
                }
            }

            public static void SaveToCache()
            {
                string assetPath = string.Format(@"{0}/AssetList", Constants.ASSETS_CACHE);
                string assetJSON = JSONTools.LoadToString(s_assets);

                if (!System.IO.Directory.Exists(Constants.ASSETS_CACHE))
                    System.IO.Directory.CreateDirectory(Constants.ASSETS_CACHE);

                using (var writer = new System.IO.StreamWriter(assetPath + ".json"))
                {
                    writer.Write(assetJSON);
                }
            }

            public static void LoadFromCache()
            {
                string assetPath = string.Format(@"{0}/AssetList.json", Constants.ASSETS_CACHE);

                if (System.IO.File.Exists(assetPath))
                {
                    foreach (var asset in JSONTools.Load<Asset[]>(assetPath))
                    {
                        if (!s_assets.Contains(asset))
                            s_assets.Add(asset);
                    }
                }

            }
        }

        [Serializable]
        public class Metadata
        {
            public List<string> tags;
            public Node node;

            public Metadata()
            {
                tags = new List<string>();
            }
        }

        public enum Process
        {
            Error = -1,
            Idle = 0,
            Creating = 1,
            Loading,
            Fetching,
            FetchingDataPath,
            FetchingMetadata,
            FetchingPreviewPath,
            Updating,
            UpdatingMetadata,
            Downloading,
            RequestingEndpoint,
            Uploading,
            Deleting
        }

        public string name;
        public string id;
        public string ownerID;
        public string creatorID;
        public string bucket;
        public string key;
        public string assetType;
        public System.DateTime modified;
        public System.DateTime created;
        public string pathToData;
        public string originalFileName;

        public string localPathToData
        {
            get
            {
                return string.IsNullOrEmpty(pathToData) ? "" : pathToData.Replace("file:///", "");
            }
        }
        public string previewUrl { get; set; }
        public Texture thumb { get; set; }

        private string sourcePath;
        private string uploadUrl;

        public Metadata metadata { get; set; }
        public List<string> tags
        {
            get
            {
                if (metadata == null)
                    metadata = new Metadata();

                if (metadata.tags == null)
                    metadata.tags = new List<string>();

                return metadata.tags;
            }

            set
            {
                metadata.tags = value;
            }
        }

        bool isProcessing;
        public uint dataVersion = 0;

        //TODO: This should become a single accessor for the loaded asset binary: bundle, texture, etc.
        public AssetBundle bundle { get; private set; }
        public Texture texture { get; private set; }
        public AudioClip audio { get; private set; }
        //public Material material;
        public ShaderInterface shaderInterface { get; private set; }
        private List<GameObject> prefabs;
        public int SceneCount
        {
            get
            {
                if (bundle != null)
                    return bundle.GetAllScenePaths().Length;
                else
                    return 0;
            }
        }

        public bool isCached { get; private set; }
        public bool isLoaded { get; private set; }

        protected Queue<AssetHandlerWidget> InstanceRequestQueue;

        #region Constructors
        public Asset()
        {
            isLoaded = false;
            onLoadAssetDone += SaveToCache;
        }

        public Asset(string id, string name = "", string type = "", bool addToManager = true)
        {
            this.id = id;
            this.name = name;
            this.assetType = type;
            isLoaded = false;

            if (addToManager)
            {
                var data = JsonUtility.ToJson(AssetsManager.AddAsset(this));
                JsonUtility.FromJsonOverwrite(data, this);
            }

            onLoadAssetDone += SaveToCache;
        }

        #endregion

        #region Static List Request
        /// <summary>
        /// Static method for getting a list of assets. All returned assets will be registered with the AssetsManager.
        /// </summary>
        /// <param name="callback">an optional callback</param>
        /// <returns>a List of Assets with names and IDs populated</returns>
        public static List<Asset> GetAssetList(RestGetAssetListCallback callback)
        {
            return RestManager.Request.GetAssetList(callback);
        }

        /// <summary>
        /// Static method for getting a list of assets. All returned assets will be registered with the AssetsManager.
        /// </summary>
        /// <returns>a List of Assets with names and IDs populated</returns>
        public static List<Asset> GetAssetList()
        {
            return RestManager.Request.GetAssetList(OnGetAssetListResponse);
        }

        private static void OnGetAssetListResponse(bool error, RestGetAssetListResponseData assetData)
        {
            if (error)
            {
                Debug.Log("[Error] Retrieving Asset list failed.");
            }
            else
            {
                Debug.Log("[Completed Process: Fetch Asset List]");
            }
        }
        #endregion

        #region Process Management
        /// <summary>
        /// Checks if the Asset is currently waiting for the given process to complete.
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public bool InProcess(Process process)
        {
            if (processSet == null)
                processSet = new List<Process>();

            return process == Process.Idle ? processSet.Count == 0 : processSet.Contains(process);
        }

        private List<Process> processSet;

        public delegate void ProcessBeginning(Asset sender, Process[] currentProcesses, Process beginningProcess);
        public delegate void ProcessEnding(Asset sender, Process[] currentProcesses, Process endingProcess);
        public delegate void ProcessErroring(Asset sender, Process[] currentProcesses, Process erroringProcess);

        public event ProcessBeginning onProcessBegin;
        public event ProcessEnding onProcessEnd;
        public event ProcessErroring onProcessError;

        private bool ProcessStart(Process newProcess)
        {
            if (processSet == null)
                processSet = new List<Process>();

            if (!processSet.Contains(newProcess))
            {
                Debug.Log(this.name + "(" + this.id + ") [Beginning Process: " + newProcess + "]");
                processSet.Add(newProcess);

                if (onProcessBegin != null)
                    onProcessBegin(this, processSet.ToArray(), newProcess);

                return true;
            }
            else
            {
                Debug.Log(this.name + "(" + this.id + ") [Failed To Start Process: " + newProcess + "]: Process already running.");
                return false;
            }
        }

        private bool ProcessEnd(Process process)
        {
            Debug.Log(this.name + " (" + id + ") [Completed Process: " + process.ToString() + "]");

            if (onProcessEnd != null)
                onProcessEnd(this, processSet.ToArray(), process);

            return processSet.Remove(process);
        }

        private void ProcessError(Process erroringProcess)
        {
            processSet.Add(Process.Error);

            Debug.Log(this.name + " (" + id + ") [Erroring Process: " + erroringProcess.ToString() + "]");

            if (onProcessError != null)
                onProcessError(this, processSet.ToArray(), erroringProcess);
        }

        private float progress;
        private string progressMessage;
        public bool isDone;

        /// <summary>
        /// The progress of download of the Asset's Data after calling FetchData().
        /// </summary>
        /// <returns></returns>
        public float GetProgress()
        {
            return progress;
        }

        public string GetProgressMessage()
        {
            return progressMessage;
        }
        #endregion

        #region Creation/Deletion
        /// <summary>
        /// Creates a new Spaces Asset on the server.
        /// </summary>
        /// <param name="assetName">Name for the asset</param>
        /// <param name="assetType">Type of asset</param>
        /// <param name="sourcePath">Path to the data file to be uploaded</param>
        /// <returns></returns>
        public static Asset Create(string assetName, string assetType, string sourcePath = "")
        {
            var createdAsset = RestManager.Request.CreateAsset(assetName, assetType, sourcePath, OnCreateAssetResponse);
            createdAsset.ProcessStart(Process.Creating);
            return createdAsset;
        }

        public void SetDataSourcePath(string path)
        {
            sourcePath = path;
        }

        private static void OnCreateAssetResponse(bool error, CreateAssetResponseData responseData)
        {
            if (error)
            {
                responseData.asset.ProcessError(Process.Creating);
            }
            else
            {
                responseData.asset.AddData(responseData.asset.sourcePath);
                responseData.asset.ProcessEnd(Process.Creating);
            }
        }


        /// <summary>
        /// Static method for deleting an asset from the server by ID.
        /// </summary>
        /// <param name="assetID">ID of Asset to be deleted</param>
        public static void Delete(string assetID)
        {
            RestManager.Request.DeleteAsset(assetID, OnDeleteAssetResponse);
        }

        /// <summary>
        /// Deletes this Asset from the server.
        /// </summary>
        public void Delete()
        {
            if (ProcessStart(Process.Deleting))
                RestManager.Request.DeleteAsset(this, OnDeleteAssetResponse);
        }

        private static void OnDeleteAssetResponse(bool error, RestDeleteAssetResponseData reply)
        {
            if (error)
            {
                reply.asset.ProcessError(Process.Deleting);
            }
            else
            {
                if (reply.asset != null)
                    reply.asset.ProcessEnd(Process.Deleting);
            }
        }

        #endregion


        #region Asset Loading
        public delegate void LoadAssetBegin(Asset asset);
        public delegate void LoadAssetDone(Asset asset);
        public delegate void LoadAssetCancel(Asset asset);

        public event LoadAssetBegin onLoadAssetBegin;
        public event LoadAssetDone onLoadAssetDone;
        public event LoadAssetCancel onLoadAssetCancel;

        /// <summary>
        /// Re-fetches the full info of this Asset.
        /// </summary>
        public void RefreshInfo()
        {
            FetchAssetInfo();
            FetchDataPath();
            FetchMetadata();
        }

        private void FetchAssetInfo()
        {
            if (ProcessStart(Process.Fetching))
            {
                if (TryFetchFromCache())
                    ProcessEnd(Process.Fetching);
                else
                    RestManager.Request.GetAsset(id, OnFetchResponse);
            }
        }

        private void OnFetchResponse(bool error, RestGetAssetResponseData assetData)
        {
            if (error)
            {
                ProcessError(Process.Fetching);
            }
            else
            {
                try
                {
                    if (JSONTools.ParseDateTimeFromString(assetData.modified_at) > this.modified)
                    {
                        CopyFromGetResponseData(assetData);
                    }

                    SaveToCache();
                    ProcessEnd(Process.Fetching);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    ProcessError(Process.Fetching);
                }
            }
        }

        private void FetchDataPath()
        {
            if (ProcessStart(Process.FetchingDataPath))
            {
                if (System.IO.File.Exists(localPathToData))
                    ProcessEnd(Process.FetchingDataPath);
                else
                {
                    #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                    RestManager.Request.GetAssetData(id, Constants.Platform.win.ToString(), OnRequestDataPathResponse);

                    #elif UNITY_ANDROID
                    RestManager.Request.GetAssetData(id, Constants.Platform.droid.ToString(), OnRequestDataPathResponse);

                    #elif UNITY_IOS
                    RestManager.Request.GetAssetData(id, Constants.Platform.ios.ToString(), OnRequestDataPathResponse);

                    #endif
                }
            }
        }

        private void OnRequestDataPathResponse(bool error, Spaces.Core.RestAPI.RestGetAssetDataResponse response)
        {
            if (error)
            {
                ProcessError(Process.FetchingDataPath);
            }
            else
            {
                var data = response.data;

                string downloadPath = "";

                if (data.Count != 0)
                {
                    //Debug.Log(this.name + " (" + this.id + ") New Path Retrieved");
                    downloadPath = data.FirstOrDefault(item => item.Key.EndsWith(".unity3d")).Value.url;
                }
                else
                {
                    Debug.LogWarning(this.name + " (" + this.id + ") [Using default pathToBundle value]: " + pathToData);
                    downloadPath = pathToData;
                }

                pathToData = downloadPath;
                ProcessEnd(Process.FetchingDataPath);
            }
        }

        private void FetchMetadata()
        {
            if (ProcessStart(Process.FetchingMetadata))
            {
                if (TryFetchMetadataFromCache())
                    ProcessEnd(Process.FetchingMetadata);
                else
                    RestManager.Request.GetAssetMetadata(id, OnFetchMetadataResponse);
            }
            
        }

        private void OnFetchMetadataResponse(bool error, RestGetAssetMetadataResponse responseData)
        {
            if (error)
            {
                ProcessError(Process.FetchingMetadata);
            }
            else
            {
                metadata = responseData.metadataObject;

                if (metadata != null && metadata.node == null)
                    metadata.node = new Node(name);

                SaveMetadataToCache();

                ProcessEnd(Process.FetchingMetadata);
            }
        }

        public void FetchPreview()
        {
            if (ProcessStart(Process.FetchingPreviewPath))
                RestManager.Request.GetAssetPreview(id, OnFetchThumbResponse);
        }

        void OnFetchThumbResponse(bool error, RestAPI.RestGetAssetPreviewResponse response)
        {
            if (error)
            {
                previewUrl = "NONE";

                if (assetType == Constants.AssetType.image.ToString())
                {
                    RefreshInfo();
                }
                else if (!TryLoadPreviewFromCache())
                {
                    //thumb = new Texture2D(0, 0);
                }

                ProcessEnd(Process.FetchingPreviewPath);
            }
            else
            {
                previewUrl = response.url;
                ProcessEnd(Process.FetchingPreviewPath);
            }
        }

        public virtual IEnumerator LoadAsset(AssetWidget handler = null)
        {
            if (ProcessStart(Process.Loading))
            {
                MonoBehaviour coroutineHandler = handler ? handler : UserSession.Instance as MonoBehaviour;

                if (onLoadAssetBegin != null)
                    onLoadAssetBegin(this);

                if (!isLoaded)
                {
                    RefreshInfo();

                    if (!isProcessing)
                    {
                        yield return coroutineHandler.StartCoroutine(ProcessFetchAsset());

                        if (!isProcessing)
                        {
                            yield return ProcessDownloadAsset(coroutineHandler);

                            try
                            {
                                GetAssetGameObjects();
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(this.name + " (" + this.id + ") [Loading Error]");
                                isDone = true;
                                progress = 1;
                                isProcessing = false;
                            }

                            isLoaded = true;
                        }
                    }
                }

                ProcessEnd(Process.Loading);

                if (onLoadAssetDone != null)
                    onLoadAssetDone(this);

            }

            if (handler)
            {
                handler.gameObject.name = name;
            }
        }

        public virtual IEnumerator LoadAssetInBackground(Widget handler = null)
        {
            if (handler && ProcessStart(Process.Loading))
            {
                MonoBehaviour coroutineHandler = handler ? handler : UserSession.Instance as MonoBehaviour;

                if (onLoadAssetBegin != null)
                    onLoadAssetBegin(this);

                if (!isLoaded)
                {
                    RefreshInfo();

                    if (!isProcessing)
                    {
                        yield return coroutineHandler.StartCoroutine(ProcessFetchAsset());

                        if (!isProcessing)
                        {
                            yield return ProcessDownloadAsset(coroutineHandler);

                            try
                            {
                                GetAssetGameObjects();
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(this.name + " (" + this.id + ") [Loading Error]");
                                isDone = true;
                                progress = 1;
                                isProcessing = false;
                            }

                            isLoaded = true;
                        }
                    }
                }

                ProcessEnd(Process.Loading);

                if (onLoadAssetDone != null)
                    onLoadAssetDone(this);
            }
        }

        public virtual void ReloadAsset(AssetWidget handler = null)
        {
            ClearFromCache(this);
            pathToData = "";
            metadata = null;

            isLoaded = false;
            prefabs = null;
            bundle = null;
            texture = null;

            dataVersion++;

            MonoBehaviour coroutineHandler = handler ? handler : UserSession.Instance as MonoBehaviour;
            coroutineHandler.StartCoroutine(LoadAsset(handler));
        }

        protected virtual IEnumerator ProcessFetchAsset()
        {
            isProcessing = true;
            float timeout = 5;
            float elapsed = 0;

            progressMessage = "Fetching\nAsset";

            while (InProcess(Process.Fetching) && !InProcess(Process.Error))
            {
                elapsed = Mathf.Clamp(elapsed + Time.deltaTime, 0, timeout);

                var current = elapsed / timeout;
                progress = current;

                yield return null;
            }

            while (InProcess(Process.FetchingDataPath) && !InProcess(Process.Error))
            {
                elapsed = Mathf.Clamp(elapsed + Time.deltaTime, 0, timeout);

                var current = elapsed / timeout;
                progress = current;

                yield return null;
            }

            while (InProcess(Process.FetchingMetadata) && !InProcess(Process.Error))
            {
                elapsed = Mathf.Clamp(elapsed + Time.deltaTime, 0, timeout);

                var current = elapsed / timeout;
                progress = current;

                yield return null;
            }

            isProcessing = false;
            progress = 1;
        }

        protected Coroutine ProcessDownloadAsset(MonoBehaviour handler)
        {
            if (!InProcess(Process.Error) && ProcessStart(Process.Downloading))
            {
                progressMessage = "Downloading";
                progress = 0;

                // TODO: handle null assetType value
                if (string.IsNullOrEmpty(assetType))
                {
                    Debug.LogWarning(this.name + " (" + this.id + ") [AssetType null]");
                    ProcessError(Process.Downloading);
                }
                else
                {
                    if (assetType == Constants.AssetType.model.ToString() 
                        || assetType == Constants.AssetType.assetbundle.ToString() 
                        || assetType.ToLower() == "fbx" 
                        || assetType.ToLower() == "blend"
                        || assetType == Constants.AssetType.audio.ToString()
                        )
                    {
                        return handler.StartCoroutine(GetAssetBundle());
                    }
                    else if (assetType == Constants.AssetType.image.ToString() || assetType.ToLower() == "jpg")
                    {
                        return handler.StartCoroutine(GetTexture());
                    }
                    else if (assetType == Constants.AssetType.video.ToString())
                    {
                        return handler.StartCoroutine(GetVideo());
                    }
                    else
                    {
                        isDone = true;
                        progress = 1;

                        Debug.Log(this.name + "[No Handler Defined] for assetType: " + assetType);
                        //TODO: Download and cache local file
                    }
                }

                isProcessing = false;
            }

            return null;
        }

        private IEnumerator GetAssetBundle()
        {
            // Wait for Fetch of data path to return.
            while (!InProcess(Process.Error) && InProcess(Process.FetchingDataPath))
                yield return null;

            if (!InProcess(Process.Error) && InProcess(Process.Downloading))
            {
                using (var request = UnityWebRequest.GetAssetBundle(pathToData, dataVersion, 0))
                {
                    var async = request.Send();

                    while (!async.isDone)
                    {
                        isDone = async.isDone;
                        progress = async.progress;
                        yield return async.isDone;
                    }

                    if (request.isError)
                    {
                        Debug.LogError(this.name + " (" + this.id + ") [AssetBundle Download Error]" + request.error);
                        ProcessError(Process.Downloading);
                    }
                    else
                    {
                        var handler = (DownloadHandlerAssetBundle)request.downloadHandler;
                        bundle = handler.assetBundle;

                        if (bundle)
                        {
                            ProcessEnd(Process.Downloading);
                        }
                        else
                        {
                            ProcessError(Process.Downloading);
                        }
                    }

                    isDone = true;
                    progress = 1;
                }
            }
        }

        private IEnumerator GetTexture()
        {
            if (TryLoadDataFromCache())
            {
                ProcessEnd(Process.Downloading);
            }
            else
            {
                using (var request = UnityWebRequest.GetTexture(pathToData, true))
                {
                    var async = request.Send();

                    while (!async.isDone)
                    {
                        isDone = async.isDone;
                        progress = async.progress;
                        yield return async.isDone;
                    }

                    if (request.isError)
                    {
                        Debug.LogError(this.name + " (" + this.id + ") [Texture Download Error] " + request.error);
                        ProcessError(Process.Downloading);
                    }
                    else
                    {
                        var tex = DownloadHandlerTexture.GetContent(request);

                        if (tex)
                        {
                            texture = tex;
                            ProcessEnd(Process.Downloading);

                            // TODO: Determine whether we want to cache the original file or convert to PNG
                            //WriteDataToCache(tex.EncodeToPNG());
                            WriteDataToCache(request.downloadHandler.data);
                        }
                        else
                        {
                            ProcessError(Process.Downloading);
                        }
                    }
                }
            }

            isDone = true;
            progress = 1;
        }

        private IEnumerator GetVideo()
        {
            if (!System.IO.File.Exists(localPathToData))
            {
                using (var request = UnityWebRequest.Get(pathToData))
                {
                    var async = request.Send();

                    while (!async.isDone)
                    {
                        isDone = async.isDone;
                        progress = async.progress;
                        yield return async.isDone;
                    }

                    if (request.isError)
                    {
                        Debug.Log(request.error);
                        ProcessError(Process.Downloading);
                    }
                    else
                    {
                        WriteDataToCache(request.downloadHandler.data);

                        if (System.IO.File.Exists(localPathToData))
                            ProcessEnd(Process.Downloading);
                        else
                            ProcessError(Process.Downloading);
                    }
                }
            }
            else
            {
                //Video already cached.
            }

            isDone = true;
            progress = 1;
        }

        private IEnumerator GetAudio(AudioType audioFormat)
        {
            //TODO: load from cache
            if (ProcessStart(Process.Downloading))
            {
                using (var request = UnityWebRequest.GetAudioClip(pathToData, audioFormat))
                {
                    var async = request.Send();

                    while (!async.isDone)
                    {
                        isDone = async.isDone;
                        progress = async.progress;
                        yield return async.isDone;
                    }

                    if (request.isError)
                    {
                        Debug.Log(this.name + " (" + this.id + ") [Audio Download Error]" + request.error);
                        ProcessError(Process.Downloading);
                    }
                    else
                    {
                        audio = ((DownloadHandlerAudioClip)request.downloadHandler).audioClip;

                        if (audio)
                        {
                            WriteDataToCache(request.downloadHandler.data);
                            ProcessEnd(Process.Downloading);
                        }
                        else
                        {
                            ProcessError(Process.Downloading);
                        }
                    }

                    isDone = true;
                    progress = 1;
                }
            }
        }

        internal void DownloadDataToCache()
        {
            if (!System.IO.File.Exists(localPathToData))
            {
                using (var client = new System.Net.WebClient())
                {
                    string assetPath = string.Format(@"{0}/{1}", Constants.ASSETS_CACHE, id);
                    string assetExt = System.IO.Path.GetExtension(string.IsNullOrEmpty(originalFileName) ? name : originalFileName);
                    string destinationPath = string.Format("file:///{0}{1}", assetPath, assetExt);

                    try
                    {
                        client.DownloadFile(pathToData, destinationPath);

                        pathToData = destinationPath;
                        SaveToCache();

                        if (assetType == Core.Constants.AssetType.image.ToString() && previewUrl == "NONE")
                        {
                            previewUrl = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(name + " (" + id + ") [Error Downloading Data to Cache] " + ex);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the list of any GameObjects that can be loaded from this Asset.
        /// </summary>
        /// <returns></returns>
        public GameObject[] GetAssetGameObjects()
        {
            // TODO: Update this to return the Default Placeholder
            if (InProcess(Process.Error) || !isLoaded)
                return new GameObject[0];

            //Debug.LogWarning(this.name + " (" + this.id + ") [PREFABS] null: " + (prefabs == null));

            //TODO: Add a try/catch to correct for errors thrown when trying to load.
            if (prefabs == null)
            {
                if (bundle && bundle.GetAllAssetNames() != null)
                {
                    prefabs = new List<GameObject>();// bundle.GetAllAssetNames().Length);

                    List<string> nameList = bundle.GetAllAssetNames().Where(name => name.EndsWith(".prefab")).ToList();

                    if (nameList.Count == 0)
                        nameList.AddRange(bundle.GetAllAssetNames());

                    foreach (var goName in nameList)
                    {
                        var go = bundle.LoadAsset<GameObject>(goName);

                        if (go != null)
                            prefabs.Add(bundle.LoadAsset<GameObject>(goName));
                    }

                    bundle.Unload(false);
                }
                else
                    return new GameObject[0];
            }

            return prefabs.ToArray();
        }

        /// <summary>
        /// Reloads all prefabs from this Asset's assetBundle.
        /// </summary>
        /// <param name="destroyAllInstancedObjects">Should objects instanced already by destroyed.</param>
        public void ReloadAssetBundle(bool destroyAllInstancedObjects = false)
        {
            UnloadAssetBundle(destroyAllInstancedObjects);
            GetAssetGameObjects();
        }

        /// <summary>
        /// Unload assetBundle for this Asset.
        /// </summary>
        /// <param name="destroyAllInstancedObjects">Should objects instanced already by destroyed.</param>
        public void UnloadAssetBundle(bool destroyAllInstancedObjects = false)
        {
            if (bundle)
                bundle.Unload(destroyAllInstancedObjects);

            prefabs = null;
        }


        /// <summary>
        /// Creates an instance of this Asset's data in the Space.
        /// </summary>
        /// <returns>the root GameObject of the instanced data</returns>
        public GameObject SpawnAssetInstance()
        {
            GameObject gameObject = new GameObject(name);

            var assetWidget = gameObject.AddComponent<AssetWidget>();
            assetWidget.Initialize(this);

            return gameObject;
        }

        public void EnqueueHandlerRequest(AssetHandlerWidget handler)
        {
            Debug.Log(this.name + " (" + this.id + ") [Handler Request Received] " + handler.name);

            if (InstanceRequestQueue == null)
            {
                InstanceRequestQueue = new Queue<AssetHandlerWidget>();
            }

            bool startHandlerProcess = InstanceRequestQueue.Count > 0 ? false : true;

            InstanceRequestQueue.Enqueue(handler);

            while (InstanceRequestQueue.Count > 0)
            {
                switch (assetType.ToLower())
                {
                    case "model":
                    case "fbx":
                        {
                            if (prefabs == null)
                                GetAssetGameObjects();

                            HandleModel(InstanceRequestQueue.Dequeue() as ModelWidget);
                        }
                        break;
                    case "audio":
                        HandleSound(InstanceRequestQueue.Dequeue() as SoundWidget);
                        break;
                    case "movfile":
                    case "video":
                        HandleVideo(InstanceRequestQueue.Dequeue() as VideoWidget);
                        break;
                    case "image":
                    case "jpg":
                    case "jpeg":
                        HandleImage(InstanceRequestQueue.Dequeue() as ImageWidget);
                        break;
                    case "text":
                        HandleDefault(InstanceRequestQueue.Dequeue());
                        break;
                    case "material":
                        HandleMaterial(InstanceRequestQueue.Dequeue() as MaterialWidget);
                        break;
                    case "shader":
                        HandleShader(InstanceRequestQueue.Dequeue() as MaterialWidget);
                        break;
                    case "skybox":
                        HandleDefault(InstanceRequestQueue.Dequeue());
                        break;
                    case "compound":
                    case "assetbundle":
                    case "scene":
                    default:
                        HandleDefault(InstanceRequestQueue.Dequeue());
                        break;

                }
            }
        }

        private void HandleModel(ModelWidget modelWidget)
        {
            foreach (var go in GetAssetGameObjects())
            {
                var instancedGO = GameObject.Instantiate(go, modelWidget.transform, false);

                if (!instancedGO.GetComponentInChildren<Collider>())
                {
                    foreach (var mesh in instancedGO.GetComponentsInChildren<Renderer>())
                    {
                        mesh.gameObject.AddComponent<BoxCollider>();
                    }

                    instancedGO.AddComponent<Rigidbody>().isKinematic = true;
                }

                var animator = instancedGO.GetComponentInChildren<Animator>();

                if (animator && animator.runtimeAnimatorController != null)
                {
                    var animWidget = modelWidget.GetComponent<AnimatorWidget>();

                    if (!animWidget)
                        animWidget = modelWidget.gameObject.AddComponent<AnimatorWidget>();

                    animWidget.Initialize(modelWidget, animator);
                }

                //MaterialWidget.GenerateMaterialWidgets(modelWidget);
                modelWidget.assetWidget.AddAssetInstance(instancedGO.transform);
            }

            //Instantiation completed Event
            modelWidget.OnInstantiateAsset();
        }

        private void HandleVideo(VideoWidget videoWidget)
        {
            videoWidget.SetContent(pathToData.Replace("file:///", "file://"));
            videoWidget.OnInstantiateAsset();
        }

        private void HandleSound(SoundWidget soundWidget)
        {
            string[] nameList = bundle.GetAllAssetNames();

            foreach (var assetName in nameList)
            {
                Debug.Log(this.name + " Asset Name: " + assetName);

                var audio = bundle.LoadAsset<AudioClip>(assetName);

                // TODO: Update to allow loading of multiple sound assets from bundle
                if (audio != null)
                {
                    soundWidget.SetContent(audio);
                }
            }

            bundle.Unload(false);

            soundWidget.OnInstantiateAsset();
        }

        private void HandleImage(ImageWidget imageWidget)
        {
            if (texture)
                Debug.Log(this.name + " (" + this.id + ") [Image Handler] " + imageWidget.name + " Content: " + texture.name + " (" + texture.width + ", " + texture.height + ")");
            else
                Debug.Log(this.name + " (" + this.id + ") [Image Handler] " + imageWidget.name + " Content: [NULL TEXTURE]");


            imageWidget.SetContent(texture);
            imageWidget.Initialize();
            imageWidget.OnInstantiateAsset();
        }

        private void HandleMaterial(MaterialWidget materialWidget)
        {
        }

        private void HandleShader(MaterialWidget materialWidget)
        {
            if (bundle)
            {
                foreach (var assetName in bundle.GetAllAssetNames().Where(name => name.EndsWith(".asset")))
                {
                    var shaderInterface = bundle.LoadAsset<ShaderInterface>(assetName);
                    var shader = bundle.LoadAsset<Shader>(shaderInterface.shaderName + ".shader");

                    if (shaderInterface)
                    {
                        materialWidget.SetShader(shaderInterface);
                        break;
                    }
                }

                bundle.Unload(false);
            }

            materialWidget.OnInstantiateAsset();
        }

        private void HandleDefault(AssetHandlerWidget handler)
        {
            handler.OnInstantiateAsset();
        }

        /// <summary>
        /// Additively loads a unity scene from this Asset's assetBundle in the current Space.
        /// </summary>
        /// <param name="index">index of scene</param>
        /// <returns></returns>
        public IEnumerator LoadSceneAdditive(int index = 0)
        {
            if (SceneCount > index)
            {
                string scene = System.IO.Path.GetFileNameWithoutExtension(bundle.GetAllScenePaths()[index]);

                var async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene, UnityEngine.SceneManagement.LoadSceneMode.Additive);

                while (!async.isDone)
                {
                    yield return isDone;
                    Debug.Log(this.name + " [Additively Loading Scene] " + scene);
                }
            }

        }

        #endregion

        #region Asset Modification
        /// <summary>
        /// Updates the information for this Asset on the server.
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="assetType"></param>
        /// <param name="ownerID"></param>
        /// <param name="originalFilename"></param>
        public void Update(string assetName, string assetType, string ownerID, string originalFilename)
        {
            if (!string.IsNullOrEmpty(assetName))
                this.name = assetName;

            if (!string.IsNullOrEmpty(assetType))
                this.assetType = assetType;

            if (!string.IsNullOrEmpty(ownerID))
                this.ownerID = ownerID;

            if (!string.IsNullOrEmpty(originalFilename))
                this.originalFileName = originalFilename;

            if (ProcessStart(Process.Updating))
                RestManager.Request.UpdateAsset(this.id, this.name, this.assetType, this.ownerID, this.originalFileName, OnUpdateAssetResponse);
        }

        private void OnUpdateAssetResponse(bool error, string reply)
        {
            if (error)
            {
                ProcessError(Process.Updating);
            }
            else
            {
                SaveToCache();
                ProcessEnd(Process.Updating);
            }
        }

        /// <summary>
        /// Adds a metadata tag to the Asset, and updates the metadata of the Asset on the server.
        /// </summary>
        /// <param name="tag">tag to be added</param>
        public void AddTag(string tag)
        {
            if (tags == null)
                tags = new List<string>();

            if (!tags.Contains(tag))
                tags.Add(tag);

            UpdateMetadata();
        }

        /// <summary>
        /// Removes a metadata tag from the Asset, and updates the metadata of the Asset on the server.
        /// </summary>
        /// <param name="tag">tag to be removed</param>
        public void RemoveTag(string tag)
        {
            if (tags != null)
                tags.Remove(tag);

            UpdateMetadata();
        }

        public void ResetToDefault()
        {
            if (metadata != null)
                metadata.node = new Node(name);

            ClearFromCache(this);
            UpdateMetadata();
        }

        public void UpdateMetadata()
        {
            if (ProcessStart(Process.UpdatingMetadata))
                RestManager.Request.UpdateAssetMetadata(id, metadata, OnUpdataMetadataResponse);
        }

        private void OnUpdataMetadataResponse(bool error, string response)
        {
            if (error)
            {
                ProcessError(Process.UpdatingMetadata);
            }
            else
            {
                SaveMetadataToCache();
                ProcessEnd(Process.UpdatingMetadata);
            }
        }

        /// <summary>
        /// Uploads a file to the server and sets it as the data for this Asset.
        /// </summary>
        /// <param name="filePath">path to the file</param>
        public void AddData(string filePath)
        {
            if (InProcess(Process.Error))
                return;

            sourcePath = filePath;

            if (string.IsNullOrEmpty(uploadUrl))
                RequestUploadEndpoint();
            else
                UploadFile(filePath);
        }

        private void RequestUploadEndpoint()
        {
            if (ProcessStart(Process.RequestingEndpoint))
                RestManager.Request.GetAssetUploadEndpoint(assetType, id, OnAssetUploadEndointResponse);
        }

        private void OnAssetUploadEndointResponse(bool error, GetAssetUploadEndpointResponseData data)
        {
            if (error)
            {
                ProcessError(Process.RequestingEndpoint);
            }
            else
            {
                uploadUrl = data.uploadUrl;
                UploadFile(sourcePath);
                ProcessEnd(Process.RequestingEndpoint);
            }
        }

        private void UploadFile(string assetPath)
        {
            if (string.IsNullOrEmpty(uploadUrl))
            {
                Debug.Log(this.name + " [Upload requested but uploadUrl is empty]");
                AddData(assetPath);
            }
            else
            {
                if (ProcessStart(Process.Uploading))
                    RestManager.Request.UploadFile(uploadUrl, assetPath, OnUploadResponse);
            }
        }

        private void OnUploadResponse(bool error, string reply)
        {
            if (error)
            {
                ProcessError(Process.Uploading);
            }
            else
            {
                ProcessEnd(Process.Uploading);
            }
        }

        #endregion

        #region Caching
        public void CachePreview()
        {
            string assetThumbPath = string.Format(@"{0}/{1}_thumb.png", Constants.ASSETS_CACHE, id);

            if (!System.IO.Directory.Exists(Constants.ASSETS_CACHE))
                System.IO.Directory.CreateDirectory(Constants.ASSETS_CACHE);

            try
            {
                System.IO.File.WriteAllBytes(assetThumbPath, ((Texture2D)thumb).EncodeToPNG());
                previewUrl = assetThumbPath;
            }
            catch (Exception ex)
            {
                Debug.LogError(name + " (" + id + ") [Error Writing Preview to Cache] " + ex);
            }
        }

        public bool TryLoadPreviewFromCache()
        {
            bool success = false;

            string assetThumbPath = string.Format(@"{0}/{1}_thumb.png", Constants.ASSETS_CACHE, id);

            if (System.IO.File.Exists(assetThumbPath))
            {
                var thumbTex = new Texture2D(1, 1);
                var thumbBytes = System.IO.File.ReadAllBytes(assetThumbPath);
                thumbTex.LoadImage(thumbBytes);
                thumb = thumbTex;

                success = true;
            }
            else if (assetType == Constants.AssetType.image.ToString())
            {
                string imgPath = localPathToData;

                if (System.IO.File.Exists(imgPath))
                {
                    var tex = new Texture2D(1, 1);
                    var texBytes = System.IO.File.ReadAllBytes(imgPath);
                    tex.LoadImage(texBytes);
                    texture = tex;
                    thumb = tex.Scale(64, 64);

                    previewUrl = "NONE";
                    CachePreview();

                    success = true;
                }

            }

            return success;
        }

        /// <summary>
        /// Serializes the Asset's info and writes it and the Asset's Data to a local cache.
        /// </summary>
        /// <param name="assetData"></param>
        public void WriteDataToCache(byte[] assetData)
        {
            string assetPath = string.Format(@"{0}/{1}", Constants.ASSETS_CACHE, id);
            string assetExt = System.IO.Path.GetExtension(originalFileName);

            try
            {
                System.IO.File.WriteAllBytes(assetPath + assetExt, assetData);
                pathToData = string.Format("file:///{0}{1}", assetPath, assetExt);

                SaveToCache();
            }
            catch (Exception ex)
            {
                Debug.LogError( name + " (" + id + ") [Error Writing Data to Cache] " + ex);
            }

            isCached = true;
        }

        public bool TryLoadDataFromCache()
        {
            bool success = false;

            string imgPath = localPathToData;

            if (System.IO.File.Exists(imgPath))
            {
                var tex = new Texture2D(1, 1);
                var texBytes = System.IO.File.ReadAllBytes(imgPath);
                tex.LoadImage(texBytes);
                //tex.LoadRawTextureData(texBytes);
                //tex.Apply();
                texture = tex;

                success = true;
            }

            return success;
        }

        /// <summary>
        /// Serializes the Asset's info and writes it to a local cache.
        /// </summary>
        public static void SaveToCache(Asset asset)
        {
            string assetPath = string.Format(@"{0}/{1}", Constants.ASSETS_CACHE, asset.id);
            string assetJSON = JSONTools.LoadToString(asset);

            if (!System.IO.Directory.Exists(Constants.ASSETS_CACHE))
                System.IO.Directory.CreateDirectory(Constants.ASSETS_CACHE);

            using (var writer = new System.IO.StreamWriter(assetPath + ".json"))
            {
                writer.Write(assetJSON);
            }

            asset.isCached = true;
        }

        public void SaveToCache()
        {
            SaveToCache(this);
        }

        private bool TryFetchFromCache()
        {
            bool success = false;
            string thisname = name;

            string assetPath = string.Format("{0}/{1}.json", Constants.ASSETS_CACHE, id);

            if (System.IO.File.Exists(assetPath))
            {
                Asset cachedAsset = JSONTools.Load<Asset>(assetPath);

                if (cachedAsset != null)
                {
                    Copy(cachedAsset);
                    success = true;
                    Debug.Log(cachedAsset.name + " asset found in cache");
                }
            }

            return success;
        }

        public static void ClearFromCache(Asset asset)
        {
            string assetPath = string.Format(@"{0}/{1}.json", Constants.ASSETS_CACHE, asset.id);

            if (System.IO.File.Exists(assetPath))
                System.IO.File.Delete(assetPath);

            string metadataPath = string.Format(@"{0}/{1}_metadata.json", Constants.ASSETS_CACHE, asset.id);

            if (System.IO.File.Exists(metadataPath))
                System.IO.File.Delete(metadataPath);
        }


        public static void SaveMetadataToCache(Asset asset)
        {
            string assetPath = string.Format(@"{0}/{1}_metadata.json", Constants.ASSETS_CACHE, asset.id);
            string assetJSON = JSONTools.LoadToString(asset.metadata);

            if (!System.IO.Directory.Exists(Constants.ASSETS_CACHE))
                System.IO.Directory.CreateDirectory(Constants.ASSETS_CACHE);

            using (var writer = new System.IO.StreamWriter(assetPath))
            {
                writer.Write(assetJSON);
            }

            asset.isCached = true;
        }

        public void SaveMetadataToCache()
        {
            SaveMetadataToCache(this);
        }

        private bool TryFetchMetadataFromCache()
        {
            bool success = false;
            string thisname = name;

            string assetPath = string.Format("{0}/{1}_metadata.json", Constants.ASSETS_CACHE, id);

            if (System.IO.File.Exists(assetPath))
            {
                var cachedAssetMetadata = JSONTools.Load<Metadata>(assetPath);

                if (cachedAssetMetadata != null)
                {
                    metadata = cachedAssetMetadata;
                    success = true;
                }
            }

            return success;
        }

        #endregion

        private void Copy(Spaces.Core.Asset sourceAsset)
        {
            //this.id = sourceAsset.id;
            this.ownerID = sourceAsset.ownerID;
            this.creatorID = sourceAsset.creatorID;
            this.name = sourceAsset.name;
            this.bucket = sourceAsset.bucket;
            this.key = sourceAsset.key;
            this.assetType = sourceAsset.assetType;
            this.modified = sourceAsset.modified;
            this.created = sourceAsset.created;
            this.pathToData = sourceAsset.pathToData;
            this.originalFileName = sourceAsset.originalFileName;
        }

        private void CopyFromGetResponseData(RestGetAssetResponseData assetData)
        {
            this.id = assetData.id;
            this.name = assetData.name;
            this.ownerID = assetData.owner_id;
            this.bucket = assetData.s3bucket;
            this.key = assetData.key;
            this.assetType = assetData.asset_type;
            this.modified = JSONTools.ParseDateTimeFromString(assetData.modified_at);
            this.created = JSONTools.ParseDateTimeFromString(assetData.created_at);
            this.originalFileName = assetData.orig_file_name;

            if (string.IsNullOrEmpty(this.pathToData))
                this.pathToData = assetData.s3url;
            //created_at = assetData.TryGetValue("created_at", out test) ? JSONTools.ParseDateTimeFromString(test.ToString()) : new System.DateTime(),
            //        modified_at = assetData.TryGetValue("modified_at", out test) ? JSONTools.ParseDateTimeFromString(test.ToString()) : new System.DateTime(),

        }

        public StatusMessage GetStatusMessage()
        {
            Process process = processSet == null || processSet.Count == 0 ? Process.Idle : processSet.Last();

            switch (process)
            {
                case Process.Error:
                    return new StatusMessage()
                    {
                        statusMessage = "Something has gone wrong. :(",
                        progressing = true
                    };
                case Process.Idle:
                    return new StatusMessage()
                    {
                        statusMessage = "",
                        progressing = false
                    };
                case Process.Fetching:
                    return new StatusMessage()
                    {
                        statusMessage = "Getting Asset info",
                        progressing = true
                    };
                case Process.FetchingDataPath:
                    return new StatusMessage()
                    {
                        statusMessage = "Getting Asset data path",
                        progressing = true
                    };
                case Process.FetchingMetadata:
                    return new StatusMessage()
                    {
                        statusMessage = "Getting Asset metadata",
                        progressing = true
                    };
                case Process.FetchingPreviewPath:
                    return new StatusMessage()
                    {
                        statusMessage = "Getting Asset preview path",
                        progressing = true
                    };
                case Process.Updating:
                    return new StatusMessage()
                    {
                        statusMessage = "Updating Asset info",
                        progressing = true
                    };
                case Process.UpdatingMetadata:
                    return new StatusMessage()
                    {
                        statusMessage = "Updating Asset metadata",
                        progressing = true
                    };
                case Process.Downloading:
                    return new StatusMessage()
                    {
                        statusMessage = "Downloading Asset file",
                        progressing = true
                    };
                case Process.Uploading:
                    return new StatusMessage()
                    {
                        statusMessage = "Uploading file for Asset",
                        progressing = true
                    };
                case Process.Deleting:
                    return new StatusMessage()
                    {
                        statusMessage = "Deleting Asset",
                        progressing = true
                    };
                default:
                    return new StatusMessage()
                    {
                        statusMessage = "In Unrecognized Process: " + process.ToString(),
                        progressing = false
                    };
            }
        }
    }
}