using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RoboutesTentacleBurgers.Weather
{
    public class BloodLifeCyclesS : IWeather
    {
        // 🔹 Fog properties
        public float FogOffsetX { get; set; } = 0;
        public float FogOffsetY { get; set; } = 0;
        public float FogSpeedX { get; set; } = 0.2f; // slower fog drift
        public float FogSpeedY { get; set; } = 0.3f;
        public bool IsFogEnabled { get; set; } = true;

        public string FogStyle =>
            $"position:absolute;" +
            $"left:0px; top:0px; width:2048px; height:2048px;" +
            $"background-image:url('/iAssets/fog02SL.png'); " +
            $"background-position:{FogOffsetX}px {FogOffsetY}px; " +
            $"background-repeat:repeat; background-size:cover; " +
            $"opacity:0.2; z-index:1500; pointer-events:none;";

        public void AdvanceFog()
        {
            FogOffsetX = (FogOffsetX + FogSpeedX) % 2048f;
            FogOffsetY = (FogOffsetY + FogSpeedY) % 2048f;
        }

        // 🔹 Weather cycle
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private readonly Random _weatherRNG = new Random();
        private double _lastWeatherChangeTime = 0;
        private const double WeatherCycleInterval = 20.0; // faster cycle

        public WeatherParticleType CurrentWeather { get; private set; } = WeatherParticleType.Clear;

        private void UpdateWeatherCycle(double currentTime)
        {
            if (currentTime - _lastWeatherChangeTime > WeatherCycleInterval)
            {
                _lastWeatherChangeTime = currentTime;
                int roll = _weatherRNG.Next(0, 100);

                if (roll < 50) CurrentWeather = WeatherParticleType.Snow;
                else if (roll < 80) CurrentWeather = WeatherParticleType.Cloud;
                else CurrentWeather = WeatherParticleType.Clear;
            }
        }

        // 🔹 Particle control
        private double _lastParticleSpawnTime = 0;

        public IEnumerable<IParticle> GetActiveParticles() => ParticleManager.GetActiveParticles();

        public void TickParticles(double deltaTime) => ParticleManager.Update(deltaTime);

        public void TickWeather(double deltaTime)
        {
            UpdateWeatherCycle(_clock.Elapsed.TotalSeconds);

            if (_clock.Elapsed.TotalSeconds - _lastParticleSpawnTime > 0.4)
            {
                _lastParticleSpawnTime = _clock.Elapsed.TotalSeconds;
                RenderWeatherParticles();
            }
        }

        private void RenderWeatherParticles()
        {
            switch (CurrentWeather)
            {
                case WeatherParticleType.Snow:
                    SpawnSnowParticles(40); // heavier snow
                    break;
                case WeatherParticleType.Cloud:
                    SpawnCloudParticles(3);
                    break;
                case WeatherParticleType.Clear:
                    break;
            }
        }

        private void SpawnSnowParticles(int count)
        {
            for (int i = 0; i < count; i++)
            {
                int x = _weatherRNG.Next(0, 2048);
                int y = _weatherRNG.Next(0, 2048);
                int drift = _weatherRNG.Next(-2, 3);
                ParticleManager.Spawn("/iAssets/SnowFlake02.png", x, y, vx: drift, vy: 1,
                    duration: 6.0, opacity: 0.9);
            }
        }

        private void SpawnCloudParticles(int count)
        {
            for (int i = 0; i < count; i++)
            {
                int x = _weatherRNG.Next(0, 2048);
                int y = _weatherRNG.Next(200, 2048);
                int drift = _weatherRNG.Next(1, 4);
                ParticleManager.Spawn("/iAssets/Cloud02.png", x, y, vx: drift, vy: 0,
                    duration: 12.0, width: 160, height: 160, opacity: 0.6);
            }
        }
    }
}
