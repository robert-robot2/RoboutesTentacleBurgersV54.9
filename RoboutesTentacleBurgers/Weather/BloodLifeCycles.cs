using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RoboutesTentacleBurgers.Weather
{
    public class BloodLifeCycles : IWeather
    {
        // 🔹 Fog properties
        public float FogOffsetX { get; set; } = 0;
        public float FogOffsetY { get; set; } = 0;
        public float FogSpeedX { get; set; } = 0.4f;
        public float FogSpeedY { get; set; } = 0.4f;
        public bool IsFogEnabled { get; set; } = true;

        public string FogStyle =>
            $"position:absolute;" +
            $"left:0px; top:0px; width:2048px; height:2048px;" +
            $"background-image:url('/iAssets/fog01SL.png'); " +
            $"background-position:{FogOffsetX}px {FogOffsetY}px; " +
            $"background-repeat:repeat; background-size:cover; " +
            $"opacity:0.1; z-index:1500; pointer-events:none;";

        public void AdvanceFog()
        {
            FogOffsetX = (FogOffsetX + FogSpeedX) % 2048f;
            FogOffsetY = (FogOffsetY + FogSpeedY) % 2048f;
        }

        // 🔹 Weather cycle
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private readonly Random _weatherRNG = new Random();
        private double _lastWeatherChangeTime = 0;
        private const double WeatherCycleInterval = 30.0; // seconds

        public WeatherParticleType CurrentWeather { get; private set; } = WeatherParticleType.Clear;

        private void UpdateWeatherCycle(double currentTime)
        {
            if (currentTime - _lastWeatherChangeTime > WeatherCycleInterval)
            {
                _lastWeatherChangeTime = currentTime;
                int roll = _weatherRNG.Next(0, 100);

                if (roll < 35) CurrentWeather = WeatherParticleType.Rain;
                else if (roll < 75) CurrentWeather = WeatherParticleType.Snow;
                else if (roll < 85) CurrentWeather = WeatherParticleType.Cloud;
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

            if (_clock.Elapsed.TotalSeconds - _lastParticleSpawnTime > 0.5)
            {
                _lastParticleSpawnTime = _clock.Elapsed.TotalSeconds;
                RenderWeatherParticles();
            }
        }

        private void RenderWeatherParticles()
        {
            switch (CurrentWeather)
            {
                case WeatherParticleType.Rain:
                    SpawnRainParticles(25);
                    if (_weatherRNG.NextDouble() < 0.05)
                        SpawnLightningBolt();
                    break;
                case WeatherParticleType.Snow:
                    SpawnSnowParticles(20);
                    break;
                case WeatherParticleType.Cloud:
                    SpawnCloudParticles(5);
                    break;
                case WeatherParticleType.Clear:
                    break;
            }
        }

        private void SpawnRainParticles(int count)
        {
            for (int i = 0; i < count; i++)
            {
                int x = _weatherRNG.Next(0, 2048);
                int y = _weatherRNG.Next(0, 2048);
                ParticleManager.Spawn("/iAssets/RainDrop01.png", x, y, vx: 0, vy: 8, duration: 2.0, opacity: 1);
            }
        }

        private void SpawnSnowParticles(int count)
        {
            for (int i = 0; i < count; i++)
            {
                int x = _weatherRNG.Next(0, 2048);
                int y = _weatherRNG.Next(0, 2048);
                int drift = _weatherRNG.Next(-1, 2);
                ParticleManager.Spawn("/iAssets/SnowFlake01.png", x, y, vx: drift, vy: 1, duration: 5.0, opacity: 1);
            }
        }

        private void SpawnCloudParticles(int count)
        {
            for (int i = 0; i < count; i++)
            {
                int x = _weatherRNG.Next(0, 2048);
                int y = _weatherRNG.Next(100, 2048);
                int drift = _weatherRNG.Next(1, 3);
                ParticleManager.Spawn("/iAssets/GOkuCloud001.png", x, y, vx: drift, vy: 0,
                    duration: 10.0, width: 128, height: 128, opacity: 0.5);
            }
        }

        private void SpawnLightningBolt()
        {
            int x = _weatherRNG.Next(0, 2048);
            int y = _weatherRNG.Next(0, 2048);
            ParticleManager.Spawn("/iAssets/LBolt002.png", x, y, vx: 0, vy: 0,
                duration: 0.3, width: 16, height: 128, opacity: 0.8);
        }
    }
}
