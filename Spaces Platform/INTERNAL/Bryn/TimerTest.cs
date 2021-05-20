using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerTest : MonoBehaviour
{
    private Spaces.Core.TimerWidget timer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            timer = Spaces.Core.TimerWidget.CreateTimer(3, true, true);
            timer.OnBegin.AddListener(Voles);
            timer.OnEnd.AddListener(Stoats);

            timer.DescriptiveName = "FAAAAKKKEEE!!!!";
        }
    }

    private void Stoats()
    {
        Debug.Log("End!!!");
        Destroy(timer.gameObject);
    }

    private void Voles()
    {
        Debug.Log("Start!!!");
    }
}
