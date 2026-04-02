
namespace SpectralXAXA.SpectralXComponent
{
    public class SpectralXCamera
    {
        public Vector3 Position { get; set; } = new Vector3(0, 0, -20);
        public Vector3 Rotation { get; set; } = new Vector3(0, 0, 0); // pitch, yaw, roll
        public float MoveSpeed { get; set; } = 0.2f;
        public float RotationSpeed { get; set; } = 0.02f;
        public Vector3 FocusPoint { get; set; } = new Vector3(0, 0, 0);

        // Calculate forward/right vectors based on rotation
        public Vector3 Forward => new Vector3(
            MathF.Sin(Rotation.Y),
            0,
            MathF.Cos(Rotation.Y)
        );

        public Vector3 Right => new Vector3(
            MathF.Cos(Rotation.Y),
            0,
            -MathF.Sin(Rotation.Y)
        );

        public void Update()
        {
            // Camera update logic (if needed)
        }

        public void Move(Vector3 direction)
        {
            Position += direction * MoveSpeed;
        }

        // Move relative to camera orientation
        public void MoveForward() => Position += Forward * MoveSpeed;
        public void MoveBackward() => Position -= Forward * MoveSpeed;
        public void StrafeLeft() => Position -= Right * MoveSpeed;
        public void StrafeRight() => Position += Right * MoveSpeed;

        public System.Numerics.Matrix4x4 GetViewMatrix()
        {
            // Look in the direction we're facing
            var target = Position + Forward;
            return System.Numerics.Matrix4x4.CreateLookAt(Position, target, Vector3.UnitY);
        }
    }

}