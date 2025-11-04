using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Unity.VisualScripting;
using CMCommon;
using System.Linq;
using System;

namespace CMAStar
{
    public class PathFinding : MonoBehaviour
    {
        CMGrid grid;

        private void Awake()
        {
            grid = GetComponent<CMGrid>();
        }

        public void FindPath(PathRequest request, Action<PathResult> callback)
        {
            Stopwatch sw = new();
            sw.Start();

            Vector3[] array_wayPoint = new Vector3[0];
            bool pathSuccess = false;

            CMNode startNode = grid.GetNodeFormWorldPoint(request.pathStart);
            CMNode targetNode = grid.GetNodeFormWorldPoint(request.pathEnd);

            if(startNode.walkable && targetNode.walkable)
            {
                CMHeap<CMNode> openSet = new CMHeap<CMNode>(grid.maxSize);
                HashSet<CMNode> closedSet = new HashSet<CMNode>();

                openSet.Add(startNode);

                while (openSet.Count > 0)
                {
                    CMNode curNode = openSet.RemoveFirst();

                    closedSet.Add(curNode);

                    if (curNode == targetNode)
                    {
                        sw.Stop();
                        CMDebug.Log("path find :" + sw.ElapsedMilliseconds + "ms");
                        pathSuccess = true;
                        break;
                    }

                    List<CMNode> list_neighbour = grid.GetNeighbours(curNode);
                    foreach (CMNode neighbour in list_neighbour)
                    {
                        if (!neighbour.walkable || closedSet.Contains(neighbour))
                            continue;

                        int newMovementCostToNeighbour = curNode.gCost + GetDistance(curNode, neighbour) + neighbour.movementPenalty;
                        if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                        {
                            neighbour.gCost = newMovementCostToNeighbour;
                            neighbour.hCost = GetDistance(neighbour, targetNode);
                            neighbour.parent = curNode;

                            if (!openSet.Contains(neighbour))
                                openSet.Add(neighbour);
                            else
                                openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }

            if (pathSuccess)
            {
                array_wayPoint = RetracePath(startNode, targetNode);
                pathSuccess = array_wayPoint.Length > 0;
            }

            callback(new PathResult(array_wayPoint, pathSuccess, request.callback));
        }

        private Vector3[] RetracePath(CMNode startNode, CMNode endNode)
        {
            List<CMNode> path = new();

            CMNode curNode = endNode;
            while (curNode != startNode)
            {
                path.Add(curNode);
                curNode = curNode.parent;
            }

            Vector3[] array_wayPoint = SimplifyPath(path);
            Array.Reverse(array_wayPoint);
            return array_wayPoint;
        }

        Vector3[] SimplifyPath(List<CMNode> list_path)
        {
            List<Vector3> list_waypoint = new();
            Vector2 directionOld = Vector2.zero;

            for (int i = 1; i < list_path.Count; i++)
            {
                int finalX = list_path[i - 1].gridX - list_path[i].gridX;
                int finalY = list_path[i - 1].gridY - list_path[i].gridY;

                Vector2 directionNew = new Vector2(finalX, finalY);
                if (directionOld != directionNew)
                    list_waypoint.Add(list_path[i].worldPosition);

                directionOld = directionNew;
            }

            return list_waypoint.ToArray();
        }

        private int GetDistance(CMNode nodeA, CMNode nodeB)
        {
            int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);

            return 14 * dstX + 10 * (dstY - dstX);
        }

    }
}

