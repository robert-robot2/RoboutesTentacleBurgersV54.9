

//Version G Orbits does not look, X Right + ,y forward -, Z up -

using RoboutesTentacleBurgers.SpectralGL.Math;
namespace RoboutesTentacleBurgers.SpectralXComponent
{
    public class SpectralXCamera
    {
//public Vec3 Position { get; set; } = new Vec3(0, 0, 0);
     //   public Vec3 Rotation { get; set; } = new Vec3(0, 0, 0);
        public float MoveSpeed { get; set; } = 0.5f;
        public float MouseSensitivity { get; set; } = 0.003f;

        // Version A Forward - proven correct coords
        public Vec3 Forward =>
            new Vec3(
                MathF.Sin(Rotation.Y) * MathF.Cos(Rotation.X),
                MathF.Cos(Rotation.Y) * MathF.Cos(Rotation.X),
                MathF.Sin(Rotation.X)
            ).Normalized();

        // Version A Right - keeping this the same
        public Vec3 Right => Forward.Cross(new Vec3(0, 0, 1)).Normalized();
        public Vec3 Up => Right.Cross(Forward).Normalized();

        public void MoveForward() => Position -= Forward * MoveSpeed;
        public void MoveBackward() => Position += Forward * MoveSpeed;

   
        public void Look(float deltaX, float deltaY)
        {
            float yaw = Rotation.Y - deltaX * MouseSensitivity;
            float pitch = Rotation.X + deltaY * MouseSensitivity;
            pitch = Math.Clamp(pitch, -MathF.PI / 2 + 0.1f, MathF.PI / 2 - 0.1f);
            Rotation = new Vec3(pitch, yaw, 0);
        }
        public void StrafeLeft() => Position -= Right * MoveSpeed;
        public void StrafeRight() => Position += Right * MoveSpeed;


        private Vec3 _lastPosition;
        private Vec3 _lastRotation;
        private Mat4 _cachedView = Mat4.Identity();
        private bool _viewDirty = true;

        public Vec3 Position
        {
            get => _position;
            set
            {
                if (_position.X != value.X || _position.Y != value.Y || _position.Z != value.Z)
                {
                    _position = value;
                    _viewDirty = true;
                }
            }
        }
        private Vec3 _position;

        public Vec3 Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation.X != value.X || _rotation.Y != value.Y || _rotation.Z != value.Z)
                {
                    _rotation = value;
                    _viewDirty = true;
                }
            }
        }
        private Vec3 _rotation;

        // Rail properties
        public float TargetZ { get; set; } = 4f;
        public float MinZ { get; set; } = -60f;
        public float MaxZ { get; set; } = 10f;
        public float RailX { get; set; } = 0f;
        public float RailY { get; set; } = -10f;
        public float RailLerpSpeed { get; set; } = 8f;

        public void ScrollRail(float delta)
        {
            if (MathF.Abs(delta) < 0.01f) return;
            TargetZ = Math.Clamp(TargetZ + delta, MinZ, MaxZ);
        }

        public void TickRail(float deltaTime)
        {
            float newZ = Position.Z + (TargetZ - Position.Z) * RailLerpSpeed * deltaTime;
            Position = new Vec3(RailX, RailY, newZ);
        }




        public Mat4 GetViewMatrix()
        {
            if (!_viewDirty) return _cachedView;
            _viewDirty = false;

            Vec3 f = Forward;
            Vec3 r = Right;
            Vec3 u = Up;

            Mat4 m = Mat4.Identity();
            m.M[0] = r.X; m.M[4] = r.Y; m.M[8] = r.Z;
            m.M[1] = u.X; m.M[5] = u.Y; m.M[9] = u.Z;
            m.M[2] = -f.X; m.M[6] = -f.Y; m.M[10] = -f.Z;
            m.M[12] = -(r.X * _position.X + r.Y * _position.Y + r.Z * _position.Z);
            m.M[13] = -(u.X * _position.X + u.Y * _position.Y + u.Z * _position.Z);
            m.M[14] = (f.X * _position.X + f.Y * _position.Y + f.Z * _position.Z);
            m.M[15] = 1f;

            _cachedView = m;
            return _cachedView;
        }

        public float[] GetViewMatrixArray()
        {
            var m = GetViewMatrix();
            return m.M;
        }

        public float[] GetProjectionMatrixArray(float fovDegrees, float aspect, float near, float far)
        {
            float fov = fovDegrees * (MathF.PI / 180f);
            float f = 1f / MathF.Tan(fov / 2f);
            float rangeInv = 1f / (near - far);

            return new float[]
            {
        f / aspect, 0,  0,                          0,
        0,          f,  0,                          0,
        0,          0,  (near + far) * rangeInv,   -1,
        0,          0,  (2 * near * far) * rangeInv, 0
            };
        }

    }
}


