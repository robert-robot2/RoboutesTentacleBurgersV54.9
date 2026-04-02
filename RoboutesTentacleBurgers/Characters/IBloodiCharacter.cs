
using RoboutesTentacleBurgers.Breakables;
using static BloodStaticObject;

public interface IBloodiCharacter
{

    // Functions

    void CharAttack(IiEnemy enemyActive, IBreakables breakActive);
    void CharSpecialAttack(IiEnemy enemyActive);
    void CharMove(string key);
    void StopMovement(string key);
    void CharTickAllAnimation();
    bool CharIsAlive { get; }

    // Position
    int CharX { get; set; }   // <-- changed from get; to get; set;
    int CharY { get; set; }   // <-- changed from get; to get; set;

    // width height?? maybe leave out for custom property but it doesnt matter? maybe this is why divs wont render right when i mvoe on int inside bws.cs


    string CharSpriteStyle { get; }
    string? CharHitEffectPath { get; }
 
    List<SplatterPuddle> SplatterPuddles { get; }


    Rectangle CharCollisionBox { get; }
    Rectangle CharPunchBox { get; }
    void CharTakeDamage(int amount);  // <-- new
    void ClearHitEffect();

    // Stats
    string CharClassName { get; }
    int CharLevel { get; set; }
    int CharXP { get; set; }
    int CharHungerCurrent { get; set; } 
    int CharHungerFull { get; set; } 
    int CharHungerDurationSeconds { get; set; } 
    int CharHitPoints { get; set; }
    int CharMaxHP { get; set; }

    int CharXPPerLevel { get; set; }

    int CharLevelCap { get; set; }


    int CharStrength { get; set; }
    int CharAlacrity { get; set; }
    int CharCelerity { get; set; }
    int CharLimenity { get; set; }
    int CharIntelligence { get; set; }

    // these are specific stats unique to certain character classes
    string CharResourceName { get;}     // "Rage", "Mana", etc.
    int CharResourceValue { get; set; }    // "35", "120", etc.
    string CharRegenLabel { get; }       // "Rage on Hit", "Mana Regen"
    int CharRegenValue { get; set; }

    string CharMaxResourceName { get; }
    int CharMaxResourceValue { get; set; }

    int CharLifeRegen { get; set; }
    int CharStatPoints { get; set; }


    // Character Color Theme Info

    string CharHPColor { get; }
    string CharInvColor { get; }
    string CharEnergyColor { get; }




    // Debugging
    string CharCollisionBoxStyle { get; }
    string CharSpriteDebugStyle { get; }
    string CharPunchBoxStyle { get; }
 //   string CharDebugText { get; }



}









