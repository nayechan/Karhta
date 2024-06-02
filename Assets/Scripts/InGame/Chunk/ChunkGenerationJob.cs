using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace InGame.Chunk
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
                        );
                        
                        noiseValue = (noiseValue + 1) / 2.0f * amplitude;

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
}