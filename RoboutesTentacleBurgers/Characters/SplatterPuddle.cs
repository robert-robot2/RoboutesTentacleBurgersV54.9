




public class SplatterPuddle
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Xpx => $"{X}px";
    public string Ypx => $"{Y}px";


    public int Width { get; set; } = 24;
    public int Height { get; set; } = 24;

    public string Widthpx => $"{(int)(Width * Scale)}px";
    public string Heightpx => $"{(int)(Height * Scale)}px";

    public double Scale { get; set; } = 1.0;
    public string Path { get; set; } = "/iAssets/BPuddle01.png";
}

public class BloodSplatterRegistry
{
    // Global list of puddles for the current map
    public static List<SplatterPuddle> All { get; } = new();

    // Clear puddles when switching maps
    public static void Clear()
    {
        All.Clear();
    }

    // Optional: add puddles manually if needed
    public static void Add(SplatterPuddle puddle)
    {
        All.Add(puddle);
    }
}
