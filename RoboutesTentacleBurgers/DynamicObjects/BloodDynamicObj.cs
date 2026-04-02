using RoboutesTentacleBurgers.DynamicObjects;

public class BloodDynamicObj
{
    
    public class CampFire : IDynamicO
    {
        public BloodCharacterHandle CharacterHandle { get; set; } = new();
        // Safe proxy, allows null checks
        public IBloodiCharacter? ActiveCharacter => CharacterHandle.ActiveCharacter;
        //public IBloodiCharacter ActiveCharacter { get; set; }= default!;

        public int DynX {get => X;set => X = value;}
        public int DynY { get => Y; set => Y = value; }
        public int DynWidth { get => DynWidth; set => DynWidth = value; }
        public int DynHeight { get => DynHeight; set => DynHeight = value; }

        // Collision box
        public Rectangle DynCollisionBox => CollisionBox;

        // Update hook (e.g. flicker, animation, AI)
        public void DynTickUpdate(IBloodiCharacter character) => TickUpdate();
        public bool DynIsActive => CampfireIsActive;
        // Style string for rendering
        public string DynSpriteStyle => SpriteStyle;

        // Debug style (optional)
        public string DynDebugStyle => DebugStyle;

        public string DynDebugCollisionStyle => CampfireCollisionDebugStyle;

        // Core

        public int X { get; set; } = 128;
        public int Y { get; set; } = 80;
        public int Width { get; set; } = 48;
        public int Height { get; set; } = 48;
        public bool CampfireIsActive { get; set; } = true;
        private const int CollisionWidth = 24;
        private const int CollisionHeight = 24;
        private const int CollisionShiftX = 12;
        private const int CollisionShiftY = 12;

        public Rectangle CollisionBox =>
            new Rectangle(
                X + CollisionShiftX,
                Y + CollisionShiftY,
                CollisionWidth,
                CollisionHeight
            );

        public string CampfireGlowColor { get; set; } = "orange";
        public string CampfireCoreColor { get; set; } = "orangered";


        // 🔥 Flicker update hook
        public void TickUpdate()
        {
            int flicker = BloodCampFireRegistry.FlickerRand.Next(0, 3);

            CampfireGlowColor = flicker switch
            {
                0 => "orange",
                1 => "gold",
                2 => "darkorange",
                _ => "orange"
            };

            CampfireCoreColor = flicker switch
            {
                0 => "orangered",
                1 => "crimson",
                2 => "tomato",
                _ => "orangered"
            };
        }


        public string SpriteStyle =>
            $"position:absolute; left:{X}px; top:{Y}px; " +
            $"width:{Width}px; height:{Height}px; " +
            $"background-image:url('/iAssets/Campfire0003.png'); background-size:cover; " +
            $"background-color:transparent; " +
            $"filter: drop-shadow(0 0 6px {CampfireGlowColor}) drop-shadow(0 0 12px {CampfireCoreColor}); " +
            $"opacity:0.95; user-select:none; touch-action:manipulation; z-index:2500;";

        public string DebugStyle =>
            $"position:absolute; left:{CollisionBox.X}px; top:{CollisionBox.Y}px; " +
            $"width:{CollisionBox.Width}px; height:{CollisionBox.Height}px; " +
            $"background-color:rgba(255,100,0,0.3); border:2px solid orange; z-index:998;";


        public string CampfireCollisionDebugStyle =>
           $"position:absolute; left:{CollisionBox.X}px; top:{CollisionBox.Y}px; " +
           $"width:{CollisionBox.Width}px; height:{CollisionBox.Height}px; " +
           $"background-color:rgba(255,0,0,0.2); border:1px dashed red; z-index:998;";




    }


    public class BloodCampFireRegistry
    {
        public static List<CampFire> All = new();

        public static void SpawnCampFire(int count)
        {
            var rand = new Random();
            for (int i = 0; i < count; i++)
            {
                All.Add(new CampFire
                {
                    X = rand.Next(50, 200),
                    Y = rand.Next(50, 200),
                });
            }
        }

        public static readonly Random FlickerRand = new Random();
        private static DateTime lastFlickerUpdate = DateTime.Now;

        public static void UpdateCampfireFlicker()
        {
            if ((DateTime.Now - lastFlickerUpdate).TotalMilliseconds > 100)
            {
                foreach (var campfire in All)
                {
                    campfire.TickUpdate();
                }
                lastFlickerUpdate = DateTime.Now;
            }
        }
    }
    public class Cheese : IDynamicO
    {
        public BloodCharacterHandle CharacterHandle { get; set; } = new();
        public IBloodiCharacter? ActiveCharacter => CharacterHandle.ActiveCharacter;

        public int DynX { get => CheeseX; set => CheeseX = value; }
        public int DynY { get => CheeseY; set => CheeseY = value; }
        public int DynWidth { get => CheeseWidth; set => CheeseWidth = value; }
        public int DynHeight { get => CheeseHeight; set => CheeseHeight = value; }
        public Rectangle DynCollisionBox => CheeseCollisionBox;

        public void DynTickUpdate(IBloodiCharacter character) => TickCheeses(character);

        public bool DynIsActive => CheeseIsActive;

        public string DynSpriteStyle => CheeseSpriteStyle;
        public string DynDebugStyle => CheeseDebugStyle;
        public string DynDebugCollisionStyle => CheeseCollisionDebugStyle;

        public int CheeseX { get; set; }
        public int CheeseY { get; set; }
        public int CheeseWidth { get; set; } = 16;
        public int CheeseHeight { get; set; } = 16;
        public bool CheeseIsActive { get; set; } = true;

        public Rectangle CheeseCollisionBox =>
            new Rectangle(
                CheeseX + CheeseCollisionShiftX,
                CheeseY + CheeseCollisionShiftY,
                CheeseCollisionWidth,
                CheeseCollisionHeight
            );

        private const int CheeseCollisionWidth = 16;
        private const int CheeseCollisionHeight = 16;
        private const int CheeseCollisionShiftX = 0;
        private const int CheeseCollisionShiftY = 0;

        public static void TickCheeses(IBloodiCharacter character)
        {
            foreach (var cheese in BloodCheeseRegistry.All)
            {
                if (!cheese.CheeseIsActive || character == null || !character.CharIsAlive)
                    continue;

                if (cheese.CheeseCollisionBox.IntersectsWith(character.CharCollisionBox))
                {
                    character.CharHungerCurrent =
                        Math.Min(character.CharHungerCurrent + 50, character.CharHungerFull);

                    cheese.CheeseIsActive = false;
                }
            }
        }


        public string CheeseSpriteStyle =>
            $"position:absolute; left:{CheeseX}px; top:{CheeseY}px; " +
            $"width:{CheeseWidth}px; height:{CheeseHeight}px; " +
            $"background-image:url('/iAssets/CHEEESE002.png'); " +
            $"background-repeat:no-repeat; background-size:cover; z-index:2500;";

        public string CheeseDebugStyle =>
            $"position:absolute; left:{CheeseX}px; top:{CheeseY}px; " +
            $"width:{CheeseWidth}px; height:{CheeseHeight}px; " +
            $"background-color:rgba(255,0,0,0.2); border:1px dashed green; z-index:998;";

        public string CheeseCollisionDebugStyle =>
            $"position:absolute; left:{CheeseCollisionBox.X}px; top:{CheeseCollisionBox.Y}px; " +
            $"width:{CheeseCollisionBox.Width}px; height:{CheeseCollisionBox.Height}px; " +
            $"background-color:rgba(255,0,0,0.2); border:1px dashed red; z-index:998;";
    }

    public static class BloodCheeseRegistry
    {
        public static readonly List<Cheese> All = new();

        public static void SpawnCheeses(int count)
        {
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                All.Add(new Cheese
                {
                    CheeseX = rand.Next(50, 1994),
                    CheeseY = rand.Next(50, 1994),
                });
            }
        }
    }


    public class HealPot : IDynamicO
    {
        public BloodCharacterHandle CharacterHandle { get; set; } = new();
        public IBloodiCharacter? ActiveCharacter => CharacterHandle.ActiveCharacter;

        public int DynX { get => HealPotX; set => HealPotX = value; }
        public int DynY { get => HealPotY; set => HealPotY = value; }
        public int DynWidth { get => HealPotWidth; set => HealPotWidth = value; }
        public int DynHeight { get => HealPotHeight; set => HealPotHeight = value; }
        public Rectangle DynCollisionBox => HealPotCollisionBox;
      
        public void DynTickUpdate(IBloodiCharacter character) => TickHealPots(character);

        public bool DynIsActive => HealPotIsActive;

        public string DynSpriteStyle => HealPotSpriteStyle;
        public string DynDebugStyle => HealPotDebugStyle;
        public string DynDebugCollisionStyle => HealPotCollisionDebugStyle;
        public int HealPotX { get; set; }
        public int HealPotY { get; set; }
        public int HealPotWidth { get; set; } = 32;
        public int HealPotHeight { get; set; } = 32;
        public bool HealPotIsActive { get; set; } = true;

        public Rectangle HealPotCollisionBox =>
            new Rectangle(
                HealPotX + HealPotCollisionShiftX,
                HealPotY + HealPotCollisionShiftY,
                HealPotCollisionWidth,
                HealPotCollisionHeight
            );

        private const int HealPotCollisionWidth = 24;
        private const int HealPotCollisionHeight = 24;
        private const int HealPotCollisionShiftX = 0;
        private const int HealPotCollisionShiftY = 0;

        public static void TickHealPots(IBloodiCharacter character)
        {
            foreach (var healPot in BloodHealPotRegistry.All)
            {
                if (!healPot.HealPotIsActive || character == null || !character.CharIsAlive)
                    continue;

                if (healPot.HealPotCollisionBox.IntersectsWith(character.CharCollisionBox))
                {
                    character.CharHitPoints = Math.Min(character.CharHitPoints + 10, character.CharMaxHP);
                    healPot.HealPotIsActive = false;
                }
            }
        }

        public string HealPotSpriteStyle =>
            $"position:absolute; left:{HealPotX}px; top:{HealPotY}px; " +
            $"width:{HealPotWidth}px; height:{HealPotHeight}px; " +
            $"background-image:url('/iAssets/SmallPot.png'); " +
            $"background-repeat:no-repeat; background-size:cover; z-index:2500;";

        public string HealPotDebugStyle =>
            $"position:absolute; left:{HealPotX}px; top:{HealPotY}px; " +
            $"width:{HealPotWidth}px; height:{HealPotHeight}px; " +
            $"background-color:rgba(255,0,0,0.2); border:1px dashed green; z-index:998;";

        public string HealPotCollisionDebugStyle =>
            $"position:absolute; left:{HealPotCollisionBox.X}px; top:{HealPotCollisionBox.Y}px; " +
            $"width:{HealPotCollisionBox.Width}px; height:{HealPotCollisionBox.Height}px; " +
            $"background-color:rgba(255,0,0,0.2); border:1px dashed red; z-index:998;";




    }



public static class BloodHealPotRegistry
    {
        public static readonly List<HealPot> All = new();

        public static void SpawnHealPots(int count)
        {
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                All.Add(new HealPot
                {
                    HealPotX = rand.Next(50, 1994),
                    HealPotY = rand.Next(50, 1994),
                });
            }
        }
    }
    public class MedHealPot : IDynamicO
    {
        public BloodCharacterHandle CharacterHandle { get; set; } = new();
        public IBloodiCharacter? ActiveCharacter => CharacterHandle.ActiveCharacter;

        public int DynX { get => MedHealPotX; set => MedHealPotX = value; }
        public int DynY { get => MedHealPotY; set => MedHealPotY = value; }
        public int DynWidth { get => MedHealPotWidth; set => MedHealPotWidth = value; }
        public int DynHeight { get => MedHealPotHeight; set => MedHealPotHeight = value; }
        public Rectangle DynCollisionBox => MedHealPotCollisionBox;

        public void DynTickUpdate(IBloodiCharacter character) => TickMedHealPots(character);

        public bool DynIsActive => MedHealPotIsActive;

        public string DynSpriteStyle => MedHealPotSpriteStyle;
        public string DynDebugStyle => MedHealPotDebugStyle;
        public string DynDebugCollisionStyle => MedHealPotCollisionDebugStyle;

        public int MedHealPotX { get; set; }
        public int MedHealPotY { get; set; }
        public int MedHealPotWidth { get; set; } = 48;
        public int MedHealPotHeight { get; set; } = 48;
        public bool MedHealPotIsActive { get; set; } = true;

        public Rectangle MedHealPotCollisionBox =>
            new Rectangle(
                MedHealPotX + MedHealPotCollisionShiftX,
                MedHealPotY + MedHealPotCollisionShiftY,
                MedHealPotCollisionWidth,
                MedHealPotCollisionHeight
            );

        private const int MedHealPotCollisionWidth = 24;
        private const int MedHealPotCollisionHeight = 24;
        private const int MedHealPotCollisionShiftX = 12;
        private const int MedHealPotCollisionShiftY = 12;

        public static void TickMedHealPots(IBloodiCharacter character)
        {
            foreach (var medHealPot in BloodMedHealPotRegistry.All)
            {
                if (!medHealPot.MedHealPotIsActive || character == null || !character.CharIsAlive)
                    continue;

                if (medHealPot.MedHealPotCollisionBox.IntersectsWith(character.CharCollisionBox))
                {
                    character.CharHitPoints =
                        Math.Min(character.CharHitPoints + 25, character.CharMaxHP);

                    medHealPot.MedHealPotIsActive = false;
                }
            }
        }

       

        public string MedHealPotSpriteStyle =>
            $"position:absolute; left:{MedHealPotX}px; top:{MedHealPotY}px; " +
            $"width:{MedHealPotWidth}px; height:{MedHealPotHeight}px; " +
            $"background-image:url('/iAssets/healpot01.png'); " +
            $"background-repeat:no-repeat; background-size:cover; z-index:2500;";

        public string MedHealPotDebugStyle =>
            $"position:absolute; left:{MedHealPotX}px; top:{MedHealPotY}px; " +
            $"width:{MedHealPotWidth}px; height:{MedHealPotHeight}px; " +
            $"background-color:rgba(255,0,0,0.2); border:1px dashed green; z-index:998;";

        public string MedHealPotCollisionDebugStyle =>
            $"position:absolute; left:{MedHealPotCollisionBox.X}px; top:{MedHealPotCollisionBox.Y}px; " +
            $"width:{MedHealPotCollisionBox.Width}px; height:{MedHealPotCollisionBox.Height}px; " +
            $"background-color:rgba(255,0,0,0.2); border:1px dashed red; z-index:998;";
    }

    public static class BloodMedHealPotRegistry
    {
        public static readonly List<MedHealPot> All = new();

        public static void SpawnMedHealPots(int count)
        {
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                All.Add(new MedHealPot
                {
                    MedHealPotX = rand.Next(50, 1994),
                    MedHealPotY = rand.Next(50, 1994),
                });
            }
        }
    }
    public class ManaPot : IDynamicO
    {
        public BloodCharacterHandle CharacterHandle { get; set; } = new();
        public IBloodiCharacter? ActiveCharacter => CharacterHandle.ActiveCharacter;

        public int DynX { get => ManaPotX; set => ManaPotX = value; }
        public int DynY { get => ManaPotY; set => ManaPotY = value; }
        public int DynWidth { get => ManaPotWidth; set => ManaPotWidth = value; }
        public int DynHeight { get => ManaPotHeight; set => ManaPotHeight = value; }
        public Rectangle DynCollisionBox => ManaPotCollisionBox;

        public void DynTickUpdate(IBloodiCharacter character) => TickManaPots(character);

        public bool DynIsActive => ManaPotIsActive;

        public string DynSpriteStyle => ManaPotSpriteStyle;
        public string DynDebugStyle => ManaPotDebugStyle;
        public string DynDebugCollisionStyle => ManaPotCollisionDebugStyle;

        public int ManaPotX { get; set; }
        public int ManaPotY { get; set; }
        public int ManaPotWidth { get; set; } = 32;
        public int ManaPotHeight { get; set; } = 32;
        public bool ManaPotIsActive { get; set; } = true;

        public Rectangle ManaPotCollisionBox =>
            new Rectangle(
                ManaPotX + ManaPotCollisionShiftX,
                ManaPotY + ManaPotCollisionShiftY,
                ManaPotCollisionWidth,
                ManaPotCollisionHeight
            );

        private const int ManaPotCollisionWidth = 24;
        private const int ManaPotCollisionHeight = 24;
        private const int ManaPotCollisionShiftX = 0;
        private const int ManaPotCollisionShiftY = 0;

        public static void TickManaPots(IBloodiCharacter character)
        {
            foreach (var manaPot in BloodManaPotRegistry.All)
            {
                if (!manaPot.ManaPotIsActive || character == null || !character.CharIsAlive)
                    continue;

                if (manaPot.ManaPotCollisionBox.IntersectsWith(character.CharCollisionBox))
                {
                    character.CharResourceValue =
                        Math.Min(character.CharResourceValue + 5, character.CharMaxResourceValue);

                    manaPot.ManaPotIsActive = false;
                }
            }
        }

     

        public string ManaPotSpriteStyle =>
            $"position:absolute; left:{ManaPotX}px; top:{ManaPotY}px; " +
            $"width:{ManaPotWidth}px; height:{ManaPotHeight}px; " +
            $"background-image:url('/iAssets/ManaPot.png'); " +
            $"background-repeat:no-repeat; background-size:cover; z-index:2500;";

        public string ManaPotDebugStyle =>
            $"position:absolute; left:{ManaPotX}px; top:{ManaPotY}px; " +
            $"width:{ManaPotWidth}px; height:{ManaPotHeight}px; " +
            $"background-color:rgba(0,0,255,0.2); border:1px dashed blue; z-index:998;";

        public string ManaPotCollisionDebugStyle =>
            $"position:absolute; left:{ManaPotCollisionBox.X}px; top:{ManaPotCollisionBox.Y}px; " +
            $"width:{ManaPotCollisionBox.Width}px; height:{ManaPotCollisionBox.Height}px; " +
            $"background-color:rgba(0,0,255,0.2); border:1px dashed navy; z-index:998;";
    }

    public static class BloodManaPotRegistry
    {
        public static readonly List<ManaPot> All = new();

        public static void SpawnManaPots(int count)
        {
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                All.Add(new ManaPot
                {
                    ManaPotX = rand.Next(50, 1994),
                    ManaPotY = rand.Next(50, 1994),
                });
            }
        }
    }
    public class StrPot : IDynamicO
    {
        public BloodCharacterHandle CharacterHandle { get; set; } = new();
        public IBloodiCharacter? ActiveCharacter => CharacterHandle.ActiveCharacter;

        public int DynX { get => StrPotX; set => StrPotX = value; }
        public int DynY { get => StrPotY; set => StrPotY = value; }
        public int DynWidth { get => StrPotWidth; set => StrPotWidth = value; }
        public int DynHeight { get => StrPotHeight; set => StrPotHeight = value; }
        public Rectangle DynCollisionBox => StrPotCollisionBox;

        public void DynTickUpdate(IBloodiCharacter character) => TickStrPots(character);

        public bool DynIsActive => StrPotIsActive;

        public string DynSpriteStyle => StrPotSpriteStyle;
        public string DynDebugStyle => StrPotDebugStyle;
        public string DynDebugCollisionStyle => StrPotCollisionDebugStyle;

        public int StrPotX { get; set; }
        public int StrPotY { get; set; }
        public int StrPotWidth { get; set; } = 32;
        public int StrPotHeight { get; set; } = 32;
        public bool StrPotIsActive { get; set; } = true;

        public Rectangle StrPotCollisionBox =>
            new Rectangle(
                StrPotX + StrPotCollisionShiftX,
                StrPotY + StrPotCollisionShiftY,
                StrPotCollisionWidth,
                StrPotCollisionHeight
            );

        private const int StrPotCollisionWidth = 24;
        private const int StrPotCollisionHeight = 24;
        private const int StrPotCollisionShiftX = 0;
        private const int StrPotCollisionShiftY = 0;

        public static void TickStrPots(IBloodiCharacter character)
        {
            foreach (var strPot in BloodStrPotRegistry.All)
            {
                if (!strPot.StrPotIsActive || character == null || !character.CharIsAlive)
                    continue;

                if (strPot.StrPotCollisionBox.IntersectsWith(character.CharCollisionBox))
                {
                    character.CharStrength += 5;
                    strPot.StrPotIsActive = false;

                    // Start timer to remove effect after 10 seconds
                    _ = RemoveStrengthAfterDelay(character);
                }
            }
        }

        private static async Task RemoveStrengthAfterDelay(IBloodiCharacter character)
        {
            await Task.Delay(10000);

            if (character.CharIsAlive)
            {
                character.CharStrength -= 5;
            }
        }


        public string StrPotSpriteStyle =>
            $"position:absolute; left:{StrPotX}px; top:{StrPotY}px; " +
            $"width:{StrPotWidth}px; height:{StrPotHeight}px; " +
            $"background-image:url('/iAssets/StrElixer.png'); " +
            $"background-repeat:no-repeat; background-size:cover; z-index:2500;";

        public string StrPotDebugStyle =>
            $"position:absolute; left:{StrPotX}px; top:{StrPotY}px; " +
            $"width:{StrPotWidth}px; height:{StrPotHeight}px; " +
            $"background-color:rgba(0,255,0,0.2); border:1px dashed green; z-index:998;";

        public string StrPotCollisionDebugStyle =>
            $"position:absolute; left:{StrPotCollisionBox.X}px; top:{StrPotCollisionBox.Y}px; " +
            $"width:{StrPotCollisionBox.Width}px; height:{StrPotCollisionBox.Height}px; " +
            $"background-color:rgba(0,255,0,0.2); border:1px dashed darkgreen; z-index:998;";
    }

    public static class BloodStrPotRegistry
    {
        public static readonly List<StrPot> All = new();

        public static void SpawnStrPots(int count)
        {
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                All.Add(new StrPot
                {
                    StrPotX = rand.Next(50, 1994),
                    StrPotY = rand.Next(50, 1994),
                });
            }
        }
    }
    public class CelPot : IDynamicO
    {
        public BloodCharacterHandle CharacterHandle { get; set; } = new();
        public IBloodiCharacter? ActiveCharacter => CharacterHandle.ActiveCharacter;

        public int DynX { get => CelPotX; set => CelPotX = value; }
        public int DynY { get => CelPotY; set => CelPotY = value; }
        public int DynWidth { get => CelPotWidth; set => CelPotWidth = value; }
        public int DynHeight { get => CelPotHeight; set => CelPotHeight = value; }
        public Rectangle DynCollisionBox => CelPotCollisionBox;

        public void DynTickUpdate(IBloodiCharacter character) => TickCelPots(character);

        public bool DynIsActive => CelPotIsActive;

        public string DynSpriteStyle => CelPotSpriteStyle;
        public string DynDebugStyle => CelPotDebugStyle;
        public string DynDebugCollisionStyle => CelPotCollisionDebugStyle;

        public int CelPotX { get; set; }
        public int CelPotY { get; set; }
        public int CelPotWidth { get; set; } = 32;
        public int CelPotHeight { get; set; } = 32;
        public bool CelPotIsActive { get; set; } = true;

        public Rectangle CelPotCollisionBox =>
            new Rectangle(
                CelPotX + CelPotCollisionShiftX,
                CelPotY + CelPotCollisionShiftY,
                CelPotCollisionWidth,
                CelPotCollisionHeight
            );

        private const int CelPotCollisionWidth = 24;
        private const int CelPotCollisionHeight = 24;
        private const int CelPotCollisionShiftX = 0;
        private const int CelPotCollisionShiftY = 0;

        public static void TickCelPots(IBloodiCharacter character)
        {
            foreach (var celPot in BloodCelPotRegistry.All)
            {
                if (!celPot.CelPotIsActive || character == null || !character.CharIsAlive)
                    continue;

                if (celPot.CelPotCollisionBox.IntersectsWith(character.CharCollisionBox))
                {
                    character.CharCelerity += 5;
                    celPot.CelPotIsActive = false;

                    // Start timer to remove effect after 10 seconds
                    _ = RemoveCelerityAfterDelay(character);
                }
            }
        }

        private static async Task RemoveCelerityAfterDelay(IBloodiCharacter character)
        {
            await Task.Delay(10000);

            if (character.CharIsAlive)
            {
                character.CharCelerity -= 5;
            }
        }

      

        public string CelPotSpriteStyle =>
            $"position:absolute; left:{CelPotX}px; top:{CelPotY}px; " +
            $"width:{CelPotWidth}px; height:{CelPotHeight}px; " +
            $"background-image:url('/iAssets/CelElixir.png'); " +
            $"background-repeat:no-repeat; background-size:cover; z-index:2500;";

        public string CelPotDebugStyle =>
            $"position:absolute; left:{CelPotX}px; top:{CelPotY}px; " +
            $"width:{CelPotWidth}px; height:{CelPotHeight}px; " +
            $"background-color:rgba(0,200,255,0.2); border:1px dashed cyan; z-index:998;";

        public string CelPotCollisionDebugStyle =>
            $"position:absolute; left:{CelPotCollisionBox.X}px; top:{CelPotCollisionBox.Y}px; " +
            $"width:{CelPotCollisionBox.Width}px; height:{CelPotCollisionBox.Height}px; " +
            $"background-color:rgba(0,200,255,0.2); border:1px dashed blue; z-index:998;";
    }

    public static class BloodCelPotRegistry
    {
        public static readonly List<CelPot> All = new();

        public static void SpawnCelPots(int count)
        {
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                All.Add(new CelPot
                {
                    CelPotX = rand.Next(50, 1994),
                    CelPotY = rand.Next(50, 1994),
                });
            }
        }
    }
    public class AlcPot : IDynamicO
    {
        public BloodCharacterHandle CharacterHandle { get; set; } = new();
        public IBloodiCharacter? ActiveCharacter => CharacterHandle.ActiveCharacter;

        public int DynX { get => AlcPotX; set => AlcPotX = value; }
        public int DynY { get => AlcPotY; set => AlcPotY = value; }
        public int DynWidth { get => AlcPotWidth; set => AlcPotWidth = value; }
        public int DynHeight { get => AlcPotHeight; set => AlcPotHeight = value; }
        public Rectangle DynCollisionBox => AlcPotCollisionBox;

        public void DynTickUpdate(IBloodiCharacter character) => TickAlcPots(character);

        public bool DynIsActive => AlcPotIsActive;

        public string DynSpriteStyle => AlcPotSpriteStyle;
        public string DynDebugStyle => AlcPotDebugStyle;
        public string DynDebugCollisionStyle => AlcPotCollisionDebugStyle;

        public int AlcPotX { get; set; }
        public int AlcPotY { get; set; }
        public int AlcPotWidth { get; set; } = 32;
        public int AlcPotHeight { get; set; } = 32;
        public bool AlcPotIsActive { get; set; } = true;

        public Rectangle AlcPotCollisionBox =>
            new Rectangle(
                AlcPotX + AlcPotCollisionShiftX,
                AlcPotY + AlcPotCollisionShiftY,
                AlcPotCollisionWidth,
                AlcPotCollisionHeight
            );

        private const int AlcPotCollisionWidth = 24;
        private const int AlcPotCollisionHeight = 24;
        private const int AlcPotCollisionShiftX = 0;
        private const int AlcPotCollisionShiftY = 0;

        public static void TickAlcPots(IBloodiCharacter character)
        {
            foreach (var alcPot in BloodAlcPotRegistry.All)
            {
                if (!alcPot.AlcPotIsActive || character == null || !character.CharIsAlive)
                    continue;

                if (alcPot.AlcPotCollisionBox.IntersectsWith(character.CharCollisionBox))
                {
                    character.CharAlacrity += 5;
                    alcPot.AlcPotIsActive = false;

                    // Start timer to remove effect after 10 seconds
                    _ = RemoveAlacrityAfterDelay(character);
                }
            }
        }

        private static async Task RemoveAlacrityAfterDelay(IBloodiCharacter character)
        {
            await Task.Delay(10000);

            if (character.CharIsAlive)
            {
                character.CharAlacrity -= 5;
            }
        }

       

        public string AlcPotSpriteStyle =>
            $"position:absolute; left:{AlcPotX}px; top:{AlcPotY}px; " +
            $"width:{AlcPotWidth}px; height:{AlcPotHeight}px; " +
            $"background-image:url('/iAssets/AlacrityElixir.png'); " +
            $"background-repeat:no-repeat; background-size:cover; z-index:2500;";

        public string AlcPotDebugStyle =>
            $"position:absolute; left:{AlcPotX}px; top:{AlcPotY}px; " +
            $"width:{AlcPotWidth}px; height:{AlcPotHeight}px; " +
            $"background-color:rgba(255,255,0,0.2); border:1px dashed orange; z-index:998;";

        public string AlcPotCollisionDebugStyle =>
            $"position:absolute; left:{AlcPotCollisionBox.X}px; top:{AlcPotCollisionBox.Y}px; " +
            $"width:{AlcPotCollisionBox.Width}px; height:{AlcPotCollisionBox.Height}px; " +
            $"background-color:rgba(255,255,0,0.2); border:1px dashed darkorange; z-index:998;";
    }

    public static class BloodAlcPotRegistry
    {
        public static readonly List<AlcPot> All = new();

        public static void SpawnAlcPots(int count)
        {
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                All.Add(new AlcPot
                {
                    AlcPotX = rand.Next(50, 1994),
                    AlcPotY = rand.Next(50, 1994),
                });
            }
        }
    }
    public class IntPot : IDynamicO
    {
        public BloodCharacterHandle CharacterHandle { get; set; } = new();
        public IBloodiCharacter? ActiveCharacter => CharacterHandle.ActiveCharacter;

        public int DynX { get => IntPotX; set => IntPotX = value; }
        public int DynY { get => IntPotY; set => IntPotY = value; }
        public int DynWidth { get => IntPotWidth; set => IntPotWidth = value; }
        public int DynHeight { get => IntPotHeight; set => IntPotHeight = value; }
        public Rectangle DynCollisionBox => IntPotCollisionBox;

        public void DynTickUpdate(IBloodiCharacter character) => TickIntPots(character);

        public bool DynIsActive => IntPotIsActive;

        public string DynSpriteStyle => IntPotSpriteStyle;
        public string DynDebugStyle => IntPotDebugStyle;
        public string DynDebugCollisionStyle => IntPotCollisionDebugStyle;

        public int IntPotX { get; set; }
        public int IntPotY { get; set; }
        public int IntPotWidth { get; set; } = 32;
        public int IntPotHeight { get; set; } = 32;
        public bool IntPotIsActive { get; set; } = true;

        public Rectangle IntPotCollisionBox =>
            new Rectangle(
                IntPotX + IntPotCollisionShiftX,
                IntPotY + IntPotCollisionShiftY,
                IntPotCollisionWidth,
                IntPotCollisionHeight
            );

        private const int IntPotCollisionWidth = 24;
        private const int IntPotCollisionHeight = 24;
        private const int IntPotCollisionShiftX = 0;
        private const int IntPotCollisionShiftY = 0;

        public static void TickIntPots(IBloodiCharacter character)
        {
            foreach (var intPot in BloodIntPotRegistry.All)
            {
                if (!intPot.IntPotIsActive || character == null || !character.CharIsAlive)
                    continue;

                if (intPot.IntPotCollisionBox.IntersectsWith(character.CharCollisionBox))
                {
                    character.CharIntelligence += 5;
                    intPot.IntPotIsActive = false;

                    // Start timer to remove effect after 10 seconds
                    _ = RemoveIntelligenceAfterDelay(character);
                }
            }
        }

        private static async Task RemoveIntelligenceAfterDelay(IBloodiCharacter character)
        {
            await Task.Delay(10000);

            if (character.CharIsAlive)
            {
                character.CharIntelligence -= 5;
            }
        }

        

        public string IntPotSpriteStyle =>
            $"position:absolute; left:{IntPotX}px; top:{IntPotY}px; " +
            $"width:{IntPotWidth}px; height:{IntPotHeight}px; " +
            $"background-image:url('/iAssets/IntElixir.png'); " +
            $"background-repeat:no-repeat; background-size:cover; z-index:2500;";

        public string IntPotDebugStyle =>
            $"position:absolute; left:{IntPotX}px; top:{IntPotY}px; " +
            $"width:{IntPotWidth}px; height:{IntPotHeight}px; " +
            $"background-color:rgba(200,0,255,0.2); border:1px dashed purple; z-index:998;";

        public string IntPotCollisionDebugStyle =>
            $"position:absolute; left:{IntPotCollisionBox.X}px; top:{IntPotCollisionBox.Y}px; " +
            $"width:{IntPotCollisionBox.Width}px; height:{IntPotCollisionBox.Height}px; " +
            $"background-color:rgba(200,0,255,0.2); border:1px dashed indigo; z-index:998;";
    }

    public static class BloodIntPotRegistry
    {
        public static readonly List<IntPot> All = new();

        public static void SpawnIntPots(int count)
        {
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                All.Add(new IntPot
                {
                    IntPotX = rand.Next(50, 1994),
                    IntPotY = rand.Next(50, 1994),
                });
            }
        }
    }



}
