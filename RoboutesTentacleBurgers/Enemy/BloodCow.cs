using RoboutesTentacleBurgers.Services;
using static BloodEnemyBoss.Skeleton;
using static RoboutesTentacleBurgers.Services.ZIndexCache;


public class BloodCow
{

   
    public class Cow : IiEnemy
    {

        // IBloodiCharacter implementation
        public void EnemyAttack(IBloodiCharacter active) =>  CowAttack(active);
        public void EnemyMove(IBloodiCharacter active) =>  TickCowMovement(active);
        //   public void StopMovement(string key) => StopMovementWarrior(key);
        public void EnemyTickAllAnimation() => TickCowAnimation();
        public void SetAggression(IBloodiCharacter active) =>  SetAggressionTarget(active);
        public bool EnemyIsAlive => CowHitPoints > 0;
        public int EnemyX => CowX;
        public int EnemyY => CowY;
        public string SpriteStyle => CowSpriteStyle;
        public string? HitEffectPath => ShowCowHitEffect ? "/iAssets/WarriorGothit01.png" : null;
        public Rectangle EnemyCollisionBox => CowCollisionBox;
        public Rectangle EnemyPunchBox => CowPunchBox;
        public void EnemyTakeDamage(int amount) => CowTakeDamage(amount);
        public void ClearHitEffect() => ClearCowHitEffects();

        public List<SplatterPuddle> SplatterPuddles { get; } = new();

        // Stats
        public string EnemyClassName => "Cow";
        public int EnemyLevel { get => CowLevel; set => CowLevel = value; }
        public int EnemyXP { get => CowXP; set => CowXP = value; }

        public int EnemyHungerCurrent { get => 0; set { } }
        public int EnemyHungerFull { get => 0; set { } }
        public int EnemyHungerDurationSeconds { get => 0; set { } }

        // public int EnemyHungerCurrent { get => CowHungerCurrent; set => CowHungerCurrent = value; }
        // public int EnemyHungerFull { get => CowHungerFull; set => CowHungerFull = value; }
        // public int EnemyHungerDurationSeconds { get => CowHungerDurationSeconds; set => CowHungerDurationSeconds = value; }

        public int EnemyHitPoints { get => CowHitPoints; set => CowHitPoints = value; }
        public int EnemyMaxHP { get => CowMaxHP; set => CowMaxHP = value; }
        public int EnemyXPPerLevel { get => CowXPPerLevel; set => CowXPPerLevel = value; }

        public int EnemyLevelCap { get => CowLevelCap; set => CowLevelCap = value; }
        public int EnemyStrength { get => cowDamageAmount; set => cowDamageAmount = value; }
        public int EnemyAlacrity { get => CowAlacrityAmount; set => CowAlacrityAmount = value; }
        public int EnemyCelerity { get => CowMovementSpeed; set => CowMovementSpeed = value; }
        public int EnemyLimenity { get => punchRange; set => punchRange = value; }
        public int EnemyIntelligence { get => SpellDamage; set => SpellDamage = value; }

        // Unique class stats
        public string EnemyResourceName => "Milk Vitality";
        public int EnemyResourceValue { get => CowMilkPoints; set => CowMilkPoints = value; }
        public string EnemyRegenLabel => "Milk on Graze";
        public int EnemyRegenValue { get => CowMilkOnGraze; set => CowMilkOnGraze = value; }
        public string EnemyMaxResourceName => "Max Milk Vitality";
        public int EnemyMaxResourceValue { get => CowMaxMilkPoints; set => CowMaxMilkPoints = value; }
        public int EnemyLifeRegen { get => CowLifeRegenRate; set => CowLifeRegenRate = value; }
        public int EnemyStatPoints { get => CowStatPoints; set => CowStatPoints = value; }

        // Enemy Color Theme Info
        public string EnemyHPColor => "rgba(139,69,19,.8)";     // brown hide
        public string EnemyInvColor => "rgba(255,255,255,1.0)"; // white milk
        public string EnemyEnergyColor => "rgba(0,128,0,.7)";   // pasture green

        // Debugger
        public string EnemyCollisionBoxStyle => CowCollisionBoxStyle;
        public string EnemySpriteDebugStyle => CowSpriteDebugStyle;
        public string EnemyPunchBoxStyle => CowPunchBoxStyle;
        // Core state






        // XP Gain
        public void AddXp(BloodLevel levelReg, double multiplier)
        {
            levelReg.AddXp("Cow", multiplier);
        }

        public int CowZIndex { get; private set; } = 6000;
        public int CowX { get; set; }
        public int CowY { get; set; }

        public int CowWidth { get; set; } = 128;
        public int CowHeight { get; set; } = 84;


        public string CowXpx => $"{CowX}px";
        public string CowYpx => $"{CowY}px";

        public int CowPunchX { get; set; } = 24;
        public int CowPunchY { get; set; } = 24;
        public int CowLevelCap { get; set; } = 100;
        public int CowXPPerLevel { get; set; } = 50;
        public int CowXP { get; set; } = 0;
        public int CowLevel { get; set; } = 1;
        public int CowHungerCurrent { get; set; } = 2000;   // starting calories
        public int CowHungerFull { get; set; } = 2000;      // max capacity
        public int CowHungerDurationSeconds { get; set; } = 86400; // 24 hours

        private DateTime _lastHungerTick = DateTime.Now;
        public int CowMaxHP { get; set; } = 100000;

        public int CowStatPoints { get; set; } = 0;
        public int CowHitPoints { get; set; } = 100000;

        private int cowDamageAmount = 1000;

        public int CowAlacrityAmount = 1;

        public int CowMovementSpeed = 4;

        public int punchRange = 15;

        public int SpellDamage = 0;

        public int CowMilkPoints { get; set; } = 0;

        public int CowMilkOnGraze { get; set; } = 10;
        public int CowMaxMilkPoints { get; set; } = 1;


        public int CowLifeRegenRate { get; set; } = 0;


        public bool CowIsAlive => CowHitPoints > 0;
        public bool ShowCowHitEffect { get; set; } = false;

        private int cowFrame = 0;
        private DateTime lastCowFrameTime = DateTime.Now;
        private bool isOneShotCowAnimation = false;
        private CowAnimationState currentCowAnimation = CowAnimationState.Idle;

        public CowAnimationState CurrentCowAnimation => currentCowAnimation;

        public string CowBackgroundPosition => $"-{cowFrame * CowFrameWidth}px 0";

        private DateTime cowIdleUntil = DateTime.MinValue;
        public DateTime CowIdleUntil => cowIdleUntil;

        public int PatrolLeftBound { get; set; } = 24;
        public int PatrolRightBound { get; set; } = 1994;
        public int PatrolTopBound { get; set; } = 24;
        public int PatrolBottomBound { get; set; } = 1994;


        public const int CowFrameWidth = 84;
        public const int CowFrameHeight = 84;

        public Rectangle CowCollisionBox =>
            new Rectangle(
                CowX + CowCollisionShiftX,
                CowY + CowCollisionShiftY,
                CowCollisionWidth,
                CowCollisionHeight
            );

        private const int CowCollisionWidth = 24;
        private const int CowCollisionHeight = 24;
        private const int CowCollisionShiftX = 32;
        private const int CowCollisionShiftY = 48;

        public const int CowPunchShiftX = 32;
        public const int CowPunchShiftY = 32;

       

        public Rectangle CowPunchBox =>
                  new Rectangle(
                      CowX + CowPunchShiftX - punchRange,
                      CowY + CowPunchShiftY - punchRange,
                      CowPunchX + (2 * punchRange),
                      CowPunchY + (2 * punchRange)
                  );

        public string CowSpriteStyle =>
            $"position:absolute; left:{CowX}px; top:{CowY}px; " +
            $"width:{CowWidth}px; height:{CowHeight}px; " +
            $"background-image:url('{CowSpriteSheet}'); " +
            $"background-position:-{cowFrame * CowWidth}px 0px; " +
            $"background-repeat:no-repeat; background-color:transparent; " +
            $"opacity:0.95; user-select:none; touch-action:manipulation; z-index:{CowZIndex};";

        private static readonly Random Randomizer = new Random();
        public void CowTakeDamage(int amount = 1)
        {
            CowHitPoints = Math.Max(CowHitPoints - amount, 0);
            ShowCowHitEffect = true;
            BloodSplatterRegistry.Add(new SplatterPuddle
            {
                X = EnemyCollisionBox.X + (int)(Randomizer.NextDouble() * 10 - 5), // ±5px
                Y = EnemyCollisionBox.Y + (int)(Randomizer.NextDouble() * 10 - 5),

                Scale = Math.Min(1.0 + (amount * 0.25), 3.0) * (0.8 + Randomizer.NextDouble() * 0.4),


            });
        }

        public void ClearCowHitEffects() => ShowCowHitEffect = false;

        private DateTime lastCowAttackTime = DateTime.MinValue;
        private bool isAttacking = false;

        public void CowAttack(IBloodiCharacter active)
        {
            int attackIntervalMs = 1000 / Math.Max(1, CowAlacrityAmount);

            if ((DateTime.Now - lastCowAttackTime).TotalMilliseconds < attackIntervalMs)
                return;
            if (isAttacking) return; // guard

            lastCowAttackTime = DateTime.Now;
            isAttacking = true;

            _ = Task.Run(async () =>
            {
                try
                {
                    SetCowAnimation(CowAnimationState.Attack);
                    isOneShotCowAnimation = true;

                    int maxFrames = animationFrameCounts.TryGetValue(CowAnimationState.Attack, out var count) ? count : 12;
                    int adjustedDelay = attackIntervalMs / maxFrames;
                    int damageDelayMs = adjustedDelay * Math.Max(1, maxFrames - 3);

                    await Task.Delay(damageDelayMs);

                    if (active.CharIsAlive && CowPunchBox.IntersectsWith(active.CharCollisionBox))
                    {
                        active.CharTakeDamage(cowDamageAmount);

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


        public enum CowAnimationState
        {
            Idle,
            Attack
        }

        private readonly Dictionary<CowAnimationState, int> cowAnimationSpeeds = new()
    {
        { CowAnimationState.Idle, 50 },
        { CowAnimationState.Attack, 50 }
    };

        private readonly Dictionary<CowAnimationState, int> animationFrameCounts = new()
    {
        { CowAnimationState.Idle, 12 },
        { CowAnimationState.Attack, 12 }
    };

        public string CowSpriteSheet => currentCowAnimation switch
        {
            CowAnimationState.Idle => "/iAssets/CowIdle01.png",
            CowAnimationState.Attack => "/iAssets/CowAttack01.png",
            _ => "/iAssets/CowIdle01.png"
        };

        public void SetCowAnimation(CowAnimationState newState)
        {
            if (newState != currentCowAnimation)
            {
                cowFrame = 0;
                lastCowFrameTime = DateTime.Now;
            }
            currentCowAnimation = newState;
        }

  
        // 🔥 OPTIMIZED Z-INDEX CALCULATION FOR ENEMIES

        // Add these fields to your enemy classes (Skeleton, Cat, Cow, TownSlut):
        private int lastZIndexY = -1;
        private const int ZIndexUpdateThreshold = 20; // Only recalc when moved 20px vertically

        // ✅ OPTIMIZED TickCowAnimation (same model as Skeleton)
        public void TickCowAnimation()
        {
            // 🔹 ONLY update Z-index if cow moved significantly in Y direction
            if (Math.Abs(CowY - lastZIndexY) > ZIndexUpdateThreshold)
            {
                UpdateCowZIndex();
                lastZIndexY = CowY;
            }

            // Rest of animation logic stays the same
            int delay = cowAnimationSpeeds.TryGetValue(currentCowAnimation, out var ms) ? ms : 150;
            int maxFrames = animationFrameCounts.TryGetValue(currentCowAnimation, out var count) ? count : 1;

            if ((DateTime.Now - lastCowFrameTime).TotalMilliseconds >= delay)
            {
                cowFrame++;

                if (isOneShotCowAnimation && cowFrame >= maxFrames)
                {
                    SetCowAnimation(CowAnimationState.Idle);
                    isOneShotCowAnimation = false;
                }
                else
                {
                    cowFrame %= maxFrames;
                }

                lastCowFrameTime = DateTime.Now;
            }
        }

        // 🔹 NEW: Separate Z-index calculation for Cow
        private void UpdateCowZIndex()
        {
            var ordered = ZIndexCache.GetSortedStaticObjects();

            foreach (var obj in ordered)
            {
                if (CowY < obj.CollisionBox.Y)
                {
                    CowZIndex = obj.ZIndex - 1;
                    return; // Early exit
                }
            }

            CowZIndex = 6000; // Default if no match
        }

        // 🔹 ALTERNATIVE: Binary search for even faster lookup
        private void UpdateCowZIndexFast()
        {
            var ordered = ZIndexCache.GetSortedStaticObjects();

            // Binary search since list is sorted by Y
            int left = 0;
            int right = ordered.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                var obj = ordered[mid];

                if (CowY < obj.CollisionBox.Y)
                {
                    // Check if this is the first object below us
                    if (mid == 0 || ordered[mid - 1].CollisionBox.Y <= CowY)
                    {
                        CowZIndex = obj.ZIndex - 1;
                        return;
                    }
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            CowZIndex = 6000; // No object below us
        }





        public void SetAggressionTarget(IBloodiCharacter active)
        {

        }

        public void TickCowMovement(IBloodiCharacter active)
        {
            if (!CowIsAlive)
                return;


            // Attack if in range and not already attacking
            if (CowIsAlive &&
        currentCowAnimation != CowAnimationState.Attack &&
        CowPunchBox.IntersectsWith(active.CharCollisionBox))
            {
                CowAttack(active);
                return;
            }

            if (DateTime.Now < cowIdleUntil)
            {
                SetCowAnimation(CowAnimationState.Idle);
                TickCowAnimation();
                return;
            }
        }




        public string CowPunchBoxStyle =>
            $"position:absolute; left:{CowPunchBox.X}px; top:{CowPunchBox.Y}px;" +
            $"width:{CowPunchBox.Width}px; height:{CowPunchBox.Height}px;" +
            $"background-color:rgba(255,0,255,0.3); border:2px solid magenta; z-index:999;";


        public string CowSpriteDebugStyle =>
            $"position:absolute; left:{CowX}px; top:{CowY}px;" +
            $"width:{CowWidth}px; height:{CowHeight}px;" +
            $"background-color:rgba(0,255,0,0.2); border:1px dashed lime; z-index:998;";

        public string CowCollisionBoxStyle =>
            $"position:absolute; left:{CowX + CowCollisionShiftX}px; top:{CowY + CowCollisionShiftY}px;" +
            $"width:{CowCollisionWidth}px; height:{CowCollisionHeight}px;" +
            $"background-color:rgba(255,0,0,0.3); border:2px solid red; z-index:999;";
    }


    public class BloodCowRegistry
    {
        public static List<Cow> All = new();
     

        public static void SpawnCows(int count)
        {
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                All.Add(new Cow
                {
                    CowX = rand.Next(25, 1994),
                    CowY = rand.Next(25, 1994),
                  
                });
            }
        }
    }




}

