using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TMPC
{
    public class CrowdGrid : CrowdSection
    {
        public float SpotSpacing = 1;

        public override Vector3[] Spots
        {
            get
            {
                var spots = new List<Vector3>();

                for (int r = 0; r < Rows; r++)
                {
                    float backness = r / (float)System.Math.Max(Rows - 1, 1);
                    var rowStart = Vector3.Lerp(FrontStart.position, BackStart.position, backness);
                    var rowEnd = Vector3.Lerp(FrontEnd.position, BackEnd.position, backness);
                    var rowSpotPlacement = (rowEnd - rowStart).normalized * SpotSpacing;

                    int rowSpots = (int)((rowEnd - rowStart).magnitude / SpotSpacing);

                    for (int s = 0; s <= rowSpots; s++)
                    {
                        spots.Add(rowStart + (rowSpotPlacement * s));
                    }
                }

                return spots.ToArray();
            }
        }
    }
}
