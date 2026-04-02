namespace SpectralXGLX.SpectralXComponent
{
    public class SpectralXViewport
    {
        public int ViewportWidth { get; private set; } = 1024;
        public int ViewportHeight { get; private set; } = 768;
        public string BackgroundColor { get; set; } = "black";

        public string Style =>
            $"position:relative;" +
            $"width:{ViewportWidth}px;" +
            $"height:{ViewportHeight}px;" +
            $"background-color:{BackgroundColor};" +
            $"overflow:hidden;" +
            $"transition: all 0.4s ease-in-out;";

        public void SetSize(int width, int height)
        {
            if (width <= 0 || height <= 0) return;

            ViewportWidth = width;
            ViewportHeight = height;

            // Optional: call JS to resize <canvas> immediately
            // SpectralGLInterop.resizeCanvas(width, height); // called from Blazor page via IJSRuntime
        }
    }
}
