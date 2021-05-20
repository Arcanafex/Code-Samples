//-----------------------------------------------------------------------------
// File: TrackingVolumeOrigin.cs
//
// This exists so that we can modify the origin used to position all tracked objects.
// 
//-----------------------------------------------------------------------------


using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

namespace Spaces.LBE
{
    public class TrackingVolume : Singleton<TrackingVolume>
    {
        [System.Serializable]
        public class Layer
        {
            public string name;
            public Transform transform;
            public Transform anchor;
            private Vector3 offset;

            // Dict to allow tracked objects to be moved as if parented to a specified transform
            private static List<Layer> s_TrackingVolumeLayers;
            internal static List<Layer> TrackingVolumeLayers
            {
                get
                {
                    if (s_TrackingVolumeLayers == null)
                        s_TrackingVolumeLayers = new List<Layer>();

                    return s_TrackingVolumeLayers;
                }
            }

            public Layer(string layerName, Transform anchor = null)
            {
                name = layerName;
                transform = new GameObject(layerName).transform;
                TrackingVolume.Add(transform);

                if (anchor != null)
                {
                    SetAnchor(anchor);
                }

                TrackingVolumeLayers.Add(this);
            }

            public void SetAnchor(Transform anchor)
            {
                this.anchor = anchor;
                this.offset = anchor.InverseTransformPoint(transform.position);
            }

            public void UnsetAnchor()
            {
                anchor = null;
            }

            public void Add(Transform trackedObject)
            {
                trackedObject.SetParent(transform);
            }

            public bool Contains(Transform trackedObject)
            {
                foreach (Transform child in transform)
                {
                    if (child == trackedObject)
                    {
                        // found it!
                        return true;
                    }
                }

                return false;
            }

            public void Clear()
            {
                var childQueue = new List<Transform>();

                foreach (Transform child in transform)
                {
                    childQueue.Add(child);
                }

                childQueue.ForEach(c => c.SetParent(transform.parent));

            }

            public void Update()
            {
                if (anchor)
                {
                    transform.SetPositionAndRotation(anchor.position + offset, anchor.rotation);
                }
            }
        }

        public static Transform Origin
        {
            get
            {
                return Instance.transform;
            }
        }

        private Transform m_SceneRoot;
        public static Transform SceneOrigin
        {
            get
            {
                if (!Instance.m_SceneRoot)
                {
                    Instance.CreateSceneRoot();
                }

                return Instance.m_SceneRoot;
            }
        }

        public static List<Layer> TrackingVolumeLayers
        {
            get
            {
                return Layer.TrackingVolumeLayers;
            }
        }

        public event Action onResetSceneOrigin;
        public event Action<Vector3, Quaternion> onUpdateSceneOrigin;
        public event Action<Vector3> onUpdateSceneOriginPosition;
        public event Action<Quaternion> onUpdateSceneOriginRotation;

        private void Awake()
        {
            if (ThisApp.instance != null)
                UpdateOrigin(ThisApp.instance.GetOriginOffset(), ThisApp.instance.GetOriginRotation());

            CreateSceneRoot();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        private void LateUpdate()
        {
            if(TrackingVolumeLayers != null)
                TrackingVolumeLayers.ForEach(layer => layer.Update());
        }

        //-------------------------------------------------------

        private void CreateSceneRoot()
        {
            m_SceneRoot = new GameObject("Scene Root").transform;
            m_SceneRoot.SetParent(transform, false);

            Debug.Log(this.name + " [Scene Root Object Created]");
            ResetSceneOrigin();
        }

        //-------------------------------------------------------

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log(this.name + " [Scene Loaded] " + scene.name + ": Updating " + SceneOrigin.name);
            ResetSceneOrigin();
        }

        //-------------------------------------------------------

        private void UpdateOrigin(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);

            Debug.Log(this.name + " [Tracking Origin Updated] pos: " + position.ToString() + " rot: " + rotation.ToString());
        }

        private void UpdateOriginPosition(Vector3 position)
        {
            transform.position = position;
        }

        private void UpdateOriginRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        //-------------------------------------------------------

        public void ResetSceneOrigin()
        {
            var sceneVolume = FindObjectOfType<TrackingVolumeBounds>();

            if (sceneVolume)
            {
                UpdateSceneOrigin(sceneVolume.transform.position, sceneVolume.transform.rotation);

                Debug.Log(this.name + " [Scene Origin Updated to " + sceneVolume.name +"] pos: " + sceneVolume.transform.position.ToString() + " rot: " + sceneVolume.transform.rotation.ToString());
            }
            else
            {
                UpdateSceneOrigin(transform.position, transform.rotation);

                Debug.Log(this.name + " [Scene Origin Updated to Tracking Volume Origin] pos: " + transform.position.ToString() + " rot: " + transform.rotation.ToString());
            }

            if (onResetSceneOrigin != null)
                onResetSceneOrigin();
        }

        public void UpdateSceneOrigin(Vector3 position, Quaternion rotation)
        {
            SceneOrigin.SetPositionAndRotation(position, rotation);

            if (onUpdateSceneOrigin != null)
                onUpdateSceneOrigin(position, rotation);
        }

        public void UpdateSceneOriginPosition(Vector3 position)
        {
            SceneOrigin.position = position;

            if (onUpdateSceneOriginPosition != null)
                onUpdateSceneOriginPosition(position);
        }

        public void UpdateSceneOriginRotation(Quaternion rotation)
        {
            SceneOrigin.rotation = rotation;

            if (onUpdateSceneOriginRotation != null)
                onUpdateSceneOriginRotation(rotation);
        }

        //-------------------------------------------------------

        /// <summary>
        /// Adds a tracked object to the Tracking Volume's scene root.
        /// </summary>
        /// <param name="trackedObject"></param>
        public static void Add(Transform trackedObject, bool maintainWorldPosition = false)
        {
            trackedObject.SetParent(SceneOrigin, maintainWorldPosition);

            Debug.Log(Instance.name + " [Tracked Object Added] " + trackedObject.name);
        }

        /// <summary>
        /// Adds a tracked object to the specified layer in the Tracking Volume. If the layer does not exist, it will be created.
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="trackedObject"></param>
        public static void Add(string layerName, Transform trackedObject)
        {
            if (String.IsNullOrEmpty(layerName))
            {
                Debug.LogWarning("[Layer Name is empty]");
            }

            Layer layer = AddLayer(layerName);
            layer.Add(trackedObject);

            Debug.Log(Instance.name + " [Tracked Object Added to Layer " + layerName + "] " + trackedObject.name);
        }

        /// <summary>
        /// Adds a layer to the Tracking Volume with the name specified.
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static Layer AddLayer(string layerName)
        {
            // Check to see if a layer by this name already exists
            Layer layer = GetLayer(layerName);

            // Create layer if it doesn't exist
            if (layer == null)
            {
                layer = new Layer(layerName);
            }

            return layer;
        }

        /// <summary>
        /// Adds a layer to the Tracking Volume with the name specified, and anchors it to move with the specified anchor.
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public static Layer AddLayer(string layerName, Transform anchor)
        {
            // Check to see if a layer by this name already exists
            Layer layer = GetLayer(layerName);

            // Create layer if it doesn't exist
            if (layer == null)
            {
                layer = new Layer(layerName, anchor);
            }
            else
            {
                layer.SetAnchor(anchor);
            }

            return layer;
        }

        public static Layer GetLayer(string layerName)
        {
            // Check to see if a layer by this name already exists
            return TrackingVolumeLayers.Count == 0 ? null : TrackingVolumeLayers.FirstOrDefault(l => l.name == layerName);
        }

        public static void SetLayerAnchor(string layerName, Transform target)
        {
            Layer layer = GetLayer(layerName);

            if (layer != null)
            {
                layer.SetAnchor(target);
            }
            else
            {
                Debug.LogWarning(Instance.name + " [Layer Does Not Exist]");
            }
        }

        public static void UnsetAnchor(string layerName)
        {
            Layer layer = GetLayer(layerName);

            if (layer != null)
            {
                layer.UnsetAnchor();
            }
            else
            {
                Debug.LogWarning(Instance.name + " [Layer Does Not Exist]");
            }
        }

        //-------------------------------------------------------

        private void OnDrawGizmos()
        {
            // Draw Up Vector
            Gizmos.color = Color.green;
            Gizmos.DrawRay(Vector3.zero, Vector3.up);

            // Draw Forward Vector
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(Vector3.zero, Vector3.forward);

            //Draw Right Vector
            Gizmos.color = Color.red;
            Gizmos.DrawRay(Vector3.zero, Vector3.right);

            // Draw World Origin
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(Vector3.zero, 0.1f);

        }

		//public void SetChaperone(Chaperone gameChaperone)
		//{
		//	m_Chaperone = gameChaperone;
		//}
	}
}






