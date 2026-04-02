namespace RoboutesTentacleBurgers.Lighting
{
    public enum LightingType
    {
        Directional,
        Cave,
        Dungeon,
        // Add more as needed
    }

    public class BloodLightHandle
    {
        public ILighting Lighting { get; set; }

        public BloodLightHandle(LightingType type = LightingType.Directional)
        {
            Lighting = type switch
            {
                LightingType.Directional => new DirectionalLightCycle
                {
                    CycleDurationMinutes = 2,
                    IsPaused = false,
                },
                LightingType.Cave => new DLightCycleC
                {
                    CycleDurationMinutes = 2,
                    IsPaused = false,
                },
                _ => new DirectionalLightCycle
                {
                    CycleDurationMinutes = 2,
                    IsPaused = false,
                }
            };
        }

    }
}