
using RoboutesTentacleBurgers.Services;
using static RoboutesTentacleBurgers.Services.ZIndexCache;

public class BloodSkelWar
{

    public class SkeletonWar : IiEnemy
    {

        // IBloodiCharacter implementation
        public void EnemyAttack(IBloodiCharacter active) =>  SkeletonWarAttack(active);
        public void EnemyMove(IBloodiCharacter active) => TickSkeletonWarMovement(active);
        //   public void StopMovement(string key) => StopMovementWarrior(key);
        public void EnemyTickAllAnimation() =>  TickSkeletonWarAnimation();
        public void SetAggression(IBloodiCharacter active) => SetAggressionTarget(active);
        public bool EnemyIsAlive => SkeletonWarHitPoints > 0;
        public int EnemyX => SkeletonWarX;
        public int EnemyY => SkeletonWarY;
        public string SpriteStyle => SkeletonWarSpriteStyle;
        public string? HitEffectPath => ShowSkeletonWarHitEffect ? "/iAssets/SkeleHit01.png" : null;
        public Rectangle EnemyCollisionBox => SkeletonWarCollisionBox;
        public Rectangle EnemyPunchBox => SkeletonWarPunchBox;
        public void EnemyTakeDamage(int amount) => SkeletonWarTakeDamage(amount);

        public List<SplatterPuddle> SplatterPuddles { get; } = new();

        // Stats
        public string EnemyClassName => "SkeletonWar";
        public int EnemyLevel { get => SkeletonWarLevel; set => SkeletonWarLevel = value; }
        public int EnemyXP { get => SkeletonWarXP; set => SkeletonWarXP = value; }

        public int EnemyHungerCurrent { get => 0; set { } }
        public int EnemyHungerFull { get => 0; set { } }
        public int EnemyHungerDurationSeconds { get => 0; set { } }

        // public int EnemyHungerCurrent { get => SkeletonWarHungerCurrent; set => SkeletonWarHungerCurrent = value; }
        // public int EnemyHungerFull { get => SkeletonWarHungerFull; set => SkeletonWarHungerFull = value; }
        // public int EnemyHungerDurationSeconds { get => SkeletonWarHungerDurationSeconds; set => SkeletonWarHungerDurationSeconds = value; }

        public int EnemyHitPoints { get => SkeletonWarHitPoints; set => SkeletonWarHitPoints = value; }
        public int EnemyMaxHP { get => SkeletonWarMaxHP; set => SkeletonWarMaxHP = value; }
        public int EnemyXPPerLevel { get => SkeletonWarXPPerLevel; set => SkeletonWarXPPerLevel = value; }

        public int EnemyLevelCap { get => SkeletonWarLevelCap; set => SkeletonWarLevelCap = value; }
        public int EnemyStrength { get => skeletonWarDamageAmount; set => skeletonWarDamageAmount = value; }
        public int EnemyAlacrity { get => SkeletonWarAlacrityAmount; set => SkeletonWarAlacrityAmount = value; }
        public int EnemyCelerity { get => SkeletonWarMovementSpeed; set => SkeletonWarMovementSpeed = value; }
        public int EnemyLimenity { get => punchRange; set => punchRange = value; }
        public int EnemyIntelligence { get => SpellDamage; set => SpellDamage = value; }

        // Unique class stats
        public string EnemyResourceName => "Warbone Authority";
        public int EnemyResourceValue { get => SkeletonWarAuthorityPoints; set => SkeletonWarAuthorityPoints = value; }
        public string EnemyRegenLabel => "Authority on Strike";
        public int EnemyRegenValue { get => SkeletonWarAuthorityOnHit; set => SkeletonWarAuthorityOnHit = value; }
        public string EnemyMaxResourceName => "Max Warbone Authority";
        public int EnemyMaxResourceValue { get => SkeletonWarMaxAuthorityPoints; set => SkeletonWarMaxAuthorityPoints = value; }
        public int EnemyLifeRegen { get => SkeletonWarLifeRegenRate; set => SkeletonWarLifeRegenRate = value; }
        public int EnemyStatPoints { get => SkeletonWarStatPoints; set => SkeletonWarStatPoints = value; }

        // Enemy Color Theme Info
        public string EnemyHPColor => "rgba(70,130,180,.8)";    // steel blue
        public string EnemyInvColor => "rgba(192,192,192,1.0)"; // silver
        public string EnemyEnergyColor => "rgba(0,191,255,.7)"; // cyan authority

        // Debugger
        public string EnemyCollisionBoxStyle => SkeletonWarCollisionBoxStyle;
        public string EnemySpriteDebugStyle => SkeletonWarSpriteDebugStyle;
        public string EnemyPunchBoxStyle => SkeletonWarPunchBoxStyle;

        // Core state




        // XP Gain

        public void AddXp(BloodLevel levelReg, double multiplier)
        {
            levelReg.AddXp("SkelWar", multiplier);
        }


        public void ClearHitEffect() => ClearSkeletonWarHitEffects();
        // Core Logic
        public int SkeletonWarZIndex { get; private set; } = 6000;
        public int SkeletonWarX { get; set; }
        public int SkeletonWarY { get; set; }

        public int SkeletonWarWidth { get; set; } = 84;
        public int SkeletonWarHeight { get; set; } = 84;

        public string SkeletonWarXpx => $"{SkeletonWarX}px";
        public string SkeletonWarYpx => $"{SkeletonWarY}px";
        public int SkeletonWarPunchX { get; set; } = 24;
        public int SkeletonWarPunchY { get; set; } = 24;


        public int SkeletonWarLevelCap { get; set; } = 100;
        public int SkeletonWarXPPerLevel { get; set; } = 50;
        public int SkeletonWarXP { get; set; } = 0;
        public int SkeletonWarLevel { get; set; } = 1;
        public int SkeletonWarHungerCurrent { get; set; } = 2000;   // starting calories
        public int SkeletonWarHungerFull { get; set; } = 2000;      // max capacity
        public int SkeletonWarHungerDurationSeconds { get; set; } = 86400; // 24 hours

        private DateTime _lastHungerTick = DateTime.Now;
        public int SkeletonWarMaxHP { get; set; } = 10;

        public int SkeletonWarStatPoints { get; set; } = 0;
        public int SkeletonWarHitPoints { get; set; } = 10;

        private int skeletonWarDamageAmount = 2;

        public int SkeletonWarAlacrityAmount = 4;

        public  int SkeletonWarMovementSpeed = 8;

        public int punchRange = 15;
        public int SpellDamage = 0;

        public int SkeletonWarAuthorityPoints { get; set; } = 0;

        public int SkeletonWarMaxAuthorityPoints { get; set; } = 10;
        public int SkeletonWarAuthorityOnHit { get; set; } = 1;


        public int SkeletonWarLifeRegenRate { get; set; } = 0;
        public int PatrolLeftBound { get; set; } = 24;
        public int PatrolRightBound { get; set; } = 1994;
        public int PatrolTopBound { get; set; } = 24;
        public int PatrolBottomBound { get; set; } = 1994;

        public bool SkeletonWarIsAlive => SkeletonWarHitPoints > 0;
        public bool ShowSkeletonWarHitEffect { get; set; } = false;

        private int skeletonWarFrame = 0;
        private DateTime lastSkeletonWarFrameTime = DateTime.Now;
        private bool isOneShotSkeletonWarAnimation = false;
        private SkeletonWarAnimationState currentSkeletonWarAnimation = SkeletonWarAnimationState.Idle;

        public SkeletonWarAnimationState CurrentSkeletonWarAnimation => currentSkeletonWarAnimation;

        public string SkeletonWarBackgroundPosition => $"-{skeletonWarFrame * SkeletonWarFrameWidth}px 0";

        private bool skeletonWarMovingRight = true;
        private DateTime skeletonWarIdleUntil = DateTime.MinValue;
        public DateTime SkeletonWarIdleUntil => skeletonWarIdleUntil;

        public const int SkeletonWarFrameWidth = 84;
        public const int SkeletonWarFrameHeight = 84;

        public Rectangle SkeletonWarCollisionBox =>
            new Rectangle(
                SkeletonWarX + SkeletonWarCollisionShiftX,
                SkeletonWarY + SkeletonWarCollisionShiftY,
                SkeletonWarCollisionWidth,
                SkeletonWarCollisionHeight
            );

        private const int SkeletonWarCollisionWidth = 24;
        private const int SkeletonWarCollisionHeight = 24;
        private const int SkeletonWarCollisionShiftX = 32;
        private const int SkeletonWarCollisionShiftY = 48;

        public const int SkeletonWarPunchShiftX = 32;
        public const int SkeletonWarPunchShiftY = 32;

        public Rectangle SkeletonWarPunchBox =>
          new Rectangle(
              SkeletonWarX + SkeletonWarPunchShiftX - punchRange,
              SkeletonWarY + SkeletonWarPunchShiftX - punchRange,
              SkeletonWarPunchX + (2 * punchRange),
              SkeletonWarPunchY + (2 * punchRange)
          );

        public string SkeletonWarSpriteStyle =>
         $"position:absolute; left:{SkeletonWarX}px; top:{SkeletonWarY}px; " +
         $"width:{SkeletonWarWidth}px; height:{SkeletonWarHeight}px; " +
         $"background-image:url('{SkeletonWarSpriteSheet}'); " +
         $"background-position:-{skeletonWarFrame * SkeletonWarWidth}px 0px; " +
         $"background-repeat:no-repeat; background-color:transparent; " +
         $"opacity:0.95; user-select:none; touch-action:manipulation; z-index:{SkeletonWarZIndex};";

        public void SkeletonWarTakeDamage(int amount = 1)
        {
            SkeletonWarHitPoints = Math.Max(SkeletonWarHitPoints - amount, 0);
            ShowSkeletonWarHitEffect = true;
        }

        public void ClearSkeletonWarHitEffects() => ShowSkeletonWarHitEffect = false;

        private DateTime lastSkeletonWarAttackTime = DateTime.MinValue;
        private bool isAttacking = false;

        public void SkeletonWarAttack(IBloodiCharacter active)
        {
            int attackIntervalMs = 1000 / Math.Max(1, SkeletonWarAlacrityAmount);

            if ((DateTime.Now - lastSkeletonWarAttackTime).TotalMilliseconds < attackIntervalMs)
                return;
            if (isAttacking) return; // guard

            lastSkeletonWarAttackTime = DateTime.Now;
            isAttacking = true;

            _ = Task.Run(async () =>
            {
                try
                {
                    SetSkeletonWarAnimation(SkeletonWarAnimationState.Attack);
                    isOneShotSkeletonWarAnimation = true;

                    int maxFrames = animationFrameCounts.TryGetValue(SkeletonWarAnimationState.Attack, out var count) ? count : 12;
                    int adjustedDelay = attackIntervalMs / maxFrames;
                    int damageDelayMs = adjustedDelay * Math.Max(1, maxFrames - 3);

                    await Task.Delay(damageDelayMs);

                    if (active.CharIsAlive && SkeletonWarPunchBox.IntersectsWith(active.CharCollisionBox))
                    {
                        active.CharTakeDamage(skeletonWarDamageAmount);

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



        public enum SkeletonWarAnimationState
        {
            Idle,
            WalkLeft,
            WalkRight,
            Attack
        }

        private readonly Dictionary<SkeletonWarAnimationState, int> skeletonWarAnimationSpeeds = new()
    {
        { SkeletonWarAnimationState.Idle, 120},
        { SkeletonWarAnimationState.WalkLeft, 50 },
        { SkeletonWarAnimationState.WalkRight, 50},
        { SkeletonWarAnimationState.Attack, 50 }
    };

        private readonly Dictionary<SkeletonWarAnimationState, int> animationFrameCounts = new()
    {
        { SkeletonWarAnimationState.Idle, 7 },
        { SkeletonWarAnimationState.WalkLeft, 8 },
        { SkeletonWarAnimationState.WalkRight, 8 },
        { SkeletonWarAnimationState.Attack, 12}
    };

        public string SkeletonWarSpriteSheet => currentSkeletonWarAnimation switch
        {
            SkeletonWarAnimationState.Idle => "/iAssets/SkeleSwordIdle01.png",
            SkeletonWarAnimationState.WalkLeft => "/iAssets/SkeleSwordLeft01.png",
            SkeletonWarAnimationState.WalkRight => "/iAssets/SkeleSwordRight01.png",
            SkeletonWarAnimationState.Attack => "/iAssets/SkeleSwordAttack01.png",
            _ => "/iAssets/SkeleSwordIdle01.png"
        };

        public void SetSkeletonWarAnimation(SkeletonWarAnimationState newState)
        {
            if (newState != currentSkeletonWarAnimation)
            {
                skeletonWarFrame = 0;
                lastSkeletonWarFrameTime = DateTime.Now;
            }
            currentSkeletonWarAnimation = newState;
        }
        private int lastZIndexY = -1;
        private const int ZIndexUpdateThreshold = 20; // Only recalc when moved 20px vertically
                                                      // ✅ OPTIMIZED TickSkelPyschoAnimation (same model as other enemies)
                                                      // ✅ OPTIMIZED TickSkeletonWarAnimation (same model as other enemies)
        public void TickSkeletonWarAnimation()
        {
            // 🔹 ONLY update Z-index if SkeletonWar moved significantly in Y direction
            if (Math.Abs(SkeletonWarY - lastZIndexY) > ZIndexUpdateThreshold)
            {
                UpdateSkeletonWarZIndex();
                lastZIndexY = SkeletonWarY;
            }

            // Rest of animation logic stays the same
            int delay = skeletonWarAnimationSpeeds.TryGetValue(currentSkeletonWarAnimation, out var ms) ? ms : 150;
            int maxFrames = animationFrameCounts.TryGetValue(currentSkeletonWarAnimation, out var count) ? count : 1;

            if ((DateTime.Now - lastSkeletonWarFrameTime).TotalMilliseconds >= delay)
            {
                skeletonWarFrame++;

                if (isOneShotSkeletonWarAnimation && skeletonWarFrame >= maxFrames)
                {
                    SetSkeletonWarAnimation(SkeletonWarAnimationState.Idle);
                    isOneShotSkeletonWarAnimation = false;
                }
                else
                {
                    skeletonWarFrame %= maxFrames;
                }

                lastSkeletonWarFrameTime = DateTime.Now;
            }
        }

        // 🔹 NEW: Separate Z-index calculation for SkeletonWar
        private void UpdateSkeletonWarZIndex()
        {
            var ordered = ZIndexCache.GetSortedStaticObjects();

            foreach (var obj in ordered)
            {
                if (SkeletonWarY < obj.CollisionBox.Y)
                {
                    SkeletonWarZIndex = obj.ZIndex - 1;
                    return; // Early exit
                }
            }

            SkeletonWarZIndex = 6000; // Default if no match
        }

        // 🔹 ALTERNATIVE: Binary search for even faster lookup
        private void UpdateSkeletonWarZIndexFast()
        {
            var ordered = ZIndexCache.GetSortedStaticObjects();

            // Binary search since list is sorted by Y
            int left = 0;
            int right = ordered.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                var obj = ordered[mid];

                if (SkeletonWarY < obj.CollisionBox.Y)
                {
                    // Check if this is the first object below us
                    if (mid == 0 || ordered[mid - 1].CollisionBox.Y <= SkeletonWarY)
                    {
                        SkeletonWarZIndex = obj.ZIndex - 1;
                        return;
                    }
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            SkeletonWarZIndex = 6000; // No object below us
        }


        public bool HasAggressionTarget { get; private set; } = false;

        public IBloodiCharacter? ActiveCharacter { get; private set; }


        public void SetAggressionTarget(IBloodiCharacter active)
        {
            if (!SkeletonWarIsAlive) return;

            ActiveCharacter = active;
            HasAggressionTarget = true;
        }


        public void TickSkeletonWarMovement(IBloodiCharacter active)
        {
            if (!SkeletonWarIsAlive)
                return;


            // Attack if in range and not already attacking
            if (SkeletonWarIsAlive &&
        currentSkeletonWarAnimation != SkeletonWarAnimationState.Attack &&
        SkeletonWarPunchBox.IntersectsWith(active.CharCollisionBox))
            {
                SkeletonWarAttack(active);
                return;
            }

            // Idle timer
            if (DateTime.Now < skeletonWarIdleUntil)
            {
                SetSkeletonWarAnimation(SkeletonWarAnimationState.Idle);
                TickSkeletonWarAnimation();
                return;
            }

            // Aggression targeting simplified: always chase the active character
            if (active != null && active.CharIsAlive)
            {
                int targetCenterX = active.CharX + 42;
                int targetCenterY = active.CharY + 42;

                int skeletonCenterX = SkeletonWarX + (SkeletonWarWidth / 2);
                int skeletonCenterY = SkeletonWarY + (SkeletonWarHeight / 2);

                if (Math.Abs(skeletonCenterX - targetCenterX) > SkeletonWarMovementSpeed)
                {
                    if (skeletonCenterX < targetCenterX && SkeletonWarX + SkeletonWarMovementSpeed + SkeletonWarWidth < PatrolRightBound)
                    {
                        SkeletonWarX += SkeletonWarMovementSpeed;
                        SetSkeletonWarAnimation(SkeletonWarAnimationState.WalkRight);
                    }
                    else if (skeletonCenterX > targetCenterX && SkeletonWarX - SkeletonWarMovementSpeed > PatrolLeftBound)
                    {
                        SkeletonWarX -= SkeletonWarMovementSpeed;
                        SetSkeletonWarAnimation(SkeletonWarAnimationState.WalkLeft);
                    }
                }

                if (Math.Abs(skeletonCenterY - targetCenterY) > SkeletonWarMovementSpeed)
                {
                    if (skeletonCenterY < targetCenterY)
                        SkeletonWarY += SkeletonWarMovementSpeed;
                    else if (skeletonCenterY > targetCenterY)
                        SkeletonWarY -= SkeletonWarMovementSpeed;
                }

                if (Math.Abs(skeletonCenterX - targetCenterX) <= SkeletonWarMovementSpeed &&
                    Math.Abs(skeletonCenterY - targetCenterY) <= SkeletonWarMovementSpeed)
                {
                    SetSkeletonWarAnimation(SkeletonWarAnimationState.Idle);
                }

                TickSkeletonWarAnimation();
                return;
            }

            // Patrol fallback
            if (skeletonWarMovingRight)
            {
                SkeletonWarX += SkeletonWarMovementSpeed;
                SetSkeletonWarAnimation(SkeletonWarAnimationState.WalkRight);

                if (SkeletonWarX >= PatrolRightBound)
                {
                    skeletonWarMovingRight = false;
                    skeletonWarIdleUntil = DateTime.Now.AddMilliseconds(2000);
                }
            }
            else
            {
                SkeletonWarX -= SkeletonWarMovementSpeed;
                SetSkeletonWarAnimation(SkeletonWarAnimationState.WalkLeft);

                if (SkeletonWarX <= PatrolLeftBound)
                {
                    skeletonWarMovingRight = true;
                    skeletonWarIdleUntil = DateTime.Now.AddMilliseconds(2000);
                }
            }

            TickSkeletonWarAnimation();
        }


        public string SkeletonWarSpriteDebugStyle =>
            $"position:absolute; left:{SkeletonWarX}px; top:{SkeletonWarY}px; " +
            $"width:{SkeletonWarWidth}px; height:{SkeletonWarHeight}px; " +
            $"background-color:rgba(0,255,0,0.2); border:1px dashed lime; z-index:998;";

        public string SkeletonWarCollisionBoxStyle =>
            $"position:absolute; left:{SkeletonWarX + SkeletonWarCollisionShiftX}px; top:{SkeletonWarY + SkeletonWarCollisionShiftY}px; " +
            $"width:{SkeletonWarCollisionWidth}px; height:{SkeletonWarCollisionHeight}px; " +
            $"background-color:rgba(255,0,0,0.3); border:2px solid red; z-index:999;";

        public string SkeletonWarPunchBoxStyle =>
            $"position:absolute; left:{SkeletonWarPunchBox.X}px; top:{SkeletonWarPunchBox.Y}px; " +
            $"width:{SkeletonWarPunchBox.Width}px; height:{SkeletonWarPunchBox.Height}px; " +
            $"background-color:rgba(255,0,255,0.3); border:2px solid magenta; z-index:999;";

    }


    public class BloodSkeletonWarRegistry
    {
        public static List<SkeletonWar> All = new();

        public static void SpawnSkeletonWars(int count)
        {
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                All.Add(new SkeletonWar
                {
                    SkeletonWarX = rand.Next(25, 1994),
                    SkeletonWarY = rand.Next(25, 1994),
                });
            }
        }
    }























}

