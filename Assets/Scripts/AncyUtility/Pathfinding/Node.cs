using System;
using UnityEngine;

namespace AncyUtility.Pathfinding
{
    [Serializable]
    public class Node
    {
        public Vector2Int pos;
        public float gCost;
        public float rhs;
        [HideInInspector] public Node parent;

        public Node(Vector2Int pos)
        {
            this.pos = pos;
            this.gCost = float.MaxValue;
            this.rhs = float.MaxValue;
        }
    }

}