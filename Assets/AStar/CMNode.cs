using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMAStar
{
    public class CMNode : ICMHeapItem<CMNode>
    {
        public bool walkable;
        public Vector3 worldPosition;

        public int gridX;
        public int gridY;
        public int movementPenalty;

        public int gCost;
        public int hCost;
        public int fCost 
        {
            get { return gCost + hCost; }
            private set { }
        }

        private int _heapIdx;
        public int heapIdx { get { return _heapIdx; } set { _heapIdx = value; } }

        public CMNode parent;

        public CMNode(bool walkable, Vector3 worldPos, int gridX, int gridY, int movementPenalty)
        {
            this.walkable = walkable;
            this.worldPosition = worldPos;
            this.gridX = gridX;
            this.gridY = gridY;
            this.movementPenalty = movementPenalty;
        }

        public int CompareTo(CMNode other)
        {
            int compare = fCost.CompareTo(other.fCost);

            if (compare == 0)
                compare = hCost.CompareTo(other.hCost);

            return -compare;
        }
    }
}

