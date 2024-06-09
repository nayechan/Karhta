using InGame.Battle;
using UnityEngine;

namespace InGame.Player
{
    public partial class Player : IAttackable, IDamageable
    {
        private void Attack()
        {
            animator.SetTrigger(AttackTrigger);
            foreach (var damageable in damageablesInArea)
            {
                Damage(damageable);
            }
        }

        public void Damage(IDamageable target)
        {
            target.TakeDamage(this, 100);
        }

        public void TakeDamage(IAttackable from, long amount)
        {
            CurrentHp -= amount;

            if (CurrentHp <= 0)
            {
                Death();
            }
            
            Refresh();
        }

        public void Death()
        {
            
        }
    }
}