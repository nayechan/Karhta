namespace InGame.Battle
{
    public interface IAttackable
    {
        void Damage(IDamageable target);
    }
}