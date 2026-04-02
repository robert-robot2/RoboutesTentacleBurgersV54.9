using RoboutesTentacleBurgers.SpectralXComponent.SpectralXLighting;
using System.Numerics;

namespace RoboutesTentacleBurgers.SpectralXComponent
{
    /// <summary>
    /// SpectralXSun — drives time of day, directional light color/intensity,
    /// sky blend factor, cloud/star UV offsets, and moon state.
    /// TimeOfDay: 0.0 = midnight, 0.25 = sunrise, 0.5 = noon, 0.75 = sunset, 1.0 = midnight
    /// Coordinate system: Blender XYZ — X=right, Y=forward, Z=up
    /// </summary>
    public class SpectralXSun
    {
        // ── Public State ────────────────────────────────────────────────────────
        public float TimeOfDay { get; private set; } = 0.5f;

        /// <summary>0.0 = full day texture, 1.0 = full night texture.</summary>
        public float SkyBlend { get; private set; } = 0f;

        /// <summary>Sun direction FROM sun TOWARD origin. XYZ space.</summary>
        public Vector3 SunDirection { get; private set; } = new Vector3(0f, 0f, -1f);

        public Vector3 SunColor { get; private set; } = new Vector3(1f, 1f, 1f);
        public float SunIntensity { get; private set; } = 4.0f;

        // ── Moon State ──────────────────────────────────────────────────────────
        /// <summary>Moon direction FROM moon TOWARD origin. Opposite arc to sun.</summary>
        public Vector3 MoonDirection { get; private set; } = new Vector3(0f, 0f, 1f);

        /// <summary>Cold blue-white moon color.</summary>
        public Vector3 MoonColor { get; private set; } = new Vector3(0.7f, 0.8f, 1.0f);

        /// <summary>Moon glow intensity — 0 at day, 1 at full night.</summary>
        public float MoonGlow { get; private set; } = 0f;

        // ── UV Scroll Offsets ───────────────────────────────────────────────────
        /// <summary>
        /// Cloud texture UV scroll offset — accumulated each tick.
        /// Faster than stars, matches real cloud drift speed.
        /// </summary>
        public float CloudOffset { get; private set; } = 0f;

        /// <summary>
        /// Star texture UV scroll offset — accumulated each tick.
        /// Very slow, barely perceptible like real star field rotation.
        /// </summary>
        public float StarOffset { get; private set; } = 0f;

      public float CloudScale { get; private set; } = 1.2f;
        public float StarScale { get; private set; } = 2.2f;

        // Cloud drift speed — units per second of UV scroll
        // Tuning note: increase for faster clouds, decrease for slower
        private const float CloudBaseSpeed = 0.025f;

        // Star drift speed — much slower than clouds
        // Tuning note: this is intentionally very subtle
        private const float StarBaseSpeed = 0.0015f;


        // ── Computed Sky Colors ─────────────────────────────────────────────────
        public Vector3 SkyZenithColor { get; private set; } = new Vector3(0.10f, 0.45f, 0.90f);
        public Vector3 SkyHorizonColor { get; private set; } = new Vector3(0.65f, 0.80f, 1.00f);

        // ── Time Stops ──────────────────────────────────────────────────────────
        private readonly struct SunStop
        {
            public readonly float Time;
            public readonly Vector3 Color;
            public readonly float Intensity;
            public SunStop(float time, float r, float g, float b, float intensity)
            {
                Time = time;
                Color = new Vector3(r, g, b);
                Intensity = intensity;
            }
        }

        private static readonly SunStop[] _stops = new[]
        {
            new SunStop(0.00f,  0.05f, 0.05f, 0.15f, 0.0f),
            new SunStop(0.20f,  0.10f, 0.10f, 0.30f, 0.2f),
            new SunStop(0.25f,  1.00f, 0.45f, 0.15f, 1.5f),
            new SunStop(0.30f,  1.00f, 0.80f, 0.40f, 2.5f),
            new SunStop(0.40f,  1.00f, 0.95f, 0.80f, 3.5f),
            new SunStop(0.50f,  1.00f, 0.98f, 0.90f, 5.0f),
            new SunStop(0.60f,  1.00f, 0.95f, 0.80f, 3.5f),
            new SunStop(0.70f,  1.00f, 0.80f, 0.40f, 2.5f),
            new SunStop(0.75f,  1.00f, 0.35f, 0.10f, 1.5f),
            new SunStop(0.80f,  0.40f, 0.15f, 0.35f, 0.5f),
            new SunStop(0.85f,  0.10f, 0.05f, 0.20f, 0.1f),
            new SunStop(1.00f,  0.05f, 0.05f, 0.15f, 0.0f),
        };

        private readonly struct SkyStop
        {
            public readonly float Time;
            public readonly Vector3 Zenith;
            public readonly Vector3 Horizon;
            public SkyStop(float time,
                float zR, float zG, float zB,
                float hR, float hG, float hB)
            {
                Time = time;
                Zenith = new Vector3(zR, zG, zB);
                Horizon = new Vector3(hR, hG, hB);
            }
        }

        private static readonly SkyStop[] _skyStops = new[]
        {
            new SkyStop(0.00f,  0.01f, 0.01f, 0.08f,  0.02f, 0.02f, 0.12f),
            new SkyStop(0.20f,  0.05f, 0.05f, 0.20f,  0.05f, 0.10f, 0.20f),
            new SkyStop(0.25f,  0.10f, 0.15f, 0.50f,  0.90f, 0.45f, 0.20f),
            new SkyStop(0.30f,  0.20f, 0.35f, 0.70f,  0.95f, 0.75f, 0.35f),
            new SkyStop(0.40f,  0.15f, 0.40f, 0.85f,  0.70f, 0.85f, 0.95f),
            new SkyStop(0.50f,  0.10f, 0.45f, 0.90f,  0.65f, 0.80f, 1.00f),
            new SkyStop(0.60f,  0.15f, 0.40f, 0.85f,  0.70f, 0.85f, 0.95f),
            new SkyStop(0.70f,  0.20f, 0.35f, 0.70f,  0.95f, 0.75f, 0.35f),
            new SkyStop(0.75f,  0.10f, 0.15f, 0.50f,  0.95f, 0.35f, 0.15f),
            new SkyStop(0.80f,  0.08f, 0.05f, 0.25f,  0.45f, 0.20f, 0.30f),
            new SkyStop(0.85f,  0.03f, 0.02f, 0.15f,  0.08f, 0.05f, 0.15f),
            new SkyStop(1.00f,  0.01f, 0.01f, 0.08f,  0.02f, 0.02f, 0.12f),
        };

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// Set time of day from UI slider (0.0 to 1.0).
        /// Recomputes all derived values immediately.
        /// </summary>
        public void SetTime(float t)
        {
            TimeOfDay = Math.Clamp(t, 0f, 1f);
            Recompute();
        }

        /// <summary>
        /// Advance time-independent offsets each frame.
        /// Call from TickSun() in the engine every frame.
        /// deltaTime in seconds.
        /// </summary>
        public void Tick(float deltaTime)
        {
            // Cloud speed scales slightly with time of day —
            // faster midday (more thermal activity), slower at dawn/dusk
            // SunIntensity is already 0 at night so this naturally slows clouds at night
            float cloudSpeed = CloudBaseSpeed * (0.5f + SunIntensity * 0.1f);
            cloudSpeed = Math.Clamp(cloudSpeed, CloudBaseSpeed * 0.5f, CloudBaseSpeed * 2.0f);

            CloudOffset = (CloudOffset + cloudSpeed * deltaTime) % 1.0f;

            // Stars always drift at constant rate — independent of sun
            StarOffset = (StarOffset + StarBaseSpeed * deltaTime) % 1.0f;
        }

        /// <summary>
        /// Apply the current sun state into a SpectralXLight directional slot.
        /// Call this every tick after SetTime/Tick.
        /// </summary>
        public void Apply(SpectralXLight light)
        {
            light.Type = LightType.Directional;
            light.Direction = SunDirection;
            light.Color = SunColor;
            light.Intensity = SunIntensity;
            light.Enabled = SunIntensity > 0.01f;

            light.Position = new Vector3(
                -SunDirection.X * 80f,
                -SunDirection.Y * 80f,
                -SunDirection.Z * 80f);
        }

        // ── Private ─────────────────────────────────────────────────────────────

        private void Recompute()
        {
            ComputeSunDirection();
            ComputeSunLight();
            ComputeSkyColors();
            ComputeSkyBlend();
            ComputeMoon();
        }

        /// <summary>
        /// Arc the sun across the sky in XYZ (Blender) space.
        /// X = east/west, Z = up, Y = unused (forward axis).
        /// t=0.25 sunrise from +X, t=0.50 noon at +Z, t=0.75 sunset at -X.
        /// </summary>
        private void ComputeSunDirection()
        {
            // Full 360 arc — noon (0.5) maps to top of arc (Z+)
            float angle = (TimeOfDay - 0.25f) * MathF.PI * 2f;

            // XZ plane arc — X=east/west, Z=up
            float sunX = MathF.Cos(angle);
            float sunZ = MathF.Sin(angle);

            // Direction FROM sun TOWARD origin
            var sunPos = new Vector3(sunX, 0f, sunZ);
            SunDirection = -Vector3.Normalize(sunPos);
        }

        /// <summary>
        /// Moon is on the exact opposite arc to the sun (180 degrees offset).
        /// Visible and glowing only when SkyBlend is high (night).
        /// Moon color is cold blue-white.
        /// </summary>
        private void ComputeMoon()
        {
            // Moon arc = sun arc + 180 degrees
            float angle = (TimeOfDay - 0.25f) * MathF.PI * 2f + MathF.PI;

            float moonX = MathF.Cos(angle);
            float moonZ = MathF.Sin(angle);

            var moonPos = new Vector3(moonX, 0f, moonZ);
            MoonDirection = -Vector3.Normalize(moonPos);

            // Moon glow scales with SkyBlend — only bright at night
            // Smooth fade in/out at dusk/dawn transitions
            MoonGlow = Math.Clamp(SkyBlend * 1.2f - 0.1f, 0f, 1f);

            // Moon color — cold blue-white, slightly warmer when near horizon
            float moonElevation = Math.Clamp(moonZ, 0f, 1f); // Z = up in XYZ
            MoonColor = Vector3.Lerp(
                new Vector3(0.9f, 0.85f, 0.7f),  // warm-white near horizon
                new Vector3(0.7f, 0.80f, 1.0f),  // cold blue-white at zenith
                moonElevation);
        }

        private void ComputeSunLight()
        {
            var times = _stops.Select(s => s.Time).ToArray();
            (float t0, float t1, float blend) = FindStopBlend(times);

            int i0 = FindStopIndex(times, t0);
            int i1 = FindStopIndex(times, t1);

            SunColor = Vector3.Lerp(_stops[i0].Color, _stops[i1].Color, blend);
            SunIntensity = Lerp(_stops[i0].Intensity, _stops[i1].Intensity, blend);
        }

        private void ComputeSkyColors()
        {
            var times = _skyStops.Select(s => s.Time).ToArray();
            (float t0, float t1, float blend) = FindStopBlend(times);

            int i0 = FindStopIndex(times, t0);
            int i1 = FindStopIndex(times, t1);

            SkyZenithColor = Vector3.Lerp(_skyStops[i0].Zenith, _skyStops[i1].Zenith, blend);
            SkyHorizonColor = Vector3.Lerp(_skyStops[i0].Horizon, _skyStops[i1].Horizon, blend);
        }

        private void ComputeSkyBlend()
        {
            float t = TimeOfDay;

            if (t >= 0.30f && t <= 0.70f) SkyBlend = 0f;
            else if (t > 0.70f && t < 0.85f) SkyBlend = (t - 0.70f) / (0.85f - 0.70f);
            else if (t >= 0.85f || t <= 0.15f) SkyBlend = 1f;
            else SkyBlend = 1f - (t - 0.15f) / (0.30f - 0.15f);
        }

        private (float lower, float upper, float blend) FindStopBlend(float[] times)
        {
            float t = TimeOfDay;
            for (int i = 0; i < times.Length - 1; i++)
            {
                if (t >= times[i] && t <= times[i + 1])
                {
                    float range = times[i + 1] - times[i];
                    float blend = range > 0f ? (t - times[i]) / range : 0f;
                    return (times[i], times[i + 1], blend);
                }
            }
            return (times[times.Length - 2], times[times.Length - 1], 1f);
        }

        private int FindStopIndex(float[] times, float time)
        {
            for (int i = 0; i < times.Length; i++)
                if (MathF.Abs(times[i] - time) < 0.0001f) return i;
            return 0;
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;
    }
}