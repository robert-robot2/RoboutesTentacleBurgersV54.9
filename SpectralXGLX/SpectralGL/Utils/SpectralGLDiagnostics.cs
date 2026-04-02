namespace SpectralXGLX.SpectralGL.Utils
{
    public class SpectralGLDiagnostics
    {
        public int VerticesSubmitted;
        public int VerticesTransformed;
        public int TrianglesRasterized;
        public int PixelsDrawn;

        public double VertexStageMs;
        public double RasterStageMs;
        public double FragmentStageMs;

        public void Reset()
        {
            VerticesSubmitted = 0;
            VerticesTransformed = 0;
            TrianglesRasterized = 0;
            PixelsDrawn = 0;

            VertexStageMs = 0;
            RasterStageMs = 0;
            FragmentStageMs = 0;
        }
    }

}
