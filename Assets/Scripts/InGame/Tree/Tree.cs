using System;
using InGame.Battle;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InGame.Tree
{
    public class Tree : MonoBehaviour, IDamageable
    {
        [field: SerializeField]
        public long CurrentHp { get; private set; }
        
        [field: SerializeField]
        public long MaxHp { get; private set; }

        [SerializeField] protected GameObject breakParticlePrefab;

        public virtual bool IsDamageable()
        {
            if (gameObject.IsDestroyed())
                return false;
            
            return gameObject.activeSelf;
        }

        protected virtual void Awake()
        {
            CurrentHp = MaxHp;
        }

        public virtual void TakeDamage(IAttackable from, long amount)
        {
            CurrentHp -= amount;

            if (CurrentHp <= 0)
            {
                Death(from);
            }
        }

        public virtual void Death(IAttackable from)
        {
            from?.OnTargetDead(this);
            TreeGenerator.Instance.RemoveTree(this);
            Instantiate(breakParticlePrefab, transform.position, Quaternion.identity);
            
            Destroy(gameObject);
        }

        public virtual string GetTreeType()
        {
            return "Tree";
        }

        public virtual IDamageable.DamageableType GetDamageableType()
        {
            return IDamageable.DamageableType.Tree;
        }
    }
}