using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spaces.Core;
using System;

public class EvenetListener : MonoBehaviour, IEditable
{
    public UnityEngine.UI.Text displayLabel;
    public bool inEditMode;

    void Start()
    {
        inEditMode = false;
    }

    void Update()
    {
        displayLabel.text = inEditMode ? "EDIT MODE!!!" : "NOT EDIT (i.e \"Play\")";
    }

    public void OnEditEnd()
    {
        inEditMode = false;
    }

    public void OnEditStart()
    {
        inEditMode = true;
    }
}
