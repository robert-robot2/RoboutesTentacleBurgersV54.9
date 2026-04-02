namespace SpectralXBXB.SpectralXComponent
{
    /// <summary>
    /// Simple FPS-style camera with mouse look support.
    /// Assumes world looks down -Z (Doom-style).
    /// </summary>
    public class SpectralXCamera
    {
        // Start IN FRONT of origin, looking toward it
        public Vector3 Position { get; set; } = new Vector3(0, 1, 25);
        // Rotation in radians: X = pitch, Y = yaw
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public float MoveSpeed { get; set; } = 0.2f;
        public float RotationSpeed { get; set; } = 0.02f;
        public float MouseSensitivity { get; set; } = 0.003f; // Mouse look sensitivity


        // Forward vector (looking down -Z by default)
        public Vector3 Forward =>
            Vector3.Normalize(new Vector3(
                MathF.Sin(Rotation.Y) * MathF.Cos(Rotation.X),
                -MathF.Sin(Rotation.X),
                -MathF.Cos(Rotation.Y) * MathF.Cos(Rotation.X)
            ));

        public Vector3 Right =>
        Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));

        public Vector3 Up =>
            Vector3.Normalize(Vector3.Cross(Right, Forward));




        public void Move(Vector3 direction)
        {
            Position += direction * MoveSpeed;
        }

      
        // FPS-style movement
        public void MoveForward() => Position += Forward * MoveSpeed;
        public void MoveBackward() => Position -= Forward * MoveSpeed;
        public void StrafeLeft() => Position += Right * MoveSpeed;
        public void StrafeRight() => Position -= Right * MoveSpeed;
       

        // Mouse look
        public void Look(float deltaX, float deltaY)
        {
            // Yaw (horizontal rotation)
            Rotation = new Vector3(
                Rotation.X,
                Rotation.Y + deltaX * MouseSensitivity,
                Rotation.Z
            );

            // Pitch (vertical rotation) with clamping
            float newPitch = Rotation.X + deltaY * MouseSensitivity;
            newPitch = Math.Clamp(newPitch, -MathF.PI / 2 + 0.1f, MathF.PI / 2 - 0.1f);

            Rotation = new Vector3(
                newPitch,
                Rotation.Y,
                Rotation.Z
            );
        }

        public Matrix4x4 GetViewMatrix()
        {
            var target = Position + Forward;
            return Matrix4x4.CreateLookAt(
                Position,
                target,
                Vector3.UnitY
            );
        }
    }
}