namespace RoboutesTentacleBurgers.SpectralGL.Math

{
    public struct Vec2
    {
        public float X;
        public float Y;

        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vec2 operator +(Vec2 a, Vec2 b)
            => new Vec2(a.X + b.X, a.Y + b.Y);

        public static Vec2 operator -(Vec2 a, Vec2 b)
            => new Vec2(a.X - b.X, a.Y - b.Y);

        public static Vec2 operator *(Vec2 a, float s)
            => new Vec2(a.X * s, a.Y * s);

        public static Vec2 operator /(Vec2 a, float s)
            => new Vec2(a.X / s, a.Y / s);

        public float Length()
            => MathF.Sqrt(X * X + Y * Y);

        public Vec2 Normalized()
        {
            float len = Length();
            return len > 0 ? this / len : new Vec2(0, 0);
        }
    }
}
