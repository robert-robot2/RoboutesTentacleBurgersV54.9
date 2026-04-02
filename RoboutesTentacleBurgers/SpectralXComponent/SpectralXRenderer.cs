using RoboutesTentacleBurgers.SpectralGL;
using RoboutesTentacleBurgers.SpectralGL.Backend.Cpu;
using RoboutesTentacleBurgers.SpectralGL.Math;
using RoboutesTentacleBurgers.SpectralXComponent.SpectralXRender;
using System.Numerics;
using SysMath = System.Math;

namespace RoboutesTentacleBurgers.SpectralXComponent
{
    public class SpectralXRenderer
    {
        private SpectralGLCpuBackend _glBackend;
        private SpectralGLProgram _defaultProgram;
        private SpectralGLTexture _defaultTexture;
        private SpectralGLFramebuffer _glFramebuffer;

        public float FieldOfViewDegrees { get; set; } = 90f;
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 100f;
        public Vec3 AmbientLight { get; set; } = new Vec3(0.2f, 0.2f, 0.2f);

        public int MeshesProcessed { get; private set; }
        public int VerticesTransformed { get; private set; }

        public SpectralGLProgram DefaultProgram => _defaultProgram;
        public SpectralGLTexture DefaultTexture => _defaultTexture;

        public void InitSpectralGL(int width, int height)
        {
            _glBackend = new SpectralGLCpuBackend(width, height);
            _defaultTexture = new SpectralGLTexture(1, width, height);
            var vs = new SpectralGLShader(1, "// vertex shader placeholder");
            var fs = new SpectralGLShader(2, "// fragment shader placeholder");
            _defaultProgram = new SpectralGLProgram(1, vs, fs);
            _glFramebuffer = new SpectralGLFramebuffer(1, _defaultTexture);
        }

        public void Resize(int width, int height)
        {
            _glBackend?.Resize(width, height);
        }

        public FrameData GetFrameData()
        {
            if (_glBackend == null)
                return new FrameData { Width = 0, Height = 0, Data = Array.Empty<byte>() };

            return _glBackend.GetFrameData();
        }

        public void RenderSceneToSpectralGL(
            SpectralXScene scene,
            SpectralXCamera camera,
            SpectralXViewport viewport)
        {
            if (_glBackend == null) return;

            MeshesProcessed = 0;
            VerticesTransformed = 0;

            int width = _glBackend.Framebuffer.Width;
            int height = _glBackend.Framebuffer.Height;
            var colorBuf = _glBackend.Framebuffer.ColorBuffer;

            // Clear color to black
            for (int i = 0; i < colorBuf.Length; i++)
                colorBuf[i] = unchecked((int)0xFF000000);

            /*
            // TEMP: hardcoded screen-space triangle test
            _glBackend.Rasterizer.RasterizeTriangle(
                400, 100, 0.5f,
                200, 500, 0.5f,
                600, 500, 0.5f,
                unchecked((int)0xFF00FF00),
                colorBuf,
                _glBackend.DepthBuffer.Buffer,
                width, height);
            // Clear depth
            */
            _glBackend.DepthBuffer.Clear();

            // Build VP matrix
            Mat4 view = camera.GetViewMatrix();
            Mat4 proj = Mat4.CreatePerspective(
                FieldOfViewDegrees * (MathF.PI / 180f),
                (float)viewport.ViewportWidth / viewport.ViewportHeight,
                NearPlane,
                FarPlane);
            Mat4 vp = proj * view;

            // Draw each mesh
            foreach (var mesh in scene.Meshes)
            {
                if (mesh == null) continue;
                ProcessMesh(mesh, vp);
            }
        }

        private void ProcessMesh(IMesh mesh, Mat4 vp)
        {
            MeshesProcessed++;

            int width = _glBackend.Framebuffer.Width;
            int height = _glBackend.Framebuffer.Height;
            var colorBuf = _glBackend.Framebuffer.ColorBuffer;
            var depthBuf = _glBackend.DepthBuffer.Buffer;

            Mat4 mvp = vp * mesh.WorldMatrix;

            // Pack mesh color to ARGB int
            var c = mesh.Color;
            byte r = (byte)(SysMath.Clamp(c.X, 0f, 1f) * 255);
            byte g = (byte)(SysMath.Clamp(c.Y, 0f, 1f) * 255);
            byte b = (byte)(SysMath.Clamp(c.Z, 0f, 1f) * 255);
            int packedColor = unchecked((int)(0xFF000000 | ((uint)r << 16) | ((uint)g << 8) | b));

            // Project all vertices to screen space
            var screenVerts = new (int x, int y, float z)[mesh.Vertices.Count];
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var v = mesh.Vertices[i];
                Vec3 clip = Mat4.TransformPoint(mvp, new Vec3(v.X, v.Y, v.Z));
                int sx = (int)((clip.X * 0.5f + 0.5f) * width);
                int sy = (int)((-clip.Y * 0.5f + 0.5f) * height);
                screenVerts[i] = (sx, sy, clip.Z);
                VerticesTransformed++;
            }

            // Rasterize each face
            foreach (var face in mesh.Faces)
            {
                var (x0, y0, z0) = screenVerts[face.A];
                var (x1, y1, z1) = screenVerts[face.B];
                var (x2, y2, z2) = screenVerts[face.C];

                _glBackend.Rasterizer.RasterizeTriangle(
                    x0, y0, z0,
                    x1, y1, z1,
                    x2, y2, z2,
                    packedColor, colorBuf, depthBuf, width, height);

                if (face.Type == FaceType.Quad)
                {
                    var (x3, y3, z3) = screenVerts[face.D];
                    _glBackend.Rasterizer.RasterizeTriangle(
                        x0, y0, z0,
                        x2, y2, z2,
                        x3, y3, z3,
                        packedColor, colorBuf, depthBuf, width, height);
                }
            }
        }
    }
}