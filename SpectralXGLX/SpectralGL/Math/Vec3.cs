namespace SpectralXGLX.SpectralGL.Math

{
    public struct Vec3
    {
        public float X;
        public float Y;
        public float Z;

        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vec3 operator +(Vec3 a, Vec3 b)
            => new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vec3 operator -(Vec3 a, Vec3 b)
            => new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vec3 operator *(Vec3 a, float s)
            => new Vec3(a.X * s, a.Y * s, a.Z * s);

        public static Vec3 operator /(Vec3 a, float s)
            => new Vec3(a.X / s, a.Y / s, a.Z / s);

        public float Dot(Vec3 b)
            => X * b.X + Y * b.Y + Z * b.Z;

        public Vec3 Cross(Vec3 b)
            => new Vec3(
                Y * b.Z - Z * b.Y,
                Z * b.X - X * b.Z,
                X * b.Y - Y * b.X
            );

        public float Length()
            => MathF.Sqrt(X * X + Y * Y + Z * Z);

        // added to display camera realtime updates xyz
        public override string ToString()
        {
            return $"({X:F2}, {Y:F2}, {Z:F2})";
        }


        public Vec3 Normalized()
        {
            float len = Length();
            return len > 0 ? this / len : new Vec3(0, 0, 0);
        }
    }
}
