using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class InputTestRig : MonoBehaviour
{
    [System.Serializable]
    public class KeyInput
    {
        public KeyCode key;
        public UnityEvent OnKeyDown;
    }

    public KeyInput[] keys;

    void Update()
    {
        foreach (var input in keys)
        {
            if (Input.GetKeyDown(input.key))
            {
                input.OnKeyDown.Invoke();
            }
        }
    }
}
