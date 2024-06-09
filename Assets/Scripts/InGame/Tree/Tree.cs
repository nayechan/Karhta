using System;
using InGame.Battle;
using UnityEngine;

namespace InGame.Tree
{
    public class Tree : MonoBehaviour, IDamageable
    {
        [field: SerializeField]
        public long CurrentHp { get; private set; }
        
        [field: SerializeField]
        public long MaxHp { get; private set; }

        private void Awake()
        {
            CurrentHp = MaxHp;
        }

        public void TakeDamage(IAttackable from, long amount)
        {
            CurrentHp -= amount;

            if (CurrentHp <= 0)
            {
                Death();
            }
        }

        public void Death()
        {
            Destroy(gameObject, 1.0f);
        }

    }
}