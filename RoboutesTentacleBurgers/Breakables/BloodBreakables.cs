namespace RoboutesTentacleBurgers.Breakables
{
    public class BloodBreakables
    {
        public BloodCharacterHandle CharacterHandle { get; set; } = new();
        // Safe proxy, allows null checks
        public IBloodiCharacter? ActiveCharacter => CharacterHandle.ActiveCharacter;
        //public IBloodiCharacter ActiveCharacter { get; set; }= default!;



        // Dummy Object
        public class Dummy:IBreakables
        {
            public int BreakX { get => DummyX; set => DummyX = value; }
            public int BreakY { get => DummyY; set => DummyY = value; }
            public int BreakWidth { get => DummyWidth; set => DummyWidth = value; }
            public int BreakHeight { get => DummyHeight; set => DummyHeight = value; }
            public int BreakHitPoints { get => DummyHitPoints; set => DummyHitPoints = value; }
            public bool BreakIsAlive => DummyIsAlive;
            public Rectangle BreakCollisionBox => DummyCollisionBox;


            public void BreakTakeDamage(int amount)=> DummyTakeDamage(amount);
            public void BreakClearHitEffects()=> DummyClearHitEffects();
            public string? BreakHitEffectPath => ShowDummyHitEffect ? "/iAssets/DummyHit005.png" : null;
            public string BreakSpriteStyle => DummySpriteStyle;
            public string BreakSpriteDebugStyle => DummySpriteDebugStyle;
            public string BreakCollisionDebugStyle => DummyCollisionDebugStyle;

            //core
            public int DummyX { get; set; }
            public int DummyY { get; set; }
            public int DummyWidth { get; set; } = 64;
            public int DummyHeight { get; set; } = 64;
            public int DummyHitPoints { get; set; } = 5;
            public bool ShowDummyHitEffect { get; set; } = false;
            public bool DummyIsAlive => DummyHitPoints > 0;

            public string DummyXpx => $"{DummyX}px";
            public string DummyYpx => $"{DummyY}px";

            public Rectangle DummyCollisionBox =>
                new Rectangle(
                    DummyX + DummyCollisionShiftX,
                    DummyY + DummyCollisionShiftY,
                    DummyCollisionWidth,
                    DummyCollisionHeight
                );

            private const int DummyCollisionWidth = 24;
            private const int DummyCollisionHeight = 24;
            private const int DummyCollisionShiftX = 20;
            private const int DummyCollisionShiftY = 42;


            public void DummyTakeDamage(int amount = 1)
            {
                DummyHitPoints = Math.Max(DummyHitPoints - amount, 0);
                ShowDummyHitEffect = true;
            }

            public void DummyClearHitEffects()
            {
                ShowDummyHitEffect = false;
            }


            public string DummySpriteStyle =>
     $"position:absolute; left:{DummyX}px; top:{DummyY}px; " +
     $"width:{DummyWidth}px; height:{DummyHeight}px; " +
     $"background-image:url('/iAssets/Dummy005.png'); " +
     $"background-position:0px 0px; " +
     $"background-repeat:no-repeat; " +
     $"opacity:0.95; user-select:none; touch-action:manipulation; z-index:2500;";


            public string DummySpriteDebugStyle =>
                $"position:absolute; left:{DummyX}px; top:{DummyY}px; " +
                $"width:{DummyWidth}px; height:{DummyHeight}px; " +
                $"background-color:rgba(255,255,0,0.2); border:1px dashed yellow; z-index:998;";

            public string DummyCollisionDebugStyle =>
                $"position:absolute; left:{DummyCollisionBox.X}px; top:{DummyCollisionBox.Y}px; " +
                $"width:{DummyCollisionBox.Width}px; height:{DummyCollisionBox.Height}px; " +
                $"background-color:rgba(0,255,255,0.3); border:2px solid cyan; z-index:999;";







        }

        public static class DummyRegistry
        {
            public static List<Dummy> All = new();

            public static void SpawnDummys(int count)
            {
                var rand = new Random();

                for (int i = 0; i < count; i++)
                {
                    All.Add(new Dummy
                    {
                        DummyX = rand.Next(25, 400),
                        DummyY = rand.Next(25, 400),
                    });
                }
            }
        }


















    }
}
