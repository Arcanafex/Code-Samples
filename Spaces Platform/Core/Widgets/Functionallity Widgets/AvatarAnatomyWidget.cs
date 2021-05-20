using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces.Core
{
    public class AvatarAnatomyWidget : Widget
    {
        public enum Location
        {
            Head,
            LeftEye,
            RightEye,
            Eyes,
            LeftHand,
            RightHand,
            LeftFoot,
            RightFoot,
            Body,
            Other,
        }

        public Location part;
    }
}