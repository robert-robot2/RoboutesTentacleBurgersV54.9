namespace RoboutesTentacleBurgers.SpectralGL.Math

{
    public struct Mat4
    {
        public float[] M; // 16 floats, column-major

        public Mat4(bool identity)
        {
            M = new float[16];
            if (identity)
            {
                M[0] = 1; M[5] = 1; M[10] = 1; M[15] = 1;
            }
        }

        public static Mat4 Identity()
            => new Mat4(true);

        public static Mat4 operator *(Mat4 a, Mat4 b)
        {
            Mat4 r = new Mat4(false);
            r.M = new float[16];

            for (int col = 0; col < 4; col++)
            {
                for (int row = 0; row < 4; row++)
                {
                    r.M[col * 4 + row] =
                        a.M[0 * 4 + row] * b.M[col * 4 + 0] +
                        a.M[1 * 4 + row] * b.M[col * 4 + 1] +
                        a.M[2 * 4 + row] * b.M[col * 4 + 2] +
                        a.M[3 * 4 + row] * b.M[col * 4 + 3];
                }
            }

            return r;
        }

        public static Vec3 TransformPoint(Mat4 m, Vec3 v)
        {
            float x = v.X * m.M[0] + v.Y * m.M[4] + v.Z * m.M[8] + m.M[12];
            float y = v.X * m.M[1] + v.Y * m.M[5] + v.Z * m.M[9] + m.M[13];
            float z = v.X * m.M[2] + v.Y * m.M[6] + v.Z * m.M[10] + m.M[14];
            float w = v.X * m.M[3] + v.Y * m.M[7] + v.Z * m.M[11] + m.M[15];

            if (w != 0f) { x /= w; y /= w; z /= w; }
            return new Vec3(x, y, z);
        }

        public static Mat4 Translation(Vec3 t)
        {
            Mat4 m = Identity();
            m.M[12] = t.X;
            m.M[13] = t.Y;
            m.M[14] = t.Z;
            return m;
        }

        public static Mat4 Scale(Vec3 s)
        {
            Mat4 m = Identity();
            m.M[0] = s.X;
            m.M[5] = s.Y;
            m.M[10] = s.Z;
            return m;
        }

        public static Mat4 RotationX(float r)
        {
            Mat4 m = Identity();
            float c = MathF.Cos(r);
            float s = MathF.Sin(r);
            m.M[5] = c;
            m.M[6] = s;
            m.M[9] = -s;
            m.M[10] = c;
            return m;
        }

        public static Mat4 RotationY(float r)
        {
            Mat4 m = Identity();
            float c = MathF.Cos(r);
            float s = MathF.Sin(r);
            m.M[0] = c;
            m.M[2] = -s;
            m.M[8] = s;
            m.M[10] = c;
            return m;
        }

        public static Mat4 RotationZ(float r)
        {
            Mat4 m = Identity();
            float c = MathF.Cos(r);
            float s = MathF.Sin(r);
            m.M[0] = c;
            m.M[1] = s;
            m.M[4] = -s;
            m.M[5] = c;
            return m;
        }

        // === Perspective Projection (Blender-accurate, right-handed) ===
        public static Mat4 CreatePerspective(float fovRadians, float aspect, float near, float far)
        {
            float f = 1f / MathF.Tan(fovRadians * 0.5f);
            float nf = 1f / (near - far);

            Mat4 m = new Mat4(false);
            m.M[0] = f / aspect;
            m.M[5] = f;
            m.M[10] = (far + near) * nf;
            m.M[11] = -1f;
            m.M[14] = 2f * far * near * nf;
            return m;
        }

        public static Mat4 CreateOrthographic(float left, float right, float bottom, float top, float near, float far)
        {
            Mat4 m = new Mat4(false);
            m.M = new float[16];

            m.M[0] = 2f / (right - left);
            m.M[5] = 2f / (top - bottom);
            m.M[10] = -2f / (far - near);
            m.M[12] = -(right + left) / (right - left);
            m.M[13] = -(top + bottom) / (top - bottom);
            m.M[14] = -(far + near) / (far - near);
            m.M[15] = 1f;

            return m;
        }

        public static Mat4 CreateScale(Vec3 s) => Scale(s); public static Mat4 CreateTranslation(Vec3 t) => Translation(t); public static Mat4 CreateFromYawPitchRoll(float yaw, float pitch, float roll) { Mat4 rx = RotationX(pitch); Mat4 ry = RotationY(yaw); Mat4 rz = RotationZ(roll); return rz * (ry * rx); }
        // === Model Matrix (T * Rz * Ry * Rx * S) ===
        public static Mat4 CreateModelMatrix(Vec3 position, Vec3 rotation, Vec3 scale)
        {
            Mat4 t = Translation(position);
            Mat4 rx = RotationX(rotation.X);
            Mat4 ry = RotationY(rotation.Y);
            Mat4 rz = RotationZ(rotation.Z);
            Mat4 s = Scale(scale);

            // Blender-style: Scale → RotX → RotY → RotZ → Translate
            return t * (rz * (ry * (rx * s)));
        }

        public static Mat4 CreateLookAt(Vec3 eye, Vec3 target, Vec3 up)
        {
            Vec3 f = (target - eye).Normalized();
            Vec3 r = f.Cross(up).Normalized();
            Vec3 u = r.Cross(f);

            Mat4 m = Identity();
            m.M[0] = r.X; m.M[4] = r.Y; m.M[8] = r.Z;
            m.M[1] = u.X; m.M[5] = u.Y; m.M[9] = u.Z;
            m.M[2] = -f.X; m.M[6] = -f.Y; m.M[10] = -f.Z;
            m.M[12] = -(r.X * eye.X + r.Y * eye.Y + r.Z * eye.Z);
            m.M[13] = -(u.X * eye.X + u.Y * eye.Y + u.Z * eye.Z);
            m.M[14] = (f.X * eye.X + f.Y * eye.Y + f.Z * eye.Z);
            m.M[15] = 1f;
            return m;
        }

        public static Mat4 Invert(Mat4 m)
        {
            // Standard 4x4 matrix inverse via cofactor expansion
            float[] s = m.M;
            float[] inv = new float[16];

            inv[0] = s[5] * s[10] * s[15] - s[5] * s[11] * s[14] - s[9] * s[6] * s[15]
                     + s[9] * s[7] * s[14] + s[13] * s[6] * s[11] - s[13] * s[7] * s[10];
            inv[4] = -s[4] * s[10] * s[15] + s[4] * s[11] * s[14] + s[8] * s[6] * s[15]
                     - s[8] * s[7] * s[14] - s[12] * s[6] * s[11] + s[12] * s[7] * s[10];
            inv[8] = s[4] * s[9] * s[15] - s[4] * s[11] * s[13] - s[8] * s[5] * s[15]
                     + s[8] * s[7] * s[13] + s[12] * s[5] * s[11] - s[12] * s[7] * s[9];
            inv[12] = -s[4] * s[9] * s[14] + s[4] * s[10] * s[13] + s[8] * s[5] * s[14]
                     - s[8] * s[6] * s[13] - s[12] * s[5] * s[10] + s[12] * s[6] * s[9];
            inv[1] = -s[1] * s[10] * s[15] + s[1] * s[11] * s[14] + s[9] * s[2] * s[15]
                     - s[9] * s[3] * s[14] - s[13] * s[2] * s[11] + s[13] * s[3] * s[10];
            inv[5] = s[0] * s[10] * s[15] - s[0] * s[11] * s[14] - s[8] * s[2] * s[15]
                     + s[8] * s[3] * s[14] + s[12] * s[2] * s[11] - s[12] * s[3] * s[10];
            inv[9] = -s[0] * s[9] * s[15] + s[0] * s[11] * s[13] + s[8] * s[1] * s[15]
                     - s[8] * s[3] * s[13] - s[12] * s[1] * s[11] + s[12] * s[3] * s[9];
            inv[13] = s[0] * s[9] * s[14] - s[0] * s[10] * s[13] - s[8] * s[1] * s[14]
                     + s[8] * s[2] * s[13] + s[12] * s[1] * s[10] - s[12] * s[2] * s[9];
            inv[2] = s[1] * s[6] * s[15] - s[1] * s[7] * s[14] - s[5] * s[2] * s[15]
                     + s[5] * s[3] * s[14] + s[13] * s[2] * s[7] - s[13] * s[3] * s[6];
            inv[6] = -s[0] * s[6] * s[15] + s[0] * s[7] * s[14] + s[4] * s[2] * s[15]
                     - s[4] * s[3] * s[14] - s[12] * s[2] * s[7] + s[12] * s[3] * s[6];
            inv[10] = s[0] * s[5] * s[15] - s[0] * s[7] * s[13] - s[4] * s[1] * s[15]
                     + s[4] * s[3] * s[13] + s[12] * s[1] * s[7] - s[12] * s[3] * s[5];
            inv[14] = -s[0] * s[5] * s[14] + s[0] * s[6] * s[13] + s[4] * s[1] * s[14]
                     - s[4] * s[2] * s[13] - s[12] * s[1] * s[6] + s[12] * s[2] * s[5];
            inv[3] = -s[1] * s[6] * s[11] + s[1] * s[7] * s[10] + s[5] * s[2] * s[11]
                     - s[5] * s[3] * s[10] - s[9] * s[2] * s[7] + s[9] * s[3] * s[6];
            inv[7] = s[0] * s[6] * s[11] - s[0] * s[7] * s[10] - s[4] * s[2] * s[11]
                     + s[4] * s[3] * s[10] + s[8] * s[2] * s[7] - s[8] * s[3] * s[6];
            inv[11] = -s[0] * s[5] * s[11] + s[0] * s[7] * s[9] + s[4] * s[1] * s[11]
                     - s[4] * s[3] * s[9] - s[8] * s[1] * s[7] + s[8] * s[3] * s[5];
            inv[15] = s[0] * s[5] * s[10] - s[0] * s[6] * s[9] - s[4] * s[1] * s[10]
                     + s[4] * s[2] * s[9] + s[8] * s[1] * s[6] - s[8] * s[2] * s[5];

            float det = s[0] * inv[0] + s[1] * inv[4] + s[2] * inv[8] + s[3] * inv[12];
            if (MathF.Abs(det) < 1e-8f) return Mat4.Identity();

            float invDet = 1f / det;
            var result = new Mat4(false); // ← fix here
            for (int i = 0; i < 16; i++)
                result.M[i] = inv[i] * invDet;

            return result;
        }

    }
}
