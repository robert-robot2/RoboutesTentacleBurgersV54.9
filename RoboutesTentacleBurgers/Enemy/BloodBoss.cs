

using RoboutesTentacleBurgers.Services;
using static RoboutesTentacleBurgers.Services.ZIndexCache;

public class BloodBoss
{
 

    public class ScavBoss : IiEnemy
    {

        // IBloodiCharacter implementation
        public void EnemyAttack(IBloodiCharacter active) =>  ScavBossAttack(active);
        public void EnemyMove(IBloodiCharacter active) =>  TickScavBossMovement(active);
        //   public void StopMovement(string key) => StopMovementWarrior(key);
        public void EnemyTickAllAnimation() => TickScavBossAnimation();
        public void SetAggression(IBloodiCharacter active) =>  SetAggressionTarget(active);
        public bool EnemyIsAlive => ScavBossHitPoints > 0;
        public int EnemyX => ScavBossX;
        public int EnemyY => ScavBossY;
        public string SpriteStyle => ScavBossSpriteStyle;
        public string? HitEffectPath => ShowScavBossHitEffect ? "/iAssets/WarriorGothit01.png" : null;
        public Rectangle EnemyCollisionBox => ScavBossCollisionBox;
        public Rectangle EnemyPunchBox => ScavBossPunchBox;
        public void EnemyTakeDamage(int amount) => ScavBossTakeDamage(amount);
        public void ClearHitEffect() => ClearScavBossHitEffects();


        public List<SplatterPuddle> SplatterPuddles { get; } = new();

        // Stats
        public string EnemyClassName => "ScavBoss";
        public int EnemyLevel { get => ScavBossLevel; set => ScavBossLevel = value; }
        public int EnemyXP { get => ScavBossXP; set => ScavBossXP = value; }

        public int EnemyHungerCurrent { get => 0; set { } }
        public int EnemyHungerFull { get => 0; set { } }
        public int EnemyHungerDurationSeconds { get => 0; set { } }

        // public int EnemyHungerCurrent { get => ScavBossHungerCurrent; set => ScavBossHungerCurrent = value; }
        // public int EnemyHungerFull { get => ScavBossHungerFull; set => ScavBossHungerFull = value; }
        // public int EnemyHungerDurationSeconds { get => ScavBossHungerDurationSeconds; set => ScavBossHungerDurationSeconds = value; }

        public int EnemyHitPoints { get => ScavBossHitPoints; set => ScavBossHitPoints = value; }
        public int EnemyMaxHP { get => ScavBossMaxHP; set => ScavBossMaxHP = value; }
        public int EnemyXPPerLevel { get => ScavBossXPPerLevel; set => ScavBossXPPerLevel = value; }

        public int EnemyLevelCap { get => ScavBossLevelCap; set => ScavBossLevelCap = value; }
        public int EnemyStrength { get => scavBossDamageAmount; set => scavBossDamageAmount = value; }
        public int EnemyAlacrity { get => ScavBossAlacrityAmount; set => ScavBossAlacrityAmount = value; }
        public int EnemyCelerity { get => ScavBossMovementSpeed; set => ScavBossMovementSpeed = value; }
        public int EnemyLimenity { get => punchRange; set => punchRange = value; }
        public int EnemyIntelligence { get => SpellDamage; set => SpellDamage = value; }

        // Unique class stats
        public string EnemyResourceName => "Boss Authority";
        public int EnemyResourceValue { get => ScavBossAuthorityPoints; set => ScavBossAuthorityPoints = value; }
        public string EnemyRegenLabel => "Authority on Command";
        public int EnemyRegenValue { get => ScavBossAuthorityOnHit; set => ScavBossAuthorityOnHit = value; }
        public string EnemyMaxResourceName => "Max Boss Authority";
        public int EnemyMaxResourceValue { get => ScavBossMaxAuthorityPoints; set => ScavBossMaxAuthorityPoints = value; }
        public int EnemyLifeRegen { get => ScavBossLifeRegenRate; set => ScavBossLifeRegenRate = value; }
        public int EnemyStatPoints { get => ScavBossStatPoints; set => ScavBossStatPoints = value; }

        // Enemy Color Theme Info
        public string EnemyHPColor => "rgba(178,34,34,.8)";    // firebrick red
        public string EnemyInvColor => "rgba(255,140,0,1.0)";  // dark orange
        public string EnemyEnergyColor => "rgba(255,255,0,.7)"; // boss gold

        // Debugger
        public string EnemyCollisionBoxStyle => ScavBossCollisionBoxStyle;
        public string EnemySpriteDebugStyle => ScavBossSpriteDebugStyle;
        public string EnemyPunchBoxStyle => ScavBossPunchBoxStyle;
        // Core state



        // XP Gain

        public void AddXp(BloodLevel levelReg, double multiplier)
        {
            levelReg.AddXp("Boss", multiplier);
        }

        public int ScavBossZIndex { get; private set; } = 6000;
        public int ScavBossX { get; set; }
        public int ScavBossY { get; set; }

        public int ScavBossWidth { get; set; } =48;
        public int ScavBossHeight { get; set; } =48;

        public string ScavBossXpx => $"{ScavBossX}px";
        public string ScavBossYpx => $"{ScavBossY}px";

        public int ScavBossPunchX { get; set; } = 24;
        public int ScavBossPunchY { get; set; } = 24;
        public int ScavBossLevelCap { get; set; } = 100;
        public int ScavBossXPPerLevel { get; set; } = 50;
        public int ScavBossXP { get; set; } = 0;
        public int ScavBossLevel { get; set; } = 1;
        public int ScavBossHungerCurrent { get; set; } = 2000;   // starting calories
        public int ScavBossHungerFull { get; set; } = 2000;      // max capacity
        public int ScavBossHungerDurationSeconds { get; set; } = 86400; // 24 hours

        private DateTime _lastHungerTick = DateTime.Now;
        public int ScavBossMaxHP { get; set; } = 15;

        public int ScavBossStatPoints { get; set; } = 0;
        public int ScavBossHitPoints { get; set; } =15 ;

        private int scavBossDamageAmount = 5;

        public int ScavBossAlacrityAmount = 3;

        public  int ScavBossMovementSpeed = 12;

        public int punchRange = 9;

        public int SpellDamage = 0;

        public int ScavBossAuthorityPoints { get; set; } = 0;

        public int ScavBossMaxAuthorityPoints { get; set; } = 10;
        public int ScavBossAuthorityOnHit { get; set; } = 1;


        public int ScavBossLifeRegenRate { get; set; } = 0;
        public int PatrolLeftBound { get; set; } = 24;
        public int PatrolRightBound { get; set; } = 1994;
        public int PatrolTopBound { get; set; } = 24;
        public int PatrolBottomBound { get; set; } = 1994;


        public bool ScavBossIsAlive => ScavBossHitPoints > 0;
        public bool ShowScavBossHitEffect { get; set; } = false;

        private int scavBossFrame = 0;
        private DateTime lastScavBossFrameTime = DateTime.Now;
        private bool isOneShotScavBossAnimation = false;
        private ScavBossAnimationState currentScavBossAnimation = ScavBossAnimationState.Idle;

        public ScavBossAnimationState CurrentScavBossAnimation => currentScavBossAnimation;

        public string ScavBossBackgroundPosition => $"-{scavBossFrame * ScavBossFrameWidth}px 0";

        private bool scavBossMovingRight = true;
        private DateTime scavBossIdleUntil = DateTime.MinValue;
        public DateTime ScavBossIdleUntil => scavBossIdleUntil;

     

        public const int ScavBossFrameWidth = 48;
        public const int ScavBossFrameHeight = 48;
        public Rectangle ScavBossCollisionBox =>
            new Rectangle(
                ScavBossX + ScavBossCollisionShiftX,
                ScavBossY + ScavBossCollisionShiftY,
                ScavBossCollisionWidth,
                ScavBossCollisionHeight
            );

        private const int ScavBossCollisionWidth = 24;
        private const int ScavBossCollisionHeight = 24;
        private const int ScavBossCollisionShiftX = 10;
        private const int ScavBossCollisionShiftY = 16;

        public const int ScavBossPunchShiftX = 10;
        public const int ScavBossPunchShiftY = 10;

        
        public Rectangle ScavBossPunchBox =>
       new Rectangle(
           ScavBossX + ScavBossPunchShiftX - punchRange,
           ScavBossY + ScavBossPunchShiftY- punchRange,
           ScavBossPunchX + (2 * punchRange),
           ScavBossPunchY + (2 * punchRange)
       );

        public string ScavBossSpriteStyle =>
            $"position:absolute; left:{ScavBossX}px; top:{ScavBossY}px; " +
            $"width:{ScavBossWidth}px; height:{ScavBossHeight}px; " +
            $"background-image:url('{ScavBossSpriteSheet}'); " +
            $"background-position:-{scavBossFrame * ScavBossWidth}px 0px; " +
            $"background-repeat:no-repeat; background-color:transparent; " +
            $"opacity:0.95; user-select:none; touch-action:manipulation; z-index:{ScavBossZIndex};";


        private static readonly Random Randomizer = new Random();
        public void ScavBossTakeDamage(int amount = 1)
        {
            ScavBossHitPoints = Math.Max(ScavBossHitPoints - amount, 0);
            ShowScavBossHitEffect = true;
            BloodSplatterRegistry.Add(new SplatterPuddle
            {
                X = EnemyCollisionBox.X + (int)(Randomizer.NextDouble() * 10 - 5), // ±5px
                Y = EnemyCollisionBox.Y + (int)(Randomizer.NextDouble() * 10 - 5),

                Scale = Math.Min(1.0 + (amount * 0.25), 3.0) * (0.8 + Randomizer.NextDouble() * 0.4),


            });
        }

        public void ClearScavBossHitEffects() => ShowScavBossHitEffect = false;

        private DateTime lastScavBossAttackTime = DateTime.MinValue;
        private bool isAttacking = false;

        public void ScavBossAttack(IBloodiCharacter active)
        {
            int attackIntervalMs = 1000 / Math.Max(1, ScavBossAlacrityAmount);

            if ((DateTime.Now - lastScavBossAttackTime).TotalMilliseconds < attackIntervalMs)
                return;
            if (isAttacking) return; // guard

            lastScavBossAttackTime = DateTime.Now;
            isAttacking = true;

            _ = Task.Run(async () =>
            {
                try
                {
                    SetScavBossAnimation(ScavBossAnimationState.Attack);
                    isOneShotScavBossAnimation = true;

                    int maxFrames = animationFrameCounts.TryGetValue(ScavBossAnimationState.Attack, out var count) ? count : 12;
                    int adjustedDelay = attackIntervalMs / maxFrames;
                    int damageDelayMs = adjustedDelay * Math.Max(1, maxFrames - 3);

                    await Task.Delay(damageDelayMs);

                    if (active.CharIsAlive && ScavBossPunchBox.IntersectsWith(active.CharCollisionBox))
                    {
                        active.CharTakeDamage(scavBossDamageAmount);

                        await Task.Delay(120);
                        active.ClearHitEffect();
                    }
                }
                finally
                {
                    isAttacking = false; // always reset
                }
            });
        }



        public enum ScavBossAnimationState
        {
            Idle,
            WalkLeft,
            WalkRight,
            Attack,
            SpAttack
        }

        private readonly Dictionary<ScavBossAnimationState, int> scavBossAnimationSpeeds = new()
    {
        { ScavBossAnimationState.Idle, 50 },
        { ScavBossAnimationState.WalkLeft, 50 },
        { ScavBossAnimationState.WalkRight, 50 },
           { ScavBossAnimationState.SpAttack, 50 },
        { ScavBossAnimationState.Attack, 50 }
    };

        private readonly Dictionary<ScavBossAnimationState, int> animationFrameCounts = new()
    {
        { ScavBossAnimationState.Idle, 12 },
        { ScavBossAnimationState.WalkLeft, 8 },
        { ScavBossAnimationState.WalkRight, 8 },
           { ScavBossAnimationState.SpAttack, 11},
        { ScavBossAnimationState.Attack, 12 }
    };

        public string ScavBossSpriteSheet => currentScavBossAnimation switch
        {
            ScavBossAnimationState.Idle => "/iAssets/ScavIdle001.png",
            ScavBossAnimationState.WalkLeft => "/iAssets/ScavLeftWalk001.png",
            ScavBossAnimationState.WalkRight => "/iAssets/ScavRightWalk001.png",
            ScavBossAnimationState.Attack => "/iAssets/ScavAttack001.png",
            ScavBossAnimationState.SpAttack => "/iAssets/ScavSPAttack001.png",
            _ => "/iAssets/ScavIdle001.png"
        };

        public void SetScavBossAnimation(ScavBossAnimationState newState)
        {
            if (newState != currentScavBossAnimation)
            {
                scavBossFrame = 0;
                lastScavBossFrameTime = DateTime.Now;
            }
            currentScavBossAnimation = newState;
        }

        // ✅ OPTIMIZED TickScavBossAnimation (same model as Skeleton/Cow/TownSlut)
        private int lastZIndexY = -1;
        private const int ZIndexUpdateThreshold = 20; // Only recalc when moved 20px vertically
        public void TickScavBossAnimation()
        {
            // 🔹 ONLY update Z-index if ScavBoss moved significantly in Y direction
            if (Math.Abs(ScavBossY - lastZIndexY) > ZIndexUpdateThreshold)
            {
                UpdateScavBossZIndex();
                lastZIndexY = ScavBossY;
            }

            // Rest of animation logic stays the same
            int delay = scavBossAnimationSpeeds.TryGetValue(currentScavBossAnimation, out var ms) ? ms : 150;
            int maxFrames = animationFrameCounts.TryGetValue(currentScavBossAnimation, out var count) ? count : 1;

            if ((DateTime.Now - lastScavBossFrameTime).TotalMilliseconds >= delay)
            {
                scavBossFrame++;

                if (isOneShotScavBossAnimation && scavBossFrame >= maxFrames)
                {
                    SetScavBossAnimation(ScavBossAnimationState.Idle);
                    isOneShotScavBossAnimation = false;
                }
                else
                {
                    scavBossFrame %= maxFrames;
                }

                lastScavBossFrameTime = DateTime.Now;
            }
        }

        // 🔹 NEW: Separate Z-index calculation for ScavBoss
        private void UpdateScavBossZIndex()
        {
            var ordered = ZIndexCache.GetSortedStaticObjects();

            foreach (var obj in ordered)
            {
                if (ScavBossY < obj.CollisionBox.Y)
                {
                    ScavBossZIndex = obj.ZIndex - 1;
                    return; // Early exit
                }
            }

            ScavBossZIndex = 6000; // Default if no match
        }

        // 🔹 ALTERNATIVE: Binary search for even faster lookup
        private void UpdateScavBossZIndexFast()
        {
            var ordered = ZIndexCache.GetSortedStaticObjects();

            // Binary search since list is sorted by Y
            int left = 0;
            int right = ordered.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                var obj = ordered[mid];

                if (ScavBossY < obj.CollisionBox.Y)
                {
                    // Check if this is the first object below us
                    if (mid == 0 || ordered[mid - 1].CollisionBox.Y <= ScavBossY)
                    {
                        ScavBossZIndex = obj.ZIndex - 1;
                        return;
                    }
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            ScavBossZIndex = 6000; // No object below us
        }


        public bool HasAggressionTarget { get; private set; } = false;


        public IBloodiCharacter? ActiveCharacter { get; private set; }


        public void SetAggressionTarget(IBloodiCharacter active)
        {
            if (!ScavBossIsAlive) return;

            ActiveCharacter=active;
            HasAggressionTarget = true;
        }

        public void TickScavBossMovement(IBloodiCharacter active)
        {
            if (!ScavBossIsAlive)
                return;


            // Attack if in range and not already attacking
            if (ScavBossIsAlive &&
        currentScavBossAnimation != ScavBossAnimationState.Attack &&
        ScavBossPunchBox.IntersectsWith(active.CharCollisionBox))
            {
                ScavBossAttack(active);
                return;
            }

            // Idle timer
            if (DateTime.Now < scavBossIdleUntil)
            {
                SetScavBossAnimation(ScavBossAnimationState.Idle);
                TickScavBossAnimation();
                return;
            }

            // Aggression targeting simplified: always chase the active character
            if (active != null && active.CharIsAlive)
            {
                int targetCenterX = active.CharX + 42;
                int targetCenterY = active.CharY + 42;

                int scavBossCenterX = ScavBossX + (ScavBossWidth / 2);
                int scavBossCenterY = ScavBossY + (ScavBossHeight / 2);

                if (Math.Abs(scavBossCenterX - targetCenterX) > ScavBossMovementSpeed)
                {
                    if (scavBossCenterX < targetCenterX && ScavBossX + ScavBossMovementSpeed + ScavBossWidth < PatrolRightBound)
                    {
                        ScavBossX += ScavBossMovementSpeed;
                        SetScavBossAnimation(ScavBossAnimationState.WalkRight);
                    }
                    else if (scavBossCenterX > targetCenterX && ScavBossX - ScavBossMovementSpeed > PatrolLeftBound)
                    {
                        ScavBossX -= ScavBossMovementSpeed;
                        SetScavBossAnimation(ScavBossAnimationState.WalkLeft);
                    }
                }

                if (Math.Abs(scavBossCenterY - targetCenterY) > ScavBossMovementSpeed)
                {
                    if (scavBossCenterY < targetCenterY)
                        ScavBossY += ScavBossMovementSpeed;
                    else if (scavBossCenterY > targetCenterY)
                        ScavBossY -= ScavBossMovementSpeed;
                }

                if (Math.Abs(scavBossCenterX - targetCenterX) <= ScavBossMovementSpeed &&
                    Math.Abs(scavBossCenterY - targetCenterY) <= ScavBossMovementSpeed)
                {
                    SetScavBossAnimation(ScavBossAnimationState.Idle);
                }

                TickScavBossAnimation();
                return;
            }

            // Patrol fallback
            if (scavBossMovingRight)
            {
                ScavBossX += ScavBossMovementSpeed;
                SetScavBossAnimation(ScavBossAnimationState.WalkRight);

                if (ScavBossX >= PatrolRightBound)
                {
                    scavBossMovingRight = false;
                    scavBossIdleUntil = DateTime.Now.AddMilliseconds(2000);
                }
            }
            else
            {
                ScavBossX -= ScavBossMovementSpeed;
                SetScavBossAnimation(ScavBossAnimationState.WalkLeft);

                if (ScavBossX <= PatrolLeftBound)
                {
                    scavBossMovingRight = true;
                    scavBossIdleUntil = DateTime.Now.AddMilliseconds(2000);
                }
            }

            TickScavBossAnimation();
        }









        public string ScavBossSpriteDebugStyle =>
            $"position:absolute; left:{ScavBossX}px; top:{ScavBossY}px; " +
            $"width:{ScavBossWidth}px; height:{ScavBossHeight}px; " +
            $"background-color:rgba(0,255,0,0.2); border:1px dashed lime; z-index:998;";

        public string ScavBossCollisionBoxStyle =>
            $"position:absolute; left:{ScavBossX + ScavBossCollisionShiftX}px; top:{ScavBossY + ScavBossCollisionShiftY}px; " +
            $"width:{ScavBossCollisionWidth}px; height:{ScavBossCollisionHeight}px; " +
            $"background-color:rgba(255,0,0,0.3); border:2px solid red; z-index:999;";
        public string ScavBossPunchBoxStyle =>
            $"position:absolute; left:{ScavBossPunchBox.X}px; top:{ScavBossPunchBox.Y}px; " +
            $"width:{ScavBossPunchBox.Width}px; height:{ScavBossPunchBox.Height}px; " +
            $"background-color:rgba(255,0,255,0.3); border:2px solid magenta; z-index:999;";



    }


    public class BloodScavBossRegistry
    {
        public static List<ScavBoss> All = new();

        public static void SpawnScavBoss(int count)
        {
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                All.Add(new ScavBoss
                {
                    ScavBossX = rand.Next(25, 1994),
                    ScavBossY = rand.Next(25, 1994),
                });
            }
        }
    }




















}

