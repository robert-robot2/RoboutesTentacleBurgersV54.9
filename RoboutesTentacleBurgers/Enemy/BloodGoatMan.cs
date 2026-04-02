
using RoboutesTentacleBurgers.Services;
using static RoboutesTentacleBurgers.Services.ZIndexCache;

public class BloodGoatMan
{

  

    public class Goatman : IiEnemy
    {
        // IBloodiCharacter implementation
        public void EnemyAttack(IBloodiCharacter active) =>  GoatmanAttack(active);
        public void EnemyMove(IBloodiCharacter active) => TickGoatmanMovement(active);
        //   public void StopMovement(string key) => StopMovementWarrior(key);
        public void EnemyTickAllAnimation() => TickGoatmanAnimation();
        public void SetAggression(IBloodiCharacter active) =>  SetAggressionTarget(active);
        public bool EnemyIsAlive => GoatmanHitPoints > 0;
        public int EnemyX => GoatmanX;
        public int EnemyY => GoatmanY;
        public string SpriteStyle => GoatmanSpriteStyle;
        public string? HitEffectPath => ShowGoatmanHitEffect ? "/iAssets/WarriorGothit01.png" : null;
        public Rectangle EnemyCollisionBox => GoatmanCollisionBox;
        public Rectangle EnemyPunchBox => GoatmanPunchBox;
        public void EnemyTakeDamage(int amount) => GoatmanTakeDamage(amount);
        public void ClearHitEffect() => ClearGoatmanHitEffects();


        public List<SplatterPuddle> SplatterPuddles { get; } = new();

        // Stats
        public string EnemyClassName => "Goatman";
        public int EnemyLevel { get => GoatmanLevel; set => GoatmanLevel = value; }
        public int EnemyXP { get => GoatmanXP; set => GoatmanXP = value; }

        public int EnemyHungerCurrent { get => 0; set { } }
        public int EnemyHungerFull { get => 0; set { } }
        public int EnemyHungerDurationSeconds { get => 0; set { } }

        // public int EnemyHungerCurrent { get => GoatmanHungerCurrent; set => GoatmanHungerCurrent = value; }
        // public int EnemyHungerFull { get => GoatmanHungerFull; set => GoatmanHungerFull = value; }
        // public int EnemyHungerDurationSeconds { get => GoatmanHungerDurationSeconds; set => GoatmanHungerDurationSeconds = value; }

        public int EnemyHitPoints { get => GoatmanHitPoints; set => GoatmanHitPoints = value; }
        public int EnemyMaxHP { get => GoatmanMaxHP; set => GoatmanMaxHP = value; }
        public int EnemyXPPerLevel { get => GoatmanXPPerLevel; set => GoatmanXPPerLevel = value; }

        public int EnemyLevelCap { get => GoatmanLevelCap; set => GoatmanLevelCap = value; }
        public int EnemyStrength { get => goatmanDamageAmount; set => goatmanDamageAmount = value; }
        public int EnemyAlacrity { get => GoatManAlacrityAmount; set => GoatManAlacrityAmount = value; }
        public int EnemyCelerity { get => GoatmanMovementSpeed; set => GoatmanMovementSpeed = value; }
        public int EnemyLimenity { get => punchRange; set => punchRange = value; }
        public int EnemyIntelligence { get => SpellDamage; set => SpellDamage = value; }

        // Unique class stats
        public string EnemyResourceName => "Goat Fury";
        public int EnemyResourceValue { get => GoatmanFuryPoints; set => GoatmanFuryPoints = value; }
        public string EnemyRegenLabel => "Fury on Charge";
        public int EnemyRegenValue { get => GoatmanFuryOnHit; set => GoatmanFuryOnHit = value; }
        public string EnemyMaxResourceName => "Max Goat Fury";
        public int EnemyMaxResourceValue { get => GoatmanMaxFuryPoints; set => GoatmanMaxFuryPoints = value; }
        public int EnemyLifeRegen { get => GoatmanLifeRegenRate; set => GoatmanLifeRegenRate = value; }
        public int EnemyStatPoints { get => GoatmanStatPoints; set => GoatmanStatPoints = value; }

        // Enemy Color Theme Info
        public string EnemyHPColor => "rgba(139,69,19,.8)";     // earthy brown
        public string EnemyInvColor => "rgba(34,139,34,1.0)";   // forest green
        public string EnemyEnergyColor => "rgba(255,215,0,.7)"; // golden fury

        // Debugger
        public string EnemyCollisionBoxStyle => GoatmanCollisionBoxStyle;
        public string EnemySpriteDebugStyle => GoatmanSpriteDebugStyle;
        public string EnemyPunchBoxStyle => GoatmanPunchBoxStyle;
        // Core state






        // XP Gain
        public void AddXp(BloodLevel levelReg, double multiplier)
        {
            levelReg.AddXp("Goatman", multiplier);
        }
        public int GoatmanZIndex { get; private set; } = 6000;
        public int GoatmanX { get; set; }
        public int GoatmanY { get; set; }

        public int GoatmanWidth { get; set; } = 84;
        public int GoatmanHeight { get; set; } = 84;

        public string GoatmanXpx => $"{GoatmanX}px";
        public string GoatmanYpx => $"{GoatmanY}px";

        public int GoatmanPunchX { get; set; } = 24;
        public int GoatmanPunchY { get; set; } = 24;


        public int GoatmanLevelCap { get; set; } = 100;
        public int GoatmanXPPerLevel { get; set; } = 50;
        public int GoatmanXP { get; set; } = 0;
        public int GoatmanLevel { get; set; } = 1;
        public int GoatmanHungerCurrent { get; set; } = 2000;   // starting calories
        public int GoatmanHungerFull { get; set; } = 2000;      // max capacity
        public int GoatmanHungerDurationSeconds { get; set; } = 86400; // 24 hours

        private DateTime _lastHungerTick = DateTime.Now;
        public int GoatmanMaxHP { get; set; } = 30;

        public int GoatmanStatPoints { get; set; } = 0;
        public int GoatmanHitPoints { get; set; } = 30;

        private int goatmanDamageAmount = 3;

        public int GoatManAlacrityAmount = 3;

        public  int GoatmanMovementSpeed = 4;

        public int punchRange = 21;

        public int SpellDamage = 0;

        public int GoatmanFuryPoints { get; set; } = 0;

        public int GoatmanMaxFuryPoints { get; set; } = 10;
        public int GoatmanFuryOnHit { get; set; } = 1;


        public int GoatmanLifeRegenRate { get; set; } = 0;
        public int PatrolLeftBound { get; set; } = 24;
        public int PatrolRightBound { get; set; } = 1994;
        public int PatrolTopBound { get; set; } = 24;
        public int PatrolBottomBound { get; set; } = 1994;


        public bool GoatmanIsAlive => GoatmanHitPoints > 0;
        public bool ShowGoatmanHitEffect { get; set; } = false;

        private int goatmanFrame = 0;
        private DateTime lastGoatmanFrameTime = DateTime.Now;
        private bool isOneShotGoatmanAnimation = false;
        private GoatmanAnimationState currentGoatmanAnimation = GoatmanAnimationState.Idle;

        public GoatmanAnimationState CurrentGoatmanAnimation => currentGoatmanAnimation;

        public string GoatmanBackgroundPosition => $"-{goatmanFrame * GoatmanFrameWidth}px 0";

        private bool goatmanMovingRight = true;
        private DateTime goatmanIdleUntil = DateTime.MinValue;
        public DateTime GoatmanIdleUntil => goatmanIdleUntil;




        public const int GoatmanFrameWidth = 84;
        public const int GoatmanFrameHeight = 84;
        public Rectangle GoatmanCollisionBox =>
            new Rectangle(
                GoatmanX + GoatmanCollisionShiftX,
                GoatmanY + GoatmanCollisionShiftY,
                GoatmanCollisionWidth,
                GoatmanCollisionHeight
            );

        private const int GoatmanCollisionWidth = 24;
        private const int GoatmanCollisionHeight = 24;
        private const int GoatmanCollisionShiftX = 32;
        private const int GoatmanCollisionShiftY = 48;

        public const int GoatmanPunchShiftX = 32;
        public const int GoatmanPunchShiftY = 32;


        public Rectangle GoatmanPunchBox =>
            new Rectangle(
                GoatmanX + GoatmanPunchShiftX - punchRange,
                GoatmanY + GoatmanPunchShiftY - punchRange,
                GoatmanPunchX + (2 * punchRange),
                GoatmanPunchY + (2 * punchRange)
            );

        public string GoatmanSpriteStyle =>
            $"position:absolute; left:{GoatmanX}px; top:{GoatmanY}px; " +
            $"width:{GoatmanWidth}px; height:{GoatmanHeight}px; " +
            $"background-image:url('{GoatmanSpriteSheet}'); " +
            $"background-position:-{goatmanFrame * GoatmanWidth}px 0px; " +
            $"background-repeat:no-repeat; background-color:transparent; " +
            $"opacity:0.95; user-select:none; touch-action:manipulation; z-index:{GoatmanZIndex};";

        private static readonly Random Randomizer = new Random();
        public void GoatmanTakeDamage(int amount = 1)
        {
            GoatmanHitPoints = Math.Max(GoatmanHitPoints - amount, 0);
            ShowGoatmanHitEffect = true;
            BloodSplatterRegistry.Add(new SplatterPuddle
            {
                X = EnemyCollisionBox.X + (int)(Randomizer.NextDouble() * 10 - 5), // ±5px
                Y = EnemyCollisionBox.Y + (int)(Randomizer.NextDouble() * 10 - 5),

                Scale = Math.Min(1.0 + (amount * 0.25), 3.0) * (0.8 + Randomizer.NextDouble() * 0.4),


            });
        }

        public void ClearGoatmanHitEffects() => ShowGoatmanHitEffect = false;

        private DateTime lastGoatmanAttackTime = DateTime.MinValue;
        private bool isAttacking = false;

        public void GoatmanAttack(IBloodiCharacter active)
        {
            int attackIntervalMs = 1000 / Math.Max(1, GoatManAlacrityAmount);

            if ((DateTime.Now - lastGoatmanAttackTime).TotalMilliseconds < attackIntervalMs)
                return;
            if (isAttacking) return; // guard

            lastGoatmanAttackTime = DateTime.Now;
            isAttacking = true;

            _ = Task.Run(async () =>
            {
                try
                {
                    SetGoatmanAnimation(GoatmanAnimationState.Attack);
                    isOneShotGoatmanAnimation = true;

                    int maxFrames = animationFrameCounts.TryGetValue(GoatmanAnimationState.Attack, out var count) ? count : 12;
                    int adjustedDelay = attackIntervalMs / maxFrames;
                    int damageDelayMs = adjustedDelay * Math.Max(1, maxFrames - 3);

                    await Task.Delay(damageDelayMs);

                    if (active.CharIsAlive && GoatmanPunchBox.IntersectsWith(active.CharCollisionBox))
                    {
                        active.CharTakeDamage(goatmanDamageAmount);

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







        public enum GoatmanAnimationState
        {
            Idle,
            WalkLeft,
            WalkRight,
            Attack
        }

        private readonly Dictionary<GoatmanAnimationState, int> goatmanAnimationSpeeds = new()
    {
        { GoatmanAnimationState.Idle, 50 },
        { GoatmanAnimationState.WalkLeft, 50 },
        { GoatmanAnimationState.WalkRight, 50 },
        { GoatmanAnimationState.Attack, 50 }
    };

        private readonly Dictionary<GoatmanAnimationState, int> animationFrameCounts = new()
    {
        { GoatmanAnimationState.Idle, 12 },
        { GoatmanAnimationState.WalkLeft, 8 },
        { GoatmanAnimationState.WalkRight, 8 },
        { GoatmanAnimationState.Attack, 12 }
    };

        public string GoatmanSpriteSheet => currentGoatmanAnimation switch
        {
            GoatmanAnimationState.Idle => "/iAssets/GoatManIdle01.png",
            GoatmanAnimationState.WalkLeft => "/iAssets/GoatManWalkLeft01.png",
            GoatmanAnimationState.WalkRight => "/iAssets/GoatManWalkRight01.png",
            GoatmanAnimationState.Attack => "/iAssets/GoatManAttack01.png",
            _ => "/iAssets/GoatmanIdle01.png"
        };

        public void SetGoatmanAnimation(GoatmanAnimationState newState)
        {
            if (newState != currentGoatmanAnimation)
            {
                goatmanFrame = 0;
                lastGoatmanFrameTime = DateTime.Now;
            }
            currentGoatmanAnimation = newState;
        }

        private int lastZIndexY = -1;
        private const int ZIndexUpdateThreshold = 20; // Only recalc when moved 20px vertically
        // ✅ OPTIMIZED TickGoatmanAnimation (same model as Skeleton/Cow/TownSlut/ScavBoss)
        public void TickGoatmanAnimation()
        {
            // 🔹 ONLY update Z-index if Goatman moved significantly in Y direction
            if (Math.Abs(GoatmanY - lastZIndexY) > ZIndexUpdateThreshold)
            {
                UpdateGoatmanZIndex();
                lastZIndexY = GoatmanY;
            }

            // Rest of animation logic stays the same
            int delay = goatmanAnimationSpeeds.TryGetValue(currentGoatmanAnimation, out var ms) ? ms : 150;
            int maxFrames = animationFrameCounts.TryGetValue(currentGoatmanAnimation, out var count) ? count : 1;

            if ((DateTime.Now - lastGoatmanFrameTime).TotalMilliseconds >= delay)
            {
                goatmanFrame++;

                if (isOneShotGoatmanAnimation && goatmanFrame >= maxFrames)
                {
                    SetGoatmanAnimation(GoatmanAnimationState.Idle);
                    isOneShotGoatmanAnimation = false;
                }
                else
                {
                    goatmanFrame %= maxFrames;
                }

                lastGoatmanFrameTime = DateTime.Now;
            }
        }

        // 🔹 NEW: Separate Z-index calculation for Goatman
        private void UpdateGoatmanZIndex()
        {
            var ordered = ZIndexCache.GetSortedStaticObjects();

            foreach (var obj in ordered)
            {
                if (GoatmanY < obj.CollisionBox.Y)
                {
                    GoatmanZIndex = obj.ZIndex - 1;
                    return; // Early exit
                }
            }

            GoatmanZIndex = 6000; // Default if no match
        }

        // 🔹 ALTERNATIVE: Binary search for even faster lookup
        private void UpdateGoatmanZIndexFast()
        {
            var ordered = ZIndexCache.GetSortedStaticObjects();

            // Binary search since list is sorted by Y
            int left = 0;
            int right = ordered.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                var obj = ordered[mid];

                if (GoatmanY < obj.CollisionBox.Y)
                {
                    // Check if this is the first object below us
                    if (mid == 0 || ordered[mid - 1].CollisionBox.Y <= GoatmanY)
                    {
                        GoatmanZIndex = obj.ZIndex - 1;
                        return;
                    }
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            GoatmanZIndex = 6000; // No object below us
        }

        public bool HasAggressionTarget { get; private set; } = false;

        public IBloodiCharacter? ActiveCharacter { get; private set; }



        public void SetAggressionTarget(IBloodiCharacter active)
        {
            if (!GoatmanIsAlive) return;

            ActiveCharacter = active;
            HasAggressionTarget = true;
        }

        public void TickGoatmanMovement(IBloodiCharacter active)
        {
            if (!GoatmanIsAlive)
                return;


            // Attack if in range and not already attacking
            if (GoatmanIsAlive &&
        currentGoatmanAnimation != GoatmanAnimationState.Attack &&
        GoatmanPunchBox.IntersectsWith(active.CharCollisionBox))
            {
                GoatmanAttack(active);
                return;
            }


            // Idle timer
            if (DateTime.Now < goatmanIdleUntil)
            {
                SetGoatmanAnimation(GoatmanAnimationState.Idle);
                TickGoatmanAnimation();
                return;
            }

            // Aggression targeting simplified: always chase the active character
            if (active != null && active.CharIsAlive)
            {
                int targetCenterX = active.CharX + 42;
                int targetCenterY = active.CharY + 42;

                int goatmanCenterX = GoatmanX + (GoatmanWidth / 2);
                int goatmanCenterY = GoatmanY + (GoatmanHeight / 2);

                if (Math.Abs(goatmanCenterX - targetCenterX) > GoatmanMovementSpeed)
                {
                    if (goatmanCenterX < targetCenterX && GoatmanX + GoatmanMovementSpeed + GoatmanWidth < PatrolRightBound)
                    {
                        GoatmanX += GoatmanMovementSpeed;
                        SetGoatmanAnimation(GoatmanAnimationState.WalkRight);
                    }
                    else if (goatmanCenterX > targetCenterX && GoatmanX - GoatmanMovementSpeed > PatrolLeftBound)
                    {
                        GoatmanX -= GoatmanMovementSpeed;
                        SetGoatmanAnimation(GoatmanAnimationState.WalkLeft);
                    }
                }

                if (Math.Abs(goatmanCenterY - targetCenterY) > GoatmanMovementSpeed)
                {
                    if (goatmanCenterY < targetCenterY)
                        GoatmanY += GoatmanMovementSpeed;
                    else if (goatmanCenterY > targetCenterY)
                        GoatmanY -= GoatmanMovementSpeed;
                }

                if (Math.Abs(goatmanCenterX - targetCenterX) <= GoatmanMovementSpeed &&
                    Math.Abs(goatmanCenterY - targetCenterY) <= GoatmanMovementSpeed)
                {
                    SetGoatmanAnimation(GoatmanAnimationState.Idle);
                }

                TickGoatmanAnimation();
                return;
            }

            // Patrol fallback
            if (goatmanMovingRight)
            {
                GoatmanX += GoatmanMovementSpeed;
                SetGoatmanAnimation(GoatmanAnimationState.WalkRight);

                if (GoatmanX >= PatrolRightBound)
                {
                    goatmanMovingRight = false;
                    goatmanIdleUntil = DateTime.Now.AddMilliseconds(2000);
                }
            }
            else
            {
                GoatmanX -= GoatmanMovementSpeed;
                SetGoatmanAnimation(GoatmanAnimationState.WalkLeft);

                if (GoatmanX <= PatrolLeftBound)
                {
                    goatmanMovingRight = true;
                    goatmanIdleUntil = DateTime.Now.AddMilliseconds(2000);
                }
            }

            TickGoatmanAnimation();
        }



        public string GoatmanPunchBoxStyle =>
          $"position:absolute; left:{GoatmanPunchBox.X}px; top:{GoatmanPunchBox.Y}px; " +
          $"width:{GoatmanPunchBox.Width}px; height:{GoatmanPunchBox.Height}px; " +
          $"background-color:rgba(255,0,255,0.3); border:2px solid magenta; z-index:999;";


        public string GoatmanSpriteDebugStyle =>
            $"position:absolute; left:{GoatmanX}px; top:{GoatmanY}px; " +
            $"width:{GoatmanWidth}px; height:{GoatmanHeight}px; " +
            $"background-color:rgba(0,255,0,0.2); border:1px dashed lime; z-index:998;";

        public string GoatmanCollisionBoxStyle =>
            $"position:absolute; left:{GoatmanX + GoatmanCollisionShiftX}px; top:{GoatmanY + GoatmanCollisionShiftY}px; " +
            $"width:{GoatmanCollisionWidth}px; height:{GoatmanCollisionHeight}px; " +
            $"background-color:rgba(255,0,0,0.3); border:2px solid red; z-index:999;";


    }



    public class BloodGoatmanRegistry
    {
        public static List<Goatman> All = new();

        public static void SpawnGoatmen(int count)
        {
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                All.Add(new Goatman
                {
                    GoatmanX = rand.Next(25, 1994),
                    GoatmanY = rand.Next(25, 1994),
                });
            }
        }
    }









}
