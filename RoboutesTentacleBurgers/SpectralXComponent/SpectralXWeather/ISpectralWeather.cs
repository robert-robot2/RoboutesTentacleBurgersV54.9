using System.Collections.Generic;

namespace RoboutesTentacleBurgers.SpectralXComponent.SpectralXWeather
{
    public interface ISpectralWeather
    {
        WeatherParticleType2 CurrentWeather { get; }
        bool IsActive { get; }

        void Init(SpectralXScene scene, SpectralXMeshLibrary meshLibrary,
            Dictionary<WeatherParticleType2, ParticleVolume> volumes);
        void Tick(float deltaTime, SpectralXCamera camera);
        void Reset();

        IEnumerable<SpectralXParticle> GetActiveParticles();
        int ActiveParticleCount { get; }
    }

    public enum WeatherParticleType2
    {
        Clear,
        Rain,
        Snow,
        Cloud,
        Lightning
    }
}