using SpectralXAXA.SpectralXComponent;

namespace SpectralXAXA.SpectralXComponent.SpectralXRender
{
    /// <summary>
    /// Unified rendering system for SpectralX engine
    /// Handles 3D projection, transformation, and output generation
    /// </summary>
    public class SpectralXRenderer
    {
        public List<ProjectedTriangle> ProjectedTriangles { get; private set; } = new();

        // Projection settings
        public float FieldOfView { get; set; } = 60f; // degrees
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 100f;

        // Rendering stats
        public int TrianglesRendered { get; private set; }
        public int EntitiesRendered { get; private set; }

        /// <summary>
        /// Render an entire scene
        /// </summary>
        public void RenderScene(SpectralXScene scene, SpectralXCamera camera, SpectralXViewport viewport, SpectralXSVGViewport xSVGViewport)
        {
            ProjectedTriangles.Clear();
            TrianglesRendered = 0;
            EntitiesRendered = 0;

            var viewMatrix = camera.GetViewMatrix();
            var projMatrix = CreateProjectionMatrix(viewport.ViewportWidth, viewport.ViewportHeight);
            var viewProj = viewMatrix * projMatrix;

            var viewMatrix2 = camera.GetViewMatrix();
            var projMatrix2 = CreateProjectionMatrix(xSVGViewport.SVGViewportWidth, xSVGViewport.SVGViewportHeight);
            var viewProj2 = viewMatrix2 * projMatrix2;


            // Render only visible entities that pass frustum culling
            foreach (var entity in scene.GetVisibleEntities())
            {
                if (!IsInFrustum(entity, viewMatrix)) continue; // SKIP if not in view!

                RenderEntity(entity, viewProj, viewport.ViewportWidth, viewport.ViewportHeight);

                RenderEntity(entity, viewProj, xSVGViewport.SVGViewportWidth, xSVGViewport.SVGViewportHeight);
                EntitiesRendered++;
            }




        }

        /// <summary>
        /// Render a single entity
        /// </summary>
        public void RenderEntity(SpectralXEntity entity, System.Numerics.Matrix4x4 viewProjMatrix, int screenWidth, int screenHeight)
        {
            var mesh = entity.Mesh;
            if (mesh == null || !mesh.IsValid()) return;

            var worldMatrix = entity.GetWorldMatrix();
            var worldViewProj = worldMatrix * viewProjMatrix;

            // Project each triangle
            for (int i = 0; i < mesh.Indices.Count; i += 3)
            {
                var v0 = mesh.Vertices[mesh.Indices[i]];
                var v1 = mesh.Vertices[mesh.Indices[i + 1]];
                var v2 = mesh.Vertices[mesh.Indices[i + 2]];

                // Transform to clip space
                var p0 = Vector3.Transform(v0, worldViewProj);
                var p1 = Vector3.Transform(v1, worldViewProj);
                var p2 = Vector3.Transform(v2, worldViewProj);

                // Basic frustum culling (check if all vertices behind camera)
                if (p0.Z <= 0 && p1.Z <= 0 && p2.Z <= 0)
                    continue;

                // Convert to screen coordinates
                var s0 = ToScreenSpace(p0, screenWidth, screenHeight);
                var s1 = ToScreenSpace(p1, screenWidth, screenHeight);
                var s2 = ToScreenSpace(p2, screenWidth, screenHeight);

                // Backface culling (optional - can be disabled for debugging)
                if (!IsBackface(s0, s1, s2))
                {
                    ProjectedTriangles.Add(new ProjectedTriangle
                    {
                        P0 = s0,
                        P1 = s1,
                        P2 = s2,
                        EntityId = entity.Id,
                        Depth = (p0.Z + p1.Z + p2.Z) / 3f // Average depth for sorting
                    });

                    TrianglesRendered++;
                }
            }
        }

        /// <summary>
        /// Render a single mesh at origin (legacy support)
        /// </summary>
        public void RenderMesh(IMesh mesh, System.Numerics.Matrix4x4 viewMatrix, int screenWidth, int screenHeight)
        {
            ProjectedTriangles.Clear();
            TrianglesRendered = 0;

            var projMatrix = CreateProjectionMatrix(screenWidth, screenHeight);
            var viewProj = viewMatrix * projMatrix;

            // Project each triangle
            for (int i = 0; i < mesh.Indices.Count; i += 3)
            {
                var v0 = mesh.Vertices[mesh.Indices[i]];
                var v1 = mesh.Vertices[mesh.Indices[i + 1]];
                var v2 = mesh.Vertices[mesh.Indices[i + 2]];

                var p0 = Vector3.Transform(v0, viewProj);
                var p1 = Vector3.Transform(v1, viewProj);
                var p2 = Vector3.Transform(v2, viewProj);

                if (p0.Z <= 0 && p1.Z <= 0 && p2.Z <= 0)
                    continue;

                var s0 = ToScreenSpace(p0, screenWidth, screenHeight);
                var s1 = ToScreenSpace(p1, screenWidth, screenHeight);
                var s2 = ToScreenSpace(p2, screenWidth, screenHeight);

                if (!IsBackface(s0, s1, s2))
                {
                    ProjectedTriangles.Add(new ProjectedTriangle
                    {
                        P0 = s0,
                        P1 = s1,
                        P2 = s2,
                        Depth = (p0.Z + p1.Z + p2.Z) / 3f
                    });

                    TrianglesRendered++;
                }
            }
        }

        /// <summary>
        /// Create perspective projection matrix
        /// </summary>
        private System.Numerics.Matrix4x4 CreateProjectionMatrix(int width, int height)
        {
            float fovRadians = FieldOfView * (MathF.PI / 180f);
            float aspect = (float)width / height;
            return System.Numerics.Matrix4x4.CreatePerspectiveFieldOfView(fovRadians, aspect, NearPlane, FarPlane);
        }

        /// <summary>
        /// Convert clip space to screen space
        /// </summary>
        private Vector2 ToScreenSpace(Vector3 clipPos, int width, int height)
        {
            // Perspective divide
            float x = clipPos.X / clipPos.Z;
            float y = clipPos.Y / clipPos.Z;

            // NDC to screen space
            return new Vector2(
                (x + 1f) * 0.5f * width,
                (1f - y) * 0.5f * height
            );
        }

        /// <summary>
        /// Check if triangle is backfacing (CCW winding)
        /// </summary>
        private bool IsBackface(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            // Cross product z-component
            float cross = (p1.X - p0.X) * (p2.Y - p0.Y) - (p1.Y - p0.Y) * (p2.X - p0.X);
            return cross < 0;
        }

        /// <summary>
        /// Sort triangles by depth (painter's algorithm)
        /// Call this before rendering for proper depth ordering
        /// </summary>
        public void SortByDepth()
        {
            ProjectedTriangles.Sort((a, b) => b.Depth.CompareTo(a.Depth)); // Far to near
        }
        /// <summary>
        /// Check if entity is in camera frustum (simple check)
        /// </summary>
        private bool IsInFrustum(SpectralXEntity entity, System.Numerics.Matrix4x4 viewMatrix)
        {
            // Transform entity position to view space
            var worldPos = entity.Position;
            var viewPos = Vector3.Transform(worldPos, viewMatrix);

            // Simple check: is it in front of camera?
            if (viewPos.Z >= 0) return false; // Behind camera

            // Could add FOV checks here for sides, but this is a quick win
            return true;
        }






        /// <summary>
        /// Clear all rendered data
        /// </summary>
        public void Clear()
        {
            ProjectedTriangles.Clear();
            TrianglesRendered = 0;
            EntitiesRendered = 0;
        }

        public class ProjectedTriangle
        {
            public Vector2 P0 { get; set; }
            public Vector2 P1 { get; set; }
            public Vector2 P2 { get; set; }
            public string EntityId { get; set; } = string.Empty;
            public float Depth { get; set; }
        }
    }
}