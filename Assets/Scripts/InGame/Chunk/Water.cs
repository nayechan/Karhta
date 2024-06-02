using UnityEngine;
using UnityEngine.Pool;

namespace InGame.Chunk
{
    public class Water : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;
        private Vector2Int chunkPos;
        [SerializeField] private float meshScale = 1.0f;
        public IObjectPool<GameObject> Pool { get; set; }
        private void Awake()
        {
            chunkPos = new Vector2Int(0, 0);
            meshFilter = GetComponent<MeshFilter>();
            meshCollider = GetComponent<MeshCollider>();
        }
        
        public void RemoveWater()
        {
            Pool.Release(gameObject);
        }

        public void InitWater(float[,] heightmap, ChunkGenerationConfiguration config)
        {
            var pos = transform.position;
            
            var chunkX = Mathf.RoundToInt(pos.x / config.chunkSize);
            var chunkZ = Mathf.RoundToInt(pos.z / config.chunkSize);
            
            chunkPos = new Vector2Int(chunkX, chunkZ);

            transform.localScale = new Vector3(config.chunkSize * 0.1f, 1, config.chunkSize * 0.1f);
        }
    }

}
