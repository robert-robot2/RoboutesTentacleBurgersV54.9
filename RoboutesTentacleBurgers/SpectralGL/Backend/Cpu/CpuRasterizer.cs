using SysMath = System.Math;

namespace RoboutesTentacleBurgers.SpectralGL.Backend.Cpu
{
    public class CpuRasterizer
    {
        public void RasterizeTriangle(
            int x0, int y0, float z0,
            int x1, int y1, float z1,
            int x2, int y2, float z2,
            int color,
            int[] colorBuffer,
            float[] depthBuffer,
            int width, int height)
        {
            // Bounding box
            int minX = SysMath.Max(0, SysMath.Min(x0, SysMath.Min(x1, x2)));
            int maxX = SysMath.Min(width - 1, SysMath.Max(x0, SysMath.Max(x1, x2)));
            int minY = SysMath.Max(0, SysMath.Min(y0, SysMath.Min(y1, y2)));
            int maxY = SysMath.Min(height - 1, SysMath.Max(y0, SysMath.Max(y1, y2)));

            // Edge function helper
            int Edge(int ax, int ay, int bx, int by, int px, int py)
                => (bx - ax) * (py - ay) - (by - ay) * (px - ax);

            int area = Edge(x0, y0, x1, y1, x2, y2);
            if (area == 0) return; // Degenerate triangle

            for (int py = minY; py <= maxY; py++)
            {
                for (int px = minX; px <= maxX; px++)
                {
                    int w0 = Edge(x1, y1, x2, y2, px, py);
                    int w1 = Edge(x2, y2, x0, y0, px, py);
                    int w2 = Edge(x0, y0, x1, y1, px, py);

                    if (w0 >= 0 && w1 >= 0 && w2 >= 0)
                    {
                        // Barycentric depth interpolation
                        float bc0 = (float)w0 / area;
                        float bc1 = (float)w1 / area;
                        float bc2 = (float)w2 / area;
                        float depth = bc0 * z0 + bc1 * z1 + bc2 * z2;

                        int idx = py * width + px;
                        if (depth < depthBuffer[idx])
                        {
                            depthBuffer[idx] = depth;
                            colorBuffer[idx] = color;
                        }
                    }
                }
            }
        }
    }
}