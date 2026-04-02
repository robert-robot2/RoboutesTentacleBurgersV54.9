using SpectralXGLX.SpectralGL;

namespace RoboutesTentacleBurgers.SpectralGL

{
    public class SpectralGLFramebuffer
    {
        public int Id { get; }
        public SpectralGLTexture ColorAttachment { get; }

        public SpectralGLFramebuffer(int id, SpectralGLTexture color)
        {
            Id = id;
            ColorAttachment = color;
        }
    }
}
