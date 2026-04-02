

public class BloodEnemyHandle
{
    public IiEnemy? ActiveEnemy { get;  set; }

    public static readonly List<IEnumerable<IiEnemy>> AllRegistries = new()
    {
        BloodEnemy.BloodSkeletonRegistry.All,
        BloodSkelPyscho.BloodSkelPyschoRegistry.All,
        BloodZombiePyscho.BloodZombiePyschoRegistry.All,
        BloodSkelWar.BloodSkeletonWarRegistry.All,
        BloodGoatMan.BloodGoatmanRegistry.All,
        BloodBoss.BloodScavBossRegistry.All,
        BloodTownSlut.BloodTownSlutRegistry.All,
        BloodCow.BloodCowRegistry.All,
        BloodEnemyBoss.BloodSkeletonRegistry.All,
        BloodCat.BloodSkeletonRegistry.All
    };

    public void UpdateActiveEnemy()
    {
        if (AllRegistries == null || AllRegistries.Count == 0)
        {
            ActiveEnemy = null;
            return;
        }

        foreach (var registry in AllRegistries)
        {
            if (registry == null) continue;

            foreach (var enemy in registry)
            {
                if (enemy != null && enemy.EnemyIsAlive)
                {
                    ActiveEnemy = enemy;
                    return;
                }
            }
        }

        ActiveEnemy = null;
    }


    public void Clear() => ActiveEnemy = null;
}
