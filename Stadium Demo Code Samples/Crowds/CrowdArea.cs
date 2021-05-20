using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TMPC
{
    public class CrowdArea : CrowdSection
    {
        public int RowSpots = 2;

        public override Vector3[] Spots
        {
            get
            {
                var spots = new Vector3[Rows * RowSpots];

                for (int r = 0; r < Rows; r++)
                {
                    float backness = r / (float)System.Math.Max(Rows - 1, 1);
                    var rowStart = Vector3.Lerp(FrontStart.position, BackStart.position, backness);
                    var rowEnd = Vector3.Lerp(FrontEnd.position, BackEnd.position, backness);

                    for (int s = 0; s < RowSpots; s++)
                    {
                        float endness = s / (float)System.Math.Max(RowSpots - 1, 1);
                        spots[s + (r * RowSpots)] = Vector3.Lerp(rowStart, rowEnd, endness);
                    }
                }

                return spots;
            }
        }
    }
}
