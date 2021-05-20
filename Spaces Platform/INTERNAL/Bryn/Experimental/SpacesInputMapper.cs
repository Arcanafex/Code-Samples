using UnityEngine;
using System.Collections;

namespace Spaces.UnityClient
{
    public class SpacesInputMapper : MonoBehaviour
    {
        [System.Serializable]
        public class SpacesInput
        {
            public enum Type
            {
                Button2,    //normal button
                Button3,    //capacitive button (off, touching, pressed)
                Axis1,      //ranged float (0 or 1) or (-1 to 1)
                Axis2,      //vector2
                Axis3       //Vector3 (potential 3-dof input)
            }

            public string name;
            public int senderID;
            public int receiverID;
            public Type type;
        }

        void Start()
        {
            //poll for connected devices
        }

        void Update()
        {
            //update state of all connected devices
        }

    }
}
