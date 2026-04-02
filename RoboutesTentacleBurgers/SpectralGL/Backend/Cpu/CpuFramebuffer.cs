


/*
    CpuFramebuffer
    ----------------
    CPU-side framebuffer used by the software renderer.

    - ColorBuffer: Internal ARGB-packed int buffer written by the CPU renderer.
      Format: 0xAARRGGBB (alpha MUST be non-zero for visible pixels).

    - RgbaBuffer: Byte buffer exposed to JS/WebGL.
      Populated via ConvertToRgba() as RGBA8 for texture upload.

    Contract:
    - Renderer writes to ColorBuffer.
    - ConvertToRgba() must be called before JS reads RgbaBuffer.
    - This class does NOT perform Y-flipping or alpha correction.
*/




namespace RoboutesTentacleBurgers.SpectralGL.Backend.Cpu
{
    public class CpuFramebuffer
    {
        public int Width { get; }
        public int Height { get; }

        // Internal ARGB buffer (renderer writes here)
        public int[] ColorBuffer { get; }

        // JS-facing RGBA buffer
        public byte[] RgbaBuffer { get; }

        public CpuFramebuffer(int width, int height)
        {
            Width = width;
            Height = height;

            ColorBuffer = new int[width * height];
            RgbaBuffer = new byte[width * height * 4];
        }

        // Convert ARGB → RGBA for JS upload
        public void ConvertToRgba()
        {
            int p = 0;

            for (int i = 0; i < ColorBuffer.Length; i++)
            {
                int argb = ColorBuffer[i];

                byte a = (byte)((argb >> 24) & 0xFF);
                byte r = (byte)((argb >> 16) & 0xFF);
                byte g = (byte)((argb >> 8) & 0xFF);
                byte b = (byte)(argb & 0xFF);

                RgbaBuffer[p++] = r;
                RgbaBuffer[p++] = g;
                RgbaBuffer[p++] = b;
                RgbaBuffer[p++] = a;
            }
        }
    }

}
