

using RoboutesTentacleBurgers.Services;
using static RoboutesTentacleBurgers.Services.ZIndexCache;


public class BloodEnemy 
{
 
   
    public class Skeleton :IiEnemy
    {
     

        // IBloodiCharacter implementation
        public void EnemyAttack(IBloodiCharacter active) => SkeletonAttack(active);
       public void EnemyMove(IBloodiCharacter active) =>  TickSkeletonMovement(active);
    //   public void StopMovement(string key) => StopMovementWarrior(key);
       public void EnemyTickAllAnimation() =>  TickSkeletonAnimation();

        public void SetAggression(IBloodiCharacter active)=>  SetAggressionTarget(active);
        public bool EnemyIsAlive => SkeletonHitPoints > 0;
       public int EnemyX => SkeletonX;
       public int EnemyY => SkeletonY;
        public string SpriteStyle => SkeletonSpriteStyle;
        public string? HitEffectPath => ShowSkeletonHitEffect ? "/iAssets/SkeleHit01.png" : null;
        public Rectangle EnemyCollisionBox => SkeletonCollisionBox;
        public Rectangle EnemyPunchBox => SkeletonPunchBox;
        public void EnemyTakeDamage(int amount) => SkeletonTakeDamage(amount);
        public void ClearHitEffect() => ClearSkeletonHitEffects();
        public List<SplatterPuddle> SplatterPuddles { get; } = new();

        // Stats
        public string EnemyClassName => "Skeleton";
        public int EnemyLevel { get => SkeletonLevel; set => SkeletonLevel = value; }
        public int EnemyXP { get => SkeletonXP; set => SkeletonXP = value; }

        public int EnemyHungerCurrent { get => 0; set { } }
        public int EnemyHungerFull { get => 0; set { } }
        public int EnemyHungerDurationSeconds { get => 0; set { } }

      //  public int EnemyHungerCurrent { get => SkeletonHungerCurrent; set => SkeletonHungerCurrent = value; }
     //   public int EnemyHungerFull { get => SkeletonHungerFull; set => SkeletonHungerFull = value; }
     //   public int EnemyHungerDurationSeconds { get => SkeletonHungerDurationSeconds; set => SkeletonHungerDurationSeconds = value; }
        public int EnemyHitPoints { get => SkeletonHitPoints; set => SkeletonHitPoints = value; }
        public int EnemyMaxHP { get => SkeletonMaxHP; set => SkeletonMaxHP = value; }
        public int EnemyXPPerLevel { get => SkeletonXPPerLevel; set => SkeletonXPPerLevel = value; }

        public int EnemyLevelCap { get => SkeletonLevelCap; set => SkeletonLevelCap = value; }
        public int EnemyStrength { get => skeletonDamageAmount; set => skeletonDamageAmount = value; }
        public int EnemyAlacrity { get => SkeletonAlacrityAmount; set => SkeletonAlacrityAmount = value; }
        public int EnemyCelerity { get => SkeletonMovementSpeed; set => SkeletonMovementSpeed = value; }
        public int EnemyLimenity { get => punchRange; set => punchRange = value; }
        public int EnemyIntelligence { get => SkeleSpellDamage; set => SkeleSpellDamage = value; }

        // Unique class stats
        public string EnemyResourceName => "Bone Rage";
        public int EnemyResourceValue { get => SkeletonRagePoints; set => SkeletonRagePoints = value; }
        public string EnemyRegenLabel => "Rage on Hit";
        public int EnemyRegenValue { get => SkeletonRageOnHit; set => SkeletonRageOnHit = value; }
        public string EnemyMaxResourceName => "Max Rage";
        public int EnemyMaxResourceValue { get => SkeletonMaxRagePoints; set => SkeletonMaxRagePoints = value; }
        public int EnemyLifeRegen { get => SkeletonLifeRegenRate; set => SkeletonLifeRegenRate = value; }
        public int EnemyStatPoints { get => SkeletonStatPoints; set => SkeletonStatPoints = value; }

        // Enemy Color Theme Info
        public string EnemyHPColor => "rgba(255,0,0,.7)";
        public string EnemyInvColor => "rgba(255,100,0,1.0)";
        public string EnemyEnergyColor => "rgba(255,100,0,.7)";

        // Debugger
        public string EnemyCollisionBoxStyle => SkeletonCollisionBoxStyle;
        public string EnemySpriteDebugStyle => SkeletonSpriteDebugStyle;
        public string EnemyPunchBoxStyle => SkeletonPunchBoxStyle;
        // Core state



        // XP Gain
        public void AddXp(BloodLevel levelReg, double multiplier)
        {
            levelReg.AddXp("Skeleton", multiplier);
        }


        // Core Logic
        public int SkeletonZIndex { get; private set; } = 6000;
        public int SkeletonX { get; set; }
        public int SkeletonY { get; set; }

        public int SkeletonWidth { get; set; } = 84;
        public int SkeletonHeight { get; set; } = 84;
      
        public string SkeletonXpx => $"{SkeletonX}px";
        public string SkeletonYpx => $"{SkeletonY}px";
        public int SkeletonPunchX { get; set; } = 24;
        public int SkeletonPunchY { get; set; } = 24;


        public int SkeletonLevelCap { get; set; } = 100;
        public int SkeletonXPPerLevel { get; set; } = 50;
        public int SkeletonXP { get; set; } = 0;
        public int SkeletonLevel { get; set; } = 1;
        public int SkeletonHungerCurrent { get; set; } = 2000;   // starting calories
        public int SkeletonHungerFull { get; set; } = 2000;      // max capacity
        public int SkeletonHungerDurationSeconds { get; set; } = 86400; // 24 hours

        private DateTime _lastHungerTick = DateTime.Now;
        public int SkeletonMaxHP { get; set; } = 2;
        public int SkeletonStatPoints { get; set; } = 0;


        public int SkeletonHitPoints { get; set; } = 2;

        private int skeletonDamageAmount = 1;

        public int SkeletonAlacrityAmount = 2;

        public int SkeletonMovementSpeed = 7;

        public int punchRange = 6;

        public int SkeleSpellDamage = 0;

        public int SkeletonRagePoints { get; set; } = 0;

        public int SkeletonMaxRagePoints { get; set; } = 10;
        public int SkeletonRageOnHit { get; set; } = 1;


        public int SkeletonLifeRegenRate { get; set; } = 0;

        public bool SkeletonIsAlive => SkeletonHitPoints > 0;
        public bool ShowSkeletonHitEffect { get; set; } = false;





        private int skeletonFrame = 0;
        private DateTime lastSkeletonFrameTime = DateTime.Now;
        private bool isOneShotSkeletonAnimation = false;
        private SkeletonAnimationState currentSkeletonAnimation = SkeletonAnimationState.Idle;

        public SkeletonAnimationState CurrentSkeletonAnimation => currentSkeletonAnimation;

        public string SkeletonBackgroundPosition => $"-{skeletonFrame * SkeletonFrameWidth}px 0";

       
        private DateTime skeletonIdleUntil = DateTime.MinValue;
        public DateTime SkeletonIdleUntil => skeletonIdleUntil;
       

        public const int SkeletonFrameWidth = 84;
        public const int SkeletonFrameHeight = 84;

        public Rectangle SkeletonCollisionBox =>
            new Rectangle(
                SkeletonX + SkeletonCollisionShiftX,
                SkeletonY + SkeletonCollisionShiftY,
                SkeletonCollisionWidth,
                SkeletonCollisionHeight
            );

        private const int SkeletonCollisionWidth = 24;
        private const int SkeletonCollisionHeight = 24;
        private const int SkeletonCollisionShiftX = 32;
        private const int SkeletonCollisionShiftY = 48;

        public const int SkeletonPunchShiftX = 32;
        public const int SkeletonPunchShiftY = 32;

        public Rectangle SkeletonPunchBox =>
          new Rectangle(
                  SkeletonX + SkeletonPunchShiftX - punchRange,
              SkeletonY + SkeletonPunchShiftX - punchRange,
              SkeletonPunchX + (2 * punchRange),
              SkeletonPunchY + (2 * punchRange)
          );
      

        public string SkeletonSpriteStyle =>
         $"position:absolute; left:{SkeletonX}px; top:{SkeletonY}px; " +
         $"width:{SkeletonWidth}px; height:{SkeletonHeight}px; " +
         $"background-image:url('{SkeletonSpriteSheet}'); " +
         $"background-position:-{skeletonFrame * SkeletonWidth}px 0px; " +
         $"background-repeat:no-repeat; background-color:transparent; " +
         $"opacity:0.95; user-select:none; touch-action:manipulation; z-index:{SkeletonZIndex};";

        private static readonly Random Randomizer = new Random();


        public void SkeletonTakeDamage(int amount = 1)
        {
            SkeletonHitPoints = Math.Max(SkeletonHitPoints - amount, 0);
            ShowSkeletonHitEffect = true;
          
        }

        public void ClearSkeletonHitEffects() => ShowSkeletonHitEffect = false;



        private DateTime lastSkeletonAttackTime = DateTime.MinValue;
        // Replace your current SkeletonAttack with this version:

        private bool isAttacking = false;


        public void SkeletonAttack(IBloodiCharacter active)
        {
            int attackIntervalMs = 1000 / Math.Max(1, SkeletonAlacrityAmount);

            if ((DateTime.Now - lastSkeletonAttackTime).TotalMilliseconds < attackIntervalMs)
                return;
            if (isAttacking) return; // guard

            lastSkeletonAttackTime = DateTime.Now;
            isAttacking = true;

            _ = Task.Run(async () =>
            {
                try
                {
                    SetSkeletonAnimation(SkeletonAnimationState.Attack);
                    isOneShotSkeletonAnimation = true;

                    int maxFrames = animationFrameCounts.TryGetValue(SkeletonAnimationState.Attack, out var count) ? count : 12;
                    int adjustedDelay = attackIntervalMs / maxFrames;
                    int damageDelayMs = adjustedDelay * Math.Max(1, maxFrames - 3);

                    await Task.Delay(damageDelayMs);

                    if (active.CharIsAlive && SkeletonPunchBox.IntersectsWith(active.CharCollisionBox))
                    {
                        active.CharTakeDamage(skeletonDamageAmount);
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







        public enum SkeletonAnimationState
        {
            Idle,
            WalkLeft,
            WalkRight,
            Attack,
            Flex,
            WalkUp,
            WalkDown
        }

        private readonly Dictionary<SkeletonAnimationState, int> skeletonAnimationSpeeds = new()
    {
        { SkeletonAnimationState.Idle, 120},
        { SkeletonAnimationState.WalkLeft, 50 },
        { SkeletonAnimationState.WalkRight, 50},
        { SkeletonAnimationState.Attack, 50 },
               { SkeletonAnimationState.Flex, 50 },
   { SkeletonAnimationState.WalkUp, 50 },
   { SkeletonAnimationState.WalkDown, 50 }




    };


        private readonly Dictionary<SkeletonAnimationState, int> animationFrameCounts = new()
{
    { SkeletonAnimationState.Idle, 6 },
    { SkeletonAnimationState.WalkLeft, 6 },
    { SkeletonAnimationState.WalkRight, 6 },
    { SkeletonAnimationState.Attack, 6},
             { SkeletonAnimationState.Flex, 8},
 { SkeletonAnimationState.WalkUp, 8},
 { SkeletonAnimationState.WalkDown, 8}



};

        public string SkeletonSpriteSheet => currentSkeletonAnimation switch
        {
            SkeletonAnimationState.Idle => "/iAssets/SkeletonIdle01.png",
            SkeletonAnimationState.WalkLeft => "/iAssets/SkeletonLeftWalk01.png",
            SkeletonAnimationState.WalkRight => "/iAssets/SkeletonRightWalk01.png",
            SkeletonAnimationState.Attack => "/iAssets/SkeletonPunch01.png",
            SkeletonAnimationState.Flex => "/iAssets/SkeletonFlex01.png",
            SkeletonAnimationState.WalkUp => "/iAssets/SkeletonUpWalk01.png",
            SkeletonAnimationState.WalkDown => "/iAssets/SkeletonDownWalk01.png",

            _ => "/iAssets/SkeletonIdle01.png"
        };



        public void SetSkeletonAnimation(SkeletonAnimationState newState)
        {
            if (newState != currentSkeletonAnimation)
            {
                skeletonFrame = 0;
                lastSkeletonFrameTime = DateTime.Now;
            }
            currentSkeletonAnimation = newState;
        }

        // 🔥 OPTIMIZED Z-INDEX CALCULATION FOR ENEMIES

        // Add these fields to your enemy classes (Skeleton, Cat, Cow, TownSlut):
        private int lastZIndexY = -1;
        private const int ZIndexUpdateThreshold = 20; // Only recalc when moved 20px vertically

        // ✅ OPTIMIZED TickSkeletonAnimation (apply to all 4 enemy types):
        public void TickSkeletonAnimation()
        {
            // 🔹 ONLY update Z-index if enemy moved significantly in Y direction
            if (Math.Abs(SkeletonY - lastZIndexY) > ZIndexUpdateThreshold)
            {
                UpdateZIndex();
                lastZIndexY = SkeletonY;
            }

            // Rest of animation logic stays the same
            int delay = skeletonAnimationSpeeds.TryGetValue(currentSkeletonAnimation, out var ms) ? ms : 150;
            int maxFrames = animationFrameCounts.TryGetValue(currentSkeletonAnimation, out var count) ? count : 1;

            if ((DateTime.Now - lastSkeletonFrameTime).TotalMilliseconds >= delay)
            {
                skeletonFrame++;

                if (isOneShotSkeletonAnimation && skeletonFrame >= maxFrames)
                {
                    SetSkeletonAnimation(SkeletonAnimationState.Idle);
                    isOneShotSkeletonAnimation = false;
                }
                else
                {
                    skeletonFrame %= maxFrames;
                }
                lastSkeletonFrameTime = DateTime.Now;
            }
        }

        // 🔹 NEW: Separate Z-index calculation method
        private void UpdateZIndex()
        {
            var ordered = ZIndexCache.GetSortedStaticObjects();

            foreach (var obj in ordered)
            {
                if (SkeletonY < obj.CollisionBox.Y)
                {
                    SkeletonZIndex = obj.ZIndex - 1;
                    return; // Early exit
                }
            }

            SkeletonZIndex = 6000; // Default if no match
        }

        // 🔹 ALTERNATIVE: Binary search for even faster lookup
        private void UpdateZIndexFast()
        {
            var ordered = ZIndexCache.GetSortedStaticObjects();

            // Binary search since list is sorted by Y
            int left = 0;
            int right = ordered.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                var obj = ordered[mid];

                if (SkeletonY < obj.CollisionBox.Y)
                {
                    // Check if this is the first object below us
                    if (mid == 0 || ordered[mid - 1].CollisionBox.Y <= SkeletonY)
                    {
                        SkeletonZIndex = obj.ZIndex - 1;
                        return;
                    }
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            SkeletonZIndex = 6000; // No object below us
        }

        // 🔹 Apply this pattern to ALL 4 enemy types:
        // - BloodEnemy.Skeleton
        // - BloodCat.Skeleton  
        // - BloodCow (if it has Z-index calc)
        // - BloodTownSlut (if it has Z-index calc)

        public void SetAggressionTarget(IBloodiCharacter active)
        {
           
        }

        private enum PatrolDirection { Left, Right, Up, Down }
        private PatrolDirection? currentDirection; // nullable so we know if it's uninitialized

        private readonly Random rng = new();

        public int PatrolLeftBound { get; set; } = 24;
        public int PatrolRightBound { get; set; } = 1994;
        public int PatrolTopBound { get; set; } = 24;
        public int PatrolBottomBound { get; set; } = 1994;

        // Pick a random direction
        private PatrolDirection GetRandomDirection()
        {
            var dirs = Enum.GetValues(typeof(PatrolDirection));
            return (PatrolDirection)dirs.GetValue(rng.Next(dirs.Length))!;
        }

        // Track slowdown state
        private bool skeletonIsTired = false;
        private DateTime skeletonTiredUntil;
        private PatrolDirection? lastPatrolDirection = null;

        public void TickSkeletonMovement(IBloodiCharacter active)
        {
            if (!SkeletonIsAlive)
                return;

            // Attack check
            if (SkeletonPunchBox.IntersectsWith(active.CharCollisionBox) &&
                currentSkeletonAnimation != SkeletonAnimationState.Attack)
            {
                SkeletonAttack(active);
                return;
            }

            // Idle timer
            if (DateTime.Now < skeletonIdleUntil)
            {
                // 🔹 FIX: Only set idle if not already idle
                if (currentSkeletonAnimation != SkeletonAnimationState.Idle)
                {
                    SetSkeletonAnimation(SkeletonAnimationState.Idle);
                    lastPatrolDirection = null;
                }
                TickSkeletonAnimation();
                return;
            }

            // Random chance to flex
            if (rng.NextDouble() < 0.01)
            {
                SetSkeletonAnimation(SkeletonAnimationState.Flex);
                skeletonIdleUntil = DateTime.Now.AddMilliseconds(rng.Next(300, 900));
                lastPatrolDirection = null;
                return;
            }

            // Random chance to change direction mid‑path
            if (rng.NextDouble() < 0.03)
            {
                currentDirection = GetRandomDirection();
                skeletonIdleUntil = DateTime.Now.AddMilliseconds(rng.Next(200, 600));
            }

            // Random chance to get tired
            if (!skeletonIsTired && rng.NextDouble() < 0.02)
            {
                skeletonIsTired = true;
                skeletonTiredUntil = DateTime.Now.AddMilliseconds(rng.Next(1000, 2000));
            }

            // Recover from tired
            if (skeletonIsTired && DateTime.Now >= skeletonTiredUntil)
            {
                skeletonIsTired = false;
            }

            // Auto‑initialize patrol direction if not set yet
            if (currentDirection == null)
            {
                currentDirection = GetRandomDirection();
            }

            // Adjust speed if tired
            int moveSpeed = skeletonIsTired ? SkeletonMovementSpeed / 2 : SkeletonMovementSpeed;

            // 🔹 FIX: Patrol logic - only set animation when direction CHANGES
            switch (currentDirection)
            {
                case PatrolDirection.Right:
                    SkeletonX += moveSpeed;
                    if (lastPatrolDirection != PatrolDirection.Right)
                    {
                        SetSkeletonAnimation(SkeletonAnimationState.WalkRight);
                        lastPatrolDirection = PatrolDirection.Right;
                    }
                    if (SkeletonX >= PatrolRightBound)
                    {
                        currentDirection = GetRandomDirection();
                        skeletonIdleUntil = DateTime.Now.AddMilliseconds(rng.Next(300, 900));
                    }
                    break;

                case PatrolDirection.Left:
                    SkeletonX -= moveSpeed;
                    if (lastPatrolDirection != PatrolDirection.Left)
                    {
                        SetSkeletonAnimation(SkeletonAnimationState.WalkLeft);
                        lastPatrolDirection = PatrolDirection.Left;
                    }
                    if (SkeletonX <= PatrolLeftBound)
                    {
                        currentDirection = GetRandomDirection();
                        skeletonIdleUntil = DateTime.Now.AddMilliseconds(rng.Next(300, 900));
                    }
                    break;

                case PatrolDirection.Up:
                    SkeletonY -= moveSpeed;
                    if (lastPatrolDirection != PatrolDirection.Up)
                    {
                        SetSkeletonAnimation(SkeletonAnimationState.WalkUp);
                        lastPatrolDirection = PatrolDirection.Up;
                    }
                    if (SkeletonY <= PatrolTopBound)
                    {
                        currentDirection = GetRandomDirection();
                        skeletonIdleUntil = DateTime.Now.AddMilliseconds(rng.Next(300, 900));
                    }
                    break;

                case PatrolDirection.Down:
                    SkeletonY += moveSpeed;
                    if (lastPatrolDirection != PatrolDirection.Down)
                    {
                        SetSkeletonAnimation(SkeletonAnimationState.WalkDown);
                        lastPatrolDirection = PatrolDirection.Down;
                    }
                    if (SkeletonY >= PatrolBottomBound)
                    {
                        currentDirection = GetRandomDirection();
                        skeletonIdleUntil = DateTime.Now.AddMilliseconds(rng.Next(300, 900));
                    }
                    break;
            }

            TickSkeletonAnimation();
        }
    

    public string SkeletonSpriteDebugStyle =>
            $"position:absolute; left:{SkeletonX}px; top:{SkeletonY}px; " +
            $"width:{SkeletonWidth}px; height:{SkeletonHeight}px; " +
            $"background-color:rgba(0,255,0,0.2); border:1px dashed lime; z-index:998;";

        public string SkeletonCollisionBoxStyle =>
            $"position:absolute; left:{SkeletonX + SkeletonCollisionShiftX}px; top:{SkeletonY + SkeletonCollisionShiftY}px; " +
            $"width:{SkeletonCollisionWidth}px; height:{SkeletonCollisionHeight}px; " +
            $"background-color:rgba(255,0,0,0.3); border:2px solid red; z-index:999;";
        public string SkeletonPunchBoxStyle =>
            $"position:absolute; left:{SkeletonPunchBox.X}px; top:{SkeletonPunchBox.Y}px; " +
            $"width:{SkeletonPunchBox.Width}px; height:{SkeletonPunchBox.Height}px; " +
            $"background-color:rgba(255,0,255,0.3); border:2px solid magenta; z-index:999;";


    }

    public class BloodSkeletonRegistry
    {
        public static List<Skeleton> All = new();

        public static void SpawnSkeletons(int count)
        {
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                All.Add(new Skeleton
                {
                    SkeletonX = rand.Next(25, 1994),
                    SkeletonY = rand.Next(25, 1994),
                });
            }
        }
    }


}
