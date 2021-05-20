using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace Spaces.UnityClient
{
    public class SpaceCaster : BaseRaycaster
    {
        public enum Type
        {
            Gaze,
            Beam
        }

        public enum Location
        {
            Head,
            LeftHand,
            RightHand,
            Other
        }

        public Type CasterType;
        public Location CasterLocation;
        public int id { get; private set; }
        public LayerMask EventMask = -1;

        protected Camera m_eventCamera;

        public CasterVisualizer visualizer;
        public CasterVisualizer m_visualizer;
        public bool casterActive;
        public bool ignoreReversedGraphics;

        protected bool hit;
        protected float physicsHitDistance;
        protected List<Graphic> graphicsToAppend;
        protected List<RaycastResult> raycastHits;

        public override Camera eventCamera
        {
            get
            {
                if (!m_eventCamera)
                {
                    m_eventCamera = GetComponent<Camera>();

                    if (!m_eventCamera)
                    {
                        m_eventCamera = gameObject.AddComponent<Camera>();

                        m_eventCamera.depth = -1;
                        m_eventCamera.stereoTargetEye = StereoTargetEyeMask.None;
                    }
                }

                return m_eventCamera;
            }
        }

        #region Unity Lifetime Calls

        protected override void Awake()
        {
            base.Awake();
            raycastHits = new List<RaycastResult>();

            if (visualizer)
            {
                m_visualizer = Instantiate(visualizer);
                m_visualizer.GenerateBeam(this);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SpacesInputModule.SpaceCasterManager.AddRaycaster(this);
            id = SpacesInputModule.SpaceCasterManager.GetRaycasters().Max(s => s.id) + 1;
            casterActive = true;

            if (m_visualizer)
            {
                StartCoroutine(m_visualizer.UpdateBeam(this, raycastHits));
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SpacesInputModule.SpaceCasterManager.RemoveRaycasters(this);
            casterActive = false;
        }

        //protected override void Start()
        //{
        //    base.Start();

        //    if (visualizer)
        //    {
        //        visualizer.GenerateBeam(this);
        //    }
        //}

        #endregion

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (eventData == null)
                return;

            if (raycastHits == null)
                raycastHits = new List<RaycastResult>();
            else
                raycastHits.Clear();

            var ray = GetRay();
            PhysicsRaycast(ray, eventData);
            CanvasRaycast(ray, eventData);

            if (raycastHits.Count > 0)
            {
                resultAppendList.AddRange(raycastHits);
                hit = true;
            }
            else
            {
                hit = false;
            }
        }

        protected virtual void PhysicsRaycast(Ray ray, PointerEventData eventData)
        {
            float dist = eventCamera.farClipPlane - eventCamera.nearClipPlane;

            foreach (RaycastHit hit in Physics.RaycastAll(ray, dist, EventMask).OrderBy(h => h.distance))
            {
                var castResult = new RaycastResult
                {
                    gameObject = hit.transform.gameObject,
                    module = this,
                    distance = hit.distance,
                    screenPosition = eventData.position,
                    index = raycastHits.Count,
                    //depth = hit.depth,
                    //sortingLayer = canvas.sortingLayerID,
                    //sortingOrder = canvas.sortingOrder,
                    worldPosition = hit.point,
                    worldNormal = hit.normal
                };

                raycastHits.Add(castResult);
            }

            physicsHitDistance = raycastHits.Count > 0 ? raycastHits.Min(h => h.distance) : float.MaxValue;
        }

        protected virtual void CanvasRaycast(Ray ray, PointerEventData eventData)
        {
            float distance = 0;
            float closestDistance = physicsHitDistance;
            RectTransform closestCanvasRect = null;
            Plane closestCanvasPlane = new Plane();
            Vector3 closestIntersectPoint = Vector3.zero;

            foreach (var canvas in CanvasTracker.GetCanvasSet(EventMask.value))
            {
                if (!canvas.canvas)
                    continue;

                Plane canvasPlane = new Plane(canvas.worldCorners[1], canvas.worldCorners[2], canvas.worldCorners[3]);

                if (ignoreReversedGraphics && !canvasPlane.GetSide(transform.position))
                    continue;

                if (canvasPlane.Raycast(ray, out distance))
                {
                    Vector3 intersectionPoint = transform.position + transform.forward * distance;
                    Vector3 localIntersectPoint = canvas.canvasRect.InverseTransformPoint(intersectionPoint);

                    if (canvas.canvasRect.rect.Contains(localIntersectPoint) && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestCanvasRect = canvas.canvasRect;
                        closestCanvasPlane = canvasPlane;
                        closestIntersectPoint = intersectionPoint;
                    }
                }
            }

            // We've determined the closest Canvas we're pointing at, if any. Check graphic compponents for hits.

            if (closestCanvasRect != null)
            {
                var canvas = closestCanvasRect.GetComponent<Canvas>();

                if (graphicsToAppend == null)
                    graphicsToAppend = new List<Graphic>();
                else
                    graphicsToAppend.Clear();

                graphicsToAppend = GraphicRegistry.GetGraphicsForCanvas(canvas).ToList()
                    .Where(graphic => graphic.depth > -1
                        && graphic.raycastTarget
                        && graphic.isActiveAndEnabled
                        && graphic.rectTransform.rect.Contains(graphic.transform.InverseTransformPoint(closestIntersectPoint)))
                    .OrderByDescending(graphic => graphic.depth)
                    .ToList();

                foreach (Graphic graphic in graphicsToAppend)
                {
                    var castResult = new RaycastResult
                    {
                        gameObject = graphic.gameObject,
                        module = this,
                        distance = closestDistance,
                        screenPosition = graphic.transform.InverseTransformPoint(closestIntersectPoint),
                        index = raycastHits.Count,
                        depth = graphic.depth,
                        sortingLayer = canvas.sortingLayerID,
                        sortingOrder = canvas.sortingOrder,
                        worldPosition = closestIntersectPoint,
                        worldNormal = closestCanvasPlane.normal
                    };

                    raycastHits.Add(castResult);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (hit)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawLine(transform.position, transform.position + (transform.forward * 10000));
        }

        public virtual Vector3 Forward
        {
            get
            {
                return transform.position + transform.forward;
            }
        }

        public virtual Ray GetRay()
        {
            return new Ray(transform.position, transform.forward);
        }
    }
}