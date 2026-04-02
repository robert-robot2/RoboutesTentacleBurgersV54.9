using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RoboutesTentacleBurgers.SpectralXComponent.SpectralXWeather
{
    public class SpectralXWeatherClass : ISpectralWeather
    {
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private readonly Random _rng = new Random();
        private readonly SpectralXParticleManager _particles = new();

        private double _lastWeatherChangeTime = 0;
        private double _lastSpawnTime = 0;
        private const double WeatherCycleInterval = 30.0;
        private const double SpawnInterval = 0.5;

        public WeatherParticleType2 CurrentWeather { get; private set; } = WeatherParticleType2.Rain;
        public bool IsActive { get; private set; } = false;
        public int ActiveParticleCount => _particles.ActiveParticleCount;

        public IEnumerable<SpectralXParticle> GetActiveParticles()
            => _particles.GetActiveParticles();

        private const int MaxParticles = 1000;
        private float[] _offsetsCache = new float[MaxParticles * 3];
        private float[] _colorsCache = new float[MaxParticles * 4];
        private float[] _sizesCache = new float[MaxParticles];

        public void Init(SpectralXScene scene, SpectralXMeshLibrary meshLibrary,
            Dictionary<WeatherParticleType2, ParticleVolume> volumes)
        {
            _particles.Init(scene, meshLibrary, volumes);
            IsActive = true;
            _clock.Restart();
            _lastWeatherChangeTime = 0;
            _lastSpawnTime = 0;
            CurrentWeather = WeatherParticleType2.Rain;
        }

        public void Tick(float deltaTime, SpectralXCamera camera)
        {
            if (!IsActive) return;

            double now = _clock.Elapsed.TotalSeconds;
            UpdateWeatherCycle(now);

            if (now - _lastSpawnTime > SpawnInterval)
            {
                _lastSpawnTime = now;
                SpawnBatch();
            }

            _particles.Tick(deltaTime, camera);
        }

        public void Reset()
        {
            _particles.Reset();
            IsActive = false;
            _clock.Reset();
        }

        private void UpdateWeatherCycle(double now)
        {
            if (now - _lastWeatherChangeTime < WeatherCycleInterval) return;
            _lastWeatherChangeTime = now;

            int roll = _rng.Next(0, 100);
            if (roll < 35) CurrentWeather = WeatherParticleType2.Rain;
            else if (roll < 75) CurrentWeather = WeatherParticleType2.Snow;
            else if (roll < 85) CurrentWeather = WeatherParticleType2.Cloud;
            else CurrentWeather = WeatherParticleType2.Clear;
        }

        private void SpawnBatch()
        {
            switch (CurrentWeather)
            {
                case WeatherParticleType2.Rain:
                    SpawnRain(25);
                    if (_rng.NextDouble() < 0.05)
                        SpawnLightning();
                    break;
                case WeatherParticleType2.Snow:
                    SpawnSnow(20);
                    break;
                case WeatherParticleType2.Cloud:
                    SpawnClouds(5);
                    break;
                case WeatherParticleType2.Clear:
                    break;
            }
        }

        private void SpawnRain(int count)
        {
            for (int i = 0; i < count; i++)
                _particles.Spawn(WeatherParticleType2.Rain,
                    vx: 0f, vy: 0f, vz: -4f,
                    lifetime: 2.0f, opacity: 0.9f);
        }

        private void SpawnSnow(int count)
        {
            for (int i = 0; i < count; i++)
            {
                float drift = (_rng.NextSingle() - 0.5f) * 0.3f;
                _particles.Spawn(WeatherParticleType2.Snow,
                    vx: drift, vy: drift, vz: -0.8f,
                    lifetime: 5.0f, opacity: 1f);
            }
        }

        private void SpawnClouds(int count)
        {
            for (int i = 0; i < count; i++)
            {
                float drift = (_rng.NextSingle()) * 0.5f + 0.2f;
                _particles.Spawn(WeatherParticleType2.Cloud,
                    vx: drift, vy: 0f, vz: 0f,
                    lifetime: 10.0f, opacity: 0.5f);
            }
        }

        private void SpawnLightning()
        {
            _particles.Spawn(WeatherParticleType2.Lightning,
                vx: 0f, vy: 0f, vz: 0f,
                lifetime: 0.3f, opacity: 0.9f);
        }

        public List<ParticleInstanceGroup> BuildInstanceGroups()
        {
            var groups = new List<ParticleInstanceGroup>();

            var typeMap = new[]
            {
                (Type: "Rain",      Particles: _particles.GetPoolSlots(WeatherParticleType2.Rain),
                    TexKey: "ParticleGeo_/iAssets/RainDrop01.png"),
                (Type: "Snow",      Particles: _particles.GetPoolSlots(WeatherParticleType2.Snow),
                    TexKey: "ParticleGeo_/iAssets/SnowFlake01.png"),
                (Type: "Cloud",     Particles: _particles.GetPoolSlots(WeatherParticleType2.Cloud),
                    TexKey: "ParticleGeo_/iAssets/GOkuCloud001.png"),
                (Type: "Lightning", Particles: _particles.GetPoolSlots(WeatherParticleType2.Lightning),
                    TexKey: "ParticleGeo_/iAssets/LBolt002.png"),
            };

            foreach (var entry in typeMap)
            {
                int count = 0;
                foreach (var p in entry.Particles)
                {
                    if (!p.IsAlive) continue;
                    _offsetsCache[count * 3] = p.X;
                    _offsetsCache[count * 3 + 1] = p.Y;
                    _offsetsCache[count * 3 + 2] = p.Z;
                    _colorsCache[count * 4] = 1f;
                    _colorsCache[count * 4 + 1] = 1f;
                    _colorsCache[count * 4 + 2] = 1f;
                    _colorsCache[count * 4 + 3] = p.Opacity;
                    _sizesCache[count] = p.Width;
                    count++;
                }
                if (count == 0) continue;

                groups.Add(new ParticleInstanceGroup
                {
                    Type = entry.Type,
                    Count = count,
                    Offsets = _offsetsCache[..(count * 3)],
                    Colors = _colorsCache[..(count * 4)],
                    Sizes = _sizesCache[..count],
                    TexKey = entry.TexKey,
                });
            }

            return groups;
        }
    }
}