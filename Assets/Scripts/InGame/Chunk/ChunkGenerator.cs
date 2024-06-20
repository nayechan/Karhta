using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AncyUtility;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;

namespace InGame.Chunk
{
    public class ChunkGenerator : MonoBehaviour
    {
        [SerializeField] private ChunkGenerationConfiguration config;
        [SerializeField] private int renderDistance;
        [SerializeField] private Player.Player player;

        private Vector3 lastPlayerPosition;
        private bool isPlayerSpawned;

        private Dictionary<Vector2Int, JobHandle> chunkGenerationJobs;
        private Dictionary<Vector2Int, NativeArray<float>> chunkGenerationResult;
        private PriorityQueue<RawHeightmap> heightmapQueue;
        
        private Dictionary<Vector2Int, Chunk> loadedChunks;
        private Dictionary<Vector2Int, Water> loadedWaters;

        private int batchCount = 1;

        struct RawHeightmap
        {
            public Vector2Int pos;
            public NativeArray<float> data;

            public RawHeightmap(Vector2Int _pos, NativeArray<float> _data)
            {
                pos = _pos;
                data = _data;
            }
        }

        private void Awake()
        {
            loadedChunks = new Dictionary<Vector2Int, Chunk>();
            loadedWaters = new Dictionary<Vector2Int, Water>();
            chunkGenerationJobs = new Dictionary<Vector2Int, JobHandle>();
            chunkGenerationResult = new Dictionary<Vector2Int, NativeArray<float>>();
            heightmapQueue = new PriorityQueue<RawHeightmap>((a, b) =>
            {
                var playerChunkPos = player.GetPlayerChunkPos(config.chunkSize);
                var distA = Distance.CalculateManhattanDistance(a.pos, playerChunkPos);
                var distB = Distance.CalculateManhattanDistance(b.pos, playerChunkPos);

                return distA - distB;
            });
            isPlayerSpawned = false;
        }

        // Start is called before the first frame update
        private void Start()
        {
            lastPlayerPosition = player.transform.position;
            //InitChunkGenerator().Forget();
        }

        public async UniTask InitChunkGenerator()
        {
            for (int x = -renderDistance; x <= renderDistance; ++x)
            {
                for (int z = -renderDistance; z <= renderDistance; ++z)
                {
                    GenerateChunk(new Vector2Int(x, z));
                }
            }

            await UniTask.WaitUntil(() =>
            {
                return isPlayerSpawned && IsChunkLoaded(player.GetPlayerChunkPos(config.chunkSize), 2);
            });
            
            player.SetIsGravityPresent(true);
        }

        public bool IsChunkLoaded(Vector2Int chunkPos, int distance=1)
        {
            for (int dx = -distance + 1; dx <= distance - 1; ++dx)
            {
                for (int dy = -distance + 1; dy <= distance - 1; ++dy)
                {
                    if (!loadedChunks.ContainsKey(chunkPos + dx * Vector2Int.right + dy * Vector2Int.up))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        
        private JobHandle GenerateChunk(Vector2Int chunkPos)
        {
            var heightmap = new NativeArray<float>(
                (config.chunkSize + 1) * (config.chunkSize + 1), Allocator.Persistent
            );
                    
            chunkGenerationResult.Add(chunkPos, heightmap);
                    
            ChunkGenerationJob chunkGenerationJob = new ChunkGenerationJob{
                heightmap = heightmap,
                chunkPos = chunkPos,
                config = config
            };

            JobHandle handle = chunkGenerationJob.Schedule();
            chunkGenerationJobs.Add(chunkPos, handle);

            return handle;
        }

        private RectInt CalculateChunkGenerationRect(Vector2Int playerPos)
        {
            return new RectInt(
                playerPos - renderDistance * Vector2Int.one,
                (renderDistance * 2 + 1) * Vector2Int.one
            );
        }

        private void UpdateChunks()
        {
            var playerChunkPos = player.GetPlayerChunkPos(config.chunkSize);

            var playerRect = CalculateChunkGenerationRect(playerChunkPos);

            var chunksToRemove = new List<Vector2Int>();
            
            // Update Chunks in the list
            foreach (var chunk in loadedChunks)
            {
                bool result = chunk.Value.UpdateChunk(playerRect);
                if (!result)
                {
                    chunksToRemove.Add(chunk.Key);
                }
            }

            foreach (var chunkPos in chunksToRemove)
            {
                loadedChunks.Remove(chunkPos);
                if (loadedWaters.ContainsKey(chunkPos))
                {
                    loadedWaters[chunkPos].RemoveWater();
                    loadedWaters.Remove(chunkPos);
                }
            }
            
            // Load chunks within render distance
            for (int x = playerRect.min.x; x < playerRect.max.x; x++)
            {
                for (int z = playerRect.min.y; z < playerRect.max.y; z++)
                {
                    Vector2Int chunkPos = new Vector2Int(x, z);

                    if (!loadedChunks.ContainsKey(chunkPos) && !chunkGenerationJobs.ContainsKey(chunkPos))
                    {
                        GenerateChunk(chunkPos);
                    }
                }
            }
        }

        // Update is called once per frame
        private void Update()
        {
            ProcessFinishedJobs();
            ProcessHeightmapQueue();
            
            if (Vector3.Distance(player.transform.position, lastPlayerPosition) > config.chunkSize)
            {
                UpdateChunks();
                lastPlayerPosition = player.transform.position;
            }
        }

        private void ProcessFinishedJobs()
        {
            var jobsToRemove = new List<Vector2Int>();

            // Update Job Dictionary
            foreach (var job in chunkGenerationJobs)
            {
                if (job.Value.IsCompleted)
                {
                    job.Value.Complete(); // Ensure the job is completed before disposing
                    heightmapQueue.Enqueue(new RawHeightmap(job.Key, chunkGenerationResult[job.Key]));
                    
                    jobsToRemove.Add(job.Key);
                }
            }
            
            foreach (var job in jobsToRemove)
            {
                chunkGenerationJobs.Remove(job);
                chunkGenerationResult.Remove(job);
            }
        }

        private void ProcessHeightmapQueue()
        {
            var rect = CalculateChunkGenerationRect(player.GetPlayerChunkPos(config.chunkSize));
            int count = 0;
            while (heightmapQueue.Count > 0 && count < batchCount)
            {
                var rawHeightmap = heightmapQueue.Dequeue();
                if (rect.Contains(rawHeightmap.pos) && !loadedChunks.ContainsKey(rawHeightmap.pos))
                {
                    GenGameObjects(rawHeightmap.pos, rawHeightmap.data).Forget();
                    ++count;
                }
                else
                {
                    rawHeightmap.data.Dispose();
                }
            }
                
        }

        private async UniTaskVoid GenGameObjects(Vector2Int chunkPos, NativeArray<float> rawHeightmap)
        {
            if (loadedChunks.ContainsKey(chunkPos)) return;
                    
            var heightmap = Dimensional.Convert1DTo2D<float>(
                rawHeightmap.ToArray(), 
                config.chunkSize + 1, 
                config.chunkSize + 1
            );
            
            if (!isPlayerSpawned)
            {
                var highestPos = Dimensional.GetMaximumIndexInFloat2DArray(heightmap);
                var highestHeight = heightmap[highestPos.x, highestPos.y];

                if (highestHeight > config.waterLevel + (1.0f / config.height))
                {
                    isPlayerSpawned = true;
                    player.Spawn(chunkPos, highestPos, highestHeight, config);
                }
            }
                    
            var actualChunkPosition = new Vector3(chunkPos.x * config.chunkSize, 0, chunkPos.y * config.chunkSize);
            await UniTask.Yield();

            var chunk = ChunkPoolManager.Instance.Pool.Get();
                    
            chunk.name = $"Chunk({chunkPos.x},{chunkPos.y})";
            chunk.transform.position = actualChunkPosition;
                    
            var chunkComponent = chunk.GetComponent<Chunk>();
                    
            chunkComponent.InitChunk(heightmap, config);
            loadedChunks.Add(chunkPos, chunkComponent);

            await UniTask.Yield();
            
            var minHeight = Dimensional.GetMinimumValueInFloat2DArray(heightmap);

            if (minHeight < config.waterLevel)
            {
                var water = WaterPoolManager.Instance.Pool.Get();
                    
                water.name = $"Water({chunkPos.x},{chunkPos.y})";
                water.transform.position =
                    actualChunkPosition
                    + config.waterLevel * config.height * Vector3.up
                    + config.chunkSize / 2.0f * Vector3.right
                    + config.chunkSize / 2.0f * Vector3.forward;

                var waterComponent = water.GetComponent<Water>();
                waterComponent.InitWater(heightmap, config);
                loadedWaters.Add(chunkPos, waterComponent);
            }

            rawHeightmap.Dispose();
        }

        private void OnDestroy()
        {
            // Ensure all remaining jobs are completed before the object is destroyed
            foreach (var job in chunkGenerationJobs)
            {
                job.Value.Complete();
            }

            foreach (var result in chunkGenerationResult)
            {
                result.Value.Dispose();
            }

            // Clear the dictionary when the object is destroyed
            chunkGenerationJobs.Clear();
            chunkGenerationResult.Clear();
        }

        public Chunk GetChunk(Vector2Int chunkPos)
        {
            loadedChunks.TryGetValue(chunkPos, out var chunk);

            return chunk;
        }

        public float GetSteepness(Vector2 pos)
        {
            var chunkPos = new Vector2Int(
                Mathf.CeilToInt(pos.x / config.chunkSize), 
                Mathf.CeilToInt(pos.y / config.chunkSize));
            
            var internalPos = new Vector2(pos.x % config.chunkSize, pos.y % config.chunkSize);

            Chunk chunk = GetChunk(chunkPos);

            if (chunk == null)
                return float.PositiveInfinity;

            var terrainData = chunk.GetTerrainData();
            var normalizedPos = internalPos / config.chunkSize;

            return terrainData.GetSteepness(normalizedPos.x, normalizedPos.y);
        }
        
        public ChunkGenerationConfiguration GetConfig(){return config;}

        public float GetHeight(Vector2 pos)
        {
            var chunkPos = new Vector2Int(
                Mathf.CeilToInt(pos.x / config.chunkSize), 
                Mathf.CeilToInt(pos.y / config.chunkSize));
            
            var internalPos = new Vector2(pos.x % config.chunkSize, pos.y % config.chunkSize);

            Chunk chunk = GetChunk(chunkPos);

            if (chunk == null)
                return 0;

            var terrainData = chunk.GetTerrainData();
            var normalizedPos = internalPos / config.chunkSize;

            return terrainData.GetInterpolatedHeight(normalizedPos.x, normalizedPos.y);
        }
        
        public float GetHeightInt(Vector2Int pos)
        {
            var chunkPos = new Vector2Int(pos.x / config.chunkSize, pos.y / config.chunkSize);
            
            var internalPos = new Vector2Int(pos.x % config.chunkSize, pos.y % config.chunkSize);

            Chunk chunk = GetChunk(chunkPos);

            if (chunk == null)
                return 0;

            var terrainData = chunk.GetTerrainData();
            var normalizedPos = internalPos / config.chunkSize;

            return terrainData.GetHeight(internalPos.x, internalPos.y);
        }
        
    }
}
