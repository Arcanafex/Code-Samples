using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Spaces.UnityClient;
using Spaces.Core.RestAPI;

namespace Spaces.Core
{
    public enum State
    {
        Play,
        Edit
    }

    public struct StatusMessage
    {
        public string statusMessage;
        public bool progressing;
    }

    [System.Serializable]
    public class Space : IProgressive
    {
        public static class SpacesManager
        {
            private static readonly List<Space> s_spaces = new List<Space>();

            //TODO: AddSpace should update member info is already in list.
            public static Space AddSpace(Space space)
            {
                var existingSpace = GetSpace(space.id);

                if (existingSpace == null)
                {
                    existingSpace = space;
                    s_spaces.Add(space);
                }

                return existingSpace;
            }

            public static Space AddSpace(string spaceID)
            {
                var existingSpace = GetSpace(spaceID);

                if (existingSpace == null)
                {
                    existingSpace = new Space(spaceID);
                    s_spaces.Add(existingSpace);
                }

                return existingSpace;
            }

            public static List<Space> GetSpaces()
            {
                if (s_spaces.Count == 0)
                    LoadFromCache();

                if (s_spaces.Count > 1)
                    s_spaces.Sort((s1, s2) => s1.name.CompareTo(s2.name) == 0 ? s1.id.CompareTo(s2.id) : s1.name.CompareTo(s2.name));

                return s_spaces;
            }

            public static List<Space> GetSpaces(string[] spaceIDs)
            {
                var spaceList = new List<Space>();

                foreach (string id in spaceIDs)
                {
                    spaceList.Add(AddSpace(id));
                }
                
                return spaceList;
            }

            public static Space GetSpace(string spaceID)
            {
                return s_spaces.FirstOrDefault(space => space.id == spaceID);
            }

            public static bool RemoveSpace(Space space)
            {
                bool removed = s_spaces.Remove(space);

                foreach (var item in s_spaces.Where(s => s.id == space.id))
                {
                    if (s_spaces.Remove(item))
                        removed = true;
                }

                return removed;
            }

            public static bool RemoveSpace(string spaceID)
            {
                bool removed = false;

                foreach (var space in s_spaces.Where(space => space.id == spaceID))
                {
                    if (s_spaces.Remove(space))
                        removed = true;
                }

                return removed;
            }

            public static void Clear()
            {
                s_spaces.Clear();
            }

            public static void SaveToCache()
            {
                string spacePath = string.Format(@"{0}/SpaceList", Constants.SPACES_CACHE);
                string spaceJSON = JSONTools.LoadToString(s_spaces);

                if (!System.IO.Directory.Exists(Constants.SPACES_CACHE))
                    System.IO.Directory.CreateDirectory(Constants.SPACES_CACHE);

                using (var writer = new System.IO.StreamWriter(spacePath + ".json"))
                {
                    writer.Write(spaceJSON);
                }
            }

            public static void LoadFromCache()
            {
                string spacePath = string.Format(@"{0}/SpaceList.json", Constants.SPACES_CACHE);

                if (System.IO.File.Exists(spacePath))
                {
                    foreach (var space in JSONTools.Load<Space[]>(spacePath))
                    {
                        if (!s_spaces.Contains(space))
                            s_spaces.Add(space);
                    }
                }

            }
        }




        public string id;
        public string creatorID;
        public string ownerID;
        public string name;
        public string basePath;
        public DateTime lastAccessed;
        public DateTime created;
        public DateTime modified;
        public List<string> assetIDs;



        public List<Asset> Assets
        {
            get
            {
                if (isGraphFetched)
                    return Asset.AssetsManager.GetAssets(Graph.assetIDs.ToArray());
                else
                    return Asset.AssetsManager.GetAssets(assetIDs.ToArray());
            }
        }
        
        private Queue<string> addAssetQueue;

        public SpaceGraph Graph { get; set; }
        public bool isGraphFetched { get; private set; }
        

        // [LEGACY?] use of unity scene bundles
        //the scenes present in the space bundle
        #region Scene Management
        private Dictionary<string, Asset> m_scenesDict;
        private List<string> m_scenes;

        public List<string> Scenes
        {
            get
            {
                if (m_scenes == null)
                    UpdateSceneList();

                return m_scenes;
            }
        }

        public void UpdateSceneList()
        {
            if (m_scenes == null)
                m_scenes = new List<string>();
            else
                m_scenes.Clear();

            if (m_scenesDict == null)
                m_scenesDict = new Dictionary<string, Asset>();
            else
                m_scenesDict.Clear();

            if (Assets != null)
            {
                foreach (Asset asset in Assets.Where(a => a.bundle != null))
                {
                    foreach (string scene in asset.bundle.GetAllScenePaths())
                    {
                        m_scenes.Add(System.IO.Path.GetFileNameWithoutExtension(scene));
                        m_scenesDict.Add(System.IO.Path.GetFileNameWithoutExtension(scene), asset);
                    }
                }
            }
        }
        #endregion

        #region State Management
        public State lastState { get; private set; }
        public State state { get; private set; }

        public delegate void StateChange(Space sender, State newState);
        public event StateChange onStateChange;

        public void ChangeState(State newState)
        {
            if (state != newState)
            {
                lastState = state;
                state = newState;

                // Notify all IEditable objects that the Space has entered edit mode.

                if (newState == State.Edit)
                    foreach (var editable in GameObject.FindObjectsOfType<GameObject>().Where(o => o.GetComponent<IEditable>() != null).Select(o => o.GetComponent<IEditable>()))
                    {
                        editable.OnEditStart();
                    }
                else if (lastState == State.Edit)
                    foreach (var editable in GameObject.FindObjectsOfType<GameObject>().Where(o => o.GetComponent<IEditable>() != null).Select(o => o.GetComponent<IEditable>()))
                    {
                        editable.OnEditEnd();
                    }

                if (onStateChange != null)
                    onStateChange(this, newState);
            }
        }
        #endregion

        #region Process Management
        public enum Process
        {
            Error = -1,
            Idle = 0,
            Loading = 1,
            Creating,
            Fetching,
            FetchingAssets,
            FetchingGraph,
            Updating,
            AddingAsset,
            RemovingAsset,
            Saving,
            Deleting
        }

        public bool InProcess(Process process)
        {
            return process == Process.Idle ? processSet.Count == 0 : processSet.Contains(process);
        }

        protected List<Process> processSet;

        public delegate void ProcessBeginning(Space sender, Process[] currentProcesses, Process beginningProcess);
        public delegate void ProcessEnding(Space sender, Process[] currentProcesses, Process endingProcess);
        public delegate void ProcessErroring(Space sender, Process[] currentProcesses, Process erroringProcess);

        public event ProcessBeginning onProcessBegin;
        public event ProcessEnding onProcessEnd;
        public event ProcessErroring onProcessError;

        private bool ProcessStart(Process newProcess)
        {
            Debug.Log(this.name + "(" + this.id + ") [Beginning Process: " + newProcess + "]");

            if (!processSet.Contains(newProcess))
            {
                processSet.Add(newProcess);

                if (onProcessBegin != null)
                    onProcessBegin(this, processSet.ToArray(), newProcess);

                return true;
            }
            else
                return false;
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

        public float GetProgress()
        {
            return progress;
        }

        public string GetProgressMessage()
        {
            return progressMessage;
        }
        #endregion



        public Space()
        {
            Initialize();            
        }

        /// <summary>
        /// Generates an available space based on a Space ID
        /// </summary>
        /// <param name="space">The userspace object</param>
        public Space(string spaceID, string spaceName = "")
        {
            id = spaceID;
            name = spaceName;

            Initialize();
        }

        private void Initialize()
        {
            processSet = new List<Process>();
            assetIDs = new List<string>();
            SpacesManager.AddSpace(this);
        }

        #region Static List Request
        /// <summary>
        /// Request for a list of available Spaces.
        /// </summary>
        /// <param name="doneCallback">Optional callback</param>
        /// <returns>A list of Spaces</returns>
        public static List<Space> GetSpaceList(RestGetSpaceListCallback doneCallback)
        {
            return RestManager.Request.GetSpaceList(doneCallback);
        }

        /// <summary>
        /// Request for a list of available Spaces.
        /// </summary>
        /// <returns>A list of Spaces</returns>
        public static List<Space> GetSpaceList()
        {
            return RestManager.Request.GetSpaceList(OnGetSpaceListResponse);
        }

        private static void OnGetSpaceListResponse(bool error, RestGetSpaceListResponseData responseData)
        {
            if (error)
            {
                Debug.Log("[Error] Retrieving Space list failed.");
            }
            else
            {
                Debug.Log("[Completed Process: Fetch Space List] ");
            }
        }
        #endregion

        #region Creation / Deletion
        /// <summary>
        /// Static method for creating a new Space.
        /// </summary>
        /// <param name="spaceName">The name of the new Space.</param>
        /// <returns></returns>
        public static Space Create(string spaceName)
        {
            var createdSpace = RestManager.Request.CreateSpace(spaceName, OnCreateSpaceResponse);
            createdSpace.ProcessStart(Process.Creating);
            return createdSpace;
        }

        private static void OnCreateSpaceResponse(bool error, CreateSpaceResponseData data)
        {
            if (error)
            {
                Debug.Log("An error occurred while creating new Space");
                data.space.ProcessError(Process.Creating);
            }
            else
            {
                Debug.Log("Space creation was successful [new ID: " + data.id + "]");
                data.space.ProcessEnd(Process.Creating);
                //data.space.Refresh();
            }
        }

        /// <summary>
        /// Static method for deleting a Space.
        /// </summary>
        /// <param name="spaceID">The ID of the Space to be deleted.</param>
        public static void Delete(string spaceID)
        {
            RestManager.Request.DeleteSpace(spaceID, OnDeleteSpaceResponse);
        }

        /// <summary>
        /// Delete this Space.
        /// </summary>
        public void Delete()
        {
            if (ProcessStart(Process.Deleting))
                Delete(id);
        }

        private static void OnDeleteSpaceResponse(bool error, RestDeleteSpaceResponse response)
        {
            if (error)
            {
                response.space.ProcessError(Process.Deleting);
            }
            else
            {
                response.space.ProcessEnd(Process.Deleting);
                SpacesManager.RemoveSpace(response.space);
            }
        }

        #endregion

        #region Fetching Space

        //TODO: Need to mark various components of Space as needing to all be refetched.
        public void Refresh()
        {
            Graph = null;
            isGraphFetched = false;

            ClearFromCache(this);

            FetchSpace();
        }

        private void FetchSpace()
        {
            if (ProcessStart(Process.Fetching))
            {
                if (TryFetchFromCache(id))
                    ProcessEnd(Process.Fetching);
                else
                    RestManager.Request.GetSpace(id, GetSpaceInfoResponse);
            }       
        }

        private void GetSpaceInfoResponse(bool error, RestGetSpaceResponseData spaceData)
        {
            if (error)
            {
                ProcessError(Process.Fetching);
            }
            else
            {
                try
                {
                    ProcessEnd(Process.Fetching);

                    // NOTE: TinyJSON does not serialize System.DateTime values correctly, so this will always return true.
                    if (this.modified < JSONTools.ParseDateTimeFromString(spaceData.modified_at))
                    {
                        CopyFromGetResponseData(spaceData);
                        SaveToCache();
                    }

                    //RegisterAssets();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    ProcessError(Process.Fetching);
                }
            }
        }

        /// <summary>
        /// Start process for getting the SpaceGraph from the server.
        /// </summary>
        public void FetchGraph()
        {
            if (ProcessStart(Process.FetchingGraph))
            {
                if (TryFetchGraphFromCache(id) && !Debug.isDebugBuild)
                    ProcessEnd(Process.FetchingGraph);
                else
                    RestManager.Request.GetSpaceMetadata(id, OnFetchGraphResponse);
            }
        }

        private void OnFetchGraphResponse(bool error, Spaces.Core.RestAPI.RestGetSpaceMetadataResponse metadata)
        {
            if (error)
            {
                ProcessError(Process.FetchingGraph);
            }
            else
            {
                //Graph = JsonUtility.FromJson<SpaceGraph>(metadata.metadata);
                Graph = metadata.spaceGraph;

                if (Graph == null)
                    Graph = new SpaceGraph();

                if (Graph.assetIDs == null)
                    Graph.assetIDs = new List<string>(assetIDs);
                else
                    assetIDs = Graph.assetIDs;

                isGraphFetched = true;
                SaveGraphToCache();
                ProcessEnd(Process.FetchingGraph);
            }
        }

        #endregion

        //NOTE: This region is Obsolete code
        #region Asset Association

        //private void RegisterAssets()
        //{
        //    if (assetIDs != null && assetIDs.Count > 0)
        //    {
        //        foreach (string assetID in assetIDs)
        //        {
        //            if (!string.IsNullOrEmpty(assetID))
        //            {
        //                var asset = Asset.AssetsManager.AddAsset(assetID);

        //                // Not currently being checked.
        //                //asset.onStateEnd += AssetUpdated;
        //            }
        //        }
        //    }
        //}

        //private void AssetUpdated(Core.Asset sender, Core.Asset.Process[] remainingStates, Core.Asset.Process endingState)
        //{
        //}

        public delegate void AssetAdded(Space space, Asset asset);
        public delegate void AssetRemoved(Space space, Asset asset);
        public event AssetAdded onAssetAdded;
        public event AssetRemoved onAssetRemoved;

        /// <summary>
        /// Associate this Asset with this Space.
        /// </summary>
        /// <param name="asset">The Asset to associate</param>
        public void AddAsset(Asset asset)
        {
            AddAsset(asset.id);
        }

        /// <summary>
        /// Associate an Asset by ID with this Space.
        /// </summary>
        /// <param name="assetID">The Asset ID</param>
        public void AddAsset(string assetID)
        {
            if (InProcess(Process.AddingAsset))
            {
                if (addAssetQueue == null)
                    addAssetQueue = new Queue<string>();

                addAssetQueue.Enqueue(assetID);
            }
            else
            {
                if (assetIDs == null)
                    assetIDs = new List<string>();

                if (!assetIDs.Contains(assetID))
                {
                    assetIDs.Add(assetID);

                    if (ProcessStart(Process.AddingAsset))
                        RestManager.Request.AddAssetToSpace(id, assetID, AddAssetToSpaceResponse);
                }
            }
        }

        protected void AddAssetToSpaceResponse(bool error, RestAddAssetToSpaceResponseData data)
        {
            if (error)
            {
                ProcessError(Process.AddingAsset);
            }
            else
            {
                ProcessEnd(Process.AddingAsset);

                if (addAssetQueue != null && addAssetQueue.Count > 0)
                {
                    AddAsset(addAssetQueue.Dequeue());
                }
                else
                {
                    if (onAssetAdded != null)
                        onAssetAdded(this, Asset.AssetsManager.GetAsset(data.assetID));
                }
            }
        }

        /// <summary>
        /// Remove association of this Asset from this Space.
        /// </summary>
        /// <param name="asset">Asset to disasssociate</param>
        public void RemoveAsset(Asset asset)
        {
            RemoveAsset(asset.id);
        }

        /// <summary>
        /// Remove associate of Asset by ID from this Space.
        /// </summary>
        /// <param name="assetID">The Asset ID</param>
        public void RemoveAsset(string assetID)
        {
            if (ProcessStart(Process.RemovingAsset))
            {
                assetIDs.Remove(assetID);
                RestManager.Request.RemoveAssetFromSpace(id, assetID, OnRemoveAssetFromSpaceResponse);
            }
        }

        void OnRemoveAssetFromSpaceResponse(bool error, RestRemoveAssetFromSpaceResponseData data)
        {
            if (error)
            {
                ProcessError(Process.RemovingAsset);
            }
            else
            {
                ProcessEnd(Process.RemovingAsset);

                if (onAssetRemoved != null)
                    onAssetRemoved(this, Asset.AssetsManager.GetAsset(data.assetID));
            }
        }
        #endregion

        #region Modifying Space
        /// <summary>
        /// Update this Space.
        /// </summary>
        /// <param name="spaceName"></param>
        /// <param name="ownerID"></param>
        public void Update(string spaceName, string ownerID)
        {
            if (!string.IsNullOrEmpty(spaceName))
                this.name = spaceName;

            if (!string.IsNullOrEmpty(ownerID))
                this.ownerID = ownerID;

            RestManager.Request.UpdateSpace(this.id, this.name, this.ownerID, OnUpdateResponse);
        }

        private void OnUpdateResponse(bool error, string response)
        {
            if (error)
            {
                ProcessError(Process.Updating);
            }
            else
            {
                ProcessEnd(Process.Updating);
                //WriteToCache();
            }
        }

        #endregion

        #region Entering Space
        public delegate void LoadSpaceBegin(Space space);
        public delegate void LoadSpaceDone(Space space);
        public delegate void LoadSpaceCancel(Space space);
        public event LoadSpaceBegin onLoadSpaceBegin;
        public event LoadSpaceDone onLoadSpaceDone;
        public event LoadSpaceCancel onLoadSpaceCancel;

        public PortalWidget SpawnPortal()
        {
            var portalGO = new GameObject(name);
            var portal = portalGO.AddComponent<PortalWidget>();
            portal.Initialize(this);

            return portal;
        }

        /// <summary>
        /// Load and enter this Space.
        /// </summary>
        public void Enter(Widget handler = null, bool enterOnLoadDone = true)
        {
            if (ProcessStart(Process.Loading))
            {
                if (onLoadSpaceBegin != null)
                    onLoadSpaceBegin(this);

                MonoBehaviour coroutineHandler = handler ? handler : UserSession.Instance as MonoBehaviour;

                //Graph = null;
                //isGraphFetched = false;

                FetchSpace();
                FetchGraph();

                coroutineHandler.StartCoroutine(LoadSpace(coroutineHandler, enterOnLoadDone));
            }
        }

        private IEnumerator LoadSpace(MonoBehaviour handler, bool enterOnDone)
        {
            yield return handler.StartCoroutine(ProcessFetchSpace());
            yield return handler.StartCoroutine(ProcessFetchGraph());

            var assetList = Assets.ToArray();

            if (assetList.Length > 0)
            {
                yield return handler.StartCoroutine(ProcessDownload(assetList));
            }

            if (onLoadSpaceDone != null)
                onLoadSpaceDone(this);

            if (enterOnDone)
            {
                yield return handler.StartCoroutine(ProcessSceneLoad());
            }
        }

        private IEnumerator ProcessFetchSpace()
        {
            Debug.Log(this.name + " (" + this.id + ") [Processing Space Fetch]");

            float timeout = 5;
            float elapsed = 0;

            progressMessage = "Fetching\nSpace";

            // Complete Fetching Space Info
            while (InProcess(Process.Fetching) && !InProcess(Process.Error) && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                var current = Mathf.Clamp(elapsed / timeout, elapsed, timeout);
                progress = current;
                yield return null;
            }

            progress = 1;
        }

        private IEnumerator ProcessDownload(Asset[] assetList)
        {
            Debug.Log(this.name + " (" + this.id + ") [Processing Asset Download]");

            progressMessage = "Downloading";
            int needAssetDownload = 0;

            foreach (var asset in assetList.Where(a => !a.isLoaded))
            {
                needAssetDownload++;
                UserSession.Instance.StartCoroutine(asset.LoadAsset());
            }

            Debug.Log(this.name + " [Assets] " + needAssetDownload);

            while (!assetList.All(a => a.isDone || a.InProcess(Asset.Process.Error)))
            {
                float grossProgress = 0;
                assetList.ToList().ForEach(a => grossProgress += a.isDone ? 1 : a.GetProgress());
                progress = grossProgress / assetList.Length;

                var done = assetList.Count(a => a.isDone);

                Debug.Log(this.name + "[Progress] Gross: " + grossProgress + " Assets.Count: " + assetList.Length + " Progress: " + progress + " Done: " + done);
                yield return null;
            }

            progress = 1;
        }

        private IEnumerator ProcessFetchGraph()
        {
            Debug.Log(this.name + " (" + this.id + ") [Processing Graph Fetch]");

            //Complete Fetching Graph
            progressMessage = "Unpacking";

           float timeout = 10;
           float elapsed = 0;

            while (InProcess(Process.FetchingGraph) && !InProcess(Process.Error) && elapsed < timeout)
            {
                elapsed = Mathf.Clamp(elapsed + Time.deltaTime, 0, timeout);
                progress = elapsed / timeout;
                yield return isGraphFetched;
            }

            progress = 1;
        }

        private IEnumerator ProcessSceneLoad()
        {
            Debug.Log(this.name + " (" + this.id + ") [Processing Scene Load]");

            UserSession.Instance.SetCurrentSpace(this);
            var sceneName = UserSession.Instance.DefaultSpace;
            SceneManager.sceneLoaded += BuildSpace;

            if (Scenes.Count > 0)
            {
                sceneName = Scenes[0];
            }

            var async = SceneManager.LoadSceneAsync(sceneName);
            progressMessage = "Entering\n" + this.name;

            while (!async.isDone)
            {
                progress = async.progress;
                yield return async.isDone;
            }

        }

        private void BuildSpace(Scene scene, LoadSceneMode mode)
        {
            Debug.Log(this.name + " (" + this.id + ") [Building Space]");
            
            SceneManager.sceneLoaded -= BuildSpace;
            SceneManager.sceneUnloaded += UnloadSpace;
            if (Graph != null) Graph.RenderGraph();

            if (!GameObject.FindObjectOfType<LightWidget>())
                UserSession.Instance.GetInstance("Default Light Rig");

            if (!GameObject.FindObjectOfType<AvatarWidget>())
            {
                var ago = UserSession.Instance.GetInstance("Avatar");
                ago.name = ago.name.Replace("(Clone)", "");
                var avatar = ago.GetComponent<AvatarWidget>();
                avatar.Initialize();
            }

            if (!GameObject.FindObjectOfType<LocationWidget>())
            {
                var location = LocationWidget.Create("Start Location", Vector3.zero);
                location.defaultLocation = true;
                location.Initialize();
            }

            ProcessEnd(Process.Loading);
        }

        /// <summary>
        /// Deserialize the SpaceGraph to the layout Space.
        /// </summary>
        /// <param name="autoSpawnAssets"></param>
        //public void RenderGraph(bool autoSpawnAssets = false)
        //{
        //    if (Graph != null && !Graph.isEmpty)
        //    {
        //        foreach (Node node in Graph.nodes)
        //        {
        //            var gameObject = node.RenderToGameObjectHierarchy();
        //            gameObject.GetComponentsInChildren<AssetWidget>().ToList().ForEach(w => w.GraphSetBySpace = true);
        //        }
        //    }
        //    else if (autoSpawnAssets)
        //    {
        //        foreach (var asset in Assets)
        //        {
        //            if (asset.SceneCount < 1)
        //                asset.SpawnAssetInstance();
        //        }
        //    }

        //}


        private void UnloadSpace(Scene scene)
        {
            SceneManager.sceneUnloaded -= UnloadSpace;
            //Assets.ForEach(a => a.UnloadAssetBundle(false));
        }

        //TODO: Generalize this to use an Asset dispose method.
        public void UnloadAssets(bool destroyAllAssetInstances = false)
        {
            foreach (var asset in Assets)
            {
                asset.UnloadAssetBundle(destroyAllAssetInstances);

                Debug.Log(this.name + " [Asset Unloaded] " + asset.name + "(" + asset.id + ")");
            }
        }

        #endregion


        #region Saving Space Graph
        /// <summary>
        /// Write out the SpaceGraph of the current Space.
        /// </summary>
        /// <returns>The resulting SpaceGraph</returns>
        public SpaceGraph Save()
        {
            if (ProcessStart(Process.Saving))
            {
                var graph = Save(id);

                Graph = graph;
                SaveGraphToCache();
            }

            return Graph;
        }

        /// <summary>
        /// Write out the SpaceGraph of a Space, serialize it and update the server.
        /// </summary>
        /// <param name="spaceID">The ID of the Space.</param>
        /// <returns>The resulting SpaceGraph</returns>
        public static SpaceGraph Save(string spaceID)
        {
            var graph = SpaceGraph.Generate();

            // Serialize Graph into json.
            string json = JsonUtility.ToJson(graph);

            if (!string.IsNullOrEmpty(spaceID))
            {
                RestManager.Request.UpdateSpaceMetadata(spaceID, json, OnUpdateMetadataResponse);
            }

            return graph;
        }

        private static void OnUpdateMetadataResponse(bool error, RestUpdateSpaceMetadataResponse response)
        {
            if (error)
            {
                response.space.ProcessError(Process.Saving);
            }
            else
            {
                response.space.ProcessEnd(Process.Saving);
            }
        }
        #endregion

        public static void SaveToCache(Space space)
        {
            string spacePath = string.Format(@"{0}/{1}.json", Constants.SPACES_CACHE, space.id);
            string spaceJSON = JSONTools.LoadToString(space);

            if (!System.IO.Directory.Exists(Constants.SPACES_CACHE))
                System.IO.Directory.CreateDirectory(Constants.SPACES_CACHE);

            using (var writer = new System.IO.StreamWriter(spacePath))
            {
                writer.Write(spaceJSON);
            }
        }

        public void SaveToCache()
        {
            SaveToCache(this);
        }

        public void SaveGraphToCache()
        {
            SaveGraphToCache(this);
        }

        public static void SaveGraphToCache(Space space)
        {
            string graphPath = string.Format(@"{0}/{1}_graph.json", Constants.SPACES_CACHE, space.id);
            string graphJSON = JsonUtility.ToJson(space.Graph);

            if (!System.IO.Directory.Exists(Constants.SPACES_CACHE))
                System.IO.Directory.CreateDirectory(Constants.SPACES_CACHE);

            using (var writer = new System.IO.StreamWriter(graphPath))
            {
                writer.Write(graphJSON);
            }
        }

        public static void ClearFromCache(Space space)
        {
            string spacePath = string.Format(@"{0}/{1}.json", Constants.SPACES_CACHE, space.id);

            if (System.IO.File.Exists(spacePath))
                System.IO.File.Delete(spacePath);

            string graphPath = string.Format(@"{0}/{1}_graph.json", Constants.SPACES_CACHE, space.id);

            if (System.IO.File.Exists(graphPath))
                System.IO.File.Delete(graphPath);
        }



        private bool TryFetchFromCache(string spaceID)
        {
            bool success = false;
            string spacePath = string.Format("{0}/{1}.json", Constants.SPACES_CACHE, spaceID);

            if (System.IO.File.Exists(spacePath))
            {
                try
                {
                    var cachedSpace = JSONTools.Load<Space>(spacePath);
                    Copy(cachedSpace);

                    success = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError(name + " (" + id + ") [Error Loading Space from Cache] " + ex);
                }
            }

            return success;
        }

        private bool TryFetchGraphFromCache(string spaceID)
        {
            bool success = false;
            string graphPath = string.Format(@"{0}/{1}_graph.json", Constants.SPACES_CACHE, spaceID);

            if (System.IO.File.Exists(graphPath))
            {
                string cachedGraphJson = System.IO.File.ReadAllText(graphPath);

                try
                {
                    var cachedGraph = JsonUtility.FromJson<SpaceGraph>(cachedGraphJson);

                    if (cachedGraph != null)
                    {
                        Graph = cachedGraph;
                    }

                    if (Graph.assetIDs == null)
                        Graph.assetIDs = new List<string>(assetIDs);
                    else
                        assetIDs = Graph.assetIDs;

                    isGraphFetched = true;
                    success = true;                    
                }
                catch (Exception ex)
                {
                    Debug.LogError(name + " (" + id + ") [Error Loading Graph from Cache] " + ex);
                }
            }


            return success;
        }


        private void Copy(Space sourceSpace)
        {
            this.id = sourceSpace.id;
            this.creatorID = sourceSpace.creatorID;
            this.ownerID = sourceSpace.ownerID;
            this.name = sourceSpace.name;
            this.basePath = sourceSpace.basePath;
            this.lastAccessed = sourceSpace.lastAccessed;
            this.created = sourceSpace.created;
            this.modified = sourceSpace.modified;
            this.assetIDs = sourceSpace.assetIDs;
        }

        private void CopyFromGetResponseData(RestGetSpaceResponseData responseData)
        {
            this.id = responseData.id;
            this.creatorID = responseData.creator_id;
            this.ownerID = responseData.owner_id;
            this.name = responseData.name;
            this.basePath = responseData.base_path;
            this.lastAccessed = JSONTools.ParseDateTimeFromString(responseData.last_accessed_at);
            this.created = JSONTools.ParseDateTimeFromString(responseData.created_at);
            this.modified = JSONTools.ParseDateTimeFromString(responseData.modified_at);
            this.assetIDs = new List<string>(responseData.asset_ids);
        }

        public StatusMessage GetStatusMessage()
        {
            Process process = processSet.Count == 0 ? Process.Idle : processSet.Last();

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
                case Process.Loading:
                    return new StatusMessage()
                    {
                        statusMessage = "Loading Space " + this.name,
                        progressing = true
                    };
                case Process.Fetching:
                    return new StatusMessage()
                    {
                        statusMessage = "Getting Space info",
                        progressing = true
                    };
                case Process.FetchingAssets:
                    return new StatusMessage()
                    {
                        statusMessage = "Getting Asset info",
                        progressing = true
                    };
                case Process.FetchingGraph:
                    return new StatusMessage()
                    {
                        statusMessage = "Getting Space Graph",
                        progressing = true
                    };
                case Process.Updating:
                    return new StatusMessage()
                    {
                        statusMessage = "Updating Space info",
                        progressing = true
                    };
                case Process.AddingAsset:
                    return new StatusMessage()
                    {
                        statusMessage = "Adding Asset to Space",
                        progressing = true
                    };
                case Process.RemovingAsset:
                    return new StatusMessage()
                    {
                        statusMessage = "Removing Asset from Space",
                        progressing = true
                    };
                case Process.Saving:
                    return new StatusMessage()
                    {
                        statusMessage = "Saving Space",
                        progressing = true
                    };
                case Process.Deleting:
                    return new StatusMessage()
                    {
                        statusMessage = "Deleting Space",
                        progressing = true
                    };
                default:
                    return new StatusMessage();
            }
        }
    }

}