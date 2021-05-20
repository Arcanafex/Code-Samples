using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Spaces.LBE;

public class Observer : MonoBehaviour
{
    [System.Serializable]
    public class ObserverUI
    {
        public Canvas canvas;
        public UnityEngine.UI.Text displayName;
        public UnityEngine.UI.Text PlayerFPSDisplay;
        public UnityEngine.UI.Slider fovSlider;
        public RectTransform m_uiPanel;
    }

    public ObserverUI m_UI;

    private Rect m_rect;
    private bool initialized;

    public ObserverCameraMount CurrentPosition { get; private set; }

    public int MountIndex
    {
        get
        {
            if (ObserverManager.Instance && ObserverManager.Instance.cameraMountList != null)
                return ObserverManager.Instance.cameraMountList.IndexOf(CurrentPosition);
            else
                return -1;
        }
    }


    private List<Renderer> IgnoredRenderers
    {
        get
        {
            if (CurrentPosition != null)
            {
                return CurrentPosition.IgnoredRenderers;
            }
            else
            {
                return null;
            }
        }
    }

    private List<Renderer> renderersHidden;

    #region Static Properties
    public static System.Action OnObserverDestroyed;

    #endregion


    // dad-cam check variables
    private const string dadCamDefaultName = "OptiTrack_RB_DadCamVirtuCamera";
    private const string flyCamDefaultName = "FlyCam";
    private bool isDadCam = false;
    private float flyCamSpeed = 5.0f; //regular speed
    private bool isFlyCam = false;
    private UnityEngine.PostProcessing.DepthOfFieldModel.Settings m_DefaultDOFSettings;
    private float m_DefaultFOV;
    private Player mLocalPlayerRef;

    private Camera m_Camera;
    public Camera Cam
    {
        get
        {
            if (!m_Camera)
            {
                m_Camera = GetComponentInChildren<Camera>();
            }

            return m_Camera;
        }
    }

    private Spaces.LBE.MachineConfigurationManager.MachineRole m_MachineRole;
    private UnityEngine.PostProcessing.PostProcessingBehaviour m_PostProcBehavior;

    public bool IsFullscreen
    {
        get
        {
            return Cam.rect.size == Vector2.one;
        }
    }

    private void Awake()
    {
        renderersHidden = new List<Renderer>();
        m_PostProcBehavior = GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>();
        m_DefaultDOFSettings = m_PostProcBehavior.profile.depthOfField.settings;
        m_DefaultFOV = Cam.fieldOfView;
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (!initialized)
            Initialize();
    }

    private void Update() {
        if (SpacesGameManager.Instance && !SpacesGameManager.Instance.isServer && !mLocalPlayerRef) {
            mLocalPlayerRef = SpacesNetworkManager.Instance.GetLocalPlayerRef();
        }
        if (isDadCam || isFlyCam) {
            if (UnityEngine.Input.GetKey(KeyCode.Q)) {
                if (m_Camera.fieldOfView > 1) {
                    m_Camera.fieldOfView--;
                    if (mLocalPlayerRef) mLocalPlayerRef.SendDadCamFOV(m_Camera.fieldOfView);
                }
            }

            if (UnityEngine.Input.GetKey(KeyCode.E)) {
                if (m_Camera.fieldOfView < 116) {
                    m_Camera.fieldOfView++;
                    if (mLocalPlayerRef) mLocalPlayerRef.SendDadCamFOV(m_Camera.fieldOfView);
                }
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.F) && m_PostProcBehavior) {
                m_PostProcBehavior.profile.depthOfField.enabled = !m_PostProcBehavior.profile.depthOfField.enabled;
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.R)) {
                m_PostProcBehavior.profile.depthOfField.settings = m_DefaultDOFSettings;
                m_Camera.fieldOfView = m_DefaultFOV;
                if (mLocalPlayerRef) mLocalPlayerRef.SendDadCamFOV(m_Camera.fieldOfView);
            }

            if (UnityEngine.Input.GetKey(KeyCode.U)) {
                var settings = m_PostProcBehavior.profile.depthOfField.settings;
                if (settings.focalLength <= 300) settings.focalLength++;
                m_PostProcBehavior.profile.depthOfField.settings = settings;
            }
            if (UnityEngine.Input.GetKey(KeyCode.J)) {
                var settings = m_PostProcBehavior.profile.depthOfField.settings;
                if (settings.focalLength > 1) settings.focalLength--;
                m_PostProcBehavior.profile.depthOfField.settings = settings;
            }
            if (UnityEngine.Input.GetKey(KeyCode.I)) {
                var settings = m_PostProcBehavior.profile.depthOfField.settings;
                settings.focusDistance+=0.2f;
                m_PostProcBehavior.profile.depthOfField.settings = settings;
            }
            if (UnityEngine.Input.GetKey(KeyCode.K)) {
                var settings = m_PostProcBehavior.profile.depthOfField.settings;
                settings.focusDistance-=0.2f;
                if (settings.focusDistance < 0.1f) settings.focusDistance = 0.1f;
                m_PostProcBehavior.profile.depthOfField.settings = settings;
            }
            if (UnityEngine.Input.GetKey(KeyCode.O)) {
                var settings = m_PostProcBehavior.profile.depthOfField.settings;
                settings.aperture += 0.2f;
                if (settings.aperture > 32) settings.aperture = 32;
                m_PostProcBehavior.profile.depthOfField.settings = settings;
            }
            if (UnityEngine.Input.GetKey(KeyCode.L)) {
                var settings = m_PostProcBehavior.profile.depthOfField.settings;
                settings.aperture -= 0.2f;
                if (settings.aperture < 0.05f) settings.aperture = 0.05f;
                m_PostProcBehavior.profile.depthOfField.settings = settings;
            }

            float fovVal = UnityEngine.Input.GetAxis("FOV");
            if (fovVal < 0 && m_Camera.fieldOfView > 1)
                m_Camera.fieldOfView--;
            if (fovVal > 0 && m_Camera.fieldOfView < 116)
                m_Camera.fieldOfView++;
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.P) && m_PostProcBehavior) {
            m_PostProcBehavior.enabled = !m_PostProcBehavior.enabled;
        }

        if (isFlyCam) {
            float craneVal = UnityEngine.Input.GetAxis("Crane");
            m_Camera.transform.position += new Vector3(0, Time.deltaTime * flyCamSpeed * craneVal, 0);

            if (UnityEngine.Input.GetKeyDown(KeyCode.Joystick1Button4))
                m_Camera.transform.eulerAngles += new Vector3(0, 0, -10.0f);
            if (UnityEngine.Input.GetKeyDown(KeyCode.Joystick1Button5))
                m_Camera.transform.eulerAngles += new Vector3(0, 0, 10.0f);

            m_Camera.transform.eulerAngles = m_Camera.transform.eulerAngles + (new Vector3(-UnityEngine.Input.GetAxis("Mouse Y"), UnityEngine.Input.GetAxis("Mouse X"), 0) * (flyCamSpeed * Time.deltaTime * 40.0f));

            Vector3 p = new Vector3(UnityEngine.Input.GetAxis("Horizontal"), 0, UnityEngine.Input.GetAxis("Vertical")) * Time.deltaTime * flyCamSpeed;
            m_Camera.transform.Translate(p);
        }
    }

    private void OnPreCull()
    {
        if (renderersHidden != null)
        {
            renderersHidden.Clear();
        }
        else
        {
            renderersHidden = new List<Renderer>();
            DebugLog.Log("AI", "new renderersHidden" + System.Environment.StackTrace);
        }

        if (IgnoredRenderers != null)
        {
            foreach (var obj in IgnoredRenderers)
            {
                if (obj && obj.enabled)
                {
                    renderersHidden.Add(obj);
                    obj.enabled = false;
                }
                else
                {
                    DebugLog.Log("AI", "obj is null" + System.Environment.StackTrace);
                }
            }
        }
        //else
        //{
        //    DebugLog.Log("AI", "IgnoredRenderers is null" + System.Environment.StackTrace);
        //}

    }

    private void OnPostRender()
    {
        foreach (var obj in renderersHidden)
        {
            obj.enabled = true;
        }
    }

    private void OnDestroy()
    {
        if (m_UI.canvas)
            Destroy(m_UI.canvas.gameObject);

        if (OnObserverDestroyed != null)
            OnObserverDestroyed();
    }

    public void Initialize(int startIndex = 0)
    {
        if (!m_Camera)
        {
            m_Camera = GetComponent<Camera>();
        }

        if (m_Camera)
        {
            SetCamRect(m_Camera.rect);
        }
        else
        {
            Debug.LogError("[Observer has no camera] Destroying " + this.name);
            Destroy(gameObject);
        }

        if (!m_UI.canvas)
        {
            m_UI.canvas = GetComponentInChildren<Canvas>();
        }

        if (m_UI.canvas)
        {
            m_UI.canvas.name = this.name + "_" + m_UI.canvas.name;

            PersistentObjectManager.Add(m_UI.canvas.gameObject);

            if (!m_UI.m_uiPanel)
            {
                var canvasChild = m_UI.canvas.transform.GetChild(0);

                if (canvasChild)
                {
                    m_UI.m_uiPanel = canvasChild.GetComponent<RectTransform>();
                }
            }
        }

        // Disable any camera postprocessing effects for Servers.
        m_MachineRole = MachineConfigurationManager.instance.GetMachineRole();

        if (m_MachineRole == MachineConfigurationManager.MachineRole.Server)
        {
            var postProcBehavior = Cam.GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>();

            if (postProcBehavior)
            {
                postProcBehavior.enabled = false;
            }
        }

        if (ObserverManager.Instance.cameraMountList.Count > 0)
        {
            int startingMountIndex = Mathf.Clamp(startIndex, 0, ObserverManager.Instance.cameraMountList.Count - 1);
            RepositionCamera(ObserverManager.Instance.cameraMountList[startingMountIndex]);
        }

        initialized = true;
    }

    public void NextPosition()
    {
        if (ObserverManager.Instance.cameraMountList == null)
            ObserverManager.Instance.UpdateMountList();

        if (ObserverManager.Instance.cameraMountList.Count > 0)
        {
            int newIndex = (MountIndex + 1) % ObserverManager.Instance.cameraMountList.Count;
            RepositionCamera(ObserverManager.Instance.cameraMountList[newIndex]);
        }
        else
        {
            Debug.LogWarning(this.name + " [No Observer Mounts in Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "]");
        }
    }

    public void PreviousPosition()
    {
        if (ObserverManager.Instance.cameraMountList == null)
            ObserverManager.Instance.UpdateMountList();

        if (ObserverManager.Instance.cameraMountList.Count > 0)
        {
            int newIndex = MountIndex > 0 ? MountIndex - 1 : ObserverManager.Instance.cameraMountList.Count - 1;
            RepositionCamera(ObserverManager.Instance.cameraMountList[newIndex]);
        }
        else
        {
            Debug.LogWarning(this.name + " [No Observer Mounts in Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "]");
        }
    }

    private void RepositionCamera(ObserverCameraMount newPosition)
    {
        if (newPosition != null)
        {
            transform.SetParent(newPosition.transform, false);

            if (m_UI.displayName)
            {
                m_UI.displayName.text = newPosition.name;
            }

            switch (newPosition.PositionType)
            {
                case ObserverCameraMount.Type.Default:
                    Cam.nearClipPlane = 0.1f;
                    break;
                case ObserverCameraMount.Type.HandHeld:
                    Cam.nearClipPlane = 0.02f;
                    isDadCam = true;
                    break;
                case ObserverCameraMount.Type.Drone:
                    Cam.nearClipPlane = 0.1f;
                    isFlyCam = true;
                    break;
            }

            if (newPosition.Orthographic)
            {
                Cam.orthographic = true;
                Cam.orthographicSize = newPosition.FovOrSize;

                if (m_UI.fovSlider)
                {
                    m_UI.fovSlider.interactable = false;
                }
            }
            else
            {
                Cam.orthographic = false;
                Cam.fieldOfView = newPosition.FovOrSize;

                if (m_UI.fovSlider)
                {
                    m_UI.fovSlider.interactable = true;
                    m_UI.fovSlider.value = newPosition.FovOrSize;
                }
            }

            CurrentPosition = newPosition;
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        //RepositionCamera(CurrentPosition);
    }

    public void ToggleControlVisibility()
    {
        if (m_UI.canvas)
            ToggleControlVisibility(!m_UI.canvas.enabled);
    }

    public void ToggleControlVisibility(bool visible)
    {
        if (!initialized)
        {
            Initialize();
        }

        if (m_UI.canvas)
        {
            m_UI.canvas.enabled = visible;
        }
    }

    public void SetCamRect(Rect rect)
    {
        m_rect = rect;
    }

    public void ToggleFullScreen()
    {
        ToggleFullScreen(!IsFullscreen);
    }

    public void ToggleFullScreen(bool fullscreen)
    {
        if (fullscreen)
        {
            Cam.rect = new Rect(Vector2.zero, Vector2.one);
            Cam.depth = 10;
        }
        else
        {
            Cam.rect = m_rect;
            Cam.depth = 0;
        }

        m_UI.m_uiPanel.anchorMin = Cam.rect.min;
        m_UI.m_uiPanel.anchorMax = Cam.rect.max;
    }
}
