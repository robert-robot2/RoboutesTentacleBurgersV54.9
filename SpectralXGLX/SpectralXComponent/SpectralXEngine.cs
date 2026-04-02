using SpectralXGLX.SpectralGL.Backend.Cpu;
using SpectralXGLX.SpectralGL.Math;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpectralXGLX.SpectralXComponent
{
    public class SpectralXEngine
    {

        public enum ShadowMode
        {
            PCF_V1 = 0,
            PCSS_V1 = 1,
            SpecXS_VDS_V1 = 2,
            SpecXS_RPD_V2 = 3,
            SpecXS_IGN_V3 = 4
        }

        public ShadowMode ActiveShadow { get; set; } = ShadowMode.PCF_V1;

        public SpectralXShadow Shadow { get; set; } = new SpectralXShadow();
        public enum AAMode
        {
            None = 0,
            MSAA = 1,
            FXAA = 2,
            SMAA = 3,
            TAA = 4,
            SpectralAA = 5,
            SpectralAAV2 = 6,
            SpectralAAV3 = 7
        }

        public AAMode ActiveAA { get; set; } = AAMode.None;

        private readonly IJSRuntime _js;

        private readonly SpectralXViewport Viewport;
        public readonly SpectralXInput Input;
        public SpectralXCamera Camera;
        private readonly SpectralXGLX SpectralXGLX;
        private readonly GamepadService _gamepad;

        public SpectralXRenderer Renderer { get; }
        public SpectralXMeshLibrary MeshLibrary { get; }
        public SpectralXScene Scene { get; }
        public SpectralXScene Scene2 { get; } = new SpectralXScene();
     

        public SpectralXWeatherClass? Weather { get; private set; }

        public SpectralXLandTileMap TileMap { get; private set; } = new();
        private bool _tileMapTexturesUploaded = false;
        private float _lastMouseX = 0f;
        private float _lastMouseY = 0f;
        private bool _isMousePainting = false;

        public SpectralXSun Sun { get; private set; } = new SpectralXSun();
        private SpectralXLight? _sunLight = null;

        public int ActiveScene { get; private set; } = 1;
        private const float PortalTriggerRadius = 1.5f;
        public PerformanceMonitor2 Performance { get; } = new();
        public bool IsRunning { get; private set; }


        public bool IsSceneLoading { get; private set; } = true;
        public event Action? OnLoadingStateChanged;

        private readonly HashSet<string> _expectedMeshIds = new();
        public int UploadedMeshCount => _uploadedMeshes.Count;
        public int ExpectedMeshCount => _expectedMeshIds.Count;


        private readonly HashSet<string> _uploadedTextures = new();
        private readonly HashSet<string> _uploadedMeshes = new();  // ADD THIS

        private readonly DateTime _startTime = DateTime.UtcNow;

        public SpectralXLight? PrimaryLight =>
    Scene.Lights.Count > 0 ? Scene.Lights[0] : null;

        private readonly SpectralXPropScatter _propScatter = new SpectralXPropScatter(seed: 42);
        private List<FoliageInstanceGroup> _foliageGroups = new();
        private List<FoliageInstanceGroup>? _cachedFoliageFrameData = null;

        private readonly List<WebGLMeshData> _meshDataCache = new();
        private bool _meshCacheDirty = true;

        private static readonly Dictionary<string, int> _gizmoMap = new()
{
    { "LightGizmo",       0 },
    { "LightCore",        0 },
    { "LightAuraInner",   0 },
    { "LightAuraOuter",   0 },
    { "Light2Gizmo",      1 },
    { "Light2Aura",       1 },
    { "Light3Gizmo",      2 },
    { "Light3Aura",       2 },
    { "SpotLightGizmo",   4 },
    { "SpotLightAura",    4 },
    { "AreaLightGizmo",   5 },
    { "AreaLightAura",    5 },
    
    // Sector lights
    { "RedSpotGizmo",         6  },
    { "GreenPointGizmo",      7  },
    { "PurplePointGizmo",     8  },
    { "OrangePointGizmo",     9  },
    { "PurpleAreaGizmo",      10 },
    { "CyanPointGizmo",       11 },
    { "DeepBluePointGizmo",   12 },
    { "WarmYellowPointGizmo", 13 },
    { "ColdWhitePointGizmo",  14 },
    { "SicklyGreenPointGizmo",15 },
    { "DeepRedPointGizmo",    16 },
    { "PinkPointGizmo",       17 },
    
};

        private readonly List<SpectralXLight> _activeLightsCache = new();
        private int _lastLightCount = -1;
        private bool _lightsDirty = true;



        public (float screenX, float screenY) ProjectToScreen(Vector3 worldPos)
        {
            Mat4 view = Camera.GetViewMatrix();
            Mat4 proj = Mat4.CreatePerspective(
      90f * (MathF.PI / 180f),
      (float)Viewport.ViewportWidth / Viewport.ViewportHeight,
      0.1f, 2000f);
            Mat4 vp = proj * view;

            float x = worldPos.X * vp.M[0] + worldPos.Y * vp.M[4] + worldPos.Z * vp.M[8] + vp.M[12];
            float y = worldPos.X * vp.M[1] + worldPos.Y * vp.M[5] + worldPos.Z * vp.M[9] + vp.M[13];
            float w = worldPos.X * vp.M[3] + worldPos.Y * vp.M[7] + worldPos.Z * vp.M[11] + vp.M[15];

            float ndcX = x / w;
            float ndcY = y / w;

            float screenX = (ndcX * 0.5f + 0.5f) * Viewport.ViewportWidth;
            float screenY = (1.0f - (ndcY * 0.5f + 0.5f)) * Viewport.ViewportHeight;

            return (screenX, screenY);
        }
        public SpectralXEngine(
            SpectralXGLX spectralX,
            SpectralXViewport viewport,
            SpectralXCamera camera,
            SpectralXInput input,
            GamepadService gamepad,
            IJSRuntime js)
        {
            SpectralXGLX = spectralX;
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
            // GL Init
            Renderer.InitSpectralGL(Viewport.ViewportWidth, Viewport.ViewportHeight);
            Scene.DefaultProgram = Renderer.DefaultProgram;
            Scene.DefaultTexture = Renderer.DefaultTexture;
            _uploadedTextures.Clear();
            _uploadedMeshes.Clear();
            _gamepad.InitAsync();


            // The Shadow Knows...
            // Initialize scene shadow settings — SpectralXS modes read from this
            Shadow = new SpectralXShadow();

            Shadow.SoftnessBias = 0.008f;   // tighter than before
            Shadow.KernelSize = 3.0f;       // controls overall disk radius
            Shadow.DepthBias = 0.003f;      // reduce acne without over-softening
            Shadow.ContactSharpness = 0.0005f; // decrease for sharper contact shadows
            Shadow.TintR = 0.2f;               // add a warm tint to shadows
            Shadow.TintStrength = 0.3f;        // how strong the tint is
            Shadow.PenumbraTintStrength = 0.4f;  // how much light color bleeds into penumbra edges


            // Lighting

            var light1 = new SpectralXLight(
          position: new Vector3(0, -2, 10),
          color: new Vector3(1f, 1f, 1f),
          intensity: 4.0f,
          range: 15f);
            light1.CastsShadows = true;
            Scene.AddLight(light1);

            var light2 = new SpectralXLight(
                position: new Vector3(-5, 5, 12),
                color: new Vector3(0f, 0.4f, 1f),
                intensity: 5.0f,
                range: 8f);
            light2.CastsShadows = true;
            Scene.AddLight(light2);

            var light3 = new SpectralXLight(
                position: new Vector3(5, 5, 12),
                color: new Vector3(0.6f, 0f, 1f),
                intensity: 5.0f,
                range: 8f);
            light3.CastsShadows = true;
            Scene.AddLight(light3);

            // Lighting

            var lightGizmo = CreateGizmoFrom("LightGizmo", "LightBulb");
            lightGizmo.Position = light1.Position;
            lightGizmo.Size = new Vector3(0.3f, 0.3f, 0.3f);
            lightGizmo.Color = new Vector4(1f, 0.98f, 0.85f, 0.4f);
           lightGizmo.IsEmissive = true;
           lightGizmo.CastsShadow = false;
           lightGizmo.EmissiveIntensity = 0.8f;
            Scene.AddMesh(lightGizmo);

            // Inner core — bright emissive filament
            var lightCore = CreateGizmoFrom("LightCore", "SmoothSphere");
            lightCore.Position = light1.Position;
            lightCore.Size = new Vector3(0.08f, 0.08f, 0.08f);
            lightCore.Color = new Vector4(1f, 0.95f, 0.6f, 1f);
            lightCore.IsEmissive = true;
            lightCore.CastsShadow = false;
            lightCore.EmissiveIntensity = 3.0f;
            Scene.AddMesh(lightCore);

            // Inner aura — warm glow just around the bulb
            var lightAuraInner = CreateGizmoFrom("LightAuraInner", "SmoothSphere");
            lightAuraInner.Position = light1.Position;
            lightAuraInner.Size = new Vector3(0.35f, 0.35f, 0.35f);
            lightAuraInner.Color = new Vector4(1f, 0.85f, 0.4f, 0.12f);
            lightAuraInner.IsEmissive = true;
            lightAuraInner.CastsShadow = false;
            lightAuraInner.EmissiveIntensity = 1.2f;
            Scene.AddMesh(lightAuraInner);

            // Outer aura — large very faint halo
            var lightAuraOuter = CreateGizmoFrom("LightAuraOuter", "SmoothSphere");
            lightAuraOuter.Position = light1.Position;
            lightAuraOuter.Size = new Vector3(0.6f, 0.6f, 0.6f);
            lightAuraOuter.Color = new Vector4(1f, 0.75f, 0.3f, 0.05f);
            lightAuraOuter.IsEmissive = true;
            lightAuraOuter.CastsShadow = false;
            lightAuraOuter.EmissiveIntensity = 0.6f;
            Scene.AddMesh(lightAuraOuter);

            // Blue light gizmo
            var light2Gizmo = CreateGizmoFrom("Light2Gizmo", "SmoothSphere");
            light2Gizmo.Position = light2.Position;
            light2Gizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            light2Gizmo.Color = new Vector4(0f, 0.4f, 1f, 1f);
            light2Gizmo.IsEmissive = true;
            light2Gizmo.CastsShadow = false;
            light2Gizmo.EmissiveIntensity = 2.0f;
            Scene.AddMesh(light2Gizmo);

            // Blue light inner aura
            var light2Aura = CreateGizmoFrom("Light2Aura", "SmoothSphere");
            light2Aura.Position = light2.Position;
            light2Aura.Size = new Vector3(0.5f, 0.5f, 0.5f);
            light2Aura.Color = new Vector4(0f, 0.4f, 1f, 0.08f);
            light2Aura.IsEmissive = true;
            light2Aura.CastsShadow = false;
            light2Aura.EmissiveIntensity = 0.8f;
            Scene.AddMesh(light2Aura);

            // Purple light gizmo
            var light3Gizmo = CreateGizmoFrom("Light3Gizmo", "SmoothSphere");
            light3Gizmo.Position = light3.Position;
            light3Gizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            light3Gizmo.Color = new Vector4(0.6f, 0f, 1f, 1f);
            light3Gizmo.IsEmissive = true;
            light3Gizmo.CastsShadow = false;
            light3Gizmo.EmissiveIntensity = 2.0f;
            Scene.AddMesh(light3Gizmo);

            // Purple light inner aura
            var light3Aura = CreateGizmoFrom("Light3Aura", "SmoothSphere");
            light3Aura.Position = light3.Position;
            light3Aura.Size = new Vector3(0.5f, 0.5f, 0.5f);
            light3Aura.Color = new Vector4(0.6f, 0f, 1f, 0.08f);
            light3Aura.IsEmissive = true;
            light3Aura.CastsShadow = false;
            light3Aura.EmissiveIntensity = 0.8f;
            Scene.AddMesh(light3Aura);


            // --- Scene Alleway DozerBox---

            var dozerBox = MeshLibrary.GetMesh("FBXDozerBox");
            if (dozerBox != null)
            {
                dozerBox.Position = new Vector3(0, 10, 0);
                dozerBox.Size = new Vector3(5f, 1f, 2f);
                dozerBox.Color = new Vector4(0.36f, 0.25f, 0.20f, 1f);
                Scene.AddMesh(dozerBox);
            }




            // 2D Triangles

            var triangle = MeshLibrary.GetMesh("PrimTriangle");
            if (triangle != null)
            {
                triangle.Position = new Vector3(-8, 0, 2);
                triangle.Size = new Vector3(1f, 1f, 1f);
                triangle.Color = new Vector4(1f, 1f, 0f, 1f);
                triangle.Rotation += new Vector3(MathF.PI / 2f, 0f, 0f);    // X +90
                Scene.AddMesh(triangle);
            }

            var TriT = MeshLibrary.GetMesh("TriT");
            if (TriT != null)
            {
                Scene.AddMesh(TriT);
                TriT.Position = new Vector3(-8, 2, 2);
                TriT.Size = new Vector3(1f, 1f, 1f);
                TriT.Rotation += new Vector3(MathF.PI / 2f, 0f, 0f);    // X +90



                //  TriT.Rotation += new Vector3(0f, MathF.PI / 2f, 0f);    // Y +90

                // --- TriT Rotation Tests --- pick one at a time and comment the rest ---

                // X axis rotations
                //  TriT.Rotation += new Vector3(-MathF.PI / 2f, 0f, 0f);     // X -90
                //  TriT.Rotation += new Vector3(MathF.PI / 2f, 0f, 0f);    // X +90
                //TriT.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180

                // Y axis rotations
                //TriT.Rotation += new Vector3(0f, -MathF.PI / 2f, 0f);   // Y -90

                //TriT.Rotation += new Vector3(0f, MathF.PI, 0f);          // Y 180

                // Z axis rotations
                //TriT.Rotation += new Vector3(0f, 0f, -MathF.PI / 2f);   // Z -90
                //TriT.Rotation += new Vector3(0f, 0f, MathF.PI / 2f);    // Z +90
                //TriT.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180

                // Combined (for later)
                //TriT.Rotation += new Vector3(-MathF.PI / 2f, 0f, MathF.PI); // X -90 + Z 180  TriT.Rotation += new Vector3(-MathF.PI / 2f, 0f, 0f);



            }






            // 3D TetraHedron


            var fbxisoPyr = MeshLibrary.GetMesh("FBXIsoPyramid");
            if (fbxisoPyr != null)
            {
                Scene.AddMesh(fbxisoPyr);
                fbxisoPyr.Position = new Vector3(-8, 4, 2);
                fbxisoPyr.Color = new Vector4(0.85f, 0.44f, 0.84f, 1f);
            }


            var fbxisoPyrT = MeshLibrary.GetMesh("FBXIsoPyramidT");
            if (fbxisoPyrT != null)
            {
                Scene.AddMesh(fbxisoPyrT);
                fbxisoPyrT.Position = new Vector3(-8, 6, 2);
                fbxisoPyrT.Color = new Vector4(0.85f, 0.44f, 0.84f, 1f);
            }




            // entrance to Scene2


            var square2 = MeshLibrary.GetMesh("PrimSquare");
            if (square2 != null)
            {
                square2.Name = "PortalSquare"; // unique name!
                Scene.AddMesh(square2);
                square2.Position = new Vector3(0, 11, 11);
                square2.Size = new Vector3(1f, 1f, 1f);
                //   square2.Color = new Vector4(0f, 0f, 1f, 0.5f);
                square2.Rotation += new Vector3(MathF.PI / 2f, 0f, 0f);    // X +90

            }

            if (square2 is SpectralXMesh portalMesh)
            {
                portalMesh.IsAnimated = true;
                portalMesh.FrameCount = 10;
                portalMesh.FrameRate = 10f;
                portalMesh.SheetWidth = 840f;
                portalMesh.SheetHeight = 84f;
                portalMesh.FramePixelWidth = 84f;
                portalMesh.FramePixelHeight = 84f;
                portalMesh.TextureDataUrl = "iAssets/PortalSheet001.png";
                portalMesh.TextureIsRawRGBA = false;
            }





            // 2D Squares


            var square = MeshLibrary.GetMesh("PrimSquare");
            if (square != null)
            {
                Scene.AddMesh(square);
                square.Position = new Vector3(-2, 0, 2);
                square.Size = new Vector3(1f, 1f, 1f);
                square.Color = new Vector4(0f, 1f, 0f, 0.5f);
                square.Rotation += new Vector3(MathF.PI / 2f, 0f, 0f);    // X +90
                // shadow casting is on my default.
              //  square.CastsShadow = false;
            }


            var plane = MeshLibrary.GetMesh("ColaSquare");
            if (plane != null)
            {
                Scene.AddMesh(plane);
                plane.Position = new Vector3(-2, 2, 2);
                plane.Size = new Vector3(1f, 1f, 1f);
                plane.Color = new Vector4(0f, 0f, 1f, 1f);
                plane.Rotation += new Vector3(MathF.PI / 2f, 0f, 0f);    // X +90
            }

            var cheeseSign = MeshLibrary.GetMesh("CheeseSign");
            if (cheeseSign != null)
            {
                Scene.AddMesh(cheeseSign);
                cheeseSign.Position = new Vector3(-2, 4, 2);
                cheeseSign.Size = new Vector3(1f, 1f, 1f);
                cheeseSign.Color = new Vector4(1f, 1f, 1f, 1f);
                cheeseSign.Rotation += new Vector3(MathF.PI / 2f, 0f, 0f);    // X +90
            }





            // --- 3D Cube ---
            var cube = MeshLibrary.GetMesh("PrimCube");
            if (cube != null)
            {
                cube.Position = new Vector3(1, 1, 2);
                cube.Size = new Vector3(1f, 1f, 1f);
                cube.Color = new Vector4(1f, 0f, 0f, 1f);
                Scene.AddMesh(cube);
            }

            var cube2 = MeshLibrary.GetMesh("FBXCubeRed");
            if (cube2 != null)
            {
                cube2.Position = new Vector3(1, 1, 6);
                cube2.Size = new Vector3(1f, 1f, 1f);
                // cube2.Color = new Vector4(1f, 0f, 0f, 1f);
                Scene.AddMesh(cube2);
            }


            var brickbox = MeshLibrary.GetMesh("BrickBox");
            if (brickbox != null)
            {
                Scene.AddMesh(brickbox);
                brickbox.Position = new Vector3(1, 4, 2);
                brickbox.Size = new Vector3(1f, 1f, 1f);

                brickbox.Size = new Vector3(1f, 1f, 1f);
            }






            // 3D Pyramid

            var pyramid = MeshLibrary.GetMesh("PrimPyramid");
            if (pyramid != null)
            {
                pyramid.Position = new Vector3(-5, 1, 2);
                pyramid.Size = new Vector3(1f, 1f, 1f);
                pyramid.Color = new Vector4(1f, 0f, 1f, 1f);
                Scene.AddMesh(pyramid);
            }





            var fbxPyr = MeshLibrary.GetMesh("FBXPyramid");
            if (fbxPyr != null)
            {
                Scene.AddMesh(fbxPyr);
                fbxPyr.Position = new Vector3(-5, 4, 2);
                fbxPyr.Color = new Vector4(0f, 0f, 1f, 1f);
            }

            var fbxPyrT = MeshLibrary.GetMesh("FBXPyramidT");
            if (fbxPyrT != null)
            {
                Scene.AddMesh(fbxPyrT);
                fbxPyrT.Position = new Vector3(-5, 7, 2);
                fbxPyrT.Color = new Vector4(0f, 0f, 1f, 1f);
            }

            // 3D Sphere

            var sphere = MeshLibrary.GetMesh("FBXSphere");
            if (sphere != null)
            {
                sphere.Position = new Vector3(4, 1, 2);
                sphere.Size = new Vector3(1f, 1f, 1f);
                sphere.Color = new Vector4(1f, 0.5f, 0f, 1f);
                Scene.AddMesh(sphere);
            }

            // need smooth shading
            var smoothsphere = MeshLibrary.GetMesh("SmoothSphere");
            if (smoothsphere != null)
            {
                smoothsphere.Position = new Vector3(4, 4, 2);
                smoothsphere.Size = new Vector3(1f, 1f, 1f);
                smoothsphere.Color = new Vector4(1f, 0.5f, 0f, 0.75f);
                Scene.AddMesh(smoothsphere);
            }
            // textures not working
            var smoothsphereT = MeshLibrary.GetMesh("SmoothSphereT");
            if (smoothsphereT != null)
            {
                smoothsphereT.Position = new Vector3(4, 7, 2);
                smoothsphereT.Size = new Vector3(1f, 1f, 1f);
                smoothsphereT.Color = new Vector4(1f, 0.5f, 0f, 1f);
                Scene.AddMesh(smoothsphereT);
            }

            // 3d hex Cylinder

            var hexCyl = MeshLibrary.GetMesh("HexCyl");
            if (hexCyl != null)
            {
                Scene.AddMesh(hexCyl);
                hexCyl.Position = new Vector3(7, 1, 2);
                hexCyl.Size = new Vector3(1f, 1f, 1f);
                hexCyl.Color = new Vector4(0f, 1f, 1f, 1f);
            }

            // textures not working

            var hexCylT = MeshLibrary.GetMesh("HexCylT");
            if (hexCylT != null)
            {
                Scene.AddMesh(hexCylT);
                hexCylT.Position = new Vector3(7, 4, 2);
                hexCylT.Size = new Vector3(1f, 1f, 1f);
                //  hexCylT.Color = new Vector4(0f, 1f, 1f, 1f);
            }

            // Light Test

            var LightBulb1 = MeshLibrary.GetMesh("LightBulb");
            if (LightBulb1 != null)
            {
                LightBulb1.Position = new Vector3(7, 7, 2);
                LightBulb1.Size = new Vector3(1f, 1f, 1f);
                LightBulb1.Color = new Vector4(1f, 1f, 1f, 0.25f);
                Scene.AddMesh(LightBulb1);
            }

            // ── Font Registration ────────────────────────────────────────────────────
            MeshLibrary.RegisterFont("Diablo",
                "/iAssets/Fonts/DiabloAtlas.json",
                "/iAssets/Fonts/DiabloAtlas.png");

            // ── Welcome Text ─────────────────────────────────────────────────────────
            var welcomeText = AddText("WELCOME",
                position: new Vector3(-3f, 0f, 8f),
                fontSize: 2f,
                fontKey: "Diablo",
                color: new Vector4(1f, 0.8f, 0.2f, 1f),
                align: TextAlignment.Center);
            welcomeText.Rotation = new Vector3(-MathF.PI / 2f, 0f, 0f);
            welcomeText.GlowRadius = 0.2f;
            welcomeText.GlowStrength = 1.0f;
            welcomeText.EmissiveIntensity = 3.0f;


            Camera.Position = new Vec3(0, -10, 4);
            Input.Register();
        }

        private void InitScene2()
        {

            _propScatter.Reset();
            _foliageGroups.Clear();

            Weather = new SpectralXWeatherClass();
            Weather.Init(Scene2, MeshLibrary, new Dictionary<WeatherParticleType, ParticleVolume>
{
    { WeatherParticleType.Rain,      new ParticleVolume(-64f, 64f, -64f, 64f,  0f, 30f) },
    { WeatherParticleType.Snow,      new ParticleVolume(-64f, 64f, -64f, 64f,  0f, 30f) },
    { WeatherParticleType.Cloud,     new ParticleVolume(-64f, 64f, -64f, 64f, 30f, 70f) },
    { WeatherParticleType.Lightning, new ParticleVolume(-64f, 64f, -64f, 64f, 30f, 70f) },
});

            // Lighting
            var light1 = new SpectralXLight(
                position: new Vector3(44, -42, 10),
                color: new Vector3(1f, 1f, 1f),
                intensity: 1.0f,
                range: 15f);
            light1.CastsShadows = false;
            Scene2.AddLight(light1);

            var light2 = new SpectralXLight(
               position: new Vector3(34, -64, 8),
               color: new Vector3(0f, 0.4f, 1f),
               intensity: 6.0f,
               range: 15f);
            light2.CastsShadows = false;
            Scene2.AddLight(light2);

            var light3 = new SpectralXLight(
                position: new Vector3(24, -64, 10),
                color: new Vector3(0.6f, 0f, 1f),
                intensity: 6.0f,
                range: 15f);
            light3.CastsShadows = false;
            Scene2.AddLight(light3);

            var lightGizmo = CreateGizmoFrom("LightGizmo", "LightBulb");
            lightGizmo.Position = light1.Position;
            lightGizmo.Size = new Vector3(0.3f, 0.3f, 0.3f);
            lightGizmo.Color = new Vector4(1f, 0.98f, 0.85f, 0.4f);
            lightGizmo.IsEmissive = true;
            lightGizmo.EmissiveIntensity = 0.8f;
            Scene2.AddMesh(lightGizmo);

            // Inner core — bright emissive filament
            var lightCore = CreateGizmoFrom("LightCore", "SmoothSphere");
            lightCore.Position = light1.Position;
            lightCore.Size = new Vector3(0.08f, 0.08f, 0.08f);
            lightCore.Color = new Vector4(1f, 0.95f, 0.6f, 1f);
            lightCore.IsEmissive = true;
            lightCore.EmissiveIntensity = 3.0f;
            Scene2.AddMesh(lightCore);

            // Inner aura — warm glow just around the bulb
            var lightAuraInner = CreateGizmoFrom("LightAuraInner", "SmoothSphere");
            lightAuraInner.Position = light1.Position;
            lightAuraInner.Size = new Vector3(0.35f, 0.35f, 0.35f);
            lightAuraInner.Color = new Vector4(1f, 0.85f, 0.4f, 0.12f);
            lightAuraInner.IsEmissive = true;
            lightAuraInner.EmissiveIntensity = 1.2f;
            Scene2.AddMesh(lightAuraInner);

            // Outer aura — large very faint halo
            var lightAuraOuter = CreateGizmoFrom("LightAuraOuter", "SmoothSphere");
            lightAuraOuter.Position = light1.Position;
            lightAuraOuter.Size = new Vector3(0.6f, 0.6f, 0.6f);
            lightAuraOuter.Color = new Vector4(1f, 0.75f, 0.3f, 0.05f);
            lightAuraOuter.IsEmissive = true;
            lightAuraOuter.EmissiveIntensity = 0.6f;
            Scene2.AddMesh(lightAuraOuter);


            // Blue light gizmo
            var light2Gizmo = CreateGizmoFrom("Light2Gizmo", "SmoothSphere");
            light2Gizmo.Position = light2.Position;
            light2Gizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            light2Gizmo.Color = new Vector4(0f, 0.4f, 1f, 1f);
            light2Gizmo.IsEmissive = true;
            light2Gizmo.EmissiveIntensity = 2.0f;
            Scene2.AddMesh(light2Gizmo);

            // Blue light inner aura
            var light2Aura = CreateGizmoFrom("Light2Aura", "SmoothSphere");
            light2Aura.Position = light2.Position;
            light2Aura.Size = new Vector3(0.5f, 0.5f, 0.5f);
            light2Aura.Color = new Vector4(0f, 0.4f, 1f, 0.08f);
            light2Aura.IsEmissive = true;
            light2Aura.EmissiveIntensity = 0.8f;
            Scene2.AddMesh(light2Aura);

            // Purple light gizmo
            var light3Gizmo = CreateGizmoFrom("Light3Gizmo", "SmoothSphere");
            light3Gizmo.Position = light3.Position;
            light3Gizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            light3Gizmo.Color = new Vector4(0.6f, 0f, 1f, 1f);
            light3Gizmo.IsEmissive = true;
            light3Gizmo.EmissiveIntensity = 2.0f;
            Scene2.AddMesh(light3Gizmo);

            // Purple light inner aura
            var light3Aura = CreateGizmoFrom("Light3Aura", "SmoothSphere");
            light3Aura.Position = light3.Position;
            light3Aura.Size = new Vector3(0.5f, 0.5f, 0.5f);
            light3Aura.Color = new Vector4(0.6f, 0f, 1f, 0.08f);
            light3Aura.IsEmissive = true;
            light3Aura.EmissiveIntensity = 0.8f;
            Scene2.AddMesh(light3Aura);
            /*
            // Red cube
            var cube = MeshLibrary.GetMesh("PrimCube");
            if (cube != null)
            {
                cube.Name = "Scene2Cube";
                cube.Position = new Vector3(0, 0, 2);
                cube.Size = new Vector3(1f, 1f, 1f);
                cube.Color = new Vector4(1f, 0f, 0f, 1f);
                Scene2.AddMesh(cube);
            }

            */

            // Props
            /*
            var bush = MeshLibrary.GetMesh("Bush001");
       
            if (bush != null)
            {
                if (bush is SpectralXMesh smBush) smBush.JSSourceMesh = "Bush001";

                bush.Name = "Scene2Bush";
                bush.Position = new Vector3(-5, -15, 1);
                bush.Size = new Vector3(1f, 1f, 1f);
                //    bush.Color = new Vector4(1f, 0f, 0f, 1f);
                bush.Rotation += new Vector3(MathF.PI / 2f, 0f, 0f);    // X +90
                Scene2.AddMesh(bush);
            }

            var rock = MeshLibrary.GetMesh("Rock001");
            if (rock != null)
            {
                if (rock is SpectralXMesh smRock) smRock.JSSourceMesh = "Rock001";  // ADD
                rock.Name = "Scene2Rock";
                rock.Position = new Vector3(16, 16, 0);
                rock.Size = new Vector3(1f, 1f, 1f);
                //  rock.Color = new Vector4(1f, 0f, 0f, 1f);

                Scene2.AddMesh(rock);
            }

            var tree = MeshLibrary.GetMesh("Tree001");
            if (tree != null)
            {
                if (tree is SpectralXMesh smTree) smTree.JSSourceMesh = "Tree001";  // ADD
                tree.Name = "Scene2Tree";
                tree.Position = new Vector3(32,32, 0);
                tree.Size = new Vector3(1f, 1f, 1f);
                //   bush.Color = new Vector4(1f, 0f, 0f, 1f);
                Scene2.AddMesh(tree);
            }
            */
            // Buildings

            var house = MeshLibrary.GetMesh("House001");
            if (house != null)
            {
                if (house is SpectralXMesh smHouse) smHouse.JSSourceMesh = "House001";
                house.Name = "Scene2House";
                house.Position = new Vector3(64, -64, 0);
                house.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);

                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
                house.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house);
                _propScatter.RegisterFootprint(house.Position.X, house.Position.Y,
       SpectralXPropScatter.DeriveFootprintRadius(house));
            }

            var house3 = MeshLibrary.GetMesh("Well001");
            if (house3 != null)
            {
                if (house3 is SpectralXMesh smWell) smWell.JSSourceMesh = "Well001";  // ADD
                house3.Name = "Scene2Well";
                house3.Position = new Vector3(34, -64, 0);
                house3.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);
                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
                house3.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house3);
                _propScatter.RegisterFootprint(house3.Position.X, house3.Position.Y,
       SpectralXPropScatter.DeriveFootprintRadius(house3));
            }

            var house1 = MeshLibrary.GetMesh("Stable001");
            if (house1 != null)
            {
                if (house1 is SpectralXMesh smStable) smStable.JSSourceMesh = "Stable001";  // ADD
                house1.Name = "Scene2Stable";
                house1.Position = new Vector3(0, 0, 0);
                house1.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);
                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
               // house1.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house1);
                _propScatter.RegisterFootprint(house1.Position.X, house1.Position.Y,
       SpectralXPropScatter.DeriveFootprintRadius(house1));
            }

            var house4 = MeshLibrary.GetMesh("Blacksmith001");
            if (house4 != null)
            {
                if (house4 is SpectralXMesh smSmith) smSmith.JSSourceMesh = "Blacksmith001";  // ADD
                house4.Name = "Scene2smith";
                house4.Position = new Vector3(34, -32, 0);
                house4.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);
                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
                house4.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house4);
                _propScatter.RegisterFootprint(house4.Position.X, house4.Position.Y,
       SpectralXPropScatter.DeriveFootprintRadius(house4));
            }

            var house5 = MeshLibrary.GetMesh("Storage001");
            if (house5 != null)
            {
                if (house5 is SpectralXMesh smStorage) smStorage.JSSourceMesh = "Storage001";  // ADD
                house5.Name = "Scene2storage";
                house5.Position = new Vector3(54, -64, 0);
                house5.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);
                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
                house5.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house5);
                _propScatter.RegisterFootprint(house5.Position.X, house5.Position.Y,
       SpectralXPropScatter.DeriveFootprintRadius(house5));
            }

            var house7 = MeshLibrary.GetMesh("House005");
            if (house7 != null)
            {
                if (house7 is SpectralXMesh smHouse7) smHouse7.JSSourceMesh = "House005";  // ADD
                house7.Name = "Scene2House5";
                house7.Position = new Vector3(-10, -64, 0);
                house7.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);

                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
                house7.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house7);
                _propScatter.RegisterFootprint(house7.Position.X, house7.Position.Y,
       SpectralXPropScatter.DeriveFootprintRadius(house7));
            }

            var house8 = MeshLibrary.GetMesh("Mill001");
            if (house8 != null)
            {
                if (house8 is SpectralXMesh smMill) smMill.JSSourceMesh = "Mill001";  // ADD
                house8.Name = "Scene2mill";
                house8.Position = new Vector3(-34, -64, 0);
                house8.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);

                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
                house8.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house8);
                _propScatter.RegisterFootprint(house8.Position.X, house8.Position.Y,
       SpectralXPropScatter.DeriveFootprintRadius(house8));
            }
            
            var house9 = MeshLibrary.GetMesh("Market001");
            if (house9 != null)
            {
                if (house9 is SpectralXMesh smMarket) smMarket.JSSourceMesh = "Market001";  // ADD
                house9.Name = "Scene2market";
                house9.Position = new Vector3(16, -64, 0);
                house9.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);

                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
                house9.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house9);
                _propScatter.RegisterFootprint(house9.Position.X, house9.Position.Y,
       SpectralXPropScatter.DeriveFootprintRadius(house9));
            }

            var house6 = MeshLibrary.GetMesh("Temple001");
            if (house6 != null)
            {
                if (house6 is SpectralXMesh smTemple) smTemple.JSSourceMesh = "Temple001";  // ADD
                house6.Name = "Scene2Temple";
                house6.Position = new Vector3(-64, 64, 0);
                house6.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);

                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
                house6.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house6);
                _propScatter.RegisterFootprint(house6.Position.X, house6.Position.Y,
       SpectralXPropScatter.DeriveFootprintRadius(house6));
            }


            var house10 = MeshLibrary.GetMesh("House003");
            if (house10 != null)
            {
                if (house10 is SpectralXMesh smHouse10) smHouse10.JSSourceMesh = "House003";  // ADD
                house10.Name = "Scene2House003";
                house10.Position = new Vector3(44, -84, 0);
                house10.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);

                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
                house10.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house10);
                _propScatter.RegisterFootprint(house10.Position.X, house10.Position.Y,
       SpectralXPropScatter.DeriveFootprintRadius(house10));
            }



            var house11 = MeshLibrary.GetMesh("House006");
            if (house11 != null)
            {
                if (house11 is SpectralXMesh smHouse11) smHouse11.JSSourceMesh = "House006";  // ADD
                house11.Name = "Scene2House006";
                house11.Position = new Vector3(24, -84, 0);
                house11.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);

                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
                house11.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house11);
            }

            var house12 = MeshLibrary.GetMesh("SawMill001");
            if (house12 != null)
            {
                if (house12 is SpectralXMesh smHouse12) smHouse12.JSSourceMesh = "SawMill001";  // ADD
                house12.Name = "Scene2SawMill001";
                house12.Position = new Vector3(-34, -84, 0);
                house12.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);

                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
                house12.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house12);
                _propScatter.RegisterFootprint(house12.Position.X, house12.Position.Y,
       SpectralXPropScatter.DeriveFootprintRadius(house12));
            }

            var house13 = MeshLibrary.GetMesh("Inn001");
            if (house13 != null)
            {
                if (house13 is SpectralXMesh smHouse13) smHouse13.JSSourceMesh = "Inn001";  // ADD
                house13.Name = "Scene2Inn001";
                house13.Position = new Vector3(4, -32, 0);
                house13.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);

                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
                house13.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house13);
                _propScatter.RegisterFootprint(house13.Position.X, house13.Position.Y,
       SpectralXPropScatter.DeriveFootprintRadius(house13));
            }

            var house14 = MeshLibrary.GetMesh("BellTower001");
            if (house14 != null)
            {
                if (house14 is SpectralXMesh smHouse14) smHouse14.JSSourceMesh = "BellTower001";  // ADD
                house14.Name = "Scene2BellTower001";
                house14.Position = new Vector3(54, -32, 0);
                house14.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);

                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
                house14.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house14);
                _propScatter.RegisterFootprint(house14.Position.X, house14.Position.Y,
       SpectralXPropScatter.DeriveFootprintRadius(house14));
            }


            var house15 = MeshLibrary.GetMesh("CastleWall001");
            if (house15 != null)
            {
                if (house15 is SpectralXMesh smHouse15) smHouse15.JSSourceMesh = "CastleWall001";  // ADD
                house15.Name = "Scene2CastleWall001";
                house15.Position = new Vector3(-64, 64, 0);
                house15.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);

                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
                house15.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house15);
                _propScatter.RegisterFootprint(house15.Position.X, house15.Position.Y,
       SpectralXPropScatter.DeriveFootprintRadius(house15));
            }

            var house16 = MeshLibrary.GetMesh("Crypt001");
            if (house16 != null)
            {
                if (house16 is SpectralXMesh smHouse16) smHouse16.JSSourceMesh = "Crypt001";  // ADD
                house16.Name = "Scene2Crypt001";
                house16.Position = new Vector3(0, 64, 0);
                house16.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);

                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
                house16.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house16);
                _propScatter.RegisterFootprint(house16.Position.X, house16.Position.Y,
       SpectralXPropScatter.DeriveFootprintRadius(house16));
            }

            var house17 = MeshLibrary.GetMesh("Shack001");
            if (house17 != null)
            {
                if (house17 is SpectralXMesh smHouse17) smHouse17.JSSourceMesh = "Shack001";  // ADD
                house17.Name = "Scene2Shack001";
                house17.Position = new Vector3(128, 128, 0);
                house17.Size = new Vector3(1f, 1f, 1f);
                // bush.Color = new Vector4(1f, 0f, 0f, 1f);

                //  house.Rotation += new Vector3(MathF.PI, 0f, 0f);          // X 180
                house17.Rotation += new Vector3(0f, 0f, MathF.PI);          // Z 180
                Scene2.AddMesh(house17);
                _propScatter.RegisterFootprint(house17.Position.X, house17.Position.Y,
       SpectralXPropScatter.DeriveFootprintRadius(house17));
            }
            // Blue portal square — return to Scene 1
            var portal = MeshLibrary.GetMesh("PrimSquare");
            if (portal != null)
            {
                portal.Name = "Scene2Portal";
                portal.Position = new Vector3(64, -54, 2);
                portal.Size = new Vector3(1f, 1f, 1f);
                //   portal.Color = new Vector4(0f, 0f, 1f, 0.5f);
                portal.Rotation += new Vector3(MathF.PI / 2f, 0f, 0f);
                Scene2.AddMesh(portal);
            }

            // ADD at the end of InitScene2(), before the closing brace:
            // ── Landscape TileMap ────────────────────────────────────────────────────
            TileMap = new SpectralXLandTileMap();
            TileMap.Init();
            _tileMapTexturesUploaded = false;

            // Test Floor
            /*
            var portal2 = MeshLibrary.GetMesh("PrimSquare");
            if (portal2 != null)
            {
                portal2.Name = "Scene2floor";
                portal2.Position = new Vector3(0, 0, 0);
                portal2.Size = new Vector3(3f, 3f, 3f);
                portal2.Color = new Vector4(1f, 1f, 1f, 1f);
             //   portal2.Rotation += new Vector3(MathF.PI / 2f, 0f, 0f);
                Scene2.AddMesh(portal2);
            }
            */


            if (portal is SpectralXMesh portalMesh)
            {
                portalMesh.IsAnimated = true;
                portalMesh.FrameCount = 10;
                portalMesh.FrameRate = 10f;
                portalMesh.SheetWidth = 840f;
                portalMesh.SheetHeight = 84f;
                portalMesh.FramePixelWidth = 84f;
                portalMesh.FramePixelHeight = 84f;
                portalMesh.TextureDataUrl = "iAssets/PortalSheet001.png";
                portalMesh.TextureIsRawRGBA = false;
            }

            // Pre-warm particle textures — one sentinel mesh per texture type
            // Uploaded frame 1, parked offscreen, never moved again
            // Texture pre-cache — uploads texture to GPU without adding geometry to scene
            // Particles need these ready before first spawn, zero frame cost after


            // ── Skysphere ────────────────────────────────────────────────────────
            // Large inverted sphere, always camera-centered, rendered emissive
            // so it skips all lighting calculations.
            // MaterialTextures[0] = day panorama, MaterialTextures[1] = night panorama
            var skySphere = CreateGizmoFrom("SkySphere", "SmoothSphere");
            skySphere.Position = new Vector3(Camera.Position.X, Camera.Position.Y, Camera.Position.Z);
            skySphere.Size = new Vector3(120f, 120f, 120f);
            skySphere.Color = new Vector4(1f, 1f, 1f, 1f);
            skySphere.IsEmissive = true;
            skySphere.EmissiveIntensity = 1.0f;
            skySphere.MaterialTextures.Add("/iAssets/SkyDay004SL.png");
            skySphere.MaterialTextures.Add("/iAssets/SkyNight005SL.png");
            Scene2.AddMesh(skySphere);

            // ── Sun Directional Light ────────────────────────────────────────────
            // Added LAST so it does not shift existing gizmo light indices (0,1,2)
            // Sun starts at noon (TimeOfDay = 0.5, set as default in SpectralXSun)
            _sunLight = new SpectralXLight(
                position: new Vector3(0f, -15f, 20f),
                color: new Vector3(1f, 0.98f, 0.90f),
                intensity: 5.0f,
                range: 200f);

            _sunLight.Type = LightType.Directional;
            _sunLight.Direction = new Vector3(0f, -0.5f, -1f);
            _sunLight.CastsShadows = true;
            _sunLight.Enabled = true;

            Sun.Apply(_sunLight);   // sync initial noon state immediately
            Scene2.AddLight(_sunLight);


            // ── Test Spot Light ──────────────────────────────────────────────────────
            var spotLight = new SpectralXLight(
                position: new Vector3(64f, -80f, 15f),
                color: new Vector3(1f, 0.9f, 0.7f),
                intensity: 12.0f,
                range: 30f);
            spotLight.Type = LightType.Spot;
            // NEW — angle toward scene center from above
            spotLight.Direction = new Vector3(0f, 0.4f, -1f);
            // Temporary test — crank it up to confirm shader is hitting
            spotLight.Intensity = 15.0f;
            spotLight.SpotAngle = 25f; // wider cone for easier testing

            spotLight.SpotSoftness = 0.15f;
            spotLight.CastsShadows = true; // within first 8 slots — gets shadow map

            Scene2.AddLight(spotLight);

            // ── Test Area Light ──────────────────────────────────────────────────────
            var areaLight = new SpectralXLight(
                position: new Vector3(48f, -64f, 2f),
                color: new Vector3(0.8f, 0.9f, 1.0f),
                intensity: 6.0f,
                range: 40f);
            areaLight.Type = LightType.Area;
            areaLight.Direction = new Vector3(0f, 0f, -1f);
            // NEW — crank for visibility test
            areaLight.Intensity = 3.0f;
            areaLight.Range = 60f;
         //   areaLight.SpotAngle = 40f;

            areaLight.CastsShadows = false; // area lights skip shadow maps by default
            Scene2.AddLight(areaLight);

            // ── Spot Light Gizmo ─────────────────────────────────────────────────────
            var spotGizmo = CreateGizmoFrom("SpotLightGizmo", "SmoothSphere");
            spotGizmo.Position = spotLight.Position;
            spotGizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            spotGizmo.Color = new Vector4(1f, 0.9f, 0.7f, 1f);
            spotGizmo.IsEmissive = true;
            spotGizmo.EmissiveIntensity = 3.0f;
            Scene2.AddMesh(spotGizmo);

            var spotAuraGizmo = CreateGizmoFrom("SpotLightAura", "SmoothSphere");
            spotAuraGizmo.Position = spotLight.Position;
            spotAuraGizmo.Size = new Vector3(0.5f, 0.5f, 0.5f);
            spotAuraGizmo.Color = new Vector4(1f, 0.9f, 0.7f, 0.08f);
            spotAuraGizmo.IsEmissive = true;
            spotAuraGizmo.EmissiveIntensity = 0.8f;
            Scene2.AddMesh(spotAuraGizmo);

            // ── Area Light Gizmo ──────────────────────────────────────────────────────
            var areaGizmo = CreateGizmoFrom("AreaLightGizmo", "SmoothSphere");
            areaGizmo.Position = areaLight.Position;
            areaGizmo.Size = new Vector3(0.3f, 0.3f, 0.3f);
            areaGizmo.Color = new Vector4(0.8f, 0.9f, 1.0f, 1f);
            areaGizmo.IsEmissive = true;
            areaGizmo.EmissiveIntensity = 2.0f;
            Scene2.AddMesh(areaGizmo);

            var areaAuraGizmo = CreateGizmoFrom("AreaLightAura", "SmoothSphere");
            areaAuraGizmo.Position = areaLight.Position;
            areaAuraGizmo.Size = new Vector3(0.7f, 0.7f, 0.7f);
            areaAuraGizmo.Color = new Vector4(0.8f, 0.9f, 1.0f, 0.06f);
            areaAuraGizmo.IsEmissive = true;
            areaAuraGizmo.EmissiveIntensity = 0.6f;
            Scene2.AddMesh(areaAuraGizmo);

            // Random lighting tests fun

            

            // ── Red Spotlight — Haunted Church ──────────────────────────────────────────
            var redSpot = new SpectralXLight(
                position: new Vector3(-64f, 54f, 22f),
                color: new Vector3(1f, 0f, 0f),
                intensity: 6.0f,
                range: 25f);
            redSpot.Type = LightType.Spot;
            redSpot.Direction = new Vector3(0f, 0f, -1f);
            redSpot.SpotAngle = 25f;
            redSpot.SpotSoftness = 0.15f;
            redSpot.CastsShadows = true;
            redSpot.Enabled = true;
            Scene2.AddLight(redSpot);

            var redSpotGizmo = CreateGizmoFrom("RedSpotGizmo", "SmoothSphere");
            redSpotGizmo.Position = redSpot.Position;
            redSpotGizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            redSpotGizmo.Color = new Vector4(1f, 0f, 0f, 1f);
            redSpotGizmo.IsEmissive = true;
            redSpotGizmo.EmissiveIntensity = 2.0f;
            Scene2.AddMesh(redSpotGizmo);

            var redSpotAura = CreateGizmoFrom("RedSpotGizmo", "SmoothSphere");
            redSpotAura.Position = redSpot.Position;
            redSpotAura.Size = new Vector3(0.5f, 0.5f, 0.5f);
            redSpotAura.Color = new Vector4(1f, 0f, 0f, 0.08f);
            redSpotAura.IsEmissive = true;
            redSpotAura.EmissiveIntensity = 0.8f;
            Scene2.AddMesh(redSpotAura);

            // ── Green Point Light — Plague/Swamp ────────────────────────────────────────
            var greenPoint = new SpectralXLight(
                position: new Vector3(-96f, -96f, 4f),
                color: new Vector3(0f, 1f, 0f),
                intensity: 6.0f,
                range: 15f);
            greenPoint.Type = LightType.Point;
            greenPoint.CastsShadows = false;
            greenPoint.Enabled = true;
            Scene2.AddLight(greenPoint);

            var greenPointGizmo = CreateGizmoFrom("GreenPointGizmo", "SmoothSphere");
            greenPointGizmo.Position = greenPoint.Position;
            greenPointGizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            greenPointGizmo.Color = new Vector4(0f, 1f, 0f, 1f);
            greenPointGizmo.IsEmissive = true;
            greenPointGizmo.EmissiveIntensity = 2.0f;
            Scene2.AddMesh(greenPointGizmo);

            var greenPointAura = CreateGizmoFrom("GreenPointAura", "SmoothSphere");
            greenPointAura.Position = greenPoint.Position;
            greenPointAura.Size = new Vector3(0.5f, 0.5f, 0.5f);
            greenPointAura.Color = new Vector4(0f, 1f, 0f, 0.08f);
            greenPointAura.IsEmissive = true;
            greenPointAura.EmissiveIntensity = 0.8f;
            Scene2.AddMesh(greenPointAura);
            
            // ── Purple Point Light — Witch's Hut ────────────────────────────────────────
            var purplePoint = new SpectralXLight(
                position: new Vector3(128f, 128f, 10f),
                color: new Vector3(0.6f, 0f, 1f),
                intensity: 6.0f,
                range: 15f);
            purplePoint.Type = LightType.Point;
            purplePoint.CastsShadows = false;
            purplePoint.Enabled = true;
            Scene2.AddLight(purplePoint);

            var purplePointGizmo = CreateGizmoFrom("PurplePointGizmo", "SmoothSphere");
            purplePointGizmo.Position = purplePoint.Position;
            purplePointGizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            purplePointGizmo.Color = new Vector4(0.6f, 0f, 1f, 1f);
            purplePointGizmo.IsEmissive = true;
            purplePointGizmo.EmissiveIntensity = 2.0f;
            Scene2.AddMesh(purplePointGizmo);

            var purplePointAura = CreateGizmoFrom("PurplePointAura", "SmoothSphere");
            purplePointAura.Position = purplePoint.Position;
            purplePointAura.Size = new Vector3(0.5f, 0.5f, 0.5f);
            purplePointAura.Color = new Vector4(0.6f, 0f, 1f, 0.08f);
            purplePointAura.IsEmissive = true;
            purplePointAura.EmissiveIntensity = 0.8f;
            Scene2.AddMesh(purplePointAura);

            // ── Orange Point Light — Blacksmith/Forge ───────────────────────────────────
            var orangePoint = new SpectralXLight(
                position: new Vector3(34f, -32f, 18f),
                color: new Vector3(1f, 0.4f, 0f),
                intensity: 6.0f,
                range: 15f);
            orangePoint.Type = LightType.Point;
            orangePoint.CastsShadows = false;
            orangePoint.Enabled = true;
            Scene2.AddLight(orangePoint);

            var orangePointGizmo = CreateGizmoFrom("OrangePointGizmo", "SmoothSphere");
            orangePointGizmo.Position = orangePoint.Position;
            orangePointGizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            orangePointGizmo.Color = new Vector4(1f, 0.4f, 0f, 1f);
            orangePointGizmo.IsEmissive = true;
            orangePointGizmo.EmissiveIntensity = 2.0f;
            Scene2.AddMesh(orangePointGizmo);

            var orangePointAura = CreateGizmoFrom("OrangePointAura", "SmoothSphere");
            orangePointAura.Position = orangePoint.Position;
            orangePointAura.Size = new Vector3(0.5f, 0.5f, 0.5f);
            orangePointAura.Color = new Vector4(1f, 0.4f, 0f, 0.08f);
            orangePointAura.IsEmissive = true;
            orangePointAura.EmissiveIntensity = 0.8f;
            Scene2.AddMesh(orangePointAura);

            // ── Purple Area Light — Coven/Ritual ────────────────────────────────────────
            var purpleArea = new SpectralXLight(
                position: new Vector3(64f, -96f, 4f),
                color: new Vector3(0.5f, 0f, 0.8f),
                intensity: 6.0f,
                range: 15f);
            purpleArea.Type = LightType.Area;
            purpleArea.Direction = new Vector3(0f, 0f, -1f);
            purpleArea.CastsShadows = false;
            purpleArea.Enabled = true;
            Scene2.AddLight(purpleArea);

            var purpleAreaGizmo = CreateGizmoFrom("PurpleAreaGizmo", "SmoothSphere");
            purpleAreaGizmo.Position = purpleArea.Position;
            purpleAreaGizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            purpleAreaGizmo.Color = new Vector4(0.5f, 0f, 0.8f, 1f);
            purpleAreaGizmo.IsEmissive = true;
            purpleAreaGizmo.EmissiveIntensity = 2.0f;
            Scene2.AddMesh(purpleAreaGizmo);

            var purpleAreaAura = CreateGizmoFrom("PurpleAreaAura", "SmoothSphere");
            purpleAreaAura.Position = purpleArea.Position;
            purpleAreaAura.Size = new Vector3(0.5f, 0.5f, 0.5f);
            purpleAreaAura.Color = new Vector4(0.5f, 0f, 0.8f, 0.08f);
            purpleAreaAura.IsEmissive = true;
            purpleAreaAura.EmissiveIntensity = 0.8f;
            Scene2.AddMesh(purpleAreaAura);

            // ── Cyan Point Light — Docks/Water ──────────────────────────────────────────
            var cyanPoint = new SpectralXLight(
                position: new Vector3(0f, -128f, 4f),
                color: new Vector3(0f, 0.25f, 1f),
                intensity: 6.0f,
                range: 15f);
            cyanPoint.Type = LightType.Point;
            cyanPoint.CastsShadows = false;
            cyanPoint.Enabled = true;
            Scene2.AddLight(cyanPoint);

            var cyanPointGizmo = CreateGizmoFrom("CyanPointGizmo", "SmoothSphere");
            cyanPointGizmo.Position = cyanPoint.Position;
            cyanPointGizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            cyanPointGizmo.Color = new Vector4(0f, 0.25f, 1f, 1f);
            cyanPointGizmo.IsEmissive = true;
            cyanPointGizmo.EmissiveIntensity = 2.0f;
            Scene2.AddMesh(cyanPointGizmo);

            var cyanPointAura = CreateGizmoFrom("CyanPointAura", "SmoothSphere");
            cyanPointAura.Position = cyanPoint.Position;
            cyanPointAura.Size = new Vector3(0.5f, 0.5f, 0.5f);
            cyanPointAura.Color = new Vector4(0f, 1f, 1f, 0.08f);
            cyanPointAura.IsEmissive = true;
            cyanPointAura.EmissiveIntensity = 0.8f;
            Scene2.AddMesh(cyanPointAura);

            // ── Deep Blue Point Light — Crypt/Underground ───────────────────────────────
            var deepBluePoint = new SpectralXLight(
                position: new Vector3(0f, 64f, 18f),
                color: new Vector3(0f, 0f, 0.8f),
                intensity: 6.0f,
                range: 15f);
            deepBluePoint.Type = LightType.Point;
            deepBluePoint.CastsShadows = false;
            deepBluePoint.Enabled = true;
            Scene2.AddLight(deepBluePoint);

            var deepBluePointGizmo = CreateGizmoFrom("DeepBluePointGizmo", "SmoothSphere");
            deepBluePointGizmo.Position = deepBluePoint.Position;
            deepBluePointGizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            deepBluePointGizmo.Color = new Vector4(0f, 0f, 0.8f, 1f);
            deepBluePointGizmo.IsEmissive = true;
            deepBluePointGizmo.EmissiveIntensity = 2.0f;
            Scene2.AddMesh(deepBluePointGizmo);

            var deepBluePointAura = CreateGizmoFrom("DeepBluePointAura", "SmoothSphere");
            deepBluePointAura.Position = deepBluePoint.Position;
            deepBluePointAura.Size = new Vector3(0.5f, 0.5f, 0.5f);
            deepBluePointAura.Color = new Vector4(0f, 0f, 0.8f, 0.08f);
            deepBluePointAura.IsEmissive = true;
            deepBluePointAura.EmissiveIntensity = 0.8f;
            Scene2.AddMesh(deepBluePointAura);

            // ── Warm Yellow Point Light — Tavern/Inn ────────────────────────────────────
            var warmYellowPoint = new SpectralXLight(
                position: new Vector3(4f, -32f, 20f),
                color: new Vector3(1f, 0.85f, 0.3f),
                intensity: 8.0f,
                range: 25f);
            warmYellowPoint.Type = LightType.Point;
            warmYellowPoint.CastsShadows = false;
            warmYellowPoint.Enabled = true;
            Scene2.AddLight(warmYellowPoint);

            var warmYellowPointGizmo = CreateGizmoFrom("WarmYellowPointGizmo", "SmoothSphere");
            warmYellowPointGizmo.Position = warmYellowPoint.Position;
            warmYellowPointGizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            warmYellowPointGizmo.Color = new Vector4(1f, 0.85f, 0.3f, 1f);
            warmYellowPointGizmo.IsEmissive = true;
            warmYellowPointGizmo.EmissiveIntensity = 2.0f;
            Scene2.AddMesh(warmYellowPointGizmo);

            var warmYellowPointAura = CreateGizmoFrom("WarmYellowPointAura", "SmoothSphere");
            warmYellowPointAura.Position = warmYellowPoint.Position;
            warmYellowPointAura.Size = new Vector3(0.5f, 0.5f, 0.5f);
            warmYellowPointAura.Color = new Vector4(1f, 0.85f, 0.3f, 0.08f);
            warmYellowPointAura.IsEmissive = true;
            warmYellowPointAura.EmissiveIntensity = 0.8f;
            Scene2.AddMesh(warmYellowPointAura);

            // ── Cold White Point Light — Temple/Holy ────────────────────────────────────
            var coldWhitePoint = new SpectralXLight(
                position: new Vector3(-10f, -64f, 20f),
                color: new Vector3(0.9f, 0.95f, 1f),
                intensity: 6.0f,
                range: 15f);
            coldWhitePoint.Type = LightType.Point;
            coldWhitePoint.CastsShadows = false;
            coldWhitePoint.Enabled = true;
            Scene2.AddLight(coldWhitePoint);

            var coldWhitePointGizmo = CreateGizmoFrom("ColdWhitePointGizmo", "SmoothSphere");
            coldWhitePointGizmo.Position = coldWhitePoint.Position;
            coldWhitePointGizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            coldWhitePointGizmo.Color = new Vector4(0.9f, 0.95f, 1f, 1f);
            coldWhitePointGizmo.IsEmissive = true;
            coldWhitePointGizmo.EmissiveIntensity = 2.0f;
            Scene2.AddMesh(coldWhitePointGizmo);

            var coldWhitePointAura = CreateGizmoFrom("ColdWhitePointAura", "SmoothSphere");
            coldWhitePointAura.Position = coldWhitePoint.Position;
            coldWhitePointAura.Size = new Vector3(0.5f, 0.5f, 0.5f);
            coldWhitePointAura.Color = new Vector4(0.9f, 0.95f, 1f, 0.08f);
            coldWhitePointAura.IsEmissive = true;
            coldWhitePointAura.EmissiveIntensity = 0.8f;
            Scene2.AddMesh(coldWhitePointAura);


            // ── Sickly Yellow-Green Point Light — Alchemist/Poison ──────────────────────
            var sicklyGreenPoint = new SpectralXLight(
                position: new Vector3(16f, -84f, 10f),
                color: new Vector3(0.6f, 0.9f, 0f),
                intensity: 6.0f,
                range: 15f);
            sicklyGreenPoint.Type = LightType.Point;
            sicklyGreenPoint.CastsShadows = false;
            sicklyGreenPoint.Enabled = true;
            Scene2.AddLight(sicklyGreenPoint);

            var sicklyGreenPointGizmo = CreateGizmoFrom("SicklyGreenPointGizmo", "SmoothSphere");
            sicklyGreenPointGizmo.Position = sicklyGreenPoint.Position;
            sicklyGreenPointGizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            sicklyGreenPointGizmo.Color = new Vector4(0.6f, 0.9f, 0f, 1f);
            sicklyGreenPointGizmo.IsEmissive = true;
            sicklyGreenPointGizmo.EmissiveIntensity = 2.0f;
            Scene2.AddMesh(sicklyGreenPointGizmo);

            var sicklyGreenPointAura = CreateGizmoFrom("SicklyGreenPointAura", "SmoothSphere");
            sicklyGreenPointAura.Position = sicklyGreenPoint.Position;
            sicklyGreenPointAura.Size = new Vector3(0.5f, 0.5f, 0.5f);
            sicklyGreenPointAura.Color = new Vector4(0.6f, 0.9f, 0f, 0.08f);
            sicklyGreenPointAura.IsEmissive = true;
            sicklyGreenPointAura.EmissiveIntensity = 0.8f;
            Scene2.AddMesh(sicklyGreenPointAura);

            // ── Deep Red Point Light — Dungeon Entrance ──────────────────────────────────
            var deepRedPoint = new SpectralXLight(
                position: new Vector3(0f, -96f, 4f),
                color: new Vector3(0.7f, 0f, 0f),
                intensity: 6.0f,
                range: 15f);
            deepRedPoint.Type = LightType.Point;
            deepRedPoint.CastsShadows = false;
            deepRedPoint.Enabled = true;
            Scene2.AddLight(deepRedPoint);

            var deepRedPointGizmo = CreateGizmoFrom("DeepRedPointGizmo", "SmoothSphere");
            deepRedPointGizmo.Position = deepRedPoint.Position;
            deepRedPointGizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            deepRedPointGizmo.Color = new Vector4(0.7f, 0f, 0f, 1f);
            deepRedPointGizmo.IsEmissive = true;
            deepRedPointGizmo.EmissiveIntensity = 2.0f;
            Scene2.AddMesh(deepRedPointGizmo);

            var deepRedPointAura = CreateGizmoFrom("DeepRedPointAura", "SmoothSphere");
            deepRedPointAura.Position = deepRedPoint.Position;
            deepRedPointAura.Size = new Vector3(0.5f, 0.5f, 0.5f);
            deepRedPointAura.Color = new Vector4(0.7f, 0f, 0f, 0.08f);
            deepRedPointAura.IsEmissive = true;
            deepRedPointAura.EmissiveIntensity = 0.8f;
            Scene2.AddMesh(deepRedPointAura);

            // ── Pink Point Light — Market/Festival ──────────────────────────────────────
            var pinkPoint = new SpectralXLight(
                position: new Vector3(8f, -64f, 10f),
                color: new Vector3(1f, 0.4f, 0.7f),
                intensity: 6.0f,
                range: 15f);
            pinkPoint.Type = LightType.Point;
            pinkPoint.CastsShadows = false;
            pinkPoint.Enabled = true;
            Scene2.AddLight(pinkPoint);

            var pinkPointGizmo = CreateGizmoFrom("PinkPointGizmo", "SmoothSphere");
            pinkPointGizmo.Position = pinkPoint.Position;
            pinkPointGizmo.Size = new Vector3(0.2f, 0.2f, 0.2f);
            pinkPointGizmo.Color = new Vector4(1f, 0.4f, 0.7f, 1f);
            pinkPointGizmo.IsEmissive = true;
            pinkPointGizmo.EmissiveIntensity = 2.0f;
            Scene2.AddMesh(pinkPointGizmo);

            var pinkPointAura = CreateGizmoFrom("PinkPointAura", "SmoothSphere");
            pinkPointAura.Position = pinkPoint.Position;
            pinkPointAura.Size = new Vector3(0.5f, 0.5f, 0.5f);
            pinkPointAura.Color = new Vector4(1f, 0.4f, 0.7f, 0.08f);
            pinkPointAura.IsEmissive = true;
            pinkPointAura.EmissiveIntensity = 0.8f;
            Scene2.AddMesh(pinkPointAura);

            



            // Auto-restore saved landscape on scene load
            _ = LoadLandscape();

            // ── Prop Scatter ─────────────────────────────────────────────
            // ── Foliage Scatter — instanced rendering, no scene mesh entries ──────────
            var scatterConfigs = new[]
            {
    new PropScatterConfig("Bush001",  50),
    new PropScatterConfig("Rock001",  50),
    new PropScatterConfig("Tree001",  25),
    new PropScatterConfig("Grass001", 200),
};

            foreach (var config in scatterConfigs)
                _foliageGroups.Add(_propScatter.Scatter(config));

            // ── Graveyard Grid ────────────────────────────────────────────────────────
            var graveyardConfigs = new[]
            {
    new GridBoundedScatterConfig("Grave001",  20, -76f, -52f, 52f, 76f),
    new GridBoundedScatterConfig("GraveS001", 20, -76f, -52f, 52f, 76f),
};

            foreach (var config in graveyardConfigs)
                _foliageGroups.Add(_propScatter.ScatterInGrid(config));



        }


        public SpectralXMesh AddText(
    string text,
    Vector3 position,
    float fontSize = 1f,
    string fontKey = "Diablo",
    Vector4? color = null,
    float letterSpacing = 0f,
    TextAlignment align = TextAlignment.Left)
        {
            var mesh = MeshLibrary.CreateTextMesh(text, fontKey, fontSize, position, align);
            mesh.Color = color ?? new Vector4(1f, 1f, 1f, 1f);
            mesh.LetterSpacing = letterSpacing;
            var activeScene = ActiveScene == 1 ? Scene : Scene2;
            activeScene.AddMesh(mesh);
            Console.WriteLine($"[Engine] AddText: '{text}' font:{fontKey} size:{fontSize}");
            return mesh;
        }


        private SpectralXMesh CreateGizmoFrom(string gizmoName, string sourceMeshName)
        {
            var gizmo = new SpectralXMesh(gizmoName);
            var source = MeshLibrary.GetMesh(sourceMeshName) as SpectralXMesh;
            if (source != null && source.Vertices.Count > 0)
            {
                gizmo.Vertices.AddRange(source.Vertices);
                gizmo.Normals.AddRange(source.Normals);
                gizmo.UVs.AddRange(source.UVs);
                foreach (var face in source.Faces)
                    gizmo.Faces.Add(face);
            }
            else
            {
                // JS-uploaded placeholder — no C# vertices, redirect to source buffer
                gizmo.JSSourceMesh = sourceMeshName;
            }
            return gizmo;
        }






        private void SwitchToScene(int sceneId)
        {
            _meshCacheDirty = true;  // ADD
            _cachedSkySphere = null;  // ADD
            _cachedFoliageFrameData = null;
            _ = _js.InvokeVoidAsync("SpectralGLLoader.reset",
        sceneId == 2,  // needsTilemap
        sceneId == 2   // needsCubemap
    );
            _uploadedMeshes.Clear();
            _uploadedTextures.Clear();
            _lightsDirty = true;
            _foliageGroups.Clear();

            ActiveScene = sceneId;

            if (sceneId == 2)
            {
                Scene2.Clear();
                InitScene2();
                _tileMapTexturesUploaded = false;
                Camera.Position = new Vec3(54, -54, 4);
            }
            else
            {
                Weather?.Reset(); // ADD THIS
                Scene.Clear();
                Init();
                // invert camera on entrance to see scene 1
                Camera.Position = new Vec3(0, -10, 4);
            }
        }

        private void CheckPortalTrigger()
        {
            Vector3 portalPos = ActiveScene == 1
                ? new Vector3(0, 11, 11)   // Scene 1 portal position
                : new Vector3(64, -54, 2);  // Scene 2 portal position

            float dx = Camera.Position.X - portalPos.X;
            float dy = Camera.Position.Y - portalPos.Y;
            float dz = Camera.Position.Z - portalPos.Z;
            float dist = MathF.Sqrt(dx * dx + dy * dy + dz * dz);

            if (dist < PortalTriggerRadius)
                SwitchToScene(ActiveScene == 1 ? 2 : 1);
        }

        [JSInvokable("TickAndGetFrame")]
        public WebGLFrameData TickAndGetFrame()
        {
            Performance.StartFrame();
            Input.ProcessHeldKeys();
            HandleGamepadInput();
            TickAnimations();
            TickWeather();
            SyncLightGizmos();
            SyncSkySphere();      
            TickSun();
            TickSceneLighting();  // ADD
            CheckPortalTrigger();
            Performance.EndFrame();
            return BuildWebGLFrame();
        }

        private SpectralXMesh? _cachedSkySphere = null;

        private void SyncSkySphere()
        {
            if (ActiveScene != 2) return;

            if (_cachedSkySphere == null)
                _cachedSkySphere = Scene2.Meshes
                    .OfType<SpectralXMesh>()
                    .FirstOrDefault(m => m.Name == "SkySphere");

            if (_cachedSkySphere == null) return;

            _cachedSkySphere.Position = new Vector3(
                Camera.Position.X,
                Camera.Position.Y,
                Camera.Position.Z);
        }

        // NEW — syncs SpectralXSun color/direction into the directional light slot     
        private void TickSun()
        {
            if (ActiveScene != 2 || _sunLight == null) return;

            // Advance cloud and star UV offsets each frame
            float now = (float)(DateTime.UtcNow - _startTime).TotalSeconds;
            float delta = now - _lastTickTime; // reuse existing tick time
            Sun.Tick(delta);

            Sun.Apply(_sunLight);
        }

        private bool _lightsWereOnAtNight = true;  // force sync on first tick

        private void TickSceneLighting()
        {
            if (ActiveScene != 2 || _sunLight == null) return;

            float t = Sun.TimeOfDay;
            bool isNight = t < 0.25f || t > 0.75f;

            // Only update when state changes — avoids setting _lightsDirty every frame
            if (isNight == _lightsWereOnAtNight) return;
            _lightsWereOnAtNight = isNight;

            foreach (var light in Scene2.Lights)
            {
                // Directional sun light always stays on
                if (light.Type == LightType.Directional) continue;
                light.Enabled = isNight;
            }

            _lightsDirty = true;
        }

        private float _lastTickTime = 0f;
        private void TickAnimations()
        {
            float now = (float)(DateTime.UtcNow - _startTime).TotalSeconds;
            float delta = now - _lastTickTime;
            _lastTickTime = now;

            var activeScene = ActiveScene == 1 ? Scene : Scene2;
            // Glow pulse tick — independent of sprite animation
            foreach (var mesh in activeScene.Meshes)
            {
                if (mesh is not SpectralXMesh sm) continue;
                if (sm.GlowPulseSpeed <= 0f) continue;

                float pulse = (MathF.Sin(now * sm.GlowPulseSpeed) * 0.5f + 0.5f);
                sm.GlowRadius = sm.GlowPulseMin + pulse * (sm.GlowPulseMax - sm.GlowPulseMin);
                sm.TextDirty = true;
            }
            foreach (var mesh in activeScene.Meshes)
            {
                if (mesh is not SpectralXMesh sm || !sm.IsAnimated) continue;

                sm.FrameTimer += delta;

                float frameDuration = 1f / sm.FrameRate;

                while (sm.FrameTimer >= frameDuration)
                {
                    sm.FrameTimer -= frameDuration;
                    sm.CurrentFrame = (sm.CurrentFrame + 1) % sm.FrameCount;

                    // 🔹 SPRITE SHEET UV CALCULATION
                    float cols = sm.SheetWidth / sm.FramePixelWidth;
                    float rows = sm.SheetHeight / sm.FramePixelHeight;

                    int frameX = sm.CurrentFrame % (int)cols;
                    int frameY = sm.CurrentFrame / (int)cols;

                    sm.UVScaleX = sm.FramePixelWidth / sm.SheetWidth;
                    sm.UVScaleY = sm.FramePixelHeight / sm.SheetHeight;

                    sm.UVOffsetX = frameX * sm.UVScaleX;
                    sm.UVOffsetY = frameY * sm.UVScaleY;
                }
            }
        }

        private float _lastWeatherTick = 0f;
        private void TickWeather()
        {
            if (Weather == null || ActiveScene != 2) return;
            float now = (float)(DateTime.UtcNow - _startTime).TotalSeconds;
            float delta = now - _lastWeatherTick;
            _lastWeatherTick = now;
            Weather.Tick(delta, Camera);
        }

        private void SyncLightGizmos()
        {
            var activeScene = ActiveScene == 1 ? Scene : Scene2;
            foreach (var mesh in activeScene.Meshes)
            {
                if (_gizmoMap.TryGetValue(mesh.Name, out int lightIndex))
                {
                    if (lightIndex < activeScene.Lights.Count)
                        mesh.Position = activeScene.Lights[lightIndex].Position;
                }
            }
        }

        private WebGLFrameData BuildWebGLFrame()
        {
            _meshDataCache.Clear();
            var meshDataList = _meshDataCache;

            Mat4 view = Camera.GetViewMatrix();
            Mat4 proj = Mat4.CreatePerspective(
       90f * (MathF.PI / 180f),
       (float)Viewport.ViewportWidth / Viewport.ViewportHeight,
       0.1f, 2000f);

            Mat4 vp = proj * view;

            var activeScene = ActiveScene == 1 ? Scene : Scene2;

            foreach (var mesh in activeScene.Meshes)
            {
                if (mesh == null) continue;

                Mat4 mvp = vp * mesh.WorldMatrix;

                if (mesh.Name.StartsWith("ParticlePool_") || mesh.Name.StartsWith("ParticleGeo_"))
                    continue;

                WebGLMeshUpload? upload = null;

                bool isParticle = mesh.Name.StartsWith("ParticlePool_");
                // Redirect gizmos built from JS-uploaded sources to the source buffer key
                string jsSource = (mesh is SpectralXMesh smjs) ? smjs.JSSourceMesh : null;
                string meshBufferKey = (mesh.Name == "SkySphere") ? "SkySphere" : (jsSource ?? mesh.Name);

                string uploadKey = isParticle
                    ? "ParticleGeo_" + ((mesh as SpectralXMesh)?.TextureDataUrl ?? mesh.Name)
                    : meshBufferKey;

                if (!_uploadedMeshes.Contains(uploadKey))
                {
                    if (jsSource != null)
                    {
                        // Buffer already exists in WebGL from JS upload — skip geometry build
                        _uploadedMeshes.Add(uploadKey);
                    }
                    else
                    {
                        var verts = new List<float>();
                        var normals = new List<float>();
                        var uvs = new List<float>();
                        var matBreaks = new List<int>();
                        var matIndices = new List<int>();

                        int? lastMatIdx = null;
                        int vertsAtLastBreak = 0;

                        var sortedFaces = mesh.Faces;

                        foreach (var face in sortedFaces)
                        {
                            int matIdx = face.MaterialIndex;

                            if (lastMatIdx.HasValue && matIdx != lastMatIdx.Value)
                            {
                                matBreaks.Add(verts.Count / 3 - vertsAtLastBreak);
                                matIndices.Add(lastMatIdx.Value);
                                vertsAtLastBreak = verts.Count / 3;
                            }
                            lastMatIdx = matIdx;

                            int[] vertIndices;
                            int[] uvIndices;

                            if (face.Type == FaceType.Quad)
                            {
                                vertIndices = new[] { face.A, face.B, face.C, face.A, face.C, face.D };
                                uvIndices = new[] { face.UVA, face.UVB, face.UVC, face.UVA, face.UVC, face.UVD };
                            }
                            else
                            {
                                vertIndices = new[] { face.A, face.B, face.C };
                                uvIndices = new[] { face.UVA, face.UVB, face.UVC };
                            }

                            var fv0 = mesh.Vertices[face.A];
                            var fv1 = mesh.Vertices[face.B];
                            var fv2 = mesh.Vertices[face.C];
                            Vector3 edge1 = fv1 - fv0;
                            Vector3 edge2 = fv2 - fv0;
                            Vector3 faceNormal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

                            for (int i = 0; i < vertIndices.Length; i++)
                            {
                                int vertIdx = vertIndices[i];
                                int uvIdx = uvIndices[i];

                                var v = mesh.Vertices[vertIdx];
                                verts.Add(v.X); verts.Add(v.Y); verts.Add(v.Z);

                                Vector3 n;
                                if (mesh.PolygonNormals.Count > 0)
                                {
                                    int[] polyMap = face.Type == FaceType.Quad
                                        ? new[] { 0, 1, 2, 0, 2, 3 }
                                        : new[] { 0, 1, 2 };
                                    int pni = face.PolygonNormalBase + polyMap[i];
                                    n = pni < mesh.PolygonNormals.Count
                                        ? mesh.PolygonNormals[pni]
                                        : faceNormal;
                                }
                                else
                                {
                                    n = faceNormal;
                                    if (mesh.Normals.Count > vertIdx) n = mesh.Normals[vertIdx];
                                }
                                normals.Add(n.X); normals.Add(n.Y); normals.Add(n.Z);

                                if (uvIdx >= 0 && uvIdx < mesh.UVs.Count)
                                { uvs.Add(mesh.UVs[uvIdx].X); uvs.Add(mesh.UVs[uvIdx].Y); }
                                else
                                { uvs.Add(0f); uvs.Add(0f); }
                            }
                        }

                        if (lastMatIdx.HasValue)
                        {
                            matBreaks.Add(verts.Count / 3 - vertsAtLastBreak);
                            matIndices.Add(lastMatIdx.Value);
                        }

                        string texCacheKey = isParticle
                            ? "ParticleGeo_" + mesh.TextureDataUrl
                            : mesh.Name;

                        string? urlToSend = null;
                        if (mesh.HasTexture && mesh.TextureDataUrl != null
                            && !_uploadedTextures.Contains(texCacheKey))
                        {
                            urlToSend = mesh.TextureDataUrl;
                            _uploadedTextures.Add(texCacheKey);
                        }

                        upload = new WebGLMeshUpload
                        {
                            MeshId = uploadKey,
                            Vertices = verts.ToArray(),
                            Normals = normals.ToArray(),
                            UVs = uvs.ToArray(),
                            TextureDataUrl = urlToSend,
                            HasTexture = mesh.HasTexture,
                            TextureWidth = mesh.TextureWidth,
                            TextureHeight = mesh.TextureHeight,
                            TextureIsRawRGBA = mesh.TextureIsRawRGBA,
                            MaterialTextures = mesh.MaterialTextures.Select((t, i) => t).ToArray(),
                            MaterialColors = mesh.MaterialColors != null && mesh.MaterialColors.Count > 0
                                ? mesh.MaterialColors.Select(c => $"{c.X},{c.Y},{c.Z},{c.W}").ToArray()
                                : Array.Empty<string>(),
                            MatBreaks = matBreaks.ToArray(),
                            MatIndices = matIndices.ToArray(),
                        };

                        _uploadedMeshes.Add(uploadKey);
                    }
                }
                else if (isParticle && !_uploadedMeshes.Contains(mesh.Name))
                {
                    string sharedGeoKey = "ParticleGeo_" +
                        ((mesh as SpectralXMesh)?.TextureDataUrl ?? mesh.Name);

                    upload = new WebGLMeshUpload
                    {
                        MeshId = mesh.Name,
                        Vertices = Array.Empty<float>(),
                        Normals = Array.Empty<float>(),
                        UVs = Array.Empty<float>(),
                        HasTexture = false,
                        TextureDataUrl = sharedGeoKey,
                        TextureIsRawRGBA = false,
                        MatBreaks = Array.Empty<int>(),
                        MatIndices = Array.Empty<int>(),
                        MaterialTextures = Array.Empty<string>(),
                        MaterialColors = Array.Empty<string>(),
                    };
                    if (_uploadedMeshes.Contains(sharedGeoKey))
                        _uploadedMeshes.Add(mesh.Name);
                }

                Mat4 model = mesh.WorldMatrix;

                meshDataList.Add(new WebGLMeshData
                {
                    MeshId = meshBufferKey,  // was mesh.Name
                    Mvp = mvp.M,
                    Model = mesh.TransformDirty ? model.M : null,
                    Upload = upload,
                    R = mesh.Color.X,
                    G = mesh.Color.Y,
                    B = mesh.Color.Z,
                    A = mesh.Color.W,
                    IsEmissive = mesh.IsEmissive,
                    EmissiveIntensity = mesh.EmissiveIntensity,
                    UVOffsetX = (mesh is SpectralXMesh sm) ? sm.UVOffsetX : 0f,
                    UVOffsetY = (mesh is SpectralXMesh sm2) ? sm2.UVOffsetY : 0f,
                    UVScaleX = (mesh is SpectralXMesh sm3) ? sm3.UVScaleX : 1f,
                    UVScaleY = (mesh is SpectralXMesh sm4) ? sm4.UVScaleY : 1f,
                    TransformDirty = mesh.TransformDirty,
                    CastsShadow = mesh is SpectralXMesh csm ? csm.CastsShadow : true
                });

                // Clear dirty after sending
                if (mesh is SpectralXMesh dirtyMesh)
                {
                    // Only clear dirty on non-foliage meshes —
                    // foliage is handled by renderFoliage instancing
                    if (!mesh.Name.StartsWith("ParticlePool_"))
                        dirtyMesh.TransformDirty = false;
                }



            }

            var particleTextures = new[]
 {
    ("ParticleGeo_/iAssets/RainDrop01.png",      "/iAssets/RainDrop01.png"),
    ("ParticleGeo_/iAssets/SnowFlake01.png",     "/iAssets/SnowFlake01.png"),
    ("ParticleGeo_/iAssets/GOkuCloud001.png",    "/iAssets/GOkuCloud001.png"),
    ("ParticleGeo_/iAssets/LBolt002.png",        "/iAssets/LBolt002.png"),
};

            foreach (var (cacheKey, texPath) in particleTextures)
            {
                if (_uploadedMeshes.Contains(cacheKey)) continue;

                meshDataList.Add(new WebGLMeshData
                {
                    MeshId = cacheKey,
                    Mvp = Mat4.Identity().M,
                    Model = Mat4.Identity().M,
                    Upload = new WebGLMeshUpload
                    {
                        MeshId = cacheKey,
                        Vertices = Array.Empty<float>(),
                        Normals = Array.Empty<float>(),
                        UVs = Array.Empty<float>(),
                        HasTexture = true,
                        TextureDataUrl = texPath,
                        TextureIsRawRGBA = false,
                        MatBreaks = Array.Empty<int>(),
                        MatIndices = Array.Empty<int>(),
                        MaterialTextures = Array.Empty<string>(),
                        MaterialColors = Array.Empty<string>(),
                    },
                    R = 1f,
                    G = 1f,
                    B = 1f,
                    A = 0f, // alpha 0 — invisible
                    IsEmissive = false,
                    EmissiveIntensity = 0f,
                    UVScaleX = 1f,
                    UVScaleY = 1f,
                });

                _uploadedMeshes.Add(cacheKey);
                _uploadedTextures.Add(cacheKey);
            }



            // Only rebuild active light list when count changes
            int currentLightCount = _activeLightsCache.Count;
            if (_lightsDirty || currentLightCount != _lastLightCount)
            {
                _activeLightsCache.Clear();
                foreach (var l in activeScene.Lights)
                {
                    if (l.Enabled) _activeLightsCache.Add(l);
                    if (_activeLightsCache.Count == 32) break;
                }
                _lastLightCount = currentLightCount;
                _lightsDirty = false;
            }

            var activeLights = _activeLightsCache;
           
            int lightCount = activeLights.Count;

            var lightPositions = new float[lightCount * 3];
            var lightColors = new float[lightCount * 3];
            var lightDirections = new float[lightCount * 3];
            var lightIntensities = new float[lightCount];
            var lightRanges = new float[lightCount];
            var lightTypes = new int[lightCount];
            var lightSpotAngles = new float[lightCount];
            var lightCastsShadows = new bool[lightCount];
            var lightVPs = new float[lightCount][];

            for (int i = 0; i < lightCount; i++)
            {
                var l = activeLights[i];

                lightPositions[i * 3] = l.Position.X;
                lightPositions[i * 3 + 1] = l.Position.Y;
                lightPositions[i * 3 + 2] = l.Position.Z;

                lightColors[i * 3] = l.Color.X;
                lightColors[i * 3 + 1] = l.Color.Y;
                lightColors[i * 3 + 2] = l.Color.Z;

                lightDirections[i * 3] = l.Direction.X;
                lightDirections[i * 3 + 1] = l.Direction.Y;
                lightDirections[i * 3 + 2] = l.Direction.Z;

                lightIntensities[i] = l.Intensity;
                lightRanges[i] = l.Range;
                lightTypes[i] = (int)l.Type;
                lightSpotAngles[i] = l.SpotAngle;
                lightCastsShadows[i] = l.CastsShadows;

                // Build LightVP per shadow casting light
                if (l.CastsShadows && i < 8) // only first 8 lights get shadow maps
                {
                    var lPos = new Vec3(l.Position.X, l.Position.Y, l.Position.Z);

                    if (l.Type == LightType.Directional)
                    {
                        Mat4 lView = Mat4.CreateLookAt(
                            lPos,
                            new Vec3(0f, 0f, 0f),
                            new Vec3(0f, 1f, 0f));

                        Mat4 lProj = Mat4.CreateOrthographic(
         -256f, 256f,
         -256f, 256f,
         1f, 500f);

                        lightVPs[i] = (lProj * lView).M;
                    }
                    else
                    {
                        Mat4 lView = Mat4.CreateLookAt(lPos, new Vec3(0, 8, 2), new Vec3(0, 0, 1));
                        Mat4 lProj = Mat4.CreatePerspective(130f * (MathF.PI / 180f), 1.0f, 0.5f, 50f);
                        lightVPs[i] = (lProj * lView).M;
                    }
                }
                else
                {
                    lightVPs[i] = Mat4.Identity().M;
                }
            }

            var jitter = GetTAAJitter();

            // Camera right and up vectors for billboard
            var camRight = new float[] { view.M[0], view.M[4], view.M[8] };
            var camUp = new float[] { view.M[1], view.M[5], view.M[9] };

            // Build particle instance groups
            List<ParticleInstanceGroup>? particleGroups = null;
            if (ActiveScene == 2 && Weather != null)
            {
                particleGroups = Weather.BuildInstanceGroups();
            }

          

            return new WebGLFrameData
            {
                Width = Viewport.ViewportWidth,
                Height = Viewport.ViewportHeight,
                VP = vp.M,
                CamRight = camRight,
                CamUp = camUp,
                ParticleInstances = particleGroups,
                FoliageInstances = ActiveScene == 2 ? BuildFoliageFrameData() : null,
                Meshes = meshDataList,
                LightCount = lightCount,
                LightPositions = lightPositions,
                LightColors = lightColors,
                LightDirections = lightDirections,
                LightIntensities = lightIntensities,
                LightRanges = lightRanges,
                LightTypes = lightTypes,
                LightSpotAngles = lightSpotAngles,
                LightCastsShadows = lightCastsShadows,
                LightVPs = lightVPs,
                CamX = Camera.Position.X,
                CamY = Camera.Position.Y,
                CamZ = Camera.Position.Z,
                AAMode = (int)ActiveAA,
                JitterX = (int)ActiveAA == 4 ? jitter.x : 0f,
                JitterY = (int)ActiveAA == 4 ? jitter.y : 0f,
                ShadowMode = (int)ActiveShadow,
                // SpectralXS shadow properties — tuneable per scene
                ShadowSoftnessBias = Shadow.SoftnessBias,
                ShadowBlockerSearchRadius = Shadow.BlockerSearchRadius,
                ShadowKernelSize = Shadow.KernelSize,
                ShadowContactSharpness = Shadow.ContactSharpness,
                ShadowDepthBias = Shadow.DepthBias,
                ShadowTintR = Shadow.TintR,
                ShadowTintG = Shadow.TintG,
                ShadowTintB = Shadow.TintB,
                ShadowTintStrength = Shadow.TintStrength,
                ShadowPenumbraTintStrength = Shadow.PenumbraTintStrength,
                TimeOfDay = Sun.TimeOfDay,
                SkyBlend = Sun.SkyBlend,
                SkyZenithR = Sun.SkyZenithColor.X,
                SkyZenithG = Sun.SkyZenithColor.Y,
                SkyZenithB = Sun.SkyZenithColor.Z,
                SkyHorizonR = Sun.SkyHorizonColor.X,
                SkyHorizonG = Sun.SkyHorizonColor.Y,
                SkyHorizonB = Sun.SkyHorizonColor.Z,
                SunDirX = Sun.SunDirection.X,
                SunDirY = Sun.SunDirection.Y,
                SunDirZ = Sun.SunDirection.Z,
                CloudOffset = Sun.CloudOffset,
                StarOffset = Sun.StarOffset,
                CloudScale = Sun.CloudScale,
                StarScale = Sun.StarScale,
                MoonDirX = Sun.MoonDirection.X,
                MoonDirY = Sun.MoonDirection.Y,
                MoonDirZ = Sun.MoonDirection.Z,
                MoonColorR = Sun.MoonColor.X,
                MoonColorG = Sun.MoonColor.Y,
                MoonColorB = Sun.MoonColor.Z,
                MoonGlow = Sun.MoonGlow,
                ActiveScene = this.ActiveScene,
                TileMap = ActiveScene == 2 ? this.TileMap.BuildFrameData() : null,

                TileMapTextures = (ActiveScene == 2 && !_tileMapTexturesUploaded)
    ? SpectralXLandTileMap.TexturePaths
    : null,
                TileMapUploaded = _tileMapTexturesUploaded,
                ViewMatrix = view.M,
                ProjMatrix = Camera.GetProjectionMatrixArray(90f,
    (float)Viewport.ViewportWidth / Viewport.ViewportHeight,
    0.1f, 2000f),
                BrushWorldX = TileMap.BrushWorldX,
                BrushWorldY = TileMap.BrushWorldY,
                BrushRadius = TileMap.BrushSize * SpectralXLandTileMap.TileSize,
                LandscapeActive = TileMap.IsActive,
                SkyDayTexUrl = ActiveScene == 2 ? "/iAssets/SkyDay004SL.png" : null,
                SkyNightTexUrl = ActiveScene == 2 ? "/iAssets/SkyNight005SL.png" : null,
                TextMeshes = BuildTextFrameData(vp),

            };
        }

        private List<WebGLTextData> BuildTextFrameData(Mat4 vp)
        {
            var result = new List<WebGLTextData>();
            var activeScene = ActiveScene == 1 ? Scene : Scene2;
            foreach (var mesh in activeScene.Meshes)
            {
                if (mesh is not SpectralXMesh sm || !sm.IsSDFText) continue;
                var atlas = MeshLibrary.GetFontAtlas(sm.FontKey);
                if (atlas == null) continue;
                Mat4 mvp = vp * mesh.WorldMatrix;
                result.Add(new WebGLTextData
                {
                    MeshId = mesh.Name,
                    Text = sm.Text,
                    FontKey = sm.FontKey,
                    JsonUrl = atlas.JsonUrl,
                    TexUrl = atlas.TextureUrl,
                    FontSize = sm.FontSize,
                    Mvp = mvp.M,
                    R = sm.Color.X,
                    G = sm.Color.Y,
                    B = sm.Color.Z,
                    A = sm.Color.W,
                    OutlineR = sm.OutlineColor.X,
                    OutlineG = sm.OutlineColor.Y,
                    OutlineB = sm.OutlineColor.Z,
                    OutlineA = sm.OutlineColor.W,
                    OutlineWidth = sm.OutlineWidth,
                    LetterSpacing = sm.LetterSpacing,
                    Align = (int)sm.TextAlign,
                    NeedsRebuild = sm.TextDirty,
                    GlowRadius = sm.GlowRadius,
                    GlowStrength = sm.GlowStrength,

                    // Glow color
                    GlowR = sm.GlowColor.X,
                    GlowG = sm.GlowColor.Y,
                    GlowB = sm.GlowColor.Z,
                    GlowA = sm.GlowColor.W,

                    // Shadow blur
                    ShadowBlur = sm.ShadowBlur,
                    ShadowR = sm.ShadowColor.X,
                    ShadowG = sm.ShadowColor.Y,
                    ShadowB = sm.ShadowColor.Z,
                    ShadowA = sm.ShadowColor.W,
                });
                sm.TextDirty = false;
            }
            return result;
        }


        private List<FoliageInstanceGroup> BuildFoliageFrameData()
        {
            // Return cached version — foliage is static, no need to rebuild every frame
            if (_cachedFoliageFrameData != null) return _cachedFoliageFrameData;

            var result = new List<FoliageInstanceGroup>(_foliageGroups.Count);
            foreach (var g in _foliageGroups)
            {
                result.Add(new FoliageInstanceGroup
                {
                    MeshId = g.MeshId,
                    TexKey = g.TexKey,
                    Count = g.Count,
                    Positions = g.Positions,
                    Scales = g.Scales,
                    Rotations = g.Rotations,
                    IsStatic = g.IsStatic,
                });
            }

            _cachedFoliageFrameData = result;
            return result;
        }

        private int _taaFrame = 0;

        private (float x, float y) GetTAAJitter()
        {
            _taaFrame = (_taaFrame + 1) % 8;
            float x = HaltonSequence(_taaFrame, 2) - 0.5f;
            float y = HaltonSequence(_taaFrame, 3) - 0.5f;
            return (x / Viewport.ViewportWidth, y / Viewport.ViewportHeight);
        }

        private float HaltonSequence(int index, int b)
        {
            float f = 1f, r = 0f;
            int i = index;
            while (i > 0)
            {
                f /= b;
                r += f * (i % b);
                i = (int)MathF.Floor(i / b);
            }
            return r;
        }

        private void HandleGamepadInput()
        {
            var movement = _gamepad.GetMovement();
            if (movement.Y < -0.3f) Camera.MoveForward();
            if (movement.Y > 0.3f) Camera.MoveBackward();
            if (movement.X < -0.3f) Camera.StrafeRight();
            if (movement.X > 0.3f) Camera.StrafeLeft();

            var look = _gamepad.GetLook();
            if (System.Math.Abs(look.X) > 0.1f || System.Math.Abs(look.Y) > 0.1f)
                Camera.Look(look.X * 5f, look.Y * 5f);
        }

        [JSInvokable]
        public void Tick() { }

        [JSInvokable("GetFrameDataAsync")]
        public async Task<FrameData> GetFrameDataAsync()
        {
            return Renderer.GetFrameData();
        }

        public void Resize(int width, int height)
        {
            Viewport.SetSize(width, height);
            Renderer.Resize(width, height);
        }

        public void SetTimeOfDay(float t)
        {
            Sun.SetTime(t);
        }



        public async Task HandleKeyDown(KeyboardEventArgs e)
        {
            await Input.HandleKeyDown(e);
        }


        // ── TileMap UI API ───────────────────────────────────────────────────────
        public void ToggleLandscapeActive()
        {
            TileMap.IsActive = !TileMap.IsActive;
        }
        public void SetActiveMaterial(TileMaterial mat)
        {
            TileMap.ActiveMaterial = mat;
        }

        public void SetPaintMode(TilePaintMode mode)
        {
            TileMap.PaintMode = mode;
        }

        public void SetBrushSize(int size)
        {
            TileMap.BrushSize = Math.Clamp(size, 0, 8);
        }

        public void SetTopologyStrength(float strength)
        {
            TileMap.TopologyStrength = Math.Clamp(strength, 0.05f, 1.0f);
        }

        public void SetFlattenTarget(float height)
        {
            TileMap.FlattenTargetHeight = height;
        }
        public void SetBlendStrength(float strength)
        {
            TileMap.BlendStrength = Math.Clamp(strength, 0f, 1f);
        }

        public async Task SaveLandscape()
        {
            var data = TileMap.ExportSaveData();
            var json = JsonSerializer.Serialize(data);
            await _js.InvokeVoidAsync("SpectralLandscape.save", json);
        }

        public async Task LoadLandscape()
        {
            try
            {
                var json = await _js.InvokeAsync<string>("SpectralLandscape.load");
                if (string.IsNullOrEmpty(json)) return;
                var data = JsonSerializer.Deserialize<LandscapeSaveData>(json);
                if (data != null)
                    await TileMap.ImportSaveDataAsync(data);
            }
            catch { /* no saved data — silent fail, flat grass default */ }
        }

        /// <summary>
        /// Called from Blazor mouse handler — screen XY to world ray → tile paint.
        /// </summary>
        public void HandleTileMapMouseDown(float screenX, float screenY)
        {
            if (!TileMap.IsActive) return;
            _isMousePainting = true;
            var (worldX, worldY) = UnprojectToGroundPlane(screenX, screenY);
            TileMap.BrushWorldX = worldX;
            TileMap.BrushWorldY = worldY;
            TryPaintAtScreen(screenX, screenY);
        }

        public void HandleTileMapMouseMove(float screenX, float screenY)
        {
            if (ActiveScene == 2)
            {
                var (worldX, worldY) = UnprojectToGroundPlane(screenX, screenY);
                TileMap.BrushWorldX = worldX;
                TileMap.BrushWorldY = worldY;
                // NO MarkDirty here — brush position goes via WebGLFrameData directly
            }

            if (!_isMousePainting) return;
            TryPaintAtScreen(screenX, screenY);
        }

        public void HandleTileMapMouseUp()
        {
            _isMousePainting = false;
        }

        private void TryPaintAtScreen(float screenX, float screenY)
        {
            if (ActiveScene != 2) return;

            var (worldX, worldY) = UnprojectToGroundPlane(screenX, screenY);
            var (tileX, tileY, hit) = TileMap.WorldToTile(worldX, worldY);



            if (hit)
                TileMap.Paint(tileX, tileY);
        }
        /// <summary>
        /// Unproject screen coordinate to world XY on the Z=0 ground plane.
        /// Reverse of ProjectToScreen — ray from camera through NDC to Z=0.
        /// </summary>
        private (float worldX, float worldY) UnprojectToGroundPlane(float screenX, float screenY)
        {
            // Convert screen to NDC (-1 to 1)
            float ndcX = (screenX / Viewport.ViewportWidth) * 2f - 1f;
            float ndcY = 1f - (screenY / Viewport.ViewportHeight) * 2f;

            Mat4 view = Camera.GetViewMatrix();
            Mat4 proj = Mat4.CreatePerspective(
      90f * (MathF.PI / 180f),
      (float)Viewport.ViewportWidth / Viewport.ViewportHeight,
      0.1f, 2000f);

            // Inverse view-projection
            Mat4 vp = proj * view;
            Mat4 vpInv = Mat4.Invert(vp);

            // Unproject near and far points
            var nearNDC = new Vector4(ndcX, ndcY, -1f, 1f);
            var farNDC = new Vector4(ndcX, ndcY, 1f, 1f);

            Vector4 nearWorld = Transform(vpInv, nearNDC);
            Vector4 farWorld = Transform(vpInv, farNDC);

            // Perspective divide
            var nearPos = new Vector3(nearWorld.X, nearWorld.Y, nearWorld.Z) / nearWorld.W;
            var farPos = new Vector3(farWorld.X, farWorld.Y, farWorld.Z) / farWorld.W;

            // Ray direction
            var rayDir = Vector3.Normalize(farPos - nearPos);

            // Intersect with Z=0 plane — solve: nearPos.Z + t * rayDir.Z = 0
            if (MathF.Abs(rayDir.Z) < 0.0001f)
                return (0f, 0f); // Ray parallel to ground — no hit

            float t = -nearPos.Z / rayDir.Z;
            float worldX = nearPos.X + t * rayDir.X;
            float worldY = nearPos.Y + t * rayDir.Y;




            return (worldX, worldY);
        }

        private static Vector4 Transform(Mat4 m, Vector4 v)
        {
            return new Vector4(
                m.M[0] * v.X + m.M[4] * v.Y + m.M[8] * v.Z + m.M[12] * v.W,
                m.M[1] * v.X + m.M[5] * v.Y + m.M[9] * v.Z + m.M[13] * v.W,
                m.M[2] * v.X + m.M[6] * v.Y + m.M[10] * v.Z + m.M[14] * v.W,
                m.M[3] * v.X + m.M[7] * v.Y + m.M[11] * v.Z + m.M[15] * v.W);
        }



        [JSInvokable("OnTileTexturesUploaded")]
        public void OnTileTexturesUploaded()
        {
            _tileMapTexturesUploaded = true;
            Console.WriteLine("[TileMap] Texture upload confirmed by JS");
        }






    }














    public class WebGLFrameData
    {
        [JsonPropertyName("width")]
        public int Width { get; set; }
        [JsonPropertyName("height")]
        public int Height { get; set; }
        [JsonPropertyName("meshes")]
        public List<WebGLMeshData> Meshes { get; set; } = new();

        // Light buffer — replaces single light floats
        [JsonPropertyName("lightCount")]
        public int LightCount { get; set; }
        [JsonPropertyName("lightPositions")]
        public float[] LightPositions { get; set; } = Array.Empty<float>();
        [JsonPropertyName("lightColors")]
        public float[] LightColors { get; set; } = Array.Empty<float>();
        [JsonPropertyName("lightIntensities")]
        public float[] LightIntensities { get; set; } = Array.Empty<float>();
        [JsonPropertyName("lightRanges")]
        public float[] LightRanges { get; set; } = Array.Empty<float>();
        [JsonPropertyName("lightTypes")]
        public int[] LightTypes { get; set; } = Array.Empty<int>();
        [JsonPropertyName("lightDirections")]
        public float[] LightDirections { get; set; } = Array.Empty<float>();
        [JsonPropertyName("lightSpotAngles")]
        public float[] LightSpotAngles { get; set; } = Array.Empty<float>();
        [JsonPropertyName("lightCastsShadows")]
        public bool[] LightCastsShadows { get; set; } = Array.Empty<bool>();
        [JsonPropertyName("lightVPs")]
        public float[][] LightVPs { get; set; } = Array.Empty<float[]>();

        // Camera
        [JsonPropertyName("camX")]
        public float CamX { get; set; }
        [JsonPropertyName("camY")]
        public float CamY { get; set; }
        [JsonPropertyName("camZ")]
        public float CamZ { get; set; }

        // AA
        [JsonPropertyName("aaMode")]
        public int AAMode { get; set; }
        [JsonPropertyName("jitterX")]
        public float JitterX { get; set; }
        [JsonPropertyName("jitterY")]
        public float JitterY { get; set; }

        // Shadow
        [JsonPropertyName("shadowMode")]
        public int ShadowMode { get; set; }

        // SpectralXS shadow properties — only read by fsSourceSpectralXSV1
        [JsonPropertyName("shadowSoftnessBias")]
        public float ShadowSoftnessBias { get; set; }

        [JsonPropertyName("shadowBlockerSearchRadius")]
        public float ShadowBlockerSearchRadius { get; set; }

        [JsonPropertyName("shadowKernelSize")]
        public float ShadowKernelSize { get; set; }

        [JsonPropertyName("shadowContactSharpness")]
        public float ShadowContactSharpness { get; set; }

        [JsonPropertyName("shadowDepthBias")]
        public float ShadowDepthBias { get; set; }

        [JsonPropertyName("shadowTintR")]
        public float ShadowTintR { get; set; }

        [JsonPropertyName("shadowTintG")]
        public float ShadowTintG { get; set; }

        [JsonPropertyName("shadowTintB")]
        public float ShadowTintB { get; set; }

        [JsonPropertyName("shadowTintStrength")]
        public float ShadowTintStrength { get; set; }
        [JsonPropertyName("shadowPenumbraTintStrength")]
        public float ShadowPenumbraTintStrength { get; set; }


        // ── Sky / Sun System ────────────────────────────────────────────────────
        // Add these properties to WebGLFrameData, after ShadowPenumbraTintStrength

        [JsonPropertyName("timeOfDay")]
        public float TimeOfDay { get; set; }

        /// <summary>
        /// 0.0 = full day texture visible, 1.0 = full night texture visible.
        /// Crossfade computed by SpectralXSun.
        /// </summary>
        [JsonPropertyName("skyBlend")]
        public float SkyBlend { get; set; }

        /// <summary>
        /// Sky gradient zenith color — top of skysphere.
        /// Shifts from deep navy (night) through bright blue (day).
        /// </summary>
        [JsonPropertyName("skyZenithR")]
        public float SkyZenithR { get; set; }

        [JsonPropertyName("skyZenithG")]
        public float SkyZenithG { get; set; }

        [JsonPropertyName("skyZenithB")]
        public float SkyZenithB { get; set; }

        /// <summary>
        /// Sky gradient horizon color — equator of skysphere.
        /// Shifts from orange-pink (sunrise/sunset) through pale blue (noon).
        /// </summary>
        [JsonPropertyName("skyHorizonR")]
        public float SkyHorizonR { get; set; }

        [JsonPropertyName("skyHorizonG")]
        public float SkyHorizonG { get; set; }

        [JsonPropertyName("skyHorizonB")]
        public float SkyHorizonB { get; set; }

        /// <summary>
        /// Sun direction vector — normalized, points FROM sun TOWARD origin.
        /// Used by sky shader to position the sun disk in the gradient.
        /// </summary>
        [JsonPropertyName("sunDirX")]
        public float SunDirX { get; set; }

        [JsonPropertyName("sunDirY")]
        public float SunDirY { get; set; }

        [JsonPropertyName("sunDirZ")]
        public float SunDirZ { get; set; }

        // After SunDirZ property, add:

        [JsonPropertyName("cloudOffset")]
        public float CloudOffset { get; set; }

        [JsonPropertyName("starOffset")]
        public float StarOffset { get; set; }

        [JsonPropertyName("cloudScale")]
        public float CloudScale { get; set; }

        [JsonPropertyName("starScale")]
        public float StarScale { get; set; }

        [JsonPropertyName("moonDirX")]
        public float MoonDirX { get; set; }

        [JsonPropertyName("moonDirY")]
        public float MoonDirY { get; set; }

        [JsonPropertyName("moonDirZ")]
        public float MoonDirZ { get; set; }

        [JsonPropertyName("moonColorR")]
        public float MoonColorR { get; set; }

        [JsonPropertyName("moonColorG")]
        public float MoonColorG { get; set; }

        [JsonPropertyName("moonColorB")]
        public float MoonColorB { get; set; }

        [JsonPropertyName("moonGlow")]
        public float MoonGlow { get; set; }

        [JsonPropertyName("particleInstances")]
        public List<ParticleInstanceGroup>? ParticleInstances { get; set; }

        [JsonPropertyName("foliageInstances")]
        public List<FoliageInstanceGroup>? FoliageInstances { get; set; }

        [JsonPropertyName("vp")]
        public float[] VP { get; set; } = Array.Empty<float>();

        [JsonPropertyName("camRight")]
        public float[] CamRight { get; set; } = Array.Empty<float>();

        [JsonPropertyName("camUp")]
        public float[] CamUp { get; set; } = Array.Empty<float>();

        [JsonPropertyName("tileMap")]
        public TileMapFrameData? TileMap { get; set; }

        [JsonPropertyName("tileMapTextures")]
        public string[]? TileMapTextures { get; set; }

        [JsonPropertyName("tileMapUploaded")]
        public bool TileMapUploaded { get; set; }


        [JsonPropertyName("viewMatrix")]
        public float[]? ViewMatrix { get; set; }

        [JsonPropertyName("projMatrix")]
        public float[]? ProjMatrix { get; set; }

        [JsonPropertyName("activeScene")]
        public int ActiveScene { get; set; }

        [JsonPropertyName("brushWorldX")]
        public float BrushWorldX { get; set; }

        [JsonPropertyName("brushWorldY")]
        public float BrushWorldY { get; set; }

        [JsonPropertyName("brushRadius")]
        public float BrushRadius { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
        [JsonPropertyName("landscapeActive")]
        public bool LandscapeActive { get; set; }

        [JsonPropertyName("skyDayTexUrl")]
        public string? SkyDayTexUrl { get; set; }

        [JsonPropertyName("skyNightTexUrl")]
        public string? SkyNightTexUrl { get; set; }

        [JsonPropertyName("textMeshes")]
        public List<WebGLTextData>? TextMeshes { get; set; }

    }

    public class WebGLMeshUpload
    {
        [System.Text.Json.Serialization.JsonPropertyName("meshId")]
        public string MeshId { get; set; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("vertices")]
        public float[] Vertices { get; set; } = Array.Empty<float>();
        [System.Text.Json.Serialization.JsonPropertyName("normals")]
        public float[] Normals { get; set; } = Array.Empty<float>();
        [System.Text.Json.Serialization.JsonPropertyName("uvs")]
        public float[] UVs { get; set; } = Array.Empty<float>();
        [System.Text.Json.Serialization.JsonPropertyName("textureDataUrl")]
        public string? TextureDataUrl { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("hasTexture")]
        public bool HasTexture { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("textureWidth")]
        public int TextureWidth { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("textureHeight")]
        public int TextureHeight { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("textureIsRawRGBA")]
        public bool TextureIsRawRGBA { get; set; }

        [JsonPropertyName("materialTextures")]
        public string[] MaterialTextures { get; set; } = Array.Empty<string>();

        [JsonPropertyName("materialColors")]
        public string[] MaterialColors { get; set; } = Array.Empty<string>();




        [JsonPropertyName("matBreaks")]
        public int[] MatBreaks { get; set; } = Array.Empty<int>();
        [JsonPropertyName("matIndices")]
        public int[] MatIndices { get; set; } = Array.Empty<int>();

    }




    public class WebGLMeshData
    {
        [System.Text.Json.Serialization.JsonPropertyName("meshId")]
        public string MeshId { get; set; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("mvp")]
        public float[] Mvp { get; set; } = Array.Empty<float>();
        [System.Text.Json.Serialization.JsonPropertyName("upload")]
        public WebGLMeshUpload? Upload { get; set; } = null;
        [System.Text.Json.Serialization.JsonPropertyName("r")]
        public float R { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("g")]
        public float G { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("b")]
        public float B { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("a")]
        public float A { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("model")]
        public float[]? Model { get; set; } = null;

        [JsonPropertyName("isEmissive")]
        public bool IsEmissive { get; set; }

        [JsonPropertyName("emissiveIntensity")]
        public float EmissiveIntensity { get; set; }
        [JsonPropertyName("uvOffsetX")]
        public float UVOffsetX { get; set; }
        [JsonPropertyName("uvOffsetY")]
        public float UVOffsetY { get; set; }

        [JsonPropertyName("uvScaleX")]
        public float UVScaleX { get; set; }
        [JsonPropertyName("uvScaleY")]
        public float UVScaleY { get; set; }

        [JsonPropertyName("transformDirty")]
        public bool TransformDirty { get; set; }
        [JsonPropertyName("castsShadow")]
        public bool CastsShadow { get; set; } = true;

    }

    public class WebGLTextData
    {
        [JsonPropertyName("meshId")]
        public string MeshId { get; set; } = "";
        [JsonPropertyName("text")]
        public string Text { get; set; } = "";
        [JsonPropertyName("fontKey")]
        public string FontKey { get; set; } = "";
        [JsonPropertyName("jsonUrl")]
        public string JsonUrl { get; set; } = "";
        [JsonPropertyName("texUrl")]
        public string TexUrl { get; set; } = "";
        [JsonPropertyName("fontSize")]
        public float FontSize { get; set; } = 1f;
        [JsonPropertyName("mvp")]
        public float[] Mvp { get; set; } = Array.Empty<float>();
        [JsonPropertyName("r")] public float R { get; set; } = 1f;
        [JsonPropertyName("g")] public float G { get; set; } = 1f;
        [JsonPropertyName("b")] public float B { get; set; } = 1f;
        [JsonPropertyName("a")] public float A { get; set; } = 1f;
        [JsonPropertyName("outlineR")] public float OutlineR { get; set; }
        [JsonPropertyName("outlineG")] public float OutlineG { get; set; }
        [JsonPropertyName("outlineB")] public float OutlineB { get; set; }
        [JsonPropertyName("outlineA")] public float OutlineA { get; set; }
        [JsonPropertyName("outlineWidth")] public float OutlineWidth { get; set; }
        [JsonPropertyName("letterSpacing")] public float LetterSpacing { get; set; }
        [JsonPropertyName("align")] public int Align { get; set; }
        [JsonPropertyName("needsRebuild")] public bool NeedsRebuild { get; set; } = true;
        [JsonPropertyName("glowRadius")] public float GlowRadius { get; set; } = 0.25f;
        [JsonPropertyName("glowStrength")] public float GlowStrength { get; set; } = 0.8f;

        // Glow color
        [JsonPropertyName("glowR")] public float GlowR { get; set; } = 1f;
        [JsonPropertyName("glowG")] public float GlowG { get; set; } = 1f;
        [JsonPropertyName("glowB")] public float GlowB { get; set; } = 1f;
        [JsonPropertyName("glowA")] public float GlowA { get; set; } = 1f;

        // Shadow blur
        [JsonPropertyName("shadowBlur")] public float ShadowBlur { get; set; } = 0f;
        [JsonPropertyName("shadowR")] public float ShadowR { get; set; } = 0f;
        [JsonPropertyName("shadowG")] public float ShadowG { get; set; } = 0f;
        [JsonPropertyName("shadowB")] public float ShadowB { get; set; } = 0f;
        [JsonPropertyName("shadowA")] public float ShadowA { get; set; } = 0f;
    }


    public class ParticleInstanceGroup
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("offsets")]
        public float[] Offsets { get; set; } = Array.Empty<float>(); // xyz per instance

        [JsonPropertyName("colors")]
        public float[] Colors { get; set; } = Array.Empty<float>(); // rgba per instance

        [JsonPropertyName("sizes")]
        public float[] Sizes { get; set; } = Array.Empty<float>(); // 1 float per instance

        [JsonPropertyName("texKey")]
        public string TexKey { get; set; } = ""; // matches _textureCache key in JS
    }


    public class TileMapFrameData
    {
        [JsonPropertyName("heights")]
        public float[] Heights { get; set; } = Array.Empty<float>();

        [JsonPropertyName("normals")]
        public float[] Normals { get; set; } = Array.Empty<float>();

        [JsonPropertyName("materials")]
        public int[] Materials { get; set; } = Array.Empty<int>();

        [JsonPropertyName("blendWeights")]
        public float[] BlendWeights { get; set; } = Array.Empty<float>();

        [JsonPropertyName("blendMaterials")]
        public int[] BlendMaterials { get; set; } = Array.Empty<int>();

        [JsonPropertyName("isDirty")]
        public bool IsDirty { get; set; }

        [JsonPropertyName("isFullUpload")]
        public bool IsFullUpload { get; set; }

        [JsonPropertyName("dirtyX")]
        public int DirtyX { get; set; }

        [JsonPropertyName("dirtyY")]
        public int DirtyY { get; set; }

        [JsonPropertyName("dirtyW")]
        public int DirtyW { get; set; }

        [JsonPropertyName("dirtyH")]
        public int DirtyH { get; set; }

    }

    public class FoliageInstanceGroup
    {
        [JsonPropertyName("meshId")]
        public string MeshId { get; set; } = "";

        [JsonPropertyName("texKey")]
        public string TexKey { get; set; } = "";

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("positions")]
        public float[] Positions { get; set; } = Array.Empty<float>();

        [JsonPropertyName("scales")]
        public float[] Scales { get; set; } = Array.Empty<float>();

        [JsonPropertyName("rotations")]
        public float[] Rotations { get; set; } = Array.Empty<float>();

        [JsonPropertyName("isStatic")]
        public bool IsStatic { get; set; } = true;

        [JsonIgnore]
        public bool Uploaded { get; set; } = false;
    }





}