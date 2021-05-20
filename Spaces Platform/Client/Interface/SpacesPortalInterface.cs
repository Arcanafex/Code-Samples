using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;



namespace Spaces.UnityClient
{
    public class SpacesPortalInterface : SpacesNavInterface
    {
        protected override void Space_Click(Spaces.Core.Space space)
        {
            var portal = space.SpawnPortal();
            portal.transform.position = transform.position;
            OnSpaceSelected.Invoke();
        }
    }
}