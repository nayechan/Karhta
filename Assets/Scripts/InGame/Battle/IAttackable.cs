namespace InGame.Battle
{
    public interface IAttackable
    {
        void Damage(IDamageable target);

        void OnTargetDead(IDamageable target);

        bool IsAttackable();
    }
}