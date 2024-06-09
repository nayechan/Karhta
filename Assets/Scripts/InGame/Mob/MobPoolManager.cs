using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace InGame.Mob
{
    public class MobPoolManager : MonoBehaviour
    {
        [SerializeField] int defaultCapacity = 70;
        [SerializeField] int maxCapacity = 300;
        [SerializeField] private GameObject mobPrefab;

        public int count { get; private set; }
        
        public static MobPoolManager Instance { get; private set; }
        public IObjectPool<GameObject> Pool { get; private set; }

        private void Awake()
        {
            count = 0;
            
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
            GameObject gameObject = Instantiate(mobPrefab, transform);
            return gameObject;
        }

        private void OnGetFromPool(GameObject gameObject)
        {
            ++count;
            gameObject.SetActive(true);
        }

        private void OnReleaseFromPool(GameObject gameObject)
        {
            --count;
            gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(GameObject gameObject)
        {
            Destroy(gameObject);
        }

        public int GetMaxCapacity()
        {
            return maxCapacity;
        }
    }
}