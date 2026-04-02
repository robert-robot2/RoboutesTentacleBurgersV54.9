
using RoboutesTentacleBurgers.Breakables;
using RoboutesTentacleBurgers.DynamicObjects;
using RoboutesTentacleBurgers.Physics;
using RoboutesTentacleBurgers.StaticObjectSets;

public class BloodStaticObject
{
   

    public class BloodStaticList
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
        public static Dictionary<string, int> ForestDictionary() => new()
{
    { "Tree", 75 },
    { "Rock", 25 },
    { "FenceBroken", 10 },
    { "TorchNew01", 5 },
    { "Chest", 4 },
    { "GStone", 10 },
    { "GStoneCross", 10 },
    { "Mushroom", 10 },
    { "Skelcorpse001", 3 },
    { "Grass01", 50 },
    { "SkullONStick", 10 },
    { "Urn", 10 },
    { "Rose01", 10 },
    { "Barrel01", 10 },
    { "Bush01", 25 }
};


        public static Dictionary<string, (int shiftX, int shiftY, int w, int h)> CollisionMap = new()
    {
        { "Tree", (72, 156, 24, 24) },
        { "Rock", (16, 16, 24, 24) },
        { "FenceBroken", (10, 20, 64, 20) },
        { "TorchNew01", (8, 32, 24, 24) },
        { "Chest", (8 , 16, 24, 24) },
        { "GStone", (8, 20, 24, 24) },
        { "GStoneCross", (8, 20, 24, 24) },
        { "Mushroom", (0, 0, 24, 24) },
        { "Skelcorpse001", (48, 0, 24, 24) },
        { "Grass01", (0, 0, 24, 24) },
        { "SkullONStick", (8, 32, 24, 24) },
        { "Urn", (0, 0, 24, 24) },
          { "Rose01", (0, 0,16,16) },
              { "Barrel01", (16 , 32, 24, 24) },
                 { "Bush01", (24 , 52, 24, 24) }


    };

        public int ZIndex => Type switch
        {
            "Tree" => 4100,
            "Rock" => 4000,
            "FenceBroken" => 3900,
            "TorchNew01" => 3800,
            "Chest" => 3700,
            "GStone" => 3600,
            "GStoneCross" => 3550,
            "Mushroom" => 3540,
            "Skelcorpse001" => 3530,
            "Grass01" => 3520,
            "SkullONStick" => 3510,
            "Urn" => 3490,
            "Rose01" => 3480,
            "Barrel01" => 3470,
            "Bush01" => 3460,
            _ => 5000
        };

        public BloodStaticList(string type, int x, int y)
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
      //  public Rectangle SpriteBox =>
//new Rectangle(X, Y, Width, Height);
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


    public static class BloodStaticRegistry
    {
        public static readonly List<BloodStaticList> All = new();
       
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
                    BloodStaticList newObj;


                    do
                    {
                        int x = rand.Next(minX, maxX);
                        int y = rand.Next(minY, maxY);
                        newObj = new BloodStaticList(type, x, y);
                        attempts++;
                        if (attempts >= maxAttempts) break;  // <-- one‑line fix
                    } while (IsOverlapping(newObj, Enemys,dynO,breakables,physics,statics) && attempts < maxAttempts);


                    All.Add(newObj);
                }
            }
        }

        private static bool IsOverlapping(BloodStaticList candidate,List<IiEnemy> Enemys, List<IDynamicO> dynO,
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

    public class Cave : IStatic
    {
        public int StaticX { get => CaveX; set => CaveX = value; }
        public int StaticY { get => CaveY; set => CaveY = value; }
        public int StaticWidth { get => CaveWidth; set => CaveWidth = value; }
        public int StaticHeight { get => CaveHeight; set => CaveHeight = value; }
        public Rectangle StaticCollisionBox => CaveCollisionBox;
        public string StaticImagePath => CaveSpriteStyle;
        public string StaticDebugImagePath => CaveSpriteDebugStyle;

        // 🕳️ Cave Entrance
        public int CaveX { get; set; } = 750;   // fixed X
        public int CaveY { get; set; } = 1350;  // fixed Y
        public int CaveWidth { get; set; } = 256;
        public int CaveHeight { get; set; } = 256;

        private const int CaveCollisionWidth = 256;
        private const int CaveCollisionHeight = 256;
        private const int CaveCollisionShiftX = 0;
        private const int CaveCollisionShiftY = 0;

        public Rectangle CaveCollisionBox =>
          new Rectangle(
              CaveX + CaveCollisionShiftX,
              CaveY + CaveCollisionShiftY,
              CaveCollisionWidth,
              CaveCollisionHeight
          );

        public string CaveSpriteStyle =>
          $"position:absolute; left:{CaveX}px; top:{CaveY}px; " +
          $"width:{CaveWidth}px; height:{CaveHeight}px; " +
          $"background-image:url('/iAssets/CaveHole001.png'); background-size:cover; " +
          $"background-color:transparent; z-index:38;";

        public string CaveSpriteDebugStyle =>
          $"position:absolute; left:{CaveX}px; top:{CaveY}px; " +
          $"width:{CaveWidth}px; height:{CaveHeight}px; " +
          $"background-color:rgba(128,0,128,0.2); border:1px dashed purple; z-index:4050;";
    }

    public class CaveRegistry
    {
        public static List<Cave> All = new();

        public static void SpawnCaves(int count)
        {
            for (int i = 0; i < count; i++)
            {
                All.Add(new Cave
                {
                    // always spawn at fixed location
                    CaveX = 750,
                    CaveY = 1350
                });
            }
        }
    }




}
