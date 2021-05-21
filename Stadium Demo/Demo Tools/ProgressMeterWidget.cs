using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

namespace TMPC.Tools
{
    public class ProgressMeterWidget : MonoBehaviour
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

        public PlayerLoader m_progressSource;

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

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        private void Start()
        {
            Hide();
            m_progressSource.OnLoading += OnProgressUpdate;
        }

        private void OnDestroy()
        {
            m_progressSource.OnLoading -= OnProgressUpdate;
        }

        private void OnProgressUpdate(LoadingEventType eventType)
        {
            switch(eventType)
            {
                case LoadingEventType.LoadingStart:
                    Show();
                    Text = "Loading...";
                    break;
                case LoadingEventType.LoadingDone:
                    Hide();
                    break;
                case LoadingEventType.LoadingProgressChange:
                    Value = m_progressSource.Progress;
                    UpdateProgress();
                    break;
                case LoadingEventType.LoadingFailed:
                    Text = "Failed";
                    break;
            }
        }

        public void UpdateProgress()
        {
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
    }
}
