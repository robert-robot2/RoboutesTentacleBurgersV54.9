




public interface IiEnemy
{



    void EnemyAttack(IBloodiCharacter active);
    void EnemyMove(IBloodiCharacter active);

  //  void StopMovement(string key);
    void EnemyTickAllAnimation();

    void SetAggression(IBloodiCharacter active);
    bool EnemyIsAlive { get; }
    int EnemyX { get; }
    int EnemyY { get; }
    string SpriteStyle { get; }
    string? HitEffectPath { get; }

    List<SplatterPuddle> SplatterPuddles { get; }

    Rectangle EnemyCollisionBox { get; }
    Rectangle EnemyPunchBox { get; }
    void EnemyTakeDamage(int amount);  
    void ClearHitEffect();

    // Stats
    string EnemyClassName { get; }
    int EnemyLevel { get; set; }
    int EnemyXP { get; set; }
    int EnemyHungerCurrent { get; set; }
    int EnemyHungerFull { get; set; }
    int EnemyHungerDurationSeconds { get; set; }
    int EnemyHitPoints { get; set; }
    int EnemyMaxHP { get; set; }

    int EnemyXPPerLevel { get; set; }

    int EnemyLevelCap { get; set; }

    int EnemyStrength { get; set; }
    int EnemyAlacrity { get; set; }
    int EnemyCelerity { get; set; }
    int EnemyLimenity { get; set; }
    int EnemyIntelligence { get; set; }

    // these are specific stats unique to certain enemy classes
    string EnemyResourceName { get; }     // "Rage", "Mana", etc.
    int EnemyResourceValue { get; set; }  // "35", "120", etc.
    string EnemyRegenLabel { get; }       // "Rage on Hit", "Mana Regen"
    int EnemyRegenValue { get; set; }

    string EnemyMaxResourceName { get; }
    int EnemyMaxResourceValue { get; set; }

    int EnemyLifeRegen { get; set; }
    int EnemyStatPoints { get; set; }

    // Enemy Color Theme Info
    string EnemyHPColor { get; }
    string EnemyInvColor { get; }
    string EnemyEnergyColor { get; }

    // Debugging
    string EnemyCollisionBoxStyle { get; }
    string EnemySpriteDebugStyle { get; }
    string EnemyPunchBoxStyle { get; }
    // string EnemyDebugText { get; }



    void AddXp(BloodLevel levelReg, double multiplier);



}

