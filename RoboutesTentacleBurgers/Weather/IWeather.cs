using System.Collections.Generic;

namespace RoboutesTentacleBurgers.Weather
{
    // 🔹 Enum for particle control (used inside weather classes)
    public enum WeatherParticleType
    {
        Clear,
        Rain,
        Snow,
        Cloud
    }

    // 🔹 Enum for handle switching (used by BloodWeatherHandle)
    public enum WeatherHandleType
    {
        Forest,
        Snow,
        Desert
        // Add more as needed
    }

    // 🔹 Particle contract
    public interface IParticle
    {
        string Texture { get; set; }
        int X { get; set; }
        int Y { get; set; }
        int VX { get; set; }
        int VY { get; set; }
        double Lifetime { get; set; }
        double Age { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        double Opacity { get; set; }
    }

    // 🔹 Weather contract
    public interface IWeather
    {
        // Fog Law
        bool IsFogEnabled { get; }
        string FogStyle { get; }
        void AdvanceFog();

        // Weather Cycle Law
        WeatherParticleType CurrentWeather { get; }
        void TickWeather(double deltaTime);

        // Particle Law
        IEnumerable<IParticle> GetActiveParticles();
        void TickParticles(double deltaTime);
    }
}
