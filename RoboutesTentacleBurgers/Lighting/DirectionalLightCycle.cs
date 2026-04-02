using System;
using System.Drawing;
using System.Numerics;
using RoboutesTentacleBurgers.Lighting;

public class DirectionalLightCycle : ILighting
{
    // Internal state
    private DateTime _lastUpdateTime = DateTime.Now;
    private double _accumulatedSeconds = 0;
    private double _pausedHour = 0;

    // Public properties
    public bool IsPaused { get; set; } = false;
    public int CycleDurationMinutes { get; set; } = 2; // 2–1440
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
        double cycleSeconds = CycleDurationMinutes * 60.0;

        var now = DateTime.Now;
        var delta = (now - _lastUpdateTime).TotalSeconds;
        _lastUpdateTime = now;

        if (!IsPaused)
            _accumulatedSeconds = (_accumulatedSeconds + delta) % cycleSeconds;

        double hour = IsPaused
            ? _pausedHour
            : (_accumulatedSeconds / cycleSeconds) * 24.0;

        if (!IsPaused)
            _pausedHour = hour;

        CurrentHour = hour;

        // Compute direction
        var angle = (hour / 24.0) * 360.0;
        var radians = angle * Math.PI / 180.0;
        LightDirection = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));

        // Compute color + intensity
        (LightColor, Intensity) = GetColorAndIntensityForHour(hour);
    }

    private (Color, float) GetColorAndIntensityForHour(double hour)
    {
        if (hour < 2)
            return (InterpolateColor(Color.Black, Color.DarkBlue, (hour - 0) / 2.0),
                    InterpolateFloat(0.55f, 0.15f, (hour - 0) / 2.0));
        if (hour < 6)
            return (InterpolateColor(Color.DarkBlue, Color.LightBlue, (hour - 2) / 4.0), 0.15f);
        if (hour < 12)
            return (InterpolateColor(Color.LightBlue, Color.LightYellow, (hour - 6) / 6.0), 0.15f);
        if (hour < 14)
            return (InterpolateColor(Color.LightYellow, Color.White, (hour - 12) / 2.0), 0.15f);
        if (hour < 18)
            return (InterpolateColor(Color.White, Color.Orange, (hour - 14) / 4.0), 0.15f);
        if (hour < 22)
            return (InterpolateColor(Color.Orange, Color.DarkBlue, (hour - 18) / 4.0), 0.15f);

        return (InterpolateColor(Color.DarkBlue, Color.Black, (hour - 22) / 2.0),
                InterpolateFloat(0.15f, 0.55f, (hour - 22) / 2.0));
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
