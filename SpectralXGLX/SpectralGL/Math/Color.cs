namespace SpectralXGLX.SpectralGL.Math

{
    public struct Color
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public Color(float r, float g, float b, float a = 1f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static Color operator *(Color c, float s)
            => new Color(c.R * s, c.G * s, c.B * s, c.A);

        public static Color operator +(Color a, Color b)
            => new Color(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);
    }
}
