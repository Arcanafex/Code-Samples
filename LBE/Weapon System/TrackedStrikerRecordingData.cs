using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackedStrikerRecordingData : MonoBehaviour
{
    public RecordTransformHierarchy m_Recorder;
    public GunSystem m_TrackedGun;

    //float m_triggerDownFloat;
    //bool m_TriggerDownBool;


    void Start()
    {
        //m_TriggerDownBool = false;
        m_Recorder.bindFloat<GunSystem>(string.Empty, "triggerDown");
        //m_Recorder.bindFloat<GunSystem>(string.Empty, "TriggerDown");
    }


    //void Update()
    //{
    //    if (m_Recorder.isRecording)
    //    {
    //        m_triggerDownFloat = m_TrackedGun.triggerDown;
    //        m_TriggerDownBool = m_TrackedGun.TriggerDown;
    //    }
    //    else if (m_Recorder.isPlayingbackClip())
    //    {
    //        m_TrackedGun.triggerDown = m_triggerDownFloat;
    //    }
    //}
}
