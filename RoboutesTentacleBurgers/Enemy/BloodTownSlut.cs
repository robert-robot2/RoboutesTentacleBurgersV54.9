

using RoboutesTentacleBurgers.Services;
using static RoboutesTentacleBurgers.Services.ZIndexCache;


public class BloodTownSlut
{
 
    public class TownSlut : IiEnemy
    {
        public void EnemyAttack(IBloodiCharacter active) =>  TownSlutAttack(active);
        public void EnemyMove(IBloodiCharacter active) => TickTownSlutMovement(active);
        public void EnemyTickAllAnimation() => TickTownSlutAnimation();
        public void SetAggression(IBloodiCharacter active) => SetAggressionTarget(active);
        public bool EnemyIsAlive => TownSlutHitPoints > 0;
        public int EnemyX => TownSlutX;
        public int EnemyY => TownSlutY;
        public string SpriteStyle => TownSlutSpriteStyle;
        public string? HitEffectPath => ShowTownSlutHitEffect ? "/iAssets/WarriorGothit01.png" : null;
        public Rectangle EnemyCollisionBox => TownSlutCollisionBox;
        public Rectangle EnemyPunchBox => TownSlutPunchBox;
        public void EnemyTakeDamage(int amount) => TownSlutTakeDamage(amount);
        public void ClearHitEffect() => ClearTownSlutHitEffects();

        public List<SplatterPuddle> SplatterPuddles { get; } = new();

        // Stats
        public string EnemyClassName => "TownSlut";
        public int EnemyLevel { get => TownSlutLevel; set => TownSlutLevel = value; }
        public int EnemyXP { get => TownSlutXP; set => TownSlutXP = value; }

        public int EnemyHungerCurrent { get => 0; set { } }
        public int EnemyHungerFull { get => 0; set { } }
        public int EnemyHungerDurationSeconds { get => 0; set { } }

        // public int EnemyHungerCurrent { get => TownSlutHungerCurrent; set => TownSlutHungerCurrent = value; }
        // public int EnemyHungerFull { get => TownSlutHungerFull; set => TownSlutHungerFull = value; }
        // public int EnemyHungerDurationSeconds { get => TownSlutHungerDurationSeconds; set => TownSlutHungerDurationSeconds = value; }

        public int EnemyHitPoints { get => TownSlutHitPoints; set => TownSlutHitPoints = value; }
        public int EnemyMaxHP { get => TownSlutMaxHP; set => TownSlutMaxHP = value; }
        public int EnemyXPPerLevel { get => TownSlutXPPerLevel; set => TownSlutXPPerLevel = value; }

        public int EnemyLevelCap { get => TownSlutLevelCap; set => TownSlutLevelCap = value; }
        public int EnemyStrength { get => townSlutDamageAmount; set => townSlutDamageAmount = value; }
        public int EnemyAlacrity { get => TownSlutAlacrityAmount; set => TownSlutAlacrityAmount = value; }
        public int EnemyCelerity { get => TownSlutMovementSpeed; set => TownSlutMovementSpeed = value; }
        public int EnemyLimenity { get => punchRange; set => punchRange = value; }
        public int EnemyIntelligence { get => SpellDamage; set => SpellDamage = value; }

        // Unique class stats
        public string EnemyResourceName => "Charm Influence";
        public int EnemyResourceValue { get => TownSlutInfluencePoints; set => TownSlutInfluencePoints = value; }
        public string EnemyRegenLabel => "Influence on Contact";
        public int EnemyRegenValue { get => TownSlutInfluenceOnHit; set => TownSlutInfluenceOnHit = value; }
        public string EnemyMaxResourceName => "Max Charm Influence";
        public int EnemyMaxResourceValue { get => TownSlutMaxInfluencePoints; set => TownSlutMaxInfluencePoints = value; }
        public int EnemyLifeRegen { get => TownSlutLifeRegenRate; set => TownSlutLifeRegenRate = value; }
        public int EnemyStatPoints { get => TownSlutStatPoints; set => TownSlutStatPoints = value; }

        // Enemy Color Theme Info
        public string EnemyHPColor => "rgba(255,20,147,.8)";   // deep pink
        public string EnemyInvColor => "rgba(255,182,193,1.0)"; // light pink
        public string EnemyEnergyColor => "rgba(255,105,180,.7)"; // hot pink energy

        // Debugger
        public string EnemyCollisionBoxStyle => TownSlutCollisionBoxStyle;
        public string EnemySpriteDebugStyle => TownSlutSpriteDebugStyle;
        public string EnemyPunchBoxStyle => TownSlutPunchBoxStyle;
        // Core state


        // XP Gain

        public void AddXp(BloodLevel levelReg, double multiplier)
        {
            levelReg.AddXp("TownSlut", multiplier);
        }
        public int TownSlutZIndex { get; private set; } = 6000;
        public int TownSlutX { get; set; }
        public int TownSlutY { get; set; }
        public int TownSlutWidth { get; set; } = 64;
        public int TownSlutHeight { get; set; } = 64;
        public string TownSlutXpx => $"{TownSlutX}px";
        public string TownSlutYpx => $"{TownSlutY}px";
        public int TownSlutPunchX { get; set; } = 24;
        public int TownSlutPunchY { get; set; } = 24;
        public int TownSlutLevelCap { get; set; } = 100;
        public int TownSlutXPPerLevel { get; set; } = 50;
        public int TownSlutXP { get; set; } = 0;
        public int TownSlutLevel { get; set; } = 1;
        public int TownSlutHungerCurrent { get; set; } = 2000;   // starting calories
        public int TownSlutHungerFull { get; set; } = 2000;      // max capacity
        public int TownSlutHungerDurationSeconds { get; set; } = 86400; // 24 hours

        private DateTime _lastHungerTick = DateTime.Now;
        public int TownSlutMaxHP { get; set; } = 100000;

        public int TownSlutStatPoints { get; set; } = 0;
        public int TownSlutHitPoints { get; set; } = 100000;

        private int townSlutDamageAmount = 0;
        public int TownSlutAlacrityAmount = 1;
        public int TownSlutMovementSpeed = 4;
        public int punchRange = 15;
        public int SpellDamage = 0;

        public int TownSlutInfluencePoints { get; set; } = 0;

        public int TownSlutMaxInfluencePoints { get; set; } = 10;
        public int TownSlutInfluenceOnHit { get; set; } = 1;


        public int TownSlutLifeRegenRate { get; set; } = 0;
        public bool TownSlutIsAlive => TownSlutHitPoints > 0;
        public bool ShowTownSlutHitEffect { get; set; } = false;

        private int townSlutFrame = 0;
        private DateTime lastTownSlutFrameTime = DateTime.Now;
        private bool isOneShotTownSlutAnimation = false;
        private TownSlutAnimationState currentTownSlutAnimation = TownSlutAnimationState.Idle;
        public TownSlutAnimationState CurrentTownSlutAnimation => currentTownSlutAnimation;
        public string TownSlutBackgroundPosition => $"-{townSlutFrame * TownSlutFrameWidth}px 0";
        private DateTime townSlutIdleUntil = DateTime.MinValue;
        public DateTime TownSlutIdleUntil => townSlutIdleUntil;
        public int PatrolLeftBound { get; set; } = 24;
        public int PatrolRightBound { get; set; } = 1994;
        public int PatrolTopBound { get; set; } = 24;
        public int PatrolBottomBound { get; set; } = 1994;

        public const int TownSlutFrameWidth = 64;
        public const int TownSlutFrameHeight = 64;

        public Rectangle TownSlutCollisionBox =>
            new Rectangle(TownSlutX + TownSlutCollisionShiftX, TownSlutY + TownSlutCollisionShiftY, TownSlutCollisionWidth, TownSlutCollisionHeight);

        private const int TownSlutCollisionWidth = 24;
        private const int TownSlutCollisionHeight = 24;
        private const int TownSlutCollisionShiftX = 32;
        private const int TownSlutCollisionShiftY = 48;

        public const int TownSlutPunchShiftX = 32;
        public const int TownSlutPunchShiftY = 32;

        public Rectangle TownSlutPunchBox =>
            new Rectangle(TownSlutX + TownSlutPunchShiftX - punchRange, TownSlutY + TownSlutPunchShiftY - punchRange,
                          TownSlutPunchX + (2 * punchRange), TownSlutPunchY + (2 * punchRange));

        public string TownSlutSpriteStyle =>
            $"position:absolute; left:{TownSlutX}px; top:{TownSlutY}px; " +
            $"width:{TownSlutWidth}px; height:{TownSlutHeight}px; " +
            $"background-image:url('{TownSlutSpriteSheet}'); " +
            $"background-position:-{townSlutFrame * TownSlutWidth}px 0px; " +
            $"background-repeat:no-repeat; background-color:transparent; " +
            $"opacity:0.95; user-select:none; touch-action:manipulation; z-index:{TownSlutZIndex};";
        private static readonly Random Randomizer = new Random();
        public void TownSlutTakeDamage(int amount = 1)
        {
            TownSlutHitPoints = Math.Max(TownSlutHitPoints - amount, 0);
            ShowTownSlutHitEffect = true;
            BloodSplatterRegistry.Add(new SplatterPuddle
            {
                X = EnemyCollisionBox.X + (int)(Randomizer.NextDouble() * 10 - 5), // ±5px
                Y = EnemyCollisionBox.Y + (int)(Randomizer.NextDouble() * 10 - 5),

                Scale = Math.Min(1.0 + (amount * 0.25), 3.0) * (0.8 + Randomizer.NextDouble() * 0.4),


            });
        }

        public void ClearTownSlutHitEffects() => ShowTownSlutHitEffect = false;

        private DateTime lastTownSlutAttackTime = DateTime.MinValue;
        private bool isAttacking = false;

        public void TownSlutAttack(IBloodiCharacter active)
        {
            int attackIntervalMs = 1000 / Math.Max(1, TownSlutAlacrityAmount);

            if ((DateTime.Now - lastTownSlutAttackTime).TotalMilliseconds < attackIntervalMs)
                return;
            if (isAttacking) return; // guard

            lastTownSlutAttackTime = DateTime.Now;
            isAttacking = true;

            _ = Task.Run(async () =>
            {
                try
                {
                    SetTownSlutAnimation(TownSlutAnimationState.Attack);
                    isOneShotTownSlutAnimation = true;

                    int maxFrames = animationFrameCounts.TryGetValue(TownSlutAnimationState.Attack, out var count) ? count : 12;
                    int adjustedDelay = attackIntervalMs / maxFrames;
                    int damageDelayMs = adjustedDelay * Math.Max(1, maxFrames - 3);

                    await Task.Delay(damageDelayMs);

                    if (active.CharIsAlive && TownSlutPunchBox.IntersectsWith(active.CharCollisionBox))
                    {
                        active.CharTakeDamage(townSlutDamageAmount);

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


        public enum TownSlutAnimationState { Idle, Attack }

        private readonly Dictionary<TownSlutAnimationState, int> townSlutAnimationSpeeds = new()
        {
            { TownSlutAnimationState.Idle, 50 },
            { TownSlutAnimationState.Attack, 50 }
        };

        private readonly Dictionary<TownSlutAnimationState, int> animationFrameCounts = new()
        {
            { TownSlutAnimationState.Idle, 12 },
            { TownSlutAnimationState.Attack, 16}
        };

        public string TownSlutSpriteSheet => currentTownSlutAnimation switch
        {
            TownSlutAnimationState.Idle => "/iAssets/BTSlut001.png",
            TownSlutAnimationState.Attack => "/iAssets/HarlotAttack002.png",
            _ => "/iAssets/BTSlut001.png"
        };

        public void SetTownSlutAnimation(TownSlutAnimationState newState)
        {
            if(newState != currentTownSlutAnimation)
            {
                townSlutFrame = 0;
                lastTownSlutFrameTime = DateTime.Now;
            }
            currentTownSlutAnimation = newState;
        }

        // ✅ OPTIMIZED TickTownSlutAnimation (same model as Skeleton/Cow)
        // Add these fields to your enemy classes (Skeleton, Cat, Cow, TownSlut):
        private int lastZIndexY = -1;
        private const int ZIndexUpdateThreshold = 20; // Only recalc when moved 20px vertically
        public void TickTownSlutAnimation()
        {
            // 🔹 ONLY update Z-index if TownSlut moved significantly in Y direction
            if (Math.Abs(TownSlutY - lastZIndexY) > ZIndexUpdateThreshold)
            {
                UpdateTownSlutZIndex();
                lastZIndexY = TownSlutY;
            }

            // Rest of animation logic stays the same
            int delay = townSlutAnimationSpeeds.TryGetValue(currentTownSlutAnimation, out var ms) ? ms : 150;
            int maxFrames = animationFrameCounts.TryGetValue(currentTownSlutAnimation, out var count) ? count : 1;

            if ((DateTime.Now - lastTownSlutFrameTime).TotalMilliseconds >= delay)
            {
                townSlutFrame++;

                if (isOneShotTownSlutAnimation && townSlutFrame >= maxFrames)
                {
                    SetTownSlutAnimation(TownSlutAnimationState.Idle);
                    isOneShotTownSlutAnimation = false;
                }
                else
                {
                    townSlutFrame %= maxFrames;
                }

                lastTownSlutFrameTime = DateTime.Now;
            }
        }

        // 🔹 NEW: Separate Z-index calculation for TownSlut
        private void UpdateTownSlutZIndex()
        {
            var ordered = ZIndexCache.GetSortedStaticObjects();

            foreach (var obj in ordered)
            {
                if (TownSlutY < obj.CollisionBox.Y)
                {
                    TownSlutZIndex = obj.ZIndex - 1;
                    return; // Early exit
                }
            }

            TownSlutZIndex = 6000; // Default if no match
        }

        // 🔹 ALTERNATIVE: Binary search for even faster lookup
        private void UpdateTownSlutZIndexFast()
        {
            var ordered = ZIndexCache.GetSortedStaticObjects();

            // Binary search since list is sorted by Y
            int left = 0;
            int right = ordered.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                var obj = ordered[mid];

                if (TownSlutY < obj.CollisionBox.Y)
                {
                    // Check if this is the first object below us
                    if (mid == 0 || ordered[mid - 1].CollisionBox.Y <= TownSlutY)
                    {
                        TownSlutZIndex = obj.ZIndex - 1;
                        return;
                    }
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            TownSlutZIndex = 6000; // No object below us
        }


        public void SetAggressionTarget(IBloodiCharacter active) { }

        public void TickTownSlutMovement(IBloodiCharacter active)
        {
            if(!TownSlutIsAlive) return;



            // Attack if in range and not already attacking
            if (TownSlutIsAlive &&
        currentTownSlutAnimation != TownSlutAnimationState.Attack &&
        TownSlutPunchBox.IntersectsWith(active.CharCollisionBox))
            {
                TownSlutAttack(active);
                return;
            }

            if (DateTime.Now < townSlutIdleUntil)
            {
                SetTownSlutAnimation(TownSlutAnimationState.Idle);
                TickTownSlutAnimation();
                return;
            }
        }

        public string TownSlutPunchBoxStyle =>
            $"position:absolute; left:{TownSlutPunchBox.X}px; top:{TownSlutPunchBox.Y}px;" +
            $"width:{TownSlutPunchBox.Width}px; height:{TownSlutPunchBox.Height}px;" +
            $"background-color:rgba(255,0,255,0.3); border:2px solid magenta; z-index:999;";

        public string TownSlutSpriteDebugStyle =>
            $"position:absolute; left:{TownSlutX}px; top:{TownSlutY}px;" +
            $"width:{TownSlutWidth}px; height:{TownSlutHeight}px; " +
            $"background-color:rgba(0,255,0,0.2); border:1px dashed lime; z-index:";
        public string TownSlutCollisionBoxStyle =>
    $"position:absolute; left:{TownSlutX + TownSlutCollisionShiftX}px; top:{TownSlutY + TownSlutCollisionShiftY}px;" +
    $"width:{TownSlutCollisionWidth}px; height:{TownSlutCollisionHeight}px;" +
    $"background-color:rgba(255,0,0,0.3); border:2px solid red; z-index:999;";
    }

    public class BloodTownSlutRegistry
    {
        public static List<TownSlut> All = new();

        public static void SpawnTownSluts(int count)
        {
            var rand = new Random();

            for(int i = 0; i < count; i++)
            {
                All.Add(new TownSlut
                {
                    TownSlutX = rand.Next(25, 1994),
                    TownSlutY = rand.Next(25, 1994),
                });
            }
        }
    }
}
