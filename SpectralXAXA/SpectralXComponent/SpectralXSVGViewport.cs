
namespace SpectralXAXA.SpectralXComponent
{

    public class SpectralXSVGViewport
    {
        public int SVGViewportWidth { get; private set; } = 320;
        public int SVGViewportHeight { get; private set; } = 240;
        public string BackgroundColor { get; set; } = "black";

        // Store rendered triangles for display
        public List<SpectralXRenderer.ProjectedTriangle> RenderedTriangles { get; private set; } = new();

        public string Style =>
            $"position:relative;" +
            $"width:{SVGViewportWidth}px;" +
            $"height:{SVGViewportHeight}px;" +
            $"background-color:{BackgroundColor};" +
            $"overflow:hidden;" +
            $"transition: all 0.4s ease-in-out;";

        public void SetSize(int width, int height)
        {
            if (width <= 0 || height <= 0) return;
            SVGViewportWidth = width;
            SVGViewportHeight = height;
        }

        /// <summary>
        /// Update the viewport with rendered triangles from the renderer
        /// Called by Engine after rendering
        /// </summary>
        public void UpdateRenderedTriangles(List<SpectralXRenderer.ProjectedTriangle> triangles)
        {
            RenderedTriangles = triangles;
        }

        /// <summary>
        /// Legacy DrawMesh method - kept for backwards compatibility
        /// Use SpectralXRenderer directly for new code
        /// </summary>
        [Obsolete("Use SpectralXRenderer.RenderMesh() instead")]
        public void DrawMesh(IMesh mesh, System.Numerics.Matrix4x4 viewMatrix)
        {
            Console.WriteLine("[SpectralXViewport] DrawMesh is deprecated. Use SpectralXRenderer instead.");
        }
    }
}