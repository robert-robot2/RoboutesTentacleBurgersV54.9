namespace SpectralXGLX
{
    public partial class SpectralXLab2 
    {
        public SpectralXViewport Viewport { get; set; } = new();
        public SpectralXRenderer Renderer { get; set; } = new();

        public SpectralXScene Scene { get; set; } = new();
        public SpectralXMeshLibrary MeshLibrary { get; set; } = new();

        public SpectralXEngine? Engine;
        public SpectralXInput? Input;
        public SpectralXCamera? Camera;

        

        [Inject] private IJSRuntime JS { get; set; } = default!;

        public bool IsFullscreen { get; set; } = false;
        private string ToggleIcon => IsFullscreen ? "🗗" : "⬜";

        /*

        protected override void OnInitialized()
        {
            Camera = new SpectralXCamera();

            Engine = new SpectralXEngine(this, Viewport, Camera, JS);

            Input = new SpectralXInput(this, Viewport, Camera, JS);  // Pass Camera and Viewport


            Engine.Init();
            Engine.Start();
        }
        */

        public async Task ToggleViewport()
        {
            await JS.InvokeVoidAsync("toggleFullscreen", "SpectralX-Viewport");
            IsFullscreen = !IsFullscreen;
            StateHasChanged();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await Input.Register();

                await Task.Delay(50);
                Engine.Tick();
                StateHasChanged();
            }

        }

        public class ViewportSize
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }

     
    }
}
