namespace RoboutesTentacleBurgers.SpectralGL.Backend.Cpu

{
    public class CpuDepthBuffer
    {
        public float[] Buffer { get; }

        public CpuDepthBuffer(int width, int height)
        {
            Buffer = new float[width * height];
        }

        public void Clear()
        {
            for (int i = 0; i < Buffer.Length; i++)
                Buffer[i] = float.MaxValue;
        }
    }
}
