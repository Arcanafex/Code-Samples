using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationControl : MonoBehaviour
{
    private Vector3 localEulers;

    private void OnEnable()
    {
        localEulers = transform.localEulerAngles;
    }

    public float RotationX
    {
        get => transform.localEulerAngles.x;

        set
        {
            localEulers = new Vector3(value, localEulers.y, localEulers.z);
            transform.localEulerAngles = localEulers;
        }
    }

    public float RotationY
    {
        get => transform.localEulerAngles.y;

        set
        {
            localEulers = new Vector3(localEulers.x, value, localEulers.z);
            transform.localEulerAngles = localEulers;
        }
    }

    public float RotationZ
    {
        get => transform.localEulerAngles.z;

        set
        {
            localEulers = new Vector3(localEulers.x, localEulers.y, value);
            transform.localEulerAngles = localEulers;
        }
    }
}
