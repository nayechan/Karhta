using System;
using UnityEngine;
using UnityEngine.Pool;

namespace InGame
{
    public class Chunk : MonoBehaviour
    {
        [SerializeField] private TerrainLayer[] terrainLayers;
        [SerializeField] private Material terrainMaterial;
        private Vector2Int chunkPos;
        public IObjectPool<GameObject> Pool { get; set; }

        private Terrain terrain;
        private TerrainCollider terrainCollider;

        private void Awake()
        {
            chunkPos = new Vector2Int(0, 0);
            terrain = GetComponent<Terrain>();
            terrainCollider = GetComponent<TerrainCollider>();
        }

        public bool UpdateChunk(RectInt renderRect)
        {
            // Unload chunks outside render distance
            if (!renderRect.Contains(chunkPos))
            {
                Pool.Release(gameObject);
                return false;
            }

            return true;
        }

        public void InitChunk(float[,] heightmap, ChunkGenerationConfiguration config)
        {
            var pos = transform.position;
            
            var chunkX = Mathf.RoundToInt(pos.x / config.chunkSize);
            var chunkZ = Mathf.RoundToInt(pos.z / config.chunkSize);
            
            chunkPos = new Vector2Int(chunkX, chunkZ);

            //var terrainData = Instantiate(terrain.terrainData);
            var terrainData = new TerrainData();
            terrainData.heightmapResolution = config.chunkSize + 1;
            terrainData.size = new Vector3(config.chunkSize, config.height, config.chunkSize);
            terrainData.SetHeights(0, 0, heightmap);

            terrainData.terrainLayers = terrainLayers;
            terrain.materialTemplate = terrainMaterial;
            
            int alphamapResolution = terrainData.alphamapResolution;
            float[,,] splatmapData = new float[alphamapResolution, alphamapResolution, terrainData.alphamapLayers];
            
            for (int y = 0; y < alphamapResolution; ++y)
            {
                for (int x = 0; x < alphamapResolution; ++x)
                {
                    splatmapData[x, y, 0] = 1.0f;
                }
            }
            
            terrainData.SetAlphamaps(0, 0, splatmapData);
            
            terrain.terrainData = terrainData;
            terrainCollider.terrainData = terrainData;
        }
    }
}