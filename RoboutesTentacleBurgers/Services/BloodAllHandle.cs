namespace RoboutesTentacleBurgers.Services
{
    public sealed class BloodAllHandle
    {
        public BloodLightHandle? LightHandle { get; set; }
        public BloodWeatherHandle? WeatherHandle { get; set; }
        public BloodStaticHandle? StaticHandle { get; set; }
        public BloodCharacterHandle? CharacterHandle { get; set; }
        public BloodEnemyHandle? EnemyHandle { get; set; }
        public BloodDynOHandle? DynOHandle { get; set; }
        public BloodBreakHandle? BreakHandle { get; set; }
        public BloodPhysicsHandle? PhysicsHandle { get; set; }

        public void EnsureDefaults()
        {
            LightHandle     ??= new BloodLightHandle();
            WeatherHandle   ??= new BloodWeatherHandle();
            StaticHandle    ??= new BloodStaticHandle();
            CharacterHandle ??= new BloodCharacterHandle();
            EnemyHandle     ??= new BloodEnemyHandle();
            DynOHandle      ??= new BloodDynOHandle();
            BreakHandle     ??= new BloodBreakHandle();
            PhysicsHandle   ??= new BloodPhysicsHandle();
        }
    }
}