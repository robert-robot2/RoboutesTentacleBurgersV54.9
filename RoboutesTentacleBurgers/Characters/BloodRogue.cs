
using Microsoft.Win32;
using RoboutesTentacleBurgers.Breakables;
using static BloodStaticObject;
using RoboutesTentacleBurgers.Services;
using static RoboutesTentacleBurgers.Services.ZIndexCache;

public class BloodRogue : IBloodiCharacter
{

    // IBloodiCharacter implementation
    public void CharAttack(IiEnemy enemyActive, IBreakables breakActive) => RogueAttack(enemyActive);
    public void CharSpecialAttack(IiEnemy enemyActive) => RogueDagger(enemyActive);
    public void CharMove(string key) => MoveRogue(key);
    public void StopMovement(string key) => StopMovementRogue(key);
    public void CharTickAllAnimation() => TickAnimationRogue();
    public bool CharIsAlive => RogueHitPoints > 0;
    public int CharX { get => RogueX; set => RogueX = value; }
    public int CharY { get => RogueY; set => RogueY = value; }
    public string CharSpriteStyle => RogueSpriteStyle;
    public string? CharHitEffectPath => ShowRogueHitEffect ? "/iAssets/WarriorGothit01.png" : null;
    public Rectangle CharCollisionBox => RogueCollisionBox;
    public Rectangle CharPunchBox => RoguePunchBox;
    public void CharTakeDamage(int amount) => RogueTakeDamage(amount);
    public void ClearHitEffect() => ClearRogueHitEffects();

    // Stats
    public string CharClassName => "Rogue";
    public int CharLevel { get => RogueLevel; set => RogueLevel = value; }
    public int CharXP { get => RogueXP; set => RogueXP = value; }
    public int CharHungerCurrent { get => RogueHungerCurrent; set => RogueHungerCurrent = value; }
    public int CharHungerFull { get => RogueHungerFull; set => RogueHungerFull = value; }
    public int CharHungerDurationSeconds { get => RogueHungerDurationSeconds; set => RogueHungerDurationSeconds = value; }
    public int CharHitPoints { get => RogueHitPoints; set => RogueHitPoints = value; }
    public int CharMaxHP { get => RogueMaxHP; set => RogueMaxHP = value; }
    public int CharXPPerLevel { get => RogueXPPerLevel; set => RogueXPPerLevel = value; }
    public int CharLevelCap { get => RogueLevelCap; set => RogueLevelCap = value; }
    public int CharStrength { get => RogueMeleeDamage; set => RogueMeleeDamage = value; }
    public int CharAlacrity { get => RogueAlacrityAmount; set => RogueAlacrityAmount = value; }
    public int CharCelerity { get => RogueMovementSpeed; set => RogueMovementSpeed = value; }
    public int CharLimenity { get => RoguepunchRange; set => RoguepunchRange = value; }
    public int CharIntelligence { get => RogueSpellAmount; set => RogueSpellAmount = value; }

    // Unique class stats
    public string CharResourceName => "Energy";
    public int CharResourceValue { get => RogueEnergyPoints; set => RogueEnergyPoints = value; }
    public string CharRegenLabel => "Energy Regen";
    public int CharRegenValue { get => RogueEnergyRegenRate; set => RogueEnergyRegenRate = value; }
    public string CharMaxResourceName => "Max Energy";
    public int CharMaxResourceValue { get => RogueMaxEnergyPoints; set => RogueMaxEnergyPoints = value; }
    public int CharLifeRegen { get => RogueLifeRegenRate; set => RogueLifeRegenRate = value; }
    public int CharStatPoints { get => RogueStatPoints; set => RogueStatPoints = value; }


    // Character Color Theme Info
    public string CharHPColor => "rgba(255,0,0,.7)";
    public string CharInvColor => "rgba(0,255,0,1)";
    public string CharEnergyColor => "rgba(0,255,0,.7)";






    // Debugging
    public string CharCollisionBoxStyle => RogueCollisionBoxStyle;
    public string CharSpriteDebugStyle => RogueSpriteDebugStyle;
    public string CharPunchBoxStyle => RoguePunchBoxStyle;
    // Core State

    public int RogueZIndex { get; private set; } = 6000;

    public int RogueX { get; set; }
    public int RogueY { get; set; }
    public int RogueWidth { get; set; } = 84;
    public int RogueHeight { get; set; } = 84;
    public string RogueXpx => $"{RogueX}px";
    public string RogueYpx => $"{RogueY}px";
    public int RoguePunchX { get; set; } = 24;
    public int RoguePunchY { get; set; } = 24;
    public bool RogueLevelUpTriggered { get; set; }
    public int LastHPGain { get; set; }
    public int RogueLevelCap { get; set; } = 100;
    public int RogueXPPerLevel { get; set; } = 50;
    public int RogueXP { get; set; } = 0;
    public int RogueLevel { get; set; } = 1;
    public int RogueStatPoints { get; set; } = 0;

    public int RogueHitPoints { get; set; } = 0;   // start dead
    public int RogueMaxHP { get; set; } = 12;
    public bool ShowRogueHitEffect { get; set; } = false;


    public int RogueMeleeDamage = 3;

    public int RogueAlacrityAmount = 3;

    public int RogueMovementSpeed = 8;

    public int RoguepunchRange = 9;

    public int RogueSpellAmount = 0;


    public int RogueEnergyPoints { get; set; } = 10;


    public int RogueMaxEnergyPoints { get; set; } = 10;

    public int RogueEnergyRegenRate { get; set; } = 1;

    private DateTime _lastEnergyRegenTime = DateTime.Now;
    public int RogueLifeRegenRate { get; set; } = 0;

    private DateTime _lastLifeRegenTime = DateTime.Now;

    public const int RogueFrameWidth = 84;
    public const int RogueFrameHeight = 84;
    public string RogueBackgroundPosition => $"-{RogueanimationFrame * RogueFrameWidth}px 0";

    private int RogueanimationFrame = 0;
    private DateTime RoguelastFrameTime = DateTime.Now;
    private bool RogueisOneShotAnimation = false;

    private RogueAnimationState RoguecurrentAnimation = RogueAnimationState.Idle;

    public Rectangle RogueCollisionBox =>
      new Rectangle(
         RogueX + RogueCollisionShiftX,
         RogueY + RogueCollisionShiftY,
         RogueCollisionWidth,
         RogueCollisionHeight
      );

    public const int RogueCollisionWidth = 24;
    public const int RogueCollisionHeight = 24;
    public const int RogueCollisionShiftX = 32;
    public const int RogueCollisionShiftY = 48;

    public const int RoguePunchShiftX = 32;
    public const int RoguePunchShiftY = 32;


    public Rectangle RoguePunchBox =>
    new Rectangle(
         RogueX + RoguePunchShiftX - RoguepunchRange,
                RogueY + RoguePunchShiftY - RoguepunchRange,
                RoguePunchX + (2 * RoguepunchRange),
                RoguePunchY + (2 * RoguepunchRange)
    );


    public string RogueSpriteStyle =>
       $"position:absolute; left:{RogueX}px; top:{RogueY}px; " +
       $"width:{RogueWidth}px; height: {RogueHeight}px; " +
       $"background-image:url('{RogueSpriteSheet}'); " +
       $"background-position:-{RogueanimationFrame * RogueWidth}px 0px; " +
       $"background-repeat:no-repeat; " +
       $"opacity:0.95; user-select:none; touch-action:manipulation; z-index:{RogueZIndex};";

    private static readonly Random Randomizer = new Random();

    public void RogueTakeDamage(int amount = 1)
    {
        RogueHitPoints = Math.Max(RogueHitPoints - amount, 0);
        ShowRogueHitEffect = true;


        BloodSplatterRegistry.Add(new SplatterPuddle
        {
            X = CharCollisionBox.X + (int)(Randomizer.NextDouble() * 10 - 5), // ±5px
            Y = CharCollisionBox.Y + (int)(Randomizer.NextDouble() * 10 - 5),

            Scale = Math.Min(1.0 + (amount * 0.25), 3.0) * (0.8 + Randomizer.NextDouble() * 0.4),

        });




    }

    public void ClearRogueHitEffects() => ShowRogueHitEffect = false;


    public List<SplatterPuddle> SplatterPuddles { get; } = new();

    public int RogueHungerCurrent { get; set; } = 2000;   // starting calories
    public int RogueHungerFull { get; set; } = 2000;      // max capacity
    public int RogueHungerDurationSeconds { get; set; } = 86400; // 24 hours

    private DateTime _lastHungerTick = DateTime.Now;

    private void TickRogueHungerDegen()
    {
        if (RogueHungerCurrent <= 0) return;

        if ((DateTime.Now - _lastHungerTick).TotalSeconds >= 1)
        {
            // Calculate per-second drain based on full hunger and duration
            double perSecondLoss = (double)RogueHungerFull / RogueHungerDurationSeconds;

            RogueHungerCurrent = (int)Math.Max(0, RogueHungerCurrent - perSecondLoss);

            _lastHungerTick = DateTime.Now;
        }
    }


    private void TickRogueLifeRegen()
    {
        if ((DateTime.Now - _lastLifeRegenTime).TotalSeconds >= 1)
        {
            RogueHitPoints = Math.Min(RogueHitPoints + RogueLifeRegenRate, RogueMaxHP);
            _lastLifeRegenTime = DateTime.Now;
        }
    }

    private void TickEnergyRegen()
    {
        if ((DateTime.Now - _lastEnergyRegenTime).TotalSeconds >= 1)
        {
            RogueEnergyPoints = Math.Min(RogueEnergyPoints + RogueEnergyRegenRate, RogueMaxEnergyPoints);
            _lastEnergyRegenTime = DateTime.Now;
        }
    }

    // Add this field to your class if not already present
    private bool isAttacking = false;
    public void RogueAttack(IiEnemy enemyActive)
    {
        if (RogueHitPoints <= 0) return;
        if (RogueEnergyPoints == 0) return; // not enough energy
        if (isAttacking) return; // prevent multiple triggers

        isAttacking = true; // mark attack in progress

        _ = Task.Run(async () =>
        {
            try
            {
                var roguePunchBox = RoguePunchBox;

                SetAnimationRogue(RogueAnimationState.Punch);
                RogueisOneShotAnimation = true;

                int alacrityBoost = Math.Max(1, RogueAlacrityAmount);
                int maxFrames = RogueanimationFrameCounts.TryGetValue(RogueAnimationState.Punch, out var count) ? count : 17;
                int totalCycleDurationMs = 1000 / alacrityBoost;
                int adjustedDelay = totalCycleDurationMs / maxFrames;

                int damageDelayMs = adjustedDelay * Math.Max(1, maxFrames - 3);

                await Task.Delay(damageDelayMs);

                foreach (var registry in BloodBreakHandle.AllRegistries)
                {
                    foreach (var breakables in registry)
                    {
                        if (breakables.BreakIsAlive && roguePunchBox.IntersectsWith(breakables.BreakCollisionBox))
                        {
                            breakables.BreakTakeDamage(RogueMeleeDamage);
                            await Task.Delay(120);
                            breakables.BreakClearHitEffects();
                        }
                    }
                }

                foreach (var registry in BloodEnemyHandle.AllRegistries)
                {
                    foreach (var enemy in registry)
                    {
                        if (enemy.EnemyIsAlive && RoguePunchBox.IntersectsWith(enemy.EnemyCollisionBox))
                        {
                            enemy.EnemyTakeDamage(RogueMeleeDamage);
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

    public void RogueDagger(IiEnemy enemyActive)
    {
        if (RogueHitPoints <= 0) return;
        if (RogueEnergyPoints <= 1) return; // not enough energy
        if (isAttacking) return; // prevent multiple triggers

        isAttacking = true; // mark dagger attack in progress

        _ = Task.Run(async () =>
        {
            try
            {
                RogueEnergyPoints -= 2;

                var roguePunchBox = RoguePunchBox;

                SetAnimationRogue(RogueAnimationState.Dags);
                RogueisOneShotAnimation = true;

                int alacrityBoost = Math.Max(1, RogueAlacrityAmount);
                int maxFrames = RogueanimationFrameCounts.TryGetValue(RogueAnimationState.Dags, out var count) ? count : 17;
                int totalCycleDurationMs = 1000 / alacrityBoost;
                int adjustedDelay = totalCycleDurationMs / maxFrames;

                int damageDelayMs = adjustedDelay * Math.Max(1, maxFrames - 3);

                await Task.Delay(damageDelayMs);

                foreach (var registry in BloodBreakHandle.AllRegistries)
                {
                    foreach (var breakables in registry)
                    {
                        if (breakables.BreakIsAlive && roguePunchBox.IntersectsWith(breakables.BreakCollisionBox))
                        {
                            breakables.BreakTakeDamage(RogueMeleeDamage * 2);
                            await Task.Delay(120);
                            breakables.BreakClearHitEffects();
                        }
                    }
                }

                foreach (var registry in BloodEnemyHandle.AllRegistries)
                {
                    foreach (var enemy in registry)
                    {
                        if (enemy.EnemyIsAlive && RoguePunchBox.IntersectsWith(enemy.EnemyCollisionBox))
                        {
                            enemy.EnemyTakeDamage(RogueMeleeDamage * 2);
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








    public readonly Dictionary<RogueAnimationState, int> RogueanimationSpeeds = new()
{
    { RogueAnimationState.Idle, 240},
    { RogueAnimationState.WalkDown, 120 },
    { RogueAnimationState.WalkUp, 120 },
    { RogueAnimationState.WalkLeft, 120 },
    { RogueAnimationState.WalkRight, 120 },
    { RogueAnimationState.Punch, 10 },
     { RogueAnimationState.Dags, 10 }
};

    public readonly Dictionary<RogueAnimationState, int> RogueanimationFrameCounts = new()
{
    { RogueAnimationState.Idle, 20 },
    { RogueAnimationState.WalkDown, 8 },
    { RogueAnimationState.WalkUp, 8 },
    { RogueAnimationState.WalkLeft, 8 },
    { RogueAnimationState.WalkRight, 8 },
    { RogueAnimationState.Punch, 17 },
        { RogueAnimationState.Dags, 10 }
};

    public string RogueSpriteSheet => RoguecurrentAnimation switch
    {
        RogueAnimationState.Idle => "/iAssets/RogueIdleCell01.png",
        RogueAnimationState.WalkDown => "/iAssets/RogueDownCell01.png",
        RogueAnimationState.WalkUp => "/iAssets/RogueUpCell01.png",
        RogueAnimationState.WalkLeft => "/iAssets/RogueLeftCell01.png",
        RogueAnimationState.WalkRight => "/iAssets/RogueRightCell01.png",
        RogueAnimationState.Punch => "/iAssets/RogueKickCell01.png",
        RogueAnimationState.Dags => "/iAssets/RogueDags01.png",

        _ => "/iAssets/RogueIdleCell01.png"
    };

    public enum RogueAnimationState
    {
        Idle,
        WalkDown,
        WalkUp,
        WalkLeft,
        WalkRight,
        Punch,
        Dags
    }

    private DateTime RogueanimationStartTime = DateTime.Now;
    public void SetAnimationRogue(RogueAnimationState newState)
    {
        if (RogueHitPoints <= 0) return;

        if (newState != RoguecurrentAnimation)
        {
            RogueanimationFrame = 0;
            RoguelastFrameTime = DateTime.Now;
            RogueanimationStartTime = DateTime.Now;
        }
        RoguecurrentAnimation = newState;
    }

    // 🔹 Add these fields
    private int lastZIndexY = -1;
    private const int ZIndexUpdateThreshold = 30; // Pixels moved before recalc

    // ✅ OPTIMIZED TickAnimationRogue (same model as Warrior)
    public void TickAnimationRogue()
    {
        if (RogueHitPoints <= 0 || RogueHungerCurrent <= 0) return;

        // 🔹 Regen / degen hooks
        TickEnergyRegen();
        if (RogueHitPoints > 0)
        {
            TickRogueLifeRegen();
        }
        if (RogueHungerCurrent > 0)
        {
            TickRogueHungerDegen();
        }

        // 🔹 OPTIMIZED: Only update Z-index when moved significantly
        if (Math.Abs(RogueY - lastZIndexY) > ZIndexUpdateThreshold)
        {
            UpdateRogueZIndex();
            lastZIndexY = RogueY;
        }

        // 🔹 Frame timing logic
        if (!RogueanimationFrameCounts.TryGetValue(RoguecurrentAnimation, out int maxFrames))
            maxFrames = 1;

        int alacrityBoost = Math.Max(1, RogueAlacrityAmount);

        if (!RogueanimationSpeeds.TryGetValue(RoguecurrentAnimation, out int delayMs))
            delayMs = 100; // fallback default

        delayMs = delayMs / alacrityBoost;

        if ((DateTime.Now - RoguelastFrameTime).TotalMilliseconds >= delayMs)
        {
            RogueanimationFrame++;

            if (RogueisOneShotAnimation && RogueanimationFrame >= maxFrames)
            {
                SetAnimationRogue(RogueAnimationState.Idle);
                RogueisOneShotAnimation = false;
            }
            else
            {
                RogueanimationFrame %= maxFrames;
            }

            RoguelastFrameTime = DateTime.Now;
        }
    }

    // 🔹 NEW: Separate Z-index update method
    private void UpdateRogueZIndex()
    {
        var ordered = ZIndexCache.GetSortedStaticObjects();

        foreach (var obj in ordered)
        {
            if (RogueY < obj.CollisionBox.Y)
            {
                RogueZIndex = obj.ZIndex - 1;
                return;
            }
        }

        RogueZIndex = 6000; // Default if no match
    }


    private int Clamp(int value, int min, int max) => Math.Max(min, Math.Min(max, value));



    public void StopMovementRogue(string key)
    {
        SetAnimationRogue(BloodRogue.RogueAnimationState.Idle);

    }

    private Vector2 lastMoveDirection = Vector2.Zero;


    public void MoveRogue(string key)
    {
        if (RogueHitPoints <= 0) return;

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

        RogueX = Clamp(RogueX + (int)(dir.X * RogueMovementSpeed), 0, 1994);
        RogueY = Clamp(RogueY + (int)(dir.Y * RogueMovementSpeed), 0, 1994);

        // Only set animation if direction changed
        if (dir != lastMoveDirection)
        {
            SetAnimationRogue(GetRogueDirection(dir));
            lastMoveDirection = dir;
        }
    }


    public BloodRogue.RogueAnimationState GetRogueDirection(Vector2 dir)
    {
        if (dir == Vector2.Zero) return BloodRogue.RogueAnimationState.Idle;
        if (dir.Y < 0) return BloodRogue.RogueAnimationState.WalkUp;
        if (dir.Y > 0) return BloodRogue.RogueAnimationState.WalkDown;
        if (dir.X < 0) return BloodRogue.RogueAnimationState.WalkLeft;
        return BloodRogue.RogueAnimationState.WalkRight;
    }




    public bool IsCollidingWithRogue(int x, int y)
    {
        var RogueFutureBox = new Rectangle(
            x + BloodRogue.RogueCollisionShiftX,
            y + BloodRogue.RogueCollisionShiftY,
            BloodRogue.RogueCollisionWidth,
            BloodRogue.RogueCollisionHeight
        );


        return false;
    }




    public string RogueSpriteDebugStyle =>
         $"position:absolute; left:{RogueX}px; top:{RogueY}px; " +
         $"width:{RogueWidth}px; height:{RogueHeight}px; " +
         $"background-color:rgba(0,128,255,0.2); border:1px dashed blue; z-index:998;";

    public string RogueCollisionBoxStyle
    {
        get
        {
            var box = RogueCollisionBox;
            return $"position:absolute; left:{box.X}px; top:{box.Y}px; " +
                   $"width:{box.Width}px; height:{box.Height}px; " +
                   $"background-color:rgba(0,255,255,0.3); border:2px solid cyan; z-index:999;";
        }
    }


    public string RoguePunchBoxStyle
    {
        get
        {
            var box = RoguePunchBox;
            return $"position:absolute; left:{box.X}px; top:{box.Y}px; " +
                   $"width:{box.Width}px; height:{box.Height}px; " +
                   $"background-color:rgba(255,0,255,0.3); border:2px solid magenta; z-index:999;";
        }
    }










}

