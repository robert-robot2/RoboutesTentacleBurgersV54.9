namespace RoboutesTentacleBurgers.Lighting
{
    using System.Drawing;
    using System.Numerics;


  


    public interface ILighting
    {
        // Core state
        bool LightingIsPaused { get; set; }
        double LightingCurrentHour { get; }
        float LightingIntensity { get; }
        Color LightingColor { get; }
        Vector2 LightingDirection { get; }

        // Cycle configuration
        int LightingCycleDurationMinutes { get; set; }
       

        // Control
        void LightingUpdate();
    }


}
