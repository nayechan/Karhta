using System;

namespace InGame.Battle
{
    public interface IDamageable
    {
        enum DamageableType
        {
            Player,
            Mob,
            Tree,
            Mineral
        }
        
        void TakeDamage(IAttackable from, long amount);
        void Death(IAttackable from);
        
        long CurrentHp { get; }
        long MaxHp { get; }
        bool IsDamageable();
        DamageableType GetDamageableType();
    }
}