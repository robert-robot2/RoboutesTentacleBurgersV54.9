namespace RoboutesTentacleBurgers.Services
{
    public static class ZIndexCache
    {
        private static List<dynamic>? _sortedStaticObjects;
        private static DateTime _lastSortTime = DateTime.MinValue;
        private static readonly TimeSpan CacheExpiry = TimeSpan.FromMilliseconds(500);

        public static List<dynamic> GetSortedStaticObjects()
        {
            if (_sortedStaticObjects == null || (DateTime.Now - _lastSortTime) > CacheExpiry)
            {
                var allStaticObjects = new List<dynamic>();

                // Add all registries dynamically
                allStaticObjects.AddRange(BloodStaticObject.BloodStaticRegistry.All);
                allStaticObjects.AddRange(BloodStaticObjectC.BloodStaticRegistryC.All);
                allStaticObjects.AddRange(BloodStaticO1010.BloodStaticRegistry1010.All);
                allStaticObjects.AddRange(BloodStaticO1020.BloodStaticRegistry1020.All);
                allStaticObjects.AddRange(BloodStaticO1030.BloodStaticRegistry1030.All);
                allStaticObjects.AddRange(BloodStaticObjectGY.BloodStaticRegistryGY.All);
                allStaticObjects.AddRange(BloodStaticObjectS.BloodStaticRegistryS.All);
                allStaticObjects.AddRange(BloodStaticObjectT.BloodStaticRegistryT.All);
                allStaticObjects.AddRange(BloodStaticO1040.BloodStaticRegistry1040.All);
                allStaticObjects.AddRange(BloodStaticO1050.BloodStaticRegistry1050.All);
                allStaticObjects.AddRange(BloodStaticO1060.BloodStaticRegistry1060.All);

                _sortedStaticObjects = allStaticObjects
                    .OrderBy(o => o.CollisionBox.Y)
                    .ToList();

                _lastSortTime = DateTime.Now;
            }
            return _sortedStaticObjects;
        }

        public static void InvalidateCache()
        {
            _sortedStaticObjects = null;
        }
    }

}
