namespace SpectralXGLX.SpectralGL

{
    public class SpectralGLShader
    {
        public int Id { get; }
        public string Source { get; }

        public SpectralGLShader(int id, string source)
        {
            Id = id;
            Source = source;
        }
    }
}
