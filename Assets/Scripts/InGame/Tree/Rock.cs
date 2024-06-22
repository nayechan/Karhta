using InGame.Battle;
using UnityEngine;

namespace InGame.Tree
{
    public class Rock : Tree
    {
        [SerializeField] private Item.Item dropItem;
        [SerializeField] private int minCount, maxCount;
        
        public override string GetTreeType()
        {
            return "Rock";
        }

        public override IDamageable.DamageableType GetDamageableType()
        {
            return IDamageable.DamageableType.Mineral;
        }

        public override void Death(IAttackable from)
        {
            from?.OnTargetDead(this);
            TreeGenerator.Instance.RemoveTree(this);
            var dropCount = Random.Range(minCount, maxCount);
            for (int count = 0; count < dropCount; ++count)
            {
                var randomOffset = new Vector3(Random.value - 0.5f, 0.5f, Random.value - 0.5f);
                dropItem.InstantiateDropItem(transform.position + randomOffset);
            }
            GameObject.Instantiate(breakParticlePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}