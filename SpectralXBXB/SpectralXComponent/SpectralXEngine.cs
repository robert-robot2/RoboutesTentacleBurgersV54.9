
namespace SpectralXBXB.SpectralXComponent
{
    public class SpectralXEngine 
    {
        private readonly SpectralXViewport Viewport;
        private readonly SpectralXInput Input;
        public SpectralXCamera Camera;
        private readonly SpectralXBX SpectralX;
        private readonly GamepadService2 _gamepad;
        // Core systems
        public SpectralXRenderer Renderer { get; }
        public SpectralXMeshLibrary MeshLibrary { get; }
        public SpectralXScene Scene { get; }

        // Diagnostics
        public PerformanceMonitor Performance { get; } = new();

        public bool IsRunning { get; private set; }
      
        public SpectralXEngine(
       SpectralXBX spectralX,
       SpectralXViewport viewport,
       SpectralXCamera camera,
       SpectralXInput input,
       GamepadService2 gamepad, 
       IJSRuntime js)
        {
            SpectralX = spectralX;
            Viewport = viewport;
            Camera = camera;
            Input = input;
            _gamepad = gamepad;
            MeshLibrary = new SpectralXMeshLibrary();
            Scene = new SpectralXScene();
            Renderer = new SpectralXRenderer();
        }


       

        public void Init()
        {

         
         
            // Lighting

            var light1 = new SpectralXLight(
             position: new Vector3(0, 0, 6),
             color: new Vector3(1f, 1f, 1f),
             intensity: 1.0f,
             range: 20f
         );
            Scene.AddLight(light1);

            // 2D

            var triangle = MeshLibrary.GetMesh("PrimTriangle");
            Scene.AddMesh(triangle);
            triangle.Position = new Vector3(0, 2, 2);
            triangle.Color = new Vector4(1f, 1f, 0f, 0.7f);
            triangle.Rotation += new Vector3(0f, MathF.PI / 4f, 0f);
            triangle.Size = new Vector3(0.5f, 0.5f, 0.5f);

            var square = MeshLibrary.GetMesh("PrimSquare");
            Scene.AddMesh(square);
            square.Position = new Vector3(0, 2, 4);
            square.Color = new Vector4(0f, 1f, 0f, 0.5f);

            // 3D



            var primCube = MeshLibrary.GetMesh("PrimCube");
            Scene.AddMesh(primCube);
            primCube.Position = new Vector3(2, 2, 6);
            primCube.Color = new Vector4(1f, 0f, 0f, 1f);
            primCube.Rotation += new Vector3(0f, MathF.PI / 4f, 0f);
            primCube.Size = new Vector3(0.5f, 0.5f, 0.5f);

            var primPyr = MeshLibrary.GetMesh("PrimPyramid");
            Scene.AddMesh(primPyr);
            primPyr.Position = new Vector3(1, 2, 8);
            primPyr.Color= new Vector4(1f, 0f, 1f, 1f);


            // FBX

            var dozerBox = MeshLibrary.GetMesh("FBXDozerBox");
            Scene.AddMesh(dozerBox);
            dozerBox.Position= new Vector3(0,0,0);
            dozerBox.Size = new Vector3(2f, 2f, 2f);
            dozerBox.Color = new Vector4(0.36f, 0.25f, 0.20f, 1f);


            // FBX Textured

            var TriT = MeshLibrary.GetMesh("TriT");
            Scene.AddMesh(TriT);
            TriT.Position = new Vector3(-2, 2, 10);
            TriT.Size = new Vector3(1f, 1f, 1f);


            var plane = MeshLibrary.GetMesh("ColaSquare");
            Scene.AddMesh(plane);
            plane.Position = new Vector3(0, 2, 12);
            plane.Color = new Vector4(0f, 0f, 1f, 1f);
            plane.Rotation += new Vector3(-MathF.PI / 2f, 0f, 0f);



            var brickbox = MeshLibrary.GetMesh("BrickBox");
            Scene.AddMesh(brickbox);
            brickbox.Position = new Vector3(0,2,14);
            brickbox.Size = new Vector3(0.5f, 0.5f, 0.5f);

            // FBX to texture test-->


            var fbxisoPyr = MeshLibrary.GetMesh("FBXIsoPyramid");
            Scene.AddMesh(fbxisoPyr);
            fbxisoPyr.Position = new Vector3(0, 3, 16);
            fbxisoPyr.Color = new Vector4(0.85f, 0.44f, 0.84f, 1f);

            var fbxisoPyrT = MeshLibrary.GetMesh("FBXIsoPyramidT");
            Scene.AddMesh(fbxisoPyrT);
            fbxisoPyrT.Position = new Vector3(0, 4, 14);
            fbxisoPyrT.Color = new Vector4(0.85f, 0.44f, 0.84f, 1f);


            var fbxPyr = MeshLibrary.GetMesh("FBXPyramid");         
            Scene.AddMesh(fbxPyr);
            fbxPyr.Position= new Vector3(0,2,18);
            fbxPyr.Color = new Vector4(0f, 0f, 1f, 1f);

            var fbxPyrT = MeshLibrary.GetMesh("FBXPyramidT");
            Scene.AddMesh(fbxPyrT);
            fbxPyrT.Position = new Vector3(0, 4, 10);
            fbxPyrT.Color = new Vector4(0f, 0f, 1f, 1f);

            // FBX works but heavy isosphereT
            var sphere = MeshLibrary.GetMesh("FBXSphere");
            Scene.AddMesh (sphere);
            sphere.Position = new Vector3(0,2,20);
            sphere.Size = new Vector3(0.2f, 0.2f, 0.2f);
            sphere.Color = new Vector4(1f, 0.5f, 0f, 1f);



            var hexCyl = MeshLibrary.GetMesh("HexCyl");
            Scene.AddMesh(hexCyl);
            hexCyl.Position= new Vector3(0,4,20);
            hexCyl.Size = new Vector3(0.2f, 0.2f, 0.2f);
            hexCyl.Color = new Vector4(0f, 1f, 1f, 1f);





        }

        
        [JSInvokable]
        public void Tick()
        {
            Performance.StartFrame();

           

            using (Performance.MeasureSection("Render"))
            {
                Render();
            }
            using (Performance.MeasureSection("Input"))
            {
                HandleGamepadInput();
            }

            using (Performance.MeasureSection("Camera"))
            {

            }
            /*
            if (Scene.Meshes.Count > 0)
            {
                if (Scene.Meshes[0] is SpectralXMesh cube)
                {
                    cube.Rotate(new Vector3(0, 0.01f, 0));
                }
            }
            */

            

            SpectralX.RequestRender();
            
            Performance.EndFrame();
        }


        private void Render()
        {
            using (Performance.MeasureSection("RenderScene"))
            {
                Renderer.RenderScene(
                    Scene,
                    Camera,
                    Viewport
                );
            }
        }

        private void HandleGamepadInput()
        {
            // Left stick = movement
            var movement = _gamepad.GetMovement();
            if (movement.Y < -0.3f) Camera.MoveForward();
            if (movement.Y > 0.3f) Camera.MoveBackward();
            if (movement.X < -0.3f) Camera.StrafeRight();
            if (movement.X > 0.3f) Camera.StrafeLeft();

            // Right stick = camera look
            var look = _gamepad.GetLook();
            if (Math.Abs(look.X) > 0.1f || Math.Abs(look.Y) > 0.1f)
            {
                Camera.Look(look.X * 5f, look.Y * 5f);  // Adjust sensitivity as needed
            }

            // Example button: A to jump (implement later)
            if (_gamepad.IsButtonPressed("A"))
            {
                Console.WriteLine("Jump!");
            }
        }








        public async Task HandleKeyDown(KeyboardEventArgs e)
        {
           await Input.HandleKeyDown(e);
        }


    }
}
