


namespace SpectralXGLX.SpectralGL
{
    public class SpectralGLCapabilities
    {
        public string BackendName { get; set; } = "CPU";
        public bool SupportsDepth { get; set; } = true;
        public bool SupportsTextures { get; set; } = true;
        public int FramebufferWidth { get; set; } = 800;
        public int FramebufferHeight { get; set; } = 600;
    }
}
