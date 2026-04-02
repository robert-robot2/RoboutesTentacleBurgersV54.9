
    public class BloodStaticHandle
    {

        public IStatic? ActiveStatic { get; set; }

        public static readonly List<IEnumerable<IStatic>> AllRegistries = new()
        {
         BloodStaticObject.CaveRegistry.All,
         BloodStaticObjectS.HouseRegistry.All,
         BloodStaticObjectT.ShrineRegistry.All,
         BloodStaticObjectT.Fountain001Registry.All,
         BloodStaticObjectT.HouseRegistry.All,
         BloodStaticObjectT.Tavern001Registry.All,




        };


        public void Clear() => ActiveStatic = null;

   }

