
namespace SpectralXAXA.SpectralXComponent

{
    public class SpectralXEngine : IDisposable
    {
        private readonly SpectralXViewport Viewport;
        private readonly SpectralXInput input;
        private readonly SpectralXCamera camera;
        private readonly SpectralXAX SpectralX;
        private System.Timers.Timer? tickTimer;
        private readonly SpectralXSVGViewport SVGViewport;
        // Public properties for external access
        public SpectralXRenderer Renderer { get; private set; }
        public SpectralXMeshLibrary MeshLibrary { get; private set; }
        public SpectralXScene Scene { get; private set; }
        public PerformanceMonitor Performance { get; private set; } = new();
        public bool IsRunning { get; private set; } = false;
        public int TargetFPS { get; set; } = 60;

        public SpectralXEngine(SpectralXAX spectralX, SpectralXViewport viewport,SpectralXSVGViewport xSVGViewport, SpectralXCamera camera, IJSRuntime js)
        {
            this.SpectralX = spectralX;
            this.Viewport = viewport;
            this.SVGViewport = xSVGViewport;    
            this.camera = camera;
            this.input = new SpectralXInput(spectralX, viewport,xSVGViewport, camera, js);


            // Initialize new systems
            MeshLibrary = new SpectralXMeshLibrary();
            Scene = new SpectralXScene();
            Renderer = new SpectralXRenderer();
        }

        public void Init()
        {
            // Get the built-in cube mesh from library
            var cubeMesh = MeshLibrary.GetMesh("PrimCube");
            if (cubeMesh != null)
            {
                var cubeEntity = Scene.Spawn(cubeMesh, new Vector3(0, 0, 0), "PrimCube");
                Console.WriteLine($"[SpectralXEngine] Spawned cube entity: {cubeEntity.Name}");
            }

            // Spawn a plane as a floor
            var planeMesh = MeshLibrary.GetMesh("Plane");
            if (planeMesh != null)
            {
                var planeEntity = Scene.Spawn(planeMesh, new Vector3(0, -2, 0), "Floor");
                planeEntity.SetScale(10f);
                Console.WriteLine($"[SpectralXEngine] Spawned plane entity: {planeEntity.Name}");
            }

            // Spawn a 2x2 maze
            int mazeSize = 2;
            float spacing = 5f;
            float offset = (mazeSize - 1) * spacing / 2f;

            for (int x = 0; x < mazeSize; x++)
            {
                for (int z = 0; z < mazeSize; z++)
                {
                    var pos = new Vector3(
                        x * spacing - offset,
                        0,
                        z * spacing - offset
                    );
                    var wallCube = Scene.Spawn(cubeMesh, pos, $"Wall_{x}_{z}");
                    Console.WriteLine($"[SpectralXEngine] Spawned cube at {pos}");
                }
            }

            Console.WriteLine($"[SpectralXEngine] Spawned {mazeSize}x{mazeSize} maze with {Scene.EntityCount} entities.");

            // ← LOAD FBX MESH (now it's preloaded, just get it!)
          //  var fbxMesh = MeshLibrary.GetMesh("PrimCube");
            var fbxMesh = MeshLibrary.GetMesh("FBXCube");
            if (fbxMesh != null)
            {
                var fbxEntity = Scene.Spawn(fbxMesh, new Vector3(0, 5, 0), "FBXCube");
                Console.WriteLine($"[SpectralXEngine] Spawned FBX Cube: {fbxEntity.Name}");
            }
            else
            {
                Console.WriteLine("[SpectralXEngine] FBXCube not found in library - did preload fail?");
            }

            Console.WriteLine($"[SpectralXEngine] Scene initialized with {Scene.EntityCount} entities.");

            input.Register();
        }

        public void Start()
        {
            if (IsRunning) return;

            double interval = 1000.0 / TargetFPS;

            tickTimer = new System.Timers.Timer(interval);
            tickTimer.Elapsed += (s, e) => Tick();
            tickTimer.AutoReset = true;
            tickTimer.Start();

            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning) return;

            tickTimer?.Stop();
            tickTimer?.Dispose();
            tickTimer = null;

            IsRunning = false;
        }

        public void Tick()
        {
            Performance.StartFrame();

            using (Performance.MeasureSection("Update"))
            {
                Update();
            }

            using (Performance.MeasureSection("Render"))
            {
                Render();
            }

            Performance.EndFrame();
        }

        private void Update()
        {
            using (Performance.MeasureSection("Input"))
            {
                input.Update();
            }

            using (Performance.MeasureSection("Camera"))
            {
                camera.Update();
            }

            using (Performance.MeasureSection("Scene"))
            {
                Scene.Update(1f / TargetFPS); // Delta time approximation
            }
        }

        private void Render()
        {
            using (Performance.MeasureSection("RenderScene"))
            {
                // Render to main viewport
                Renderer.RenderScene(Scene, camera, Viewport, SVGViewport);

              
            }

            using (Performance.MeasureSection("UpdateViewport"))
            {
                // Update main viewport with rendered triangles
                Viewport.UpdateRenderedTriangles(Renderer.ProjectedTriangles);

                // Update SVG viewport with rendered triangles
                SVGViewport.UpdateRenderedTriangles(Renderer.ProjectedTriangles);
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}










