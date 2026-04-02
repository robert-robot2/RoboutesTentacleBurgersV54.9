
namespace SpectralXGLX.SpectralGL
{
    public class SpectralGLTexture
    {
        public int Id { get; }
        public int Width { get; }
        public int Height { get; }

        public SpectralGLTexture(int id, int width, int height)
        {
            Id = id;
            Width = width;
            Height = height;
        }
    }
}
