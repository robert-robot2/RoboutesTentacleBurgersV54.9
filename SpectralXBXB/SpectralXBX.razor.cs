




namespace SpectralXBXB
{
    public partial class SpectralXBX : IDisposable
    {
        public SpectralXViewport Viewport { get; set; } = new();

        public SpectralXEngine Engine { get; private set; } = default!;
        //can be one?
       DotNetObjectReference<SpectralXEngine>? _engineRef;
       
 public SpectralXCamera Camera { get; private set; } = default!;



        public SpectralXInput Input { get; private set; } = default!;

       public SpectralXScene Scene => Engine.Scene;
        public SpectralXMeshLibrary MeshLibrary => Engine.MeshLibrary;
        public SpectralXRenderer Renderer => Engine.Renderer;

        public SpectralXDebugRender Debug { get; private set; } = default!;

  
        // this errors on page load
        [Inject] private GamepadService2 Gamepad2 { get; set; } = default!;

      [Inject] private IJSRuntime JS { get; set; } = default!;
       [Inject] private HttpClient Http { get; set; } = default!;

        public bool IsFullscreen { get; set; } = false;
        private string ToggleIcon => IsFullscreen ? "🗗" : "⬜";

        bool _loopStarted;

        protected override async Task OnInitializedAsync()
        {
            
 Camera = new SpectralXCamera();

            
            Input = new SpectralXInput(this, Viewport, Camera, JS);
            
            Engine = new SpectralXEngine(
                this,
                Viewport,
                Camera,
                Input,
                Gamepad2,// ← Pass the Input instance
                JS
            );
            



            Debug = new SpectralXDebugRender(
                Engine,
                Viewport,
                Input,
                Camera,
                MeshLibrary
            );

            Input.Debug = Debug;


            try
            {
                // Load all meshes
                await Engine.MeshLibrary.LoadFromFBXParserAsync(Http, "iMeshes/Cube.fbx", "FBXCube");
                await Engine.MeshLibrary.LoadFromFBXParserAsync(Http, "iMeshes/Sphere.fbx", "FBXSphere");
                await Engine.MeshLibrary.LoadFromFBXParserAsync(Http, "iMeshes/Pyramid.fbx", "FBXPyramid");
                await Engine.MeshLibrary.LoadFromFBXParserAsync(Http, "iMeshes/PyramidT.fbx", "FBXPyramidT");
                await Engine.MeshLibrary.LoadFromFBXParserAsync(Http, "iMeshes/IsoPyramid.fbx", "FBXIsoPyramid");
                await Engine.MeshLibrary.LoadFromFBXParserAsync(Http, "iMeshes/IsoPyramidT.fbx", "FBXIsoPyramidT");
                await Engine.MeshLibrary.LoadFromFBXParserAsync(Http, "iMeshes/BullDozerBox.fbx", "FBXDozerBox");
                await Engine.MeshLibrary.LoadFromFBXParserAsync(Http, "iMeshes/BullDozerBox2.fbx", "FBXDozerBox2");
                await Engine.MeshLibrary.LoadFromFBXParserAsync(Http, "iMeshes/BrickBox.fbx", "BrickBox");
                await Engine.MeshLibrary.LoadFromFBXParserAsync(Http, "iMeshes/TriangleT.fbx", "TriT");
                await Engine.MeshLibrary.LoadFromFBXParserAsync(Http, "iMeshes/Hex002.fbx", "HexCyl");
                await Engine.MeshLibrary.LoadFromFBXParserAsync(Http, "iMeshes/JibbaCola.fbx", "ColaSquare");
            }
            catch (Exception ex)
            {
                Console.WriteLine("LOAD FAILED:");
                Console.WriteLine(ex.ToString());
            }
       


            Engine.Init();
            
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            
            if (firstRender && !_loopStarted)
            {
                _engineRef = DotNetObjectReference.Create(Engine);
                await JS.InvokeVoidAsync("spectralXLoop", _engineRef);
                _loopStarted = true;
              await  Gamepad2.InitAsync();

              await  Input.Register();

            }
            
        }

        public void RequestRender()
        {
            InvokeAsync(StateHasChanged);
        }
        public async Task ToggleViewport()
        {
           
            await JS.InvokeVoidAsync("toggleFullscreen", "SpectralX-Viewport");

            IsFullscreen = !IsFullscreen;
            await Task.Delay(100);

            if (IsFullscreen)
            {
                var size = await JS.InvokeAsync<ViewportSize>(
                    "getViewportSize",
                    "SpectralX-Viewport"
                );

                if (size != null)
                    Viewport.SetSize(size.Width, size.Height);
            }
            else
            {
                Viewport.SetSize(800, 600);
            }

            StateHasChanged();
            
        }
       

        public void Dispose()
        {
           _engineRef?.Dispose();
        }


        public class ViewportSize
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }

    




    }
}
