

namespace SpectralXGLX.SpectralGL.Backend.Cpu
{
    public static class CpuMathHelpers
    {
        public static float Clamp(float v, float min, float max)
            => v < min ? min : (v > max ? max : v);
    }
}
