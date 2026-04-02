
public class BloodLevel
{

    public BloodWyrmService Game { get; set; } = default!;
    public BloodCharacterHandle CharacterHandle { get; set; } = new();
    public IBloodiCharacter? ActiveCharacter => CharacterHandle.ActiveCharacter;
   

    private Random rng = new();

    public void AddXp(string source, double multiplier)
    {
        int baseXpGain = source switch
        {
            "Skeleton" => 1,
            "ZombiePyscho" => 2,
            "SkelPyscho" => 3,
            "SkelWar" => 4,
            "Goatman" => 5,
            "Cow" => 0,
            "Boss" => 10,
            "SkelBoss" => 100,
            "TownSlut" => 0,
            _ => 0
        };

        int finalXp = (int)(baseXpGain * multiplier);
        Game.ActiveCharacter.CharXP += finalXp;
        CheckLevelUp(Game.ActiveCharacter);
    }

    private void CheckLevelUp(IBloodiCharacter character)
    {
        int threshold = GetXPThreshold(character);

        if (character.CharXP >= threshold && character.CharLevel < character.CharLevelCap)
        {
            character.CharLevel++;
            int roll = rng.Next(1, character.CharMaxHP);
            character.CharMaxHP += roll;
            character.CharHitPoints += roll;
            character.CharStatPoints += 2;
        }
    }

    public int GetXPThreshold(IBloodiCharacter character)
    {
        return (int)(character.CharXPPerLevel * Math.Pow(character.CharLevel, 2));
    }

    public void AllocateStat(string stat)
    {
        var character = Game.ActiveCharacter;
        if (character == null || character.CharStatPoints <= 0) return;

        switch (stat)
        {
            case "Strength": character.CharStrength++; break;
            case "Alacrity": character.CharAlacrity++; break;
            case "Celerity": character.CharCelerity++; break;
            case "Limenity": character.CharLimenity++; break;
            case "Intelligence": character.CharIntelligence++; break;
            case "uniqueS1": character.CharResourceValue++; break;
            case "uniqueS2": character.CharRegenValue++; break;
            case "Life Regen": character.CharLifeRegen++; break;
        }

        character.CharStatPoints--;
    }
}
