using UnityEngine;
using System.Collections;
using System.Linq;
//using DG.Tweening;

namespace Spaces.Core
{
    public class MoveWidget : Widget
    {
        public float A;
        public float B;
        public float duration;

        private Widget model;

        void Start()
        {
            model = parentWidget;
        }

        //public void runDotween()
        //{
        //    if (model.transform.position.x > 0)
        //    {
        //        model.transform.DOMoveX(A, duration);
        //    }
        //    else
        //    {
        //        model.transform.DOMoveX(B, duration);
        //    }
        //}

        //[System.Serializable]
        //public class MoveState
        //{
        //    public string name;
        //    public Vector3 startValue;
        //    public Vector3 endValue;
        //    public float duration;
        //    public AnimationCurve curve;
        //    public bool loop;
        //}

        //public MoveState[] States;
        //public bool playOnStart;
        //public bool loopStates;

        //private Widget m_widget;
        //public override Widget parentWidget
        //{
        //    get { return m_widget; }
        //}

        //private bool isPlaying;
        //private float elapsedTime;
        //private Vector3 lastValue;

        //private int currentIndex;
        //public int CurrentIndex
        //{
        //    get
        //    {
        //        return currentIndex < States.Length && currentIndex > -1 ? currentIndex : 0;
        //    }
        //    set
        //    {
        //        currentIndex = value < States.Length && value > -1 ? value : 0;
        //    }
        //}

        //public MoveState CurrentState
        //{
        //    get { return currentIndex < States.Length && currentIndex > -1 ? States[currentIndex] : null; }
        //}

        //void Start()
        //{
        //    currentIndex = 0;
        //    isPlaying = false;

        //    m_widget = GetComponentsInParent<Widget>().FirstOrDefault(w => w.GetComponent<IModelWidget>() != null);
        //    lastValue = CurrentState.startValue;
        //    m_widget.transform.position = lastValue;

        //    if (playOnStart)
        //        Play();
        //}

        //void Update()
        //{
        //    if (isPlaying)
        //    {
        //        elapsedTime += Time.deltaTime;

        //        if (elapsedTime > CurrentState.duration)
        //        {
        //            if (CurrentState.loop)
        //                elapsedTime = 0;
        //            else
        //                Next();
        //        }
        //        else
        //        {
        //            m_widget.transform.position = Vector3.Lerp(
        //                lastValue,
        //                CurrentState.endValue,
        //                CurrentState.curve.Evaluate(elapsedTime / CurrentState.duration)
        //                );
        //        }
        //    }
        //}

        //public void Play()
        //{
        //    Play(CurrentIndex);
        //}

        //public void Play(int state)
        //{
        //    if (CurrentIndex != state)
        //    {
        //        lastValue = m_widget.transform.position;
        //        CurrentIndex = state;
        //    }

        //    isPlaying = true;
        //}

        //public void Pause()
        //{
        //    isPlaying = false;
        //}

        //public void Stop()
        //{
        //    isPlaying = false;
        //    elapsedTime = 0;
        //    CurrentIndex = 0;
        //    lastValue = CurrentState.startValue;
        //    m_widget.transform.position = CurrentState.startValue;
        //}

        //public void Next()
        //{
        //    if (CurrentIndex + 1 < States.Length || loopStates)
        //    {
        //        elapsedTime = 0;

        //        if (isPlaying)
        //            Play(CurrentIndex + 1);
        //        else
        //            Play();
        //    }
        //    else if (!loopStates && elapsedTime > CurrentState.duration)
        //    {
        //        Stop();
        //    }
        //}

        //public void Previous()
        //{
        //    elapsedTime = 0;

        //    if (isPlaying)
        //        Play(CurrentIndex - 1);
        //    else
        //        Play();
        //}

    }
}