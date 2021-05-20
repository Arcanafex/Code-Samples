using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using System;

namespace Spaces.Core
{
    public class PortalWidget : Widget//, IProgressive
    {
        public string spaceID;
        //public string sceneName;
        public bool generateHotspot;
        public bool LoadOnStart;

        public UnityEvent OnSpaceLoad;
        public UnityEvent OnSpaceLoadComplete;
        public UnityEvent OnSpaceEnter;

        //All the associated parts
        //private ModelWidget model;
        private HotspotWidget hotspot;

        private Space m_space;
        //protected bool initialized;

        public virtual void Initialize(Space space = null)
        {
            initialized = true;

            SetSpace(space);

            if (m_space == null)
                GetSpace(spaceID);

            if (!hotspot && generateHotspot)
            {
                var sphere = gameObject.AddComponent<SphereCollider>();
                sphere.radius = 1;
                sphere.isTrigger = true;

                if (generateHotspot)
                {
                    var portalVisualization = UnityClient.UserSession.Instance.GetInstance("Portal Interface", transform);
                    var popup = portalVisualization.GetComponentInChildren<PopupWidget>();
                    var label = portalVisualization.GetComponentInChildren<TextMesh>();

                    if (popup) popup.FaceCamera();
                    if (label) label.text = m_space.name;
                }

                hotspot = gameObject.AddComponent<HotspotWidget>();
                hotspot.OnActivate = new UnityEvent();
                hotspot.OnActivate.AddListener(Load);
            }
        }

        protected override void Start()
        {
            if (!initialized)
                Initialize(m_space);

            if (LoadOnStart)
                Load();
        }

        private void GetSpace(string spaceID)
        {
            m_space = Space.SpacesManager.GetSpace(spaceID);

            if (m_space == null)
            {
                m_space = Space.SpacesManager.GetSpaces().FirstOrDefault(s => s.name == spaceID);

                if (m_space == null)
                {
                    m_space = new Space(spaceID);
                    Space.SpacesManager.AddSpace(m_space);
                }
            }
        }

        public virtual void SetSpace(Space space)
        {
            m_space = space;

            if (m_space != null)
                spaceID = m_space.id;
        }

        public void Load()
        {
            Debug.Log(this.name + " Loading " + m_space.name + " (" + m_space.id + ")!");

            var progressMeter = OpenProgressMeter();
            progressMeter.Initialize(m_space);
            //m_space.onLoadSpaceDone += progressMeter.Dispose;
            m_space.Enter(this);
        }

        private ProgressMeterWidget OpenProgressMeter()
        {
            ProgressMeterWidget progressMeter = null;

            var avatar = AvatarWidget.UserAvatar; //UnityClient.UserSession.Instance.Avatar;
            Transform observer = avatar ? avatar.head : Camera.main ? Camera.main.transform : new GameObject().transform;

            Vector3 uiPos = observer.position;
            Quaternion uiRot = Quaternion.LookRotation(observer.position - uiPos, Vector3.up);

            var meterGO = UnityClient.UserSession.Instance.GetInstance("ProgressMeter", this.transform);

            if (meterGO)
            {
                meterGO.transform.rotation = uiRot;
                progressMeter = meterGO.GetComponent<Core.ProgressMeterWidget>();
                progressMeter.FaceCamera();
            }

            return progressMeter;
        }
    }
}