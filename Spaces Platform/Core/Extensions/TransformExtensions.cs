using UnityEngine;

namespace Spaces.Extensions
{
    /// <summary>
    /// Extension methods for Unity's transform.
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Set the world Z position.
        /// </summary>
        /// <param name="argTransform">Transform to change position of.</param>
        /// <param name="argX">The new world Z coordinate.</param>
        public static void SetPositionX(this Transform argTransform, float argX)
        {
            TransformExtensions.SetPosition(argTransform, argX, argTransform.position.y, argTransform.position.z);
        }

        /// <summary>
        /// Set the world Y position.
        /// </summary>
        /// <param name="argTransform">Transform to change position of.</param>
        /// <param name="argY">The new world Y coordinate.</param>
        public static void SetPositionY(this Transform argTransform, float argY)
        {
            TransformExtensions.SetPosition(argTransform, argTransform.position.x, argY, argTransform.position.z);
        }

        /// <summary>
        /// Set the world Z position.
        /// </summary>
        /// <param name="argTransform">Transform to change position of.</param>
        /// <param name="argZ">The new world Z coordinate.</param>
        public static void SetPositionZ(this Transform argTransform, float argZ)
        {
            TransformExtensions.SetPosition(argTransform, argTransform.position.x, argTransform.position.y, argZ);
        }

        /// <summary>
        /// Set the world position.
        /// </summary>
        /// <param name="argTransform">Transform to change position of.</param>
        /// <param name="argX">The new world X coordinate.</param>
        /// <param name="argY">The new world Y coordinate.</param>
        /// <param name="argZ">The new world Z coordinate.</param>
        public static void SetPosition(this Transform argTransform, float argX, float argY, float argZ)
        {
            argTransform.position = new Vector3(argX, argY, argZ);
        }

        /// <summary>
        /// Set the local X position.
        /// </summary>
        /// <param name="argTransform">Transform to change position of.</param>
        /// <param name="argX">The new local X coordinate.</param>
        public static void SetLocalPositionX(this Transform argTransform, float argX)
        {
            TransformExtensions.SetPosition(argTransform, argX, argTransform.localPosition.y, argTransform.localPosition.z);
        }

        /// <summary>
        /// Set the local Y position.
        /// </summary>
        /// <param name="argTransform">Transform to change position of.</param>
        /// <param name="argY">The new local Y coordinate.</param>
        public static void SetLocalPositionY(this Transform argTransform, float argY)
        {
            TransformExtensions.SetPosition(argTransform, argTransform.localPosition.x, argY, argTransform.localPosition.z);
        }

        /// <summary>
        /// Set the local Z position.
        /// </summary>
        /// <param name="argTransform">Transform to change position of.</param>
        /// <param name="argZ">The new local Z coordinate.</param>
        public static void SetLocalPositionZ(this Transform argTransform, float argZ)
        {
            TransformExtensions.SetPosition(argTransform, argTransform.localPosition.x, argTransform.localPosition.y, argZ);
        }

        /// <summary>
        /// Set the local position.
        /// </summary>
        /// <param name="argTransform">Transform to change position of.</param>
        /// <param name="argX">The new local X coordinate.</param>
        /// <param name="argY">The new local Y coordinate.</param>
        /// <param name="argZ">The new local Z coordinate.</param>
        public static void SetLocalPosition(this Transform argTransform, float argX, float argY, float argZ)
        {
            argTransform.localPosition = new Vector3(argX, argY, argZ);
        }

        /// <summary>
        /// Destroys all children of the transform.
        /// </summary>
        /// <param name="argTransform">The transform that will have its children destroyed.</param>
        /// <param name="argDestroyImmediately">If the children should be destroyed immediately.</param>
        public static void DestroyChildren(this Transform argTransform, bool argDestroyImmediately = false)
        {
            foreach (Transform child in argTransform)
            {
                try
                {
                    if (argDestroyImmediately == true)
                    {
                        Transform.DestroyImmediate(child.gameObject);
                    }
                    else
                    {
                        Transform.Destroy(child.gameObject);
                    }
                }
                catch { }
            }
        }

        public static Bounds CalculateLocalBounds(this Transform transform)
        {
            var bounds = transform.CalculateBounds();

            Vector3 worldScale = transform.lossyScale;

            var localSize = new Vector3(
                bounds.size.x / worldScale.x,
                bounds.size.y / worldScale.y,
                bounds.size.z / worldScale.z);

            var localCenter = new Vector3(
                bounds.center.x / worldScale.x,
                bounds.center.y / worldScale.y,
                bounds.center.z / worldScale.z);

            bounds.size = localSize;
            bounds.center = localCenter;

            return bounds;
        }

        public static Bounds CalculateBounds(this Transform transform)
        {
            Quaternion currentRotation = transform.rotation;
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            Bounds bounds = new Bounds(transform.position, Vector3.zero);

            foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }

            foreach (var rectTransform in transform.GetComponentsInChildren<RectTransform>())
            {
                var worldCorners = new Vector3[4];
                rectTransform.GetWorldCorners(worldCorners);

                foreach (var corner in worldCorners)
                {
                    bounds.Encapsulate(corner);
                }
            }

            Vector3 localCenter = bounds.center - transform.position;
            bounds.center = localCenter;

            //Debug.Log("Bounds center: " + bounds.center + " | Bounds size: " + bounds.size);

            transform.rotation = currentRotation;

            return bounds;
        }
    }
}