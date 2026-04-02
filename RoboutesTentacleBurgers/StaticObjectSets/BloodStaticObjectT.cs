

// 🪵 Static Object Definition Blood Town Map Spawn -1,0 BloodStaticListT
using RoboutesTentacleBurgers.Breakables;
using RoboutesTentacleBurgers.DynamicObjects;
using RoboutesTentacleBurgers.Physics;
using RoboutesTentacleBurgers.StaticObjectSets;
using static BloodStaticObject;

public class BloodStaticObjectT
{


    public class BloodStaticListT
    {

        public string Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        private static Dictionary<string, (int w, int h)> DefaultSizeMap = new()
    {
         { "Tree", (164, 188) },
        { "Rock", (74, 74) },
        { "FenceBroken", (100, 64) },
        { "TorchNew01", (48, 64) },
        { "Chest", (48, 48) },
        { "GStone", (36, 48) },
        { "GStoneCross", (36, 48) },
        { "Mushroom", (36, 36) },
        { "Skelcorpse001", (112, 48) },
        { "Grass01", (32, 32) },
        { "SkullONStick", (48, 64) },
        { "Urn", (36, 36) },
           { "Rose01", (16, 16) },
              { "Barrel01", (64, 64) },
                 { "Bush01", (88, 88) }






    };
        public static Dictionary<string, int> TownDictionary() => new()
{
    { "Tree", 35},
    { "Rock", 25 },
    { "FenceBroken", 20 },
    { "TorchNew01", 5 },
    { "Chest", 2 },
    { "GStone", 5 },
    { "GStoneCross", 5 },
    { "Mushroom", 25 },
    { "Skelcorpse001", 0 },
    { "Grass01", 250 },
    { "SkullONStick", 0 },
    { "Urn", 0 },
    { "Rose01", 60 },
    { "Barrel01", 15 },
    { "Bush01", 40 }
};


        public static Dictionary<string, (int shiftX, int shiftY, int w, int h)> CollisionMap = new()
    {
        { "Tree", (72, 156, 24, 24) },
        { "Rock", (12, 12, 48, 48) },
        { "FenceBroken", (10, 20, 64, 20) },
        { "TorchNew01", (0, 0, 24, 24) },
        { "Chest", (0, 0, 24, 24) },
        { "GStone", (0, 0, 24, 24) },
        { "GStoneCross", (0, 0, 24, 24) },
        { "Mushroom", (0, 0, 24, 24) },
        { "Skelcorpse001", (48, 0, 24, 24) },
        { "Grass01", (0, 0, 32, 32) },
        { "SkullONStick", (0, 0, 24, 24) },
        { "Urn", (0, 0, 24, 24) },
          { "Rose01", (0, 0,16,16) },
              { "Barrel01", (0, 0, 64, 64) },
                 { "Bush01", (0, 0, 88, 88) }

    };

        public int ZIndex => Type switch
        {
            "Tree" => 4100,
            "Rock" => 4000,
            "FenceBroken" => 3900,
            "TorchNew01" => 3800,
            "Chest" => 3700,
            "GStone" => 3600,
            "GStoneCross" => 3500,
            "Mushroom" => 3500,
            "Skelcorpse001" => 3500,
            "Grass01" => 3500,
            "SkullONStick" => 3500,
            "Urn" => 3500,
            "Rose01" => 3500,
            "Barrel01" => 3500,
            "Bush01" => 3500,
            _ => 5000
        };

        public BloodStaticListT(string type, int x, int y)
        {
            Type = type;
            X = x;
            Y = y;

            if (DefaultSizeMap.TryGetValue(type, out var size))
            {
                Width = size.w;
                Height = size.h;
            }
            else
            {
                Width = 48;
                Height = 48;
            }
        }

        public string SpriteStyle =>
            $"position:absolute; left:{X}px; top:{Y}px; " +
            $"width:{Width}px; height:{Height}px; " +
            $"background-image:url('/iAssets/{Type}.png'); background-size:cover; " +
            $"background-color:transparent; " +
            $"user-select:none; touch-action:manipulation; z-index:{ZIndex};";

        public string DebugSpriteStyle =>
            $"position:absolute; left:{X}px; top:{Y}px; " +
            $"width:{Width}px; height:{Height}px; " +
            $"background-color:rgba(255,255,0,0.2); border:1px dashed yellow; z-index:{ZIndex + 900};";

        public string DebugCollisionStyle =>
            $"position:absolute; left:{CollisionBox.X}px; top:{CollisionBox.Y}px; " +
            $"width:{CollisionBox.Width}px; height:{CollisionBox.Height}px; " +
            $"background-color:rgba(255,100,0,0.3); border:2px solid orange; z-index:998;";

        public Rectangle CollisionBox =>
            CollisionMap.TryGetValue(Type, out var c)
                ? new Rectangle(X + c.shiftX, Y + c.shiftY, c.w, c.h)
                : new Rectangle(X, Y, Width, Height);
    }

    public static class BloodStaticRegistryT
    {
        public static readonly List<BloodStaticListT> All = new();

        public static void SpawnByType(
            Dictionary<string, int> typeCounts,
            int mapWidth,
            int mapHeight,
            int xDown,
            int xRight,
            int yDown,
            int yRight,
            List<IiEnemy> Enemys,
            List<IDynamicO> dynO,
            List<IBreakables> breakables,
            List<IPhysics> physics,
            List<IStatic> statics



            )
        {
            var rand = new Random();
            const int maxAttempts = 150;

            int minX = xDown;
            int maxX = mapWidth - xRight;

            int minY = yDown;
            int maxY = mapHeight - yRight;

            foreach (var kvp in typeCounts)
            {
                string type = kvp.Key;
                int count = kvp.Value;

                for (int i = 0; i < count; i++)
                {
                    int attempts = 0;
                    BloodStaticListT newObj;


                    do
                    {
                        int x = rand.Next(minX, maxX);
                        int y = rand.Next(minY, maxY);
                        newObj = new BloodStaticListT(type, x, y);
                        attempts++;
                        if (attempts >= maxAttempts) break;  // <-- one‑line fix
                    } while (IsOverlapping(newObj, Enemys, dynO, breakables, physics, statics) && attempts < maxAttempts);

                    if (!IsOverlapping(newObj, Enemys, dynO, breakables, physics, statics))
                    {
                        All.Add(newObj);
                    }
                }
            }
        }

        private static bool IsOverlapping(BloodStaticListT candidate, List<IiEnemy> Enemys, List<IDynamicO> dynO,
            List<IBreakables> breakables,
            List<IPhysics> physics, List<IStatic> statics)

        {
            //add calls to other object spawns.

            foreach (var existing in All)
            {
                if (candidate.CollisionBox.IntersectsWith(existing.CollisionBox))
                    return true;
            }



            foreach (var enemy in Enemys)
            {
                if (candidate.CollisionBox.IntersectsWith(enemy.EnemyCollisionBox))
                {
                    return true;
                    // Trigger interaction
                }
            }

            foreach (var idynO in dynO)
            {
                if (candidate.CollisionBox.IntersectsWith(idynO.DynCollisionBox))
                {
                    return true;
                    // Trigger interaction
                }
            }
            foreach (var breakable in breakables)
            {
                if (candidate.CollisionBox.IntersectsWith(breakable.BreakCollisionBox))
                {
                    return true;
                    // Trigger interaction
                }
            }

            foreach (var iphysics in physics)
            {
                if (candidate.CollisionBox.IntersectsWith(iphysics.PhysCollisionBox))
                {
                    return true;
                    // Trigger interaction
                }
            }

            foreach (var istatics in statics)
            {
                if (candidate.CollisionBox.IntersectsWith(istatics.StaticCollisionBox))
                {
                    return true;
                    // Trigger interaction
                }
            }



            return false;
        }



    }





    public class Tavern001 : IStatic
    {
        public int StaticX { get => TavernX; set => TavernX = value; }
        public int StaticY { get => TavernY; set => TavernY = value; }
        public int StaticWidth { get => TavernWidth; set => TavernWidth = value; }
        public int StaticHeight { get => TavernHeight; set => TavernHeight = value; }
        public Rectangle StaticCollisionBox => TavernCollisionBox;
        public string StaticImagePath => TavernSpriteStyle;
        public string StaticDebugImagePath => TavernSpriteDebugStyle;

        // 🔥 Tavern of the Rising Sun
        public int TavernX { get; set; }
        public int TavernY { get; set; }
        public int TavernWidth { get; set; } = 630;
        public int TavernHeight { get; set; } = 402;

        private const int TavernCollisionWidth = 630;
        private const int TavernCollisionHeight = 402;
        private const int TavernCollisionShiftX = 0;
        private const int TavernCollisionShiftY = 0;

        public Rectangle TavernCollisionBox =>
          new Rectangle(
              TavernX + TavernCollisionShiftX,
              TavernY + TavernCollisionShiftY,
              TavernCollisionWidth,
              TavernCollisionHeight
          );

        public string TavernSpriteStyle =>
          $"position:absolute; left:{TavernX}px; top:{TavernY}px; " +
          $"width:{TavernWidth}px; height:{TavernHeight}px; " +
          $"background-image:url('/iAssets/Tavern001.png'); background-size:cover; " +
          $"background-color:transparent; z-index:98;";

        public string TavernSpriteDebugStyle =>
          $"position:absolute; left:{TavernX}px; top:{TavernY}px; " +
          $"width:{TavernWidth}px; height:{TavernHeight}px; " +
          $"background-color:rgba(255,0,0,0.2); border:1px dashed red; z-index:4050;";
    }

    public class Tavern001Registry
    {
        public static List<Tavern001> All = new();

        public static void SpawnTaverns(int count)
        {
            var rand = new Random();
            const int maxAttempts = 150;

            for (int i = 0; i < count; i++)
            {
                int attempts = 0;
                Tavern001 newTavern;

                do
                {
                    int x = rand.Next(384, 1684);
                    int y = rand.Next(384, 1684);
                    newTavern = new Tavern001 { TavernX = x, TavernY = y };
                    attempts++;
                    if (attempts >= maxAttempts) break;
                } while (IsTavernOverlapping(newTavern) && attempts < maxAttempts);

                if (!IsTavernOverlapping(newTavern))
                {
                    All.Add(newTavern);
                }
            }
        }

        private static bool IsTavernOverlapping(Tavern001 candidate)
        {
            // Check against other taverns
            foreach (var existing in All)
            {
                if (candidate.TavernCollisionBox.IntersectsWith(existing.TavernCollisionBox))
                    return true;
            }

            // Check against houses
            foreach (var house in HouseRegistry.All)
            {
                if (candidate.TavernCollisionBox.IntersectsWith(house.HouseCollisionBox))
                    return true;
            }

            // Check against fountains
            foreach (var fountain in Fountain001Registry.All)
            {
                if (candidate.TavernCollisionBox.IntersectsWith(fountain.FountainCollisionBox))
                    return true;
            }

            // Check against shrines
            foreach (var shrine in ShrineRegistry.All)
            {
                if (candidate.TavernCollisionBox.IntersectsWith(shrine.ShrineCollisionBox))
                    return true;
            }

            return false;
        }
    }

    public class House : IStatic
    {

        public int StaticX { get => HouseX; set => HouseX = value; }
        public int StaticY { get => HouseY; set => HouseY = value; }

        public int StaticWidth { get => HouseWidth; set => HouseWidth = value; }

        public int StaticHeight { get => HouseHeight; set => HouseHeight = value; }

        public Rectangle StaticCollisionBox => HouseCollisionBox;

        public string StaticImagePath => HouseSpriteStyle;

        public string StaticDebugImagePath => HouseSpriteDebugStyle;






        // 🏠 Tristram House
        public int HouseX { get; set; }
        public int HouseY { get; set; }
        public int HouseWidth { get; set; } = 440;
        public int HouseHeight { get; set; } = 335;

        private const int HouseCollisionWidth = 440;
        private const int HouseCollisionHeight = 335;
        private const int HouseCollisionShiftX = 0;
        private const int HouseCollisionShiftY = 0;

        public Rectangle HouseCollisionBox =>
          new Rectangle(
              HouseX + HouseCollisionShiftX,
              HouseY + HouseCollisionShiftY,
              HouseCollisionWidth,
              HouseCollisionHeight
          );

        public string HouseSpriteStyle =>
          $"position:absolute; left:{HouseX}px; top:{HouseY}px; " +
          $"width:{HouseWidth}px; height:{HouseHeight}px; " +
          $"background-image:url('/iAssets/House001.png'); background-size:cover; " +
          $"background-color:transparent; z-index:97;";


        public string HouseSpriteDebugStyle =>
     $"position:absolute; left:{HouseX}px; top:{HouseY}px; " +
     $"width:{HouseWidth}px; height:{HouseHeight}px; " +
     $"background-color:rgba(0,128,255,0.2); border:1px dashed blue; z-index:4050;";




    }

    public class HouseRegistry
    {
        public static List<House> All = new();

        public static void SpawnHousess(int count)
        {
            var rand = new Random();
            const int maxAttempts = 150;

            for (int i = 0; i < count; i++)
            {
                int attempts = 0;
                House newHouse;

                do
                {
                    int x = rand.Next(384, 1684);
                    int y = rand.Next(384, 1684);
                    newHouse = new House { HouseX = x, HouseY = y };
                    attempts++;
                    if (attempts >= maxAttempts) break;
                } while (IsHouseOverlapping(newHouse) && attempts < maxAttempts);

                if (!IsHouseOverlapping(newHouse))
                {
                    All.Add(newHouse);
                }
            }
        }

        private static bool IsHouseOverlapping(House candidate)
        {
            foreach (var existing in All)
            {
                if (candidate.HouseCollisionBox.IntersectsWith(existing.HouseCollisionBox))
                    return true;
            }

            // Check against other building types
            foreach (var tavern in Tavern001Registry.All)
            {
                if (candidate.HouseCollisionBox.IntersectsWith(tavern.TavernCollisionBox))
                    return true;
            }

            foreach (var fountain in Fountain001Registry.All)
            {
                if (candidate.HouseCollisionBox.IntersectsWith(fountain.FountainCollisionBox))
                    return true;
            }

            foreach (var shrine in ShrineRegistry.All)
            {
                if (candidate.HouseCollisionBox.IntersectsWith(shrine.ShrineCollisionBox))
                    return true;
            }

            return false;
        }
    }
    public class Fountain001 : IStatic
    {
        public int StaticX { get => FountainX; set => FountainX = value; }
        public int StaticY { get => FountainY; set => FountainY = value; }
        public int StaticWidth { get => FountainWidth; set => FountainWidth = value; }
        public int StaticHeight { get => FountainHeight; set => FountainHeight = value; }
        public Rectangle StaticCollisionBox => FountainCollisionBox;
        public string StaticImagePath => FountainSpriteStyle;
        public string StaticDebugImagePath => FountainSpriteDebugStyle;

        // 💧 Tristram Fountain
        public int FountainX { get; set; } 
        public int FountainY { get; set; } 
        public int FountainWidth { get; set; } = 128;
        public int FountainHeight { get; set; } = 128;

        private const int FountainCollisionWidth = 128;
        private const int FountainCollisionHeight = 128;
        private const int FountainCollisionShiftX = 0;
        private const int FountainCollisionShiftY = 0;

        public Rectangle FountainCollisionBox =>
          new Rectangle(
              FountainX + FountainCollisionShiftX,
              FountainY + FountainCollisionShiftY,
              FountainCollisionWidth,
              FountainCollisionHeight
          );

        public string FountainSpriteStyle =>
          $"position:absolute; left:{FountainX}px; top:{FountainY}px; " +
          $"width:{FountainWidth}px; height:{FountainHeight}px; " +
          $"background-image:url('/iAssets/Fountain001.png'); background-size:cover; " +
          $"background-color:transparent; z-index:38;";

        public string FountainSpriteDebugStyle =>
          $"position:absolute; left:{FountainX}px; top:{FountainY}px; " +
          $"width:{FountainWidth}px; height:{FountainHeight}px; " +
          $"background-color:rgba(0,128,255,0.2); border:1px dashed blue; z-index:4050;";
    }

    public class Fountain001Registry
    {
        public static List<Fountain001> All = new();

        public static void SpawnFountains(int count)
        {
            var rand = new Random();
            const int maxAttempts = 150;

            for (int i = 0; i < count; i++)
            {
                int attempts = 0;
                Fountain001 newFountain;

                do
                {
                    int x = rand.Next(384, 1684);
                    int y = rand.Next(384, 1684);
                    newFountain = new Fountain001 { FountainX = x, FountainY = y };
                    attempts++;
                    if (attempts >= maxAttempts) break;
                } while (IsFountainOverlapping(newFountain) && attempts < maxAttempts);

                if (!IsFountainOverlapping(newFountain))
                {
                    All.Add(newFountain);
                }
            }
        }

        private static bool IsFountainOverlapping(Fountain001 candidate)
        {
            // Check against other fountains
            foreach (var existing in All)
            {
                if (candidate.FountainCollisionBox.IntersectsWith(existing.FountainCollisionBox))
                    return true;
            }

            // Check against houses
            foreach (var house in HouseRegistry.All)
            {
                if (candidate.FountainCollisionBox.IntersectsWith(house.HouseCollisionBox))
                    return true;
            }

            // Check against taverns
            foreach (var tavern in Tavern001Registry.All)
            {
                if (candidate.FountainCollisionBox.IntersectsWith(tavern.TavernCollisionBox))
                    return true;
            }

            // Check against shrines
            foreach (var shrine in ShrineRegistry.All)
            {
                if (candidate.FountainCollisionBox.IntersectsWith(shrine.ShrineCollisionBox))
                    return true;
            }

            return false;
        }
    }

    public class Shrine : IStatic
    {
        public int StaticX { get => ShrineX; set => ShrineX = value; }
        public int StaticY { get => ShrineY; set => ShrineY = value; }
        public int StaticWidth { get => ShrineWidth; set => ShrineWidth = value; }
        public int StaticHeight { get => ShrineHeight; set => ShrineHeight = value; }
        public Rectangle StaticCollisionBox => ShrineCollisionBox;
        public string StaticImagePath => ShrineSpriteStyle;
        public string StaticDebugImagePath => ShrineSpriteDebugStyle;

        // 🕯️ Shrine Marker
        public int ShrineX { get; set; } 
        public int ShrineY { get; set; } 
        public int ShrineWidth { get; set; } = 128;
        public int ShrineHeight { get; set; } = 128;

        private const int ShrineCollisionWidth = 128;
        private const int ShrineCollisionHeight = 128;
        private const int ShrineCollisionShiftX = 0;
        private const int ShrineCollisionShiftY = 0;

        public Rectangle ShrineCollisionBox =>
          new Rectangle(
              ShrineX + ShrineCollisionShiftX,
              ShrineY + ShrineCollisionShiftY,
              ShrineCollisionWidth,
              ShrineCollisionHeight
          );

        public string ShrineSpriteStyle =>
          $"position:absolute; left:{ShrineX}px; top:{ShrineY}px; " +
          $"width:{ShrineWidth}px; height:{ShrineHeight}px; " +
          $"background-image:url('/iAssets/Shrine001.png'); background-size:cover; " +
          $"background-color:transparent; z-index:95;";

        public string ShrineSpriteDebugStyle =>
          $"position:absolute; left:{ShrineX}px; top:{ShrineY}px; " +
          $"width:{ShrineWidth}px; height:{ShrineHeight}px; " +
          $"background-color:rgba(255,255,0,0.2); border:1px dashed gold; z-index:4050;";
    }

    // ShrineRegistry with overlap checking
    public class ShrineRegistry
    {
        public static List<Shrine> All = new();

        public static void SpawnShrines(int count)
        {
            var rand = new Random();
            const int maxAttempts = 150;

            for (int i = 0; i < count; i++)
            {
                int attempts = 0;
                Shrine newShrine;

                do
                {
                    int x = rand.Next(384, 1684);
                    int y = rand.Next(384, 1684);
                    newShrine = new Shrine { ShrineX = x, ShrineY = y };
                    attempts++;
                    if (attempts >= maxAttempts) break;
                } while (IsShrineOverlapping(newShrine) && attempts < maxAttempts);

                if (!IsShrineOverlapping(newShrine))
                {
                    All.Add(newShrine);
                }
            }
        }

        private static bool IsShrineOverlapping(Shrine candidate)
        {
            // Check against other shrines
            foreach (var existing in All)
            {
                if (candidate.ShrineCollisionBox.IntersectsWith(existing.ShrineCollisionBox))
                    return true;
            }

            // Check against houses
            foreach (var house in HouseRegistry.All)
            {
                if (candidate.ShrineCollisionBox.IntersectsWith(house.HouseCollisionBox))
                    return true;
            }

            // Check against taverns
            foreach (var tavern in Tavern001Registry.All)
            {
                if (candidate.ShrineCollisionBox.IntersectsWith(tavern.TavernCollisionBox))
                    return true;
            }

            // Check against fountains
            foreach (var fountain in Fountain001Registry.All)
            {
                if (candidate.ShrineCollisionBox.IntersectsWith(fountain.FountainCollisionBox))
                    return true;
            }

            return false;
        }
    }







}

