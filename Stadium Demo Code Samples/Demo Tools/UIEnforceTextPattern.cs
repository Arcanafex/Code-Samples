using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class UIEnforceTextPattern : MonoBehaviour
{
    public string RegexPattern;
    public UnityEngine.UI.InputField InputField;
    public Color WarningColor = Color.red;

    private Regex m_regex;
    private Color m_color;

    private void Start()
    {
        m_regex = new Regex($"^{RegexPattern}$");

        if (!InputField)
            InputField = GetComponent<UnityEngine.UI.InputField>();

        m_color = InputField?.textComponent?.color ?? Color.white;
    }

    public void EnforcePattern(string input)
    {
        if (m_regex.IsMatch(input))
        {
            InputField.textComponent.color = m_color;
        }
        else
        {
            Debug.LogWarning($"[{this.name}] Input value violates patter ({RegexPattern})");
            InputField.textComponent.color = WarningColor;
        }
    }
}
