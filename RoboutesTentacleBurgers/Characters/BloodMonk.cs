using RoboutesTentacleBurgers.Breakables;
using static BloodStaticObject;
using RoboutesTentacleBurgers.Services;
using static RoboutesTentacleBurgers.Services.ZIndexCache;

public class BloodMonk : IBloodiCharacter
{

    // IBloodiCharacter implementation

    public void CharAttack(IiEnemy enemyActive, IBreakables breakActive) => MonkAttack(enemyActive);
    public void CharSpecialAttack(IiEnemy enemyActive) => Monk10kFist(enemyActive);
    public void CharMove(string key) => MoveMonk(key);
    public void StopMovement(string key) => StopMovementMonk(key);
    public void CharTickAllAnimation() => TickAnimationMonk();
    public bool CharIsAlive => MonkHitPoints > 0;
    public int CharX { get => MonkX; set => MonkX = value; }
    public int CharY { get => MonkY; set => MonkY = value; }
    public string CharSpriteStyle => MonkSpriteStyle;
    public string? CharHitEffectPath => ShowMonkHitEffect ? "/iAssets/WarriorGothit01.png" : null;
    public Rectangle CharCollisionBox => MonkCollisionBox;
    public Rectangle CharPunchBox => MonkPunchBox;
    public void CharTakeDamage(int amount) => MonkTakeDamage(amount);
    public void ClearHitEffect() => ClearMonkHitEffects();

    // Stats
    public string CharClassName => "Monk";
    public int CharLevel { get => MonkLevel; set => MonkLevel = value; }
    public int CharXP { get => MonkXP; set => MonkXP = value; }
    public int CharHungerCurrent { get => MonkHungerCurrent; set => MonkHungerCurrent = value; }
    public int CharHungerFull { get => MonkHungerFull; set => MonkHungerFull = value; }
    public int CharHungerDurationSeconds { get => MonkHungerDurationSeconds; set => MonkHungerDurationSeconds = value; }
    public int CharHitPoints { get => MonkHitPoints; set => MonkHitPoints = value; }
    public int CharMaxHP { get => MonkMaxHP; set => MonkMaxHP = value; }
    public int CharXPPerLevel { get => MonkXPPerLevel; set => MonkXPPerLevel = value; }
    public int CharLevelCap { get => MonkLevelCap; set => MonkLevelCap = value; }
    public int CharStrength { get => MonkMeleeDamage; set => MonkMeleeDamage = value; }
    public int CharAlacrity { get => MonkAlacrityAmount; set => MonkAlacrityAmount = value; }
    public int CharCelerity { get => MonkMovementSpeed; set => MonkMovementSpeed = value; }
    public int CharLimenity { get => MonkpunchRange; set => MonkpunchRange = value; }
    public int CharIntelligence { get => MonkSpellAmount; set => MonkSpellAmount = value; }

    // Unique class stats
    public string CharResourceName => "Kai";
    public int CharResourceValue { get => MonkKaiPoints; set => MonkKaiPoints = value; }
    public string CharRegenLabel => "Kai on Hit";
    public int CharRegenValue { get => MonkKaiOnHit; set => MonkKaiOnHit = value; }
    public string CharMaxResourceName => "Max Kai";
    public int CharMaxResourceValue { get => MonkMaxKaiPoints; set => MonkMaxKaiPoints = value; }
    public int CharLifeRegen { get => MonkLifeRegenRate; set => MonkLifeRegenRate = value; }
    public int CharStatPoints { get => MonkStatPoints; set => MonkStatPoints = value; }


    // Character Color Theme Info
    public string CharHPColor => "rgba(255,0,0,.7)";
    public string CharInvColor => "rgba(255,192,0,1)";
    public string CharEnergyColor => "rgba(255,192,0,.7)";



    // Debugging
    public string CharCollisionBoxStyle => MonkCollisionBoxStyle;
    public string CharSpriteDebugStyle => MonkSpriteDebugStyle;
    public string CharPunchBoxStyle => MonkPunchBoxStyle;

    // Core State

    public int MonkZIndex { get; private set; } = 6000;

    public int MonkX { get; set; }
    public int MonkY { get; set; }
    public int MonkWidth { get; set; } = 84;
    public int MonkHeight { get; set; } = 84;
    public string MonkXpx => $"{MonkX}px";
    public string MonkYpx => $"{MonkY}px";
    public int MonkPunchX { get; set; } = 24;
    public int MonkPunchY { get; set; } = 24;
    public bool MonkLevelUpTriggered { get; set; }
    public int LastHPGain { get; set; }
    public int MonkLevelCap { get; set; } = 100;
    public int MonkXPPerLevel { get; set; } = 50;
    public int MonkXP { get; set; } = 0;
    public int MonkLevel { get; set; } = 1;
    public int MonkStatPoints { get; set; } = 0;

    public int MonkHitPoints { get; set; } = 0;
    public int MonkMaxHP { get; set; } = 4;
    public bool ShowMonkHitEffect { get; set; } = false;

    public int MonkMeleeDamage = 0;
    public int MonkAlacrityAmount = 5;
    public int MonkMovementSpeed = 10;
    public int MonkpunchRange = 3;
    public int MonkSpellAmount = 5;
    public int MonkKaiPoints { get; set; } = 5;

    public int MonkMaxKaiPoints { get; set; } = 5;


    // kai on hit
    public int MonkKaiOnHit { get; set; } = 1;

    public int MonkLifeRegenRate { get; set; } = 0;

    private DateTime _lastLifeRegenTime = DateTime.Now;

    public const int MonkFrameWidth = 84;
    public const int MonkFrameHeight = 84;
    public string MonkBackgroundPosition => $"-{MonkanimationFrame * MonkFrameWidth}px 0";

    private int MonkanimationFrame = 0;
    private DateTime MonklastFrameTime = DateTime.Now;
    private bool MonkisOneShotAnimation = false;

    private MonkAnimationState MonkcurrentAnimation = MonkAnimationState.Idle;

    public Rectangle MonkCollisionBox =>
      new Rectangle(
         MonkX + MonkCollisionShiftX,
         MonkY + MonkCollisionShiftY,
         MonkCollisionWidth,
         MonkCollisionHeight
      );

    public const int MonkCollisionWidth = 24;
    public const int MonkCollisionHeight = 24;
    public const int MonkCollisionShiftX = 32;
    public const int MonkCollisionShiftY = 48;

    public const int MonkPunchShiftX = 32;
    public const int MonkPunchShiftY = 32;

    public Rectangle MonkPunchBox =>
    new Rectangle(
         MonkX + MonkPunchShiftX - MonkpunchRange,
         MonkY + MonkPunchShiftY - MonkpunchRange,
         MonkPunchX + (2 * MonkpunchRange),
         MonkPunchY + (2 * MonkpunchRange)
    );

    public string MonkSpriteStyle =>
       $"position:absolute; left:{MonkX}px; top:{MonkY}px; " +
       $"width:{MonkWidth}px; height: {MonkHeight}px; " +
       $"background-image:url('{MonkSpriteSheet}'); " +
       $"background-position:-{MonkanimationFrame * MonkWidth}px 0px; " +
       $"background-repeat:no-repeat; " +
       $"opacity:0.95; user-select:none; touch-action:manipulation; z-index:{MonkZIndex};";

    private static readonly Random Randomizer = new Random();

    public void MonkTakeDamage(int amount = 1)
    {
        MonkHitPoints = Math.Max(MonkHitPoints - amount, 0);
        ShowMonkHitEffect = true;

        BloodSplatterRegistry.Add(new SplatterPuddle
        {
            X = CharCollisionBox.X + (int)(Randomizer.NextDouble() * 10 - 5), // ±5px
            Y = CharCollisionBox.Y + (int)(Randomizer.NextDouble() * 10 - 5),

            Scale = Math.Min(1.0 + (amount * 0.25), 3.0) * (0.8 + Randomizer.NextDouble() * 0.4),

        });



    }

    public void ClearMonkHitEffects() => ShowMonkHitEffect = false;

    public List<SplatterPuddle> SplatterPuddles { get; } = new();
    public int MonkHungerCurrent { get; set; } = 2000;   // starting calories
    public int MonkHungerFull { get; set; } = 2000;      // max capacity
    public int MonkHungerDurationSeconds { get; set; } = 86400; // 24 hours

    private DateTime _lastHungerTick = DateTime.Now;

    private void TickMonkHungerDegen()
    {
        if (MonkHungerCurrent <= 0) return;

        if ((DateTime.Now - _lastHungerTick).TotalSeconds >= 1)
        {
            // Calculate per-second drain based on full hunger and duration
            double perSecondLoss = (double)MonkHungerFull / MonkHungerDurationSeconds;

            MonkHungerCurrent = (int)Math.Max(0, MonkHungerCurrent - perSecondLoss);

            _lastHungerTick = DateTime.Now;
        }
    }

    private void TickMonkLifeRegen()
    {
        if ((DateTime.Now - _lastLifeRegenTime).TotalSeconds >= 1)
        {
            MonkHitPoints = Math.Min(MonkHitPoints + MonkLifeRegenRate, MonkMaxHP);
            _lastLifeRegenTime = DateTime.Now;
        }
    }

    // Add this field to your class if not already present
    private bool isAttacking = false;
    public void MonkAttack(IiEnemy enemyActive)
    {
        if (MonkHitPoints <= 0) return;
        if (MonkKaiPoints == 0) return; // not enough mana
        if (isAttacking) return; // prevent multiple triggers

        isAttacking = true; // mark attack in progress

        _ = Task.Run(async () =>
        {
            try
            {
                MonkKaiPoints -= MonkKaiOnHit;

                var monkPunchBox = MonkPunchBox;

                SetAnimationMonk(MonkAnimationState.Punch);
                MonkisOneShotAnimation = true;

                int alacrityBoost = Math.Max(1, MonkAlacrityAmount);
                int maxFrames = MonkanimationFrameCounts.TryGetValue(MonkAnimationState.Punch, out var count) ? count : 17;
                int totalCycleDurationMs = 1000 / alacrityBoost;
                int adjustedDelay = totalCycleDurationMs / maxFrames;

                int damageDelayMs = adjustedDelay * Math.Max(1, maxFrames - 3);

                await Task.Delay(damageDelayMs);

                foreach (var registry in BloodBreakHandle.AllRegistries)
                {
                    foreach (var breakables in registry)
                    {
                        if (breakables.BreakIsAlive && monkPunchBox.IntersectsWith(breakables.BreakCollisionBox))
                        {
                            breakables.BreakTakeDamage(MonkSpellAmount);
                            await Task.Delay(120);
                            breakables.BreakClearHitEffects();
                        }
                    }
                }

                foreach (var registry in BloodEnemyHandle.AllRegistries)
                {
                    foreach (var enemy in registry)
                    {
                        if (enemy.EnemyIsAlive && MonkPunchBox.IntersectsWith(enemy.EnemyCollisionBox))
                        {
                            enemy.EnemyTakeDamage(MonkSpellAmount);
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

    public void Monk10kFist(IiEnemy enemyActive)
    {
        if (MonkHitPoints <= 0) return;
        if (MonkKaiPoints >= 1) return; // not enough mana
        if (isAttacking) return; // prevent multiple triggers

        isAttacking = true; // mark fist in progress

        _ = Task.Run(async () =>
        {
            try
            {
                MonkKaiPoints = MonkKaiOnHit + 4;

                var monkPunchBox = MonkPunchBox;

                SetAnimationMonk(MonkAnimationState.Fist10);
                MonkisOneShotAnimation = true;

                int alacrityBoost = Math.Max(1, MonkAlacrityAmount);
                int maxFrames = MonkanimationFrameCounts.TryGetValue(MonkAnimationState.Fist10, out var count) ? count : 17;
                int totalCycleDurationMs = 1000 / alacrityBoost;
                int adjustedDelay = totalCycleDurationMs / maxFrames;

                int damageDelayMs = adjustedDelay * Math.Max(1, maxFrames - 3);

                await Task.Delay(damageDelayMs);

                foreach (var registry in BloodBreakHandle.AllRegistries)
                {
                    foreach (var breakables in registry)
                    {
                        if (breakables.BreakIsAlive && monkPunchBox.IntersectsWith(breakables.BreakCollisionBox))
                        {
                            breakables.BreakTakeDamage(MonkSpellAmount * 2);
                            await Task.Delay(120);
                            breakables.BreakClearHitEffects();
                        }
                    }
                }

                foreach (var registry in BloodEnemyHandle.AllRegistries)
                {
                    foreach (var enemy in registry)
                    {
                        if (enemy.EnemyIsAlive && MonkPunchBox.IntersectsWith(enemy.EnemyCollisionBox))
                        {
                            enemy.EnemyTakeDamage(MonkSpellAmount * 2);
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



    public readonly Dictionary<MonkAnimationState, int> MonkanimationSpeeds = new()
{
    { MonkAnimationState.Idle, 240},
    { MonkAnimationState.WalkDown, 120 },
    { MonkAnimationState.WalkUp, 120 },
    { MonkAnimationState.WalkLeft, 120 },
    { MonkAnimationState.WalkRight, 120 },
    { MonkAnimationState.Punch, 10 },
    { MonkAnimationState.Fist10, 10 }


};

    public readonly Dictionary<MonkAnimationState, int> MonkanimationFrameCounts = new()
{
    { MonkAnimationState.Idle, 20 },
    { MonkAnimationState.WalkDown, 8 },
    { MonkAnimationState.WalkUp, 8 },
    { MonkAnimationState.WalkLeft, 8 },
    { MonkAnimationState.WalkRight, 8 },
    { MonkAnimationState.Punch, 14 },
    { MonkAnimationState.Fist10, 14 }
};

    public string MonkSpriteSheet => MonkcurrentAnimation switch
    {
        MonkAnimationState.Idle => "/iAssets/MonkIdleCell.png",
        MonkAnimationState.WalkDown => "/iAssets/MonkDownIdleCell.png",
        MonkAnimationState.WalkUp => "/iAssets/MonkUpIdleCell.png",
        MonkAnimationState.WalkLeft => "/iAssets/MonkLeftIdleCell.png",
        MonkAnimationState.WalkRight => "/iAssets/MonkRightidleCell.png",
        MonkAnimationState.Punch => "/iAssets/MonkButtCell.png",
        MonkAnimationState.Fist10 => "/iAssets/Monk10Fist.png",
        _ => "/iAssets/MonkIdleCell.png"
    };

    public enum MonkAnimationState
    {
        Idle,
        WalkDown,
        WalkUp,
        WalkLeft,
        WalkRight,
        Punch,
        Fist10
    }

    private DateTime MonkanimationStartTime = DateTime.Now;
    public void SetAnimationMonk(MonkAnimationState newState)
    {
        if (MonkHitPoints <= 0) return;

        if (newState != MonkcurrentAnimation)
        {
            MonkanimationFrame = 0;
            MonklastFrameTime = DateTime.Now;
            MonkanimationStartTime = DateTime.Now;
        }
        MonkcurrentAnimation = newState;
    }

    // 🔹 Add these fields
    private int lastZIndexY = -1;
    private const int ZIndexUpdateThreshold = 30; // Pixels moved before recalc
    // ✅ OPTIMIZED TickAnimationMonk (same model as Warrior/Rogue)
    public void TickAnimationMonk()
    {
        if (MonkHitPoints <= 0 || MonkHungerCurrent <= 0) return;

        // 🔹 Regen / degen hooks
        if (MonkHitPoints > 0)
        {
            TickMonkLifeRegen();
        }
        if (MonkHungerCurrent > 0)
        {
            TickMonkHungerDegen(); // ✅ fixed typo
        }

        // 🔹 OPTIMIZED: Only update Z-index when moved significantly
        if (Math.Abs(MonkY - lastZIndexY) > ZIndexUpdateThreshold)
        {
            UpdateMonkZIndex();
            lastZIndexY = MonkY;
        }

        // 🔹 Frame timing logic
        if (!MonkanimationFrameCounts.TryGetValue(MonkcurrentAnimation, out int maxFrames))
            maxFrames = 1;

        int alacrityBoost = Math.Max(1, MonkAlacrityAmount);

        if (!MonkanimationSpeeds.TryGetValue(MonkcurrentAnimation, out int delayMs))
            delayMs = 100; // fallback default

        delayMs = delayMs / alacrityBoost;

        if ((DateTime.Now - MonklastFrameTime).TotalMilliseconds >= delayMs)
        {
            MonkanimationFrame++;

            if (MonkisOneShotAnimation && MonkanimationFrame >= maxFrames)
            {
                SetAnimationMonk(MonkAnimationState.Idle);
                MonkisOneShotAnimation = false;
            }
            else
            {
                MonkanimationFrame %= maxFrames;
            }

            MonklastFrameTime = DateTime.Now;
        }
    }

    // 🔹 NEW: Separate Z-index update method
    private void UpdateMonkZIndex()
    {
        var ordered = ZIndexCache.GetSortedStaticObjects();

        foreach (var obj in ordered)
        {
            if (MonkY < obj.CollisionBox.Y)
            {
                MonkZIndex = obj.ZIndex - 1;
                return;
            }
        }

        MonkZIndex = 6000; // Default if no match
    }


    private int Clamp(int value, int min, int max) => Math.Max(min, Math.Min(max, value));

    public void StopMovementMonk(string key)
    {
        SetAnimationMonk(BloodMonk.MonkAnimationState.Idle);
    }

    private Vector2 lastMoveDirection = Vector2.Zero;

    public void MoveMonk(string key)
    {
        if (MonkHitPoints <= 0) return;

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

        MonkX = Clamp(MonkX + (int)(dir.X * MonkMovementSpeed), 0, 1994);
        MonkY = Clamp(MonkY + (int)(dir.Y * MonkMovementSpeed), 0, 1994);

        // Only set animation if direction changed
        if (dir != lastMoveDirection)
        {
            SetAnimationMonk(GetMonkDirection(dir));
            lastMoveDirection = dir;
        }
    }

    public BloodMonk.MonkAnimationState GetMonkDirection(Vector2 dir)
    {
        if (dir == Vector2.Zero) return BloodMonk.MonkAnimationState.Idle;
        if (dir.Y < 0) return BloodMonk.MonkAnimationState.WalkUp;
        if (dir.Y > 0) return BloodMonk.MonkAnimationState.WalkDown;
        if (dir.X < 0) return BloodMonk.MonkAnimationState.WalkLeft;
        return BloodMonk.MonkAnimationState.WalkRight;
    }

    public bool IsCollidingWithMonk(int x, int y)
    {
        var MonkFutureBox = new Rectangle(
            x + BloodMonk.MonkCollisionShiftX,
            y + BloodMonk.MonkCollisionShiftY,
            BloodMonk.MonkCollisionWidth,
            BloodMonk.MonkCollisionHeight
        );

        return false;
    }

    public string MonkSpriteDebugStyle =>
         $"position:absolute; left:{MonkX}px; top:{MonkY}px; " +
         $"width:{MonkWidth}px; height:{MonkHeight}px; " +
         $"background-color:rgba(0,128,255,0.2); border:1px dashed blue; z-index:998;";

    public string MonkCollisionBoxStyle
    {
        get
        {
            var box = MonkCollisionBox;
            return $"position:absolute; left:{box.X}px; top:{box.Y}px; " +
                   $"width:{box.Width}px; height:{box.Height}px; " +
                   $"background-color:rgba(0,255,255,0.3); border:2px solid cyan; z-index:999;";
        }
    }

    public string MonkPunchBoxStyle
    {
        get
        {
            var box = MonkPunchBox;
            return $"position:absolute; left:{box.X}px; top:{box.Y}px; " +
                   $"width:{box.Width}px; height:{box.Height}px; " +
                   $"background-color:rgba(255,0,255,0.3); border:2px solid magenta; z-index:999;";
        }
    }
























}

