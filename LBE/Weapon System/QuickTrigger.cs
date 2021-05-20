using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class QuickTrigger : MonoBehaviour
{
    public VRTK_ControllerEvents controller;
    public GunSystem gun;

    private bool menuAlreadyPressed;

    private void Start()
    {
        controller = GetComponentInParent<VRTK_ControllerEvents>();
        gun = GetComponentInChildren<GunSystem>();
    }

    private void Update()
    {
        if (gun)
        {
            if (controller)
            {
                gun.TriggerDown = controller.triggerPressed;

                if (controller.menuPressed)
                {
                    //Debug.Log("MENU BUTTON PRESSED");

                    if (!menuAlreadyPressed)
                    {
                        gun.Reload();
                        menuAlreadyPressed = true;
                    }
                }
                else
                {
                    menuAlreadyPressed = false;
                }
            }
            else
            {
                gun.TriggerDown = Input.GetKey(KeyCode.Space);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                gun.Reload();
            }
        }
    }
}