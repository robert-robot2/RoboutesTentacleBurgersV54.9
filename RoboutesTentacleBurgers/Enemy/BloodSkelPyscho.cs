
using RoboutesTentacleBurgers.Services;
using static RoboutesTentacleBurgers.Services.ZIndexCache;

public class BloodSkelPyscho
{
    public class SkelPyscho : IiEnemy
    {

        // IBloodiCharacter implementation
        public void EnemyAttack(IBloodiCharacter active) => SkelPyschoAttack(active);
        public void EnemyMove(IBloodiCharacter active) =>  TickSkelPyschoMovement(active);
        //   public void StopMovement(string key) => StopMovementWarrior(key);
        public void EnemyTickAllAnimation() => TickSkelPyschoAnimation();
        public void SetAggression(IBloodiCharacter active) =>  SetAggressionTarget(active);
        public bool EnemyIsAlive => SkelPyschoHitPoints > 0;
        public int EnemyX => SkelPyschoX;
        public int EnemyY => SkelPyschoY;
        public string SpriteStyle => SkelPyschoSpriteStyle;
        public string? HitEffectPath => ShowSkelPyschoHitEffect ? "/iAssets/SkeleHit01.png" : null;
        public Rectangle EnemyCollisionBox => SkelPyschoCollisionBox;
        public Rectangle EnemyPunchBox => SkelPyschoPunchBox;
        public void EnemyTakeDamage(int amount) => SkelPyschoTakeDamage(amount);
        public void ClearHitEffect() => ClearSkelPyschoHitEffects();
        public List<SplatterPuddle> SplatterPuddles { get; } = new();

        // Stats
        public string EnemyClassName => "SkelPyscho";
        public int EnemyLevel { get => SkelPyschoLevel; set => SkelPyschoLevel = value; }
        public int EnemyXP { get => SkelPyschoXP; set => SkelPyschoXP = value; }

        public int EnemyHungerCurrent { get => 0; set { } }
        public int EnemyHungerFull { get => 0; set { } }
        public int EnemyHungerDurationSeconds { get => 0; set { } }

        // public int EnemyHungerCurrent { get => SkelPyschoHungerCurrent; set => SkelPyschoHungerCurrent = value; }
        // public int EnemyHungerFull { get => SkelPyschoHungerFull; set => SkelPyschoHungerFull = value; }
        // public int EnemyHungerDurationSeconds { get => SkelPyschoHungerDurationSeconds; set => SkelPyschoHungerDurationSeconds = value; }

        public int EnemyHitPoints { get => SkelPyschoHitPoints; set => SkelPyschoHitPoints = value; }
        public int EnemyMaxHP { get => SkelPyschoMaxHP; set => SkelPyschoMaxHP = value; }
        public int EnemyXPPerLevel { get => SkelPyschoXPPerLevel; set => SkelPyschoXPPerLevel = value; }

        public int EnemyLevelCap { get => SkelPyschoLevelCap; set => SkelPyschoLevelCap = value; }
        public int EnemyStrength { get => skelPyschoDamageAmount; set => skelPyschoDamageAmount = value; }
        public int EnemyAlacrity { get => SkelPyschoAlacrityAmount; set => SkelPyschoAlacrityAmount = value; }
        public int EnemyCelerity { get => SkelPyschoMovementSpeed; set => SkelPyschoMovementSpeed = value; }
        public int EnemyLimenity { get => punchRange; set => punchRange = value; }
        public int EnemyIntelligence { get => SpellDamage; set => SpellDamage = value; }

        // Unique class stats
        public string EnemyResourceName => "Bone Frenzy";
        public int EnemyResourceValue { get => SkelPyschoRagePoints; set => SkelPyschoRagePoints = value; }
        public string EnemyRegenLabel => "Frenzy on Hit";
        public int EnemyRegenValue { get => SkelPyschoRageOnHit; set => SkelPyschoRageOnHit = value; }
        public string EnemyMaxResourceName => "Max Bone Frenzy";
        public int EnemyMaxResourceValue { get => SkelPyschoMaxRagePoints; set => SkelPyschoMaxRagePoints = value; }
        public int EnemyLifeRegen { get => SkelPyschoLifeRegenRate; set => SkelPyschoLifeRegenRate = value; }
        public int EnemyStatPoints { get => SkelPyschoStatPoints; set => SkelPyschoStatPoints = value; }

        // Enemy Color Theme Info
        public string EnemyHPColor => "rgba(200,0,0,.8)";
        public string EnemyInvColor => "rgba(100,255,0,1.0)";
        public string EnemyEnergyColor => "rgba(150,0,150,.7)";

        // Debugger
        public string EnemyCollisionBoxStyle => SkelPyschoCollisionBoxStyle;
        public string EnemySpriteDebugStyle => SkelPyschoSpriteDebugStyle;
        public string EnemyPunchBoxStyle => SkelPyschoPunchBoxStyle;
        // Core state











        public void AddXp(BloodLevel levelReg, double multiplier)
        {
            levelReg.AddXp("SkelPyscho", multiplier);
        }


        // Core Logic
        public int SkelPyschoZIndex { get; private set; } = 6000;
        public int SkelPyschoX { get; set; }
        public int SkelPyschoY { get; set; }

        public int SkelPyschoWidth { get; set; } = 84;
        public int SkelPyschoHeight { get; set; } = 84;

        public string SkelPyschoXpx => $"{SkelPyschoX}px";
        public string SkelPyschoYpx => $"{SkelPyschoY}px";
        public int SkelPyschoPunchX { get; set; } = 24;
        public int SkelPyschoPunchY { get; set; } = 24;




        public int SkelPyschoLevelCap { get; set; } = 100;
        public int SkelPyschoXPPerLevel { get; set; } = 50;
        public int SkelPyschoXP { get; set; } = 0;
        public int SkelPyschoLevel { get; set; } = 1;
        public int SkelPyschoHungerCurrent { get; set; } = 2000;   // starting calories
        public int SkelPyschoHungerFull { get; set; } = 2000;      // max capacity
        public int SkelPyschoHungerDurationSeconds { get; set; } = 86400; // 24 hours

        private DateTime _lastHungerTick = DateTime.Now;
        public int SkelPyschoMaxHP { get; set; } = 4;

        public int SkelPyschoStatPoints { get; set; } = 0;

        public int SkelPyschoHitPoints { get; set; } = 4;

        private int skelPyschoDamageAmount = 1;

        public int SkelPyschoAlacrityAmount = 2;

        public int SkelPyschoMovementSpeed = 11;

        public int punchRange = 6;

        public int SpellDamage = 0;

        public int SkelPyschoRagePoints { get; set; } = 0;

        public int SkelPyschoMaxRagePoints { get; set; } = 10;
        public int SkelPyschoRageOnHit { get; set; } = 1;


        public int SkelPyschoLifeRegenRate { get; set; } = 0;

        public int PatrolLeftBound { get; set; } = 24;
        public int PatrolRightBound { get; set; } = 1994;
        public int PatrolTopBound { get; set; } = 24;
        public int PatrolBottomBound { get; set; } = 1994;

        public bool SkelPyschoIsAlive => SkelPyschoHitPoints > 0;
        public bool ShowSkelPyschoHitEffect { get; set; } = false;

        private int skelPyschoFrame = 0;
        private DateTime lastSkelPyschoFrameTime = DateTime.Now;
        private bool isOneShotSkelPyschoAnimation = false;
        private SkelPyschoAnimationState currentSkelPyschoAnimation = SkelPyschoAnimationState.Idle;

        public SkelPyschoAnimationState CurrentSkelPyschoAnimation => currentSkelPyschoAnimation;

        public string SkelPyschoBackgroundPosition => $"-{skelPyschoFrame * SkelPyschoFrameWidth}px 0";

        private bool skelPyschoMovingRight = true;
        private DateTime skelPyschoIdleUntil = DateTime.MinValue;
        public DateTime SkelPyschoIdleUntil => skelPyschoIdleUntil;

        public const int SkelPyschoFrameWidth = 84;
        public const int SkelPyschoFrameHeight = 84;

        public Rectangle SkelPyschoCollisionBox =>
            new Rectangle(
                SkelPyschoX + SkelPyschoCollisionShiftX,
                SkelPyschoY + SkelPyschoCollisionShiftY,
                SkelPyschoCollisionWidth,
                SkelPyschoCollisionHeight
            );

        private const int SkelPyschoCollisionWidth = 24;
        private const int SkelPyschoCollisionHeight = 24;
        private const int SkelPyschoCollisionShiftX = 32;
        private const int SkelPyschoCollisionShiftY = 48;

        public const int SkelPyschoPunchShiftX = 32;
        public const int SkelPyschoPunchShiftY = 32;

        public Rectangle SkelPyschoPunchBox =>
          new Rectangle(
              SkelPyschoX + SkelPyschoPunchShiftX - punchRange,
              SkelPyschoY + SkelPyschoPunchShiftX - punchRange,
              SkelPyschoPunchX + (2 * punchRange),
              SkelPyschoPunchY + (2 * punchRange)
          );

        public string SkelPyschoSpriteStyle =>
         $"position:absolute; left:{SkelPyschoX}px; top:{SkelPyschoY}px; " +
         $"width:{SkelPyschoWidth}px; height:{SkelPyschoHeight}px; " +
         $"background-image:url('{SkelPyschoSpriteSheet}'); " +
         $"background-position:-{skelPyschoFrame * SkelPyschoWidth}px 0px; " +
         $"background-repeat:no-repeat; background-color:transparent; " +
         $"opacity:0.95; user-select:none; touch-action:manipulation; z-index:{SkelPyschoZIndex};";

        public void SkelPyschoTakeDamage(int amount = 1)
        {
            SkelPyschoHitPoints = Math.Max(SkelPyschoHitPoints - amount, 0);
            ShowSkelPyschoHitEffect = true;
        }

        public void ClearSkelPyschoHitEffects() => ShowSkelPyschoHitEffect = false;

        private DateTime lastSkelPyschoAttackTime = DateTime.MinValue;
        private bool isAttacking = false;

        public void SkelPyschoAttack(IBloodiCharacter active)
        {
            int attackIntervalMs = 1000 / Math.Max(1, SkelPyschoAlacrityAmount);

            if ((DateTime.Now - lastSkelPyschoAttackTime).TotalMilliseconds < attackIntervalMs)
                return;
            if (isAttacking) return; // guard

            lastSkelPyschoAttackTime = DateTime.Now;
            isAttacking = true;

            _ = Task.Run(async () =>
            {
                try
                {
                    SetSkelPyschoAnimation(SkelPyschoAnimationState.Attack);
                    isOneShotSkelPyschoAnimation = true;

                    int maxFrames = animationFrameCounts.TryGetValue(SkelPyschoAnimationState.Attack, out var count) ? count : 12;
                    int adjustedDelay = attackIntervalMs / maxFrames;
                    int damageDelayMs = adjustedDelay * Math.Max(1, maxFrames - 3);

                    await Task.Delay(damageDelayMs);

                    if (active.CharIsAlive && SkelPyschoPunchBox.IntersectsWith(active.CharCollisionBox))
                    {
                        active.CharTakeDamage(skelPyschoDamageAmount);

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



        public enum SkelPyschoAnimationState
        {
            Idle,
            WalkLeft,
            WalkRight,
            Attack
        }

        private readonly Dictionary<SkelPyschoAnimationState, int> skelPyschoAnimationSpeeds = new()
    {
        { SkelPyschoAnimationState.Idle, 120},
        { SkelPyschoAnimationState.WalkLeft, 50 },
        { SkelPyschoAnimationState.WalkRight, 50},
        { SkelPyschoAnimationState.Attack, 50 }
    };

        private readonly Dictionary<SkelPyschoAnimationState, int> animationFrameCounts = new()
    {
        { SkelPyschoAnimationState.Idle, 6 },
        { SkelPyschoAnimationState.WalkLeft, 6 },
        { SkelPyschoAnimationState.WalkRight, 6 },
        { SkelPyschoAnimationState.Attack, 6}
    };

        public string SkelPyschoSpriteSheet => currentSkelPyschoAnimation switch
        {
            SkelPyschoAnimationState.Idle => "/iAssets/SkeletonIdle01.png",
            SkelPyschoAnimationState.WalkLeft => "/iAssets/SkeletonLeftWalk01.png",
            SkelPyschoAnimationState.WalkRight => "/iAssets/SkeletonRightWalk01.png",
            SkelPyschoAnimationState.Attack => "/iAssets/SkeletonPunch01.png",
            _ => "/iAssets/SkeletonIdle01.png"
        };

      



        public void SetSkelPyschoAnimation(SkelPyschoAnimationState newState)
        {
            if (newState != currentSkelPyschoAnimation)
            {
                skelPyschoFrame = 0;
                lastSkelPyschoFrameTime = DateTime.Now;
            }
            currentSkelPyschoAnimation = newState;
        }
        private int lastZIndexY = -1;
        private const int ZIndexUpdateThreshold = 20; // Only recalc when moved 20px vertically
                                                      // ✅ OPTIMIZED TickSkelPyschoAnimation (same model as other enemies)
        public void TickSkelPyschoAnimation()
        {
            // 🔹 ONLY update Z-index if SkelPyscho moved significantly in Y direction
            if (Math.Abs(SkelPyschoY - lastZIndexY) > ZIndexUpdateThreshold)
            {
                UpdateSkelPyschoZIndex();
                lastZIndexY = SkelPyschoY;
            }

            // Rest of animation logic stays the same
            int delay = skelPyschoAnimationSpeeds.TryGetValue(currentSkelPyschoAnimation, out var ms) ? ms : 150;
            int maxFrames = animationFrameCounts.TryGetValue(currentSkelPyschoAnimation, out var count) ? count : 1;

            if ((DateTime.Now - lastSkelPyschoFrameTime).TotalMilliseconds >= delay)
            {
                skelPyschoFrame++;

                if (isOneShotSkelPyschoAnimation && skelPyschoFrame >= maxFrames)
                {
                    SetSkelPyschoAnimation(SkelPyschoAnimationState.Idle);
                    isOneShotSkelPyschoAnimation = false;
                }
                else
                {
                    skelPyschoFrame %= maxFrames;
                }

                lastSkelPyschoFrameTime = DateTime.Now;
            }
        }

        // 🔹 NEW: Separate Z-index calculation for SkelPyscho
        private void UpdateSkelPyschoZIndex()
        {
            var ordered = ZIndexCache.GetSortedStaticObjects();

            foreach (var obj in ordered)
            {
                if (SkelPyschoY < obj.CollisionBox.Y)
                {
                    SkelPyschoZIndex = obj.ZIndex - 1;
                    return; // Early exit
                }
            }

            SkelPyschoZIndex = 6000; // Default if no match
        }

        // 🔹 ALTERNATIVE: Binary search for even faster lookup
        private void UpdateSkelPyschoZIndexFast()
        {
            var ordered = ZIndexCache.GetSortedStaticObjects();

            // Binary search since list is sorted by Y
            int left = 0;
            int right = ordered.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                var obj = ordered[mid];

                if (SkelPyschoY < obj.CollisionBox.Y)
                {
                    // Check if this is the first object below us
                    if (mid == 0 || ordered[mid - 1].CollisionBox.Y <= SkelPyschoY)
                    {
                        SkelPyschoZIndex = obj.ZIndex - 1;
                        return;
                    }
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            SkelPyschoZIndex = 6000; // No object below us
        }



        public bool HasAggressionTarget { get; private set; } = false;


        public IBloodiCharacter? ActiveCharacter { get; private set; }



        public void SetAggressionTarget(IBloodiCharacter active)
        {
            if (!SkelPyschoIsAlive) return;

            ActiveCharacter = active;
            HasAggressionTarget = true;
        }

       
        public void TickSkelPyschoMovement(IBloodiCharacter active)
        {
            if (!SkelPyschoIsAlive)
                return;

            // Attack if in range and not already attacking
            if (SkelPyschoIsAlive &&
        currentSkelPyschoAnimation != SkelPyschoAnimationState.Attack &&
        SkelPyschoPunchBox.IntersectsWith(active.CharCollisionBox))
            {
                SkelPyschoAttack(active);
                return;
            }


            // Idle timer
            if (DateTime.Now < skelPyschoIdleUntil)
            {
                SetSkelPyschoAnimation(SkelPyschoAnimationState.Idle);
                TickSkelPyschoAnimation();
                return;
            }

            // Aggression targeting simplified: always chase the active character
            if (active != null && active.CharIsAlive)
            {
                int targetCenterX = active.CharX + 42;
                int targetCenterY = active.CharY + 42;

                int psychoCenterX = SkelPyschoX + (SkelPyschoWidth / 2);
                int psychoCenterY = SkelPyschoY + (SkelPyschoHeight / 2);

                if (Math.Abs(psychoCenterX - targetCenterX) > SkelPyschoMovementSpeed)
                {
                    if (psychoCenterX < targetCenterX && SkelPyschoX + SkelPyschoMovementSpeed + SkelPyschoWidth < PatrolRightBound)
                    {
                        SkelPyschoX += SkelPyschoMovementSpeed;
                        SetSkelPyschoAnimation(SkelPyschoAnimationState.WalkRight);
                    }
                    else if (psychoCenterX > targetCenterX && SkelPyschoX - SkelPyschoMovementSpeed > PatrolLeftBound)
                    {
                        SkelPyschoX -= SkelPyschoMovementSpeed;
                        SetSkelPyschoAnimation(SkelPyschoAnimationState.WalkLeft);
                    }
                }

                if (Math.Abs(psychoCenterY - targetCenterY) > SkelPyschoMovementSpeed)
                {
                    if (psychoCenterY < targetCenterY)
                        SkelPyschoY += SkelPyschoMovementSpeed;
                    else if (psychoCenterY > targetCenterY)
                        SkelPyschoY -= SkelPyschoMovementSpeed;
                }

                if (Math.Abs(psychoCenterX - targetCenterX) <= SkelPyschoMovementSpeed &&
                    Math.Abs(psychoCenterY - targetCenterY) <= SkelPyschoMovementSpeed)
                {
                    SetSkelPyschoAnimation(SkelPyschoAnimationState.Idle);
                }

                TickSkelPyschoAnimation();
                return;
            }

            // Patrol fallback
            if (skelPyschoMovingRight)
            {
                SkelPyschoX += SkelPyschoMovementSpeed;
                SetSkelPyschoAnimation(SkelPyschoAnimationState.WalkRight);

                if (SkelPyschoX >= PatrolRightBound)
                {
                    skelPyschoMovingRight = false;
                    skelPyschoIdleUntil = DateTime.Now.AddMilliseconds(2000);
                }
            }
            else
            {
                SkelPyschoX -= SkelPyschoMovementSpeed;
                SetSkelPyschoAnimation(SkelPyschoAnimationState.WalkLeft);

                if (SkelPyschoX <= PatrolLeftBound)
                {
                    skelPyschoMovingRight = true;
                    skelPyschoIdleUntil = DateTime.Now.AddMilliseconds(2000);
                }
            }

            TickSkelPyschoAnimation();
        }








        public string SkelPyschoSpriteDebugStyle =>
            $"position:absolute; left:{SkelPyschoX}px; top:{SkelPyschoY}px; " +
            $"width:{SkelPyschoWidth}px; height:{SkelPyschoHeight}px; " +
            $"background-color:rgba(0,255,0,0.2); border:1px dashed lime; z-index:998;";

        public string SkelPyschoCollisionBoxStyle =>
            $"position:absolute; left:{SkelPyschoX + SkelPyschoCollisionShiftX}px; top:{SkelPyschoY + SkelPyschoCollisionShiftY}px; " +
            $"width:{SkelPyschoCollisionWidth}px; height:{SkelPyschoCollisionHeight}px; " +
            $"background-color:rgba(255,0,0,0.3); border:2px solid red; z-index:999;";

        public string SkelPyschoPunchBoxStyle =>
            $"position:absolute; left:{SkelPyschoPunchBox.X}px; top:{SkelPyschoPunchBox.Y}px; " +
            $"width:{SkelPyschoPunchBox.Width}px; height:{SkelPyschoPunchBox.Height}px; " +
            $"background-color:rgba(255,0,255,0.3); border:2px solid magenta; z-index:999;";

    }




    public class BloodSkelPyschoRegistry
    {
        public static List<SkelPyscho> All = new();

        public static void SpawnSkelPyschos(int count)
        {
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                All.Add(new SkelPyscho
                {
                    SkelPyschoX = rand.Next(25, 1994),
                    SkelPyschoY = rand.Next(25, 1994),
                });
            }
        }
    }























}

