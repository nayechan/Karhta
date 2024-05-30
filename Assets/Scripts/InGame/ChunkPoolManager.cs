using System;
using UnityEngine;
using UnityEngine.Pool;

namespace InGame
{
    public class ChunkPoolManager : MonoBehaviour
    {

        [SerializeField] int defaultCapacity = 256;
        [SerializeField] int maxCapacity = 512;
        [SerializeField] private GameObject chunkPrefab;
        
        public static ChunkPoolManager Instance { get; private set; }
        public IObjectPool<GameObject> Pool { get; private set; }

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
            Pool = new ObjectPool<GameObject>(
                CreatePooledItem, OnGetFromPool, OnReleaseFromPool, OnDestroyPoolObject,
                true, defaultCapacity, maxCapacity);
        }

        private GameObject CreatePooledItem()
        {
            GameObject gameObject = Instantiate(chunkPrefab, transform);
            gameObject.GetComponent<Chunk>().Pool = this.Pool;
            return gameObject;
        }

        private void OnGetFromPool(GameObject gameObject)
        {
            gameObject.name = "unnamed_chunk";
            gameObject.SetActive(true);
        }

        private void OnReleaseFromPool(GameObject gameObject)
        {
            gameObject.name = "unloaded_chunk";
            gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(GameObject gameObject)
        {
            Destroy(gameObject);
        }

    }
}