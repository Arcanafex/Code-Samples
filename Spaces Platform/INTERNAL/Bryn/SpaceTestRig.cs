using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceTestRig : MonoBehaviour
{
    public UnityEngine.UI.Text spaceText;

    void Start()
    {
        Spaces.UnityClient.UserSession.Instance.SetCurrentSpace(new Spaces.Core.Space(System.Guid.NewGuid().ToString(), "Test Dummy"));
    }

    void Update()
    {
        spaceText.text = "Space: " + Spaces.UnityClient.UserSession.Instance.CurrentSpace.name + "\nID: " + Spaces.UnityClient.UserSession.Instance.CurrentSpace.id;


    }

}
