using static SpectralXGLX.SpectralXComponent.SpectralXEngine;

namespace SpectralXGLX
{
    public partial class SpectralXGLX : IDisposable
    {
        public SpectralXViewport Viewport { get; set; } = new();

        public SpectralXEngine Engine { get; private set; } = default!;
        private DotNetObjectReference<SpectralXEngine>? _engineRef;

        public SpectralXCamera Camera { get; private set; } = default!;
        public SpectralXInput Input { get; private set; } = default!;
        public SpectralXScene Scene => Engine.Scene;
        public SpectralXMeshLibrary MeshLibrary => Engine.MeshLibrary;
        public SpectralXRenderer Renderer => Engine.Renderer;

        public SpectralXDebugRender Debug { get; private set; } = default!;

        [Inject] private GamepadService Gamepad { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;

        private ElementReference CanvasRef;

        public bool IsFullscreen { get; set; } = false;
        private string ToggleIcon => IsFullscreen ? "🗗" : "⬜";

        private bool _loopStarted;
        private bool _savedFlash = false;
        private bool _uiHidden = false;  // ADD — press 4 to toggle all UI

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("[SpectralX] OnInitializedAsync fired");
            Camera = new SpectralXCamera();
            Input = new SpectralXInput(this, Viewport, Camera, JS);

            Engine = new SpectralXEngine(
                this,
                Viewport,
                Camera,
                Input,
                Gamepad,
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
        }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Mesh loading moved here — JS interop fully available
                try
                {
                    // Single JS call — all 30 meshes fetch and upload in parallel
                    // inside the browser via Promise.all, no WASM boundary per mesh
                    var meshList = new[]
                    {
        new { url = "iMeshes/Cube.fbx",          name = "FBXCube" },
        new { url = "iMeshes/Sphere.fbx",         name = "FBXSphere" },
        new { url = "iMeshes/Pyramid.fbx",        name = "FBXPyramid" },
        new { url = "iMeshes/PyramidT.fbx",       name = "FBXPyramidT" },
        new { url = "iMeshes/IsoPyramid.fbx",     name = "FBXIsoPyramid" },
        new { url = "iMeshes/IsoPyramidT.fbx",    name = "FBXIsoPyramidT" },
        new { url = "iMeshes/BullDozerBox3.fbx",  name = "FBXDozerBox" },
        new { url = "iMeshes/BrickBox.fbx",       name = "BrickBox" },
        new { url = "iMeshes/TriangleT.fbx",      name = "TriT" },
        new { url = "iMeshes/Hex002.fbx",         name = "HexCyl" },
        new { url = "iMeshes/JibbaCola.fbx",      name = "ColaSquare" },
        new { url = "iMeshes/SmoothSphere001.fbx",name = "SmoothSphere" },
        new { url = "iMeshes/SmoothSphereT001.fbx",name = "SmoothSphereT" },
        new { url = "iMeshes/HexCylT001.fbx",     name = "HexCylT" },
        new { url = "iMeshes/LightBulb002.fbx",   name = "LightBulb" },
        new { url = "iMeshes/CheeseSign004.fbx",  name = "CheeseSign" },
        new { url = "iMeshes/UVSphere001.fbx",    name = "UVSphere" },
        new { url = "iMeshes/FBXCube001.fbx",     name = "FBXCubeRed" },

        new { url = "iMeshes/BushGroup001.fbx",   name = "Bush001" },
        new { url = "iMeshes/RockGroup002.fbx",   name = "Rock001" },
        new { url = "iMeshes/TreeGroup002.fbx",   name = "Tree001" },
         new { url = "iMeshes/Grass001.fbx",   name = "Grass001" },
          new { url = "iMeshes/Grave001.fbx",   name = "Grave001" },
           new { url = "iMeshes/GraveS001.fbx",   name = "GraveS001" },

        new { url = "iMeshes/HouseGroup001.fbx",  name = "House001" },
        new { url = "iMeshes/Stables001.fbx",     name = "Stable001" },
        new { url = "iMeshes/Well001.fbx",        name = "Well001" },
        new { url = "iMeshes/Blacksmith001.fbx",  name = "Blacksmith001" },
        new { url = "iMeshes/Temple001.fbx",      name = "Temple001" },
        new { url = "iMeshes/Storage001.fbx",     name = "Storage001" },
        new { url = "iMeshes/House005.fbx",       name = "House005" },
        new { url = "iMeshes/Mill001.fbx",        name = "Mill001" },
        new { url = "iMeshes/Market001.fbx",      name = "Market001" },
        new { url = "iMeshes/House003.fbx",      name = "House003" },
        new { url = "iMeshes/House006.fbx",      name = "House006" },
        new { url = "iMeshes/SawMill001.fbx",      name = "SawMill001" },
        new { url = "iMeshes/Inn001.fbx",      name = "Inn001" },
        new { url = "iMeshes/BellTower001.fbx",      name = "BellTower001" },
        new { url = "iMeshes/CastleWall001.fbx",      name = "CastleWall001" },
        new { url = "iMeshes/Crypt001.fbx",      name = "Crypt001" },
        new { url = "iMeshes/Shack001.fbx",      name = "Shack001" },

    };

                    // Blazor serializes anonymous arrays oddly — serialize to JSON string
                    // and parse on the JS side to guarantee it arrives as a proper array
                    var meshListJson = System.Text.Json.JsonSerializer.Serialize(meshList);
                    await JS.InvokeVoidAsync("SpectralFBXHelper.loadAllAndUploadJson", meshListJson);

                    // Register all meshes on C# side as JS-uploaded placeholders
                    foreach (var m in meshList)
                        Engine.MeshLibrary.RegisterJSMesh(m.name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("LOAD FAILED: " + ex.ToString());
                }

                // Init AFTER all meshes loaded — same as before
                Engine.Init();
                _engineRef = DotNetObjectReference.Create(Engine);
            }

            // Render loop start — now checks _engineRef as before
            if (!_loopStarted && !showIntro && _engineRef != null)
            {
                try
                {
                    while (!await JS.InvokeAsync<bool>("eval",
                        "window._SpectralShaders !== undefined"))
                        await Task.Delay(50);

                    await JS.InvokeVoidAsync("SpectralGLInterop.startRenderLoop",
                        CanvasRef, _engineRef);
                    await JS.InvokeVoidAsync("registerFullscreenListener",
                        _engineRef, "SpectralX-Container");
                    _loopStarted = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[SpectralX] Failed to start render loop: " + ex);
                }
            }
        }
        private void OnAAModeChanged(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int mode))
                Engine.ActiveAA = (AAMode)mode;
        }

        private void OnShadowModeChanged(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int mode))
                Engine.ActiveShadow = (SpectralXEngine.ShadowMode)mode;
        }
        public async Task ToggleViewport()
        {
            await JS.InvokeVoidAsync("toggleFullscreen", "SpectralX-Container");
            IsFullscreen = !IsFullscreen;
            await Task.Delay(100);

            if (IsFullscreen)
            {
                var size = await JS.InvokeAsync<ViewportSize>("getViewportSize", "SpectralX-Container");
                if (size != null)

                    Engine.Resize(size.Width, size.Height);
            }
            else
            {
              // part 5 Question about resolutions hold off until wer eodne with all parts 1-4.
              //  var size = await JS.InvokeAsync<ViewportSize>("getViewportSize", "SpectralX-Container");
              //  if (size != null)

               //     Engine.Resize(size.Width, size.Height);


                // Part 1 fix JS side return values were never updated to match viewport
                Engine.Resize(1024, 768);
             //   Engine.Resize(800, 600);
            }

            StateHasChanged();
        }
        public void ToggleUIHidden()
        {
            _uiHidden = !_uiHidden;
            StateHasChanged();
        }
        public void Dispose()
        {
            _ = JS.InvokeVoidAsync("SpectralGLInterop.stopRenderLoop");
            _engineRef?.Dispose();

        }


        public class ViewportSize
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }

        private bool showIntro = true;
        private string _starsOutput = string.Empty;
        private System.Timers.Timer? _starsTimer;

        protected override void OnInitialized()
        {
            StartStars();
        }

        private void StartStars()
        {
            _starsTimer = new System.Timers.Timer(1000);
            _starsTimer.Elapsed += (s, e) =>
            {
                var rnd = new Random();
                _starsOutput = string.Empty;
                for (int i = 0; i < 20; i++)
                {
                    int x = rnd.Next(800);
                    int y = rnd.Next(600);
                    _starsOutput += $@"<div style='width:4px;height:4px;background:white;border-radius:50%;position:absolute;top:{y}px;left:{x}px;box-shadow:0 0 10px white;'></div>";
                }
                InvokeAsync(StateHasChanged);
            };
            _starsTimer.Start();
        }

        private void HideIntro()
        {
            showIntro = false;
            _starsTimer?.Stop();
            _starsOutput = string.Empty;
            _ = Task.Delay(1).ContinueWith(_ =>
                JS.InvokeVoidAsync("SpectralGLLoader.reset", false, false));
        }

        private float _timeOfDay = 0.5f;

        private void OnTimeOfDayChanged(ChangeEventArgs e)
        {
            if (float.TryParse(e.Value?.ToString(),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out float val))
            {
                _timeOfDay = val;
                Engine.SetTimeOfDay(val);
            }
        }

        // Converts 0-1 time float to a readable label for the slider
        private string GetTimeLabel()
        {
            float t = _timeOfDay;

            if (t < 0.05f) return "Midnight";
            else if (t < 0.22f) return "Late Night";
            else if (t < 0.27f) return "Sunrise";
            else if (t < 0.35f) return "Golden Hour";
            else if (t < 0.45f) return "Morning";
            else if (t < 0.55f) return "Noon";
            else if (t < 0.65f) return "Afternoon";
            else if (t < 0.73f) return "Golden Hour";
            else if (t < 0.78f) return "Sunset";
            else if (t < 0.85f) return "Dusk";
            else if (t < 0.95f) return "Evening";
            else return "Midnight";
        }


    }
}