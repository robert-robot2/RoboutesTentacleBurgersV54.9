
public class BloodWaveReg1040
{

    public BloodCharacterHandle CharacterHandle { get; set; } = new();
    public IBloodiCharacter? ActiveCharacter => CharacterHandle.ActiveCharacter;
    public IiEnemy? ActiveEnemy { get; private set; }

    private readonly Dictionary<int, List<object>> Waves = new();
    private int currentWave = 1;

    // designate the last wave for this map
    private int lastWaveId = 3; // change per map (e.g. 5 for dungeons)

    // respawn timer tracking
    private DateTime? nextRespawnAt;
    private readonly Random rng = new();

    public void LoadWave(int waveId)
    {
        var waveEntities = new List<object>();

        int beforeEnemy = BloodEnemy.BloodSkeletonRegistry.All.Count;
        int beforeZombie = BloodZombiePyscho.BloodZombiePyschoRegistry.All.Count;
        int beforePsycho = BloodSkelPyscho.BloodSkelPyschoRegistry.All.Count;
        int beforeWar = BloodSkelWar.BloodSkeletonWarRegistry.All.Count;
        int beforeGoat = BloodGoatMan.BloodGoatmanRegistry.All.Count;
        int beforeBoss = BloodBoss.BloodScavBossRegistry.All.Count;

        if (waveId == 1)
        {
         //   BloodEnemy.BloodSkeletonRegistry.SpawnSkeletons(1);
            BloodZombiePyscho.BloodZombiePyschoRegistry.SpawnZombiePyschos(1);
            //   BloodSkelWar.BloodSkeletonWarRegistry.SpawnSkeletonWars(1);
            BloodSkelPyscho.BloodSkelPyschoRegistry.SpawnSkelPyschos(1);
            //  BloodGoatMan.BloodGoatmanRegistry.SpawnGoatmen(1);
            // BloodBoss.BloodScavBossRegistry.SpawnScavBoss(1);

        }

        if (waveId == 2)
        {
          //  BloodEnemy.BloodSkeletonRegistry.SpawnSkeletons(2);
            BloodZombiePyscho.BloodZombiePyschoRegistry.SpawnZombiePyschos(1);
            // BloodSkelWar.BloodSkeletonWarRegistry.SpawnSkeletonWars(1);
            BloodSkelPyscho.BloodSkelPyschoRegistry.SpawnSkelPyschos(1);
            //  BloodGoatMan.BloodGoatmanRegistry.SpawnGoatmen(1);
            // BloodBoss.BloodScavBossRegistry.SpawnScavBoss(1);
        }

        if (waveId == 3)
        {
         //   BloodEnemy.BloodSkeletonRegistry.SpawnSkeletons(3);
            BloodZombiePyscho.BloodZombiePyschoRegistry.SpawnZombiePyschos(2);
            // BloodSkelWar.BloodSkeletonWarRegistry.SpawnSkeletonWars(1);
            BloodSkelPyscho.BloodSkelPyschoRegistry.SpawnSkelPyschos(3);
            //  BloodGoatMan.BloodGoatmanRegistry.SpawnGoatmen(1);
            // BloodBoss.BloodScavBossRegistry.SpawnScavBoss(1);
        }
        if (waveId == 3)
        {
           // BloodEnemy.BloodSkeletonRegistry.SpawnSkeletons(3);
            BloodZombiePyscho.BloodZombiePyschoRegistry.SpawnZombiePyschos(2);
            // BloodSkelWar.BloodSkeletonWarRegistry.SpawnSkeletonWars(1);
            BloodSkelPyscho.BloodSkelPyschoRegistry.SpawnSkelPyschos(3);
            //  BloodGoatMan.BloodGoatmanRegistry.SpawnGoatmen(1);
            // BloodBoss.BloodScavBossRegistry.SpawnScavBoss(1);
        }
        if (waveId == 3)
        {
          // BloodEnemy.BloodSkeletonRegistry.SpawnSkeletons(3);
            BloodZombiePyscho.BloodZombiePyschoRegistry.SpawnZombiePyschos(2);
            BloodSkelWar.BloodSkeletonWarRegistry.SpawnSkeletonWars(1);
            BloodSkelPyscho.BloodSkelPyschoRegistry.SpawnSkelPyschos(3);
            //  BloodGoatMan.BloodGoatmanRegistry.SpawnGoatmen(1);
            // BloodBoss.BloodScavBossRegistry.SpawnScavBoss(1);
        }
        var newEnemies = BloodEnemy.BloodSkeletonRegistry.All.Skip(beforeEnemy).ToList();
        var newZombies = BloodZombiePyscho.BloodZombiePyschoRegistry.All.Skip(beforeZombie).ToList();
        var newPsychos = BloodSkelPyscho.BloodSkelPyschoRegistry.All.Skip(beforePsycho).ToList();
        var newWars = BloodSkelWar.BloodSkeletonWarRegistry.All.Skip(beforeWar).ToList();
        var newGoats = BloodGoatMan.BloodGoatmanRegistry.All.Skip(beforeGoat).ToList();
        var newBosses = BloodBoss.BloodScavBossRegistry.All.Skip(beforeBoss).ToList();

        waveEntities.AddRange(newEnemies);
        waveEntities.AddRange(newZombies);
        waveEntities.AddRange(newPsychos);
        waveEntities.AddRange(newWars);
        waveEntities.AddRange(newGoats);
        waveEntities.AddRange(newBosses);

        foreach (var entity in waveEntities)
        {
            switch (entity)
            {
                case BloodEnemy.Skeleton:
                    break;
                case BloodZombiePyscho.ZombiePyscho z:
                    z.SetAggressionTarget(ActiveCharacter);
                    break;
                case BloodSkelPyscho.SkelPyscho w:
                    w.SetAggressionTarget(ActiveCharacter);
                    break;
                case BloodSkelWar.SkeletonWar a:
                    a.SetAggressionTarget(ActiveCharacter);
                    break;
                case BloodGoatMan.Goatman g:
                    g.SetAggressionTarget(ActiveCharacter);
                    break;
                case BloodBoss.ScavBoss f:
                    f.SetAggressionTarget(ActiveCharacter);
                    break;
            }
        }

        Waves[waveId] = waveEntities;
        currentWave = waveId;
    }

    public bool IsWaveCooked()
    {
        if (!Waves.ContainsKey(currentWave)) return false;

        foreach (var entity in Waves[currentWave])
        {
            switch (entity)
            {
                case BloodEnemy.Skeleton s when s.SkeletonIsAlive:
                case BloodZombiePyscho.ZombiePyscho z when z.ZombiePyschoIsAlive:
                case BloodSkelPyscho.SkelPyscho w when w.SkelPyschoIsAlive:
                case BloodSkelWar.SkeletonWar a when a.SkeletonWarIsAlive:
                case BloodGoatMan.Goatman g when g.GoatmanIsAlive:
                case BloodBoss.ScavBoss f when f.ScavBossIsAlive:
                    return false;
            }
        }

        return true;
    }

    public void TryAdvanceWave()
    {
        if (IsWaveCooked())
        {
            if (currentWave < lastWaveId)
            {
                // normal progression
                LoadWave(currentWave + 1);
            }
            else
            {
                // last wave cooked → schedule respawn of wave 1 after random delay
                if (nextRespawnAt == null)
                {
                    int delaySeconds = rng.Next(5, 31); // random 5–30s
                    nextRespawnAt = DateTime.Now.AddSeconds(delaySeconds);
                }

                if (DateTime.Now >= nextRespawnAt)
                {
                    LoadWave(1);
                    nextRespawnAt = null; // reset timer
                }
            }
        }
    }

    public void ClearWave()
    {
        if (Waves.ContainsKey(currentWave))
        {
            Waves[currentWave].Clear();
            Waves.Remove(currentWave);
        }

        BloodEnemy.BloodSkeletonRegistry.All.Clear();
        BloodZombiePyscho.BloodZombiePyschoRegistry.All.Clear();
        BloodSkelPyscho.BloodSkelPyschoRegistry.All.Clear();
        BloodSkelWar.BloodSkeletonWarRegistry.All.Clear();
        BloodGoatMan.BloodGoatmanRegistry.All.Clear();
        BloodBoss.BloodScavBossRegistry.All.Clear();

        currentWave = 0;
    }
}