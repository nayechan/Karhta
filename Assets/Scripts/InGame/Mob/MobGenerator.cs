using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InGame.Mob
{
    public class MobGenerator : MonoBehaviour
    {
        [SerializeField] private Player.Player player;
        [SerializeField] private float delay = 3.0f;
        [SerializeField] private int maxDistance = 128;
        
        private MobPoolManager mobPoolManager;

        private const int maxHeight = 256;

        private void Start()
        {
            mobPoolManager = MobPoolManager.Instance;
            GenerateMob().Forget();
        }

        private Vector3 GetSpawnPos()
        {
            var layerMasks = new[] { "Ground", "Water", "Obstacle", "Tree" };
            var layerMask = layerMasks.Aggregate(
                0,
                (current, layer) => current | (1 << LayerMask.NameToLayer(layer))
            );

            int tryCount = 5;

            do
            {
                var isAvailable = true;
                
                float angleInRadians = Random.Range(0, 2 * Mathf.PI);
                float radius = Random.Range(maxDistance * 0.5f, maxDistance);
        
                float x = Mathf.Cos(angleInRadians) * radius;
                float z = Mathf.Sin(angleInRadians) * radius;

                var ray = new Ray(new Vector3(
                    player.transform.position.x + x, maxHeight, player.transform.position.z + z), Vector3.down);

                if (Physics.SphereCast(ray, 1.0f, out var hit, maxHeight, layerMask))
                {
                    if (hit.transform.CompareTag("Ground"))
                    {
                        return hit.point;
                    }
                }

                --tryCount;
            } while (tryCount > 0);

            return -Vector3.one;
        }

        private async UniTaskVoid GenerateMob()
        {
            await UniTask.WaitUntil(() => player.isSpawned);
            while (true)
            {
                if (mobPoolManager.count < mobPoolManager.GetMaxCapacity())
                {
                    var pos = GetSpawnPos();

                    if (pos != -Vector3.one)
                    {
                        var mob = mobPoolManager.Pool.Get();
                        mob.transform.position = GetSpawnPos() + Vector3.up;
                    }
                }
                
                await UniTask.WaitForSeconds(delay);
            }
        }
    }
}
