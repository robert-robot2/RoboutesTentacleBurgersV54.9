using RoboutesTentacleBurgers.Services;
using static RoboutesTentacleBurgers.Services.ZIndexCache;

public class BloodZombiePyscho
{
     public class ZombiePyscho : IiEnemy
    {
        public void EnemyAttack(IBloodiCharacter active) => ZombiePyschoAttack(active);
        public void EnemyMove(IBloodiCharacter active) => TickZombiePyschoMovement(active);
        public void EnemyTickAllAnimation() => TickZombiePyschoAnimation();
        public void SetAggression(IBloodiCharacter active) => SetAggressionTarget(active);
        public bool EnemyIsAlive => ZombiePyschoHitPoints > 0;
        public int EnemyX => ZombiePyschoX;
        public int EnemyY => ZombiePyschoY;
        public string SpriteStyle => ZombiePyschoSpriteStyle;
        public string? HitEffectPath => ShowZombiePyschoHitEffect ? "/iAssets/ZombieHit01.png" : null;
        public Rectangle EnemyCollisionBox => ZombiePyschoCollisionBox;
        public Rectangle EnemyPunchBox => ZombiePyschoPunchBox;
        public void EnemyTakeDamage(int amount) => ZombiePyschoTakeDamage(amount);
        public void ClearHitEffect() => ClearZombiePyschoHitEffects();
        public List<SplatterPuddle> SplatterPuddles { get; } = new();

        // Stats
        public string EnemyClassName => "ZombiePsycho";
        public int EnemyLevel { get => ZombiePyschoLevel; set => ZombiePyschoLevel = value; }
        public int EnemyXP { get => ZombiePyschoXP; set => ZombiePyschoXP = value; }

        public int EnemyHungerCurrent { get => 0; set { } }
        public int EnemyHungerFull { get => 0; set { } }
        public int EnemyHungerDurationSeconds { get => 0; set { } }

        // public int EnemyHungerCurrent { get => ZombiePsychoHungerCurrent; set => ZombiePsychoHungerCurrent = value; }
        // public int EnemyHungerFull { get => ZombiePsychoHungerFull; set => ZombiePsychoHungerFull = value; }
        // public int EnemyHungerDurationSeconds { get => ZombiePsychoHungerDurationSeconds; set => ZombiePsychoHungerDurationSeconds = value; }

        public int EnemyHitPoints { get => ZombiePyschoHitPoints; set => ZombiePyschoHitPoints = value; }
        public int EnemyMaxHP { get => ZombiePyschoMaxHP; set => ZombiePyschoMaxHP = value; }
        public int EnemyXPPerLevel { get => ZombiePyschoXPPerLevel; set => ZombiePyschoXPPerLevel = value; }

        public int EnemyLevelCap { get => ZombiePyschoLevelCap; set => ZombiePyschoLevelCap = value; }
        public int EnemyStrength { get => zombiePyschoDamageAmount; set => zombiePyschoDamageAmount = value; }
        public int EnemyAlacrity { get => ZombiePyschoAlacrityAmount; set => ZombiePyschoAlacrityAmount = value; }
        public int EnemyCelerity { get => ZombiePyschoMovementSpeed; set => ZombiePyschoMovementSpeed = value; }
        public int EnemyLimenity { get => punchRange; set => punchRange = value; }
        public int EnemyIntelligence { get => SpellDamage; set => SpellDamage = value; }

        // Unique class stats
        public string EnemyResourceName => "Psychotic Rage";
        public int EnemyResourceValue { get => ZombiePyschoRagePoints; set => ZombiePyschoRagePoints = value; }
        public string EnemyRegenLabel => "Rage on Frenzy";
        public int EnemyRegenValue { get => ZombiePyschoRageOnHit; set => ZombiePyschoRageOnHit = value; }
        public string EnemyMaxResourceName => "Max Psychotic Rage";
        public int EnemyMaxResourceValue { get => ZombiePyschoMaxRagePoints; set => ZombiePyschoMaxRagePoints = value; }
        public int EnemyLifeRegen { get => ZombiePyschoLifeRegenRate; set => ZombiePyschoLifeRegenRate = value; }
        public int EnemyStatPoints { get => ZombiePyschoStatPoints; set => ZombiePyschoStatPoints = value; }

        // Enemy Color Theme Info
        public string EnemyHPColor => "rgba(200,0,0,.8)";
        public string EnemyInvColor => "rgba(100,255,0,1.0)";
        public string EnemyEnergyColor => "rgba(150,0,150,.7)";

        // Debugger
        public string EnemyCollisionBoxStyle => ZombiePyschoCollisionBoxStyle;
        public string EnemySpriteDebugStyle => ZombiePyschoSpriteDebugStyle;
        public string EnemyPunchBoxStyle => ZombiePyschoPunchBoxStyle;
        // Core state


        // XP gain
        public void AddXp(BloodLevel levelReg, double multiplier)
        {
            levelReg.AddXp("ZombiePyscho", multiplier);
        }
        public int ZombiePyschoZIndex { get; private set; } = 6000;
        public int ZombiePyschoX { get; set; }
        public int ZombiePyschoY { get; set; }

        public int ZombiePyschoWidth { get; set; } = 84;
        public int ZombiePyschoHeight { get; set; } = 84;

        public string ZombiePyschoXpx => $"{ZombiePyschoX}px";
        public string ZombiePyschoYpx => $"{ZombiePyschoY}px";
        public int ZombiePyschoPunchX { get; set; } = 24;
        public int ZombiePyschoPunchY { get; set; } = 24;



        public int ZombiePyschoLevelCap { get; set; } = 100;
        public int ZombiePyschoXPPerLevel { get; set; } = 50;
        public int ZombiePyschoXP { get; set; } = 0;
        public int ZombiePyschoLevel { get; set; } = 1;
        public int ZombiePyschoHungerCurrent { get; set; } = 2000;   // starting calories
        public int ZombiePyschoHungerFull { get; set; } = 2000;      // max capacity
        public int ZombiePyschoHungerDurationSeconds { get; set; } = 86400; // 24 hours

        private DateTime _lastHungerTick = DateTime.Now;
        public int ZombiePyschoMaxHP { get; set; } = 6;

        public int ZombiePyschoStatPoints { get; set; } = 0;


        public int ZombiePyschoHitPoints { get; set; } = 6;

        private int zombiePyschoDamageAmount = 2;

        public int ZombiePyschoAlacrityAmount = 1;

        public int ZombiePyschoMovementSpeed = 1;

        public int punchRange = 6;

        public int SpellDamage = 0;

        public int ZombiePyschoRagePoints { get; set; } = 0;

        public int ZombiePyschoMaxRagePoints { get; set; } = 10;
        public int ZombiePyschoRageOnHit { get; set; } = 1;


        public int ZombiePyschoLifeRegenRate { get; set; } = 0;

        public int PatrolLeftBound { get; set; } = 24;
        public int PatrolRightBound { get; set; } = 1994;
        public int PatrolTopBound { get; set; } = 24;
        public int PatrolBottomBound { get; set; } = 1994;

        public bool ZombiePyschoIsAlive => ZombiePyschoHitPoints > 0;
        public bool ShowZombiePyschoHitEffect { get; set; } = false;

        private int zombiePyschoFrame = 0;
        private DateTime lastZombiePyschoFrameTime = DateTime.Now;
        private bool isOneShotZombiePyschoAnimation = false;
        private ZombiePyschoAnimationState currentZombiePyschoAnimation = ZombiePyschoAnimationState.Idle;

        public ZombiePyschoAnimationState CurrentZombiePyschoAnimation => currentZombiePyschoAnimation;

        public string ZombiePyschoBackgroundPosition => $"-{zombiePyschoFrame * ZombiePyschoFrameWidth}px 0";

        private bool zombiePyschoMovingRight = true;
        private DateTime zombiePyschoIdleUntil = DateTime.MinValue;
        public DateTime ZombiePyschoIdleUntil => zombiePyschoIdleUntil;

        public const int ZombiePyschoFrameWidth = 84;
        public const int ZombiePyschoFrameHeight = 84;

        public Rectangle ZombiePyschoCollisionBox =>
            new Rectangle(
                ZombiePyschoX + ZombiePyschoCollisionShiftX,
                ZombiePyschoY + ZombiePyschoCollisionShiftY,
                ZombiePyschoCollisionWidth,
                ZombiePyschoCollisionHeight
            );

        private const int ZombiePyschoCollisionWidth = 24;
        private const int ZombiePyschoCollisionHeight = 24;
        private const int ZombiePyschoCollisionShiftX = 32;
        private const int ZombiePyschoCollisionShiftY = 48;

        public const int ZombiePyschoPunchShiftX = 32;
        public const int ZombiePyschoPunchShiftY = 32;

        public Rectangle ZombiePyschoPunchBox =>
          new Rectangle(
              ZombiePyschoX + ZombiePyschoPunchShiftX - punchRange,
              ZombiePyschoY + ZombiePyschoPunchShiftX - punchRange,
              ZombiePyschoPunchX + (2 * punchRange),
              ZombiePyschoPunchY + (2 * punchRange)
          );

        public string ZombiePyschoSpriteStyle =>
         $"position:absolute; left:{ZombiePyschoX}px; top:{ZombiePyschoY}px; " +
         $"width:{ZombiePyschoWidth}px; height:{ZombiePyschoHeight}px; " +
         $"background-image:url('{ZombiePyschoSpriteSheet}'); " +
         $"background-position:-{zombiePyschoFrame * ZombiePyschoWidth}px 0px; " +
         $"background-repeat:no-repeat; background-color:transparent; " +
         $"opacity:0.95; user-select:none; touch-action:manipulation; z-index:{ZombiePyschoZIndex};";

        private static readonly Random Randomizer = new Random();

        public void ZombiePyschoTakeDamage(int amount = 1)
        {
            ZombiePyschoHitPoints = Math.Max(ZombiePyschoHitPoints - amount, 0);
            ShowZombiePyschoHitEffect = true;
            BloodSplatterRegistry.Add(new SplatterPuddle
            {
                X = EnemyCollisionBox.X + (int)(Randomizer.NextDouble() * 10 - 5), // ±5px
                Y = EnemyCollisionBox.Y + (int)(Randomizer.NextDouble() * 10 - 5),

                Scale = Math.Min(1.0 + (amount * 0.25), 3.0) * (0.8 + Randomizer.NextDouble() * 0.4),


            });
        }

        public void ClearZombiePyschoHitEffects() => ShowZombiePyschoHitEffect = false;

        private DateTime lastZombiePyschoAttackTime = DateTime.MinValue;
        private bool isAttacking = false;

        public void ZombiePyschoAttack(IBloodiCharacter active)
        {
            int attackIntervalMs = 1000 / Math.Max(1, ZombiePyschoAlacrityAmount);

            if ((DateTime.Now - lastZombiePyschoAttackTime).TotalMilliseconds < attackIntervalMs)
                return;
            if (isAttacking) return; // guard

            lastZombiePyschoAttackTime = DateTime.Now;
            isAttacking = true;

            _ = Task.Run(async () =>
            {
                try
                {
                    SetZombiePyschoAnimation(ZombiePyschoAnimationState.Attack);
                    isOneShotZombiePyschoAnimation = true;

                    int maxFrames = animationFrameCounts.TryGetValue(ZombiePyschoAnimationState.Attack, out var count) ? count : 12;
                    int adjustedDelay = attackIntervalMs / maxFrames;
                    int damageDelayMs = adjustedDelay * Math.Max(1, maxFrames - 3);

                    await Task.Delay(damageDelayMs);

                    if (active.CharIsAlive && ZombiePyschoPunchBox.IntersectsWith(active.CharCollisionBox))
                    {
                        active.CharTakeDamage(zombiePyschoDamageAmount);

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


        public enum ZombiePyschoAnimationState
        {
            Idle,
            WalkLeft,
            WalkRight,
            Attack
        }

        private readonly Dictionary<ZombiePyschoAnimationState, int> zombiePyschoAnimationSpeeds = new()
        {
            { ZombiePyschoAnimationState.Idle, 120},
            { ZombiePyschoAnimationState.WalkLeft, 50 },
            { ZombiePyschoAnimationState.WalkRight, 50},
            { ZombiePyschoAnimationState.Attack, 50 }
        };

        private readonly Dictionary<ZombiePyschoAnimationState, int> animationFrameCounts = new()
        {
            { ZombiePyschoAnimationState.Idle, 10 },
            { ZombiePyschoAnimationState.WalkLeft, 23 },
            { ZombiePyschoAnimationState.WalkRight, 23 },
            { ZombiePyschoAnimationState.Attack, 12}
        };

        public string ZombiePyschoSpriteSheet => currentZombiePyschoAnimation switch
        {
            ZombiePyschoAnimationState.Idle => "/iAssets/ZombieIdleCell01.png",
            ZombiePyschoAnimationState.WalkLeft => "/iAssets/ZombieLeftWalkCell01.png",
            ZombiePyschoAnimationState.WalkRight => "/iAssets/ZombieRightWalkCell01.png",
            ZombiePyschoAnimationState.Attack => "/iAssets/ZombieAttackCell01.png",
            _ => "/iAssets/ZombieIdleCell01.png"
        };

        public void SetZombiePyschoAnimation(ZombiePyschoAnimationState newState)
        {
            if (newState != currentZombiePyschoAnimation)
            {
                zombiePyschoFrame = 0;
                lastZombiePyschoFrameTime = DateTime.Now;
            }
            currentZombiePyschoAnimation = newState;
        }
        private int lastZIndexY = -1;
        private const int ZIndexUpdateThreshold = 20; // Only recalc when moved 20px vertically
                                                      // ✅ OPTIMIZED TickZombiePyschoAnimation (same model as other enemies)
        public void TickZombiePyschoAnimation()
        {
            // 🔹 ONLY update Z-index if ZombiePyscho moved significantly in Y direction
            if (Math.Abs(ZombiePyschoY - lastZIndexY) > ZIndexUpdateThreshold)
            {
                UpdateZombiePyschoZIndex();
                lastZIndexY = ZombiePyschoY;
            }

            // Rest of animation logic stays the same
            int delay = zombiePyschoAnimationSpeeds.TryGetValue(currentZombiePyschoAnimation, out var ms) ? ms : 150;
            int maxFrames = animationFrameCounts.TryGetValue(currentZombiePyschoAnimation, out var count) ? count : 1;

            if ((DateTime.Now - lastZombiePyschoFrameTime).TotalMilliseconds >= delay)
            {
                zombiePyschoFrame++;

                if (isOneShotZombiePyschoAnimation && zombiePyschoFrame >= maxFrames)
                {
                    SetZombiePyschoAnimation(ZombiePyschoAnimationState.Idle);
                    isOneShotZombiePyschoAnimation = false;
                }
                else
                {
                    zombiePyschoFrame %= maxFrames;
                }

                lastZombiePyschoFrameTime = DateTime.Now;
            }
        }

        // 🔹 NEW: Separate Z-index calculation for ZombiePyscho
        private void UpdateZombiePyschoZIndex()
        {
            var ordered = ZIndexCache.GetSortedStaticObjects();

            foreach (var obj in ordered)
            {
                if (ZombiePyschoY < obj.CollisionBox.Y)
                {
                    ZombiePyschoZIndex = obj.ZIndex - 1;
                    return; // Early exit
                }
            }

            ZombiePyschoZIndex = 6000; // Default if no match
        }

        // 🔹 ALTERNATIVE: Binary search for even faster lookup
        private void UpdateZombiePyschoZIndexFast()
        {
            var ordered = ZIndexCache.GetSortedStaticObjects();

            // Binary search since list is sorted by Y
            int left = 0;
            int right = ordered.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                var obj = ordered[mid];

                if (ZombiePyschoY < obj.CollisionBox.Y)
                {
                    // Check if this is the first object below us
                    if (mid == 0 || ordered[mid - 1].CollisionBox.Y <= ZombiePyschoY)
                    {
                        ZombiePyschoZIndex = obj.ZIndex - 1;
                        return;
                    }
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            ZombiePyschoZIndex = 6000; // No object below us
        }


        public bool HasAggressionTarget { get; private set; } = false;
        public IBloodiCharacter? ActiveCharacter { get; private set; }
        public void SetAggressionTarget(IBloodiCharacter active)
        {
            if (!ZombiePyschoIsAlive) return;

            ActiveCharacter = active;
            HasAggressionTarget = true;
        }

        public void TickZombiePyschoMovement(IBloodiCharacter active)
        {
            if (!ZombiePyschoIsAlive)
                return;

            if (ZombiePyschoIsAlive &&
                currentZombiePyschoAnimation != ZombiePyschoAnimationState.Attack &&
                ZombiePyschoPunchBox.IntersectsWith(active.CharCollisionBox))
            {
                ZombiePyschoAttack(active);
                return;
            }

            if (DateTime.Now < zombiePyschoIdleUntil)
            {
                SetZombiePyschoAnimation(ZombiePyschoAnimationState.Idle);
                TickZombiePyschoAnimation();
                return;
            }

            if (active != null && active.CharIsAlive)
            {
                int targetCenterX = active.CharX + 42;
                int targetCenterY = active.CharY + 42;

                int psychoCenterX = ZombiePyschoX + (ZombiePyschoWidth / 2);
                int psychoCenterY = ZombiePyschoY + (ZombiePyschoHeight / 2);

                if (Math.Abs(psychoCenterX - targetCenterX) > ZombiePyschoMovementSpeed)
                {
                    if (psychoCenterX < targetCenterX && ZombiePyschoX + ZombiePyschoMovementSpeed + ZombiePyschoWidth < PatrolRightBound)
                    {
                        ZombiePyschoX += ZombiePyschoMovementSpeed;
                        SetZombiePyschoAnimation(ZombiePyschoAnimationState.WalkRight);
                    }
                    else if (psychoCenterX > targetCenterX && ZombiePyschoX - ZombiePyschoMovementSpeed > PatrolLeftBound)
                    {
                        ZombiePyschoX -= ZombiePyschoMovementSpeed;
                        SetZombiePyschoAnimation(ZombiePyschoAnimationState.WalkLeft);
                    }
                }

                if (Math.Abs(psychoCenterY - targetCenterY) > ZombiePyschoMovementSpeed)
                {
                    if (psychoCenterY < targetCenterY)
                        ZombiePyschoY += ZombiePyschoMovementSpeed;
                    else if (psychoCenterY > targetCenterY)
                        ZombiePyschoY -= ZombiePyschoMovementSpeed;
                }

                if (Math.Abs(psychoCenterX - targetCenterX) <= ZombiePyschoMovementSpeed &&
                    Math.Abs(psychoCenterY - targetCenterY) <= ZombiePyschoMovementSpeed)
                {
                    SetZombiePyschoAnimation(ZombiePyschoAnimationState.Idle);
                }

                TickZombiePyschoAnimation();
                return;
            }

            if (zombiePyschoMovingRight)
            {
                ZombiePyschoX += ZombiePyschoMovementSpeed;
                SetZombiePyschoAnimation(ZombiePyschoAnimationState.WalkRight);

                if (ZombiePyschoX >= PatrolRightBound)
                {
                    zombiePyschoMovingRight = false;
                    zombiePyschoIdleUntil = DateTime.Now.AddMilliseconds(2000);
                }
            }
            else
            {
                ZombiePyschoX -= ZombiePyschoMovementSpeed;
                SetZombiePyschoAnimation(ZombiePyschoAnimationState.WalkLeft);

                if (ZombiePyschoX <= PatrolLeftBound)
                {
                    zombiePyschoMovingRight = true;
                    zombiePyschoIdleUntil = DateTime.Now.AddMilliseconds(2000);
                }
            }

            TickZombiePyschoAnimation();
        }

        public string ZombiePyschoSpriteDebugStyle =>
            $"position:absolute; left:{ZombiePyschoX}px; top:{ZombiePyschoY}px; " +
            $"width:{ZombiePyschoWidth}px; height:{ZombiePyschoHeight}px; " +
            $"background-color:rgba(0,255,0,0.2); border:1px dashed lime; z-index:998;";

        public string ZombiePyschoCollisionBoxStyle =>
            $"position:absolute; left:{ZombiePyschoX + ZombiePyschoCollisionShiftX}px; top:{ZombiePyschoY + ZombiePyschoCollisionShiftY}px; " +
            $"width:{ZombiePyschoCollisionWidth}px; height:{ZombiePyschoCollisionHeight}px; " +
            $"background-color:rgba(255,0,0,0.3); border:2px solid red; z-index:999;";

        public string ZombiePyschoPunchBoxStyle =>
            $"position:absolute; left:{ZombiePyschoPunchBox.X}px; top:{ZombiePyschoPunchBox.Y}px; " +
            $"width:{ZombiePyschoPunchBox.Width}px; height:{ZombiePyschoPunchBox.Height}px; " +
            $"background-color:rgba(255,0,255,0.3); border:2px solid magenta; z-index:999;";
    }
    public class BloodZombiePyschoRegistry
    {
        public static List<ZombiePyscho> All = new();

        public static void SpawnZombiePyschos(int count)
        {
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                All.Add(new ZombiePyscho
                {
                    ZombiePyschoX = rand.Next(25, 1994),
                    ZombiePyschoY = rand.Next(25, 1994),
                });
            }
        }
    }






}