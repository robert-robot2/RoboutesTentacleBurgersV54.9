

using RoboutesTentacleBurgers.Breakables;
using static BloodStaticObject;
using RoboutesTentacleBurgers.Services;
using static RoboutesTentacleBurgers.Services.ZIndexCache;

public class BloodMage : IBloodiCharacter
{


    // IBloodiCharacter implementation

    public void CharAttack(IiEnemy enemyActive, IBreakables breakActive) => MageAttack(enemyActive);
    public void CharSpecialAttack(IiEnemy enemyActive) => MageFireWall(enemyActive);
    public void CharMove(string key) => MoveMage(key);
    public void StopMovement(string key) => StopMovementMage(key);
    public void CharTickAllAnimation() => TickAnimationMage();
    public bool CharIsAlive => MageHitPoints > 0;
    public int CharX { get => MageX; set => MageX = value; }
    public int CharY { get => MageY; set => MageY = value; }
    public string CharSpriteStyle => MageSpriteStyle;
    public string? CharHitEffectPath => ShowMageHitEffect ? "/iAssets/WarriorGothit01.png" : null;
    public Rectangle CharCollisionBox => MageCollisionBox;
    public Rectangle CharPunchBox => MagePunchBox;
    public void CharTakeDamage(int amount) => MageTakeDamage(amount);
    public void ClearHitEffect() => ClearMageHitEffects();

    // Stats
    public string CharClassName => "Mage";
    public int CharLevel { get => MageLevel; set => MageLevel = value; }
    public int CharXP { get => MageXP; set => MageXP = value; }
    public int CharHungerCurrent { get => MageHungerCurrent; set => MageHungerCurrent = value; }
    public int CharHungerFull { get => MageHungerFull; set => MageHungerFull = value; }
    public int CharHungerDurationSeconds { get => MageHungerDurationSeconds; set => MageHungerDurationSeconds = value; }
    public int CharHitPoints { get => MageHitPoints; set => MageHitPoints = value; }
    public int CharMaxHP { get => MageMaxHP; set => MageMaxHP = value; }
    public int CharXPPerLevel { get => MageXPPerLevel; set => MageXPPerLevel = value; }
    public int CharLevelCap { get => MageLevelCap; set => MageLevelCap = value; }
    public int CharStrength { get => MageMeleeDamage; set => MageMeleeDamage = value; }
    public int CharAlacrity { get => MageAlacrityAmount; set => MageAlacrityAmount = value; }
    public int CharCelerity { get => MageMovementSpeed; set => MageMovementSpeed = value; }
    public int CharLimenity { get => MagepunchRange; set => MagepunchRange = value; }
    public int CharIntelligence { get => MageDamageAmount; set => MageDamageAmount = value; }

    // Unique class stats
    public string CharResourceName => "Mana";
    public int CharResourceValue { get => MageManaPoints; set => MageManaPoints = value; }
    public string CharRegenLabel => "Mana Regen";
    public int CharRegenValue { get => MageManaRegenRate; set => MageManaRegenRate = value; }
    public string CharMaxResourceName => "Max Mana";
    public int CharMaxResourceValue { get => MageMaxManaPoints; set => MageMaxManaPoints = value; }
    public int CharLifeRegen { get => MageLifeRegenRate; set => MageLifeRegenRate = value; }
    public int CharStatPoints { get => MageStatPoints; set => MageStatPoints = value; }


    // Character Color Theme Info
    public string CharHPColor => "rgba(255,0,0,.7)";
    public string CharInvColor => "rgba(0,0,255,1.0)";
    public string CharEnergyColor => "rgba(0,0,255,.7)";


    // Debugging
    public string CharCollisionBoxStyle => MageCollisionBoxStyle;
    public string CharSpriteDebugStyle => MageSpriteDebugStyle;
    public string CharPunchBoxStyle => MagePunchBoxStyle;
    // Core State

    public int MageZIndex { get; private set; } = 6000;

    public int MageX { get; set; }
    public int MageY { get; set; }
    public int MageWidth { get; set; } = 84;
    public int MageHeight { get; set; } = 84;
    public string MageXpx => $"{MageX}px";
    public string MageYpx => $"{MageY}px";
    public int MagePunchX { get; set; } = 24;
    public int MagePunchY { get; set; } = 24;
    public bool MageLevelUpTriggered { get; set; }
    public int LastHPGain { get; set; }
    public int MageLevelCap { get; set; } = 100;
    public int MageXPPerLevel { get; set; } = 50;
    public int MageXP { get; set; } = 0;
    public int MageLevel { get; set; } = 1;
    public int MageStatPoints { get; set; } = 0;

    public int MageHitPoints { get; set; } = 0;   // start dead
    public int MageMaxHP { get; set; } = 8;
    public bool ShowMageHitEffect { get; set; } = false;



    public int MageAlacrityAmount = 4;

    public int MageMeleeDamage = 0;

    public int MageMovementSpeed = 9;

    public int MagepunchRange = 6;

    public int MageDamageAmount = 4;


    public int MageManaPoints { get; set; } = 20;

    public int MageMaxManaPoints { get; set; } = 20;

    public int MageManaRegenRate { get; set; } = 1;

    private DateTime _lastManaRegenTime = DateTime.Now;
    public int MageLifeRegenRate { get; set; } = 0;

    private DateTime _lastLifeRegenTime = DateTime.Now;

    public string MageBackgroundPosition => $"-{MageanimationFrame * MageFrameWidth}px 0";


    public const int MageFrameWidth = 84;
    public const int MageFrameHeight = 84;


    private int MageanimationFrame = 0;
    private DateTime MagelastFrameTime = DateTime.Now;
    private bool MageisOneShotAnimation = false;


    private MageAnimationState MagecurrentAnimation = MageAnimationState.Idle;

    public Rectangle MageCollisionBox =>
      new Rectangle(
         MageX + MageCollisionShiftX,
          MageY + MageCollisionShiftY,
          MageCollisionWidth,
         MageCollisionHeight
      );

    public const int MageCollisionWidth = 24;
    public const int MageCollisionHeight = 24;
    public const int MageCollisionShiftX = 32;
    public const int MageCollisionShiftY = 48;


    public const int MagePunchShiftX = 32;
    public const int MagePunchShiftY = 32;



    public Rectangle MagePunchBox =>
   new Rectangle(
        MageX + MagePunchShiftX - MagepunchRange,
               MageY + MagePunchShiftY - MagepunchRange,
               MagePunchX + (2 * MagepunchRange),
               MagePunchY + (2 * MagepunchRange)
   );

    public string MageSpriteStyle =>
       $"position:absolute; left:{MageX}px; top:{MageY}px; " +
       $"width:{MageWidth}px; height: {MageHeight}px; " +
       $"background-image:url('{MageSpriteSheet}'); " +
       $"background-position:-{MageanimationFrame * MageWidth}px 0px; " +
       $"background-repeat:no-repeat; " +
       $"opacity:0.95; user-select:none; touch-action:manipulation; z-index:{MageZIndex};";



    private static readonly Random Randomizer = new Random();



    public void MageTakeDamage(int amount = 1)
    {
        MageHitPoints = Math.Max(MageHitPoints - amount, 0);
        ShowMageHitEffect = true;

        BloodSplatterRegistry.Add(new SplatterPuddle
        {
            X = CharCollisionBox.X + (int)(Randomizer.NextDouble() * 10 - 5), // ±5px
            Y = CharCollisionBox.Y + (int)(Randomizer.NextDouble() * 10 - 5),

            Scale = Math.Min(1.0 + (amount * 0.25), 3.0) * (0.8 + Randomizer.NextDouble() * 0.4),

        });

    }
    public void ClearMageHitEffects() => ShowMageHitEffect = false;


    public List<SplatterPuddle> SplatterPuddles { get; } = new();

    public int MageHungerCurrent { get; set; } = 2000;   // starting calories
    public int MageHungerFull { get; set; } = 2000;      // max capacity
    public int MageHungerDurationSeconds { get; set; } = 86400; // 24 hours

    private DateTime _lastHungerTick = DateTime.Now;

    private void TickMageHungerDegen()
    {
        if (MageHungerCurrent <= 0) return;

        if ((DateTime.Now - _lastHungerTick).TotalSeconds >= 1)
        {
            // Calculate per-second drain based on full hunger and duration
            double perSecondLoss = (double)MageHungerFull / MageHungerDurationSeconds;

            MageHungerCurrent = (int)Math.Max(0, MageHungerCurrent - perSecondLoss);

            _lastHungerTick = DateTime.Now;
        }
    }


    private void TickMageLifeRegen()
    {
        if ((DateTime.Now - _lastLifeRegenTime).TotalSeconds >= 1)
        {
            MageHitPoints = Math.Min(MageHitPoints + MageLifeRegenRate, MageMaxHP);
            _lastLifeRegenTime = DateTime.Now;
        }
    }

    private void TickManaRegen()
    {
        if ((DateTime.Now - _lastManaRegenTime).TotalSeconds >= 1)
        {
            MageManaPoints = Math.Min(MageManaPoints + MageManaRegenRate, MageMaxManaPoints);
            _lastManaRegenTime = DateTime.Now;
        }
    }
    // Add this field to your class if not already present
    private bool isAttacking = false;
    public void MageAttack(IiEnemy enemyActive)
    {
        if (MageHitPoints <= 0) return;
        if (MageManaPoints == 0) return; // not enough mana
        if (isAttacking) return; // prevent multiple triggers

        isAttacking = true; // mark attack in progress

        _ = Task.Run(async () =>
        {
            try
            {
                var magePunchBox = MagePunchBox;

                SetAnimationMage(MageAnimationState.Punch);
                MageisOneShotAnimation = true;

                int alacrityBoost = Math.Max(1, MageAlacrityAmount);
                int maxFrames = MageanimationFrameCounts.TryGetValue(MageAnimationState.Punch, out var count) ? count : 12;
                int totalCycleDurationMs = 1000 / alacrityBoost;
                int adjustedDelay = totalCycleDurationMs / maxFrames;

                int damageDelayMs = adjustedDelay * Math.Max(1, maxFrames - 3);

                await Task.Delay(damageDelayMs);

                foreach (var registry in BloodBreakHandle.AllRegistries)
                {
                    foreach (var breakables in registry)
                    {
                        if (breakables.BreakIsAlive && magePunchBox.IntersectsWith(breakables.BreakCollisionBox))
                        {
                            breakables.BreakTakeDamage(MageDamageAmount);
                            await Task.Delay(120);
                            breakables.BreakClearHitEffects();
                        }
                    }
                }

                foreach (var registry in BloodEnemyHandle.AllRegistries)
                {
                    foreach (var enemy in registry)
                    {
                        if (enemy.EnemyIsAlive && MagePunchBox.IntersectsWith(enemy.EnemyCollisionBox))
                        {
                            enemy.EnemyTakeDamage(MageDamageAmount);
                            await Task.Delay(120);
                            enemy.ClearHitEffect();
                        }
                    }
                }
            }
            finally
            {
                isAttacking = false; // always reset
            }
        });
    }

    public void MageFireWall(IiEnemy enemyActive)
    {
        if (MageHitPoints <= 0) return;
        if (MageManaPoints <= 4) return; // not enough mana
        if (isAttacking) return; // prevent multiple triggers

        isAttacking = true; // mark firewall in progress

        _ = Task.Run(async () =>
        {
            try
            {
                MageManaPoints -= 5; // consume mana

                var magePunchBox = MagePunchBox;

                SetAnimationMage(MageAnimationState.FireWall);
                MageisOneShotAnimation = true;

                int alacrityBoost = Math.Max(1, MageAlacrityAmount);
                int maxFrames = MageanimationFrameCounts.TryGetValue(MageAnimationState.FireWall, out var count) ? count : 12;
                int totalCycleDurationMs = 1000 / alacrityBoost;
                int adjustedDelay = totalCycleDurationMs / maxFrames;

                int damageDelayMs = adjustedDelay * Math.Max(1, maxFrames - 3);

                await Task.Delay(damageDelayMs);

                foreach (var registry in BloodBreakHandle.AllRegistries)
                {
                    foreach (var breakables in registry)
                    {
                        if (breakables.BreakIsAlive && magePunchBox.IntersectsWith(breakables.BreakCollisionBox))
                        {
                            breakables.BreakTakeDamage(MageDamageAmount * 2);
                            await Task.Delay(120);
                            breakables.BreakClearHitEffects();
                        }
                    }
                }

                foreach (var registry in BloodEnemyHandle.AllRegistries)
                {
                    foreach (var enemy in registry)
                    {
                        if (enemy.EnemyIsAlive && MagePunchBox.IntersectsWith(enemy.EnemyCollisionBox))
                        {
                            enemy.EnemyTakeDamage(MageDamageAmount * 2);
                            await Task.Delay(120);
                            enemy.ClearHitEffect();
                        }
                    }
                }
            }
            finally
            {
                isAttacking = false; // always reset
            }
        });
    }


    // rest of code here...


    public readonly Dictionary<MageAnimationState, int> MageanimationSpeeds = new()
    {
        { MageAnimationState.Idle, 240},
        { MageAnimationState.WalkDown, 120 },
        { MageAnimationState.WalkUp, 120 },
        { MageAnimationState.WalkLeft, 120 },
        { MageAnimationState.WalkRight, 120 },
        { MageAnimationState.Punch, 10},
         { MageAnimationState.FireWall, 10}


    };
    public readonly Dictionary<MageAnimationState, int> MageanimationFrameCounts = new()
{
    { MageAnimationState.Idle, 20 },
    { MageAnimationState.WalkDown, 8 },
    { MageAnimationState.WalkUp, 8 },
    { MageAnimationState.WalkLeft, 8 },
    { MageAnimationState.WalkRight, 8 },
    { MageAnimationState.Punch, 12},
         { MageAnimationState.FireWall, 10}

};

    public string MageSpriteSheet => MagecurrentAnimation switch
    {
        MageAnimationState.Idle => "/iAssets/MageIdlecell01.png",
        MageAnimationState.WalkDown => "/iAssets/MageWalkDown01.png",
        MageAnimationState.WalkUp => "/iAssets/MageWalkUp01.png",
        MageAnimationState.WalkLeft => "/iAssets/MageWalkLeft01.png",
        MageAnimationState.WalkRight => "/iAssets/MageWalkRight01.png",
        MageAnimationState.Punch => "/iAssets/MageCast02.png",
        MageAnimationState.FireWall => "/iAssets/MageFirewallAttack01.png",
        _ => "/iAssets/MageIdlecell01.png"
    };


    public enum MageAnimationState
    {
        Idle,
        WalkDown,
        WalkUp,
        WalkLeft,
        WalkRight,
        Punch,
        FireWall
    }
    private DateTime MageanimationStartTime = DateTime.Now;

    public void SetAnimationMage(MageAnimationState newState)
    {
        if (MageHitPoints <= 0) return;

        if (newState != MagecurrentAnimation)
        {
            MageanimationFrame = 0;
            MagelastFrameTime = DateTime.Now;
            MageanimationStartTime = DateTime.Now;
        }
        MagecurrentAnimation = newState;
    }

    // 🔹 Add these fields
    private int lastZIndexY = -1;
    private const int ZIndexUpdateThreshold = 30; // Pixels moved before recalc
                                                  // ✅ OPTIMIZED TickAnimationMage (same model as Warrior/Rogue/Monk)
    public void TickAnimationMage()
    {
        if (MageHitPoints <= 0 || MageHungerCurrent <= 0) return;

        // 🔹 Regen / degen hooks
        TickManaRegen();
        if (MageHitPoints > 0)
        {
            TickMageLifeRegen();
        }
        if (MageHungerCurrent > 0)
        {
            TickMageHungerDegen(); // ✅ fixed typo
        }

        // 🔹 OPTIMIZED: Only update Z-index when moved significantly
        if (Math.Abs(MageY - lastZIndexY) > ZIndexUpdateThreshold)
        {
            UpdateMageZIndex();
            lastZIndexY = MageY;
        }

        // 🔹 Frame timing logic
        if (!MageanimationFrameCounts.TryGetValue(MagecurrentAnimation, out int maxFrames))
            maxFrames = 1;

        int alacrityBoost = Math.Max(1, MageAlacrityAmount);

        if (!MageanimationSpeeds.TryGetValue(MagecurrentAnimation, out int delayMs))
            delayMs = 100; // fallback default

        delayMs = delayMs / alacrityBoost;

        if ((DateTime.Now - MagelastFrameTime).TotalMilliseconds >= delayMs)
        {
            MageanimationFrame++;

            if (MageisOneShotAnimation && MageanimationFrame >= maxFrames)
            {
                SetAnimationMage(MageAnimationState.Idle);
                MageisOneShotAnimation = false;
            }
            else
            {
                MageanimationFrame %= maxFrames;
            }

            MagelastFrameTime = DateTime.Now;
        }
    }

    // 🔹 NEW: Separate Z-index update method
    private void UpdateMageZIndex()
    {
        var ordered = ZIndexCache.GetSortedStaticObjects();

        foreach (var obj in ordered)
        {
            if (MageY < obj.CollisionBox.Y)
            {
                MageZIndex = obj.ZIndex - 1;
                return;
            }
        }

        MageZIndex = 6000; // Default if no match
    }



    private int Clamp(int value, int min, int max) => Math.Max(min, Math.Min(max, value));

    public void StopMovementMage(string key)
    {
        SetAnimationMage(BloodMage.MageAnimationState.Idle);

    }


    private Vector2 lastMoveDirection = Vector2.Zero;

    public void MoveMage(string key)
    {
        if (MageHitPoints <= 0) return;

        // Simple movement vector lookup
        var dir = key switch
        {
            "w" => new Vector2(0, -1),  // Up
            "s" => new Vector2(0, 1),   // Down
            "a" => new Vector2(-1, 0),  // Left
            "d" => new Vector2(1, 0),   // Right
            _ => Vector2.Zero
        };

        if (dir == Vector2.Zero) return;

        MageX = Clamp(MageX + (int)(dir.X * MageMovementSpeed), 0, 1994);
        MageY = Clamp(MageY + (int)(dir.Y * MageMovementSpeed), 0, 1994);

        // Only set animation if direction changed
        if (dir != lastMoveDirection)
        {
            SetAnimationMage(GetMageDirection(dir));
            lastMoveDirection = dir;
        }
    }



    public BloodMage.MageAnimationState GetMageDirection(Vector2 dir)
    {
        if (dir == Vector2.Zero) return BloodMage.MageAnimationState.Idle;
        if (dir.Y < 0) return BloodMage.MageAnimationState.WalkUp;
        if (dir.Y > 0) return BloodMage.MageAnimationState.WalkDown;
        if (dir.X < 0) return BloodMage.MageAnimationState.WalkLeft;
        return BloodMage.MageAnimationState.WalkRight;
    }


    public bool IsCollidingWithMage(int x, int y)
    {
        var MageFutureBox = new Rectangle(
            x + BloodMage.MageCollisionShiftX,
            y + BloodMage.MageCollisionShiftY,
            BloodMage.MageCollisionWidth,
            BloodMage.MageCollisionHeight
        );





        return false;
    }





    public string MageSpriteDebugStyle =>
   $"position:absolute; left:{MageX}px; top:{MageY}px; " +
   $"width:{MageWidth}px; height:{MageHeight}px; " +
   $"background-color:rgba(0,128,255,0.2); border:1px dashed blue; z-index:998;";

    public string MageCollisionBoxStyle
    {
        get
        {
            var box = MageCollisionBox;
            return $"position:absolute; left:{box.X}px; top:{box.Y}px; " +
                   $"width:{box.Width}px; height:{box.Height}px; " +
                   $"background-color:rgba(0,255,255,0.3); border:2px solid cyan; z-index:999;";
        }
    }


    public string MagePunchBoxStyle
    {
        get
        {
            var box = MagePunchBox; // or use WarriorPunchBox if you went with the property version
            return $"position:absolute; left:{box.X}px; top:{box.Y}px; " +
                   $"width:{box.Width}px; height:{box.Height}px; " +
                   $"background-color:rgba(255,0,255,0.3); border:2px solid magenta; z-index:999;";
        }
    }




















}

