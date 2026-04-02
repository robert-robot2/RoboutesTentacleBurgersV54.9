
using RoboutesTentacleBurgers.Physics;

public class BloodPhysicsHandle
{
    public IPhysics? Physics { get; set; }

    public static readonly List<IEnumerable<IPhysics>> AllRegistries = new()
    {
        BloodPhysics.BloodUndeadGFRegistry.All,


    };

    public void Clear() => Physics = null;
}
