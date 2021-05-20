using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObserverManager : Singleton<ObserverManager>
{
    [System.Serializable]
    public class ObserverDefinition
    {
        public string name;
        public Observer prefab;
    }

    public ObserverDefinition[] m_ObserverDefinitions;
    public Observer[] m_Observers { get; private set; }
    public int fullScreenIndex = -1;
    public Rect lastRect;

    public List<ObserverCameraMount> cameraMountList;

    private const float listUpdateFreq = 5;
    private float lastListUpdate;

    private bool isUIVisible;

    public static event System.Action OnToggleObserverUI;
    public static event System.Action<int> OnToggleObserverFullscreen;

    private readonly KeyCode[] numCode =
    {
         KeyCode.Alpha1,
         KeyCode.Alpha2,
         KeyCode.Alpha3,
         KeyCode.Alpha4,
         KeyCode.Alpha5,
         KeyCode.Alpha6,
         KeyCode.Alpha7,
         KeyCode.Alpha8,
         KeyCode.Alpha9,
    };

    private void Awake()
    {
        if (Instance != this)
        {
            Debug.LogWarning("There's already an ObserverManager instance");
            gameObject.SetActive(false);
            //Destroy(gameObject);
        }

        m_Observers = new Observer[m_ObserverDefinitions.Length];
        Debug.LogWarning("UI Hidden: " + PlayerPrefs.HasKey("OberverUIHidden"));
        isUIVisible = !PlayerPrefs.HasKey("OberverUIHidden");

        PersistentObjectManager.Add(gameObject);
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        Observer.OnObserverDestroyed += UpdateObservers;
        ObserverCameraMount.OnMountAdded += UpdateMountList;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        Observer.OnObserverDestroyed += UpdateObservers;
        ObserverCameraMount.OnMountAdded -= UpdateMountList;
    }

    private void Start()
    {
        UpdateMountList();
        UpdateObservers();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (m_Observers != null && m_Observers.Length > 0)
        {
            for (int i = 0; i < m_Observers.Length; i++)
            {
                if (m_Observers[i] != null)
                    Destroy(m_Observers[i].gameObject);
            }
        }
    }

    private void Update()
    {
        if (Time.time - lastListUpdate > listUpdateFreq)
        {
            lastListUpdate = Time.time;
            UpdateMountList();
            UpdateObservers();
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        UpdateMountList();
        UpdateObservers();
    }

    public void UpdateObservers()
    {
        for (int i = 0; i < m_Observers.Length; i++)
        {
            if (!m_Observers[i])
            {
                if (m_ObserverDefinitions[i].prefab)
                {
                    var observerInstance = Instantiate(m_ObserverDefinitions[i].prefab.gameObject, transform);

                    m_Observers[i] = observerInstance.GetComponent<Observer>();
                    m_Observers[i].Initialize(i);
                    m_Observers[i].ToggleControlVisibility(isUIVisible);
                }
                else
                {
                    Debug.LogError(this.name + " [No Observer Prefab Defined] def " + i + " - " +  m_ObserverDefinitions[i].name);
                }
            }
        }
    }

    public void UpdateMountList()
    {
        ObserverCameraMount.GetObserverCameraPositions(ref cameraMountList);
    }

    public void ToggleFullscreen(int screenIndex)
    {
        if (fullScreenIndex > -1)
        {
            bool toggleFullScreenOff = fullScreenIndex == screenIndex;

            foreach (var observer in m_Observers)
            {
                observer.ToggleFullScreen(false);
                observer.ToggleControlVisibility(!PlayerPrefs.HasKey("OberverUIHidden") && toggleFullScreenOff);
            }

            fullScreenIndex = -1;

            if (toggleFullScreenOff)
                return;
        }
        else
        {
            foreach (var observer in m_Observers)
            {
                observer.ToggleControlVisibility(false);
            }
        }

        if (screenIndex < m_Observers.Length)
        {
            m_Observers[screenIndex].ToggleFullScreen(true);
            m_Observers[screenIndex].ToggleControlVisibility(!PlayerPrefs.HasKey("OberverUIHidden"));
            fullScreenIndex = screenIndex;
        }

        if (OnToggleObserverFullscreen != null)
            OnToggleObserverFullscreen(screenIndex);
    }

    public void ToggleCameraControlVisibility()
    {
        if (fullScreenIndex > -1 && fullScreenIndex < m_Observers.Length)
        {
            m_Observers[fullScreenIndex].ToggleControlVisibility();
        }
        else
        {
            foreach (var cam in m_Observers)
            {
                cam.ToggleControlVisibility();
            }
        }

        if (PlayerPrefs.HasKey("OberverUIHidden"))
        {
            PlayerPrefs.DeleteKey("OberverUIHidden");
        }
        else
        {
            PlayerPrefs.SetInt("OberverUIHidden", 1);
        }

        PlayerPrefs.Save();

        if (OnToggleObserverUI != null)
            OnToggleObserverUI();
    }
}