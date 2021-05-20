using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Singleton for managing canvas references, and returning them by layerMask.
/// </summary>
public class CanvasTracker : MonoBehaviour
{
    public class CanvasInfo
    {
        public Canvas canvas;
        public RectTransform canvasRect;
        public int eventLayer;
        public Vector3[] worldCorners;

        public CanvasInfo (Canvas canvas)
        {
            this.canvas = canvas;
            this.canvasRect = canvas.GetComponent<RectTransform>();
            eventLayer = this.canvas.gameObject.layer;

            worldCorners = new Vector3[4];
            canvasRect.GetWorldCorners(worldCorners);
        }

        public void UpdateWorldCorners()
        {
            canvasRect.GetWorldCorners(worldCorners);
        }
    }
    
    protected static CanvasTracker instance;
    public static CanvasTracker Instance
    {
        get
        {
            if (!instance)
                instance = FindObjectOfType<CanvasTracker>();

            if (!instance)
                instance = new GameObject("Canvas Tracker").AddComponent<CanvasTracker>();

            return instance;
        }
    }

    protected bool active;

    #region Unity Lifetime calls

    void Awake()
    {
        canvasList = new List<CanvasInfo>();
    }

    protected void OnEnable()
    {
        active = true;
        StartCoroutine(UpdateCanvasSet());
    }

    protected void OnDisable()
    {
        active = false;
    }

    #endregion

    List<CanvasInfo> canvasList;

    protected virtual IEnumerator UpdateCanvasSet()
    {
        while (active)
        {
            if (canvasList == null)
                canvasList = new List<CanvasInfo>();
            else
                canvasList.Clear();

            foreach (Canvas canvas in FindObjectsOfType<Canvas>())
            {
                var canvasInfo = canvasList.FirstOrDefault(info => info.canvas == canvas);

                if (canvasInfo != null)
                    canvasInfo.UpdateWorldCorners();
                else
                    canvasList.Add(new CanvasInfo(canvas));
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    public static IEnumerable<CanvasInfo> GetCanvasSet(int layerMask)
    {
        return Instance.canvasList != null ? Instance.canvasList.Where(info => (layerMask & (1 << info.eventLayer)) == (1 << info.eventLayer)) : new List<CanvasInfo>(0);
    }
}
