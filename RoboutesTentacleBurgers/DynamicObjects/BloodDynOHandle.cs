
using RoboutesTentacleBurgers.DynamicObjects;

public class BloodDynOHandle
{
    public IDynamicO? ActiveDynO { get; set; }

    public static readonly List<IEnumerable<IDynamicO>> AllRegistries = new()
    {
        BloodDynamicObj.BloodCampFireRegistry.All,
        BloodDynamicObj.BloodCheeseRegistry.All,
        BloodDynamicObj.BloodHealPotRegistry.All,
        BloodDynamicObj.BloodMedHealPotRegistry.All,
        BloodDynamicObj.BloodManaPotRegistry.All,
        BloodDynamicObj.BloodStrPotRegistry.All,
        BloodDynamicObj.BloodCelPotRegistry.All,
        BloodDynamicObj.BloodAlcPotRegistry.All,
        BloodDynamicObj.BloodIntPotRegistry.All
    };

    public void Clear() => ActiveDynO = null;
}
