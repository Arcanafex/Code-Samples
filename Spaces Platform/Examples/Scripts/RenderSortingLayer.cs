using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class RenderSortingLayer : MonoBehaviour
{
    public string sortingLayer;
    public int orderInLayer;

    void Update()
    {
        Renderer rend = GetComponent<Renderer>();

        if (rend != null)
        {
            rend.sortingLayerName = sortingLayer;
            rend.sortingOrder = orderInLayer;
        }
    }


}
