

// 🪵 Static Object Definition Blood Town Map Spawn -1,0 BloodStaticListT
using RoboutesTentacleBurgers.Breakables;
using RoboutesTentacleBurgers.DynamicObjects;
using RoboutesTentacleBurgers.Physics;
using static BloodStaticObject;

public class BloodStaticObjectC
{



    // 🪵 Static Object Definition Blood Town Map Spawn -1,0 BloodStaticListT
    public class BloodStaticListC
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
        public static Dictionary<string, int> CaveDictionary() => new()
{
    { "Tree", 0},
    { "Rock", 25 },
    { "FenceBroken", 15 },
    { "TorchNew01", 15 },
    { "Chest", 5 },
    { "GStone", 20 },
    { "GStoneCross", 20 },
    { "Mushroom", 45 },
    { "Skelcorpse001", 10 },
    { "Grass01", 0 },
    { "SkullONStick", 25 },
    { "Urn", 5 },
    { "Rose01", 0 },
    { "Barrel01", 5 },
    { "Bush01", 0 }
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
            _ => 5000,

        };

        public BloodStaticListC(string type, int x, int y)
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

    public static class BloodStaticRegistryC
    {
        public static readonly List<BloodStaticListC> All = new();

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
                    BloodStaticListC newObj;


                    do
                    {
                        int x = rand.Next(minX, maxX);
                        int y = rand.Next(minY, maxY);
                        newObj = new BloodStaticListC(type, x, y);
                        attempts++;
                        if (attempts >= maxAttempts) break;  // <-- one‑line fix
                    } while (IsOverlapping(newObj, Enemys, dynO, breakables, physics, statics) && attempts < maxAttempts);


                    All.Add(newObj);
                }
            }
        }

        private static bool IsOverlapping(BloodStaticListC candidate, List<IiEnemy> Enemys, List<IDynamicO> dynO,
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


}

    