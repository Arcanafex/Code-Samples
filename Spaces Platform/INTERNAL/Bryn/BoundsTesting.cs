using UnityEngine;
using System.Collections;
using Spaces.Extensions;

public class BoundsTesting : MonoBehaviour
{

    public Bounds localBounds;
    public Bounds worldBounds;

    // Use this for initialization
    void Start()
    {
        localBounds = transform.CalculateLocalBounds();
        worldBounds = transform.CalculateBounds();

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(transform.name + " [world Bounds] " + transform.CalculateBounds().size.x + ", " + transform.CalculateBounds().size.y + ", " + transform.CalculateBounds().size.z);
            Debug.Log(transform.name + " [local Bounds] " + localBounds.size.x + ", " + localBounds.size.y + ", " + localBounds.size.z);
            Debug.Log(transform.name + " [local scale] " + transform.localScale.x + ", " + transform.localScale.y + ", " + transform.localScale.z);
            Debug.Log(transform.name + " [world scale] " + transform.lossyScale.x + ", " + transform.lossyScale.y + ", " + transform.lossyScale.z);
        }
    }
}
