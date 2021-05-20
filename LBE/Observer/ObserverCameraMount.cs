using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ObserverCameraMount : MonoBehaviour
{
    public enum Type
    {
        Default,
        Player,
        HandHeld,
        Drone
    }

    public int Ordinal;
    public Type PositionType;
    public bool Orthographic;
    public float FovOrSize;
    public Cinemachine.CinemachineVirtualCamera digitalCameraRef;
    public bool hasCameraRef = false;

    public List<Transform> IgnoredObjects;

    private List<Renderer> m_IgnoreRenderers;
    public List<Renderer> IgnoredRenderers
    {
        get
        {
            if (m_IgnoreRenderers == null)
                m_IgnoreRenderers = new List<Renderer>();

            return m_IgnoreRenderers;
        }

        set
        {
            m_IgnoreRenderers = value;
        }
    }

    private Queue<Transform> rendererSearchQueue;
    private const float listUpdateFreq = 5;
    private float lastListUpdate;

    #region Static Properties
    private static HashSet<ObserverCameraMount> s_ObserverMounts;
    public static int Count { get { return s_ObserverMounts != null ? s_ObserverMounts.Count : 0; } }

    private static void Add(ObserverCameraMount mount)
    {
        if (s_ObserverMounts == null)
            s_ObserverMounts = new HashSet<ObserverCameraMount>();

        if (s_ObserverMounts.Add(mount))
        {
            Spaces.LBE.DebugLog.Log("operatorui", "[Observer Mount Added] " + mount.name);

            if (OnMountAdded != null)
                OnMountAdded();
        }
    }

    private static void Remove(ObserverCameraMount mount)
    {
        if (s_ObserverMounts != null)
            s_ObserverMounts.Remove(mount);
    }

    public static void GetObserverCameraPositions(ref List<ObserverCameraMount> posList)
    {
        posList = s_ObserverMounts.OrderBy(mount => mount.Ordinal).ThenBy(mount => mount.name).ToList();
    }

    public static System.Action OnMountAdded;
    
    #endregion



    private void Awake()
    {
        ObserverCameraMount.Add(this);
    }


    private void Start()
    {
        hasCameraRef = false;

        if (Spaces.LBE.SpacesGameManager.Instance && digitalCameraRef)
        {
            Spaces.LBE.SpacesGameManager.Instance.mDadCamViewfinderCameraRef = digitalCameraRef;
        }
    }

    public void Update()
    {
        if (hasCameraRef == false && digitalCameraRef && Spaces.LBE.SpacesGameManager.Instance)
        {
            hasCameraRef = true;
            Spaces.LBE.SpacesGameManager.Instance.mDadCamViewfinderCameraRef = digitalCameraRef;
        }

        if (Time.time - lastListUpdate > listUpdateFreq)
        {
            lastListUpdate = Time.time;

            if (IgnoredObjects == null)
                IgnoredObjects = new List<Transform>();

            foreach (var obj in IgnoredObjects)
            {
                if (obj)
                    UpdateIgnoredRendererList(obj.transform);
            }
        }
    }

    private void OnDestroy()
    {
        ObserverCameraMount.Remove(this);
    }



    private void UpdateIgnoredRendererList(Transform root)
    {
        if (rendererSearchQueue == null)
            rendererSearchQueue = new Queue<Transform>();
        else
            rendererSearchQueue.Clear();

        rendererSearchQueue.Enqueue(root);

        while (rendererSearchQueue.Count > 0)
        {
            var currentNode = rendererSearchQueue.Dequeue();

            foreach (var renderer in currentNode.GetComponents<Renderer>())
            {
                if (!IgnoredRenderers.Contains(renderer))
                {
                    IgnoredRenderers.Add(renderer);
                    Debug.Log(this.name + " [Adding Renderer To " + this.name + " Ignore List] " + renderer.name);
                }
            }

            foreach (Transform child in currentNode)
            {
                rendererSearchQueue.Enqueue(child);
            }
        }
    }

}
