using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


namespace Spaces.Core
{
    public interface IProgressive
    {
        string GetProgressMessage();
        float GetProgress();
    }

    public class ProgressMeterWidget : Widget, IPopupWidget, IDisplay<IProgressive>
    {
        public enum DisplayFormat
        {
            Percentage,
            Normalized
        }

        public Text NumericDisplay;
        public Text Label;
        public Image ProgressDisplay;
        public DisplayFormat displayFormat;

        private float m_value;
        private float lastValue;
        private IProgressive m_progressSource;

        public float Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public string Text
        {
            get { return Label ? Label.text : ""; }
            set
            {
                if (Label)
                    Label.text = value;
            }
        }

        public float ProgressPercentage
        {
            get { return m_value * 100; }
        }

        public void Initialize(IProgressive source = null)
        {
            SetContent(source);

            if (m_progressSource != null)
            {
                Text = m_progressSource.GetProgressMessage();
                Value = m_progressSource.GetProgress();
            }
        }

        public void Initialize(Space source)
        {
            Subscribe(source);
            SetContent(source);

            if (m_progressSource != null)
            {
                Text = m_progressSource.GetProgressMessage();
                Value = m_progressSource.GetProgress();
            }
        }

        public void Initialize(Asset source)
        {
            Subscribe(source);
            SetContent(source);

            if (m_progressSource != null)
            {
                Text = m_progressSource.GetProgressMessage();
                Value = m_progressSource.GetProgress();
            }
        }

        void Update()
        {
            if (m_progressSource != null)
            {
                Text = m_progressSource.GetProgressMessage();
                Value = m_progressSource.GetProgress();
            }

            if (m_value != lastValue)
                UpdateProgress();
        }

        public void UpdateProgress()
        {
            Label.text = Text;

            if (displayFormat == DisplayFormat.Percentage)
            {
                NumericDisplay.text = Value.ToString("0%");
            }
            else
            {
                NumericDisplay.text = Value.ToString("0.00");
            }

            ProgressDisplay.fillAmount = m_value;
            lastValue = m_value;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetContent(IProgressive content)
        {
            m_progressSource = content;
        }

        public void FaceCamera()
        {
            //transform.LookAt(UnityClient.UserSession.Instance.Avatar ? UnityClient.UserSession.Instance.Avatar.transform : Camera.main ? Camera.main.transform : new GameObject().transform);
            transform.LookAt(AvatarWidget.UserAvatar ? AvatarWidget.UserAvatar.transform : Camera.main ? Camera.main.transform : new GameObject().transform);
        }

        public void Dispose(Space space)
        {
            if (!this)
                return;

            Debug.Log(this.name + "[Disposing]");
            Hide();
            DestroyImmediate(gameObject);
        }

        public void Dispose(Asset asset)
        {
            if (!this)
                return;

            Debug.Log(this.name + "[Disposing]");
            Hide();
            DestroyImmediate(gameObject);
        }

        public void Dispose()
        {
            Debug.Log(this.name + "[Disposing]");
            Hide();
            DestroyImmediate(gameObject);
        }

        void OnDestroy()
        {
            if (isSubscribedToSpace)
                UnSubscribe(m_progressSource as Space);

            if (isSubscribedToAsset)
                UnSubscribe(m_progressSource as Asset);
        }

        protected virtual bool isSubscribedToAsset { get; private set; }
        protected virtual bool isSubscribedToSpace { get; private set; }
        protected virtual bool isSubscribedToProgressSource { get; private set; }

        protected virtual void Subscribe(Asset asset)
        {
            if (asset == null)
                return;

            if (!isSubscribedToAsset)
            {
                isSubscribedToAsset = true;
                asset.onLoadAssetDone += Dispose;
            }
        }

        protected virtual void UnSubscribe(Asset asset)
        {
            if (asset == null)
                return;

            if (isSubscribedToAsset)
            {
                isSubscribedToAsset = false;
                asset.onLoadAssetDone -= Dispose;
            }
        }

        protected virtual void Subscribe(Space space)
        {
            if (space == null)
                return;

            if (!isSubscribedToSpace)
            {
                isSubscribedToSpace = true;
                space.onLoadSpaceDone += Dispose;
            }
        }

        protected virtual void UnSubscribe(Space space)
        {
            if (space == null)
                return;

            if (isSubscribedToSpace)
            {
                isSubscribedToSpace = false;
                space.onLoadSpaceDone -= Dispose;
            }
        }
    }
}