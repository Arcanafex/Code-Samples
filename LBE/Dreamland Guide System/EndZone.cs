using UnityEngine;
using System.Collections;

namespace Spaces.LBE
{
    public class EndZone : MonoBehaviour
    {

        protected virtual void OnEnable()
        {
            //SpacesEventManager.StartListening(EventNames.TELEPORT_TO_CLOUDENTRY, OnTeleportToCloudEntry);
            //SpacesEventManager.StartListening(EventNames.TELEPORT_TO_BRIDGE, OnTeleportToBridge);
            //SpacesEventManager.StartListening(EventNames.TELEPORT_TO_UNDERSEA, OnTeleportToUndersea);
            //SpacesEventManager.StartListening(EventNames.TELEPORT_TO_JUNGLE, OnTeleportToJungle);
            //SpacesEventManager.StartListening(EventNames.TELEPORT_TO_RUINS, OnTeleportToRuins);
            //SpacesEventManager.StartListening(EventNames.TELEPORT_TO_MOUNTAIN, OnTeleportToMountain);
            //SpacesEventManager.StartListening(EventNames.TELEPORT_TO_CLOUDEXIT, OnTeleportToCloudExit);
            //SpacesEventManager.StartListening(EventNames.TELEPORT_TO_POSTSHOW, OnTeleportToPostShow);
        }

        protected virtual void OnDisable()
        {
            //SpacesEventManager.StopListening(EventNames.TELEPORT_TO_CLOUDENTRY, OnTeleportToCloudEntry);
            //SpacesEventManager.StopListening(EventNames.TELEPORT_TO_BRIDGE, OnTeleportToBridge);
            //SpacesEventManager.StopListening(EventNames.TELEPORT_TO_UNDERSEA, OnTeleportToUndersea);
            //SpacesEventManager.StopListening(EventNames.TELEPORT_TO_JUNGLE, OnTeleportToJungle);
            //SpacesEventManager.StopListening(EventNames.TELEPORT_TO_RUINS, OnTeleportToRuins);
            //SpacesEventManager.StopListening(EventNames.TELEPORT_TO_MOUNTAIN, OnTeleportToMountain);
            //SpacesEventManager.StopListening(EventNames.TELEPORT_TO_CLOUDEXIT, OnTeleportToCloudExit);
            //SpacesEventManager.StopListening(EventNames.TELEPORT_TO_POSTSHOW, OnTeleportToPostShow);

        }
    }
}