using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AncyUtility;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace InGame.Chunk
{
    public class ChunkGenerator : MonoBehaviour
    {
        [SerializeField] private ChunkGenerationConfiguration config;
        [SerializeField] private int renderDistance;
        [SerializeField] private Player player;
        [SerializeField] private Backdrop backdrop;

        private Vector3 lastPlayerPosition;

        private Dictionary<Vector2Int, JobHandle> chunkGenerationJobs;
        private Dictionary<Vector2Int, NativeArray<float>> chunkGenerationResult;
        private Dictionary<Vector2Int, Chunk> loadedChunks;
        private Dictionary<Vector2Int, Water> loadedWaters;

        private void Awake()
        {
            loadedChunks = new Dictionary<Vector2Int, Chunk>();
            loadedWaters = new Dictionary<Vector2Int, Water>();
            chunkGenerationJobs = new Dictionary<Vector2Int, JobHandle>();
            chunkGenerationResult = new Dictionary<Vector2Int, NativeArray<float>>();
        }

        // Start is called before the first frame update
        void Start()
        {
            lastPlayerPosition = player.transform.position;
            InitChunkGenerator();
        }

        async UniTaskVoid InitChunkGenerator()
        {
            backdrop.Activate("Loading Chunks...");
            for (int x = -renderDistance; x <= renderDistance; ++x)
            {
                for (int z = -renderDistance; z <= renderDistance; ++z)
                {
                    GenerateChunk(new Vector2Int(x, z));
                }
            }
            
        }

        JobHandle GenerateChunk(Vector2Int chunkPos)
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

        void UpdateChunks()
        {
            var playerPos = player.transform.position;
            
            // Calculate player's chunk coordinates
            int playerChunkX = Mathf.RoundToInt(playerPos.x / config.chunkSize);
            int playerChunkZ = Mathf.RoundToInt(playerPos.z / config.chunkSize);
            
            var playerRect = new RectInt(
                playerChunkX - renderDistance,
                playerChunkZ - renderDistance,
                renderDistance * 2 + 1,
                renderDistance * 2 + 1
            );

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

            var jobHandles = chunkGenerationJobs.Values.ToArray();
            var jobHandleNativeArray = new NativeArray<JobHandle>(jobHandles, Allocator.Temp);
            JobHandle.CombineDependencies(jobHandleNativeArray).Complete();
            
            
        }

        // Update is called once per frame
        void Update()
        {
            ProcessFinishedJobs();
            
            if (Vector3.Distance(player.transform.position, lastPlayerPosition) > config.chunkSize)
            {
                UpdateChunks();
                lastPlayerPosition = player.transform.position;
            }
            
        }

        void ProcessFinishedJobs()
        {
            var jobsToRemove = new List<Vector2Int>();
            Dictionary<Vector2Int, NativeArray<float>> heightmaps = new Dictionary<Vector2Int, NativeArray<float>>();

            // Update Job Dictionary
            foreach (var job in chunkGenerationJobs)
            {
                if (job.Value.IsCompleted)
                {
                    job.Value.Complete(); // Ensure the job is completed before disposing
                    heightmaps[job.Key] = chunkGenerationResult[job.Key];
                    
                    jobsToRemove.Add(job.Key);
                }
            }

            if (heightmaps.Count > 0) StartCoroutine(GenerateGameObjects(heightmaps));
            
            
            foreach (var job in jobsToRemove)
            {
                chunkGenerationJobs.Remove(job);
                chunkGenerationResult.Remove(job);
            }

            if (chunkGenerationJobs.Count == 0)
            {
                backdrop.Deactivate();
            }
        }

        IEnumerator GenerateGameObjects(Dictionary<Vector2Int, NativeArray<float>> heightmaps)
        {
            Queue<NativeArray<float>> heightMapQueue = new Queue<NativeArray<float>>();
            
            var playerPos = player.transform.position;
            var playerChunkX = Mathf.RoundToInt(playerPos.x / config.chunkSize);
            var playerChunkZ = Mathf.RoundToInt(playerPos.z / config.chunkSize);
            var playerChunkPos = new Vector2Int(playerChunkX, playerChunkZ);
            
            Vector2Int [] iterateOrder =
            {
                new Vector2Int(-1,-1),
                new Vector2Int(0,-1),
                new Vector2Int(1,-1),
                new Vector2Int(-1,0),
                new Vector2Int(1,0),
                new Vector2Int(-1,1),
                new Vector2Int(0,1),
                new Vector2Int(1,1)
            };
            
            foreach (var heightmapPair in heightmaps)
            {
                var rawHeightmap = heightmapPair.Value;
                var chunkPos = heightmapPair.Key;
                
                if(loadedChunks.ContainsKey(chunkPos)) continue;
                    
                var heightmap = Dimensional.Convert1DTo2D<float>(
                    rawHeightmap.ToArray(), 
                    config.chunkSize + 1, 
                    config.chunkSize + 1
                );
                    
                var actualChunkPosition = new Vector3(chunkPos.x * config.chunkSize, 0, chunkPos.y * config.chunkSize);

                var chunk = ChunkPoolManager.Instance.Pool.Get();
                    
                chunk.name = $"Chunk({chunkPos.x},{chunkPos.y})";
                chunk.transform.position = actualChunkPosition;
                    
                var chunkComponent = chunk.GetComponent<Chunk>();
                    
                chunkComponent.InitChunk(heightmap, config);
                loadedChunks.Add(chunkPos, chunkComponent);

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
                yield return null;
            }
        }

        void OnDestroy()
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
    }
}
