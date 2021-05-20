using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using System.Linq;

namespace Spaces.Core
{
    public class HotspotWidget : Widget, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler, IEventSystemHandler//, IMoveHandler, IPointerDownHandler, IPointerUpHandler
    {
        public enum HotspotState
        {
            Enabled,
            Activated,
            Disabled
        }

        public enum EventType
        {
            None,
            Enter,
            Activate,
            Deactivate,
            Exit
        }

        public HotspotState hotspotState { get; private set; }

        [Tooltip("When clicked does hotspot stay activated?")]
        public bool toggle;

        [Header("Tinting")]
        public bool useTinting;
        public Color NormalColor;
        public Color HighlightedColor;
        public Color ActivatedColor;
        public Color DisabledColor;

        private Color currentColor;
        private Color CurrentColor
        {
            get { return currentColor; }
            set
            {
                currentColor = value;

                if (m_target == null)
                    Debug.LogWarning(this.name + " [Not attached to a ModelWidget]");
                else
                    m_target.SetColor(currentColor);
            }
        }

        public float FadeDuration;
        private bool inHover;

        private IModelWidget m_target;
        private Widget m_widget;
        public override Widget parentWidget
        {
            get { return m_widget; }
        }

        [Header("Events")]
        public UnityEvent OnEnter;
        public UnityEvent OnActivate;
        public UnityEvent OnDeactivate;
        public UnityEvent OnExit;


        private void Initialize(IModelWidget iModel = null)
        {
            m_target = iModel != null ? iModel : GetComponentInChildren<IModelWidget>();
            m_widget = m_target as Widget;

            if (useTinting)
            {
                if (hotspotState == HotspotState.Disabled)
                    CurrentColor = DisabledColor;
                else if (hotspotState == HotspotState.Activated)
                    CurrentColor = ActivatedColor;
                else
                    CurrentColor = NormalColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            inHover = true;

            if (useTinting)
            {
                StopAllCoroutines();

                if (hotspotState != HotspotState.Disabled)
                {
                    StartCoroutine(UpdateColor(HighlightedColor, FadeDuration));
                }
            }

            OnEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            inHover = false;

            if (useTinting)
            {
                StopAllCoroutines();

                if (hotspotState == HotspotState.Activated)
                    StartCoroutine(UpdateColor(ActivatedColor, FadeDuration));
                else
                    StartCoroutine(UpdateColor(NormalColor, FadeDuration));
            }

            OnExit.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (hotspotState == HotspotState.Enabled)
            {
                if (toggle)
                {
                    hotspotState = HotspotState.Activated;

                    if (useTinting)
                    {
                        StopAllCoroutines();
                        StartCoroutine(UpdateColor(ActivatedColor, FadeDuration));
                    }
                }
                //else
                //{
                //    if (useTinting)
                //    {
                //        lastColor = CurrentColor;
                //        CurrentColor = ActivatedColor;
                //        StartCoroutine(UpdateColor(NormalColor, Mathf.Clamp(FadeDuration, 0.05f, FadeDuration)));
                //    }

                //}

                OnActivate.Invoke();
            }
            else
            {
                Deactivate();
            }
        }

        public void Disable()
        {
            hotspotState = HotspotState.Disabled;
        }

        public void Enable()
        {
            hotspotState = HotspotState.Enabled;
        }

        public void Deactivate()
        {
            hotspotState = HotspotState.Enabled;

            if (useTinting)
            {
                StopAllCoroutines();
                StartCoroutine(UpdateColor(NormalColor, FadeDuration));
            }

            OnDeactivate.Invoke();
        }

        private IEnumerator UpdateColor(Color targetColor, float duration)
        {
            Color startingColor = CurrentColor;
            float elapsedTime = 0;

            do
            {
                if (duration == 0)
                    elapsedTime = duration = 1;
                else
                    elapsedTime = Mathf.Clamp(elapsedTime + Time.deltaTime, 0, duration);

                CurrentColor = Color.Lerp(startingColor, targetColor, elapsedTime / duration);
                yield return null;
            }
            while (elapsedTime < duration);
        }


        // TODO: enable toggle groups of hotspots
        public void OnSelect(BaseEventData eventData)
        {
            Debug.Log(this.name + " [Selected]");
            //throw new NotImplementedException();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            Debug.Log(this.name + " [De-Selected]");
            //throw new NotImplementedException();
        }
    }
}