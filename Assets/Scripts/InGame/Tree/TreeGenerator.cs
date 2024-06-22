using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace InGame.Tree
{
    [Serializable]
    public struct TreeGeneratorData
    {
        public GameObject prefab;
        public int weight;
        public int minHeight;
        public int maxHeight;
    }
    
    public class TreeGenerator : MonoBehaviour
    {
        public static TreeGenerator Instance { get; private set; }

        public TreeGeneratorData[] treeGenerationDataset;
        public int distanceWithOther = 8;

        private Dictionary<Vector2Int, List<Tree>> trees;

        private const int chunkSize = 256;
        [SerializeField] private float minHeight = 144.0f;
        [SerializeField] private float maxHeight = 256.0f;
        private int treeGenerationAttemptCount = 4;
        private int batchSize = 32;

        private GameObject GetTree(float height)
        {
            var filteredDataset = treeGenerationDataset
                .Where(x => x.minHeight <= height && height <= x.maxHeight).ToList();
            
            var totalWeight = filteredDataset.Sum(x => x.weight);

            if (totalWeight <= 0) return null;
            
            var randomValue = Random.Range(0, totalWeight);

            foreach (var treeGenerationData in filteredDataset)
            {
                if (randomValue < treeGenerationData.weight)
                    return treeGenerationData.prefab;

                randomValue -= treeGenerationData.weight;
            }

            return null;
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this.gameObject);

            Init();
        }

        private void Init()
        {
            trees = new Dictionary<Vector2Int, List<Tree>>();
            
            string[] files = Directory.GetFiles(Application.persistentDataPath, "treeData*");
        
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }

        public async UniTaskVoid GenerateTrees(Vector2Int chunkPos)
        {
            var treePoints = GetTreePoints(chunkPos);
            int operationCount = 0;

            foreach (var treePoint in treePoints)
            {
                GameObject prefabToGenerate = null;
                if (treeGenerationDataset.Length > 0)
                    prefabToGenerate = GetTree(treePoint.y);

                if (prefabToGenerate != null)
                {
                    var treeInstance = Instantiate(
                        prefabToGenerate,
                        treePoint,
                        Quaternion.identity,
                        transform);

                    var treeComponent = treeInstance.GetComponent<Tree>();

                    if (!trees.ContainsKey(chunkPos))
                        trees[chunkPos] = new List<Tree>();

                    trees[chunkPos].Add(treeComponent);
                }

                ++operationCount;

                if(operationCount % batchSize == 0)
                    await UniTask.Yield();
            }
        }

        public async UniTaskVoid UnloadTrees(Vector2Int chunkPos)
        {
            int operationCount = 0;
            
            if (!trees.ContainsKey(chunkPos)) return;

            foreach (var tree in trees[chunkPos])
            {
                Destroy(tree.gameObject);
                
                ++operationCount;
                
                if(operationCount % batchSize == 0)
                    await UniTask.Yield();
            }

            trees.Remove(chunkPos);
        }

        public void RemoveTree(Tree tree)
        {
            var pos = tree.transform.position;
            var chunkX = Mathf.FloorToInt(pos.x / chunkSize);
            var chunkZ = Mathf.FloorToInt(pos.z / chunkSize);
            var chunkPos = new Vector2Int(chunkX, chunkZ);

            var filePath = 
                string.Format("{0}/treeData_{1}_{2}.dat", Application.persistentDataPath, chunkX, chunkZ);

            if (trees.ContainsKey(chunkPos))
            {
                trees[chunkPos].Remove(tree);
                Destroy(tree.gameObject);

                var treePosList = trees[chunkPos].Select(treePos => treePos.transform.position).ToList();
                SaveTreePointsToFile(filePath, treePosList);
            }
        }

        List<Vector3> GetTreePoints(Vector2Int chunkPos)
        {
            var filePath = 
                string.Format("{0}/treeData_{1}_{2}.dat", Application.persistentDataPath, chunkPos.x, chunkPos.y);
            if (File.Exists(filePath))
            {
                return ReadTreePointsFromFile(filePath);
            }
            else
            {
                List<Vector3> treePoints = GenerateTreePoints(chunkPos);
                SaveTreePointsToFile(filePath, treePoints);
                return treePoints;
            }
        }

        private void SaveTreePointsToFile(string filePath, List<Vector3> treePoints)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var point in treePoints)
                {
                    writer.WriteLine($"{point.x},{point.y},{point.z}");
                }
            }
        }

        private List<Vector3> ReadTreePointsFromFile(string filePath)
        {
            List<Vector3> treePoints = new List<Vector3>();
            string[] lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                string[] parts = line.Split(',');
                float x = float.Parse(parts[0]);
                float y = float.Parse(parts[1]);
                float z = float.Parse(parts[2]);
                treePoints.Add(new Vector3(x, y, z));
            }
            return treePoints;
        }
        
        private float GetHeightFromTerrain(Vector2 _position)
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

        private bool IsValidPoint(Vector2 point, Vector2Int size, float cellSize, 
            float radius, Vector2[,] grid, Vector2 offset)
        {
            if (point.x < 0 || point.x >= size.x || point.y < 0 || point.y >= size.y)
            {
                return false;
            }

            var height = GetHeightFromTerrain(point + offset);
            if (minHeight > height || height > maxHeight)
            {
                return false;
            }

            int gridX = Mathf.FloorToInt(point.x / cellSize);
            int gridY = Mathf.FloorToInt(point.y / cellSize);

            int minX = Mathf.Max(0, gridX - 2);
            int maxX = Mathf.Min(gridX + 2, grid.GetLength(0) - 1);
            int minY = Mathf.Max(0, gridY - 2);
            int maxY = Mathf.Min(gridY + 2, grid.GetLength(1) - 1);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (grid[x, y] != Vector2.zero)
                    {
                        float distance = Vector2.Distance(point, grid[x, y]);
                        if (distance < radius)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private List<Vector3> GenerateTreePoints(Vector2Int chunkPos)
        {
            List<Vector3> treePoints = new List<Vector3>();
            var offset = 
                chunkPos.x * chunkSize * Vector3.right + chunkPos.y * chunkSize * Vector3.forward;

            // Parameters for Poisson Disk Sampling
            float radius = distanceWithOther;
            float cellSize = radius / Mathf.Sqrt(2);

            int gridWidth = Mathf.CeilToInt(chunkSize / cellSize);
            int gridHeight = Mathf.CeilToInt(chunkSize / cellSize);

            Vector2[,] grid = new Vector2[gridWidth, gridHeight];
            List<Vector2> activeList = new List<Vector2>();

            // Generate first point
            Vector2 firstPoint;
            int sampleTryCount = Mathf.Max(treeGenerationAttemptCount * 4, 128);
            
            do
            {
                firstPoint = new Vector2(Random.Range(0, chunkSize), Random.Range(0, chunkSize));
                --sampleTryCount;
            } while (
                sampleTryCount > 0 &&
                !IsValidPoint(firstPoint, chunkSize * Vector2Int.one, cellSize, 
                    radius, grid, chunkSize * chunkPos)
            );

            if (sampleTryCount == 0)
            {
                return treePoints;
            }
            
            activeList.Add(firstPoint);
            int firstGridX = Mathf.FloorToInt(firstPoint.x / cellSize);
            int firstGridY = Mathf.FloorToInt(firstPoint.y / cellSize);
            grid[firstGridX, firstGridY] = firstPoint;

            while (activeList.Count > 0)
            {
                int activeIndex = Random.Range(0, activeList.Count);
                Vector2 activePoint = activeList[activeIndex];
                bool pointFound = false;

                for (int k = 0; k < treeGenerationAttemptCount; k++)
                {
                    float angle = Random.Range(0f, Mathf.PI * 2);
                    float distance = Random.Range(radius, 2 * radius);
                    Vector2 newPoint = activePoint + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;

                    if (IsValidPoint(newPoint, chunkSize * Vector2Int.one, cellSize, radius, 
                            grid, chunkSize * chunkPos))
                    {
                        activeList.Add(newPoint);
                        int gridX = Mathf.FloorToInt(newPoint.x / cellSize);
                        int gridY = Mathf.FloorToInt(newPoint.y / cellSize);
                        grid[gridX, gridY] = newPoint;
                        treePoints.Add(offset + new Vector3(
                           newPoint.x, 
                           GetHeightFromTerrain(newPoint+chunkSize * chunkPos), 
                           newPoint.y
                        ));
                        pointFound = true;
                        break;
                    }
                }

                if (!pointFound)
                {
                    activeList.RemoveAt(activeIndex);
                }
            }

            return treePoints;
        }
    }
}
