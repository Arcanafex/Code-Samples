using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Spaces.Core
{
    public class TimelineWidget : Widget, IProgressive
    {
        public enum ProgressMode
        {
            ElapsedTime,
            RemainingTime
        }

        public string DescriptiveName;
        public float Duration;

        public ProgressMode ProgressDisplayMode;
        public bool BeginAtStart;
        public bool PersistAcrossSpaces;

        public UnityEvent OnBegin;
        public List<TimelineEvent> TimelineEvents;
        public UnityEvent OnEnd;

        private int timelineEventIndex;
        private float currentTime;
        private bool isTimerRunning;

        public bool IsTimerRunning
        {
            get { return isTimerRunning; }
        }

        public float CurrentTime
        {
            get { return currentTime; }
        }

        [System.Serializable]
        public class TimelineEvent
        {
            public string Description;
            public float EventTime;
            public UnityEvent OnEvent;
        }

        void Awake()
        {
            if (PersistAcrossSpaces)
            {
                DontDestroyOnLoad(gameObject);
                OnEnd.AddListener(Dispose);

                Spaces.UnityClient.UserSession.Instance.OnChangingSpace.AddListener(Dispose);
            }
        }

        void Start()
        {
            Reset();

            if (BeginAtStart)
                Begin();
            else
                Pause();
        }

        private IEnumerator ProgressingTimeline()
        {
            Debug.Log(this.name + " [Progressing Timeline] - " + currentTime);

            while (currentTime <= Duration)
            {
                if (isTimerRunning)
                {
                    currentTime += Time.deltaTime;

                    if (timelineEventIndex < TimelineEvents.Count && currentTime >= TimelineEvents[timelineEventIndex].EventTime)
                    {
                        Debug.Log(this.name + " [Event " + timelineEventIndex + "] - " + currentTime + " - ");

                        TimelineEvents[timelineEventIndex].OnEvent.Invoke();
                        timelineEventIndex++;
                    }
                }

                yield return null;
            }

            End();
        }

        public void Begin()
        {
            Debug.Log(this.name + " [Begin] - " + currentTime);

            isTimerRunning = true;
            StartCoroutine(ProgressingTimeline());

            OnBegin.Invoke();
        }

        public void Pause()
        {
            Debug.Log(this.name + " [Pause] - " + currentTime);

            isTimerRunning = false;
        }

        public void Unpause()
        {
            Debug.Log(this.name + " [Upause] - " + currentTime);

            isTimerRunning = true;
        }

        public void Reset()
        {
            Debug.Log(this.name + " [Reset] - " + currentTime);

            timelineEventIndex = 0;
            currentTime = 0;

            TimelineEvents.Sort((e1, e2) => e1.EventTime.CompareTo(e2.EventTime));
        }

        public void Stop()
        {
            Pause();
            StopAllCoroutines();
        }

        public void End()
        {
            currentTime = Duration;
            isTimerRunning = false;

            if (PersistAcrossSpaces)
                transform.SetParent(null);

            OnEnd.Invoke();

            if (PersistAcrossSpaces)
                Destroy(gameObject);
        }

        public string GetProgressMessage()
        {
            return "";
        }

        public float GetProgress()
        {
            if (ProgressDisplayMode == ProgressMode.ElapsedTime)
                return CurrentTime / Duration;
            else
                return Duration - CurrentTime / Duration;
        }

        public void Dispose()
        {
            Stop();
            Destroy(gameObject);
        }
    }
}