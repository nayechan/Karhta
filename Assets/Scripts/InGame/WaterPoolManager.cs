using System;
using UnityEngine;
using UnityEngine.Pool;

namespace InGame
{
    public class WaterPoolManager : MonoBehaviour
    {

        [SerializeField] int defaultCapacity = 256;
        [SerializeField] int maxCapacity = 512;
        [SerializeField] private GameObject waterPrefab;
        
        public static WaterPoolManager Instance { get; private set; }
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
            GameObject gameObject = Instantiate(waterPrefab, transform);
            gameObject.GetComponent<Water>().Pool = this.Pool;
            return gameObject;
        }

        private void OnGetFromPool(GameObject gameObject)
        {
            gameObject.name = "unnamed_water";
            gameObject.SetActive(true);
        }

        private void OnReleaseFromPool(GameObject gameObject)
        {
            gameObject.name = "unloaded_water";
            gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(GameObject gameObject)
        {
            Destroy(gameObject);
        }

    }
}