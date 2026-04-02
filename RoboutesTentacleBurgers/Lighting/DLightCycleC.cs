using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using RoboutesTentacleBurgers.Lighting;

public class DLightCycleC : ILighting
{
    // Internal state
    public bool IsPaused { get; set; } = false;
    public int CycleDurationMinutes { get; set; } = 2; // 2–1440
    private DateTime _lastUpdateTime = DateTime.Now;
    private double _accumulatedSeconds = 0;
    private Stopwatch _clock = Stopwatch.StartNew();
    private double _pausedHour = 0;

    public double CurrentHour { get; private set; } = 0;
    public Vector2 LightDirection { get; private set; }
    public Color LightColor { get; private set; }
    public float Intensity { get; private set; }
    public enum DayNightFlowRate
    {
        Fast2Min,
        RealTime24Hr
    }

    public DayNightFlowRate FlowRate { get; set; } = DayNightFlowRate.Fast2Min;

    // Interface mappings (Option B style)
    bool ILighting.LightingIsPaused { get => IsPaused; set => IsPaused = value; }
    double ILighting.LightingCurrentHour => CurrentHour;
    float ILighting.LightingIntensity => Intensity;
    Color ILighting.LightingColor => LightColor;
    Vector2 ILighting.LightingDirection => LightDirection;
    int ILighting.LightingCycleDurationMinutes { get => CycleDurationMinutes; set => CycleDurationMinutes = value; }
 

    void ILighting.LightingUpdate() => Update();

    // Deterministic update
    public void Update()
    {
        double seconds = _clock.Elapsed.TotalSeconds;
        double cycleSeconds = CycleDurationMinutes * 60.0;
        double hour = IsPaused
            ? _pausedHour
            : (seconds % cycleSeconds) / cycleSeconds * 24.0;

        if (!IsPaused)
            _pausedHour = hour;

        var now = DateTime.Now;
        var delta = (now - _lastUpdateTime).TotalSeconds;
        _lastUpdateTime = now;

        if (!IsPaused)
            _accumulatedSeconds += delta;

        CurrentHour = hour;

        // Direction (360°)
        var angle = (hour / 24.0) * 360.0;
        var radians = angle * Math.PI / 180.0;
        LightDirection = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));

        // Color + intensity (cave cycle law)
        (LightColor, Intensity) = GetColorAndIntensityForHour(hour);
    }

    private (Color, float) GetColorAndIntensityForHour(double hour)
    {
        // Cave palette: black → dark blue → teal → back to black
        if (hour < 6)
        {
            return (
                InterpolateColor(Color.Black, Color.DarkBlue, hour / 6.0),
                InterpolateFloat(0.25f, 0.20f, hour / 6.0)
            );
        }
        if (hour < 12)
        {
            return (
                InterpolateColor(Color.DarkBlue, Color.Teal, (hour - 6) / 6.0),
                0.20f
            );
        }
        if (hour < 18)
        {
            return (
                InterpolateColor(Color.Teal, Color.DarkBlue, (hour - 12) / 6.0),
                0.20f
            );
        }
        return (
            InterpolateColor(Color.DarkBlue, Color.Black, (hour - 18) / 6.0),
            InterpolateFloat(0.20f, 0.25f, (hour - 18) / 6.0)
        );
    }

    private float InterpolateFloat(float start, float end, double t) =>
        (float)(start + (end - start) * t);

    private Color InterpolateColor(Color from, Color to, double t)
    {
        int r = (int)(from.R + (to.R - from.R) * t);
        int g = (int)(from.G + (to.G - from.G) * t);
        int b = (int)(from.B + (to.B - from.B) * t);
        return Color.FromArgb(r, g, b);
    }
}
