using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIValueDisplayer : MonoBehaviour
{
    public bool IsPercentile;
    private UnityEngine.UI.Text m_text;

    private void Start()
    {
        m_text = GetComponent<UnityEngine.UI.Text>();
    }

    public void UpdateDisplay(float value)
    {
        if (m_text)
            m_text.text = IsPercentile ? $"{(value * 100):N0}%" : value.ToString();
    }
}
