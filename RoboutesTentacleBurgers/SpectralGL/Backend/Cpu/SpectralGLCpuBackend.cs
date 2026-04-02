





namespace RoboutesTentacleBurgers.SpectralGL.Backend.Cpu
{
    public class SpectralGLCpuBackend
    {
        public CpuFramebuffer Framebuffer { get; private set; }
        public CpuDepthBuffer DepthBuffer { get; private set; }
        public CpuRasterizer Rasterizer { get; }
        public CpuVertexProcessor VertexProcessor { get; }
        public CpuFragmentProcessor FragmentProcessor { get; }

        public SpectralGLCpuBackend(int width, int height)
        {
            Framebuffer = new CpuFramebuffer(width, height);
            DepthBuffer = new CpuDepthBuffer(width, height);
            Rasterizer = new CpuRasterizer();
            VertexProcessor = new CpuVertexProcessor();
            FragmentProcessor = new CpuFragmentProcessor();
        }

        // ⭐ Objective 5: Resize framebuffer + depth buffer
        public void Resize(int width, int height)
        {
            Framebuffer = new CpuFramebuffer(width, height);
            DepthBuffer = new CpuDepthBuffer(width, height);
        }

        // ⭐ Objective 5: Expose framebuffer to JS
        public FrameData GetFrameData()
        {
            Framebuffer.ConvertToRgba();

            return new FrameData
            {
                Width = Framebuffer.Width,
                Height = Framebuffer.Height,
                Data = Framebuffer.RgbaBuffer
            };
        }




    }
}
