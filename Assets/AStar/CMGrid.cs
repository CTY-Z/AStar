using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMAStar
{
    [System.Serializable]
    public class TerrainType
    {
        public LayerMask layerMask;
        public int terrainPenalty;
    }

    public class CMGrid : MonoBehaviour
    {
        public bool displayGridGizmos;
        //public Transform player;
        public LayerMask unwalkableMask;
        public Vector2 gridWorldSize;
        public float nodeRadius;
        public TerrainType[] array_walkableRegion;
        public int obstacleProximityPenalty = 10;

        public int maxSize { get { return gridSizeX * gridSizeY; } }

        CMNode[,] grid;
        float nodeDiameter;
        int gridSizeX, gridSizeY;
        private LayerMask walkableMask;
        private Dictionary<float, int> dic_walkableRegion = new();

        private int penaltyMin = int.MaxValue;
        private int penaltyMax = int.MinValue;

        private void Awake()
        {
            nodeDiameter = nodeRadius * 2;
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

            foreach (TerrainType type in array_walkableRegion)
            {
                walkableMask.value |= type.layerMask.value;
                dic_walkableRegion.Add(Mathf.Log(type.layerMask.value, 2), type.terrainPenalty);
            }

            CreateGrid();
        }

        private void CreateGrid()
        {
            grid = new CMNode[gridSizeX, gridSizeY];
            Vector3 worldBottomLeft = transform.position
                - (Vector3.right * gridWorldSize.x / 2)
                - (Vector3.forward * gridWorldSize.y / 2);

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius)
                        + Vector3.forward * (y * nodeDiameter + nodeRadius);

                    bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

                    int movementPenalty = 0;
                    Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100, walkableMask))
                        dic_walkableRegion.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);

                    if (!walkable)
                        movementPenalty += obstacleProximityPenalty;

                    grid[x, y] = new CMNode(walkable, worldPoint, x, y, movementPenalty);

                }
            }

            BlurPenaltyMap(3);
        }

        private void BlurPenaltyMap(int blurSize)
        {
            int kernelSize = blurSize * 2 + 1;
            int kernelExtents = (kernelSize - 1) / 2;

            int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
            int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

            for (int y = 0; y < gridSizeY; y++)
            {
                for (int x = -kernelExtents; x <= kernelExtents; x++)
                {
                    int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                    penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;
                }

                for (int x = 1; x < gridSizeX; x++)
                {
                    int removeIdx = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
                    int addIdx = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);

                    penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y]
                        - grid[removeIdx, y].movementPenalty + grid[addIdx, y].movementPenalty;
                }
            }

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = -kernelExtents; y <= kernelExtents; y++)
                {
                    int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                    penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
                }

                int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
                grid[x, 0].movementPenalty = blurredPenalty;

                for (int y = 1; y < gridSizeY; y++)
                {
                    int removeIdx = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
                    int addIdx = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);

                    penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1]
                        - penaltiesHorizontalPass[x, removeIdx] + penaltiesHorizontalPass[x, addIdx];

                    blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                    grid[x, y].movementPenalty = blurredPenalty;

                    if (blurredPenalty > penaltyMax)
                        penaltyMax = blurredPenalty;

                    if (blurredPenalty < penaltyMin)
                        penaltyMin = blurredPenalty;
                }
            }

        }

        public List<CMNode> GetNeighbours(CMNode node)
        {
            List<CMNode> list_neighbour = new();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                    if(checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                        list_neighbour.Add(grid[checkX, checkY]);
                }
            }

            return list_neighbour;
        }

        public CMNode GetNodeFormWorldPoint(Vector3 worldPos)
        {
            float percentX = (worldPos.x + gridWorldSize.x / 2) / gridWorldSize.x;
            float percentY = (worldPos.z + gridWorldSize.y / 2) / gridWorldSize.y;
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

            return grid[x, y];
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
            if (grid != null && displayGridGizmos)
            {
                //CMNode playerNode = GetNodeFormWorldPoint(player.position);
                foreach (CMNode node in grid)
                {
                    Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, node.movementPenalty));

                    Gizmos.color = node.walkable ? Gizmos.color : Color.red * 0.6f;
                    //if (node == playerNode)
                    //    Gizmos.color = Color.yellow * 0.6f;
                    Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter));
                }
            }
        }
    }
}


