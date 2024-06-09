namespace InGame.Battle
{
    public interface IDamageable
    {
        void TakeDamage(IAttackable from, long amount);
        void Death();
        long CurrentHp { get; }
        long MaxHp { get; }
    }
}