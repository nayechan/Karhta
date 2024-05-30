using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using AncyUtility;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor.Playables;

namespace InGame
{
    [Serializable]
    public struct ChunkGenerationConfiguration
    {
        [SerializeField] public uint seed;
        [SerializeField] public int lacunarity;
        [SerializeField] public float persistence;

        [SerializeField] public int chunkSize;
        [SerializeField] public int baseScale;
        [SerializeField] public int height;
    }
    
    public class ChunkGenerator : MonoBehaviour
    {
        [BurstCompile]
        struct ChunkGenerationJob : IJob
        {
            [ReadOnly] public ChunkGenerationConfiguration config;
            [ReadOnly] public Vector2Int chunkPos;
            [WriteOnly] public NativeArray<float> heightmap;

            // Constants for improved readability
            const int NumOffsets = 4;
            const int OffsetRange = 8;
            const int OffsetBitCount = 4;
            const int NoiseOffsetSize = 1024;
            
            void GenerateHeights(int startX, int startZ)
            {
                int index = 0;
                uint _seed = config.seed;

                NativeArray<Vector2Int> offsets = 
                    new NativeArray<Vector2Int>(NumOffsets, Allocator.Temp);
                
                for (int i = 0; i < NumOffsets; ++i)
                {
                    int offsetX = (int)(_seed & 0xf) - OffsetRange;
                    _seed >>= OffsetBitCount;

                    int offsetY = (int)(_seed & 0xf) - OffsetRange;
                    _seed >>= OffsetBitCount;

                    offsets[i] = new Vector2Int(offsetX, offsetY);
                }


                for (int z = 0; z <= config.chunkSize; ++z)
                {
                    for (int x = 0; x <= config.chunkSize; ++x)
                    {
                        float noiseSum = 0;
                        float weightSum = 0;
                        float frequency = Mathf.Pow(config.lacunarity, -NumOffsets+1);
                        float amplitude = 1.0f;
                        
                        for (int octaveIndex = 0; octaveIndex < NumOffsets; ++octaveIndex)
                        {
                            float noiseValue = Unity.Mathematics.noise.snoise(
                                new Unity.Mathematics.float2(
                                    (startX + x + NoiseOffsetSize * offsets[octaveIndex].x) 
                                    * frequency / config.baseScale,
                                    (startZ + z + NoiseOffsetSize * offsets[octaveIndex].y) 
                                    * frequency / config.baseScale
                                )
                            ) * amplitude;

                            noiseSum += noiseValue;
                            weightSum += amplitude;

                            frequency *= config.lacunarity;
                            amplitude *= config.persistence;
                        }
                        
                        // Calculate the Perlin noise value using the seeded function
                        heightmap[index++] = noiseSum / weightSum;
                    }
                }

                offsets.Dispose();
            }
            
            public void Execute()
            {
                GenerateHeights(
                    chunkPos.x * config.chunkSize, 
                    chunkPos.y * config.chunkSize
                );
            }
        }

        [SerializeField] public ChunkGenerationConfiguration config;
        
        [SerializeField] private int renderDistance;
        [SerializeField] private Player player;

        private Vector3 lastPlayerPosition;

        private Dictionary<Vector2Int, JobHandle> chunkGenerationJobs;
        private Dictionary<Vector2Int, NativeArray<float>> chunkGenerationResult;
        private Dictionary<Vector2Int, Chunk> loadedChunks;

        private List<Vector2Int> jobsToRemove;
        private List<Vector2Int> chunksToRemove;

        void InitialChunkGeneration()
        {
            for (int x = -renderDistance; x <= renderDistance; ++x)
            {
                for (int z = -renderDistance; z <= renderDistance; ++z)
                {
                    Vector2Int chunkPos = new Vector2Int(x, z);
                    
                    var heightmap = new NativeArray<float>(
                        (config.chunkSize + 1) * (config.chunkSize + 1), Allocator.Persistent
                    );
                    
                    chunkGenerationResult.Add(
                        chunkPos, 
                        heightmap
                    );
                    
                    ChunkGenerationJob chunkGenerationJob = new ChunkGenerationJob{
                        heightmap = heightmap,
                        chunkPos = chunkPos,
                        config = config
                    };

                    JobHandle handle = chunkGenerationJob.Schedule();
                    chunkGenerationJobs.Add(chunkPos, handle);
                }
            }
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

            chunksToRemove.Clear();
            
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
            }
            
            // Load chunks within render distance
            for (int x = playerRect.min.x; x < playerRect.max.x; x++)
            {
                for (int z = playerRect.min.y; z < playerRect.max.y; z++)
                {
                    Vector2Int chunkPos = new Vector2Int(x, z);

                    if (!loadedChunks.ContainsKey(chunkPos) && !chunkGenerationJobs.ContainsKey(chunkPos))
                    {
                        var heightmap = new NativeArray<float>(
                            (config.chunkSize + 1) * (config.chunkSize + 1), Allocator.Persistent
                        );
                        
                        chunkGenerationResult.Add(
                            chunkPos, 
                            heightmap
                        );
                        
                        ChunkGenerationJob chunkGenerationJob = new ChunkGenerationJob{
                            heightmap = heightmap,
                            chunkPos = chunkPos,
                            config = config
                        };

                        JobHandle handle = chunkGenerationJob.Schedule();
                        chunkGenerationJobs.Add(chunkPos, handle);
                    }
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            chunksToRemove = new List<Vector2Int>();
            jobsToRemove = new List<Vector2Int>();
            
            lastPlayerPosition = player.transform.position;
            
            loadedChunks = new Dictionary<Vector2Int, Chunk>();
            chunkGenerationJobs = new Dictionary<Vector2Int, JobHandle>();
            chunkGenerationResult = new Dictionary<Vector2Int, NativeArray<float>>();
            
            InitialChunkGeneration();
        }

        // Update is called once per frame
        void Update()
        {
            jobsToRemove.Clear();
            
            // Update Job Dictionary
            foreach (var job in chunkGenerationJobs)
            {
                if (job.Value.IsCompleted)
                {
                    job.Value.Complete(); // Ensure the job is completed before disposing
                    var rawHeightmap = chunkGenerationResult[job.Key];
                    
                    var heightmap = Dimensional.Convert1DTo2D<float>(
                        rawHeightmap.ToArray(), 
                        config.chunkSize + 1, 
                        config.chunkSize + 1
                    );

                    var chunk = ChunkPoolManager.Instance.Pool.Get();
                    
                    var actualChunkPosition = new Vector3(job.Key.x * config.chunkSize, 0, job.Key.y * config.chunkSize);
                    chunk.name = "Chunk_" + job.Key.x + "_" + job.Key.y;
                    chunk.transform.position = actualChunkPosition;
                    
                    var chunkComponent = chunk.GetComponent<Chunk>();
                    chunkComponent.SetTerrainData(heightmap, config);

                    rawHeightmap.Dispose();
                    
                    loadedChunks.Add(job.Key, chunkComponent);
                    jobsToRemove.Add(job.Key);
                }
            }
            
            foreach (var job in jobsToRemove)
            {
                chunkGenerationJobs.Remove(job);
                chunkGenerationResult.Remove(job);
            }
            
            if (Vector3.Distance(player.transform.position, lastPlayerPosition) > config.chunkSize)
            {
                UpdateChunks();
                lastPlayerPosition = player.transform.position;
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
