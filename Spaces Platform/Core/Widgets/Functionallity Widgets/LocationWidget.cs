using UnityEngine;

namespace Spaces.Core
{
    public class LocationWidget : Widget, IPopupWidget
    {
        public string description;
        public bool defaultLocation;

        public bool overrideRotation;
        public bool overrideScale;
        public Vector3 positionOffset;

        private SkyboxWidget m_skybox;
        private PopupWidget m_Popup;
        private EnvironmentWidget m_environment;

        public static LocationWidget Create(string locationName, Vector3 position)
        {
            var lgo = new GameObject(locationName);
            lgo.transform.position = position;
            var location = lgo.AddComponent<LocationWidget>();

            return location;
        }

        public override void Initialize()
        {
            base.Initialize();

            // component discovery.
            m_skybox = GetComponentInChildren<SkyboxWidget>();
            m_Popup = GetComponentInChildren<PopupWidget>();
            m_environment = GetComponentInChildren<EnvironmentWidget>();

            if (defaultLocation)
                Activate();
        }
        
        public void Activate()
        {
            if (m_Popup)
                m_Popup.Show();

            if (m_skybox)
                m_skybox.ApplySkybox();

            var avatar = AvatarWidget.UserAvatar;

            if (avatar)
            {
                avatar.transform.position = transform.position + positionOffset;

                if (overrideRotation)
                    avatar.transform.rotation = transform.rotation;

                if (overrideScale)
                    avatar.transform.localScale = transform.localScale;
            }

        }

        public void Deactivate()
        {
            m_Popup.Hide();
        }

        [ContextMenu("Activate Location")]
        public void ActivateLocation()
        {
            foreach (LocationWidget location in FindObjectsOfType<LocationWidget>())
            {
                if (location.InstanceID == InstanceID)
                {
                    var popup = location.GetComponentInChildren<PopupWidget>();
                    var skybox = location.GetComponentInChildren<SkyboxWidget>();

                    if (popup)
                    {
                        popup.Show();
                        popup.ShowAtStart = true;
                    }

                    if (skybox)
                    {
                        skybox.ApplySkybox();
                    }
                }
                else
                {
                    location.GetComponentInChildren<PopupWidget>().Hide();
                }
            }

        }

        public void ActivateLocation(string id)
        {
            foreach (LocationWidget location in FindObjectsOfType<LocationWidget>())
            {
                if (location.InstanceID == id)
                {
                    location.Activate();
                }
                else
                {
                    if (m_Popup)
                        m_Popup.Hide();
                }
            }
        }

        public void Show()
        {
            Activate();
        }

        public void Hide()
        {
            Deactivate();
        }

        public void FaceCamera()
        {
            //Set Rotation of Skybox maybe? alter facing of Avatar?
        }
    }
}
