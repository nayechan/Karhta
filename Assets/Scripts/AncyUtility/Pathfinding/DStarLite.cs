using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using InGame.Chunk;
using UnityEngine;

namespace AncyUtility.Pathfinding
{
    public class DStarLite
    {
        private Dictionary<Vector2Int, Node> nodes;
        public Node startNode;
        public Node goalNode;

        private PriorityQueue<Node> openList;
        private HashSet<Node> openSet;
        private HashSet<Node> closedList;
        private Node lastNode;
        private int maxDistance;
        private int maxHeight;
        private List<Node> path;

        public DStarLite(Vector3 startPos, Vector3 endPos, int _maxDistance, int _maxHeight)
        {
            nodes = new Dictionary<Vector2Int, Node>();
            openList = new PriorityQueue<Node>((a, b) =>
            {
                var keyA = CalculateKey(a);
                var keyB = CalculateKey(b);

                return CompareKey(keyA, keyB);
            });
            openSet = new HashSet<Node>();
            closedList = new HashSet<Node>();

            maxDistance = _maxDistance;
            maxHeight = _maxHeight;

            var _startPos = GetVec2IntPos(startPos);
            var _endPos = GetVec2IntPos(endPos);
            
            startNode = GetNode(_startPos, false);
            goalNode = GetNode(_endPos, false);

            if (startNode == null || goalNode == null)
            {
                throw new ArgumentException("Start or goal node is out of range");
            }

            startNode.gCost = float.MaxValue;
            startNode.rhs = 0;
            goalNode.gCost = float.MaxValue;
            goalNode.rhs = Heuristic(startNode, goalNode);

            openList.Enqueue(goalNode);
            openSet.Add(goalNode);
            lastNode = startNode;
        }

        Vector2 CalculateKey(Node node)
        {
            float minCost = Mathf.Min(node.gCost, node.rhs);
            return new Vector2(minCost + Heuristic(node, startNode), minCost);
        }

        int CompareKey(Vector2 a, Vector2 b)
        {
            if (Mathf.Abs(a.x - b.x) > Mathf.Epsilon)
            {
                return a.x.CompareTo(b.x);
            }

            return a.y.CompareTo(b.y);
        }

        float Heuristic(Node a, Node b)
        {
            Vector2 diff = a.pos - b.pos;
            return Mathf.Abs(diff.x) +  Mathf.Abs(diff.y);
        }

        public void UpdateObstacles()
        {
            foreach (var node in path)
            {
                if(node.pos != startNode.pos && node.pos != goalNode.pos)
                    UpdateMap(node);
            }
            CalculatePath();
        }

        public void UpdateMap(Node changedNode)
        {
            if (changedNode != goalNode)
            {
                changedNode.rhs = float.MaxValue;
                foreach (Node neighbor in GetNeighbors(changedNode))
                {
                    changedNode.rhs = Mathf.Min(changedNode.rhs, GetCost(changedNode, neighbor) + neighbor.gCost);
                }
            }
            UpdateNode(changedNode);
        }

        void UpdateNode(Node node)
        {
            if (openSet.Contains(node))
            {
                openList.Remove(node);
                openSet.Remove(node);
            }

            if (Mathf.Abs(node.gCost - node.rhs) > Mathf.Epsilon)
            {
                openList.Enqueue(node);
                openSet.Add(node);
            }
        }

        Node GetNode(Vector2Int pos, bool checkDistance = true)
        {
            if (nodes.ContainsKey(pos))
            {
                return nodes[pos];
            }

            if (!checkDistance || (startNode != null && Vector2Int.Distance(pos, startNode.pos) < maxDistance))
            {
                var node = new Node(pos);
                nodes[pos] = node;

                return node;
            }

            return null;
        }

        List<Node> GetNeighbors(Node node)
        {
            List<Node> neighbors = new List<Node>();

            Vector2Int[] directions =
            {
                Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up,
                Vector2Int.left + Vector2Int.up, Vector2Int.up + Vector2Int.right,
                Vector2Int.right + Vector2Int.down, Vector2Int.down + Vector2Int.left
            };

            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighborPosition = node.pos + direction;

                var neighborNode = GetNode(neighborPosition);
                if (neighborNode != null)
                    neighbors.Add(neighborNode);
            }

            return neighbors;
        }

        private float GetHeightFromTerrain(Vector2Int _position)
        {
            
            var origin = new Vector3(_position.x, maxHeight, _position.y); // Starting from the ground level
            RaycastHit hit;
            
            int groundLayerMask = 1 << LayerMask.NameToLayer("Ground");

            // Perform the raycast from the ground up
            if (Physics.Raycast(origin, Vector3.down, out hit, maxHeight, groundLayerMask))
            {
                return hit.point.y;
            }
            
            return 0;
        }

        private float GetCost(Node from, Node to)
        {
            RaycastHit hit;

            var yPos = GetHeightFromTerrain(to.pos);
            var position = new Vector3(to.pos.x, yPos, to.pos.y);

            const float sphereRadius = 1.5f;
            int layerMask = 1 << LayerMask.NameToLayer("Water");
            layerMask |= 1 << LayerMask.NameToLayer("Obstacle");
            layerMask |= 1 << LayerMask.NameToLayer("Tree");

            if (Physics.SphereCast(
                    position + Vector3.up * sphereRadius,
                    sphereRadius,
                    Vector3.up,
                    out hit,
                    2 * sphereRadius,
                    layerMask))
            {
                return float.PositiveInfinity; // Set obstacle cost to infinity
            }

            return Vector2Int.Distance(from.pos, to.pos);
        }

        public void ComputeShortestPath()
        {
            while (
                CompareKey(CalculateKey(openList.Peek()), CalculateKey(startNode)) <= 0 || 
                Mathf.Abs(startNode.rhs - startNode.gCost) > Mathf.Epsilon)
            {
                Node current = openList.Dequeue();
                openSet.Remove(current);

                if (current.gCost > current.rhs)
                {
                    current.gCost = current.rhs;
                    foreach (Node neighbor in GetNeighbors(current))
                    {
                        if (neighbor != goalNode)
                        {
                            neighbor.rhs = Mathf.Min(neighbor.rhs, GetCost(current, neighbor) + current.gCost);
                        }
                        UpdateNode(neighbor);
                    }
                }
                else
                {
                    current.gCost = float.MaxValue;
                    foreach (Node neighbor in GetNeighbors(current))
                    {
                        if (neighbor != goalNode)
                        {
                            neighbor.rhs = Mathf.Min(neighbor.rhs, GetCost(current, neighbor) + neighbor.gCost);
                        }
                        UpdateNode(neighbor);
                    }
                    UpdateNode(current);
                }
            }
        }

        public List<Node> CalculatePath()
        {
            ComputeShortestPath();
            path = new List<Node>();
            var visition = new HashSet<Vector2Int>();
            
            Node currentNode = startNode;

            while (currentNode != goalNode)
            {
                if(currentNode != startNode)
                    path.Add(currentNode);
                visition.Add(currentNode.pos);
                currentNode = GetNextNode(currentNode, visition);
                if (currentNode == null)
                {
                    path = null;
                    throw new Exception("Path not found");
                }
                Debug.Log(currentNode.pos);

            }
            path.Add(goalNode);

            return path;
        }

        private Node GetNextNode(Node currentNode, HashSet<Vector2Int> visition)
        {
            Node nextNode = null;
            float minCost = float.MaxValue;

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                float cost = GetCost(currentNode, neighbor) + neighbor.gCost;
                if (cost < minCost && !visition.Contains(neighbor.pos))
                {
                    minCost = cost;
                    nextNode = neighbor;
                }
            }

            return nextNode;
        }

        public List<Node> GetPath()
        {
            return path;
        }

        public Vector2Int GetVec2IntPos(Vector3 pos)
        {
            return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
        }


    }
}
