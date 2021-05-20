using UnityEngine;
using System.Collections;

public class EchoToConsole : MonoBehaviour
{

    public void EchoAsLog(string statement)
    {
        Debug.Log(statement);
    }
}