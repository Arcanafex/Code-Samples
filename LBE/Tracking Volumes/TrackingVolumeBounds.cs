using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces.LBE
{
    public class TrackingVolumeBounds : MonoBehaviour
    {
        public Bounds CurrentBounds
        {
            get
            {
                return GetBounds();
            }
        }

        private Bounds GetBounds()
        {
            var bounds = new Bounds(transform.position, Vector3.zero);

            foreach (var collider in GetComponentsInChildren<Collider>())
            {
                bounds.Encapsulate(collider.bounds);
            }

            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.2f);
        }
    }
}
