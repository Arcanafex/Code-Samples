using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spaces.Core;
using Spaces.Core.RestAPI;

namespace Spaces.UnityClient
{
    public class UserSession : MonoBehaviour
    {
        private static UserSession instance;
        public static UserSession Instance
        {
            get
            {
                if (!instance)
                    instance = FindObjectOfType<UserSession>();

                if (!instance)
                {
                    instance = CreateSession(Resources.Load<SessionSettings>(Constants.SESSION_SETTINGS));
                }

                return instance;
            }
        }

        private static UserSession CreateSession(SessionSettings settings)
        {
            var session = new GameObject("UserSession").AddComponent<UserSession>();
            session.m_settings = settings;// ? settings : Resources.Load("_Spaces SDK/Examples/Prefabs/Settings/_DefaultSessionSettings.asset") as SessionSettings;//.FindObjectsOfTypeAll<SessionSettings>().FirstOrDefault();

            return session;
        }

        public SessionSettings m_settings;
        public bool debugClearStoredDataOnStart { get { return m_settings ? m_settings.debugClearStoredDataOnStart : false; } }

        #region prefabRefs
        public GameObject userLoginPrefab { get { return m_settings ? m_settings.GetPrefab("LoginUI") : null; } }
        public GameObject navInterfacePrefab { get { return m_settings ? m_settings.GetPrefab("SpaceList") : null; } }
        public GameObject assetInterfacePrefab { get { return m_settings ? m_settings.GetPrefab("AssetList") : null; } }
        public GameObject spacesEventSystemPrefab { get { return m_settings ? m_settings.GetPrefab("EventSystem") : null; } }
        public GameObject avatarPrefab { get { return m_settings ? m_settings.GetPrefab("Avatar") : null; } }
        public GameObject progressMeterPrefab { get { return m_settings ? m_settings.GetPrefab("ProgressMeter") : null; } }
        public GameObject modelPlaceholderPrefab { get { return m_settings ? m_settings.GetPrefab("ModelPlaceholder") : null; } }
        #endregion

        //private ProgressMeterWidget progressMeter;
        private GameObject navUI;
        private float uiOffset
        {
            get
            {
                return m_settings ? m_settings.UIDistance : 5;
            }
        }

        public bool MenuOpen
        {
            get { return navUI; }
        }

        // Do the stuff to add missing parts to a Space upon entry.
        // public bool doSpaceSetup { get { return m_settings ? m_settings.doSpaceSetup : false; } }

        // public bool autoSpawnAssetsInEmptySpace { get { return m_settings ? m_settings.autoSpawnAssetsInEmptySpace : false; } }
        public string LobbySpace { get { return m_settings ? m_settings.LobbySpace : null; } }
        public string DefaultSpace { get { return m_settings ? m_settings.DefaultSpace : null; } }

        //private AvatarWidget m_avatar;
        //public AvatarWidget Avatar
        //{
        //    get
        //    {
        //        return m_avatar;
        //    }
        //    set
        //    {
        //        m_avatar = value;
        //    }
        //}
        
        private Spaces.Core.User m_currentUser;
        public Spaces.Core.User CurrentUser
        {
            get { return m_currentUser; }
        }

        private Spaces.Core.Space m_currentSpace;
        public Core.Space CurrentSpace
        {
            get { return m_currentSpace; }
        }

        private Spaces.Core.Space m_lastSpace;
        public Core.Space LastSpace
        {
            get { return m_lastSpace; }
        }

        private List<Spaces.Core.User> users;
        public List<Spaces.Core.User> Users
        {
            get { return users; }
        }

        public UnityEvent OnUserChanged;
        public UnityEvent OnAvailableSpacesUpdated;
        public UnityEvent OnAvailableAssetsUpdated;
        public UnityEvent OnChangingSpace;


        void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            if (OnAvailableSpacesUpdated == null)
                OnAvailableSpacesUpdated = new UnityEvent();

            if (OnChangingSpace == null)
                OnChangingSpace = new UnityEvent();
        }

        void Start()
        {
            if (!m_settings)
            {
                var settings = Resources.FindObjectsOfTypeAll<SessionSettings>();
                m_settings = settings.FirstOrDefault();
            }

            m_settings.InitializePlatformServiceSettings();
            RestManager.PlatformSettings.LoadSavedSettings();
        }

        //#region HACK_CODE
        ////<HACK>

        //void OnEnable()
        //{
        //    SceneManager.sceneLoaded += SpaceLoadHacks;
        //    SceneManager.sceneUnloaded += SpaceUnLoadHacks;
        //}

        //void OnDisable()
        //{
        //    SceneManager.sceneLoaded -= SpaceLoadHacks;
        //    SceneManager.sceneUnloaded -= SpaceUnLoadHacks;
        //}
        
        //void SpaceLoadHacks(Scene scene, LoadSceneMode mode)
        //{
        //    Debug.LogWarning("WELCOME TO " + scene.name + "!!!");

        //    if (doSpaceSetup)
        //    {
        //        RefreshEventSystem();

        //        FindObjectsOfType<Camera>().Where(c => c.tag == "MainCamera" && !c.GetComponentInParent<AvatarWidget>()).ToList()
        //            .ForEach(cam => cam.transform.root.gameObject.SetActive(false));

        //        AvatarWidget avatarWidget = FindObjectOfType<AvatarWidget>();

        //        // Create Avatar if none exists
        //        if (!avatarWidget)
        //        {
        //            GameObject ago = Instantiate(avatarPrefab);
        //            avatarWidget = ago.GetComponent<AvatarWidget>();
        //        }

        //        // Turn off any extra AudioListeners.
        //        foreach (AudioListener listener in FindObjectsOfType<AudioListener>())
        //        {
        //            if (listener.transform.root != avatarWidget.transform)
        //                listener.enabled = false;
        //        }
        //    }

        //}

        //void SpaceUnLoadHacks(Scene scene)
        //{
        //    Debug.LogWarning("SO LONG, " + scene.name + "!!!");
        //}

        //void RefreshEventSystem()
        //{
        //    EventSystem eventSystem = FindObjectOfType<EventSystem>();
        //    BaseInputModule inputModule = null;

        //    // Make sure there's an EventSystem set up.
        //    if (!eventSystem)
        //    {
        //        GameObject ego = Instantiate(spacesEventSystemPrefab);
        //        ego.name = "EventSystem";
        //        eventSystem = ego.GetComponent<EventSystem>();
        //    }
        //    else
        //    {
        //        foreach (BaseInputModule module in eventSystem.gameObject.GetComponents<BaseInputModule>())
        //            module.enabled = false;
        //    }

        //    if (!inputModule)
        //    {
        //        inputModule = eventSystem.GetComponent<SpacesInputModule>() ? eventSystem.GetComponent<SpacesInputModule>() : eventSystem.gameObject.AddComponent<SpacesInputModule>();
        //        inputModule.enabled = true;
        //    }

        //}

        //// </HACK>
        //#endregion

        #region User Interfaces
        
        public GameObject GetPrefab(string prefabName)
        {
            return m_settings ? m_settings.GetPrefab(prefabName) : null;
        }

        public GameObject GetInstance(string prefabName, Vector3 position, Vector3 rotation, Transform parent = null)
        {
            if (string.IsNullOrEmpty(prefabName))
                return null;

            var prefab = GetPrefab(prefabName);

            if (m_settings)
            {
                if (parent)
                    return Instantiate<GameObject>(prefab, position, Quaternion.Euler(rotation), parent);
                else
                    return Instantiate<GameObject>(prefab, position, Quaternion.Euler(rotation));
            }
            return
                null;
        }

        public GameObject GetInstance(string prefabName, Transform parent = null)
        {
            if (string.IsNullOrEmpty(prefabName))
                return null;

            var prefab = GetPrefab(prefabName);

            if (m_settings && prefab)
            {
                if (parent)
                    return Instantiate(prefab, parent, false);
                else
                    return Instantiate(prefab);
            }
            return null;
        }


        public void OpenLoginInterface()
        {
            OpenLoginInterface(null);
        }

        public void OpenLoginInterface(Transform requester)
        {
            if (navUI)
            {
                CloseNavInterface();
                return;
            }

            if (!requester)
            {
                if (AvatarWidget.UserAvatar)
                {
                    if (AvatarWidget.UserAvatar.head)
                        requester = AvatarWidget.UserAvatar.head;
                    else
                        requester = AvatarWidget.UserAvatar.transform;
                }
                else
                    requester = GameObject.FindObjectOfType<Camera>().transform;
            }

            Vector3 uiPos = requester ? requester.position + (Vector3.Scale(requester.forward, new Vector3(1, 0, 1)).normalized * uiOffset) : Vector3.up * 2;
            Quaternion uiRot = Quaternion.LookRotation(uiPos - requester.position, Vector3.up);

            if (userLoginPrefab)
            {
                navUI = GetInstance("LoginUI", uiPos, Vector3.up * uiRot.eulerAngles.y, this.transform);
                navUI.GetComponent<PopupWidget>().Show();
            }
        }

        public void OpenNavInterface()
        {
            OpenNavInterface(null);
        }

        public void OpenNavInterface(Transform requester)
        {
            if (navUI)
            {
                CloseNavInterface();
                return;
            }

            if (!requester)
            {
                if (AvatarWidget.UserAvatar)
                {
                    if (AvatarWidget.UserAvatar.head)
                        requester = AvatarWidget.UserAvatar.head;
                    else
                        requester = AvatarWidget.UserAvatar.transform;
                }
                else
                    requester = GameObject.FindObjectOfType<Camera>().transform;
            }

            Vector3 uiPos = requester ? requester.position + (Vector3.Scale(requester.forward, new Vector3(1, 0, 1)).normalized * uiOffset) : Vector3.up * 2;
            Quaternion uiRot = Quaternion.LookRotation(uiPos - requester.position, Vector3.up);

            uiRot = Quaternion.Euler(0, uiRot.eulerAngles.y, 0);

            //Debug.Log("uiPos: " + uiPos + " uiRot: " + uiRot.eulerAngles);

            if (navInterfacePrefab)
            {
                navUI = GetInstance("SpaceList", uiPos, Vector3.up * uiRot.eulerAngles.y, this.transform);
            }
        }

        public void OpenAssetInterface()
        {
            OpenAssetInterface(null);
        }

        public void OpenAssetInterface(Transform requester)
        {
            if (navUI)
            {
                CloseNavInterface();
                return;
            }

            if (!requester)
            {
                if (AvatarWidget.UserAvatar)
                {
                    if (AvatarWidget.UserAvatar.head)
                        requester = AvatarWidget.UserAvatar.head;
                    else
                        requester = AvatarWidget.UserAvatar.transform;
                }
                else
                    requester = GameObject.FindObjectOfType<Camera>().transform;
            }

            Vector3 uiPos = requester ? requester.position + (Vector3.Scale(requester.forward, new Vector3(1, 0, 1)).normalized * uiOffset) : Vector3.up * 2;
            Quaternion uiRot = Quaternion.LookRotation(uiPos - requester.position, Vector3.up);

            if (assetInterfacePrefab)
            {
                navUI = GetInstance("AssetList", uiPos, Vector3.up * uiRot.eulerAngles.y, this.transform);
            }
        }

        public void CloseNavInterface()
        {
            if (navUI)
                Destroy(navUI);
        }

        ////TODO: pin position or parent to requester.
        //public void OpenProgressMeter(Transform requester = null)
        //{
        //    if (navUI)
        //        CloseNavInterface();

        //    if (requester == null)
        //    {
        //        AvatarWidget playerAvatar = FindObjectsOfType<AvatarWidget>().FirstOrDefault(avatar => avatar.gameObject.activeInHierarchy);

        //        if (playerAvatar)
        //            requester = playerAvatar.head;
        //        else
        //            requester = Camera.main.transform;
        //    }

        //    Vector3 uiPos = requester.position + (Vector3.Scale(requester.forward, new Vector3(1, 0, 1)).normalized * uiOffset);
        //    Quaternion uiRot = Quaternion.LookRotation(requester.position - uiPos, Vector3.up);
        //    uiRot = Quaternion.Euler(0, uiRot.eulerAngles.y, 0);

        //    if (!progressMeter && progressMeterPrefab)
        //    {
        //        var pmgo = Instantiate(progressMeterPrefab, uiPos, uiRot) as GameObject;

        //        if (pmgo)
        //            progressMeter = pmgo.GetComponent<ProgressMeterWidget>();

        //        pmgo.transform.SetParent(requester.root);
        //    }
        //}

        //public void CloseProgressMeter()
        //{
        //    if (progressMeter)
        //        Destroy(progressMeter.gameObject);
        //}

        //public void SetProgressMeterText(string text)
        //{
        //    if (progressMeter)
        //        progressMeter.Text = text;
        //}

        //public void SetProgressMeterValue(float value)
        //{
        //    if (progressMeter)
        //        progressMeter.Value = value;
        //}

        #endregion

        #region UserAuthentication

        public void Login(string userName, string password)
        {
            m_currentUser = new User(userName);
            CurrentUser.FetchInfo();
            OnUserChanged.Invoke();

            RestManager.Request.Login(CurrentUser.name, password, OnLoginResponseReceived);
        }

        private void OnLoginResponseReceived(bool error, RestLoginResponseData login)
        {
            if (error)
            {
                Debug.LogError(this.name + " [Login] User Authentication failed.");
            }
            else
            {
                Debug.Log(this.name + " [Login] User Authenticated.");
                CurrentUser.SetToken(login.token);
                RestManager.PlatformSettings.Token = CurrentUser.token;
            }
        }

        public void Logout()
        {
            m_settings.InitializePlatformServiceSettings();
            RestManager.PlatformSettings.SaveSettings();

            RestManager.Request.Logout(LogoutRequestResponse);
            OnUserChanged.Invoke();
        }

        private void LogoutRequestResponse(bool error, RestLogoutResponseData response)
        {
            if (error)
            {
                Debug.Log("[Logout] Request failed.");
            }
            else
            {
                Debug.Log("[Logout] Request completed successfully.");
                CurrentUser.SetToken(null);
                m_currentUser = null;
            }
        }

        #endregion

        public void GoToLobby()
        {
            SetCurrentSpace(null);
            SceneManager.LoadScene(LobbySpace);
        }

        public void SetCurrentSpace(Spaces.Core.Space space)
        {
            if (m_currentSpace != space)
            {
                m_lastSpace = m_currentSpace;
                m_currentSpace = space;

                //if (m_lastSpace != null)
                //    m_lastSpace.UnloadAssets();
            }
        }

        public void SaveCurrentSpace()
        {
            if (CurrentSpace != null)
            {
                var graph = CurrentSpace.Save();

                var timer = TimerWidget.CreateTimer(2, true, true);
                timer.DescriptiveName = "Saving...";
                timer.transform.SetParent(AvatarWidget.UserAvatar.transform);//Avatar.transform);
                timer.OnEnd.AddListener(delegate { Destroy(timer.gameObject); });
            }
        }

        public State GetCurrentSpaceState()
        {
            return CurrentSpace == null ? State.Play : CurrentSpace.state;
        }

        public void EditSpace()
        {
            if (CurrentSpace != null)
            {
                CurrentSpace.ChangeState(State.Edit);
            }
        }

        public void PlaySpace()
        {
            if (CurrentSpace != null)
            {
                CurrentSpace.ChangeState(State.Play);
            }
        }
    }
}