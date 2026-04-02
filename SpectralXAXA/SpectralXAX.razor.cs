
namespace SpectralXAXA
{
    public partial class SpectralXAX : IDisposable
    {
        public SpectralXViewport Viewport { get; set; } = new();
        public SpectralXSVGViewport SVGViewport { get; set; } = new();
        public SpectralXScene Scene => Engine.Scene;
        public SpectralXMeshLibrary MeshLibrary => Engine.MeshLibrary;
        public SpectralXRenderer Renderer => Engine.Renderer;
        public SpectralXEngine? Engine;
        public SpectralXInput? Input;
        public SpectralXCamera? Camera;

        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!; // ← ADD THIS

        public bool IsFullscreen { get; set; } = false;
        private string ToggleIcon => IsFullscreen ? "🗗" : "⬜";
        private SpectralXScene2 Scene2;

      
        protected override async Task OnInitializedAsync()
        {
            Scene2 = new SpectralXScene2();
            Camera = new SpectralXCamera();
            Engine = new SpectralXEngine(this, Viewport,SVGViewport, Camera, JS);
            Input = new SpectralXInput(this, Viewport,SVGViewport, Camera, JS);         
            await Engine.MeshLibrary.LoadFromFBXParserAsync(Http, "iMeshes/Cube.fbx", "FBXCube");
            Engine.Init();
            Engine.Start();
            //removed because caused error with fullscreen toggle
            /*
               try
            {
                Console.WriteLine("[SpectralX] Preloading Cube.fbx (Custom Parser)...");
                var mesh = await Engine.MeshLibrary.LoadFromFBXParserAsync(Http, "iMeshes/Cube.fbx", "FBXCube");

                if (mesh != null)
                {
                    Console.WriteLine("[SpectralX] Cube.fbx parsed + loaded into library!");
                }
                else
                {
                    Console.WriteLine("[SpectralX] Cube.fbx failed to load!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SpectralX] FBX preload failed: {ex.Message}");
            }
              
             
             */
        }


        public async Task ToggleViewport()
        {
            await JS.InvokeVoidAsync("toggleFullscreen", "SpectralX-Viewport");
            IsFullscreen = !IsFullscreen;
            await Task.Delay(100);

            if (IsFullscreen)
            {
                var size = await JS.InvokeAsync<ViewportSize>("getViewportSize", "SpectralX-Viewport");
                if (size != null)
                    Viewport.SetSize(size.Width, size.Height);
            }
            if (!IsFullscreen)
            {
                Viewport.SetSize(800, 600);
            }

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

        public void Dispose()
        {
            Engine?.Stop();
            Engine?.Dispose();
        }
    }
}