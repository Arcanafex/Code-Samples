using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardCommands : MonoBehaviour
{
    private void Update()
    {
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.Q))
            Application.Quit();
    }

}
