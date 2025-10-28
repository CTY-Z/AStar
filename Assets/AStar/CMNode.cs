using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMAStar
{
    public class CMNode
    {
        public bool walkable;
        public Vector3 worldPosition;

        public CMNode(bool walkable, Vector3 worldPos)
        {
            this.walkable = walkable;
            this.worldPosition = worldPos;
        }
    }
}

