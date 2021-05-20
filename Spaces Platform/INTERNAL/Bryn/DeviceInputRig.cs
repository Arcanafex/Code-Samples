using UnityEngine;
using System.Collections;
using Valve.VR;

public class DeviceInputRig : MonoBehaviour
{
    public int tick;

    public SteamVR_Controller.Device device;
    public SteamVR_TrackedObject trackedObj;

    public uint controllerIndex;

    public Vector2 touchpad;
    public float trigger;

    [Space]
    public bool triggerPressed = false;
    public bool triggerUp = false;
    public bool triggerDown = false;
    [Space]
    public bool menuPressed = false;
    public bool menuUp = false;
    public bool menuDown = false;
    [Space]
    public bool gripped = false;
    public bool gripUp = false;
    public bool gripDown = false;
    [Space]
    public bool padPressed = false;
    public bool padPressUp = false;
    public bool padPressDown = false;
    [Space]
    public bool padTouched = false;
    public bool padTouchUp = false;
    public bool padTouchDown = false;


    void Start()
    {
        InitController();
    }

    void InitController()
    {
        trackedObj = GetComponentInParent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)trackedObj.index);
        controllerIndex = device.index;
        Debug.Log("Device index: " + device.index + " [valid: " + device.valid + "]");
    }

    void Update()
    {
        if (device == null)
            InitController();

        if (device != null && device.valid)
        {
            ++tick;

            //device.GetHairTriggerDown();

            touchpad = device.GetAxis();
            trigger = device.GetAxis(EVRButtonId.k_EButton_SteamVR_Trigger).x;

            menuPressed = device.GetPress(EVRButtonId.k_EButton_ApplicationMenu);
            menuUp = device.GetPressUp(EVRButtonId.k_EButton_ApplicationMenu);
            menuDown = device.GetPressDown(EVRButtonId.k_EButton_ApplicationMenu);

            gripped = device.GetPress(EVRButtonId.k_EButton_Grip);
            gripUp = device.GetPressUp(EVRButtonId.k_EButton_Grip);
            gripDown = device.GetPressDown(EVRButtonId.k_EButton_Grip);

            triggerPressed = device.GetPress(EVRButtonId.k_EButton_SteamVR_Trigger);
            triggerUp = device.GetPressUp(EVRButtonId.k_EButton_SteamVR_Trigger);
            triggerDown = device.GetPressDown(EVRButtonId.k_EButton_SteamVR_Trigger);

            padPressed = device.GetPress(EVRButtonId.k_EButton_SteamVR_Touchpad);
            padPressUp = device.GetPressUp(EVRButtonId.k_EButton_SteamVR_Touchpad);
            padPressDown = device.GetPressDown(EVRButtonId.k_EButton_SteamVR_Touchpad);

            padTouched = device.GetTouch(EVRButtonId.k_EButton_SteamVR_Touchpad);
            padTouchUp = device.GetTouchUp(EVRButtonId.k_EButton_SteamVR_Touchpad);
            padTouchDown = device.GetTouchDown(EVRButtonId.k_EButton_SteamVR_Touchpad);

            if (menuPressed || menuUp || menuDown) Debug.Log("Menu: [" + tick + "]" + menuPressed + "|" + menuUp + "|" + menuDown);
            if (gripped || gripUp || gripDown) Debug.Log("Grip: [" + tick + "]" + gripped + "|" + gripUp + "|" + gripDown);
            if (triggerPressed || triggerUp || triggerDown) Debug.Log("Trigger [" + tick + "]: " + triggerPressed + "|" + triggerUp + "|" + triggerDown);
            if (padPressed || padPressUp || padPressDown) Debug.Log("Pad Press [" + tick + "]: " + padPressed + "|" + padPressUp + "|" + padPressDown);
            if (padTouched || padTouchUp || padTouchDown) Debug.Log("Pad Touch [" + tick + "]: " + padTouched + "|" + padTouchUp + "|" + padTouchDown);
        }   
        else
        {
            device = SteamVR_Controller.Input((int)GetComponent<SteamVR_TrackedObject>().index);
        }

    }
}
