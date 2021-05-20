using UnityEngine;
using System.Collections;
using System;

namespace Spaces.Core
{
    public interface IPopupWidget
    {
        void Show();
        void Hide();
        void FaceCamera();
    }

    public interface IPopupWidget<T> : IPopupWidget
    {
        void SetContent(T content);
    }

    public class PopupWidget : Widget, IPopupWidget
    {
        public HotspotWidget.EventType ShowOn;
        public HotspotWidget.EventType HideOn;
        public bool ShowAtStart;
        public bool AlwaysFaceCamera;

        private bool isShowing;
        public bool IsShowing
        {
            get { return isShowing; }
        }

        public virtual void Show()
        {
            isShowing = true;

            foreach (Transform child in transform)
                child.gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            isShowing = false;

            foreach (Transform child in transform)
                child.gameObject.SetActive(false);
        }

        public virtual void FaceCamera()
        {
            transform.LookAt(Camera.main.transform);
        }

        void Update()
        {
            if (isShowing && AlwaysFaceCamera)
                FaceCamera();
        }

        void Start()
        {
            foreach (var hotspot in GetComponentsInParent<HotspotWidget>())
            {
                switch (ShowOn)
                {
                    case HotspotWidget.EventType.Activate:
                        foreach (IPopupWidget widgetInterface in GetComponents<IPopupWidget>())
                            hotspot.OnActivate.AddListener(widgetInterface.Show);
                        break;
                    case HotspotWidget.EventType.Deactivate:
                        foreach (IPopupWidget widgetInterface in GetComponentsInChildren<IPopupWidget>())
                            hotspot.OnDeactivate.AddListener(widgetInterface.Show);
                        break;
                    case HotspotWidget.EventType.Enter:
                        foreach (IPopupWidget widgetInterface in GetComponentsInChildren<IPopupWidget>())
                            hotspot.OnEnter.AddListener(widgetInterface.Show);
                        break;
                    case HotspotWidget.EventType.Exit:
                        foreach (IPopupWidget widgetInterface in GetComponentsInChildren<IPopupWidget>())
                            hotspot.OnExit.AddListener(widgetInterface.Show);
                        break;
                    default:
                        break;
                }

                switch (HideOn)
                {
                    case HotspotWidget.EventType.Activate:
                        foreach (IPopupWidget widgetInterface in GetComponentsInChildren<IPopupWidget>())
                            hotspot.OnActivate.AddListener(widgetInterface.Hide);
                        break;
                    case HotspotWidget.EventType.Deactivate:
                        foreach (IPopupWidget widgetInterface in GetComponentsInChildren<IPopupWidget>())
                            hotspot.OnDeactivate.AddListener(widgetInterface.Hide);
                        break;
                    case HotspotWidget.EventType.Enter:
                        foreach (IPopupWidget widgetInterface in GetComponentsInChildren<IPopupWidget>())
                            hotspot.OnEnter.AddListener(widgetInterface.Hide);
                        break;
                    case HotspotWidget.EventType.Exit:
                        foreach (IPopupWidget widgetInterface in GetComponentsInChildren<IPopupWidget>())
                            hotspot.OnExit.AddListener(widgetInterface.Hide);
                        break;
                    default:
                        break;
                }
            }

            if (ShowAtStart)
                Show();
            else
                Hide();

        }

    }
}