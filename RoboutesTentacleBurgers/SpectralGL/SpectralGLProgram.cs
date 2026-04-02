namespace RoboutesTentacleBurgers.SpectralGL

{
    public class SpectralGLProgram
    {
        public int Id { get; }
        public SpectralGLShader VertexShader { get; }
        public SpectralGLShader FragmentShader { get; }

        public SpectralGLProgram(int id, SpectralGLShader vs, SpectralGLShader fs)
        {
            Id = id;
            VertexShader = vs;
            FragmentShader = fs;
        }
    }
}
