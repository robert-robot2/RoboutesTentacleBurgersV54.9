using System.Collections.Generic;
using System.Linq;

namespace RoboutesTentacleBurgers.Weather
{
    // 🔹 Concrete particle implementation
    public class Particle : IParticle
    {
        public string Texture { get; set; } = default!;
        public int X { get; set; }
        public int Y { get; set; }
        public int VX { get; set; }
        public int VY { get; set; }
        public double Lifetime { get; set; }
        public double Age { get; set; }
        public int Width { get; set; } = 8;
        public int Height { get; set; } = 8;
        public double Opacity { get; set; } = 1;
    }

    // 🔹 Particle manager
    public static class ParticleManager
    {
        private static readonly List<Particle> _particles = new();

        public static void Spawn(
            string texturePath, int x, int y,
            int vx = 0, int vy = 0,
            double duration = 2.0,
            int width = 8, int height = 8,
            double opacity = 1.0)
        {
            const int MaxParticles = 250;
            if (_particles.Count >= MaxParticles) return;

            _particles.Add(new Particle
            {
                Texture = texturePath,
                X = x,
                Y = y,
                VX = vx,
                VY = vy,
                Lifetime = duration,
                Age = 0,
                Width = width,
                Height = height,
                Opacity = opacity
            });
        }

        public static void Update(double deltaTime)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.X += p.VX;
                p.Y += p.VY;
                p.Age += deltaTime;

                if (p.Age >= p.Lifetime)
                    _particles.RemoveAt(i);
            }
        }

        // 🔹 Expose particles as interface type
        public static IEnumerable<IParticle> GetActiveParticles() => _particles.Cast<IParticle>();
    }
}
