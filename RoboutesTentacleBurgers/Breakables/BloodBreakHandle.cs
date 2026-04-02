
using RoboutesTentacleBurgers.Breakables;

public class BloodBreakHandle
{
    public IBreakables? Breakables { get; set; }

    public static readonly List<IEnumerable<IBreakables>> AllRegistries = new()
    {
        BloodBreakables.DummyRegistry.All,


    };
    public void UpdateActiveEnemy()
    {
        if (AllRegistries == null || AllRegistries.Count == 0)
        {
            Breakables = null;
            return;
        }

        foreach (var registry in AllRegistries)
        {
            if (registry == null) continue;

            foreach (var breakables in registry)
            {
                if (breakables != null && breakables.BreakIsAlive)
                {
                    Breakables = breakables;
                    return;
                }
            }
        }

        Breakables = null;
    }
    public void Clear() => Breakables = null;
}
