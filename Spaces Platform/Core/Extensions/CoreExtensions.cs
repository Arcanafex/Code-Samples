using UnityEngine;
using System.Collections;

namespace Spaces.Extensions
{
    public static class CurveExtensions
    {
        public static void Set()
        {
        }
    }

    public static class GradientExtensions
    {
        public static void Set(this Gradient gradient, Color color)
        {
            gradient.alphaKeys = new GradientAlphaKey[2]
            {
                new GradientAlphaKey(color.a, 0),
                new GradientAlphaKey(color.a, 1)
            };

            gradient.colorKeys = new GradientColorKey[2]
            {
                new GradientColorKey(color, 0),
                new GradientColorKey(color, 1)
            };

            gradient.mode = GradientMode.Fixed;
        }

        public static void Set(this Gradient gradient, Color startColor, Color endColor)
        {
            gradient.alphaKeys = new GradientAlphaKey[2]
            {
                new GradientAlphaKey(startColor.a, 0),
                new GradientAlphaKey(endColor.a, 1)
            };

            gradient.colorKeys = new GradientColorKey[2]
            {
                new GradientColorKey(startColor, 0),
                new GradientColorKey(endColor, 1)
            };

            gradient.mode = GradientMode.Blend;
        }
    }
}