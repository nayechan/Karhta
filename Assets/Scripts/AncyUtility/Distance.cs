using UnityEngine;

namespace AncyUtility
{
    public static class Distance
    {
        public static int CalculateManhattanDistance(Vector2Int point1, Vector2Int point2)
        {
            int deltaX = Mathf.Abs(point1.x - point2.x);
            int deltaY = Mathf.Abs(point1.y - point2.y);
            return deltaX + deltaY;
        }
    }
}