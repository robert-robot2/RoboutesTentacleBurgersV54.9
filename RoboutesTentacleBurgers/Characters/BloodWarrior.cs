using RoboutesTentacleBurgers.Breakables;
using System;
using static BloodStaticObject;
using RoboutesTentacleBurgers.Services;
using static RoboutesTentacleBurgers.Services.ZIndexCache;

public class BloodWarrior : IBloodiCharacter
{

    // IBloodiCharacter implementation
    public void CharAttack(IiEnemy enemyActive, IBreakables breakActive) => WarriorAttack(enemyActive, breakActive);
    public void CharSpecialAttack(IiEnemy enemyActive) => WarriorShield(enemyActive);
    public void CharMove(string key) => MoveWarrior(key);
    public void StopMovement(string key) => StopMovementWarrior(key);
    public void CharTickAllAnimation() => TickAnimation();
    public bool CharIsAlive => WarriorHitPoints > 0;
    public int CharX { get => WarriorX; set => WarriorX = value; }
    public int CharY { get => WarriorY; set => WarriorY = value; }
    public string CharSpriteStyle => WarriorSpriteStyle;
    public string? CharHitEffectPath => ShowWarriorHitEffect ? "/iAssets/WarriorGothit01.png" : null;
    public Rectangle CharCollisionBox => WarriorCollisionBox;
    public Rectangle CharPunchBox => WarriorPunchBox;
    public void CharTakeDamage(int amount) => WarriorTakeDamage(amount);
    public void ClearHitEffect() => ClearWarriorHitEffects();
    public List<SplatterPuddle> SplatterPuddles { get; } = new();

    // Stats
    public string CharClassName => "Warrior";
    public int CharLevel { get => WarriorLevel; set => WarriorLevel = value; }
    public int CharXP { get => WarriorXP; set => WarriorXP = value; }
    public int CharHungerCurrent { get => WarriorHungerCurrent; set => WarriorHungerCurrent = value; }
    public int CharHungerFull { get => WarriorHungerFull; set => WarriorHungerFull = value; }
    public int CharHungerDurationSeconds { get => WarriorHungerDurationSeconds; set => WarriorHungerDurationSeconds = value; }
    public int CharHitPoints { get => WarriorHitPoints; set => WarriorHitPoints = value; }
    public int CharMaxHP { get => WarMaxHP; set => WarMaxHP = value; }
    public int CharXPPerLevel { get => WarXPPerLevel; set => WarXPPerLevel = value; }

    public int CharLevelCap { get => WarLevelCap; set => WarLevelCap = value; }
    public int CharStrength { get => warriorDamageAmount; set => warriorDamageAmount = value; }
    public int CharAlacrity { get => warriorAlacrityAmount; set => warriorAlacrityAmount = value; }
    public int CharCelerity { get => WarMovementSpeed; set => WarMovementSpeed = value; }
    public int CharLimenity { get => punchRange; set => punchRange = value; }
    public int CharIntelligence { get => WarSpellDamage; set => WarSpellDamage = value; }

    // Unique class stats
    public string CharResourceName => "Rage";
    public int CharResourceValue { get => WarriorRagePoints; set => WarriorRagePoints = value; }
    public string CharRegenLabel => "Rage on Hit";
    public int CharRegenValue { get => WarriorRageOnHit; set => WarriorRageOnHit = value; }
    public string CharMaxResourceName => "Max Rage";
    public int CharMaxResourceValue { get => WarriorMaxRagePoints; set => WarriorMaxRagePoints = value; }
    public int CharLifeRegen { get => WarriorLifeRegenRate; set => WarriorLifeRegenRate = value; }
    public int CharStatPoints { get => WarriorStatPoints; set => WarriorStatPoints = value; }


    // Character Color Theme Info
    public string CharHPColor => "rgba(255,0,0,.7)";
    public string CharInvColor => "rgba(255,100,0,1.0)";
    public string CharEnergyColor => "rgba(255,100,0,.7)";




    // Debugger
    public string CharCollisionBoxStyle => WarriorCollisionBoxStyle;
    public string CharSpriteDebugStyle => WarriorSpriteDebugStyle;
    public string CharPunchBoxStyle => WarriorPunchBoxStyle;
    // Core state

    public int WarriorZIndex { get; private set; } = 6000;

    public int WarriorX { get; set; }
    public int WarriorY { get; set; }
    public int WarriorWidth { get; set; } = 84;
    public int WarriorHeight { get; set; } = 84;
    public string WarriorXpx => $"{WarriorX}px";
    public string WarriorYpx => $"{WarriorY}px";
    public int WarPunchX { get; set; } = 24;
    public int WarPunchY { get; set; } = 24;
    public bool WarriorLevelUpTriggered { get; set; }
    public int LastHPGain { get; set; }
    public int WarLevelCap { get; set; } = 100;
    public int WarXPPerLevel { get; set; } = 50;
    public int WarriorXP { get; set; } = 0;
    public int WarriorLevel { get; set; } = 1;
    public int WarriorHungerCurrent { get; set; } = 2000;   // starting calories
    public int WarriorHungerFull { get; set; } = 2000;      // max capacity
    public int WarriorHungerDurationSeconds { get; set; } = 86400; // 24 hours

    private DateTime _lastHungerTick = DateTime.Now;
    public int WarMaxHP { get; set; } = 20;
    public int WarriorStatPoints { get; set; } = 0;




    public int WarriorHitPoints { get; set; } = 0;   // start dead

    public bool ShowWarriorHitEffect { get; set; } = false;




    public int warriorDamageAmount = 1;

    public int warriorAlacrityAmount = 1;

    public int WarMovementSpeed = 6;

    public int punchRange = 15;

    public int WarSpellDamage = 0;



    public int WarriorRagePoints { get; set; } = 0;

    public int WarriorMaxRagePoints { get; set; } = 10;


    //rage on hit not rage regen
    public int WarriorRageOnHit { get; set; } = 1;


    public int WarriorLifeRegenRate { get; set; } = 0;

    private DateTime _lastLifeRegenTime = DateTime.Now;

    public const int FrameWidth = 84;
    public const int FrameHeight = 84;
    public string WarriorBackgroundPosition => $"-{animationFrame * FrameWidth}px 0";

    private int animationFrame = 0;
    private DateTime lastFrameTime = DateTime.Now;
    private bool isOneShotAnimation = false;


    private WarriorAnimationState currentAnimation = WarriorAnimationState.Idle;


    public Rectangle WarriorCollisionBox =>
      new Rectangle(
         WarriorX + WarriorCollisionShiftX,
          WarriorY + WarriorCollisionShiftY,
          WarriorCollisionWidth,
         WarriorCollisionHeight
      );

    public const int WarriorCollisionWidth = 24;
    public const int WarriorCollisionHeight = 24;
    public const int WarriorCollisionShiftX = 32;
    public const int WarriorCollisionShiftY = 48;

    public const int WarriorPunchShiftX = 32;
    public const int WarriorPunchShiftY = 32;


    public Rectangle WarriorPunchBox =>
    new Rectangle(
         WarriorX + WarriorPunchShiftX - punchRange,
                WarriorY + WarriorPunchShiftY - punchRange,
                WarPunchX + (2 * punchRange),
                WarPunchY + (2 * punchRange)
    );




    public string WarriorSpriteStyle =>
           $"position:absolute; left:{WarriorX}px; top:{WarriorY}px; " +
           $"width:{WarriorWidth}px; height:{WarriorHeight}px; " +
           $"background-image:url('{WarriorSpriteSheet}'); " +
           $"background-position:-{animationFrame * WarriorWidth}px 0px; " +
           $"background-repeat:no-repeat; " +
           $"opacity:0.95; user-select:none; touch-action:manipulation; z-index:{WarriorZIndex};";

    private static readonly Random Randomizer = new Random();


    public void WarriorTakeDamage(int amount = 1)
    {
        WarriorHitPoints = Math.Max(WarriorHitPoints - amount, 0);
        ShowWarriorHitEffect = true;

        BloodSplatterRegistry.Add(new SplatterPuddle
        {
            X = CharCollisionBox.X + (int)(Randomizer.NextDouble() * 10 - 5), // ±5px
            Y = CharCollisionBox.Y + (int)(Randomizer.NextDouble() * 10 - 5),

            Scale = Math.Min(1.0 + (amount * 0.25), 3.0) * (0.8 + Randomizer.NextDouble() * 0.4),


        });
    }


    public void ClearWarriorHitEffects()
    {
        ShowWarriorHitEffect = false;

    }


    private void TickWarLifeRegen()
    {
        if ((DateTime.Now - _lastLifeRegenTime).TotalSeconds >= 1)
        {
            WarriorHitPoints = Math.Min(WarriorHitPoints + WarriorLifeRegenRate, WarMaxHP);
            _lastLifeRegenTime = DateTime.Now;
        }
    }


    private void TickWarHungerDegen()
    {
        if (WarriorHitPoints <= 0 || WarriorHungerCurrent <= 0)
            return;

        if ((DateTime.Now - _lastHungerTick).TotalSeconds >= 1)
        {
            // Calculate per-second drain based on full hunger and duration
            double perSecondLoss = (double)WarriorHungerFull / WarriorHungerDurationSeconds;

            WarriorHungerCurrent = (int)Math.Max(0, WarriorHungerCurrent - perSecondLoss);

            _lastHungerTick = DateTime.Now;
        }
    }

    // Add this field to your class
    private bool isAttacking = false;
    public void WarriorAttack(IiEnemy enemyActive, IBreakables breakActive)
    {
        Console.WriteLine($"=== WarriorAttack START ===");
        Console.WriteLine($"HP: {WarriorHitPoints}, Rage: {WarriorRagePoints}, isAttacking: {isAttacking}");

        if (WarriorHitPoints <= 0)
        {
            Console.WriteLine("BLOCKED: HP <= 0");
            return;
        }
        if (WarriorRagePoints >= 10)
        {
            Console.WriteLine("BLOCKED: Rage >= 10");
            return;
        }
        if (isAttacking)
        {
            Console.WriteLine("BLOCKED: Already attacking");
            return;
        }

        isAttacking = true;
        Console.WriteLine("Starting Task.Run...");

        _ = Task.Run(async () =>
        {
            try
            {
                Console.WriteLine("=== TASK STARTED ===");
                WarriorRagePoints += WarriorRageOnHit;
                var warriorPunchBox = WarriorPunchBox;
                Console.WriteLine($"Captured PunchBox: {warriorPunchBox}");

                SetAnimation(WarriorAnimationState.Punch);
                isOneShotAnimation = true;

                int alacrityBoost = Math.Max(1, warriorAlacrityAmount);
                int maxFrames = animationFrameCounts.TryGetValue(WarriorAnimationState.Punch, out var count) ? count : 16;
                int totalCycleDurationMs = 1000 / alacrityBoost;
                int adjustedDelay = totalCycleDurationMs / maxFrames;
                int damageDelayMs = adjustedDelay * Math.Max(1, maxFrames - 3);

                Console.WriteLine($"Delay: {damageDelayMs}ms, waiting...");
                await Task.Delay(damageDelayMs);
                Console.WriteLine("Delay complete! Applying damage now...");

                int breakableCount = 0;
                int enemyCount = 0;

                foreach (var registry in BloodBreakHandle.AllRegistries)
                {
                    foreach (var breakables in registry)
                    {
                        if (breakables.BreakIsAlive && warriorPunchBox.IntersectsWith(breakables.BreakCollisionBox))
                        {
                            breakableCount++;
                            Console.WriteLine($"HIT BREAKABLE at ({breakables.BreakX}, {breakables.BreakY})");
                            breakables.BreakTakeDamage(warriorDamageAmount);
                            await Task.Delay(120);
                            breakables.BreakClearHitEffects();
                        }
                    }
                }

                Console.WriteLine($"Breakables hit: {breakableCount}");
                Console.WriteLine($"Checking enemies in {BloodEnemyHandle.AllRegistries.Count} registries...");

                foreach (var registry in BloodEnemyHandle.AllRegistries)
                {


                    Console.WriteLine($"Registry has {registry.Count()} enemies");
                    foreach (var enemy in registry)
                    {
                        Console.WriteLine($"Enemy VISUAL at ({enemy.EnemyX}, {enemy.EnemyY})");
                        Console.WriteLine($"Enemy COLLISION at {enemy.EnemyCollisionBox}");
                        Console.WriteLine($"Warrior VISUAL at ({WarriorX}, {WarriorY})");
                        Console.WriteLine($"Warrior PUNCH at {warriorPunchBox}");
                        Console.WriteLine($"Enemy at ({enemy.EnemyX}, {enemy.EnemyY}), Alive: {enemy.EnemyIsAlive}, CollisionBox: {enemy.EnemyCollisionBox}");

                        if (enemy.EnemyIsAlive && warriorPunchBox.IntersectsWith(enemy.EnemyCollisionBox))
                        {
                            enemyCount++;
                            Console.WriteLine($"HIT ENEMY! Dealing {warriorDamageAmount * 2} damage");
                            enemy.EnemyTakeDamage(warriorDamageAmount * 2);
                            await Task.Delay(120);
                            enemy.ClearHitEffect();
                        }
                    }
                }

                Console.WriteLine($"Enemies hit: {enemyCount}");
                Console.WriteLine("=== TASK COMPLETE ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!! EXCEPTION IN TASK: {ex.Message}");
                Console.WriteLine($"!!! STACK: {ex.StackTrace}");
            }
            finally
            {
                Console.WriteLine("Finally: resetting isAttacking");
                isAttacking = false;
            }
        });

        Console.WriteLine("Task.Run launched, WarriorAttack exiting");
    }
    public void WarriorShield(IiEnemy enemyActive)
    {
        if (WarriorHitPoints <= 0) return;
        if (WarriorRagePoints <= 4) return;
        if (isAttacking) return; // prevent multiple triggers

        isAttacking = true; // mark shield in progress

        _ = Task.Run(async () =>
        {
            try
            {
                WarriorRagePoints -= 5;

                var warriorPunchBox = WarriorPunchBox;

                SetAnimation(WarriorAnimationState.Shield);
                isOneShotAnimation = true;

                int alacrityBoost = Math.Max(1, warriorAlacrityAmount);
                int maxFrames = animationFrameCounts.TryGetValue(WarriorAnimationState.Shield, out var count) ? count : 16;
                int totalCycleDurationMs = 1000 / alacrityBoost;
                int adjustedDelay = totalCycleDurationMs / maxFrames;

                int damageDelayMs = adjustedDelay * Math.Max(1, maxFrames - 3);

                // 🔹 Wait asynchronously before applying shield damage
                await Task.Delay(damageDelayMs);

                foreach (var registry in BloodBreakHandle.AllRegistries)
                {
                    foreach (var breakables in registry)
                    {
                        if (breakables.BreakIsAlive && warriorPunchBox.IntersectsWith(breakables.BreakCollisionBox))
                        {
                            breakables.BreakTakeDamage(warriorDamageAmount * 2);
                            await Task.Delay(120);
                            breakables.BreakClearHitEffects();
                        }
                    }
                }

                foreach (var registry in BloodEnemyHandle.AllRegistries)
                {
                    foreach (var enemy in registry)
                    {
                        if (enemy.EnemyIsAlive && warriorPunchBox.IntersectsWith(enemy.EnemyCollisionBox))
                        {
                            enemy.EnemyTakeDamage(warriorDamageAmount * 2);
                            await Task.Delay(120);
                            enemy.ClearHitEffect();
                        }
                    }
                }
            }
            finally
            {
                // 🔹 Always reset, even if something throws
                isAttacking = false;
            }
        });
    }



    public Dictionary<WarriorAnimationState, int> animationSpeeds = new()
{
    { WarriorAnimationState.Idle, 120},
    { WarriorAnimationState.WalkDown, 120 },
    { WarriorAnimationState.WalkUp, 120 },
    { WarriorAnimationState.WalkLeft, 120 },
    { WarriorAnimationState.WalkRight, 120 },
    { WarriorAnimationState.Punch, 5 },
         { WarriorAnimationState.Shield, 5 }

};

    public readonly Dictionary<WarriorAnimationState, int> animationFrameCounts = new()
{
    { WarriorAnimationState.Idle, 20 },
    { WarriorAnimationState.WalkDown, 8 },
    { WarriorAnimationState.WalkUp, 8 },
    { WarriorAnimationState.WalkLeft, 8 },
    { WarriorAnimationState.WalkRight, 8 },
    { WarriorAnimationState.Punch, 16 },
                { WarriorAnimationState.Shield, 16 }
};

    public string WarriorSpriteSheet => currentAnimation switch
    {
        WarriorAnimationState.Idle => "/iAssets/WarriorIdlecell2016x8.png",
        WarriorAnimationState.WalkDown => "/iAssets/WarWalkDown01.png",
        WarriorAnimationState.WalkUp => "/iAssets/WarWalkUp01.png",
        WarriorAnimationState.WalkLeft => "/iAssets/WarWalkLeft02.png",
        WarriorAnimationState.WalkRight => "/iAssets/WarWalkRight02.png",
        WarriorAnimationState.Punch => "/iAssets/WarPunch01.png",
        WarriorAnimationState.Shield => "/iAssets/WarShield01.png",
        _ => "/iAssets/WarriorIdlecell2016x8.png"
    };



    public enum WarriorAnimationState
    {
        Idle,
        WalkDown,
        WalkUp,
        WalkLeft,
        WalkRight,
        Punch,
        Shield
    }

    private DateTime animationStartTime = DateTime.Now;


    public void SetAnimation(WarriorAnimationState newState)
    {
        if (WarriorHitPoints <= 0) return;

        if (newState != currentAnimation)
        {
            animationFrame = 0;
            lastFrameTime = DateTime.Now;
            animationStartTime = DateTime.Now;
        }
        currentAnimation = newState;  // ← MOVE THIS LINE OUT HERE!
    }



    // 🔹 Add these fields
    private int lastZIndexY = -1;
    private const int ZIndexUpdateThreshold = 30; // Pixels moved before recalc

    // ... existing code ...

    public void TickAnimation()
    {
        if (WarriorHitPoints <= 0 || WarriorHungerCurrent <= 0) return;

        if (WarriorHitPoints > 0)
        {
            TickWarLifeRegen();
        }

        if (WarriorHungerCurrent > 0)
        {
            TickWarHungerDegen();
        }

        // 🔹 OPTIMIZED: Only update Z-index when moved significantly
        if (Math.Abs(WarriorY - lastZIndexY) > ZIndexUpdateThreshold)
        {
            UpdateWarriorZIndex();
            lastZIndexY = WarriorY;
        }

        // Rest of animation logic (unchanged)
        if (!animationFrameCounts.TryGetValue(currentAnimation, out int maxFrames))
            maxFrames = 1;

        int alacrityBoost = Math.Max(1, warriorAlacrityAmount);

        if (!animationSpeeds.TryGetValue(currentAnimation, out int delayMs))
            delayMs = 100;

        delayMs = delayMs / Math.Max(1, warriorAlacrityAmount);

        if ((DateTime.Now - lastFrameTime).TotalMilliseconds >= delayMs)
        {
            animationFrame++;

            if (isOneShotAnimation && animationFrame >= maxFrames)
            {
                SetAnimation(WarriorAnimationState.Idle);
                isOneShotAnimation = false;
            }
            else
            {
                animationFrame %= maxFrames;
            }

            lastFrameTime = DateTime.Now;
        }
    }

    // 🔹 NEW: Separate Z-index update method
    private void UpdateWarriorZIndex()
    {
        var ordered = ZIndexCache.GetSortedStaticObjects();

        foreach (var obj in ordered)
        {
            if (WarriorY < obj.CollisionBox.Y)
            {
                WarriorZIndex = obj.ZIndex - 1;
                return;
            }
        }

        WarriorZIndex = 6000;
    }


    private int Clamp(int value, int min, int max) => Math.Max(min, Math.Min(max, value));

    private Vector2 lastMoveDirection = Vector2.Zero;

    public void MoveWarrior(string key)
    {
        Console.WriteLine($"[WARRIOR] MoveWarrior called with key: '{key}'");
        Console.WriteLine($"[WARRIOR] HP: {WarriorHitPoints}, Position: ({WarriorX}, {WarriorY})");

        if (WarriorHitPoints <= 0)
        {
            Console.WriteLine("[WARRIOR] ❌ Blocked - HP <= 0");
            return;
        }

        // Simple movement vector lookup
        var dir = key switch
        {
            "w" => new Vector2(0, -1),
            "s" => new Vector2(0, 1),
            "a" => new Vector2(-1, 0),
            "d" => new Vector2(1, 0),
            _ => Vector2.Zero
        };

        Console.WriteLine($"[WARRIOR] Direction: {dir}");

        if (dir == Vector2.Zero)
        {
            Console.WriteLine("[WARRIOR] ❌ Direction is Zero");
            return;
        }

        WarriorX = Clamp(WarriorX + (int)(dir.X * WarMovementSpeed), 0, 1994);
        WarriorY = Clamp(WarriorY + (int)(dir.Y * WarMovementSpeed), 0, 1994);

        Console.WriteLine($"[WARRIOR] New position: ({WarriorX}, {WarriorY})");

        // Only set animation if direction changed
        if (dir != lastMoveDirection)
        {
            SetAnimation(GetWarriorDirection(dir));
            lastMoveDirection = dir;
        }
    }

    public void StopMovementWarrior(string key)
    {
        SetAnimation(WarriorAnimationState.Idle);
        lastMoveDirection = Vector2.Zero; // 🔹 Reset direction tracking
    }

    public WarriorAnimationState GetWarriorDirection(Vector2 dir)
    {
        if (dir == Vector2.Zero) return WarriorAnimationState.Idle;
        if (dir.Y < 0) return WarriorAnimationState.WalkUp;
        if (dir.Y > 0) return WarriorAnimationState.WalkDown;
        if (dir.X < 0) return WarriorAnimationState.WalkLeft;
        return WarriorAnimationState.WalkRight;
    }

    public bool IsCollidingWithWarrior(int x, int y)
    {
        var warriorFutureBox = new Rectangle(
            x + WarriorCollisionShiftX,
            y + WarriorCollisionShiftY,
            WarriorCollisionWidth,
            WarriorCollisionHeight
        );


        //Tag basic coilliosion setup



        return false;
    }

    public string WarriorPunchBoxStyle
    {
        get
        {
            var box = WarriorPunchBox; // or use WarriorPunchBox if you went with the property version
            return $"position:absolute; left:{box.X}px; top:{box.Y}px; " +
                   $"width:{box.Width}px; height:{box.Height}px; " +
                   $"background-color:rgba(255,0,255,0.3); border:2px solid magenta; z-index:999;";
        }
    }

    public string WarriorSpriteDebugStyle =>
   $"position:absolute; left:{WarriorX}px; top:{WarriorY}px; " +
   $"width:{WarriorWidth}px; height:{WarriorHeight}px; " +
   $"background-color:rgba(0,128,255,0.2); border:1px dashed blue; z-index:998;";

    public string WarriorDebugText =>
        $"X: {WarriorX}, Y: {WarriorY}, HP: {WarriorHitPoints}/{WarMaxHP}";

    public string WarriorCollisionBoxStyle
    {
        get
        {
            var box = WarriorCollisionBox;
            return $"position:absolute; left:{box.X}px; top:{box.Y}px; " +
                   $"width:{box.Width}px; height:{box.Height}px; " +
                   $"background-color:rgba(0,255,255,0.3); border:2px solid cyan; z-index:999;";
        }
    }

}
