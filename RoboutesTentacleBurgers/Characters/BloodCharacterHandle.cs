using static BloodStaticObject;

public class BloodCharacterHandle
{
    public IBloodiCharacter? ActiveCharacter { get; set; }
    public bool HasSpawned { get; set; } = false;

    public BloodCharacterHandle()
    {
        ActiveCharacter = null;
    }

    public void SpawnWarrior()
    {
        var warrior = new BloodWarrior();
        SetActiveCharacter(warrior);
        InitializePosition();
    }

    public void SpawnMage()
    {
        var mage = new BloodMage();
        SetActiveCharacter(mage);
        InitializePosition();
    }

    public void SpawnRogue()
    {
        var rogue = new BloodRogue();
        SetActiveCharacter(rogue);
        InitializePosition();
    }

    public void SpawnMonk()
    {
        var monk = new BloodMonk();
        SetActiveCharacter(monk);
        InitializePosition();
    }

    private void InitializePosition()
    {
        if (ActiveCharacter is null) return;
        ActiveCharacter.CharX = 25;
        ActiveCharacter.CharY = 25;
    }

    public void SetActiveCharacter(IBloodiCharacter character)
    {
        ActiveCharacter = character;
        ResetHitPoints();
        HasSpawned = true;
    }

    private void ResetHitPoints()
    {
        switch (ActiveCharacter)
        {
            case BloodWarrior warrior:
                if (warrior.WarriorHitPoints <= 0)
                    warrior.WarriorHitPoints = warrior.WarMaxHP;
                break;
            case BloodMage mage:
                if (mage.MageHitPoints <= 0)
                    mage.MageHitPoints = mage.MageMaxHP;
                break;
            case BloodRogue rogue:
                if (rogue.RogueHitPoints <= 0)
                    rogue.RogueHitPoints = rogue.RogueMaxHP;
                break;
            case BloodMonk monk:
                if (monk.MonkHitPoints <= 0)
                    monk.MonkHitPoints = monk.MonkMaxHP;
                break;
        }
    }

    public void Clear()
    {
        ActiveCharacter = null;
        HasSpawned = false;

    }

  
}
