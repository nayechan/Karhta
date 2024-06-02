using System;
using UnityEngine;

namespace InGame.Chunk
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

        [SerializeField] public float waterLevel;
    }
}