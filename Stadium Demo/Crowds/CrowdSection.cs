using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TMPC
{
    public abstract class CrowdSection : MonoBehaviour
    {
        public Transform FrontStart;
        public Transform FrontEnd;
        public Transform BackStart;
        public Transform BackEnd;

        public int Rows = 1;
        public abstract Vector3[] Spots { get; }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(FrontStart.position, FrontEnd.position);
            Gizmos.DrawLine(BackStart.position, BackEnd.position);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(FrontStart.position, BackStart.position);
            Gizmos.DrawLine(FrontEnd.position, BackEnd.position);

            Gizmos.color = Color.green;
            foreach (var point in Spots)
            {
                Gizmos.DrawSphere(point, 0.1f);
            }
        }
    }
}
