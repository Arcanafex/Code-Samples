using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DebugWindow : MonoBehaviour
{
    public Text text;
    public Queue<string> buffer;
    

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
        buffer = new Queue<string>();
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
        buffer = null;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Log)
        {
            if (buffer.Count > 24)
            {
                buffer.Dequeue();
            }

            buffer.Enqueue(logString);

            text.text = string.Join("\n", buffer.ToArray());
        }
    }
}
