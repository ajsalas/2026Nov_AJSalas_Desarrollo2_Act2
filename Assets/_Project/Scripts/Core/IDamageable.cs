namespace DungeonSurvivor.Core
{

    public interface IDamageable
    {
        void TakeDamage(DamageInfo damageInfo);
        bool IsAlive { get; }
        int CurrentHealth { get; }
        int MaxHealth { get; }
    }
}

