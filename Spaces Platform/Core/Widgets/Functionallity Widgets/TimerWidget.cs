using UnityEngine;
using UnityEngine.Events;

namespace Spaces.Core
{
    public class TimerWidget : Widget, IProgressive
    {
        public enum ProgressMode
        {
            ElapsedTime,
            RemainingTime
        }

        public string DescriptiveName;
        public float Duration;
        public float RemainingTime;
        public ProgressMode DisplayMode;

        public bool BeginAtStart;
        private bool isTimerRunning;

        public bool IsTimerRunning
        {
            get { return isTimerRunning; }
        }

        public UnityEvent OnBegin;
        public UnityEvent OnEnd;

        public override void Initialize()
        {
            base.Initialize();

            ResetTimer();

            if (BeginAtStart)
                Begin();
            else
                PauseTimer();
        }

        void Update()
        {
            if (isTimerRunning)
            {
                RemainingTime -= Time.deltaTime;

                if (RemainingTime <= 0)
                {
                    End();
                }
            }
        }

        public void Begin()
        {
            isTimerRunning = true;
            OnBegin.Invoke();
        }

        public void PauseTimer()
        {
            isTimerRunning = false;
        }

        public void UnpauseTimer()
        {
            isTimerRunning = true;
        }

        public void ResetTimer()
        {
            RemainingTime = Duration;
        }

        public void End()
        {
            RemainingTime = 0;
            PauseTimer();
            OnEnd.Invoke();
        }

        public string GetProgressMessage()
        {
            return DescriptiveName;
        }

        public float GetProgress()
        {
            if (DisplayMode == ProgressMode.ElapsedTime)
                return (Duration - RemainingTime) / Duration;
            else
                return RemainingTime / Duration;
        }

        public static TimerWidget CreateTimer(float duration, bool immedateStart, bool visible)
        {
            var timer = new GameObject().AddComponent<TimerWidget>();

            timer.Duration = duration;
            timer.BeginAtStart = immedateStart;
            timer.OnBegin = new UnityEvent();
            timer.OnEnd = new UnityEvent();

            if (visible)
            {
                var progressMeterGO = UnityClient.UserSession.Instance.GetInstance("ProgressMeter", timer.transform);
                var progressMeter = progressMeterGO.GetComponent<ProgressMeterWidget>();
                progressMeter.Initialize(timer);
                progressMeter.FaceCamera();
            }

            return timer;
        }
    }
}