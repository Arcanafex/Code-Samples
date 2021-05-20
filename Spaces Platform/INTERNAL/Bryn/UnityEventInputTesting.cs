using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityEventInputTesting : MonoBehaviour
{
    public void DoSomething()
    {
        Debug.Log("Something");
    }

    public void DoSomethingWithBool(bool myBool)
    {
        Debug.Log("Bool: " + myBool);
    }

    public void DoSomethingWithInt(int myInt)
    {
        Debug.Log("Int: " + myInt);
    }

    public void DoSomethingWithFloat(float myFloat)
    {
        Debug.Log("Float: " + myFloat);
    }

    public void DoSomethingWithObject(Object myObject)
    {
        Debug.Log("Object: " + myObject.name);
    }

    public void DoSomethingWithWidget(Spaces.Core.Widget myWidget)
    {
        Debug.Log("Widget: " + myWidget.name);
    }
}
