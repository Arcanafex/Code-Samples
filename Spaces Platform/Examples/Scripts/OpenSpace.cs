using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenSpace : MonoBehaviour
{
    public string mySpaceID;
    public Spaces.Core.Space mySpace;

    void Start()
    {
        mySpace = new Spaces.Core.Space(mySpaceID);
        mySpace.Enter();
    }
}
