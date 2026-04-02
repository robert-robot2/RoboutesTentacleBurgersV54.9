namespace SpectralXBXB.SpectralXComponent
{
    public class SpectralXViewport
    {
        public int ViewportWidth { get; private set; } = 800;
        public int ViewportHeight { get; private set; } = 600;
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
        }
    }
}
