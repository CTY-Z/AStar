using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMAStar
{
    public class CMPath
    {
        public readonly Vector3[] array_lookPoint;
        public readonly CMLine[] array_turnBoundary;
        public readonly int finishLineIdx;
        public readonly int slowdownIdx;

        public CMPath(Vector3[] array_waypoint, Vector3 startPos, float turnDst, float stoppingDst)
        {
            array_lookPoint = array_waypoint;
            array_turnBoundary = new CMLine[array_lookPoint.Length];
            finishLineIdx = array_turnBoundary.Length - 1;

            Vector2 previousPoint = V3xzToV2(startPos);

            Debug.Log("array_lookPoint :" + array_lookPoint.Length);
            for (int i = 0; i < array_lookPoint.Length; i++)
            {
                Vector2 curPoint = V3xzToV2(array_lookPoint[i]);
                Vector2 dirToCurPoint = (curPoint - previousPoint).normalized;
                Vector2 turnBoundaryPoint = (i == finishLineIdx) ? curPoint : curPoint - dirToCurPoint * turnDst;
                array_turnBoundary[i] = new CMLine(turnBoundaryPoint, previousPoint - dirToCurPoint * turnDst);
                previousPoint = turnBoundaryPoint;
            }

            float dstFromEndPoint = 0.0f;

            for (int i = array_lookPoint.Length - 1; i > 0; i--)
            {
                dstFromEndPoint += Vector2.Distance(array_lookPoint[i], array_lookPoint[i - 1]);
                if (dstFromEndPoint > stoppingDst)
                {
                    slowdownIdx = i;
                    break;
                }
            }

        }

        Vector2 V3xzToV2(Vector3 v3)
        {
            return new Vector2(v3.x, v3.z);
        }

        public void DrawWithGizmos()
        {
            Gizmos.color = Color.black;
            foreach (Vector3 p in array_lookPoint)
                Gizmos.DrawCube(p + Vector3.up, Vector3.one);

            Gizmos.color = Color.white;
            foreach (CMLine l in array_turnBoundary)
                l.DrawWithGizmos(10);
        }
    }
}
