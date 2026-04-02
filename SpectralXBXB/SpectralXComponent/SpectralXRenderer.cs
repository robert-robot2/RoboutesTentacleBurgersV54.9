using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace SpectralXBXB.SpectralXComponent
{
    /// <summary>
    /// Unified rendering system for SpectralX engine
    /// Handles 3D projection, transformation, lighting, and markup output
    /// </summary>
    public class SpectralXRenderer
    {
        public List<ProjectedTriangle> ProjectedTriangles { get; private set; } = new();
        public List<ProjectedQuad> ProjectedQuads { get; private set; } = new();

        // Final frame output (the "framebuffer")
        public MarkupString Output { get; private set; }

        // Projection settings
        public float FieldOfView { get; set; } = 90f;
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 100f;

        // Lighting settings
        public Vector3 AmbientLight { get; set; } = new Vector3(0.2f, 0.2f, 0.2f);

        // Stats
        public int TrianglesRendered { get; private set; }
        public int QuadsRendered { get; private set; }

        // ===== Cached CSS fragments (Part 1.b) =====
        private const string CommonDivPrefix = "<div style=\"position:absolute;";
        private const string UnitTriangleClip = "clip-path:polygon(0px 0px,1px 0px,0px 1px);";
        private const string UnitQuadClip = "clip-path:polygon(0px 0px,1px 0px,1px 1px,0px 1px);";

        private readonly StringBuilder _htmlBuilder = new StringBuilder(1024 * 64);

        private readonly Dictionary<int, string> _primitiveMarkupCache = new();

        public void RenderScene(
            SpectralXScene scene,
            SpectralXCamera camera,
            SpectralXViewport viewport)
        {
            ProjectedTriangles.Clear();
            TrianglesRendered = 0;
            ProjectedQuads.Clear();
            QuadsRendered = 0;

            var viewMatrix = camera.GetViewMatrix();
            var projectionMatrix = CreateProjectionMatrix(
                viewport.ViewportWidth,
                viewport.ViewportHeight);

            var viewProjectionMatrix = viewMatrix * projectionMatrix;

            foreach (var mesh in scene.Meshes)
            {
                RenderMeshInternal(
                    mesh,
                    scene.Lights,
                    viewProjectionMatrix,
                    viewport.ViewportWidth,
                    viewport.ViewportHeight,
                    camera.Position);  // ADD THIS
            }


            SortByDepth();
            BuildMarkup(viewport.ViewportWidth, viewport.ViewportHeight);
        }

   private void RenderMeshInternal(
    IMesh mesh,
    IReadOnlyList<SpectralXLight> lights,
    Matrix4x4 viewProjectionMatrix,
    int screenWidth,
    int screenHeight,
    Vector3 cameraPosition)
{
    var scale = mesh.Size == Vector3.Zero ? Vector3.One : mesh.Size;

    var modelMatrix =
        Matrix4x4.CreateScale(scale) *
        Matrix4x4.CreateFromYawPitchRoll(
            mesh.Rotation.Y,
            mesh.Rotation.X,
            mesh.Rotation.Z) *
        Matrix4x4.CreateTranslation(mesh.Position);

    var modelViewProjectionMatrix = modelMatrix * viewProjectionMatrix;

    foreach (var face in mesh.Faces)
    {
        if (face.Type == FaceType.Triangle)
        {
            RenderTriangleFace(
                mesh, face, lights,
                modelViewProjectionMatrix,
                screenWidth, screenHeight,
                cameraPosition);
        }
        else if (face.Type == FaceType.Quad)
        {
            RenderQuadFace(
                mesh, face, lights,
                modelViewProjectionMatrix,
                screenWidth, screenHeight,
                cameraPosition);
        }
    }
}


        private Vector3 CalculateLighting(Vector3 position, Vector3 normal, IReadOnlyList<SpectralXLight> lights)
        {
            // Start with ambient light
            Vector3 color = AmbientLight;

            // Add contribution from each light
            foreach (var light in lights)
            {
                if (!light.Enabled) continue;
                color += light.CalculateColor(position, normal);
            }

            // Clamp to 0-1 range
            color.X = Math.Clamp(color.X, 0f, 1f);
            color.Y = Math.Clamp(color.Y, 0f, 1f);
            color.Z = Math.Clamp(color.Z, 0f, 1f);

            return color;
        }

        private Matrix4x4 CreateProjectionMatrix(int width, int height)
        {
            float fovRadians = FieldOfView * (MathF.PI / 180f);
            float aspectRatio = (float)width / height;

            return Matrix4x4.CreatePerspectiveFieldOfView(
                fovRadians,
                aspectRatio,
                NearPlane,
                FarPlane);
        }

        private Vector3 SampleTexture(Vector3[,] textureData, Vector2 uv)
        {
            // Wrap UV coordinates to 0-1 range
            float u = uv.X - MathF.Floor(uv.X);
            float v = uv.Y - MathF.Floor(uv.Y);

            // Get texture dimensions
            int textureWidth = textureData.GetLength(0);
            int textureHeight = textureData.GetLength(1);

            // Convert UV to pixel coordinates
            int pixelX = (int)(u * (textureWidth - 1));
            int pixelY = (int)(v * (textureHeight - 1));

            // Clamp to valid range
            pixelX = Math.Clamp(pixelX, 0, textureWidth - 1);
            pixelY = Math.Clamp(pixelY, 0, textureHeight - 1);

            // Sample the texture
            return textureData[pixelX, pixelY];
        }

        private Vector2 ToScreenSpace(Vector4 clip, int w, int h)
        {
            if (clip.W <= 0.0001f)
                return Vector2.Zero;

            float invW = 1f / clip.W;

            float ndcX = clip.X * invW;
            float ndcY = clip.Y * invW;

            return new Vector2(
                (ndcX + 1f) * 0.5f * w,
                (1f - ndcY) * 0.5f * h
            );
        }


        private void SortByDepth()
        {
            ProjectedTriangles.Sort((a, b) => b.AverageDepth.CompareTo(a.AverageDepth));
            ProjectedQuads.Sort((a, b) => b.AverageDepth.CompareTo(a.AverageDepth));
        }

        private void BuildMarkup(int viewportWidth, int viewportHeight)
        {
            _htmlBuilder.Clear();

            // Merge triangles and quads into one depth-sorted list
            var allPrimitives = new List<(float depth, bool isTriangle, int index)>();

            for (int i = 0; i < ProjectedTriangles.Count; i++)
                allPrimitives.Add((ProjectedTriangles[i].AverageDepth, true, i));

            for (int i = 0; i < ProjectedQuads.Count; i++)
                allPrimitives.Add((ProjectedQuads[i].AverageDepth, false, i));

            allPrimitives.Sort((a, b) => b.depth.CompareTo(a.depth));

            foreach (var primitive in allPrimitives)
            {
                if (primitive.isTriangle)
                {
                    var tri = ProjectedTriangles[primitive.index];

                    string markup = BuildTriangleMarkup(
                        tri,
                        viewportWidth,
                        viewportHeight);

                    if (!_primitiveMarkupCache.TryGetValue(tri.PrimitiveIndex, out var cached) ||
                        cached != markup)
                    {
                        _primitiveMarkupCache[tri.PrimitiveIndex] = markup;
                    }

                    _htmlBuilder.Append(_primitiveMarkupCache[tri.PrimitiveIndex]);
                }
                else
                {
                    var quad = ProjectedQuads[primitive.index];

                    string markup = BuildQuadMarkup(
                        quad,
                        viewportWidth,
                        viewportHeight);

                    if (!_primitiveMarkupCache.TryGetValue(quad.PrimitiveIndex, out var cached) ||
                        cached != markup)
                    {
                        _primitiveMarkupCache[quad.PrimitiveIndex] = markup;
                    }

                    _htmlBuilder.Append(_primitiveMarkupCache[quad.PrimitiveIndex]);
                }
            }

            Output = new MarkupString(_htmlBuilder.ToString());
        }

        private string BuildTriangleMarkup(
    ProjectedTriangle triangle,
    int viewportWidth,
    int viewportHeight)
        {
            var sb = new StringBuilder(256);

            if (triangle.IsTextured && !string.IsNullOrEmpty(triangle.TextureDataUrl))
            {
                Vector2 A = triangle.ScreenVertex0;
                Vector2 B = triangle.ScreenVertex1;
                Vector2 C = triangle.ScreenVertex2;

                Vector2 U = B - A;
                Vector2 V = C - A;

                const float MaxScale = 2000f;
                if (U.LengthSquared() > MaxScale * MaxScale ||
                    V.LengthSquared() > MaxScale * MaxScale)
                {
                    return string.Empty;
                }

                sb.Append(
                    $"<div style=\"position:absolute;" +
                    $"left:{A.X}px;top:{A.Y}px;" +
                    $"width:1px;height:1px;" +
                    $"transform:matrix({U.X},{U.Y},{V.X},{V.Y},0,0);" +
                    $"transform-origin:0 0;" +
                    $"background-image:url('{triangle.TextureDataUrl}');" +
                    $"background-size:1px 1px;" +
                    $"background-repeat:no-repeat;" +
                    $"clip-path:polygon(0px 0px,1px 0px,0px 1px);" +
                    $"opacity:0.9;" +
                    $"will-change:transform;\"></div>"
                );

                return sb.ToString();
            }
            else
            {
                int red = (int)(triangle.Color.X * 255);
                int green = (int)(triangle.Color.Y * 255);
                int blue = (int)(triangle.Color.Z * 255);
                float alpha = triangle.Color.W;

                sb.Append(
                    $"<div style=\"position:absolute;" +
                    $"left:0;top:0;" +
                    $"width:{viewportWidth}px;height:{viewportHeight}px;" +
                    $"clip-path:polygon(" +
                    $"{triangle.ScreenVertex0.X}px {triangle.ScreenVertex0.Y}px," +
                    $"{triangle.ScreenVertex1.X}px {triangle.ScreenVertex1.Y}px," +
                    $"{triangle.ScreenVertex2.X}px {triangle.ScreenVertex2.Y}px);" +
                    $"background:rgba({red},{green},{blue},{alpha});\">" +
                    $"</div>"
                );

                return sb.ToString();
            }
        }

        private string BuildQuadMarkup(
    ProjectedQuad quad,
    int viewportWidth,
    int viewportHeight)
        {
            if (quad.IsTextured && !string.IsNullOrEmpty(quad.TextureDataUrl))
            {
                Vector2 origin = quad.ScreenVertex0;
                Vector2 U = quad.ScreenVertex1 - quad.ScreenVertex0;
                Vector2 V = quad.ScreenVertex3 - quad.ScreenVertex0;

                var sb = new StringBuilder(256);

                sb.Append(CommonDivPrefix);

                sb.Append("left:"); sb.Append(origin.X); sb.Append("px;");
                sb.Append("top:"); sb.Append(origin.Y); sb.Append("px;");
                sb.Append("width:1px;height:1px;");

                sb.Append("transform:matrix(");
                sb.Append(U.X); sb.Append(',');
                sb.Append(U.Y); sb.Append(',');
                sb.Append(V.X); sb.Append(',');
                sb.Append(V.Y);
                sb.Append(",0,0);");

                sb.Append("transform-origin:0 0;");

                sb.Append("background-image:url('");
                sb.Append(quad.TextureDataUrl);
                sb.Append("');");

                sb.Append("background-size:1px 1px;");
                sb.Append("background-repeat:no-repeat;");
                sb.Append(UnitQuadClip);
                sb.Append("opacity:0.9;will-change:transform;\"></div>");

                return sb.ToString();
            }
            else
            {
                int red = (int)(quad.Color.X * 255);
                int green = (int)(quad.Color.Y * 255);
                int blue = (int)(quad.Color.Z * 255);
                float alpha = quad.Color.W;

                return
                    $"<div style=\"position:absolute;" +
                    $"left:0;top:0;" +
                    $"width:{viewportWidth}px;height:{viewportHeight}px;" +
                    $"clip-path:polygon(" +
                    $"{quad.ScreenVertex0.X}px {quad.ScreenVertex0.Y}px," +
                    $"{quad.ScreenVertex1.X}px {quad.ScreenVertex1.Y}px," +
                    $"{quad.ScreenVertex2.X}px {quad.ScreenVertex2.Y}px," +
                    $"{quad.ScreenVertex3.X}px {quad.ScreenVertex3.Y}px);" +
                    $"background:rgba({red},{green},{blue},{alpha});\">" +
                    $"</div>";
            }
        }










        private void RenderTriangleFace(
      IMesh mesh,
      Face face,
      IReadOnlyList<SpectralXLight> lights,
      Matrix4x4 modelViewProjectionMatrix, // 👈 ADD THIS
      int screenWidth,
      int screenHeight,
      Vector3 cameraPosition)

        {
           

            // Use face indices instead of mesh.Indices[i]
            int vertexIndex0 = face.A;
            int vertexIndex1 = face.B;
            int vertexIndex2 = face.C;

            var worldVertex0 = mesh.Vertices[vertexIndex0];
            var worldVertex1 = mesh.Vertices[vertexIndex1];
            var worldVertex2 = mesh.Vertices[vertexIndex2];

            // Transform vertices to clip space (KEEP W)
            Vector4 clipSpaceVertex0 = Vector4.Transform(
                new Vector4(worldVertex0, 1f),
                modelViewProjectionMatrix
            );

            Vector4 clipSpaceVertex1 = Vector4.Transform(
                new Vector4(worldVertex1, 1f),
                modelViewProjectionMatrix
            );

            Vector4 clipSpaceVertex2 = Vector4.Transform(
                new Vector4(worldVertex2, 1f),
                modelViewProjectionMatrix
            );



            // TEMP safety: skip triangles that cross the near plane
            if (clipSpaceVertex0.W <= 0f &&
                clipSpaceVertex1.W <= 0f &&
                clipSpaceVertex2.W <= 0f)
            {
                return;
            }

            // Hybrid near-plane rescue (intentional distortion zone)
            const float MinW = 0.05f;

            clipSpaceVertex0.W = Math.Max(clipSpaceVertex0.W, MinW);
            clipSpaceVertex1.W = Math.Max(clipSpaceVertex1.W, MinW);
            clipSpaceVertex2.W = Math.Max(clipSpaceVertex2.W, MinW);


            // Now project - no faces will be rejected
            var screenVertex0 = ToScreenSpace(clipSpaceVertex0, screenWidth, screenHeight);
            var screenVertex1 = ToScreenSpace(clipSpaceVertex1, screenWidth, screenHeight);
            var screenVertex2 = ToScreenSpace(clipSpaceVertex2, screenWidth, screenHeight);


            // Calculate triangle center for lighting
            Vector3 triangleCenter = (worldVertex0 + worldVertex1 + worldVertex2) / 3f;

            // Get or calculate triangle normal
            Vector3 triangleNormal;
            if (mesh.Normals != null &&
                mesh.Normals.Count > vertexIndex0 &&
                mesh.Normals.Count > vertexIndex1 &&
                mesh.Normals.Count > vertexIndex2)
            {
                triangleNormal = (mesh.Normals[vertexIndex0] + mesh.Normals[vertexIndex1] + mesh.Normals[vertexIndex2]) / 3f;
                triangleNormal = Vector3.Normalize(triangleNormal);
            }
            else
            {
                // Calculate face normal from vertices
                Vector3 edge1 = worldVertex1 - worldVertex0;
                Vector3 edge2 = worldVertex2 - worldVertex0;
                triangleNormal = Vector3.Normalize(Vector3.Cross(edge1, edge2));
            }

          
            Vector4 finalColor;

            if (mesh.HasTexture && mesh.TextureData != null)
            {
                Vector3 sampled = SampleTexture(
                    mesh.TextureData,
                    (mesh.UVs[vertexIndex0] + mesh.UVs[vertexIndex1] + mesh.UVs[vertexIndex2]) / 3f
                );

                finalColor = new Vector4(
                    sampled.X,
                    sampled.Y,
                    sampled.Z,
                    mesh.Color.W // alpha from material
                );
            }
            else
            {
                var lighting = CalculateLighting(triangleCenter, triangleNormal, lights);

                finalColor = new Vector4(
                    mesh.Color.X * lighting.X,
                    mesh.Color.Y * lighting.Y,
                    mesh.Color.Z * lighting.Z,
                    mesh.Color.W
                );
            }


            string triangleTextureUrl = null;
            if (mesh.HasTexture && mesh.TextureData != null && mesh.UVs.Count > vertexIndex2)
            {
                triangleTextureUrl = mesh.TextureDataUrl;
            }
       

        ProjectedTriangles.Add(new ProjectedTriangle
            {
                PrimitiveIndex = face.PrimitiveIndex, // 🔥 KEY LINE
                ScreenVertex0 = screenVertex0,
                ScreenVertex1 = screenVertex1,
                ScreenVertex2 = screenVertex2,
                AverageDepth = Math.Max(
             Math.Max(clipSpaceVertex0.Z, clipSpaceVertex1.Z),
             clipSpaceVertex2.Z),
                Color = finalColor,
                IsTextured = mesh.HasTexture && mesh.TextureData != null,
                TextureDataUrl = triangleTextureUrl,
                UV0 = mesh.UVs.Count > vertexIndex0 ? mesh.UVs[vertexIndex0] : Vector2.Zero,
                UV1 = mesh.UVs.Count > vertexIndex1 ? mesh.UVs[vertexIndex1] : Vector2.Zero,
                UV2 = mesh.UVs.Count > vertexIndex2 ? mesh.UVs[vertexIndex2] : Vector2.Zero
            });


            TrianglesRendered++;
        }



        private void RenderQuadFace(
     IMesh mesh,
     Face face,
     IReadOnlyList<SpectralXLight> lights,
     Matrix4x4 modelViewProjectionMatrix, // 👈 ADD THIS
     int screenWidth,
     int screenHeight,
     Vector3 cameraPosition)

        {


            // Get all 4 vertex indices
            int vertexIndex0 = face.A;
            int vertexIndex1 = face.B;
            int vertexIndex2 = face.C;
            int vertexIndex3 = face.D;

            var worldVertex0 = mesh.Vertices[vertexIndex0];
            var worldVertex1 = mesh.Vertices[vertexIndex1];
            var worldVertex2 = mesh.Vertices[vertexIndex2];
            var worldVertex3 = mesh.Vertices[vertexIndex3];

            // Transform vertices to clip space (KEEP W)
            Vector4 clipSpaceVertex0 = Vector4.Transform(
                new Vector4(worldVertex0, 1f),
                modelViewProjectionMatrix
            );

            Vector4 clipSpaceVertex1 = Vector4.Transform(
                new Vector4(worldVertex1, 1f),
                modelViewProjectionMatrix
            );

            Vector4 clipSpaceVertex2 = Vector4.Transform(
                new Vector4(worldVertex2, 1f),
                modelViewProjectionMatrix
            );

            Vector4 clipSpaceVertex3 = Vector4.Transform(
                new Vector4(worldVertex3, 1f),
                modelViewProjectionMatrix
            );

            if (clipSpaceVertex0.W <= 0f && clipSpaceVertex1.W <= 0f && clipSpaceVertex2.W <= 0f && clipSpaceVertex3.W <= 0f)
                return;
            // Hybrid near-plane rescue (intentional distortion zone)
            const float MinW = 0.05f;

            clipSpaceVertex0.W = Math.Max(clipSpaceVertex0.W, MinW);
            clipSpaceVertex1.W = Math.Max(clipSpaceVertex1.W, MinW);
            clipSpaceVertex2.W = Math.Max(clipSpaceVertex2.W, MinW);
            clipSpaceVertex3.W= Math.Max(clipSpaceVertex3.W, MinW);

            // Now project - no faces will be rejected
            var screenVertex0 = ToScreenSpace(clipSpaceVertex0, screenWidth, screenHeight);
            var screenVertex1 = ToScreenSpace(clipSpaceVertex1, screenWidth, screenHeight);
            var screenVertex2 = ToScreenSpace(clipSpaceVertex2, screenWidth, screenHeight);
            var screenVertex3 = ToScreenSpace(clipSpaceVertex3, screenWidth, screenHeight);


            // Calculate quad center for lighting
            Vector3 quadCenter = (worldVertex0 + worldVertex1 + worldVertex2 + worldVertex3) / 4f;

            // Get or calculate quad normal
            Vector3 quadNormal;
            if (mesh.Normals != null &&
                mesh.Normals.Count > vertexIndex0 &&
                mesh.Normals.Count > vertexIndex1 &&
                mesh.Normals.Count > vertexIndex2 &&
                mesh.Normals.Count > vertexIndex3)
            {
                quadNormal = (mesh.Normals[vertexIndex0] + mesh.Normals[vertexIndex1] +
                              mesh.Normals[vertexIndex2] + mesh.Normals[vertexIndex3]) / 4f;
                quadNormal = Vector3.Normalize(quadNormal);
            }
            else
            {
                // Calculate face normal from first 3 vertices
                Vector3 edge1 = worldVertex1 - worldVertex0;
                Vector3 edge2 = worldVertex2 - worldVertex0;
                quadNormal = Vector3.Normalize(Vector3.Cross(edge1, edge2));
            }


         
            Vector4 finalColor;

            if (mesh.HasTexture && mesh.TextureData != null)
            {
                Vector3 sampled = SampleTexture(
                    mesh.TextureData,
                    (mesh.UVs[vertexIndex0] +
                     mesh.UVs[vertexIndex1] +
                     mesh.UVs[vertexIndex2] +
                     mesh.UVs[vertexIndex3]) / 4f
                );

                finalColor = new Vector4(
                    sampled.X,
                    sampled.Y,
                    sampled.Z,
                    mesh.Color.W // alpha from material
                );
            }
            else
            {
                var lighting = CalculateLighting(quadCenter, quadNormal, lights);

                finalColor = new Vector4(
                    mesh.Color.X * lighting.X,
                    mesh.Color.Y * lighting.Y,
                    mesh.Color.Z * lighting.Z,
                    mesh.Color.W
                );
            }

            string quadTextureUrl = null;
            if (mesh.HasTexture && mesh.TextureData != null && mesh.UVs.Count > vertexIndex3)
            {
                quadTextureUrl = mesh.TextureDataUrl;
            }

            ProjectedQuads.Add(new ProjectedQuad
            {
                PrimitiveIndex = face.PrimitiveIndex, // 🔥 KEY LINE
                ScreenVertex0 = screenVertex0,
                ScreenVertex1 = screenVertex1,
                ScreenVertex2 = screenVertex2,
                ScreenVertex3 = screenVertex3,
                AverageDepth = Math.Max(
           Math.Max(clipSpaceVertex0.Z, clipSpaceVertex1.Z),
           Math.Max(clipSpaceVertex2.Z, clipSpaceVertex3.Z)),
                Color = finalColor,
                IsTextured = mesh.HasTexture && mesh.TextureData != null,
                TextureDataUrl = quadTextureUrl,
                UV0 = mesh.UVs.Count > vertexIndex0 ? mesh.UVs[vertexIndex0] : Vector2.Zero,
                UV1 = mesh.UVs.Count > vertexIndex1 ? mesh.UVs[vertexIndex1] : Vector2.Zero,
                UV2 = mesh.UVs.Count > vertexIndex2 ? mesh.UVs[vertexIndex2] : Vector2.Zero,
                UV3 = mesh.UVs.Count > vertexIndex3 ? mesh.UVs[vertexIndex3] : Vector2.Zero
            });


            QuadsRendered++; // Add this counter too
        }



        public class ProjectedTriangle
        {
            public Vector2 ScreenVertex0 { get; set; }
            public Vector2 ScreenVertex1 { get; set; }
            public Vector2 ScreenVertex2 { get; set; }
            public float AverageDepth { get; set; }
            public Vector4 Color { get; set; } = Vector4.One;
            public bool IsTextured { get; set; }
            public string TextureDataUrl { get; set; }
            public Vector2 UV0 { get; set; }
            public Vector2 UV1 { get; set; }
            public Vector2 UV2 { get; set; }
            public int PrimitiveIndex { get; set; }
        }

        public class ProjectedQuad
        {
            public Vector2 ScreenVertex0 { get; set; }
            public Vector2 ScreenVertex1 { get; set; }
            public Vector2 ScreenVertex2 { get; set; }
            public Vector2 ScreenVertex3 { get; set; }
            public float AverageDepth { get; set; }
            public Vector4 Color { get; set; } = Vector4.One;
            public bool IsTextured { get; set; }
            public string TextureDataUrl { get; set; }
            public Vector2 UV0 { get; set; }
            public Vector2 UV1 { get; set; }
            public Vector2 UV2 { get; set; }
            public Vector2 UV3 { get; set; }
            public int PrimitiveIndex { get; set; }
        }


    }
}