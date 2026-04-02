namespace SpectralXAXA.SpectralXComponent.SpectralXRender
{
   
        /// <summary>
        /// Represents an instance of a mesh in the 3D world
        /// Has position, rotation, scale (world transform)
        /// Multiple entities can share the same mesh data
        /// </summary>
        public class SpectralXEntity
        {
            /// <summary>
            /// Unique identifier for this entity instance
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Display name for this entity
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Reference to the mesh data (shared, not copied)
            /// </summary>
            public IMesh Mesh { get; set; }

            /// <summary>
            /// World position
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// World rotation (Euler angles in radians)
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// World scale
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// Is this entity active and should be rendered?
            /// </summary>
            public bool IsActive { get; set; }

            /// <summary>
            /// Is this entity visible (culling can set this)
            /// </summary>
            public bool IsVisible { get; set; }

            /// <summary>
            /// Optional: Tag for grouping/filtering entities
            /// </summary>
            public string Tag { get; set; }

            public SpectralXEntity(IMesh mesh, Vector3? position = null, Vector3? rotation = null, Vector3? scale = null)
            {
                Id = Guid.NewGuid().ToString();
                Name = mesh.Name + "_Instance";
                Mesh = mesh;
                Position = position ?? Vector3.Zero;
                Rotation = rotation ?? Vector3.Zero;
                Scale = scale ?? Vector3.One;
                IsActive = true;
                IsVisible = true;
                Tag = string.Empty;
            }

            /// <summary>
            /// Get the world transformation matrix for this entity
            /// Order: Scale → Rotate → Translate
            /// </summary>
            public System.Numerics.Matrix4x4 GetWorldMatrix()
            {
                var scaleMatrix = System.Numerics.Matrix4x4.CreateScale(Scale);
                var rotationMatrix = System.Numerics.Matrix4x4.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
                var translationMatrix = System.Numerics.Matrix4x4.CreateTranslation(Position);

                return scaleMatrix * rotationMatrix * translationMatrix;
            }

            /// <summary>
            /// Move entity by offset
            /// </summary>
            public void Translate(Vector3 offset)
            {
                Position += offset;
            }

            /// <summary>
            /// Rotate entity by offset (in radians)
            /// </summary>
            public void Rotate(Vector3 rotation)
            {
                Rotation += rotation;
            }

            /// <summary>
            /// Set position directly
            /// </summary>
            public void SetPosition(float x, float y, float z)
            {
                Position = new Vector3(x, y, z);
            }

            /// <summary>
            /// Set rotation directly (in radians)
            /// </summary>
            public void SetRotation(float yaw, float pitch, float roll)
            {
                Rotation = new Vector3(pitch, yaw, roll);
            }

            /// <summary>
            /// Set scale uniformly
            /// </summary>
            public void SetScale(float uniformScale)
            {
                Scale = new Vector3(uniformScale, uniformScale, uniformScale);
            }

            /// <summary>
            /// Update entity (override in subclasses for custom behavior)
            /// </summary>
            public virtual void Update(float deltaTime)
            {
                // Override in derived classes for entity-specific logic
            }
        }
    
}
