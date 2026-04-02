

// 🪵 Static Object Definition Blood Town Map Spawn -1,0 BloodStaticListT
using RoboutesTentacleBurgers.Breakables;
using RoboutesTentacleBurgers.DynamicObjects;
using RoboutesTentacleBurgers.Physics;
using RoboutesTentacleBurgers.StaticObjectSets;
using static BloodStaticObject;
using static BloodStaticObjectT;

public class BloodStaticObjectS
{




    // 🪵 Static Object Definition Snow Forest Map Spawn 10,0
    public class BloodStaticListS
    {

        public string SType { get; set; }
        public int SX { get; set; }
        public int SY { get; set; }
        public int SWidth { get; set; }
        public int SHeight { get; set; }

        public static Dictionary<string, (int w, int h)> DefaultSizeMap = new()
{
    { "STree", (164 , 188) },
    { "SRock001", (74, 74) },
    { "Sbrokenfence01", (100, 64) },
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
    { "SBush01", (88, 88) }
};

        public static Dictionary<string, int> SnowForestDictionary() => new()
{
    { "STree", 50},
    { "SRock001", 25 },
    { "Sbrokenfence01", 15 },
    { "TorchNew01", 8 },
    { "Chest", 6 },
    { "GStone", 12 },
    { "GStoneCross", 12 },
    { "Mushroom", 20 },
    { "Skelcorpse001", 0 },
    { "Grass01", 25},
    { "SkullONStick", 0 },
    { "Urn", 12 },
    { "Rose01", 15 },
    { "Barrel01", 12 },
    { "SBush01", 25 }
};
        public static Dictionary<string, (int shiftX, int shiftY, int w, int h)> CollisionMap = new()
{
    { "STree", (72, 156, 24, 24) },
    { "SRock001", (12, 12, 48, 48) },
    { "Sbrokenfence01", (10, 20, 64, 20) },
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
    { "SBush01", (0, 0, 88, 88) }
};

        public int ZIndex => SType switch
        {
            "STree" => 4100,
            "SRock001" => 4000,
            "Sbrokenfence01" => 3900,
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
            "SBush01" => 3500,
            _ => 5000
        };

        public BloodStaticListS(string type, int x, int y)
        {
            SType = type;
            SX = x;
            SY = y;

            if (DefaultSizeMap.TryGetValue(type, out var size))
            {
                SWidth = size.w;
                SHeight = size.h;
            }
            else
            {
                SWidth = 48;
                SHeight = 48;
            }
        }

        public string SSpriteStyle =>
            $"position:absolute; left:{SX}px; top:{SY}px; " +
            $"width:{SWidth}px; height:{SHeight}px; " +
            $"background-image:url('/iAssets/{SType}.png'); background-size:cover; " +
            $"background-color:transparent; " +
            $"user-select:none; touch-action:manipulation; z-index:{ZIndex};";

        public string DebugSpriteStyle =>
            $"position:absolute; left:{SWidth} px; top: {SHeight}px; " +
            $"width:{SWidth} px; height: {SHeight}px; " +
            $"background-color:rgba(255,255,0,0.2); border:1px dashed yellow; z-index:{ZIndex + 900};";

        public string DebugCollisionStyle =>
            $"position:absolute; left:{CollisionBox.X}px; top:{CollisionBox.Y}px; " +
            $"width:{CollisionBox.Width}px; height:{CollisionBox.Height}px; " +
            $"background-color:rgba(255,100,0,0.3); border:2px solid orange; z-index:998;";

        public Rectangle CollisionBox =>
            CollisionMap.TryGetValue(SType, out var c)
                ? new Rectangle(SX + c.shiftX, SY + c.shiftY, c.w, c.h)
                : new Rectangle(SX, SY, SWidth, SHeight);
    }

    public static class BloodStaticRegistryS
    {
        public static readonly List<BloodStaticListS> All = new();

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
                    BloodStaticListS newObj;


                    do
                    {
                        int x = rand.Next(minX, maxX);
                        int y = rand.Next(minY, maxY);
                        newObj = new BloodStaticListS(type, x, y);
                        attempts++;
                        if (attempts >= maxAttempts) break;  // <-- one‑line fix
                    } while (IsOverlapping(newObj, Enemys, dynO, breakables, physics, statics) && attempts < maxAttempts);


                    All.Add(newObj);
                }
            }
        }

        private static bool IsOverlapping(BloodStaticListS candidate, List<IiEnemy> Enemys, List<IDynamicO> dynO,
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


    public class House :IStatic
    {

        public  int StaticX {get => HouseX;set => HouseX = value;}
        public int StaticY { get => HouseY; set => HouseY = value; }

        public int StaticWidth { get => HouseWidth; set => HouseWidth = value; }

        public int StaticHeight { get => HouseHeight; set => HouseHeight = value; }

        public Rectangle StaticCollisionBox => HouseCollisionBox;

        public string StaticImagePath => HouseSpriteStyle;

        public string StaticDebugImagePath=>HouseSpriteDebugStyle;






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


            return false;
        }
    }






}

