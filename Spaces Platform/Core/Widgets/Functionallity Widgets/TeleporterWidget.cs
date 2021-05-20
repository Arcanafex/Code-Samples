using UnityEngine;
using System.Linq;

namespace Spaces.Core
{
    public class TeleporterWidget : Widget
    {
        public void Teleport(string locationID)
        {
            FindObjectsOfType<LocationWidget>().ToList().ForEach(l => l.Deactivate());

            LocationWidget location = FindObjectsOfType<LocationWidget>().FirstOrDefault(l => l.InstanceID == locationID);

            if (location)
                location.Activate();
            else
                Debug.Log(this.name + " [Teleport Failed] Location " + locationID + " not found.");
        }
    }
}