using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtrasProfileController : MonoBehaviour
{
    public Transform Props;

    public ExtrasProfile[] Profiles;

    [System.Serializable]
    public class ExtrasProfile
    {
        public string Description;
        public GameObject[] ActiveObjects;
    }

    private void Awake()
    {
        foreach (Transform child in Props)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void SetProfile(int extrasProfileIndex)
    {
        if (extrasProfileIndex > -1 && extrasProfileIndex < Profiles.Length)
        {
            foreach (Transform child in Props)
            {
                child.gameObject.SetActive(false);
            }

            var profile = Profiles[extrasProfileIndex];

            foreach (var obj in profile.ActiveObjects)
            {
                obj.SetActive(true);
            }
        }
    }
}
