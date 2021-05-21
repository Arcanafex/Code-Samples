using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIOptionsListener : MonoBehaviour
{
    public GameObject OptionsSource;

    private UnityEngine.UI.Dropdown m_dropdown;

    private void Start()
    {
        m_dropdown = GetComponent<UnityEngine.UI.Dropdown>();
    }
}
