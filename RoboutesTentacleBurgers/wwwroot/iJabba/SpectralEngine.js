








window.SpectralGLInterop = (function () {

    // ============================================================
    // Core WebGL Engine State
    // ============================================================
    // Main WebGL objects and global rendering state.
    // These are initialized during engine startup and reused
    // throughout the lifetime of the renderer.

    let _canvas = null;
    let _gl = null;
    let _dotnetRef = null;
    let _animationHandle = null;

    let _programs = [null, null, null, null, null];
    let _activeProgram = null;
    let _activeLocs = null;
    let _programLocations = [];

    let _textureCache = {};
    let _meshBuffers = {};
    let _textureReady = {};

    // Meshes parsed before WebGL is ready are stored here and
    // uploaded to GPU during the first render frame.
    let _pendingUploads = [];


    // ============================================================
    // Shadow Rendering System
    // ============================================================
    // Handles shadow map generation and shadow rendering pipeline.
    // Multiple lights can generate shadows, but only the first
    // MAX_SHADOW_LIGHTS allocate shadow framebuffers.

    let _shadowProgram = null;
    let _shadowFbos = [];
    let _shadowDepthTexs = [];
    let _shadowLightMVPLoc = null;
    let _shadowModelLoc = null;
    let _shadowPosLoc = null;

    const MAX_LIGHTS = 32;
    const MAX_SHADOW_LIGHTS = 8; // shadow FBOs only allocated for first 8 lights

    let _shadowInstancedLoc = null;
    let _shadowInstPosLoc = null;


    // ------------------------------------------------------------
    // Shadow Map Configuration
    // ------------------------------------------------------------
    // GPU shadow resolution settings. Maximum size is clamped to
    // avoid crashes on lower-end devices.

    const SHADOW_SIZE_MAX = 4096;
    let SHADOW_SIZE = 1024; // set during init after GL is ready
    const SHADOW_SIZE_MSAA = 4096;

    let _shadowMode = 0;

    let _shadowMapLoc, _lightVPLoc;
    let _shadowTexCoordLoc = null;
    let _shadowHasTextureLoc = null;
    let _shadowTextureLoc = null;
    let _shadowAlphaThresholdLoc = null;


    // ============================================================
    // Particle Rendering System
    // ============================================================
    // Instanced particle renderer used for effects such as smoke,
    // sparks, fire, etc. Uses a shared quad with instance buffers.

    let _particleProgram = null;
    let _particleQuadVbo = null;
    let _particleQuadUbo = null;
    let _particleInstanceBuffers = {};

    // Cached particle uniform + attribute locations
    let _pLocs = null;


    // ============================================================
    // General Rendering Optimizations
    // ============================================================
    // Small cached helpers that reduce repeated GL lookups.

    let _quadPosLocs = {};


    // ============================================================
    // FXAA (Fast Approximate Anti-Aliasing)
    // ============================================================
    // Single-pass screen-space anti-aliasing technique.

    let _fxaaProgram = null;
    let _fxaaFbo = null;
    let _fxaaColorTex = null;

    let _fxaaPosLoc = null;
    let _fxaaTexLoc = null;
    let _fxaaResLoc = null;

    let _fxaaQuadVbo = null;
    let _fxaaTexWidth = 0;
    let _fxaaTexHeight = 0;

    // Shared fullscreen quad used by post-processing passes
    let _fullscreenQuadVbo = null;


    // ============================================================
    // SMAA (Subpixel Morphological Anti-Aliasing)
    // ============================================================
    // Multi-pass AA technique that detects edges then blends them.

    let _smaaProgram1 = null; // Edge detection
    let _smaaProgram2 = null; // Blend weights
    let _smaaProgram3 = null; // Neighbourhood blend

    let _smaaEdgeFbo = null;
    let _smaaBlendFbo = null;

    let _smaaEdgeTex = null;
    let _smaaBlendTex = null;

    let _smaaQuadVbo = null; // shared fullscreen quad with FXAA


    // ============================================================
    // TAA (Temporal Anti-Aliasing)
    // ============================================================
    // Accumulates frames over time using history buffers to
    // smooth edges and reduce shimmering.

    let _taaProgram = null;

    let _taaCurrentFbo = null;
    let _taaHistoryFbo = null;

    let _taaCurrentTex = null;
    let _taaHistoryTex = null;

    let _taaTexWidth = 0;
    let _taaTexHeight = 0;


    // ============================================================
    // SpectralAA (Experimental Anti-Aliasing)
    // ============================================================
    // Experimental anti-aliasing pipeline based on spectral edge
    // detection and geometric reconstruction.

    let _spectralProgram1 = null; // Edge detect + angle
    let _spectralProgram2 = null; // Angle map refinement
    let _spectralProgram3 = null; // Geometric composite

    let _spectralEdgeFbo = null;
    let _spectralEdgeTex = null;


    // ============================================================
    // SpectralAA V2
    // ============================================================
    // Improved version with staircase detection and triangle fill.

    let _spectralV2Program1 = null; // Staircase detection
    let _spectralV2Program2 = null; // Triangle fill composite

    let _spectralV2EdgeFbo = null;
    let _spectralV2EdgeTex = null;


    // ============================================================
    // SpectralAA V3
    // ============================================================
    // Advanced reconstruction pipeline that performs:
    // 1) Binary edge classification
    // 2) Line reconstruction
    // 3) Triangle coverage fill

    let _spectralV3Program1 = null; // Binary edge classification
    let _spectralV3Program2 = null; // Line reconstruction
    let _spectralV3Program3 = null; // Triangle coverage fill

    let _spectralV3EdgeFbo = null;
    let _spectralV3EdgeTex = null;

    let _spectralV3LineFbo = null;
    let _spectralV3LineTex = null;


    // ============================================================
    // Sky Rendering System
    // ============================================================
    // Handles skybox rendering including day/night transitions
    // using both 2D textures and cubemaps.

    let _skyProgram = null;

    let _skyDayTex = null;
    let _skyNightTex = null;

    let _skyTexsLoaded = { day: false, night: false };

    let _skyCubeDayTex = null;
    let _skyCubeNightTex = null;

    let _skyCubeLoaded = { day: false, night: false };

    let _lastSkyDayUrl = null;
    let _lastSkyNightUrl = null;
    let _lastGL = null;
    // ============================================================
    // Text Rendering System
    // ============================================================
    // GPU text rendering pipeline.

    let _textProgram = null;
    let _textLocs = null;


    // ============================================================
    // TileMap GPU Rendering System
    // ============================================================
    // Large terrain grid rendered on the GPU. Vertex positions are
    // static while height and normals are updated dynamically.

    let _tileProgram = null;

    let _tileGridVAO = null;
    let _tileGridVBO = null;     // static XY positions
    let _tileHeightVBO = null;   // dynamic Z heights
    let _tileNormalVBO = null;   // dynamic vertex normals
    let _tileIBO = null;         // static index buffer

    let _tileMatTex = null;      // dynamic material texture
    let _tileTextures = {};
    let _tileTexturesReady = false;

    let _tileIdxCount = 0;

    let _tileUniforms = null;
    let _tileViewMatrixF32 = null;
    let _tileProjMatrixF32 = null;


    // ------------------------------------------------------------
    // Tile Grid Configuration
    // ------------------------------------------------------------

    const GRID_SIZE = 512;
    const GRID_VERTS = (GRID_SIZE + 1) * (GRID_SIZE + 1);

    const TILE_SIZE = 1.0;

    const GRID_ORIGIN_X = -(GRID_SIZE * TILE_SIZE) / 2.0;
    const GRID_ORIGIN_Y = -(GRID_SIZE * TILE_SIZE) / 2.0;


    // ============================================================
    // Foliage Rendering System
    // ============================================================
    // Instanced foliage renderer used for grass, bushes, trees,
    // and other vegetation elements.

    let _foliageProgram = null;
    let _foliageBuffers = {};
    let _fLocs = null;

    // ============================================================
    // Scrollbar Rendering System
    // ============================================================
    // Maybe be remove and added into c# rendering via meshes
    
    let _scrollbarProgram = null;
    let _scrollbarLocs = null;
    let _scrollbarVbo = null;
    let _scrollbarMinZ = -60;
    let _scrollbarMaxZ = 10;
    let _scrollbarCurrentZ = 10;
    let _scrollbarThumbDragging = false;
    let _scrollbarThumbY = 0;
    let _scrollbarThumbH = 0;
    let _scrollbarGlowPhase = 0;


    // ============================================================
    // Shaders
    // ============================================================


    const shadowVsSource = `#version 300 es
in vec3 aPosition;
in vec2 aTexCoord;
in vec3 aInstancePos;
uniform mat4 uLightVP;
uniform mat4 uModel;
uniform int uIsInstanced;
out vec2 vTexCoord;
void main() {
    vec3 worldPos = aPosition;
    if (uIsInstanced == 1) {
        float c = 1.0; float s = 0.0;
        worldPos = vec3(
            aPosition.x * c - aPosition.y * s + aInstancePos.x,
            aPosition.x * s + aPosition.y * c + aInstancePos.y,
            aPosition.z + aInstancePos.z
        );
        gl_Position = uLightVP * vec4(worldPos, 1.0);
    } else {
       vec4 pos = uLightVP * uModel * vec4(aPosition, 1.0);
pos.z -= 0.01;
gl_Position = pos;
    }
    vTexCoord = aTexCoord;
}
`;

    const shadowFsSource = `#version 300 es
precision mediump float;
in vec2 vTexCoord;
uniform sampler2D uShadowTexture;
uniform bool uShadowHasTexture;
uniform float uShadowAlphaThreshold;
out vec4 fragColor;
void main() {
    if (uShadowHasTexture) {
        float a = texture(uShadowTexture, vTexCoord).a;
        if (a < uShadowAlphaThreshold) discard;
    }
    fragColor = vec4(1.0);
}
`;

    const vsSource = `#version 300 es
        in vec3 aPosition;
        in vec3 aNormal;
        in vec2 aTexCoord;
        uniform vec2 uUVOffset;
        uniform vec2 uUVScale;
        uniform mat4 uMVP;
        uniform mat4 uModel;
        uniform mat4 uLightVP;
        uniform vec2 uJitter;
        out vec3 vNormal;
        out vec3 vWorldPos;
        out vec2 vTexCoord;
        out vec4 vShadowCoord;
        void main() {
            vec4 worldPos = uModel * vec4(aPosition, 1.0);
            vec4 pos = uMVP * vec4(aPosition, 1.0);
            pos.xy += uJitter * pos.w;
            gl_Position = pos;
            vWorldPos = worldPos.xyz;
            vNormal = normalize(mat3(uModel) * aNormal);
              // ...
                vTexCoord = aTexCoord  * uUVScale + uUVOffset;
            vShadowCoord = uLightVP * worldPos;
        }
         `;


    const textVsSrc = `#version 300 es
in vec3 aPosition;
in vec2 aTexCoord;
uniform mat4 uMVP;
out vec2 vTexCoord;
void main() {
    vTexCoord = aTexCoord;
    gl_Position = uMVP * vec4(aPosition, 1.0);
}`;

    const textFsSrc = `#version 300 es
precision mediump float;
in vec2 vTexCoord;
uniform sampler2D uAtlas;
uniform vec4 uColor;
uniform vec4 uOutlineColor;
uniform float uOutlineWidth;
uniform float uSoftness;
uniform float uGlowRadius;
uniform float uGlowStrength;
out vec4 fragColor;

float median(float r, float g, float b) {
    return max(min(r,g), min(max(r,g),b));
}

void main() {
    vec3 s = texture(uAtlas, vTexCoord).rgb;
    float d = median(s.r, s.g, s.b);
    float w = fwidth(d) * 0.5;
    float alpha = smoothstep(0.5 - w - uSoftness, 0.5 + w + uSoftness, d);
    if (alpha < 0.001) discard;

    float glowAlpha = smoothstep(0.5 - uGlowRadius, 0.5 + uGlowRadius * 0.5, d);
    vec3 glowColor = uColor.rgb * 2.0;
    float glow = (1.0 - alpha) * glowAlpha * uGlowStrength;

    if (uOutlineWidth > 0.0) {
        float outlineA = smoothstep(
            0.5 - uOutlineWidth - w,
            0.5 - uOutlineWidth + w, d);
        vec4 outline = vec4(uOutlineColor.rgb, uOutlineColor.a * outlineA);
        fragColor = mix(outline, vec4(uColor.rgb, uColor.a * alpha), alpha);
    } else {
        fragColor = vec4(uColor.rgb, uColor.a * alpha);
    }

    fragColor.rgb = mix(fragColor.rgb, glowColor, glow);
    fragColor.a = max(fragColor.a, glow);
}`;

    const particleVsSrc = `#version 300 es
        in vec3 aPosition;
        in vec2 aTexCoord;
        in vec3 iOffset;      // per-instance world position
        in vec4 iColor;       // per-instance color
        in float iSize;       // per-instance scale
        uniform mat4 uVP;     // view-projection only, no model
        uniform vec3 uCamRight;
        uniform vec3 uCamUp;
        out vec2 vTexCoord;
        out vec4 vColor;
        void main() {
            vec3 worldPos = iOffset
                + uCamRight * aPosition.x * iSize
                + uCamUp    * aPosition.y * iSize;
            gl_Position = uVP * vec4(worldPos, 1.0);
            vTexCoord = aTexCoord;
            vColor = iColor;
        }`;

    const particleFsSrc = `#version 300 es
        precision mediump float;
        in vec2 vTexCoord;
        in vec4 vColor;
        uniform sampler2D uTexture;
        uniform bool uHasTexture;
        out vec4 fragColor;
        void main() {
            vec4 base = uHasTexture ? texture(uTexture, vTexCoord) : vColor;
            if (base.a < 0.1) discard;
            fragColor = base * vColor;
        }`;


        /*
    // ?? Sky Vertex Shader ????????????????????????????????????????????????????
    const skyVsSrc = `#version 300 es
                in vec3 aPosition;
                in vec2 aTexCoord;
                uniform mat4 uMVP;
                out vec2 vTexCoord;
                out vec3 vLocalPos;
                void main() {
                    vTexCoord = aTexCoord;
                    vLocalPos = aPosition;
                    gl_Position = uMVP * vec4(aPosition, 1.0);
                }
            `;


    const skyFsSrc = `#version 300 es
precision mediump float;
in vec2 vTexCoord;
in vec3 vLocalPos;

uniform samplerCube uDayTex;
uniform samplerCube uNightTex;
uniform float uSkyBlend;
uniform vec3  uZenithColor;
uniform vec3  uHorizonColor;
uniform vec3  uSunDir;
uniform float uTimeOfDay;
uniform float uCloudOffset;
uniform float uStarOffset;
uniform float uCloudScale;
uniform float uStarScale;
uniform vec3  uMoonDir;
uniform vec3  uMoonColor;
uniform float uMoonGlow;

out vec4 fragColor;

void main() {
    vec3 dir = normalize(vLocalPos);

    float vertFactor     = clamp(dir.z * 0.5 + 0.5, 0.0, 1.0);
    float gradientFactor = pow(vertFactor, 0.6);
    vec3  gradientColor  = mix(uHorizonColor, uZenithColor, gradientFactor);

    // Cubemap sampling — no UV math, no seam, scales with no artifacts
    vec3 cloudDir = vec3(dir.x * uCloudScale, dir.y * uCloudScale, dir.z);
    vec3 starDir  = vec3(dir.x * uStarScale,  dir.y * uStarScale,  dir.z);

    vec4  dayTex   = texture(uDayTex,   cloudDir);
    vec4  nightTex = texture(uNightTex, starDir);

    float poleAngle = acos(abs(dir.z));
    float poleMask  = smoothstep(0.0, 0.25, poleAngle);
    vec3  poleColor = dir.z > 0.0 ? uZenithColor : uHorizonColor;
    float horizonFade = smoothstep(0.0, 0.18, abs(dir.z));

    float dayAlpha = max(dayTex.r, max(dayTex.g, dayTex.b));
    dayAlpha *= poleMask * horizonFade;

    float nightAlpha = max(nightTex.r, max(nightTex.g, nightTex.b));
    nightAlpha *= uSkyBlend * 2.5 * poleMask;
    nightAlpha  = clamp(nightAlpha, 0.0, 1.0);

    vec3 color = gradientColor;
    color = mix(color, dayTex.rgb,   dayAlpha);
    color = mix(color, nightTex.rgb, nightAlpha);
    color = mix(poleColor, color, poleMask);

    float horizonBand     = 1.0 - smoothstep(0.0, 0.22, abs(dir.z));
    float horizonStrength = horizonBand * 0.12 * (1.0 - uSkyBlend * 0.5);
    color = mix(color, uHorizonColor, horizonStrength);

    vec3  topPoleColor = mix(uHorizonColor, uZenithColor, 1.0);
    float topApex      = 1.0 - smoothstep(0.0, 0.65, acos(clamp(dir.z, -1.0, 1.0)));
    color = mix(color, topPoleColor, topApex * 0.92);

    vec3  botPoleColor = mix(uHorizonColor, uZenithColor, 0.0);
    float botApex      = 1.0 - smoothstep(0.0, 0.65, acos(clamp(-dir.z, -1.0, 1.0)));
    color = mix(color, botPoleColor, botApex * 0.88);

    vec3  sunPos      = -uSunDir;
    float sunElevZ    = sunPos.z;
    float sunAngle    = acos(clamp(dot(dir, sunPos), -1.0, 1.0));
    vec3  sunDiskColor = mix(vec3(1.0, 0.40, 0.05), vec3(1.0, 0.98, 0.85), clamp(sunElevZ * 1.5, 0.0, 1.0));
    float sunCore     = 1.0 - smoothstep(0.04, 0.07, sunAngle);
    float sunGlow     = 1.0 - smoothstep(0.07, 0.22, sunAngle);
    float sunScatter  = (1.0 - smoothstep(0.15, 0.55, sunAngle)) * clamp(1.0 - sunElevZ * 2.5, 0.0, 1.0);
    float sunVisible  = clamp(1.0 - uSkyBlend * 2.5, 0.0, 1.0) * clamp(sunElevZ + 0.12, 0.0, 1.0);
    vec3  scatterColor = mix(vec3(1.0, 0.55, 0.15), vec3(1.0, 0.85, 0.6), clamp(sunElevZ, 0.0, 1.0));
    color = mix(color, scatterColor,   sunScatter * sunVisible * 0.18);
    color = mix(color, sunDiskColor,   sunGlow    * sunVisible * 0.55);
    color = mix(color, vec3(1.0, 0.99, 0.95), sunCore * sunVisible * 0.90);

    vec3  moonPos      = -uMoonDir;
    float moonElevZ    = moonPos.z;
    float moonAngle    = acos(clamp(dot(dir, moonPos), -1.0, 1.0));
    float moonVisible  = clamp(uMoonGlow * 1.3, 0.0, 1.0) * clamp(moonElevZ + 0.08, 0.0, 1.0);
    float moonCore     = 1.0 - smoothstep(0.03, 0.06, moonAngle);
    float moonGlowRing = 1.0 - smoothstep(0.06, 0.18, moonAngle);
    color = mix(color, uMoonColor * 0.7, moonGlowRing * moonVisible * 0.35);
    color = mix(color, uMoonColor,       moonCore     * moonVisible * 0.88);

    fragColor = vec4(color, 1.0);
}
`;
*/







    // ?? Sky Vertex Shader ????????????????????????????????????????????????????
    const skyVsSrc = `#version 300 es
                in vec3 aPosition;
                in vec2 aTexCoord;
                uniform mat4 uMVP;
                out vec2 vTexCoord;
                out vec3 vLocalPos;
                void main() {
                    vTexCoord = aTexCoord;
                    vLocalPos = aPosition;
                    gl_Position = uMVP * vec4(aPosition, 1.0);
                }
            `;


    const skyFsSrc = `#version 300 es
precision mediump float;
in vec2 vTexCoord;
in vec3 vLocalPos;

uniform sampler2D uDayTex;
uniform sampler2D uNightTex;
uniform float uSkyBlend;
uniform vec3  uZenithColor;
uniform vec3  uHorizonColor;
uniform vec3  uSunDir;
uniform float uTimeOfDay;
uniform float uCloudOffset;
uniform float uStarOffset;
uniform float uCloudScale;
uniform float uStarScale;
uniform vec3  uMoonDir;
uniform vec3  uMoonColor;
uniform float uMoonGlow;

out vec4 fragColor;

void main() {
    vec3 dir = normalize(vLocalPos);

    float vertFactor     = clamp(dir.z * 0.5 + 0.5, 0.0, 1.0);
    float gradientFactor = pow(vertFactor, 0.6);
    vec3  gradientColor  = mix(uHorizonColor, uZenithColor, gradientFactor);

        float longitude = atan(dir.x, dir.y);
        float latitude  = asin(clamp(dir.z, -1.0, 1.0));
        float u = longitude / (2.0 * 3.14159265) + 0.5;
        float v = latitude  / 3.14159265 + 0.5;

// Base UV
vec2 baseUV = vTexCoord;

// 🌤 Clouds — faster, larger scale
vec2 cloudUV = vec2(
    fract(baseUV.x * uCloudScale + uCloudOffset),
    baseUV.y
);

// 🌙 Stars — slower, smaller scale
vec2 starUV = vec2(
    fract(baseUV.x * uStarScale + uStarOffset),
    baseUV.y
);

vec4 dayTex   = texture(uDayTex,   cloudUV);
vec4 nightTex = texture(uNightTex, starUV);


    float poleAngle = acos(abs(dir.z));
    float poleMask  = smoothstep(0.0, 0.25, poleAngle);
    vec3  poleColor = dir.z > 0.0 ? uZenithColor : uHorizonColor;
    float horizonFade = smoothstep(0.0, 0.18, abs(dir.z));

    float dayAlpha = max(dayTex.r, max(dayTex.g, dayTex.b));
    dayAlpha *= poleMask * horizonFade;

    float nightAlpha = max(nightTex.r, max(nightTex.g, nightTex.b));
    nightAlpha *= uSkyBlend * 2.5 * poleMask;
    nightAlpha  = clamp(nightAlpha, 0.0, 1.0);

    vec3 color = gradientColor;
    color = mix(color, dayTex.rgb,   dayAlpha);
    color = mix(color, nightTex.rgb, nightAlpha);
    color = mix(poleColor, color, poleMask);

    float horizonBand     = 1.0 - smoothstep(0.0, 0.22, abs(dir.z));
    float horizonStrength = horizonBand * 0.12 * (1.0 - uSkyBlend * 0.5);
    color = mix(color, uHorizonColor, horizonStrength);

vec3  topPoleColor = gradientColor;
float topApex = 1.0 - smoothstep(0.0, 1.2, acos(clamp(dir.z, -1.0, 1.0)));
color = mix(color, topPoleColor, topApex * 0.92);

    vec3  botPoleColor = mix(uHorizonColor, uZenithColor, 0.0);
    float botApex      = 1.0 - smoothstep(0.0, 0.65, acos(clamp(-dir.z, -1.0, 1.0)));
    color = mix(color, botPoleColor, botApex * 0.88);

    vec3  sunPos      = -uSunDir;
    float sunElevZ    = sunPos.z;
    float sunAngle    = acos(clamp(dot(dir, sunPos), -1.0, 1.0));
    vec3  sunDiskColor = mix(vec3(1.0, 0.40, 0.05), vec3(1.0, 0.98, 0.85), clamp(sunElevZ * 1.5, 0.0, 1.0));
    float sunCore     = 1.0 - smoothstep(0.04, 0.07, sunAngle);
    float sunGlow     = 1.0 - smoothstep(0.07, 0.22, sunAngle);
    float sunScatter  = (1.0 - smoothstep(0.15, 0.55, sunAngle)) * clamp(1.0 - sunElevZ * 2.5, 0.0, 1.0);
    float sunVisible  = clamp(1.0 - uSkyBlend * 2.5, 0.0, 1.0) * clamp(sunElevZ + 0.12, 0.0, 1.0);
    vec3  scatterColor = mix(vec3(1.0, 0.55, 0.15), vec3(1.0, 0.85, 0.6), clamp(sunElevZ, 0.0, 1.0));
    color = mix(color, scatterColor,   sunScatter * sunVisible * 0.18);
    color = mix(color, sunDiskColor,   sunGlow    * sunVisible * 0.55);
    color = mix(color, vec3(1.0, 0.99, 0.95), sunCore * sunVisible * 0.90);

    vec3  moonPos      = -uMoonDir;
    float moonElevZ    = moonPos.z;
    float moonAngle    = acos(clamp(dot(dir, moonPos), -1.0, 1.0));
    float moonVisible  = clamp(uMoonGlow * 1.3, 0.0, 1.0) * clamp(moonElevZ + 0.08, 0.0, 1.0);
    float moonCore     = 1.0 - smoothstep(0.03, 0.06, moonAngle);
    float moonGlowRing = 1.0 - smoothstep(0.06, 0.18, moonAngle);
    color = mix(color, uMoonColor * 0.7, moonGlowRing * moonVisible * 0.35);
    color = mix(color, uMoonColor,       moonCore     * moonVisible * 0.88);

    fragColor = vec4(color, 1.0);
}
`;

/*

          // ?? Sky Vertex Shader ????????????????????????????????????????????????????
        const skyVsSrc = `#version 300 es
            in vec3 aPosition;
            in vec2 aTexCoord;
            uniform mat4 uMVP;
            out vec2 vTexCoord;
            out vec3 vLocalPos;
            void main() {
                vTexCoord = aTexCoord;
                vLocalPos = aPosition;
                gl_Position = uMVP * vec4(aPosition, 1.0);
            }
        `;


    const skyFsSrc = `#version 300 es
    precision mediump float;
    in vec2 vTexCoord;
    in vec3 vLocalPos;

    uniform sampler2D uDayTex;
    uniform sampler2D uNightTex;
    uniform float uSkyBlend;
    uniform vec3  uZenithColor;
    uniform vec3  uHorizonColor;
    uniform vec3  uSunDir;
    uniform float uTimeOfDay;
    uniform float uCloudOffset;
    uniform float uStarOffset;
    uniform vec3  uMoonDir;
    uniform vec3  uMoonColor;
    uniform float uMoonGlow;

    out vec4 fragColor;

    void main() {
        // ?? Fragment direction — XYZ space: X=right, Y=forward, Z=up ????????????
        vec3 dir = normalize(vLocalPos);

        // ?? Sky gradient ?????????????????????????????????????????????????????????
        float vertFactor     = clamp(dir.z * 0.5 + 0.5, 0.0, 1.0);
        float gradientFactor = pow(vertFactor, 0.6);
        vec3  gradientColor  = mix(uHorizonColor, uZenithColor, gradientFactor);

        // ?? Spherical UV ?????????????????????????????????????????????????????????
        float longitude = atan(dir.x, dir.y);
        float latitude  = asin(clamp(dir.z, -1.0, 1.0));
        float u = longitude / (2.0 * 3.14159265) + 0.5;
        float v = latitude  / 3.14159265 + 0.5;

        // ?? Cloud UV — scrolls at cloud speed ????????????????????????????????????
        vec2 cloudUV = vec2(fract(u + uCloudOffset), v);

        // ?? Star UV — scrolls very slowly ????????????????????????????????????????
        vec2 starUV = vec2(fract(u + uStarOffset), v);

        // ?? Pole mask ?????????????????????????????????????????????????????????????
        float poleAngle = acos(abs(dir.z));
        float poleMask  = smoothstep(0.0, 0.25, poleAngle);
        vec3  poleColor = dir.z > 0.0 ? uZenithColor : uHorizonColor;

        // ?? Horizon fade — suppresses bottom pole seam ???????????????????????????
        float horizonFade = smoothstep(0.0, 0.18, abs(dir.z));

        // ?? Day texture (clouds) ?????????????????????????????????????????????????
        vec4  dayTex   = texture(uDayTex, cloudUV);
        float dayAlpha = max(dayTex.r, max(dayTex.g, dayTex.b));
        dayAlpha *= poleMask * horizonFade;

        // ?? Night texture (stars) ?????????????????????????????????????????????????
        vec4  nightTex   = texture(uNightTex, starUV);
        float nightAlpha = max(nightTex.r, max(nightTex.g, nightTex.b));
        nightAlpha *= uSkyBlend * 2.5 * poleMask;
        nightAlpha  = clamp(nightAlpha, 0.0, 1.0);

        // ?? Base composite ???????????????????????????????????????????????????????
        vec3 color = gradientColor;
        color = mix(color, dayTex.rgb,   dayAlpha);
        color = mix(color, nightTex.rgb, nightAlpha);
        color = mix(poleColor, color, poleMask);

        // ?? Horizon headband — subtle, locked to uHorizonColor ???????????????????
        // Wide gradual falloff so no visible band boundary
        float horizonBand     = 1.0 - smoothstep(0.0, 0.22, abs(dir.z));
        float horizonStrength = horizonBand * 0.12 * (1.0 - uSkyBlend * 0.5);
        color = mix(color, uHorizonColor, horizonStrength);

        // ?? Apex blurs — seamless vignette, NOT a halo ???????????????????????????
        // Key: very wide smoothstep so falloff is invisible as a distinct element
        // Strength kept low so it tints not brightens — blends into gradient

    // Top pole — matches exact gradient color at Z=1.0, high strength to cover seam
    vec3  topPoleColor = mix(uHorizonColor, uZenithColor, 1.0); // = uZenithColor exactly
    float topApex      = 1.0 - smoothstep(0.0, 0.65, acos(clamp(dir.z, -1.0, 1.0)));
    float topStrength  = topApex * 0.92;
    color = mix(color, topPoleColor, topStrength);

    // Bottom pole — matches exact gradient color at Z=-1.0, high strength to cover seam
    vec3  botPoleColor = mix(uHorizonColor, uZenithColor, 0.0); // = uHorizonColor exactly
    float botApex      = 1.0 - smoothstep(0.0, 0.65, acos(clamp(-dir.z, -1.0, 1.0)));
    float botStrength  = botApex * 0.88;
    color = mix(color, botPoleColor, botStrength);

        // ?? Sun disk — acos angular distance, tight focused glow ?????????????????
        // uSunDir points FROM sun TOWARD origin — so sun position = -uSunDir
        vec3  sunPos    = -uSunDir;
        float sunElevZ  = sunPos.z; // Z = up in XYZ

        // Angular distance from fragment to sun center
        float sunAngle  = acos(clamp(dot(dir, sunPos), -1.0, 1.0));

        // Sun disk color — deep orange at horizon, warm white at zenith
        vec3 sunDiskColor = mix(
            vec3(1.0, 0.40, 0.05),
            vec3(1.0, 0.98, 0.85),
            clamp(sunElevZ * 1.5, 0.0, 1.0));

        // Hard core disk — very tight
        // Tuning: inner 0.04 rad = ~2.3 degrees, adjust if too small/large
        float sunCore  = 1.0 - smoothstep(0.04, 0.07, sunAngle);

        // Soft inner glow ring — atmospheric corona just around disk
        // Tuning: 0.07 to 0.22 gives a visible but contained glow
        float sunGlow  = 1.0 - smoothstep(0.07, 0.22, sunAngle);

        // Wide atmospheric scatter — very faint, horizon only
        // Tuning: only bleeds wide when sun is low (sunElevZ near 0)
        float sunScatter = (1.0 - smoothstep(0.15, 0.55, sunAngle))
                         * clamp(1.0 - sunElevZ * 2.5, 0.0, 1.0);

        // Sun visibility — fades below horizon and fully off at night
        float sunVisible  = clamp(1.0 - uSkyBlend * 2.5, 0.0, 1.0);
        sunVisible       *= clamp(sunElevZ + 0.12, 0.0, 1.0);

        // Scatter color — warm orange tint for atmospheric haze
        vec3 scatterColor = mix(vec3(1.0, 0.55, 0.15), vec3(1.0, 0.85, 0.6),
                               clamp(sunElevZ, 0.0, 1.0));

        color = mix(color, scatterColor,  sunScatter * sunVisible * 0.18);
        color = mix(color, sunDiskColor,  sunGlow    * sunVisible * 0.55);
        color = mix(color, vec3(1.0, 0.99, 0.95), sunCore * sunVisible * 0.90);

        // ?? Moon disk — same acos approach, smaller, cooler ???????????????????????
        vec3  moonPos   = -uMoonDir;
        float moonElevZ = moonPos.z;

        float moonAngle = acos(clamp(dot(dir, moonPos), -1.0, 1.0));

        // Moon visibility — only at night, fades below horizon
        float moonVisible  = clamp(uMoonGlow * 1.3, 0.0, 1.0);
        moonVisible       *= clamp(moonElevZ + 0.08, 0.0, 1.0);

        // Moon hard disk — slightly smaller than sun
        // Tuning: 0.03 rad core, 0.06 edge
        float moonCore = 1.0 - smoothstep(0.03, 0.06, moonAngle);

        // Moon soft glow ring
        // Tuning: 0.06 to 0.18
        float moonGlowRing = 1.0 - smoothstep(0.06, 0.18, moonAngle);

        color = mix(color, uMoonColor * 0.7, moonGlowRing * moonVisible * 0.35);
        color = mix(color, uMoonColor,       moonCore     * moonVisible * 0.88);

        fragColor = vec4(color, 1.0);
    }
    `;



*/


    // ?? Tile Map Vertex Shader ????????????????????????????????????????????????
    const tileVsSrc = `#version 300 es
    precision mediump float;

    layout(location = 0) in vec2 aGridPos;
    layout(location = 1) in float aHeight;
    layout(location = 2) in vec3 aNormal;

    uniform mat4 uView;
    uniform mat4 uProjection;

    out vec3 vNormal;
    out vec3 vWorldPos;

    void main() {
        vec3 worldPos = vec3(aGridPos.x, aGridPos.y, aHeight);
        vWorldPos   = worldPos;
        vNormal     = aNormal;
        gl_Position = uProjection * uView * vec4(worldPos, 1.0);
    }
    `;

    // ?? Tile Map Fragment Shader ??????????????????????????????????????????????

    const tileFsSrc = `#version 300 es
precision mediump float;

in vec3 vNormal;
in vec3 vWorldPos;

uniform sampler2D uTex0;
uniform sampler2D uTex1;
uniform sampler2D uTex2;
uniform sampler2D uTex3;
uniform sampler2D uTex4;
uniform sampler2D uTex5;
uniform sampler2D uTileData;

uniform vec2  uGridOrigin;
uniform float uGridSize;
uniform float uTileSize;

uniform vec3  uSunDir;
uniform vec3  uSunColor;
uniform float uSunIntensity;
uniform vec3  uAmbient;
uniform vec2  uBrushPos;

const int TILE_MAX_LIGHTS = 32;
uniform int   uTileLightCount;
uniform vec3  uTileLightPos[TILE_MAX_LIGHTS];
uniform vec3  uTileLightColor[TILE_MAX_LIGHTS];
uniform float uTileLightIntensity[TILE_MAX_LIGHTS];
uniform float uTileLightRange[TILE_MAX_LIGHTS];
uniform int   uTileLightType[TILE_MAX_LIGHTS];
uniform vec3  uTileLightDir[TILE_MAX_LIGHTS];
uniform float uTileLightSpotAngle[TILE_MAX_LIGHTS];

uniform float uBrushRadius;
uniform float uBrushActive;
uniform sampler2D uShadowMap0;
uniform mat4      uLightVP0;
uniform float     uShadowBias;

out vec4 fragColor;

vec4 sampleMaterial(int idx, vec2 uv) {
    if (idx == 0) return texture(uTex0, uv);
    if (idx == 1) return texture(uTex1, uv);
    if (idx == 2) return texture(uTex2, uv);
    if (idx == 3) return texture(uTex3, uv);
    if (idx == 4) return texture(uTex4, uv);
    if (idx == 5) return texture(uTex5, uv);
    return texture(uTex2, uv);
}

vec4 triplanar(int idx, vec3 worldPos, vec3 norm) {
    vec2 uvXY = worldPos.xy * 0.5;
    vec2 uvXZ = worldPos.xz * 0.5;
    vec2 uvYZ = worldPos.yz * 0.5;

    vec4 colXY = sampleMaterial(idx, uvXY);
    vec4 colXZ = sampleMaterial(idx, uvXZ);
    vec4 colYZ = sampleMaterial(idx, uvYZ);

    vec3 blend = abs(norm);
    blend = max(blend - 0.2, 0.0);
    blend /= (blend.x + blend.y + blend.z);

    return colXY * blend.z + colXZ * blend.y + colYZ * blend.x;
}

float tileShadowFactor(vec3 worldPos, vec3 norm, vec3 lightDir) {
    vec4 shadowCoord = uLightVP0 * vec4(worldPos, 1.0);
    vec3 proj = shadowCoord.xyz / shadowCoord.w;
    proj = proj * 0.5 + 0.5;

    if (proj.x < 0.0 || proj.x > 1.0 ||
        proj.y < 0.0 || proj.y > 1.0 ||
        proj.z > 1.0) return 1.0;

    float currentDepth = proj.z;
    float cosTheta     = clamp(dot(norm, lightDir), 0.0, 1.0);
    float bias         = mix(0.005, 0.001, cosTheta) + uShadowBias;

    float shadow   = 0.0;
    vec2 texelSize = vec2(1.0 / 1024.0);
    for (int x = -1; x <= 1; x++) {
        for (int y = -1; y <= 1; y++) {
            float pcfDepth = texture(uShadowMap0,
                proj.xy + vec2(float(x), float(y)) * texelSize).r;
            shadow += currentDepth - bias > pcfDepth ? 0.0 : 1.0;
        }
    }
    return shadow / 9.0;
}

// Sample a tile's colour at a given tileUV position
vec4 sampleTileColor(vec2 uv, vec3 norm) {
    vec4 td = texture(uTileData, uv);
    int mat     = int(td.r + 0.5);
    int blendMat = int(td.b + 0.5);
    float blendW = td.g;

    float slopeAngle = 1.0 - abs(dot(norm, vec3(0.0, 0.0, 1.0)));
    float autoRock   = smoothstep(0.55, 0.75, slopeAngle);
    float triBlend   = smoothstep(0.35, 0.65, slopeAngle);
    vec2  flatUV     = vWorldPos.xy * 0.5;

    vec4 colA = mix(sampleMaterial(mat,      flatUV), triplanar(mat,      vWorldPos, norm), triBlend);
    vec4 colB = mix(sampleMaterial(blendMat, flatUV), triplanar(blendMat, vWorldPos, norm), triBlend);
    vec4 colR = triplanar(1, vWorldPos, norm);

    vec4 painted = mix(colA, colB, smoothstep(0.0, 1.0, blendW));
    return mix(painted, colR, autoRock);
}

void main() {
    // Where are we inside our tile — 0 to 1 on each axis
    vec2 localUV = (vWorldPos.xy - uGridOrigin) / uTileSize;
    vec2 tileFrac = fract(localUV);

    // Texel size in tile data texture
    vec2 texel = vec2(1.0) / vec2(uGridSize);

    // Current tile UV
    vec2 tileUV = clamp((vWorldPos.xy - uGridOrigin) / (uGridSize * uTileSize), 0.0, 1.0);

    // 4 neighbouring tile UVs
    vec2 uvC  = tileUV;
    vec2 uvR  = tileUV + vec2( texel.x, 0.0);
    vec2 uvU  = tileUV + vec2( 0.0,  texel.y);
    vec2 uvRU = tileUV + vec2( texel.x,  texel.y);

    // Sample colour of each neighbouring tile
    vec3 norm = normalize(vNormal);
    vec4 colC  = sampleTileColor(uvC,  norm);
    vec4 colR  = sampleTileColor(uvR,  norm);
    vec4 colU  = sampleTileColor(uvU,  norm);
    vec4 colRU = sampleTileColor(uvRU, norm);

    // Blend zone width — how far from tile edge the blend starts
    // Higher = wider blend, softer edges
    float blendZone = 0.35;
    float wx = smoothstep(0.5 - blendZone, 0.5 + blendZone, tileFrac.x);
    float wy = smoothstep(0.5 - blendZone, 0.5 + blendZone, tileFrac.y);

    // Bilinear blend across the 4 tiles
    vec4 albedo = mix(
        mix(colC, colR,  wx),
        mix(colU, colRU, wx),
        wy
    );

    // Lighting — unchanged from your original
    vec3 lightDir = normalize(-uSunDir);
    float diff    = max(dot(norm, lightDir), 0.0);
    float shadow  = tileShadowFactor(vWorldPos, norm, lightDir);
    vec3 lighting = uAmbient + uSunColor * uSunIntensity * diff * shadow;

    for (int i = 0; i < TILE_MAX_LIGHTS; i++) {
        if (i >= uTileLightCount) break;
        if (uTileLightType[i] == 1) continue;

        vec3 toLightDir;
        float attenuation;

        if (uTileLightType[i] == 2) {
            vec3 toLight   = uTileLightPos[i] - vWorldPos;
            float distance = length(toLight);
            toLightDir     = normalize(toLight);
            attenuation    = 1.0 / (1.0 + (distance * distance) /
                (uTileLightRange[i] * uTileLightRange[i]));
            attenuation    = attenuation * attenuation * attenuation;
            float cosAngle = cos(radians(uTileLightSpotAngle[i]));
            float cosOuter = cos(radians(uTileLightSpotAngle[i] * 1.3));
            float spotDot  = dot(-toLightDir, normalize(uTileLightDir[i]));
            attenuation   *= smoothstep(cosOuter, cosAngle, spotDot);
        } else if (uTileLightType[i] == 3) {
            vec3 upRef     = abs(uTileLightDir[i].z) < 0.9 ? vec3(0.0, 0.0, 1.0) : vec3(0.0, 1.0, 0.0);
            vec3 areaRight = normalize(cross(uTileLightDir[i], upRef));
            vec3 areaUp    = normalize(cross(areaRight, uTileLightDir[i]));
            float hw       = uTileLightSpotAngle[i] * 0.1;
            float hh       = uTileLightRange[i] * 0.05;
            vec3 c0 = uTileLightPos[i] + areaRight * hw + areaUp * hh;
            vec3 c1 = uTileLightPos[i] - areaRight * hw + areaUp * hh;
            vec3 c2 = uTileLightPos[i] + areaRight * hw - areaUp * hh;
            vec3 c3 = uTileLightPos[i] - areaRight * hw - areaUp * hh;
            toLightDir  = normalize(
                normalize(c0 - vWorldPos) + normalize(c1 - vWorldPos) +
                normalize(c2 - vWorldPos) + normalize(c3 - vWorldPos));
            float dist  = length(uTileLightPos[i] - vWorldPos);
            attenuation = 1.0 / (1.0 + (dist * dist) /
                (uTileLightRange[i] * uTileLightRange[i]));
            attenuation = attenuation * attenuation;
        } else {
            vec3 toLight   = uTileLightPos[i] - vWorldPos;
            float distance = length(toLight);
            toLightDir     = normalize(toLight);
            attenuation    = 1.0 / (1.0 + (distance * distance) /
                (uTileLightRange[i] * uTileLightRange[i]));
            attenuation    = attenuation * attenuation * attenuation;
        }

        float tileDiff = max(dot(norm, toLightDir), 0.0);
        lighting += uTileLightColor[i] * uTileLightIntensity[i] * tileDiff * attenuation;
    }

    lighting = clamp(lighting, 0.0, 2.0);

    float brushDist = length(vWorldPos.xy - uBrushPos);
    float ringInner = uBrushRadius - 0.15;
    float ringOuter = uBrushRadius + 0.15;
    float ring      = smoothstep(ringInner - 0.1, ringInner, brushDist) *
                      (1.0 - smoothstep(ringOuter, ringOuter + 0.1, brushDist));
    vec3 ringColor  = vec3(1.0, 1.0, 0.3);
    vec3 litAlbedo  = albedo.rgb * lighting;
    fragColor = vec4(mix(litAlbedo, ringColor, ring * uBrushActive * 0.85), albedo.a);
}
`;




    function initScrollbar() {
        const gl = _gl;
        const vsSrc = `#version 300 es
        in vec2 aPosition;
        void main() {
            gl_Position = vec4(aPosition, 0.0, 1.0);
        }`;

        const fsSrc = `#version 300 es
        precision mediump float;
        uniform vec4 uColor;
        uniform float uGlow;
        out vec4 fragColor;
        void main() {
            fragColor = vec4(uColor.rgb * (1.0 + uGlow), uColor.a);
        }`;

        _scrollbarProgram = buildProgram(vsSrc, fsSrc);
        _scrollbarLocs = {
            pos: gl.getAttribLocation(_scrollbarProgram, 'aPosition'),
            color: gl.getUniformLocation(_scrollbarProgram, 'uColor'),
            glow: gl.getUniformLocation(_scrollbarProgram, 'uGlow'),
        };

        _scrollbarVbo = gl.createBuffer();
        console.log('[SpectralX] Scrollbar initialized');
    }



    function initParticles() {
        const gl = _gl;
        _particleProgram = buildProgram(particleVsSrc, particleFsSrc);

        // Billboard quad — two triangles, centered at origin
        const quadVerts = new Float32Array([
            -0.5, -0.5, 0,
            0.5, -0.5, 0,
            -0.5, 0.5, 0,
            -0.5, 0.5, 0,
            0.5, -0.5, 0,
            0.5, 0.5, 0,
        ]);
        _particleQuadVbo = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, _particleQuadVbo);
        gl.bufferData(gl.ARRAY_BUFFER, quadVerts, gl.STATIC_DRAW);

        const quadUVs = new Float32Array([
            0, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 1
        ]);
        _particleQuadUbo = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, _particleQuadUbo);
        gl.bufferData(gl.ARRAY_BUFFER, quadUVs, gl.STATIC_DRAW);

        // Pre-allocate max-size instance buffers — reused every frame, no GPU reallocation
        const MAX_PARTICLES = 5000;
        _particleInstanceBuffers['__preallocated__'] = {
            offsetBuf: gl.createBuffer(),
            colorBuf: gl.createBuffer(),
            sizeBuf: gl.createBuffer(),
        };
        // Size the buffers once at max capacity
        gl.bindBuffer(gl.ARRAY_BUFFER, _particleInstanceBuffers['__preallocated__'].offsetBuf);
        gl.bufferData(gl.ARRAY_BUFFER, MAX_PARTICLES * 3 * 4, gl.DYNAMIC_DRAW);
        gl.bindBuffer(gl.ARRAY_BUFFER, _particleInstanceBuffers['__preallocated__'].colorBuf);
        gl.bufferData(gl.ARRAY_BUFFER, MAX_PARTICLES * 4 * 4, gl.DYNAMIC_DRAW);
        gl.bindBuffer(gl.ARRAY_BUFFER, _particleInstanceBuffers['__preallocated__'].sizeBuf);
        gl.bufferData(gl.ARRAY_BUFFER, MAX_PARTICLES * 1 * 4, gl.DYNAMIC_DRAW);


        // Cache all particle locations once — never call getUniformLocation in renderParticles again
        _pLocs = {
            vp: gl.getUniformLocation(_particleProgram, 'uVP'),
            camR: gl.getUniformLocation(_particleProgram, 'uCamRight'),
            camU: gl.getUniformLocation(_particleProgram, 'uCamUp'),
            tex: gl.getUniformLocation(_particleProgram, 'uTexture'),
            hasTex: gl.getUniformLocation(_particleProgram, 'uHasTexture'),
            pos: gl.getAttribLocation(_particleProgram, 'aPosition'),
            uv: gl.getAttribLocation(_particleProgram, 'aTexCoord'),
            off: gl.getAttribLocation(_particleProgram, 'iOffset'),
            col: gl.getAttribLocation(_particleProgram, 'iColor'),
            size: gl.getAttribLocation(_particleProgram, 'iSize'),
        };

        console.log('[SpectralGL] Particle instanced renderer initialized');
    }

    function initFoliage() {
        const gl = _gl;

        const foliageVsSrc = `#version 300 es
        in vec3 aPosition;
        in vec3 aNormal;
        in vec2 aTexCoord;
        in vec3 iInstancePos;
        in float iInstanceScale;
        in float iInstanceRot;
        uniform mat4 uVP;
        uniform mat4 uLightVP;
        out vec3 vNormal;
        out vec3 vWorldPos;
        out vec2 vTexCoord;
        out vec4 vShadowCoord;
        void main() {
            float c = cos(iInstanceRot);
            float s = sin(iInstanceRot);
            vec3 rotated = vec3(
                aPosition.x * c - aPosition.y * s,
                aPosition.x * s + aPosition.y * c,
                aPosition.z
            );
            vec3 worldPos = rotated * iInstanceScale + iInstancePos;
            vWorldPos   = worldPos;
            vNormal     = vec3(-s * aNormal.x + c * aNormal.y,
                                c * aNormal.x + s * aNormal.y,
                                aNormal.z);
            vTexCoord   = aTexCoord;
            vShadowCoord = uLightVP * vec4(worldPos, 1.0);
            gl_Position = uVP * vec4(worldPos, 1.0);
        }`;

        const foliageFsSrc = `#version 300 es
        precision mediump float;
        in vec3 vNormal;
        in vec3 vWorldPos;
        in vec2 vTexCoord;
        in vec4 vShadowCoord;
        uniform sampler2D uTexture;
        uniform bool uHasTexture;
        uniform vec3 uCamPos;
        uniform int uLightCount;
        uniform vec3 uLightPos[32];
        uniform vec3 uLightColor[32];
        uniform float uLightIntensity[32];
        uniform float uLightRange[32];
        uniform int uLightType[32];
        uniform sampler2D uShadowMap;
        uniform mat4 uLightVP;
        out vec4 fragColor;

        float shadowFactor() {
            vec3 proj = vShadowCoord.xyz / vShadowCoord.w;
            proj = proj * 0.5 + 0.5;
            if (proj.x < 0.0 || proj.x > 1.0 ||
                proj.y < 0.0 || proj.y > 1.0 ||
                proj.z > 1.0) return 1.0;
            float bias = 0.005;
            float depth = texture(uShadowMap, proj.xy).r;
            return proj.z - bias > depth ? 0.4 : 1.0;
        }

        void main() {
            vec4 base = uHasTexture
                ? texture(uTexture, vTexCoord)
                : vec4(0.6, 0.8, 0.4, 1.0);
            if (base.a < 0.1) discard;

            vec3 norm    = normalize(vNormal);
            vec3 ambient = vec3(0.3);
            vec3 light   = ambient;

            for (int i = 0; i < 32; i++) {
                if (i >= uLightCount) break;
                if (uLightType[i] == 1) {
                    // Directional
                    vec3 dir  = normalize(-uLightPos[i]);
                    float diff = max(dot(norm, dir), 0.0);
                    light += uLightColor[i] * uLightIntensity[i]
                             * diff * shadowFactor();
                } else {
                    // Point
                    vec3 toLight = uLightPos[i] - vWorldPos;
                    float dist   = length(toLight);
                    float att    = 1.0 / (1.0 + (dist * dist) /
                        (uLightRange[i] * uLightRange[i]));
                    att = att * att * att;
                    float diff   = max(dot(norm, normalize(toLight)), 0.0);
                    light += uLightColor[i] * uLightIntensity[i] * diff * att;
                }
            }

            light = clamp(light, 0.0, 2.0);
            fragColor = vec4(base.rgb * light, base.a);
        }`;

        _foliageProgram = buildProgram(foliageVsSrc, foliageFsSrc);

        const MAX_LIGHTS = 32;
        _fLocs = {
            vp: gl.getUniformLocation(_foliageProgram, 'uVP'),
            lightVP: gl.getUniformLocation(_foliageProgram, 'uLightVP'),
            camPos: gl.getUniformLocation(_foliageProgram, 'uCamPos'),
            lightCount: gl.getUniformLocation(_foliageProgram, 'uLightCount'),
            tex: gl.getUniformLocation(_foliageProgram, 'uTexture'),
            hasTex: gl.getUniformLocation(_foliageProgram, 'uHasTexture'),
            shadowMap: gl.getUniformLocation(_foliageProgram, 'uShadowMap'),
            pos: gl.getAttribLocation(_foliageProgram, 'aPosition'),
            norm: gl.getAttribLocation(_foliageProgram, 'aNormal'),
            uv: gl.getAttribLocation(_foliageProgram, 'aTexCoord'),
            iPos: gl.getAttribLocation(_foliageProgram, 'iInstancePos'),
            iScale: gl.getAttribLocation(_foliageProgram, 'iInstanceScale'),
            iRot: gl.getAttribLocation(_foliageProgram, 'iInstanceRot'),
            lightPos: Array.from({ length: MAX_LIGHTS }, (_, i) =>
                gl.getUniformLocation(_foliageProgram, `uLightPos[${i}]`)),
            lightColor: Array.from({ length: MAX_LIGHTS }, (_, i) =>
                gl.getUniformLocation(_foliageProgram, `uLightColor[${i}]`)),
            lightIntensity: Array.from({ length: MAX_LIGHTS }, (_, i) =>
                gl.getUniformLocation(_foliageProgram, `uLightIntensity[${i}]`)),
            lightRange: Array.from({ length: MAX_LIGHTS }, (_, i) =>
                gl.getUniformLocation(_foliageProgram, `uLightRange[${i}]`)),
            lightType: Array.from({ length: MAX_LIGHTS }, (_, i) =>
                gl.getUniformLocation(_foliageProgram, `uLightType[${i}]`)),
        };

        console.log('[SpectralGL] Foliage instanced renderer initialized');
    }



    function compileShader(type, src) {
        const s = _gl.createShader(type);
        _gl.shaderSource(s, src);
        _gl.compileShader(s);
        if (!_gl.getShaderParameter(s, _gl.COMPILE_STATUS)) {
            console.error("Shader error:", _gl.getShaderInfoLog(s));
            return null;
        }
        return s;
    }


    function initSMAA() {
        const gl = _gl;

        // Pass 1 — Edge Detection
        const edgeVsSrc = `
                 attribute vec2 aPosition;
                 varying vec2 vTexCoord;
                 varying vec2 vOffset[3];
                 uniform vec2 uResolution;
                 void main() {
                     vTexCoord = aPosition * 0.5 + 0.5;
                     vec2 px = 1.0 / uResolution;
                     vOffset[0] = vTexCoord + px * vec2(-1.0,  0.0);
                     vOffset[1] = vTexCoord + px * vec2( 0.0, -1.0);
                     vOffset[2] = vTexCoord + px * vec2( 1.0,  0.0);
                     gl_Position = vec4(aPosition, 0.0, 1.0);
                 }
             `;
        const edgeFsSrc = `
                 precision mediump float;
                 uniform sampler2D uColorTex;
                 uniform vec2 uResolution;
                 varying vec2 vTexCoord;
                 varying vec2 vOffset[3];
                 void main() {
                     vec3 luma = vec3(0.2126, 0.7152, 0.0722);
                     float L     = dot(texture2D(uColorTex, vTexCoord).rgb,  luma);
                     float Lleft = dot(texture2D(uColorTex, vOffset[0]).rgb, luma);
                     float Ltop  = dot(texture2D(uColorTex, vOffset[1]).rgb, luma);
                     vec2 delta = abs(vec2(L - Lleft, L - Ltop));
                     vec2 edges = step(0.1, delta);
                     if (dot(edges, vec2(1.0)) == 0.0) discard;
                     gl_FragColor = vec4(edges, 0.0, 1.0);
                 }
             `;

        // Pass 2 — Blending Weights
        const blendVsSrc = `
                 attribute vec2 aPosition;
                 varying vec2 vTexCoord;
                 uniform vec2 uResolution;
                 void main() {
                     vTexCoord = aPosition * 0.5 + 0.5;
                     gl_Position = vec4(aPosition, 0.0, 1.0);
                 }
             `;
        const blendFsSrc = `
                 precision mediump float;
                 uniform sampler2D uEdgeTex;
                 uniform vec2 uResolution;
                 varying vec2 vTexCoord;
                 void main() {
                     vec2 px = 1.0 / uResolution;
                     vec2 edges = texture2D(uEdgeTex, vTexCoord).rg;
                     vec4 weights = vec4(0.0);
                     if (edges.g > 0.0) {
                         // Horizontal edge — search left/right
                         float left  = 0.0;
                         float right = 0.0;
                         for (int i = 1; i <= 8; i++) {
                             if (texture2D(uEdgeTex, vTexCoord + vec2(-float(i), 0.0) * px).g > 0.0) left  += 1.0; else break;
                         }
                         for (int i = 1; i <= 8; i++) {
                             if (texture2D(uEdgeTex, vTexCoord + vec2( float(i), 0.0) * px).g > 0.0) right += 1.0; else break;
                         }
                         weights.r = 0.5 / (left + right + 1.0);
                     }
                     if (edges.r > 0.0) {
                         // Vertical edge — search up/down
                         float top    = 0.0;
                         float bottom = 0.0;
                         for (int i = 1; i <= 8; i++) {
                             if (texture2D(uEdgeTex, vTexCoord + vec2(0.0, -float(i)) * px).r > 0.0) top    += 1.0; else break;
                         }
                         for (int i = 1; i <= 8; i++) {
                             if (texture2D(uEdgeTex, vTexCoord + vec2(0.0,  float(i)) * px).r > 0.0) bottom += 1.0; else break;
                         }
                         weights.g = 0.5 / (top + bottom + 1.0);
                     }
                     gl_FragColor = weights;
                 }
             `;

        // Pass 3 — Neighbourhood Blend
        const nBlendVsSrc = `
                 attribute vec2 aPosition;
                 varying vec2 vTexCoord;
                 void main() {
                     vTexCoord = aPosition * 0.5 + 0.5;
                     gl_Position = vec4(aPosition, 0.0, 1.0);
                 }
             `;
        const nBlendFsSrc = `
                 precision mediump float;
                 uniform sampler2D uColorTex;
                 uniform sampler2D uBlendTex;
                 uniform vec2 uResolution;
                 varying vec2 vTexCoord;
                 void main() {
                     vec2 px = 1.0 / uResolution;
                     vec4 blendW = texture2D(uBlendTex, vTexCoord);
                     vec4 color  = texture2D(uColorTex, vTexCoord);
                     if (blendW.r > 0.0)
                         color = mix(color, texture2D(uColorTex, vTexCoord + vec2( px.x, 0.0)), blendW.r);
                     if (blendW.g > 0.0)
                         color = mix(color, texture2D(uColorTex, vTexCoord + vec2(0.0,  px.y)), blendW.g);
                     gl_FragColor = color;
                 }
             `;

        // Compile all 3 programs
        _smaaProgram1 = buildProgram(edgeVsSrc, edgeFsSrc);
        _smaaProgram2 = buildProgram(blendVsSrc, blendFsSrc);
        _smaaProgram3 = buildProgram(nBlendVsSrc, nBlendFsSrc);

        // Edge texture + FBO
        _smaaEdgeTex = createColorTexture(_canvas.width, _canvas.height);
        _smaaEdgeFbo = createFboForTexture(_smaaEdgeTex, _canvas.width, _canvas.height);

        // Blend weight texture + FBO
        _smaaBlendTex = createColorTexture(_canvas.width, _canvas.height);
        _smaaBlendFbo = createFboForTexture(_smaaBlendTex, _canvas.width, _canvas.height);

        console.log("[SpectralGL] SMAA initialized", _canvas.width, _canvas.height);
    }

    function initSharedFbo() {
        const gl = _gl;

        _fxaaColorTex = gl.createTexture();
        gl.bindTexture(gl.TEXTURE_2D, _fxaaColorTex);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA,
            _canvas.width, _canvas.height, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);

        const depthRb = gl.createRenderbuffer();
        gl.bindRenderbuffer(gl.RENDERBUFFER, depthRb);
        gl.renderbufferStorage(gl.RENDERBUFFER, gl.DEPTH_COMPONENT16,
            _canvas.width, _canvas.height);

        _fxaaFbo = gl.createFramebuffer();
        gl.bindFramebuffer(gl.FRAMEBUFFER, _fxaaFbo);
        gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0,
            gl.TEXTURE_2D, _fxaaColorTex, 0);
        gl.framebufferRenderbuffer(gl.FRAMEBUFFER, gl.DEPTH_ATTACHMENT,
            gl.RENDERBUFFER, depthRb);
        gl.bindFramebuffer(gl.FRAMEBUFFER, null);

        _fxaaTexWidth = _canvas.width;
        _fxaaTexHeight = _canvas.height;

        console.log("[SpectralGL] Shared FBO initialized", _canvas.width, _canvas.height);
    }



    function initFXAA() {
        const gl = _gl;

        const fxaaVsSrc = `
                 attribute vec2 aPosition;
                 varying vec2 vTexCoord;
                 void main() {
                     vTexCoord = aPosition * 0.5 + 0.5;
                     gl_Position = vec4(aPosition, 0.0, 1.0);
                 }
             `;

        const fxaaFsSrc = `
                 precision mediump float;
                 uniform sampler2D uTexture;
                 uniform vec2 uResolution;
                 varying vec2 vTexCoord;

                 void main() {
                     vec2 texel = 1.0 / uResolution;

                     vec3 rgbNW = texture2D(uTexture, vTexCoord + vec2(-1.0, -1.0) * texel).rgb;
                     vec3 rgbNE = texture2D(uTexture, vTexCoord + vec2( 1.0, -1.0) * texel).rgb;
                     vec3 rgbSW = texture2D(uTexture, vTexCoord + vec2(-1.0,  1.0) * texel).rgb;
                     vec3 rgbSE = texture2D(uTexture, vTexCoord + vec2( 1.0,  1.0) * texel).rgb;
                     vec3 rgbM  = texture2D(uTexture, vTexCoord).rgb;

                     vec3 luma = vec3(0.299, 0.587, 0.114);
                     float lumaNW = dot(rgbNW, luma);
                     float lumaNE = dot(rgbNE, luma);
                     float lumaSW = dot(rgbSW, luma);
                     float lumaSE = dot(rgbSE, luma);
                     float lumaM  = dot(rgbM,  luma);

                     float lumaMin = min(lumaM, min(min(lumaNW, lumaNE), min(lumaSW, lumaSE)));
                     float lumaMax = max(lumaM, max(max(lumaNW, lumaNE), max(lumaSW, lumaSE)));
                     float lumaRange = lumaMax - lumaMin;

                     // Skip non-edges
                     if (lumaRange < max(0.0312, lumaMax * 0.125)) {
                         gl_FragColor = vec4(rgbM, 1.0);
                         return;
                     }

                     vec2 dir = vec2(
                         -((lumaNW + lumaNE) - (lumaSW + lumaSE)),
                          ((lumaNW + lumaSW) - (lumaNE + lumaSE))
                     );

                     float dirReduce = max((lumaNW + lumaNE + lumaSW + lumaSE) * 0.03125, 0.0078125);
                     float rcpDirMin = 1.0 / (min(abs(dir.x), abs(dir.y)) + dirReduce);
                     dir = clamp(dir * rcpDirMin, vec2(-8.0), vec2(8.0)) * texel;

                     vec3 rgbA = 0.5 * (
                         texture2D(uTexture, vTexCoord + dir * (1.0/3.0 - 0.5)).rgb +
                         texture2D(uTexture, vTexCoord + dir * (2.0/3.0 - 0.5)).rgb
                     );
                     vec3 rgbB = rgbA * 0.5 + 0.25 * (
                         texture2D(uTexture, vTexCoord + dir * -0.5).rgb +
                         texture2D(uTexture, vTexCoord + dir *  0.5).rgb
                     );

                     float lumaB = dot(rgbB, luma);
                     if (lumaB < lumaMin || lumaB > lumaMax)
                         gl_FragColor = vec4(rgbA, 1.0);
                     else
                         gl_FragColor = vec4(rgbB, 1.0);
                 }
             `;

        const fv = compileShader(gl.VERTEX_SHADER, fxaaVsSrc);
        const ff = compileShader(gl.FRAGMENT_SHADER, fxaaFsSrc);
        _fxaaProgram = gl.createProgram();
        gl.attachShader(_fxaaProgram, fv);
        gl.attachShader(_fxaaProgram, ff);
        gl.linkProgram(_fxaaProgram);

        _fxaaPosLoc = gl.getAttribLocation(_fxaaProgram, "aPosition");
        _fxaaTexLoc = gl.getUniformLocation(_fxaaProgram, "uTexture");
        _fxaaResLoc = gl.getUniformLocation(_fxaaProgram, "uResolution");

        // Fullscreen quad
        // REPLACE with:
        _fxaaQuadVbo = _fullscreenQuadVbo; // alias — quad already created in init()



        console.log("[SpectralGL] FXAA initialized", _canvas.width, _canvas.height);
        console.log("[SpectralGL] FXAA initialized");
    }

    function initTAA() {
        const gl = _gl;

        const taaVsSrc = `
                 attribute vec2 aPosition;
                 varying vec2 vTexCoord;
                 void main() {
                     vTexCoord = aPosition * 0.5 + 0.5;
                     gl_Position = vec4(aPosition, 0.0, 1.0);
                 }
             `;

        const taaFsSrc = `
                 precision mediump float;
                 uniform sampler2D uCurrentTex;
                 uniform sampler2D uHistoryTex;
                 uniform vec2 uResolution;
                 uniform float uBlend;
                 varying vec2 vTexCoord;

                 vec3 clipToAABB(vec3 color, vec3 minimum, vec3 maximum) {
                     vec3 center = 0.5 * (maximum + minimum);
                     vec3 extents = 0.5 * (maximum - minimum);
                     vec3 offset = color - center;
                     vec3 ts = abs(extents / (offset + 0.0001));
                     float t = clamp(min(min(ts.x, ts.y), ts.z), 0.0, 1.0);
                     return center + offset * t;
                 }

                 void main() {
                     vec2 px = 1.0 / uResolution;

                     vec3 current = texture2D(uCurrentTex, vTexCoord).rgb;

                     // Sample 3x3 neighbourhood for variance clipping
                     vec3 minC = current;
                     vec3 maxC = current;
                     for (int x = -1; x <= 1; x++) {
                         for (int y = -1; y <= 1; y++) {
                             vec3 s = texture2D(uCurrentTex,
                                 vTexCoord + vec2(float(x), float(y)) * px).rgb;
                             minC = min(minC, s);
                             maxC = max(maxC, s);
                         }
                     }

                     vec3 history = texture2D(uHistoryTex, vTexCoord).rgb;

                     // Clip history to neighbourhood AABB to reduce ghosting
                     history = clipToAABB(history, minC, maxC);

                     gl_FragColor = vec4(mix(current, history, uBlend), 1.0);
                 }
             `;

        _taaProgram = buildProgram(taaVsSrc, taaFsSrc);

        _taaCurrentTex = createColorTexture(_canvas.width, _canvas.height);
        _taaCurrentFbo = createFboForTexture(_taaCurrentTex, _canvas.width, _canvas.height);

        _taaHistoryTex = createColorTexture(_canvas.width, _canvas.height);
        _taaHistoryFbo = createFboForTexture(_taaHistoryTex, _canvas.width, _canvas.height);

        _taaTexWidth = _canvas.width;
        _taaTexHeight = _canvas.height;

        console.log("[SpectralGL] TAA initialized", _canvas.width, _canvas.height);
    }


    function initSpectralAA() {
        const gl = _gl;

        // Pass 1 — Edge detection with angle encoding
        const edgeVsSrc = `
                 attribute vec2 aPosition;
                 varying vec2 vTexCoord;
                 void main() {
                     vTexCoord = aPosition * 0.5 + 0.5;
                     gl_Position = vec4(aPosition, 0.0, 1.0);
                 }
             `;

        const edgeFsSrc = `
                 precision mediump float;
                 uniform sampler2D uColorTex;
                 uniform vec2 uResolution;
                 varying vec2 vTexCoord;

                 void main() {
                     vec2 px = 1.0 / uResolution;
                     vec3 luma = vec3(0.2126, 0.7152, 0.0722);

                     float c  = dot(texture2D(uColorTex, vTexCoord).rgb, luma);
                     float cR = dot(texture2D(uColorTex, vTexCoord + vec2( px.x, 0.0)).rgb, luma);
                     float cL = dot(texture2D(uColorTex, vTexCoord + vec2(-px.x, 0.0)).rgb, luma);
                     float cU = dot(texture2D(uColorTex, vTexCoord + vec2(0.0,  px.y)).rgb, luma);
                     float cD = dot(texture2D(uColorTex, vTexCoord + vec2(0.0, -px.y)).rgb, luma);

                     float dX = cR - cL;
                     float dY = cU - cD;
                     float edgeStrength = sqrt(dX * dX + dY * dY);

                     if (edgeStrength < 0.08) {
                         gl_FragColor = vec4(0.0);
                         return;
                     }

                     // Encode edge angle into RG, strength into B
                     // Normalize gradient direction to 0..1 range for storage
                     float angleX = dX * 0.5 + 0.5;
                     float angleY = dY * 0.5 + 0.5;
                     gl_FragColor = vec4(angleX, angleY, edgeStrength, 1.0);
                 }
             `;

        // Pass 2 — Geometric subpixel composite
        const compositeVsSrc = `
                 attribute vec2 aPosition;
                 varying vec2 vTexCoord;
                 void main() {
                     vTexCoord = aPosition * 0.5 + 0.5;
                     gl_Position = vec4(aPosition, 0.0, 1.0);
                 }
             `;

        const compositeFsSrc = `
                 precision mediump float;
                 uniform sampler2D uColorTex;
                 uniform sampler2D uEdgeTex;
                 uniform vec2 uResolution;
                 varying vec2 vTexCoord;

                 void main() {
                     vec2 px = 1.0 / uResolution;
                     vec4 edgeData = texture2D(uEdgeTex, vTexCoord);
                     float edgeStrength = edgeData.b;

                     // Not an edge — pass through unchanged
                     if (edgeStrength < 0.08) {
                         gl_FragColor = texture2D(uColorTex, vTexCoord);
                         return;
                     }

                     // Recover gradient direction from storage
                     float dX = edgeData.r * 2.0 - 1.0;
                     float dY = edgeData.g * 2.0 - 1.0;

                     // Normalize to get perpendicular edge direction
                     float len = sqrt(dX * dX + dY * dY) + 0.0001;
                     vec2 gradDir = vec2(dX, dY) / len;

                     // Sample the two colours on either side of the edge
                     vec4 colorA = texture2D(uColorTex, vTexCoord + gradDir * px);
                     vec4 colorB = texture2D(uColorTex, vTexCoord - gradDir * px);
                     vec4 colorC = texture2D(uColorTex, vTexCoord);

                     // Calculate sub-pixel coverage using the gradient angle
                     // This is the micro-triangle fill — what fraction of this pixel
                     // belongs to each side of the edge
                     float angle = atan(abs(dY), abs(dX));
                     float coverage = angle / 1.5708; // normalize to 0..1 (pi/2)

                     // Geometric composite — blend the two sides at the coverage ratio
                     // This is sharper than FXAA because we use the actual edge angle
                     // rather than a luma gradient blur
                     vec4 edgeComposite = mix(colorA, colorB, coverage);

                     // Scale blend by edge strength — weak edges get less correction
                     float blendFactor = clamp(edgeStrength * 3.0, 0.0, 0.8);
                     gl_FragColor = mix(colorC, edgeComposite, blendFactor);
                 }
             `;

        _spectralProgram1 = buildProgram(edgeVsSrc, edgeFsSrc);
        _spectralProgram2 = buildProgram(compositeVsSrc, compositeFsSrc);

        _spectralEdgeTex = createColorTexture(_canvas.width, _canvas.height);
        _spectralEdgeFbo = createFboForTexture(_spectralEdgeTex, _canvas.width, _canvas.height);

        console.log("[SpectralGL] SpectralAA initialized", _canvas.width, _canvas.height);
    }

    function initSpectralAAV2() {
        const gl = _gl;

        // Pass 1 — Staircase topology detection
        const detectVsSrc = `
                 attribute vec2 aPosition;
                 varying vec2 vTexCoord;
                 void main() {
                     vTexCoord = aPosition * 0.5 + 0.5;
                     gl_Position = vec4(aPosition, 0.0, 1.0);
                 }
             `;

        const detectFsSrc = `
                 precision mediump float;
                 uniform sampler2D uColorTex;
                 uniform vec2 uResolution;
                 varying vec2 vTexCoord;

                 float luma(vec3 c) {
                     return dot(c, vec3(0.2126, 0.7152, 0.0722));
                 }

                 void main() {
                     vec2 px = 1.0 / uResolution;

                     float C  = luma(texture2D(uColorTex, vTexCoord).rgb);
                     float R  = luma(texture2D(uColorTex, vTexCoord + vec2( px.x,  0.0)).rgb);
                     float L  = luma(texture2D(uColorTex, vTexCoord + vec2(-px.x,  0.0)).rgb);
                     float U  = luma(texture2D(uColorTex, vTexCoord + vec2( 0.0,   px.y)).rgb);
                     float D  = luma(texture2D(uColorTex, vTexCoord + vec2( 0.0,  -px.y)).rgb);
                     float DR = luma(texture2D(uColorTex, vTexCoord + vec2( px.x, -px.y)).rgb);
                     float DL = luma(texture2D(uColorTex, vTexCoord + vec2(-px.x, -px.y)).rgb);
                     float UR = luma(texture2D(uColorTex, vTexCoord + vec2( px.x,  px.y)).rgb);
                     float UL = luma(texture2D(uColorTex, vTexCoord + vec2(-px.x,  px.y)).rgb);

                     float threshold = 0.08;

                     // Check all 4 staircase corner patterns:
                     // Pattern: differs from orthogonal neighbours, matches diagonal

                     // Bottom-right staircase corner
                     float dR  = abs(C - R);
                     float dD  = abs(C - D);
                     float dDR = abs(C - DR);
                     if (dR > threshold && dD > threshold && dDR < threshold * 0.5) {
                         // Coverage = ratio of horizontal vs vertical contrast
                         float coverage = dD / (dR + dD + 0.0001);
                         // Encode: corner type 1, gradient direction, coverage
                         gl_FragColor = vec4(1.0, coverage, dR, dD);
                         return;
                     }

                     // Bottom-left staircase corner
                     float dL  = abs(C - L);
                     float dDL = abs(C - DL);
                     if (dL > threshold && dD > threshold && dDL < threshold * 0.5) {
                         float coverage = dD / (dL + dD + 0.0001);
                         gl_FragColor = vec4(2.0 / 4.0, coverage, dL, dD);
                         return;
                     }

                     // Top-right staircase corner
                     float dU  = abs(C - U);
                     float dUR = abs(C - UR);
                     if (dR > threshold && dU > threshold && dUR < threshold * 0.5) {
                         float coverage = dU / (dR + dU + 0.0001);
                         gl_FragColor = vec4(3.0 / 4.0, coverage, dR, dU);
                         return;
                     }

                     // Top-left staircase corner
                     float dUL = abs(C - UL);
                     if (dL > threshold && dU > threshold && dUL < threshold * 0.5) {
                         float coverage = dU / (dL + dU + 0.0001);
                         // REPLACE:
         gl_FragColor = vec4(0.25, coverage, dL, dU);
                         return;
                     }

                     // Not a staircase pixel
                     gl_FragColor = vec4(0.0);
                 }
             `;

        // Pass 2 — Triangle fill composite
        const fillVsSrc = `
                 attribute vec2 aPosition;
                 varying vec2 vTexCoord;
                 void main() {
                     vTexCoord = aPosition * 0.5 + 0.5;
                     gl_Position = vec4(aPosition, 0.0, 1.0);
                 }
             `;

        const fillFsSrc = `
                 precision mediump float;
                 uniform sampler2D uColorTex;
                 uniform sampler2D uStaircaseTex;
                 uniform vec2 uResolution;
                 varying vec2 vTexCoord;

                 void main() {
                     vec2 px = 1.0 / uResolution;
                     vec4 staircase = texture2D(uStaircaseTex, vTexCoord);
                     vec4 colorC = texture2D(uColorTex, vTexCoord);

                     // No staircase detected — pass through completely untouched
                     if (staircase.a < 0.01) {
                         gl_FragColor = colorC;
                         return;
                     }

                     float cornerType = staircase.r;
                     float coverage   = staircase.g;

                     // Clamp coverage to reasonable triangle fill range
                     coverage = clamp(coverage, 0.1, 0.9);

                     // Fetch the fill colour from the diagonal neighbour
                     // (the colour that should fill the triangular gap)
                     vec4 fillColor;

                   // FIND and REPLACE the whole cornerType block:
         if (cornerType > 0.9) {
             // Bottom-right corner — fill from bottom-right diagonal
             fillColor = texture2D(uColorTex, vTexCoord + vec2( px.x, -px.y));
         } else if (cornerType > 0.68) {
             // Top-right corner — fill from top-right diagonal
             fillColor = texture2D(uColorTex, vTexCoord + vec2( px.x,  px.y));
         } else if (cornerType > 0.37) {
             // Bottom-left corner — fill from bottom-left diagonal
             fillColor = texture2D(uColorTex, vTexCoord + vec2(-px.x, -px.y));
         } else {
             // Top-left corner — fill from top-left diagonal
             fillColor = texture2D(uColorTex, vTexCoord + vec2(-px.x,  px.y));
         }
                     // Triangle fill — replace exactly the coverage fraction
                     // with the diagonal colour. No blur — pure geometric replacement.
                     gl_FragColor = mix(colorC, fillColor, coverage * 0.5);
                 }
             `;

        _spectralV2Program1 = buildProgram(detectVsSrc, detectFsSrc);
        _spectralV2Program2 = buildProgram(fillVsSrc, fillFsSrc);

        _spectralV2EdgeTex = createColorTexture(_canvas.width, _canvas.height);
        _spectralV2EdgeFbo = createFboForTexture(_spectralV2EdgeTex, _canvas.width, _canvas.height);

        console.log("[SpectralGL] SpectralAA V2 initialized", _canvas.width, _canvas.height);
    }

    function initSpectralAAV3() {
        const gl = _gl;

        // Pass 1 — Binary edge classification
        const classifyVsSrc = `
                 attribute vec2 aPosition;
                 varying vec2 vTexCoord;
                 void main() {
                     vTexCoord = aPosition * 0.5 + 0.5;
                     gl_Position = vec4(aPosition, 0.0, 1.0);
                 }
             `;

        const classifyFsSrc = `
                 precision mediump float;
                 uniform sampler2D uColorTex;
                 uniform vec2 uResolution;
                 varying vec2 vTexCoord;

                 float luma(vec3 c) {
                     return dot(c, vec3(0.2126, 0.7152, 0.0722));
                 }

                 void main() {
                     vec2 px = 1.0 / uResolution;

                     float C  = luma(texture2D(uColorTex, vTexCoord).rgb);

                     // Sample all 8 neighbours
                     float R  = luma(texture2D(uColorTex, vTexCoord + vec2( px.x,  0.0)).rgb);
                     float L  = luma(texture2D(uColorTex, vTexCoord + vec2(-px.x,  0.0)).rgb);
                     float U  = luma(texture2D(uColorTex, vTexCoord + vec2( 0.0,   px.y)).rgb);
                     float D  = luma(texture2D(uColorTex, vTexCoord + vec2( 0.0,  -px.y)).rgb);
                     float UR = luma(texture2D(uColorTex, vTexCoord + vec2( px.x,  px.y)).rgb);
                     float UL = luma(texture2D(uColorTex, vTexCoord + vec2(-px.x,  px.y)).rgb);
                     float DR = luma(texture2D(uColorTex, vTexCoord + vec2( px.x, -px.y)).rgb);
                     float DL = luma(texture2D(uColorTex, vTexCoord + vec2(-px.x, -px.y)).rgb);

                     // Hard threshold — binary inside/outside classification
                     // Higher threshold than V1/V2 to exclude shadow gradients
                     float threshold = 0.15;

                     float maxDelta = max(
                         max(abs(C-R), abs(C-L)),
                         max(abs(C-U), abs(C-D))
                     );

                     // Not an edge pixel at all — discard early
                     if (maxDelta < threshold) {
                         gl_FragColor = vec4(0.0);
                         return;
                     }

                     // Shadow gradient gate — shadows have gradual falloff
                     // True geometry edges have at least one very sharp neighbour transition
                     float minDelta = min(
                         min(abs(C-R), abs(C-L)),
                         min(abs(C-U), abs(C-D))
                     );

                     // If min and max delta are close together its a gradient not an edge
                     if (minDelta > threshold * 0.4) {
                         gl_FragColor = vec4(0.0);
                         return;
                     }

                     // Classify each neighbour as same side (1) or other side (0)
                     float inside = step(C - threshold * 0.5, 0.0);

                     // Encode neighbour binary pattern into RG channels
                     // R = horizontal/vertical pattern
                     // G = diagonal pattern
                     // B = edge strength
                     // A = inside/outside flag
                     float hv = (step(threshold, abs(C-R)) +
                                step(threshold, abs(C-L)) +
                                step(threshold, abs(C-U)) +
                                step(threshold, abs(C-D))) / 4.0;

                     float diag = (step(threshold, abs(C-UR)) +
                                  step(threshold, abs(C-UL)) +
                                  step(threshold, abs(C-DR)) +
                                  step(threshold, abs(C-DL))) / 4.0;

                     gl_FragColor = vec4(hv, diag, maxDelta, inside);
                 }
             `;

        // Pass 2 — Line reconstruction from binary pattern
        const lineVsSrc = `
                 attribute vec2 aPosition;
                 varying vec2 vTexCoord;
                 void main() {
                     vTexCoord = aPosition * 0.5 + 0.5;
                     gl_Position = vec4(aPosition, 0.0, 1.0);
                 }
             `;

        const lineFsSrc = `
                 precision mediump float;
                 uniform sampler2D uClassifyTex;
                 uniform vec2 uResolution;
                 varying vec2 vTexCoord;

                 void main() {
                     vec2 px = 1.0 / uResolution;
                     vec4 data = texture2D(uClassifyTex, vTexCoord);

                     // Not classified as edge — pass through
                     if (data.b < 0.15) {
                         gl_FragColor = vec4(0.0);
                         return;
                     }

                     float hv   = data.r; // how many orthogonal neighbours differ
                     float diag = data.g; // how many diagonal neighbours differ

                     // Reconstruct line angle from neighbour pattern
                     // Pure horizontal edge — line is horizontal
                     // Pure vertical edge — line is vertical
                     // Mixed — diagonal line at angle determined by hv/diag ratio

                     // Sample neighbours to get gradient direction
                     float R  = texture2D(uClassifyTex, vTexCoord + vec2( px.x,  0.0)).b;
                     float L  = texture2D(uClassifyTex, vTexCoord + vec2(-px.x,  0.0)).b;
                     float U  = texture2D(uClassifyTex, vTexCoord + vec2( 0.0,   px.y)).b;
                     float D  = texture2D(uClassifyTex, vTexCoord + vec2( 0.0,  -px.y)).b;

                     // Line direction vector from local edge topology
                     float dX = R - L;
                     float dY = U - D;
                     float len = sqrt(dX*dX + dY*dY) + 0.0001;

                     // Normalize
                     vec2 lineDir = vec2(dX, dY) / len;

                     // Coverage — how far across this pixel the line sits
                     // Derived from the ratio of orthogonal vs diagonal edge hits
                     float coverage = clamp(hv / (hv + diag + 0.0001), 0.1, 0.9);

                     // Encode: line direction in RG, coverage in B, valid in A
                     gl_FragColor = vec4(lineDir * 0.5 + 0.5, coverage, 1.0);
                 }
             `;

        // Pass 3 — Triangle coverage fill
        const fillVsSrc = `
                 attribute vec2 aPosition;
                 varying vec2 vTexCoord;
                 void main() {
                     vTexCoord = aPosition * 0.5 + 0.5;
                     gl_Position = vec4(aPosition, 0.0, 1.0);
                 }
             `;

        const fillFsSrc = `
                 precision mediump float;
                 uniform sampler2D uColorTex;
                 uniform sampler2D uLineTex;
                 uniform vec2 uResolution;
                 varying vec2 vTexCoord;

                 void main() {
                     vec2 px = 1.0 / uResolution;
                     vec4 lineData = texture2D(uLineTex, vTexCoord);

                     // No line data — pass through completely unchanged
                     if (lineData.a < 0.5) {
                         gl_FragColor = texture2D(uColorTex, vTexCoord);
                         return;
                     }

                     // Recover line direction
                     vec2 lineDir = lineData.rg * 2.0 - 1.0;
                     float coverage = lineData.b;

                     // The perpendicular to the line direction gives us
                     // which side to sample for fill color
                     vec2 perpDir = vec2(-lineDir.y, lineDir.x);

                     // Sample the two colors on either side of the true geometric line
                     vec4 colorInside  = texture2D(uColorTex, vTexCoord + perpDir * px);
                     vec4 colorOutside = texture2D(uColorTex, vTexCoord - perpDir * px);
                     vec4 colorCenter  = texture2D(uColorTex, vTexCoord);

                     // True triangle coverage fill
                     // coverage = fraction of pixel area on the inside of the line
                     // This is the key difference from V1/V2 — we use area not blend weight
                     // coverage 0.5 = line passes through center = equal triangles
                     // coverage 0.1 = line near edge = small triangle on inside
                     // coverage 0.9 = line near other edge = large triangle fill

                     // Reconstruct pixel as weighted area of inside + outside colors
                     vec4 triangleFill = mix(colorOutside, colorInside, coverage);

                     // Hard gate — only apply where we have strong confident edge data
                     float edgeConfidence = clamp(lineData.b * 2.0, 0.0, 1.0);

                     gl_FragColor = mix(colorCenter, triangleFill, edgeConfidence * 0.85);
                 }
             `;

        _spectralV3Program1 = buildProgram(classifyVsSrc, classifyFsSrc);
        _spectralV3Program2 = buildProgram(lineVsSrc, lineFsSrc);
        _spectralV3Program3 = buildProgram(fillVsSrc, fillFsSrc);

        _spectralV3EdgeTex = createColorTexture(_canvas.width, _canvas.height);
        _spectralV3EdgeFbo = createFboForTexture(_spectralV3EdgeTex, _canvas.width, _canvas.height);

        _spectralV3LineTex = createColorTexture(_canvas.width, _canvas.height);
        _spectralV3LineFbo = createFboForTexture(_spectralV3LineTex, _canvas.width, _canvas.height);

        console.log("[SpectralGL] SpectralAA V3 initialized", _canvas.width, _canvas.height);
    }


    function initTextShader() {
        const gl = _gl;
        _textProgram = buildProgram(textVsSrc, textFsSrc);
        _textLocs = {
            mvp: gl.getUniformLocation(_textProgram, 'uMVP'),
            atlas: gl.getUniformLocation(_textProgram, 'uAtlas'),
            color: gl.getUniformLocation(_textProgram, 'uColor'),
            outlineColor: gl.getUniformLocation(_textProgram, 'uOutlineColor'),
            outlineWidth: gl.getUniformLocation(_textProgram, 'uOutlineWidth'),
            softness: gl.getUniformLocation(_textProgram, 'uSoftness'),
            glowRadius: gl.getUniformLocation(_textProgram, 'uGlowRadius'),
            glowStrength: gl.getUniformLocation(_textProgram, 'uGlowStrength'),
            pos: gl.getAttribLocation(_textProgram, 'aPosition'),
            uv: gl.getAttribLocation(_textProgram, 'aTexCoord'),
        };
      //  console.log('[TextSystem] SDF shader initialized');
    }




    let _skyLocs = null;

    function initSkyProgram() {
        _skyProgram = buildProgram(skyVsSrc, skyFsSrc);
        _skyLocs = {
            mvp: _gl.getUniformLocation(_skyProgram, 'uMVP'),
            skyBlend: _gl.getUniformLocation(_skyProgram, 'uSkyBlend'),
            zenith: _gl.getUniformLocation(_skyProgram, 'uZenithColor'),
            horizon: _gl.getUniformLocation(_skyProgram, 'uHorizonColor'),
            sunDir: _gl.getUniformLocation(_skyProgram, 'uSunDir'),
            timeOfDay: _gl.getUniformLocation(_skyProgram, 'uTimeOfDay'),
            cloudOffset: _gl.getUniformLocation(_skyProgram, 'uCloudOffset'),
            starOffset: _gl.getUniformLocation(_skyProgram, 'uStarOffset'),
            moonDir: _gl.getUniformLocation(_skyProgram, 'uMoonDir'),
            moonColor: _gl.getUniformLocation(_skyProgram, 'uMoonColor'),
            moonGlow: _gl.getUniformLocation(_skyProgram, 'uMoonGlow'),
            dayTex: _gl.getUniformLocation(_skyProgram, 'uDayTex'),
            nightTex: _gl.getUniformLocation(_skyProgram, 'uNightTex'),
            pos: _gl.getAttribLocation(_skyProgram, 'aPosition'),
            uv: _gl.getAttribLocation(_skyProgram, 'aTexCoord'),
            cloudScale: _gl.getUniformLocation(_skyProgram, 'uCloudScale'),
            starScale: _gl.getUniformLocation(_skyProgram, 'uStarScale'),
        };
        console.log('[SpectralGL] Sky shader initialized');
    }



    function initTileMap(gl) {
        _tileProgram = buildProgram(tileVsSrc, tileFsSrc);
        if (!_tileProgram) { console.error('[TileMap] shader failed'); return; }

        const GS = GRID_SIZE;
        const GSP1 = GS + 1;

        // Static XY grid positions — built synchronously, small enough
        const xyData = new Float32Array(GSP1 * GSP1 * 2);
        for (let y = 0; y <= GS; y++) {
            for (let x = 0; x <= GS; x++) {
                const i = (y * GSP1 + x) * 2;
                xyData[i] = GRID_ORIGIN_X + x * TILE_SIZE;
                xyData[i + 1] = GRID_ORIGIN_Y + y * TILE_SIZE;
            }
        }
        _tileGridVBO = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, _tileGridVBO);
        gl.bufferData(gl.ARRAY_BUFFER, xyData, gl.STATIC_DRAW);

        // Dynamic VBOs and texture — allocate now, no data yet
        _tileHeightVBO = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, _tileHeightVBO);
        gl.bufferData(gl.ARRAY_BUFFER, GRID_VERTS * 4, gl.DYNAMIC_DRAW);

        _tileNormalVBO = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, _tileNormalVBO);
        gl.bufferData(gl.ARRAY_BUFFER, GRID_VERTS * 3 * 4, gl.DYNAMIC_DRAW);

        _tileMatTex = gl.createTexture();
        gl.bindTexture(gl.TEXTURE_2D, _tileMatTex);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA32F,
            GS, GS, 0, gl.RGBA, gl.FLOAT, null);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);

        // Cache uniforms immediately — safe to do before IBO ready
        _tileUniforms = {
            view: gl.getUniformLocation(_tileProgram, 'uView'),
            projection: gl.getUniformLocation(_tileProgram, 'uProjection'),
            tex0: gl.getUniformLocation(_tileProgram, 'uTex0'),
            tex1: gl.getUniformLocation(_tileProgram, 'uTex1'),
            tex2: gl.getUniformLocation(_tileProgram, 'uTex2'),
            tex3: gl.getUniformLocation(_tileProgram, 'uTex3'),
            tex4: gl.getUniformLocation(_tileProgram, 'uTex4'),
            tex5: gl.getUniformLocation(_tileProgram, 'uTex5'),
            tileData: gl.getUniformLocation(_tileProgram, 'uTileData'),
            gridOrigin: gl.getUniformLocation(_tileProgram, 'uGridOrigin'),
            gridSize: gl.getUniformLocation(_tileProgram, 'uGridSize'),
            tileSize: gl.getUniformLocation(_tileProgram, 'uTileSize'),
            sunDir: gl.getUniformLocation(_tileProgram, 'uSunDir'),
            sunColor: gl.getUniformLocation(_tileProgram, 'uSunColor'),
            sunIntensity: gl.getUniformLocation(_tileProgram, 'uSunIntensity'),
            ambient: gl.getUniformLocation(_tileProgram, 'uAmbient'),
            brushPos: gl.getUniformLocation(_tileProgram, 'uBrushPos'),
            brushRadius: gl.getUniformLocation(_tileProgram, 'uBrushRadius'),
            brushActive: gl.getUniformLocation(_tileProgram, 'uBrushActive'),
            shadowMap0: gl.getUniformLocation(_tileProgram, 'uShadowMap0'),
            lightVP0: gl.getUniformLocation(_tileProgram, 'uLightVP0'),
            shadowBias: gl.getUniformLocation(_tileProgram, 'uShadowBias'),
            tileLightCount: gl.getUniformLocation(_tileProgram, 'uTileLightCount'),
            tileLightPos: Array.from({ length: 32 }, (_, i) => gl.getUniformLocation(_tileProgram, `uTileLightPos[${i}]`)),
            tileLightColor: Array.from({ length: 32 }, (_, i) => gl.getUniformLocation(_tileProgram, `uTileLightColor[${i}]`)),
            tileLightIntensity: Array.from({ length: 32 }, (_, i) => gl.getUniformLocation(_tileProgram, `uTileLightIntensity[${i}]`)),
            tileLightRange: Array.from({ length: 32 }, (_, i) => gl.getUniformLocation(_tileProgram, `uTileLightRange[${i}]`)),
            tileLightType: Array.from({ length: 32 }, (_, i) => gl.getUniformLocation(_tileProgram, `uTileLightType[${i}]`)),
            tileLightDir: Array.from({ length: 32 }, (_, i) => gl.getUniformLocation(_tileProgram, `uTileLightDir[${i}]`)),
            tileLightSpotAngle: Array.from({ length: 32 }, (_, i) => gl.getUniformLocation(_tileProgram, `uTileLightSpotAngle[${i}]`)),
        };

        // Chunked async index buffer build — yields to browser every CHUNK rows
        // Keeps main thread responsive during heavy index generation
        const totalIndices = GS * GS * 6;
        const indices = new Uint32Array(totalIndices);
        const CHUNK = 512; // rows per chunk — tune if needed
        let row = 0;
        let ii = 0;

        function buildChunk() {
            const endRow = Math.min(row + CHUNK, GS);
            for (let y = row; y < endRow; y++) {
                for (let x = 0; x < GS; x++) {
                    const bl = y * GSP1 + x;
                    const br = bl + 1;
                    const tl = bl + GSP1;
                    const tr = tl + 1;
                    if ((x + y) % 2 === 0) {
                        indices[ii++] = bl; indices[ii++] = br; indices[ii++] = tl;
                        indices[ii++] = br; indices[ii++] = tr; indices[ii++] = tl;
                    } else {
                        indices[ii++] = bl; indices[ii++] = br; indices[ii++] = tr;
                        indices[ii++] = bl; indices[ii++] = tr; indices[ii++] = tl;
                    }
                }
            }
            row = endRow;

            if (row < GS) {
                // Yield to browser then continue next chunk
                setTimeout(buildChunk, 0);
            } else {
                // All chunks done — upload to GPU and build VAO
                _tileIBO = gl.createBuffer();
                gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, _tileIBO);
                gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, indices, gl.STATIC_DRAW);
                _tileIdxCount = indices.length;

                // VAO — safe to build now that IBO exists
                _tileGridVAO = gl.createVertexArray();
                gl.bindVertexArray(_tileGridVAO);
                gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, _tileIBO);

                gl.bindBuffer(gl.ARRAY_BUFFER, _tileGridVBO);
                gl.enableVertexAttribArray(0);
                gl.vertexAttribPointer(0, 2, gl.FLOAT, false, 0, 0);

                gl.bindBuffer(gl.ARRAY_BUFFER, _tileHeightVBO);
                gl.enableVertexAttribArray(1);
                gl.vertexAttribPointer(1, 1, gl.FLOAT, false, 0, 0);

                gl.bindBuffer(gl.ARRAY_BUFFER, _tileNormalVBO);
                gl.enableVertexAttribArray(2);
                gl.vertexAttribPointer(2, 3, gl.FLOAT, false, 0, 0);

                gl.bindVertexArray(null);

                console.log('[TileMap] Grid mesh ready — verts:', GRID_VERTS, 'indices:', _tileIdxCount);
                SpectralGLLoader.onSpecialComplete('tilemap');
            }
        }

        // Kick off first chunk
        setTimeout(buildChunk, 0);
    }

    function initShadowMaps(count) {
        const gl = _gl;

        // Shadow shader program — compile once
        const sv = compileShader(gl.VERTEX_SHADER, shadowVsSource);
        const sf = compileShader(gl.FRAGMENT_SHADER, shadowFsSource);
        _shadowProgram = gl.createProgram();
        gl.attachShader(_shadowProgram, sv);
        gl.attachShader(_shadowProgram, sf);
        gl.linkProgram(_shadowProgram);
        _shadowLightMVPLoc = gl.getUniformLocation(_shadowProgram, "uLightVP");
        _shadowModelLoc = gl.getUniformLocation(_shadowProgram, "uModel");
        _shadowPosLoc = gl.getAttribLocation(_shadowProgram, "aPosition");
        _shadowInstancedLoc = gl.getUniformLocation(_shadowProgram, "uIsInstanced");
        _shadowInstPosLoc = gl.getAttribLocation(_shadowProgram, "aInstancePos");
        // after existing location caching lines:
        _shadowTexCoordLoc = gl.getAttribLocation(_shadowProgram, "aTexCoord");
        _shadowHasTextureLoc = gl.getUniformLocation(_shadowProgram, "uShadowHasTexture");
        _shadowTextureLoc = gl.getUniformLocation(_shadowProgram, "uShadowTexture");
        _shadowAlphaThresholdLoc = gl.getUniformLocation(_shadowProgram, "uShadowAlphaThreshold");
        // Create one FBO + depth texture per light slot
        _shadowFbos = [];
        _shadowDepthTexs = [];

        for (let i = 0; i < count; i++) {
            const depthTex = gl.createTexture();
            gl.bindTexture(gl.TEXTURE_2D, depthTex);
            gl.texImage2D(gl.TEXTURE_2D, 0, gl.DEPTH_COMPONENT24,
                SHADOW_SIZE, SHADOW_SIZE, 0, gl.DEPTH_COMPONENT, gl.UNSIGNED_INT, null);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
            _shadowDepthTexs.push(depthTex);

            // WebGL2 doesn't require color attachment but we add one for safety
            const colorTex = gl.createTexture();
            gl.bindTexture(gl.TEXTURE_2D, colorTex);
            gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA,
                SHADOW_SIZE, SHADOW_SIZE, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);

            const fbo = gl.createFramebuffer();
            gl.bindFramebuffer(gl.FRAMEBUFFER, fbo);
            gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.DEPTH_ATTACHMENT,
                gl.TEXTURE_2D, depthTex, 0);
            gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0,
                gl.TEXTURE_2D, colorTex, 0);
            _shadowFbos.push(fbo);
        }

        gl.bindFramebuffer(gl.FRAMEBUFFER, null);
        console.log("[SpectralGL] Shadow maps initialized:", count);
    }




    function init(canvasRef, dotnetRef) {
        _canvas = canvasRef instanceof HTMLCanvasElement ? canvasRef : null;
        if (!_canvas) { console.error("SpectralGL: no canvas"); return; }

        console.log("[SpectralGL] Canvas size at init:", _canvas.width, _canvas.height);
        console.log('[INIT] called — meshBuffers keys before clear:',
            Object.keys(_meshBuffers).length,
            Object.keys(_meshBuffers).slice(0, 3));
        // ?? Reset caches on every init ????????????????????????????????????????????
        _textureReady = {};
        _textureCache = {};
        _meshBuffers = {};
        for (const [meshName, cached] of Object.entries(_parsedMeshCache)) {
            _pendingUploads.push({ meshName, data: cached.data, textures: cached.textures, materialColors: cached.materialColors });
        }

        // Clear and reset test error is this the right place after clear cache i get function.reset error
        window.SpectralTextSystem.reset();
        console.log('[INIT] caches cleared');
        // Temp add to relload AA on Page Redirect but glitches in debug.
        _initialized = false;
        // ?? WebGL2 context — fall back to WebGL1 if unavailable ??????????????????
        _gl = _canvas.getContext("webgl2", { antialias: true });
        if (!_gl) {
            console.warn("SpectralGL: WebGL2 not supported, falling back to WebGL1");
            _gl = _canvas.getContext("webgl", { antialias: true });
        }
        if (!_gl) { console.error("SpectralGL: WebGL not supported"); return; }

        _dotnetRef = dotnetRef;

        // ?? Compile one main program per shadow mode (PCF / PCSS / SpectralXSV1) ?
        const fsSources = [
            window._SpectralShaders.fsSourcePCF,
            window._SpectralShaders.fsSourcePCSS,
            window._SpectralShaders.fsSourceSpectralXSV1,
            window._SpectralShaders.fsSourceSpectralXSV2,
            window._SpectralShaders.fsSourceSpectralXSV3
        ];
        const vs = compileShader(_gl.VERTEX_SHADER, vsSource);
        for (let i = 0; i < 5; i++) {
            const fs = compileShader(_gl.FRAGMENT_SHADER, fsSources[i]);
            _programs[i] = _gl.createProgram();
            _gl.attachShader(_programs[i], vs);
            _gl.attachShader(_programs[i], fs);
            _gl.linkProgram(_programs[i]);
            if (!_gl.getProgramParameter(_programs[i], _gl.LINK_STATUS)) {
                console.error("Program error [" + i + "]:", _gl.getProgramInfoLog(_programs[i]));
            }
        }

        // ?? Cache all uniform + attrib locations once per program ?????????????????
        _programLocations = [];
        for (let pi = 0; pi < 5; pi++) {
            const p = _programs[pi];
            const locs = {
                pos: _gl.getAttribLocation(p, "aPosition"),
                norm: _gl.getAttribLocation(p, "aNormal"),
                texCoord: _gl.getAttribLocation(p, "aTexCoord"),
                mvp: _gl.getUniformLocation(p, "uMVP"),
                color: _gl.getUniformLocation(p, "uColor"),
                tex: _gl.getUniformLocation(p, "uTexture"),
                hasTex: _gl.getUniformLocation(p, "uHasTexture"),
                model: _gl.getUniformLocation(p, "uModel"),
                camPos: _gl.getUniformLocation(p, "uCamPos"),
                lightCount: _gl.getUniformLocation(p, "uLightCount"),
                emissive: _gl.getUniformLocation(p, "uIsEmissive"),
                emissiveIntensity: _gl.getUniformLocation(p, "uEmissiveIntensity"),
                jitter: _gl.getUniformLocation(p, "uJitter"),
                // SpectralXS tuneable shadow uniforms — only active in program 2
                shadowSoftnessBias: _gl.getUniformLocation(p, "uShadowSoftnessBias"),
                shadowBlockerSearchRadius: _gl.getUniformLocation(p, "uShadowBlockerSearchRadius"),
                shadowKernelSize: _gl.getUniformLocation(p, "uShadowKernelSize"),
                shadowContactSharpness: _gl.getUniformLocation(p, "uShadowContactSharpness"),
                shadowDepthBias: _gl.getUniformLocation(p, "uShadowDepthBias"),
                shadowTintR: _gl.getUniformLocation(p, "uShadowTintR"),
                shadowTintG: _gl.getUniformLocation(p, "uShadowTintG"),
                shadowTintB: _gl.getUniformLocation(p, "uShadowTintB"),
                shadowTintStrength: _gl.getUniformLocation(p, "uShadowTintStrength"),
                shadowPenumbraTintStrength: _gl.getUniformLocation(p, "uShadowPenumbraTintStrength"),
                uvOffset: _gl.getUniformLocation(p, "uUVOffset"),
                uvScale: _gl.getUniformLocation(p, "uUVScale"),
                lightPos: [],
                lightColor: [],
                lightDir: [],
                lightIntensity: [],
                lightRange: [],
                lightType: [],
                lightSpotAngle: [],
                shadowMap: [],
                lightVP: [],
            };
            for (let i = 0; i < MAX_LIGHTS; i++) {
                locs.lightPos[i] = _gl.getUniformLocation(p, `uLightPos[${i}]`);
                locs.lightColor[i] = _gl.getUniformLocation(p, `uLightColor[${i}]`);
                locs.lightDir[i] = _gl.getUniformLocation(p, `uLightDir[${i}]`);
                locs.lightIntensity[i] = _gl.getUniformLocation(p, `uLightIntensity[${i}]`);
                locs.lightRange[i] = _gl.getUniformLocation(p, `uLightRange[${i}]`);
                locs.lightType[i] = _gl.getUniformLocation(p, `uLightType[${i}]`);
                locs.lightSpotAngle[i] = _gl.getUniformLocation(p, `uLightSpotAngle[${i}]`);
                locs.shadowMap[i] = _gl.getUniformLocation(p, `uShadowMap${i}`);
                locs.lightVP[i] = _gl.getUniformLocation(p, `uLightVP${i}`);
            }
            _programLocations.push(locs);
        }

        // ?? Fullscreen quad VBO — shared by all AA + shadow post passes ???????????
        _fullscreenQuadVbo = _gl.createBuffer();
        _gl.bindBuffer(_gl.ARRAY_BUFFER, _fullscreenQuadVbo);
        _gl.bufferData(_gl.ARRAY_BUFFER, new Float32Array([
            -1, -1, 1, -1, -1, 1,
            -1, 1, 1, -1, 1, 1
        ]), _gl.STATIC_DRAW);

        // ?? Init shared FBO + all AA systems (sized at init time) ????????????????
        initSharedFbo();
        initFXAA();
        initSMAA();
        initTAA();
        initSpectralAA();
        initSpectralAAV2();
        initSpectralAAV3();
        initParticles();
        initFoliage();
        initSkyProgram();
        initTextShader();
        initScrollbar();
   
        // ?? Init shadow depth map FBOs — one slot per light ???????????????????????
        // Set shadow map size to GPU max, capped at 4096
        const gpuMax = _gl.getParameter(_gl.MAX_TEXTURE_SIZE);
        SHADOW_SIZE = Math.min(gpuMax, SHADOW_SIZE_MAX);
        console.log('[SpectralGL] Shadow map size set to:', SHADOW_SIZE, '(GPU max:', gpuMax + ')');
        initShadowMaps(MAX_SHADOW_LIGHTS);
        initTileMap(_gl);
        console.log("[SpectralGL] TileMap initialized at", _canvas.width, _canvas.height);

        console.log("[SpectralGL] initSharedFbo result — _fxaaFbo:", !!_fxaaFbo, "_fxaaColorTex:", !!_fxaaColorTex);
        console.log("[SpectralGL] WebGL ready");

        //   window._glReady = true;  // ADD THIS LINE
    }

    function uploadMesh(upload) {
        const gl = _gl;

        // If JS-direct upload already registered this mesh with real geometry,
        // don't overwrite it with the empty C# placeholder
        // Check both the base name and clone name for existing JS-uploaded geometry
        const baseName = upload.meshId.replace('_Clone', '');
        const existing = _meshBuffers[upload.meshId] || _meshBuffers[baseName];
        if (existing && existing.vertCount > 0) {
            // Still handle texture upload if needed
            if (upload.hasTexture && upload.textureDataUrl &&
                !_textureReady[upload.meshId]) {
                _textureReady[upload.meshId] = true;
                SpectralGLLoader.onAssetRequested();
                const tex = gl.createTexture();
                gl.bindTexture(gl.TEXTURE_2D, tex);
                gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 1, 1, 0, gl.RGBA,
                    gl.UNSIGNED_BYTE, new Uint8Array([255, 0, 255, 255]));
                const img = new Image();
                img.onload = () => {
                    gl.bindTexture(gl.TEXTURE_2D, tex);
                    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, img);
                    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
                    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
                    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
                    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
                    _textureCache[upload.meshId] = tex;
                    SpectralGLLoader.onAssetComplete();
                };
                img.src = upload.textureDataUrl;
            }
            return; // geometry already uploaded — skip the rest
        }


        const vbo = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, vbo);
        gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(upload.vertices), gl.STATIC_DRAW);

        const nbo = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, nbo);
        gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(upload.normals), gl.STATIC_DRAW);

        const ubo = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, ubo);
        gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(upload.uvs || []), gl.STATIC_DRAW);

        // Main texture upload
        if (upload.hasTexture && upload.textureDataUrl && !_textureReady[upload.meshId]) {
            _textureReady[upload.meshId] = true;
            SpectralGLLoader.onAssetRequested();

            const tex = gl.createTexture();
            gl.bindTexture(gl.TEXTURE_2D, tex);
            gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 1, 1, 0, gl.RGBA,
                gl.UNSIGNED_BYTE, new Uint8Array([255, 0, 255, 255]));

            if (upload.textureIsRawRGBA) {
                const pixels = Uint8Array.from(atob(upload.textureDataUrl), c => c.charCodeAt(0));
                gl.bindTexture(gl.TEXTURE_2D, tex);
                gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA,
                    upload.textureWidth, upload.textureHeight,
                    0, gl.RGBA, gl.UNSIGNED_BYTE, pixels);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
                SpectralGLLoader.onAssetComplete();
            } else {
                const img = new Image();
                img.onload = () => {
                    gl.bindTexture(gl.TEXTURE_2D, tex);
                    gl.pixelStorei(gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL, false);
                    gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
                    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, img);
                    const isPOT = (img.width & (img.width - 1)) === 0 && (img.height & (img.height - 1)) === 0;
                    if (isPOT) {
                        gl.generateMipmap(gl.TEXTURE_2D);
                        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR_MIPMAP_LINEAR);
                    } else {
                        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
                        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
                        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
                    }
                    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
                    SpectralGLLoader.onAssetComplete();
                };
                img.onerror = (e) => console.error('[SpectralGL] Texture FAILED:', upload.meshId, e);
                img.src = upload.textureDataUrl;
            }

            _textureCache[upload.meshId] = tex;
        }

        const matTexCache = new Array(upload.materialTextures ? upload.materialTextures.length : 0).fill(null);
        const matTexLoaded = new Array(matTexCache.length).fill(false);

        // Pool slot alias — no geometry, just point to shared buffer
        if ((!upload.vertices || upload.vertices.length === 0)
            && upload.meshId.startsWith('ParticlePool_')) {
            const sharedKey = upload.textureDataUrl;
            if (sharedKey && _meshBuffers[sharedKey]) {
                _meshBuffers[upload.meshId] = {
                    ..._meshBuffers[sharedKey],
                    texCacheKey: sharedKey
                };
            } else {
                console.warn('[SpectralGL] Alias not ready, will retry:', upload.meshId);
                delete _uploadedMeshBuffers;
            }
            return;
        }

        _meshBuffers[upload.meshId] = {
            vbo, nbo, ubo,
            vertCount: upload.vertices.length / 3,
            hasTexture: upload.hasTexture,
            texCacheKey: upload.meshId,
            materialTextures: matTexCache,
            matTexLoaded: matTexLoaded,
            matBreaks: upload.matBreaks || [],
            matIndices: upload.matIndices || [],
            materialColors: upload.materialColors || []
        };

        // ADD RIGHT AFTER:
        const uploadBaseName = upload.meshId.replace('_Clone', '');
        if (uploadBaseName !== upload.meshId) {
            _meshBuffers[uploadBaseName] = _meshBuffers[upload.meshId];
        }
        // Material textures
        if (upload.materialTextures && upload.materialTextures.length > 0) {
            for (let i = 0; i < upload.materialTextures.length; i++) {
                const dataUrl = upload.materialTextures[i];
                if (!dataUrl || dataUrl === '') continue;
                const capturedIdx = i;
                const capturedMeshId = upload.meshId;
                const tex = gl.createTexture();
                matTexCache[i] = tex;
                _meshBuffers[capturedMeshId].materialTextures[i] = tex;
                gl.bindTexture(gl.TEXTURE_2D, tex);
                gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 1, 1, 0, gl.RGBA,
                    gl.UNSIGNED_BYTE, new Uint8Array([255, 0, 255, 255]));
                SpectralGLLoader.onAssetRequested();
                const img = new Image();
                img.onload = () => {
                    gl.bindTexture(gl.TEXTURE_2D, tex);
                    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, img);
                    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
                    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
                    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
                    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
                    _meshBuffers[capturedMeshId].matTexLoaded[capturedIdx] = true;
                    SpectralGLLoader.onAssetComplete();
                };
                img.src = dataUrl;
            }
        }

        console.log('[SpectralGL] Mesh uploaded:', upload.meshId, 'verts:', upload.vertices.length / 3);
    }

    function uploadParsedMesh(meshName, data, textures, materialColors) {
        const gl = _gl;
        if (!gl) {
            _pendingUploads.push({ meshName, data, textures, materialColors });
            return;
        }
        _doUploadParsedMesh(meshName, data, textures, materialColors);
    }
    let _parsedMeshCache = {};

    function _doUploadParsedMesh(meshName, data, textures, materialColors) {
        _parsedMeshCache[meshName] = { data, textures: textures || [], materialColors: materialColors || [] };
        console.log('[doUpload] meshName:', meshName,
            '_gl ready:', !!_gl,
            'existing vertCount:', _meshBuffers[meshName]?.vertCount ?? 'none');
        const gl = _gl;
        if (_meshBuffers[meshName] &&
            _meshBuffers[meshName].vertCount > 0 &&
            meshName.startsWith('Text_')) {
            return;
        }
        const vbo = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, vbo);
        gl.bufferData(gl.ARRAY_BUFFER, data.vertices, gl.STATIC_DRAW);

        const nbo = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, nbo);
        gl.bufferData(gl.ARRAY_BUFFER, data.normals, gl.STATIC_DRAW);

        const ubo = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, ubo);
        gl.bufferData(gl.ARRAY_BUFFER, data.uvs, gl.STATIC_DRAW);

        const matTextures = [];
        const matTexLoaded = [];

        if (textures && textures.length > 0) {
            for (let i = 0; i < textures.length; i++) {
                const dataUrl = textures[i];
                matTextures.push(null);
                matTexLoaded.push(false);
                if (!dataUrl) continue;

                const capturedIdx = i;
                SpectralGLLoader.onAssetRequested();
                const tex = gl.createTexture();
                gl.bindTexture(gl.TEXTURE_2D, tex);
                gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 1, 1, 0,
                    gl.RGBA, gl.UNSIGNED_BYTE, new Uint8Array([255, 0, 255, 255]));

                const img = new Image();
                img.onload = () => {
                    gl.bindTexture(gl.TEXTURE_2D, tex);
                    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, img);
                    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
                    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
                    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
                    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
                    matTextures[capturedIdx] = tex;
                    matTexLoaded[capturedIdx] = true;
                    console.log('[SpectralGL] Texture loaded for:', meshName, 'slot:', capturedIdx);
                    SpectralGLLoader.onAssetComplete();
                };
                img.onerror = () => {
                    console.warn('[SpectralGL] Texture failed for:', meshName, 'slot:', capturedIdx);
                    SpectralGLLoader.onAssetComplete();
                };
                img.src = dataUrl;
                matTextures[capturedIdx] = tex;
            }
        }

        const matBreaks = data.matBreaks || [];
        const matIndices = data.matIndices || [];

        const bufEntry = {
            vbo, nbo, ubo,
            vertCount: data.vertices.length / 3,
            hasTexture: matTextures.length > 0,
            texCacheKey: meshName,
            materialTextures: matTextures,
            matTexLoaded: matTexLoaded,
            matBreaks: matBreaks,
            matIndices: matIndices,
            materialColors: materialColors || []  // NEW
        };

        _meshBuffers[meshName] = bufEntry;
        _meshBuffers[meshName + '_Clone'] = bufEntry;



        console.log('[SpectralGL] JS-direct upload:', meshName,
            'verts:', data.vertices.length / 3,
            'texSlots:', matTextures.length,
            'colorSlots:', (materialColors || []).length);  // ADD THIS
    }


    let _initialized = false;


    async function renderFrame() {
        if (!_gl || !_dotnetRef) return;



        try {

            // Flush any meshes that were parsed before GL was ready
            if (_pendingUploads.length > 0) {
                console.log('[RenderFrame] draining pendingUploads:', _pendingUploads.length);
                for (const p of _pendingUploads)
                    _doUploadParsedMesh(p.meshName, p.data, p.textures || [], p.materialColors || []);
                _pendingUploads = [];
            }

            const frame = await _dotnetRef.invokeMethodAsync("TickAndGetFrame");
            if (!frame || !frame.meshes) return;

            const gl = _gl;

            // ?? Canvas resize ?????????????????????????????????????????????????????
            if (_canvas.width !== frame.width || _canvas.height !== frame.height) {
                _canvas.width = frame.width;
                _canvas.height = frame.height;
                gl.viewport(0, 0, frame.width, frame.height);
            }



            // ?? Shadow depth pass — render geometry into each light's shadow FBO ??
            _shadowMode = frame.shadowMode || 0;


            if (_shadowProgram && frame.lightVPs && frame.lightCount > 0) {
                for (let li = 0; li < frame.lightCount; li++) {
                    if (!frame.lightCastsShadows[li]) continue;
                    const lv = frame.lightVPs[li];
                    if (!lv) continue;
                    gl.depthMask(true);
                    gl.enable(gl.DEPTH_TEST);
                    gl.disable(gl.BLEND);
                    gl.bindFramebuffer(gl.FRAMEBUFFER, _shadowFbos[li]);
                    gl.viewport(0, 0, SHADOW_SIZE, SHADOW_SIZE);
                    gl.enable(gl.DEPTH_TEST);
                    gl.clear(gl.DEPTH_BUFFER_BIT | gl.COLOR_BUFFER_BIT);
                    gl.disable(gl.CULL_FACE);  // ADD THIS ONE LINE
                    gl.useProgram(_shadowProgram);
                    gl.uniform1i(_shadowInstancedLoc, 0);
                    // BEFORE the for loop
                    //console.log('[ShadowDebug] frame.meshes ids:', frame.meshes.map(m => m.meshId));
                    //console.log('[ShadowDebug] meshBuffers keys:', Object.keys(_meshBuffers).filter(k => k.includes('Prim') || k.includes('Square')));
                    // Normal mesh shadow pass
                    for (const mesh of frame.meshes) {
                        /*
                           if (mesh.meshId === 'PrimSquare') {
                    console.log('[ShadowDebug] PrimSquare',
                        'buf:', !!_meshBuffers[mesh.meshId],
                        'a:', mesh.a,
                        'emissive:', mesh.isEmissive,
                        'castsShadow:', mesh.castsShadow,
                        'model:', !!mesh.model,
                        'lastModel:', !!(_meshBuffers[mesh.meshId]?.lastModel),
                        'vertCount:', _meshBuffers[mesh.meshId]?.vertCount
                    );
                }
                */
               /*
                        if (mesh.meshId === 'SmoothSphere' || mesh.meshId === 'LightBulb') {
                            console.log('[Shadow] gizmo:', mesh.meshId, 'castsShadow:', mesh.castsShadow);
                        }
                        */
                       // console.log('[ShadowDebug] meshBuffers keys:', Object.keys(_meshBuffers).filter(k => k.includes('Prim') || k.includes('Square')));
                        // ADD THIS:
                        const sq = frame.meshes.find(m => m.meshId === 'PrimSquare_Clone');
                       // console.log('[ShadowDebug] PrimSquare_Clone alpha:', sq?.a, 'castsShadow:', sq?.castsShadow, 'emissive:', sq?.isEmissive);
                        const buf = _meshBuffers[mesh.meshId] || _meshBuffers[mesh.meshId.replace('_Clone', '')];
                        if (!buf) continue;
                        if (!buf) continue;
                        if (mesh.isEmissive) continue;
                        // i tried to disable this to enable shadows but something else is guaurding it
                        if (mesh.castsShadow === false) continue; // explicit non-caster
                        //     if (mesh.a < 0.99) continue;

                        if (mesh.meshId.startsWith('ParticlePool_')) continue;
                        if (mesh.meshId.startsWith('ParticlePrewarm_')) continue;
                        if (mesh.meshId.startsWith('ParticleGeo_')) continue;
                        if (mesh.meshId.startsWith('Text_')) continue;
                        // DEBUG — log every mesh being written into shadow map
                        //    console.log('[ShadowPass] casting:', mesh.meshId, 'alpha:', mesh.a, 'emissive:', mesh.isEmissive);
                        const model = mesh.model || buf.lastModel;
                        if (!model) continue;


                        // Skip meshes sitting at this light's position — light source meshes
                        const lx = frame.lightPositions[li * 3];
                        const ly = frame.lightPositions[li * 3 + 1];
                        const lz = frame.lightPositions[li * 3 + 2];
                        const mx = model[12];
                        const my = model[13];
                        const mz = model[14];
                        const dx = mx - lx, dy = my - ly, dz = mz - lz;
                        if (dx * dx + dy * dy + dz * dz < 1.5) continue;

                        gl.uniformMatrix4fv(_shadowLightMVPLoc, false, lv);
                        gl.uniformMatrix4fv(_shadowModelLoc, false, model);

                        // Bind texture for alpha-tested shadow casting
                        const hasTex = buf.hasTexture && _textureCache[buf.texCacheKey];
                        if (_shadowHasTextureLoc !== null) {
                            gl.uniform1i(_shadowHasTextureLoc, hasTex ? 1 : 0);
                        }
                        if (hasTex) {
                            gl.activeTexture(gl.TEXTURE0);
                            gl.bindTexture(gl.TEXTURE_2D, _textureCache[buf.texCacheKey]);
                            gl.uniform1i(_shadowTextureLoc, 0);
                            gl.uniform1f(_shadowAlphaThresholdLoc, 0.15); // discard fragments below 15% alpha
                        }

                        // Bind UV buffer for shadow VS
                        if (_shadowTexCoordLoc >= 0) {
                            gl.bindBuffer(gl.ARRAY_BUFFER, buf.ubo);
                            gl.enableVertexAttribArray(_shadowTexCoordLoc);
                            gl.vertexAttribPointer(_shadowTexCoordLoc, 2, gl.FLOAT, false, 0, 0);
                        }

                        gl.bindBuffer(gl.ARRAY_BUFFER, buf.vbo);
                        gl.enableVertexAttribArray(_shadowPosLoc);
                        gl.vertexAttribPointer(_shadowPosLoc, 3, gl.FLOAT, false, 0, 0);
                        gl.drawArrays(gl.TRIANGLES, 0, buf.vertCount);
                    }

                    // Foliage instanced shadow pass
                    if (frame.foliageInstances && frame.activeScene === 2) {
                        gl.uniform1i(_shadowInstancedLoc, 1);
                        for (const group of frame.foliageInstances) {
                            if (!group || group.count <= 0) continue;
                            const buf = _meshBuffers[group.meshId];
                            if (!buf || buf.vertCount < 3) continue;
                            const fb = _foliageBuffers[group.meshId];
                            if (!fb || fb.maxCount === 0) continue;

                            gl.uniformMatrix4fv(_shadowLightMVPLoc, false, lv);

                            gl.bindBuffer(gl.ARRAY_BUFFER, buf.vbo);
                            gl.enableVertexAttribArray(_shadowPosLoc);
                            gl.vertexAttribPointer(_shadowPosLoc, 3, gl.FLOAT, false, 0, 0);

                            gl.bindBuffer(gl.ARRAY_BUFFER, fb.posBuf);
                            gl.enableVertexAttribArray(_shadowInstPosLoc);
                            gl.vertexAttribPointer(_shadowInstPosLoc, 3, gl.FLOAT, false, 12, 0);
                            gl.vertexAttribDivisor(_shadowInstPosLoc, 1);

                            gl.drawArraysInstanced(gl.TRIANGLES, 0, buf.vertCount, group.count);

                            gl.vertexAttribDivisor(_shadowInstPosLoc, 0);
                            gl.disableVertexAttribArray(_shadowInstPosLoc);
                        }
                        gl.uniform1i(_shadowInstancedLoc, 0);
                    }

                    // Tile grid shadow pass
                    if (_tileGridVAO && _tileIdxCount > 0 && frame.activeScene === 2) {
                        gl.uniformMatrix4fv(_shadowLightMVPLoc, false, lv);
                        gl.uniformMatrix4fv(_shadowModelLoc, false, new Float32Array([
                            1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1
                        ]));
                        gl.bindVertexArray(_tileGridVAO);
                        gl.bindBuffer(gl.ARRAY_BUFFER, _tileGridVBO);
                        gl.enableVertexAttribArray(_shadowPosLoc);
                        gl.vertexAttribPointer(_shadowPosLoc, 2, gl.FLOAT, false, 0, 0);
                        gl.drawElements(gl.TRIANGLES, _tileIdxCount, gl.UNSIGNED_INT, 0);
                        gl.bindVertexArray(null);
                    }
                }

                gl.bindFramebuffer(gl.FRAMEBUFFER, null);
                gl.viewport(0, 0, _canvas.width, _canvas.height);
            }


            // ?? AA pre-pass — redirect scene render into offscreen FBO if needed ??
            const aaMode = frame.aaMode || 0;
            if ((aaMode === 2 || aaMode === 3 || aaMode === 4 ||
                aaMode === 5 || aaMode === 6 || aaMode === 7) && _fxaaFbo) {
                gl.bindFramebuffer(gl.FRAMEBUFFER, _fxaaFbo);
                gl.activeTexture(gl.TEXTURE0);
                gl.bindTexture(gl.TEXTURE_2D, null);
                gl.activeTexture(gl.TEXTURE1);
                gl.bindTexture(gl.TEXTURE_2D, null);
                gl.activeTexture(gl.TEXTURE2);
                gl.bindTexture(gl.TEXTURE_2D, null);
            } else {
                gl.bindFramebuffer(gl.FRAMEBUFFER, null);
            }

            // ?? Scene clear ???????????????????????????????????????????????????????
            gl.clearColor(0, 0, 0, 1);
            gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
            gl.enable(gl.DEPTH_TEST);
            gl.enable(gl.BLEND);
            gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
            gl.disable(gl.CULL_FACE);
            gl.cullFace(gl.BACK);
            gl.depthFunc(gl.LESS);

            // ?? SkySphere pre-pass ????????????????????????????????????????????????
            // Rendered before all scene geometry.
            // Depth writes OFF so the sky never occludes anything.
            // Uses dedicated sky shader — bypasses all lighting math.
            const skyMesh = frame.meshes.find(m => m.meshId === 'SkySphere');
            if (skyMesh && _skyProgram) {
            
                // Trigger texture loads on first encounter
                if (frame.skyDayTexUrl) {
                    ensureSkyTextures(frame.skyDayTexUrl, frame.skyNightTexUrl);
                }
              // resetSkyTextures();
                gl.depthMask(false);
                gl.disable(gl.DEPTH_TEST);
                gl.disable(gl.CULL_FACE);
                gl.useProgram(_skyProgram);

                // Upload geometry on first frame
                if (skyMesh.upload) uploadMesh(skyMesh.upload);


                // REPLACE WITH:
                const skyBuf = _meshBuffers['UVSphere'] ?? _meshBuffers['SmoothSphere'] ?? _meshBuffers['SkySphere'];
                if (skyBuf) {

                    // MVP — skysphere uses same camera matrix
                    gl.uniformMatrix4fv(_skyLocs.mvp, false, skyMesh.mvp);

                    // Sky uniforms
                    gl.uniform1f(_skyLocs.skyBlend, frame.skyBlend ?? 0.0);
                    gl.uniform3f(_skyLocs.zenith, frame.skyZenithR ?? 0.1, frame.skyZenithG ?? 0.45, frame.skyZenithB ?? 0.9);
                    gl.uniform3f(_skyLocs.horizon, frame.skyHorizonR ?? 0.65, frame.skyHorizonG ?? 0.8, frame.skyHorizonB ?? 1.0);
                    gl.uniform3f(_skyLocs.sunDir, frame.sunDirX ?? 0.0, frame.sunDirY ?? -1.0, frame.sunDirZ ?? 0.0);
                    gl.uniform1f(_skyLocs.timeOfDay, frame.timeOfDay ?? 0.5);
                    gl.uniform1f(_skyLocs.cloudOffset, frame.cloudOffset ?? 0.0);
                    gl.uniform1f(_skyLocs.starOffset, frame.starOffset ?? 0.0);
                    gl.uniform1f(_skyLocs.cloudScale, frame.cloudScale ?? 2.0);
                    gl.uniform1f(_skyLocs.starScale, frame.starScale ?? 3.0);
                    gl.uniform3f(_skyLocs.moonDir, frame.moonDirX ?? 0.0, frame.moonDirY ?? 0.0, frame.moonDirZ ?? 1.0);
                    gl.uniform3f(_skyLocs.moonColor, frame.moonColorR ?? 0.7, frame.moonColorG ?? 0.8, frame.moonColorB ?? 1.0);
                    gl.uniform1f(_skyLocs.moonGlow, frame.moonGlow ?? 0.0);


                    gl.activeTexture(gl.TEXTURE0);
                    gl.bindTexture(gl.TEXTURE_2D, _skyDayTex ?? null);
                    gl.uniform1i(_skyLocs.dayTex, 0);

                    gl.activeTexture(gl.TEXTURE1);
                    gl.bindTexture(gl.TEXTURE_2D, _skyNightTex ?? null);
                    gl.uniform1i(_skyLocs.nightTex, 1);

                    // Vertex buffers
                    gl.bindBuffer(gl.ARRAY_BUFFER, skyBuf.vbo);
                    gl.enableVertexAttribArray(_skyLocs.pos);
                    gl.vertexAttribPointer(_skyLocs.pos, 3, gl.FLOAT, false, 0, 0);

                    gl.bindBuffer(gl.ARRAY_BUFFER, skyBuf.ubo);
                    gl.enableVertexAttribArray(_skyLocs.uv);
                    gl.vertexAttribPointer(_skyLocs.uv, 2, gl.FLOAT, false, 0, 0);


                    gl.drawArrays(gl.TRIANGLES, 0, skyBuf.vertCount);
                }

                // Restore state for main scene render
                gl.depthMask(true);
                gl.enable(gl.DEPTH_TEST);
            }



            // Upload tile textures on first scene 2 frame
            if (frame.tileMapTextures && frame.tileMapTextures.length > 0) {
                uploadTileTextures(gl, frame.tileMapTextures);
            }

            // Update instance buffer when tilemap is dirty
            if (frame.tileMap && frame.tileMap.isDirty) {
                updateTileHeights(gl, frame.tileMap);
            }
            // Draw landscape tilemap
            if (frame.activeScene === 2) {
                drawTileMap(gl, frame);
            }


            // ?? Select active shadow program + cached locations for this mode ?????
            const shadowMode = frame.shadowMode || 0;
            _activeProgram = _programs[shadowMode] || _programs[0];
            _activeLocs = _programLocations[shadowMode] || _programLocations[0];
            gl.useProgram(_activeProgram);

            // ?? Global uniforms — camera + lights, set once per frame ?????????????
            gl.uniform3f(_activeLocs.camPos, frame.camX, frame.camY, frame.camZ);
            gl.uniform1i(_activeLocs.lightCount, frame.lightCount);

            for (let li = 0; li < frame.lightCount; li++) {
                gl.uniform3f(_activeLocs.lightPos[li],
                    frame.lightPositions[li * 3], frame.lightPositions[li * 3 + 1], frame.lightPositions[li * 3 + 2]);
                gl.uniform3f(_activeLocs.lightColor[li],
                    frame.lightColors[li * 3], frame.lightColors[li * 3 + 1], frame.lightColors[li * 3 + 2]);
                gl.uniform3f(_activeLocs.lightDir[li],
                    frame.lightDirections[li * 3], frame.lightDirections[li * 3 + 1], frame.lightDirections[li * 3 + 2]);
                gl.uniform1f(_activeLocs.lightIntensity[li], frame.lightIntensities[li]);
                gl.uniform1f(_activeLocs.lightRange[li], frame.lightRanges[li]);
                gl.uniform1i(_activeLocs.lightType[li], frame.lightTypes[li]);
                gl.uniform1f(_activeLocs.lightSpotAngle[li], frame.lightSpotAngles[li]);
            }

            // ?? Bind shadow depth textures to texture units 1-8 ??????????????????
            for (let li = 0; li < frame.lightCount; li++) {
                if (_shadowDepthTexs[li]) {
                    gl.activeTexture(gl.TEXTURE1 + li);
                    gl.bindTexture(gl.TEXTURE_2D, _shadowDepthTexs[li]);
                    gl.uniform1i(_activeLocs.shadowMap[li], 1 + li);
                    if (frame.lightVPs[li]) {
                        gl.uniformMatrix4fv(_activeLocs.lightVP[li], false, frame.lightVPs[li]);
                    }
                }
            }

            // ?? TAA jitter — subpixel offset for temporal accumulation ????????????
            if (aaMode === 4 && _activeLocs.jitter) {
                gl.uniform2f(_activeLocs.jitter, frame.jitterX, frame.jitterY);
            }

            // ?? SpectralXS shadow uniforms — only sent when mode 2 is active ?????????????
            if (shadowMode === 2 || shadowMode === 3 || shadowMode === 4) {
                gl.uniform1f(_activeLocs.shadowSoftnessBias, frame.shadowSoftnessBias);
                gl.uniform1f(_activeLocs.shadowBlockerSearchRadius, frame.shadowBlockerSearchRadius);
                gl.uniform1f(_activeLocs.shadowKernelSize, frame.shadowKernelSize);
                gl.uniform1f(_activeLocs.shadowContactSharpness, frame.shadowContactSharpness);
                gl.uniform1f(_activeLocs.shadowDepthBias, frame.shadowDepthBias);
                gl.uniform1f(_activeLocs.shadowTintR, frame.shadowTintR);
                gl.uniform1f(_activeLocs.shadowTintG, frame.shadowTintG);
                gl.uniform1f(_activeLocs.shadowTintB, frame.shadowTintB);
                gl.uniform1f(_activeLocs.shadowTintStrength, frame.shadowTintStrength);
                gl.uniform1f(_activeLocs.shadowPenumbraTintStrength, frame.shadowPenumbraTintStrength);
            }

            // ?? Mesh draw loop — opaque first, transparent after ??????????????????
            const sortedMeshes = [...frame.meshes].sort((a, b) => {
                const aT = a.a < 0.99 ? 1 : 0;
                const bT = b.a < 0.99 ? 1 : 0;
                if (aT !== bT) return aT - bT; // opaques first
                if (aT === 1) {
                    // both transparent — back to front by distance
                    const ax = (a.model?.[12] ?? 0) - frame.camX;
                    const ay = (a.model?.[13] ?? 0) - frame.camY;
                    const az = (a.model?.[14] ?? 0) - frame.camZ;
                    const bx = (b.model?.[12] ?? 0) - frame.camX;
                    const by = (b.model?.[13] ?? 0) - frame.camY;
                    const bz = (b.model?.[14] ?? 0) - frame.camZ;
                    return (bx * bx + by * by + bz * bz) - (ax * ax + ay * ay + az * az);
                }
                return 0;
            });

            for (const mesh of sortedMeshes) {
                // Upload geometry to GPU on first encounter — needed for texture cache
                if (mesh.upload) uploadMesh(mesh.upload);

                // Skip particles — drawn by instanced renderParticles()
                if (mesh.meshId.startsWith('ParticlePool_') ||
                    mesh.meshId.startsWith('ParticleGeo_') ||
                    mesh.meshId.startsWith('ParticlePrewarm_') ||
                    mesh.meshId.startsWith('Text_') ||
                    mesh.meshId === 'SkySphere') continue;

                const buf = _meshBuffers[mesh.meshId];
                if (!buf) continue;

                // Per-mesh transforms + material
                gl.uniformMatrix4fv(_activeLocs.mvp, false, mesh.mvp);
                if (mesh.model) {
                    gl.uniformMatrix4fv(_activeLocs.model, false, mesh.model);
                    buf.lastModel = mesh.model;
                } else if (buf.lastModel) {
                    gl.uniformMatrix4fv(_activeLocs.model, false, buf.lastModel);
                }
                gl.uniform4f(_activeLocs.color, mesh.r, mesh.g, mesh.b, mesh.a);
                gl.uniform1i(_activeLocs.emissive, mesh.isEmissive ? 1 : 0);
                gl.uniform1f(_activeLocs.emissiveIntensity, mesh.emissiveIntensity ?? 1.0);
                gl.uniform2f(_activeLocs.uvOffset, mesh.uvOffsetX || 0.0, mesh.uvOffsetY || 0.0);
                gl.uniform2f(_activeLocs.uvScale, mesh.uvScaleX || 1.0, mesh.uvScaleY || 1.0);

                if (mesh.a < 0.99) {
                    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
                    gl.depthMask(false);
                } else {
                    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
                    gl.depthMask(mesh.a >= 0.99);
                }
                if (mesh.a < 0.99) {
                    gl.depthMask(false);
                } else {
                    gl.depthMask(true);
                    gl.enable(gl.DEPTH_TEST);
                }

                // Vertex buffers
                gl.bindBuffer(gl.ARRAY_BUFFER, buf.vbo);
                gl.enableVertexAttribArray(_activeLocs.pos);
                gl.vertexAttribPointer(_activeLocs.pos, 3, gl.FLOAT, false, 0, 0);

                gl.bindBuffer(gl.ARRAY_BUFFER, buf.nbo);
                gl.enableVertexAttribArray(_activeLocs.norm);
                gl.vertexAttribPointer(_activeLocs.norm, 3, gl.FLOAT, false, 0, 0);

                gl.bindBuffer(gl.ARRAY_BUFFER, buf.ubo);
                gl.enableVertexAttribArray(_activeLocs.texCoord);
                gl.vertexAttribPointer(_activeLocs.texCoord, 2, gl.FLOAT, false, 0, 0);

                // Find texture — use texCacheKey from buffer (handles shared particle geo)
                const resolvedTexKey = buf.texCacheKey || mesh.meshId;

                if (buf.hasTexture && _textureCache[resolvedTexKey]) {
                    gl.activeTexture(gl.TEXTURE0);
                    gl.bindTexture(gl.TEXTURE_2D, _textureCache[resolvedTexKey]);
                    gl.uniform1i(_activeLocs.tex, 0);
                    gl.uniform1i(_activeLocs.hasTex, 1);
                } else {
                    gl.uniform1i(_activeLocs.hasTex, 0);
                }

                // ?? Transparent mesh — double-sided two-pass render ???????????????
                if (mesh.a < 0.99) {
                    gl.depthMask(false);
                    gl.enable(gl.CULL_FACE);

                    // Pass 1 — back faces
                    gl.cullFace(gl.FRONT);
                    if (buf.matBreaks && buf.matBreaks.length >= 1) {
                        let offsets = [], running = 0;
                        for (let i = 0; i < buf.matBreaks.length; i++) {
                            offsets[i] = running;
                            running += buf.matBreaks[i];
                        }
                        for (let m = 0; m < buf.matBreaks.length; m++) {
                            const matIdx = buf.matIndices[m];
                            const matTex = buf.materialTextures[matIdx];
                            const matLoaded = buf.matTexLoaded[matIdx];
                            if (matTex && matLoaded) {
                                gl.activeTexture(gl.TEXTURE0);
                                gl.bindTexture(gl.TEXTURE_2D, matTex);
                                gl.uniform1i(_activeLocs.tex, 0);
                                gl.uniform1i(_activeLocs.hasTex, 1);
                                gl.uniform4f(_activeLocs.color, mesh.r, mesh.g, mesh.b, mesh.a);
                            } else if (buf.materialColors && buf.materialColors[matIdx]) {
                                const hasCSharpColor = !(mesh.r >= 0.99 && mesh.g >= 0.99 && mesh.b >= 0.99);
                                if (hasCSharpColor) {
                                    gl.uniform4f(_activeLocs.color, mesh.r, mesh.g, mesh.b, mesh.a);
                                } else {
                                    const parts = buf.materialColors[matIdx].split(',');
                                    gl.uniform4f(_activeLocs.color,
                                        parseFloat(parts[0]),
                                        parseFloat(parts[1]),
                                        parseFloat(parts[2]),
                                        mesh.a);
                                }
                                gl.uniform1i(_activeLocs.hasTex, 0);


                            } else {
                                gl.uniform4f(_activeLocs.color, mesh.r, mesh.g, mesh.b, mesh.a);
                                gl.uniform1i(_activeLocs.hasTex, 0);
                            }



                            gl.drawArrays(gl.TRIANGLES, offsets[m], buf.matBreaks[m]);
                        }
                    } else {

                        gl.drawArrays(gl.TRIANGLES, 0, buf.vertCount);
                    }

                    // Pass 2 — front faces
                    gl.cullFace(gl.BACK);
                    if (buf.matBreaks && buf.matBreaks.length >= 1) {
                        let offsets = [], running = 0;
                        for (let i = 0; i < buf.matBreaks.length; i++) {
                            offsets[i] = running;
                            running += buf.matBreaks[i];
                        }
                        for (let m = 0; m < buf.matBreaks.length; m++) {
                            const matIdx = buf.matIndices[m];
                            const matTex = buf.materialTextures[matIdx];
                            const matLoaded = buf.matTexLoaded[matIdx];
                            if (matTex && matLoaded) {
                                gl.activeTexture(gl.TEXTURE0);
                                gl.bindTexture(gl.TEXTURE_2D, matTex);
                                gl.uniform1i(_activeLocs.tex, 0);
                                gl.uniform1i(_activeLocs.hasTex, 1);
                                gl.uniform4f(_activeLocs.color, mesh.r, mesh.g, mesh.b, mesh.a);
                            } else if (buf.materialColors && buf.materialColors[matIdx]) {
                                const hasCSharpColor = !(mesh.r >= 0.99 && mesh.g >= 0.99 && mesh.b >= 0.99);
                                if (hasCSharpColor) {
                                    gl.uniform4f(_activeLocs.color, mesh.r, mesh.g, mesh.b, mesh.a);
                                } else {
                                    const parts = buf.materialColors[matIdx].split(',');
                                    gl.uniform4f(_activeLocs.color,
                                        parseFloat(parts[0]),
                                        parseFloat(parts[1]),
                                        parseFloat(parts[2]),
                                        mesh.a);
                                }
                                gl.uniform1i(_activeLocs.hasTex, 0);

                            } else {
                                gl.uniform4f(_activeLocs.color, mesh.r, mesh.g, mesh.b, mesh.a);
                                gl.uniform1i(_activeLocs.hasTex, 0);
                            }
                            gl.drawArrays(gl.TRIANGLES, offsets[m], buf.matBreaks[m]);
                        }
                    } else {
                        gl.drawArrays(gl.TRIANGLES, 0, buf.vertCount);
                    }

                    gl.disable(gl.CULL_FACE);
                    gl.cullFace(gl.BACK);
                    gl.depthMask(true);

                    /*
// Right before drawing each mesh
console.log('[Cull state]', mesh.meshId, 
'cullFace enabled:', gl.isEnabled(gl.CULL_FACE),
'depthMask:', gl.getParameter(gl.DEPTH_WRITEMASK),
'alpha:', mesh.a);

*/
                    // ?? Opaque mesh — standard single-pass render ?????????????????????
                } else {
                    gl.depthMask(true);
                    gl.enable(gl.DEPTH_TEST);

                    if (buf.matBreaks && buf.matBreaks.length >= 1) {
                        let offsets = [], running = 0;
                        for (let i = 0; i < buf.matBreaks.length; i++) {
                            offsets[i] = running;
                            running += buf.matBreaks[i];
                        }
                        for (let m = 0; m < buf.matBreaks.length; m++) {
                            const matIdx = buf.matIndices[m];
                            const matTex = buf.materialTextures[matIdx];
                            const matLoaded = buf.matTexLoaded[matIdx];
                            if (matTex && matLoaded) {
                                gl.activeTexture(gl.TEXTURE0);
                                gl.bindTexture(gl.TEXTURE_2D, matTex);
                                gl.uniform1i(_activeLocs.tex, 0);
                                gl.uniform1i(_activeLocs.hasTex, 1);
                                gl.uniform4f(_activeLocs.color, mesh.r, mesh.g, mesh.b, mesh.a);
                            } else if (buf.hasTexture && _textureCache[buf.texCacheKey]) {
                                // Fallback — mesh-level texture (used by primitives with sprite sheets)
                                gl.activeTexture(gl.TEXTURE0);
                                gl.bindTexture(gl.TEXTURE_2D, _textureCache[buf.texCacheKey]);
                                gl.uniform1i(_activeLocs.tex, 0);
                                gl.uniform1i(_activeLocs.hasTex, 1);
                                gl.uniform4f(_activeLocs.color, mesh.r, mesh.g, mesh.b, mesh.a);
                            } else if (buf.materialColors && buf.materialColors[matIdx]) {
                                const hasCSharpColor = !(mesh.r >= 0.99 && mesh.g >= 0.99 && mesh.b >= 0.99);
                                if (hasCSharpColor) {
                                    gl.uniform4f(_activeLocs.color, mesh.r, mesh.g, mesh.b, mesh.a);
                                } else {
                                    const parts = buf.materialColors[matIdx].split(',');
                                    gl.uniform4f(_activeLocs.color,
                                        parseFloat(parts[0]),
                                        parseFloat(parts[1]),
                                        parseFloat(parts[2]),
                                        mesh.a);
                                }
                                gl.uniform1i(_activeLocs.hasTex, 0);
                            } else {
                                gl.uniform4f(_activeLocs.color, mesh.r, mesh.g, mesh.b, mesh.a);
                                gl.uniform1i(_activeLocs.hasTex, 0);
                            }

                            gl.drawArrays(gl.TRIANGLES, offsets[m], buf.matBreaks[m]);
                        }
                    } else {
                        gl.drawArrays(gl.TRIANGLES, 0, buf.vertCount);
                    }
                }

                gl.depthMask(true);
                gl.enable(gl.DEPTH_TEST);
            }
            // After the main for(mesh of sortedMeshes) loop closes
            renderParticles(frame);
            renderFoliage(frame);
            // ?? AA post-pass routing ??????????????????????????????????????????????
            if (aaMode === 1) applyMSAA();
            else if (aaMode === 2) applyFXAA();
            else if (aaMode === 3) applySMAA();
            else if (aaMode === 4) applyTAA();
            else if (aaMode === 5) applySpectralAA();
            else if (aaMode === 6) applySpectralAAV2();
            else if (aaMode === 7) applySpectralAAV3();
            // Text renders AFTER AA — always draws to default framebuffer
            gl.bindFramebuffer(gl.FRAMEBUFFER, null);
            renderText(frame);
            _scrollbarCurrentZ = frame.scrollbarZ ?? 10;
            renderScrollbar(frame);
            // Check loader done condition every frame

            if (SpectralGLLoader._visible) {
                if (SpectralGLLoader.isDone()) {
                    SpectralGLLoader.hide();
                } else {
                    console.log('[Loader] not done —',
                        'requested:', SpectralGLLoader._requested,
                        'completed:', SpectralGLLoader._completed,
                        'tilemap:', SpectralGLLoader._specialFlags.tilemap,
                        'cubemap:', SpectralGLLoader._specialFlags.cubemap
                    );
                }
            }
        } catch (ex) {
            console.error("[SpectralGL] renderFrame error:", ex);
        }
    }

    function ensureSkyTexturesCube(dayUrl, nightUrl) {
        const gl = _gl;

        function loadSeamlessCube(url, callback) {
            const img = new Image();
            img.onload = () => {
                const tex = gl.createTexture();
                gl.bindTexture(gl.TEXTURE_CUBE_MAP, tex);

                // Stamp the same seamless texture onto all 6 cube faces
                const faces = [
                    gl.TEXTURE_CUBE_MAP_POSITIVE_X,
                    gl.TEXTURE_CUBE_MAP_NEGATIVE_X,
                    gl.TEXTURE_CUBE_MAP_POSITIVE_Y,
                    gl.TEXTURE_CUBE_MAP_NEGATIVE_Y,
                    gl.TEXTURE_CUBE_MAP_POSITIVE_Z,
                    gl.TEXTURE_CUBE_MAP_NEGATIVE_Z,
                ];

                for (const face of faces) {
                    gl.texImage2D(face, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, img);
                }

                gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
                gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
                gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
                gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
                gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_WRAP_R, gl.CLAMP_TO_EDGE);

                callback(tex);
                console.log('[SpectralGL] Seamless cubemap loaded:', url, img.width, img.height);
            };
            img.onerror = () => console.error('[SpectralGL] Cubemap FAILED:', url);
            img.src = url;
        }

        if (!_skyCubeLoaded.day && dayUrl) {
            _skyCubeLoaded.day = true;
            loadSeamlessCube(dayUrl, (tex) => {
                _skyCubeDayTex = tex;
            });
        }

        if (!_skyCubeLoaded.night && nightUrl) {
            _skyCubeLoaded.night = true;
            loadSeamlessCube(nightUrl, (tex) => {
                _skyCubeNightTex = tex;
            });
        }
    }

    function ensureSkyTextures(dayUrl, nightUrl) {
        const gl = _gl;

        // ✅ Reset if URLs change
        if (dayUrl !== _lastSkyDayUrl || nightUrl !== _lastSkyNightUrl) {
            resetSkyTextures();
            _lastSkyDayUrl = dayUrl;
            _lastSkyNightUrl = nightUrl;
        }

        // ✅ Reset if GL context changes
        if (_lastGL !== _gl) {
            resetSkyTextures();
            _lastGL = _gl;
        }

        // 🌤 DAY TEXTURE
        if (!_skyTexsLoaded.day && dayUrl) {
            _skyDayTex = gl.createTexture();
            gl.bindTexture(gl.TEXTURE_2D, _skyDayTex);

            // temp pixel so shader has something immediately
            gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 1, 1, 0,
                gl.RGBA, gl.UNSIGNED_BYTE, new Uint8Array([255, 0, 255, 255]));

            const img = new Image();

            img.onload = () => {
                gl.bindTexture(gl.TEXTURE_2D, _skyDayTex);
                gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);

                gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA,
                    gl.RGBA, gl.UNSIGNED_BYTE, img);

                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.REPEAT);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);

                _skyTexsLoaded.day = true; // ✅ moved here

                console.log('[SpectralGL] Sky day texture loaded:', img.width, img.height);
            };

            img.onerror = () => {
                console.error('[SpectralGL] Sky day texture FAILED:', dayUrl);
            };

            img.src = dayUrl;
        }

        // 🌙 NIGHT TEXTURE
        if (!_skyTexsLoaded.night && nightUrl) {
            _skyNightTex = gl.createTexture();
            gl.bindTexture(gl.TEXTURE_2D, _skyNightTex);

            // temp pixel
            gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 1, 1, 0,
                gl.RGBA, gl.UNSIGNED_BYTE, new Uint8Array([0, 0, 0, 255]));

            const img = new Image();

            img.onload = () => {
                gl.bindTexture(gl.TEXTURE_2D, _skyNightTex);
                gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);

                gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA,
                    gl.RGBA, gl.UNSIGNED_BYTE, img);

                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.REPEAT);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);

                _skyTexsLoaded.night = true; // ✅ moved here

                console.log('[SpectralGL] Sky night texture loaded:', img.width, img.height);
            };

            img.onerror = () => {
                console.error('[SpectralGL] Sky night texture FAILED:', nightUrl);
            };

            img.src = nightUrl;
        }
    }

    function uploadTileTextures(gl, texturePaths) {
        let loaded = 0;
        const total = texturePaths.length;

        texturePaths.forEach((path, i) => {
            const img = new Image();
            SpectralGLLoader.onAssetRequested();
            img.onload = () => {
                const tex = gl.createTexture();
                gl.bindTexture(gl.TEXTURE_2D, tex);
                gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA,
                    gl.UNSIGNED_BYTE, img);

                // Nearest-neighbor for pixel art tiles, mipmap for distance
                gl.generateMipmap(gl.TEXTURE_2D);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER,
                    gl.NEAREST_MIPMAP_LINEAR);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER,
                    gl.NEAREST);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S,
                    gl.REPEAT);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T,
                    gl.REPEAT);

                _tileTextures[i] = tex;
                loaded++;
                SpectralGLLoader.onAssetComplete();

                if (loaded === total) {
                    _tileTexturesReady = true;
                    console.log('[TileMap] All ' + total + ' textures uploaded');
                    // Notify C# that upload is complete — prevents double send
                    if (_dotnetRef)
                        _dotnetRef.invokeMethodAsync('OnTileTexturesUploaded');
                }
            };
            img.onerror = () => console.warn('[TileMap] Failed to load: ' + path);
            img.src = path;
        });
    }


    function updateTileHeights(gl, tileData) {
        const GS = GRID_SIZE;
        const GSP1 = GS + 1;

        if (tileData.isFullUpload) {
            // Full upload path
            const heights = new Float32Array(tileData.heights);
            gl.bindBuffer(gl.ARRAY_BUFFER, _tileHeightVBO);
            gl.bufferSubData(gl.ARRAY_BUFFER, 0, heights);

            // Normals computed in JS from heights — not sent from C#
            const normals = computeTileNormalsFromHeights(heights, GRID_SIZE);
            console.log('[Normals] sample:', normals[0], normals[1], normals[2], 'length:', normals.length);
            gl.bindBuffer(gl.ARRAY_BUFFER, _tileNormalVBO);
            gl.bufferSubData(gl.ARRAY_BUFFER, 0, normals);

            const matData = new Float32Array(GS * GS * 4);
            for (let i = 0; i < GS * GS; i++) {
                matData[i * 4] = tileData.materials[i];
                matData[i * 4 + 1] = tileData.blendWeights[i];
                matData[i * 4 + 2] = tileData.blendMaterials[i];
                matData[i * 4 + 3] = 0;
            }
            gl.bindTexture(gl.TEXTURE_2D, _tileMatTex);
            gl.texSubImage2D(gl.TEXTURE_2D, 0, 0, 0,
                GS, GS, gl.RGBA, gl.FLOAT, matData);

        } else {
            // Partial upload — only dirty rectangle
            const x0 = tileData.dirtyX, y0 = tileData.dirtyY;
            const w = tileData.dirtyW, h = tileData.dirtyH;
            const vW = w + 1, vH = h + 1;

            // Heights — patch rows into full VBO
            const heights = new Float32Array(tileData.heights);
            for (let row = 0; row < vH; row++) {
                const globalY = y0 + row;
                if (globalY > GS) break;
                const byteOffset = (globalY * GSP1 + x0) * 4;
                const rowSlice = heights.subarray(row * vW, row * vW + vW);
                gl.bindBuffer(gl.ARRAY_BUFFER, _tileHeightVBO);
                gl.bufferSubData(gl.ARRAY_BUFFER, byteOffset, rowSlice);
            }

            // Normals — compute from patch heights, upload with GSP1 stride
            const normals = computeTileNormalsFromHeights(heights, vW - 1);
            for (let row = 0; row <= h; row++) {
                const gy = y0 + row;
                if (gy > GS) break;
                const byteOffset = (gy * GSP1 + x0) * 3 * 4;
                const rowSlice = normals.subarray(row * vW * 3, (row + 1) * vW * 3);
                gl.bindBuffer(gl.ARRAY_BUFFER, _tileNormalVBO);
                gl.bufferSubData(gl.ARRAY_BUFFER, byteOffset, rowSlice);
            }

            // Material texture — partial patch
            const matPatch = new Float32Array(w * h * 4);
            for (let i = 0; i < w * h; i++) {
                matPatch[i * 4] = tileData.materials[i];
                matPatch[i * 4 + 1] = tileData.blendWeights[i];
                matPatch[i * 4 + 2] = tileData.blendMaterials[i];
                matPatch[i * 4 + 3] = 0;
            }
            gl.bindTexture(gl.TEXTURE_2D, _tileMatTex);
            gl.texSubImage2D(gl.TEXTURE_2D, 0, x0, y0,
                w, h, gl.RGBA, gl.FLOAT, matPatch);
        }
    }

    function computeTileNormalsFromHeights(heights, gridSize) {
        const GSP1 = gridSize + 1;
        const normals = new Float32Array(GSP1 * GSP1 * 3);

        for (let y = 0; y <= gridSize; y++) {
            for (let x = 0; x <= gridSize; x++) {
                const hC = heights[y * GSP1 + x];
                const hL = x > 0 ? heights[y * GSP1 + (x - 1)] : hC;
                const hR = x < gridSize ? heights[y * GSP1 + (x + 1)] : hC;
                const hD = y > 0 ? heights[(y - 1) * GSP1 + x] : hC;
                const hU = y < gridSize ? heights[(y + 1) * GSP1 + x] : hC;

                const dX = (hR - hL) * 0.5;
                const dY = (hU - hD) * 0.5;
                const nx = -dX, ny = -dY, nz = 1.0;
                const len = Math.sqrt(nx * nx + ny * ny + nz * nz);

                const idx = (y * GSP1 + x) * 3;
                normals[idx] = nx / len;
                normals[idx + 1] = ny / len;
                normals[idx + 2] = nz / len;
            }
        }
        return normals;
    }








    function drawTileMap(gl, frame) {
        if (!_tileProgram || !_tileGridVAO || !_tileTexturesReady) return;
        if (!_tileMatTex || !_tileUniforms || _tileIdxCount === 0) return;

        gl.useProgram(_tileProgram);
        gl.bindVertexArray(_tileGridVAO);

        if (!_tileViewMatrixF32) _tileViewMatrixF32 = new Float32Array(16);
        if (!_tileProjMatrixF32) _tileProjMatrixF32 = new Float32Array(16);
        _tileViewMatrixF32.set(frame.viewMatrix);
        _tileProjMatrixF32.set(frame.projMatrix);

        gl.uniformMatrix4fv(_tileUniforms.view, false, _tileViewMatrixF32);
        gl.uniformMatrix4fv(_tileUniforms.projection, false, _tileProjMatrixF32);

        // Material textures on units 0-5
        for (let i = 0; i < 6; i++) {
            gl.activeTexture(gl.TEXTURE0 + i);
            gl.bindTexture(gl.TEXTURE_2D, _tileTextures[i] || null);
            gl.uniform1i(_tileUniforms['tex' + i], i);
        }

        // Tile data texture on unit 6
        gl.activeTexture(gl.TEXTURE6);
        gl.bindTexture(gl.TEXTURE_2D, _tileMatTex);
        gl.uniform1i(_tileUniforms.tileData, 6);

        gl.uniform2f(_tileUniforms.gridOrigin, GRID_ORIGIN_X, GRID_ORIGIN_Y);
        gl.uniform1f(_tileUniforms.gridSize, GRID_SIZE);
        gl.uniform1f(_tileUniforms.tileSize, TILE_SIZE);
        gl.uniform3f(_tileUniforms.sunDir, frame.sunDirX, frame.sunDirY, frame.sunDirZ);
        gl.uniform3f(_tileUniforms.sunColor, frame.sunColorR ?? 1.0, frame.sunColorG ?? 0.95, frame.sunColorB ?? 0.8);
        gl.uniform1f(_tileUniforms.sunIntensity, frame.sunIntensity ?? 1.0);
        gl.uniform3f(_tileUniforms.ambient, frame.ambientR ?? 0.3, frame.ambientG ?? 0.3, frame.ambientB ?? 0.3);

        const tm = frame.tileMap;
        gl.uniform2f(_tileUniforms.brushPos, frame.brushWorldX ?? 0.0, frame.brushWorldY ?? 0.0);
        gl.uniform1f(_tileUniforms.brushRadius, Math.max(frame.brushRadius ?? 0.5, 0.5));
        gl.uniform1f(_tileUniforms.brushActive, frame.landscapeActive ? 1.0 : 0.0);


        // Sun shadow map — always slot 7 to avoid colliding with material textures 0-6
        // Find directional light index — lightTypes[i] === 1 is directional
        let sunIdx = -1;
        if (frame.lightTypes) {
            for (let i = 0; i < frame.lightCount; i++) {
                if (frame.lightTypes[i] === 1) { sunIdx = i; break; }
            }
        }

        // Upload tile lights regardless of shadow state
        gl.uniform1i(_tileUniforms.tileLightCount, frame.lightCount);
        for (let i = 0; i < frame.lightCount; i++) {
            gl.uniform3f(_tileUniforms.tileLightPos[i],
                frame.lightPositions[i * 3],
                frame.lightPositions[i * 3 + 1],
                frame.lightPositions[i * 3 + 2]);
            gl.uniform3f(_tileUniforms.tileLightColor[i],
                frame.lightColors[i * 3],
                frame.lightColors[i * 3 + 1],
                frame.lightColors[i * 3 + 2]);
            gl.uniform1f(_tileUniforms.tileLightIntensity[i], frame.lightIntensities[i]);
            gl.uniform1f(_tileUniforms.tileLightRange[i], frame.lightRanges[i]);
            gl.uniform1i(_tileUniforms.tileLightType[i], frame.lightTypes[i]);
            gl.uniform3f(_tileUniforms.tileLightDir[i],
                frame.lightDirections[i * 3],
                frame.lightDirections[i * 3 + 1],
                frame.lightDirections[i * 3 + 2]);
            gl.uniform1f(_tileUniforms.tileLightSpotAngle[i], frame.lightSpotAngles[i]);
        }

        // Shadow map bind — separate from light upload
        if (sunIdx >= 0 && _shadowDepthTexs[sunIdx]) {
            gl.activeTexture(gl.TEXTURE7);
            gl.bindTexture(gl.TEXTURE_2D, _shadowDepthTexs[sunIdx]);
            gl.uniform1i(_tileUniforms.shadowMap0, 7);
            if (frame.lightVPs && frame.lightVPs[sunIdx]) {
                gl.uniformMatrix4fv(_tileUniforms.lightVP0, false,
                    new Float32Array(frame.lightVPs[sunIdx]));
            }
            gl.uniform1f(_tileUniforms.shadowBias, 0.003);
        }


        gl.drawElements(gl.TRIANGLES, _tileIdxCount, gl.UNSIGNED_INT, 0);

        gl.bindVertexArray(null);
    }



    function renderScrollbar(frame) {
        if (!_scrollbarProgram) initScrollbar();
        if (!frame || frame.cameraMode !== 0) return; // only in WebpageView

        const gl = _gl;
        const w = _canvas.width;
        const h = _canvas.height;

        // Update glow pulse
        _scrollbarGlowPhase += 0.05;
        const glow = (Math.sin(_scrollbarGlowPhase) * 0.5 + 0.5) * 2.0;

        // Track dimensions in NDC
        const trackX0 = 0.96;
        const trackX1 = 1.0;
        const trackY0 = -0.98;
        const trackY1 = 0.98;

        // Thumb size and position
        const range = _scrollbarMaxZ - _scrollbarMinZ;
        const t = 1.0 - ((_scrollbarCurrentZ - _scrollbarMinZ) / range);
        const thumbH = 0.15;
        const thumbY1 = trackY1 - t * (trackY1 - trackY0 - thumbH);
        const thumbY0 = thumbY1 - thumbH;

        // Store for drag detection
        _scrollbarThumbY = thumbY0;
        _scrollbarThumbH = thumbH;

        gl.disable(gl.DEPTH_TEST);
        gl.depthMask(false);
        gl.enable(gl.BLEND);
        gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
        gl.bindFramebuffer(gl.FRAMEBUFFER, null);
        gl.useProgram(_scrollbarProgram);

        // Draw track
        drawScrollbarRect(
            trackX0, trackY0, trackX1, trackY1,
            0.3, 0.0, 0.0, 0.6,
            glow * 0.3
        );

        // Draw thumb — SUPER SAIYAN RED GLOW
        drawScrollbarRect(
            trackX0 - 0.01, thumbY0, trackX1 + 0.01, thumbY1,
            1.0, 0.05, 0.0, 0.95,
            glow
        );

        // Inner thumb highlight
        drawScrollbarRect(
            trackX0 + 0.005, thumbY0 + 0.005, trackX1 - 0.005, thumbY1 - 0.005,
            1.0, 0.4, 0.1, 0.8,
            glow * 1.5
        );

        gl.enable(gl.DEPTH_TEST);
        gl.depthMask(true);
    }

    function drawScrollbarRect(x0, y0, x1, y1, r, g, b, a, glow) {
        const gl = _gl;
        const verts = new Float32Array([
            x0, y0, x1, y0, x0, y1,
            x0, y1, x1, y0, x1, y1,
        ]);
        gl.bindBuffer(gl.ARRAY_BUFFER, _scrollbarVbo);
        gl.bufferData(gl.ARRAY_BUFFER, verts, gl.DYNAMIC_DRAW);
        gl.enableVertexAttribArray(_scrollbarLocs.pos);
        gl.vertexAttribPointer(_scrollbarLocs.pos, 2, gl.FLOAT, false, 0, 0);
        gl.uniform4f(_scrollbarLocs.color, r, g, b, a);
        gl.uniform1f(_scrollbarLocs.glow, glow);
        gl.drawArrays(gl.TRIANGLES, 0, 6);
    }




    function renderParticles(frame) {


        if (!_particleProgram || !frame.particleInstances || !frame.particleInstances.length) return;

        const gl = _gl;

        gl.useProgram(_particleProgram);
        gl.enable(gl.BLEND);
        gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
        gl.depthMask(false);

        const { vp: vpLoc, camR: camRLoc, camU: camULoc, tex: texLoc,
            hasTex: hasTexLoc, pos: posLoc, uv: uvLoc,
            off: offLoc, col: colLoc, size: sizeLoc } = _pLocs;

        gl.uniformMatrix4fv(vpLoc, false, frame.vp);
        gl.uniform3f(camRLoc, frame.camRight[0], frame.camRight[1], frame.camRight[2]);
        gl.uniform3f(camULoc, frame.camUp[0], frame.camUp[1], frame.camUp[2]);



        // Bind shared quad geo
        gl.bindBuffer(gl.ARRAY_BUFFER, _particleQuadVbo);
        gl.enableVertexAttribArray(posLoc);
        gl.vertexAttribPointer(posLoc, 3, gl.FLOAT, false, 0, 0);

        gl.bindBuffer(gl.ARRAY_BUFFER, _particleQuadUbo);
        gl.enableVertexAttribArray(uvLoc);
        gl.vertexAttribPointer(uvLoc, 2, gl.FLOAT, false, 0, 0);

        for (const group of frame.particleInstances) {
            if (!group || group.count <= 0) continue;
            if (!group.offsets || group.offsets.length < group.count * 3) { console.warn('[Particles] bad offsets', group.type, group.count, group.offsets?.length); continue; }
            if (!group.colors || group.colors.length < group.count * 4) { console.warn('[Particles] bad colors', group.type, group.count, group.colors?.length); continue; }
            if (!group.sizes || group.sizes.length < group.count) { console.warn('[Particles] bad sizes', group.type, group.count, group.sizes?.length); continue; }



            if (!_particleInstanceBuffers[group.type]) {
                _particleInstanceBuffers[group.type] = {
                    offsetBuf: gl.createBuffer(),
                    colorBuf: gl.createBuffer(),
                    sizeBuf: gl.createBuffer(),
                    maxCount: 0,
                };
            }
            const bufs = _particleInstanceBuffers[group.type];

            const offsets = group.offsets instanceof Float32Array ? group.offsets : new Float32Array(group.offsets);
            const colors = group.colors instanceof Float32Array ? group.colors : new Float32Array(group.colors);
            const sizes = group.sizes instanceof Float32Array ? group.sizes : new Float32Array(group.sizes);

            // Upload offsets
            if (group.count > bufs.maxCount) {
                bufs.maxCount = group.count;
                gl.bindBuffer(gl.ARRAY_BUFFER, bufs.offsetBuf);
                gl.bufferData(gl.ARRAY_BUFFER, offsets, gl.DYNAMIC_DRAW);
                gl.bindBuffer(gl.ARRAY_BUFFER, bufs.colorBuf);
                gl.bufferData(gl.ARRAY_BUFFER, colors, gl.DYNAMIC_DRAW);
                gl.bindBuffer(gl.ARRAY_BUFFER, bufs.sizeBuf);
                gl.bufferData(gl.ARRAY_BUFFER, sizes, gl.DYNAMIC_DRAW);
            } else {
                gl.bindBuffer(gl.ARRAY_BUFFER, bufs.offsetBuf);
                gl.bufferSubData(gl.ARRAY_BUFFER, 0, offsets);
                gl.bindBuffer(gl.ARRAY_BUFFER, bufs.colorBuf);
                gl.bufferSubData(gl.ARRAY_BUFFER, 0, colors);
                gl.bindBuffer(gl.ARRAY_BUFFER, bufs.sizeBuf);
                gl.bufferSubData(gl.ARRAY_BUFFER, 0, sizes);
            }

            gl.bindBuffer(gl.ARRAY_BUFFER, bufs.offsetBuf);
            gl.enableVertexAttribArray(offLoc);
            gl.vertexAttribPointer(offLoc, 3, gl.FLOAT, false, 12, 0);
            gl.vertexAttribDivisor(offLoc, 1);

            // Colors — rgba per instance
            gl.bindBuffer(gl.ARRAY_BUFFER, bufs.colorBuf);
            gl.enableVertexAttribArray(colLoc);
            gl.vertexAttribPointer(colLoc, 4, gl.FLOAT, false, 16, 0);
            gl.vertexAttribDivisor(colLoc, 1);

            // Sizes — 1 float per instance
            gl.bindBuffer(gl.ARRAY_BUFFER, bufs.sizeBuf);
            gl.enableVertexAttribArray(sizeLoc);
            gl.vertexAttribPointer(sizeLoc, 1, gl.FLOAT, false, 4, 0);
            gl.vertexAttribDivisor(sizeLoc, 1);



            // Texture
            const tex = _textureCache[group.texKey];
            if (tex) {
                gl.activeTexture(gl.TEXTURE0);
                gl.bindTexture(gl.TEXTURE_2D, tex);
                gl.uniform1i(texLoc, 0);
                gl.uniform1i(hasTexLoc, 1);
            } else {
                gl.uniform1i(hasTexLoc, 0);
            }

            gl.drawArraysInstanced(gl.TRIANGLES, 0, 6, group.count);

            gl.vertexAttribDivisor(offLoc, 0);
            gl.vertexAttribDivisor(colLoc, 0);
            gl.vertexAttribDivisor(sizeLoc, 0);
        }

        // End of renderParticles(), before the closing brace
        gl.useProgram(_activeProgram);
        gl.depthMask(true);
    }

    function renderText(frame) {
        if (!frame.textMeshes || !frame.textMeshes.length) return;
        if (!_textProgram) return;
        const gl = _gl;

        gl.disable(gl.DEPTH_TEST);
        gl.depthMask(false);
        gl.enable(gl.BLEND);
        gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
        gl.disable(gl.CULL_FACE);
        gl.bindFramebuffer(gl.FRAMEBUFFER, null);
        gl.useProgram(_textProgram);

        for (const tm of frame.textMeshes) {
            if (!window.SpectralTextSystem.isAtlasLoaded(tm.fontKey)) {
                window.SpectralTextSystem.loadAtlas(tm.fontKey, tm.jsonUrl, tm.texUrl);
                continue;
            }

            const existingBuf = _meshBuffers[tm.meshId];
            const needsBuild = tm.needsRebuild || !existingBuf || existingBuf.vertCount === 0;
            if (needsBuild) {
                window.SpectralTextSystem.buildTextGeometry(
                    tm.meshId, tm.text, tm.fontKey,
                    tm.fontSize, tm.letterSpacing, tm.align);
            }

            const buf = _meshBuffers[tm.meshId];
            if (!buf || buf.vertCount === 0) continue;

            const atlasTex = window.SpectralTextSystem.getAtlasTexture(tm.fontKey);
            if (!atlasTex) continue;

            gl.uniform4f(_textLocs.outlineColor,
                tm.outlineR, tm.outlineG, tm.outlineB, tm.outlineA);
            gl.uniform1f(_textLocs.outlineWidth, tm.outlineWidth);
            gl.uniform1f(_textLocs.softness, 0.05);
            gl.uniform1f(_textLocs.glowRadius, tm.glowRadius ?? 0.25);
            gl.uniform1f(_textLocs.glowStrength, tm.glowStrength ?? 0.8);

            gl.activeTexture(gl.TEXTURE0);
            gl.bindTexture(gl.TEXTURE_2D, atlasTex);
            gl.uniform1i(_textLocs.atlas, 0);

            gl.bindBuffer(gl.ARRAY_BUFFER, buf.vbo);
            gl.enableVertexAttribArray(_textLocs.pos);
            gl.vertexAttribPointer(_textLocs.pos, 3, gl.FLOAT, false, 0, 0);

            gl.bindBuffer(gl.ARRAY_BUFFER, buf.ubo);
            gl.enableVertexAttribArray(_textLocs.uv);
            gl.vertexAttribPointer(_textLocs.uv, 2, gl.FLOAT, false, 0, 0);

            // Glow passes — 8 directions
            // Layer 1 — Shadow blur passes (CSS style soft feathered bloom)
            if ((tm.shadowBlur ?? 0) > 0.01) {
                const shadowPasses = [
                    { spread: tm.shadowBlur * 0.5, alpha: 0.6 },
                    { spread: tm.shadowBlur * 1.0, alpha: 0.4 },
                    { spread: tm.shadowBlur * 1.5, alpha: 0.2 },
                    { spread: tm.shadowBlur * 2.0, alpha: 0.1 },
                ];
                const shadowDirs = [
                    [1, 0], [-1, 0], [0, 1], [0, -1],
                    [1, 1], [-1, 1], [1, -1], [-1, -1],
                ];
                for (const pass of shadowPasses) {
                    for (const [dx, dy] of shadowDirs) {
                        const m = new Float32Array(tm.mvp);
                        m[12] += dx * pass.spread;
                        m[13] += dy * pass.spread;
                        gl.uniformMatrix4fv(_textLocs.mvp, false, m);
                        gl.uniform4f(_textLocs.color,
                            tm.shadowR ?? 0,
                            tm.shadowG ?? 0,
                            tm.shadowB ?? 0,
                            (tm.shadowA ?? 0) * pass.alpha);
                        gl.drawArrays(gl.TRIANGLES, 0, buf.vertCount);
                    }
                }
            }

            // Layer 2 — Mirror glow passes (existing effect kept as is)
            if ((tm.glowStrength ?? 0.8) > 0.01 && (tm.glowRadius ?? 0.25) > 0.01) {
                const spread = (tm.glowRadius ?? 0.25) * 0.5;
                const glowOffsets = [
                    [spread, 0], [-spread, 0],
                    [0, spread], [0, -spread],
                    [spread, spread], [-spread, spread],
                    [spread, -spread], [-spread, -spread],
                ];
                for (const [ox, oy] of glowOffsets) {
                    const m = new Float32Array(tm.mvp);
                    m[12] += ox;
                    m[13] += oy;
                    gl.uniformMatrix4fv(_textLocs.mvp, false, m);
                    gl.uniform4f(_textLocs.color,
                        tm.glowR ?? tm.r,
                        tm.glowG ?? tm.g,
                        tm.glowB ?? tm.b,
                        (tm.glowA ?? 1.0) * (tm.glowStrength ?? 0.8) * 0.4);
                    gl.drawArrays(gl.TRIANGLES, 0, buf.vertCount);
                }
            }

            // Main draw on top
            gl.uniformMatrix4fv(_textLocs.mvp, false, tm.mvp);
            gl.uniform4f(_textLocs.color, tm.r, tm.g, tm.b, tm.a);
            gl.drawArrays(gl.TRIANGLES, 0, buf.vertCount);
        }

        gl.enable(gl.DEPTH_TEST);
        gl.depthMask(true);
        gl.enable(gl.CULL_FACE);
    }




    function renderFoliage(frame) {
        if (!frame.foliageInstances || !frame.foliageInstances.length) return;
        const gl = _gl;

        if (!_foliageProgram) initFoliage();
        if (!_foliageProgram) return;

        gl.useProgram(_foliageProgram);
        gl.enable(gl.DEPTH_TEST);
        gl.depthMask(true);
        gl.enable(gl.BLEND);
        gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
        gl.disable(gl.CULL_FACE);  // billboards are double-sided

        gl.uniformMatrix4fv(_fLocs.vp, false, frame.vp);
        gl.uniform3f(_fLocs.camPos, frame.camX, frame.camY, frame.camZ);
        gl.uniform1i(_fLocs.lightCount, frame.lightCount);

        for (let li = 0; li < frame.lightCount; li++) {
            gl.uniform3f(_fLocs.lightPos[li],
                frame.lightPositions[li * 3],
                frame.lightPositions[li * 3 + 1],
                frame.lightPositions[li * 3 + 2]);
            gl.uniform3f(_fLocs.lightColor[li],
                frame.lightColors[li * 3],
                frame.lightColors[li * 3 + 1],
                frame.lightColors[li * 3 + 2]);
            gl.uniform1f(_fLocs.lightIntensity[li], frame.lightIntensities[li]);
            gl.uniform1f(_fLocs.lightRange[li], frame.lightRanges[li]);
            gl.uniform1i(_fLocs.lightType[li], frame.lightTypes[li]);
        }

        // Shadow map for directional light
        let sunIdx = -1;
        if (frame.lightTypes) {
            for (let i = 0; i < frame.lightCount; i++) {
                if (frame.lightTypes[i] === 1) { sunIdx = i; break; }
            }
        }
        if (sunIdx >= 0 && _shadowDepthTexs[sunIdx]) {
            gl.activeTexture(gl.TEXTURE1);
            gl.bindTexture(gl.TEXTURE_2D, _shadowDepthTexs[sunIdx]);
            gl.uniform1i(_fLocs.shadowMap, 1);
            if (frame.lightVPs && frame.lightVPs[sunIdx])
                gl.uniformMatrix4fv(_fLocs.lightVP, false,
                    new Float32Array(frame.lightVPs[sunIdx]));
        }

        for (const group of frame.foliageInstances) {
            if (!group || group.count <= 0) continue;

            const buf = _meshBuffers[group.meshId];

            if (!buf || buf.vertCount < 3) continue;

            const positions = group.positions instanceof Float32Array
                ? group.positions : new Float32Array(group.positions);
            const scales = group.scales instanceof Float32Array
                ? group.scales : new Float32Array(group.scales);
            const rotations = group.rotations instanceof Float32Array
                ? group.rotations : new Float32Array(group.rotations);

            // Upload instance buffers once
            if (!_foliageBuffers[group.meshId] || _foliageBuffers[group.meshId].maxCount === 0) {
                if (!_foliageBuffers[group.meshId]) {
                    _foliageBuffers[group.meshId] = {
                        posBuf: gl.createBuffer(),
                        scaleBuf: gl.createBuffer(),
                        rotBuf: gl.createBuffer(),
                        maxCount: 0,
                    };
                }
                const fb = _foliageBuffers[group.meshId];
                fb.maxCount = group.count;
                gl.bindBuffer(gl.ARRAY_BUFFER, fb.posBuf);
                gl.bufferData(gl.ARRAY_BUFFER, positions, gl.STATIC_DRAW);
                gl.bindBuffer(gl.ARRAY_BUFFER, fb.scaleBuf);
                gl.bufferData(gl.ARRAY_BUFFER, scales, gl.STATIC_DRAW);
                gl.bindBuffer(gl.ARRAY_BUFFER, fb.rotBuf);
                gl.bufferData(gl.ARRAY_BUFFER, rotations, gl.STATIC_DRAW);
            }

            const fb = _foliageBuffers[group.meshId];
            if (!fb) continue;

            // Geometry buffers
            gl.bindBuffer(gl.ARRAY_BUFFER, buf.vbo);
            gl.enableVertexAttribArray(_fLocs.pos);
            gl.vertexAttribPointer(_fLocs.pos, 3, gl.FLOAT, false, 0, 0);

            gl.bindBuffer(gl.ARRAY_BUFFER, buf.nbo);
            gl.enableVertexAttribArray(_fLocs.norm);
            gl.vertexAttribPointer(_fLocs.norm, 3, gl.FLOAT, false, 0, 0);

            gl.bindBuffer(gl.ARRAY_BUFFER, buf.ubo);
            gl.enableVertexAttribArray(_fLocs.uv);
            gl.vertexAttribPointer(_fLocs.uv, 2, gl.FLOAT, false, 0, 0);

            // Instance attributes
            gl.bindBuffer(gl.ARRAY_BUFFER, fb.posBuf);
            gl.enableVertexAttribArray(_fLocs.iPos);
            gl.vertexAttribPointer(_fLocs.iPos, 3, gl.FLOAT, false, 12, 0);
            gl.vertexAttribDivisor(_fLocs.iPos, 1);

            gl.bindBuffer(gl.ARRAY_BUFFER, fb.scaleBuf);
            gl.enableVertexAttribArray(_fLocs.iScale);
            gl.vertexAttribPointer(_fLocs.iScale, 1, gl.FLOAT, false, 4, 0);
            gl.vertexAttribDivisor(_fLocs.iScale, 1);

            gl.bindBuffer(gl.ARRAY_BUFFER, fb.rotBuf);
            gl.enableVertexAttribArray(_fLocs.iRot);
            gl.vertexAttribPointer(_fLocs.iRot, 1, gl.FLOAT, false, 4, 0);
            gl.vertexAttribDivisor(_fLocs.iRot, 1);

            // Multi-material instanced draw — one call per material segment
            if (buf.matBreaks && buf.matBreaks.length > 1) {
                let vertOffset = 0;
                for (let m = 0; m < buf.matBreaks.length; m++) {
                    const segVerts = buf.matBreaks[m];
                    const matIdx = buf.matIndices[m];
                    const matTex = buf.materialTextures && buf.materialTextures[matIdx];
                    const loaded = buf.matTexLoaded && buf.matTexLoaded[matIdx];

                    if (matTex && loaded) {
                        gl.activeTexture(gl.TEXTURE0);
                        gl.bindTexture(gl.TEXTURE_2D, matTex);
                        gl.uniform1i(_fLocs.tex, 0);
                        gl.uniform1i(_fLocs.hasTex, 1);
                    } else {
                        gl.uniform1i(_fLocs.hasTex, 0);
                    }

                    gl.drawArraysInstanced(gl.TRIANGLES, vertOffset, segVerts, group.count);
                    vertOffset += segVerts;
                }
            } else {
                // Single material
                const matTex = buf.materialTextures && buf.materialTextures[0];
                if (matTex && buf.matTexLoaded && buf.matTexLoaded[0]) {
                    gl.activeTexture(gl.TEXTURE0);
                    gl.bindTexture(gl.TEXTURE_2D, matTex);
                    gl.uniform1i(_fLocs.tex, 0);
                    gl.uniform1i(_fLocs.hasTex, 1);
                } else {
                    gl.uniform1i(_fLocs.hasTex, 0);
                }
                gl.drawArraysInstanced(gl.TRIANGLES, 0, buf.vertCount, group.count);
            }

            gl.vertexAttribDivisor(_fLocs.iPos, 0);
            gl.vertexAttribDivisor(_fLocs.iScale, 0);
            gl.vertexAttribDivisor(_fLocs.iRot, 0);
        }
        gl.enable(gl.CULL_FACE); // restore after foliage
    }






    function applyMSAA() {
        // Hardware MSAA — enabled at context creation via { antialias: true }
        // No per-frame work needed
    }

    function applySMAA() {
        const gl = _gl;
        if (!_smaaProgram1) initSMAA();
        if (!_smaaProgram1) return;

        const w = _canvas.width;
        const h = _canvas.height;

        // Use FXAA's quad VBO since it's the same fullscreen quad
        const quadVbo = _fxaaQuadVbo;


        // Pass 1 — Edge detection into _smaaEdgeFbo
        gl.bindFramebuffer(gl.FRAMEBUFFER, _smaaEdgeFbo);
        gl.viewport(0, 0, w, h);
        gl.clearColor(0, 0, 0, 0);
        gl.clear(gl.COLOR_BUFFER_BIT);
        gl.disable(gl.DEPTH_TEST);
        gl.useProgram(_smaaProgram1);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, _fxaaColorTex); // scene is in FXAA's color tex
        gl.uniform1i(gl.getUniformLocation(_smaaProgram1, "uColorTex"), 0);
        gl.uniform2f(gl.getUniformLocation(_smaaProgram1, "uResolution"), w, h);
        drawQuad(_smaaProgram1);

        // Pass 2 — Blend weights into _smaaBlendFbo
        gl.bindFramebuffer(gl.FRAMEBUFFER, _smaaBlendFbo);
        gl.clearColor(0, 0, 0, 0);
        gl.clear(gl.COLOR_BUFFER_BIT);
        gl.useProgram(_smaaProgram2);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, _smaaEdgeTex);
        gl.uniform1i(gl.getUniformLocation(_smaaProgram2, "uEdgeTex"), 0);
        gl.uniform2f(gl.getUniformLocation(_smaaProgram2, "uResolution"), w, h);
        drawQuad(_smaaProgram2);

        // Pass 3 — Neighbourhood blend to screen
        gl.bindFramebuffer(gl.FRAMEBUFFER, null);
        gl.viewport(0, 0, w, h);
        gl.useProgram(_smaaProgram3);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, _fxaaColorTex);
        gl.uniform1i(gl.getUniformLocation(_smaaProgram3, "uColorTex"), 0);
        gl.activeTexture(gl.TEXTURE1);
        gl.bindTexture(gl.TEXTURE_2D, _smaaBlendTex);
        gl.uniform1i(gl.getUniformLocation(_smaaProgram3, "uBlendTex"), 1);
        gl.uniform2f(gl.getUniformLocation(_smaaProgram3, "uResolution"), w, h);
        drawQuad(_smaaProgram3);

        gl.enable(gl.DEPTH_TEST);
    }


    function applyFXAA() {
        const gl = _gl;
        if (!_fxaaProgram) {
            initFXAA();
        }
        if (!_fxaaProgram || !_fxaaFbo) return;
        // Blit the default framebuffer into the FXAA color texture
        // by re-rendering — instead we redirect the main pass to _fxaaFbo
        // then run FXAA on top. See renderFrame FXAA redirect below.

        gl.bindFramebuffer(gl.FRAMEBUFFER, null);
        gl.viewport(0, 0, _canvas.width, _canvas.height);
        gl.disable(gl.DEPTH_TEST);
        gl.useProgram(_fxaaProgram);

        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, _fxaaColorTex);
        gl.uniform1i(_fxaaTexLoc, 0);
        gl.uniform2f(_fxaaResLoc, _canvas.width, _canvas.height);

        gl.bindBuffer(gl.ARRAY_BUFFER, _fullscreenQuadVbo);
        gl.enableVertexAttribArray(_fxaaPosLoc);
        gl.vertexAttribPointer(_fxaaPosLoc, 2, gl.FLOAT, false, 0, 0);
        gl.drawArrays(gl.TRIANGLES, 0, 6);

        gl.enable(gl.DEPTH_TEST);
    }

    function applyTAA() {
        const gl = _gl;
        if (!_taaProgram || _taaTexWidth !== _canvas.width || _taaTexHeight !== _canvas.height) {
            initTAA();
        }
        if (!_taaProgram) return;

        const w = _canvas.width;
        const h = _canvas.height;

        // Blend current frame (in _fxaaColorTex) with history into _taaCurrentFbo
        gl.bindFramebuffer(gl.FRAMEBUFFER, _taaCurrentFbo);
        gl.viewport(0, 0, w, h);
        gl.disable(gl.DEPTH_TEST);
        gl.useProgram(_taaProgram);

        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, _fxaaColorTex); // current frame lives here
        gl.uniform1i(gl.getUniformLocation(_taaProgram, "uCurrentTex"), 0);

        gl.activeTexture(gl.TEXTURE1);
        gl.bindTexture(gl.TEXTURE_2D, _taaHistoryTex);
        gl.uniform1i(gl.getUniformLocation(_taaProgram, "uHistoryTex"), 1);

        gl.uniform2f(gl.getUniformLocation(_taaProgram, "uResolution"), w, h);
        gl.uniform1f(gl.getUniformLocation(_taaProgram, "uBlend"), 0.9); // 90% history weight

        const pos = gl.getAttribLocation(_taaProgram, "aPosition");
        gl.bindBuffer(gl.ARRAY_BUFFER, _fullscreenQuadVbo);
        gl.enableVertexAttribArray(pos);
        gl.vertexAttribPointer(pos, 2, gl.FLOAT, false, 0, 0);
        gl.drawArrays(gl.TRIANGLES, 0, 6);

        // Blit _taaCurrentTex to screen
        gl.bindFramebuffer(gl.FRAMEBUFFER, null);
        gl.viewport(0, 0, w, h);
        gl.useProgram(_taaProgram);

        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, _taaCurrentTex);
        gl.uniform1i(gl.getUniformLocation(_taaProgram, "uCurrentTex"), 0);

        gl.activeTexture(gl.TEXTURE1);
        gl.bindTexture(gl.TEXTURE_2D, _taaCurrentTex); // blend with itself = passthrough
        gl.uniform1i(gl.getUniformLocation(_taaProgram, "uHistoryTex"), 1);
        gl.uniform1f(gl.getUniformLocation(_taaProgram, "uBlend"), 0.0); // 0% history = show current only

        gl.drawArrays(gl.TRIANGLES, 0, 6);

        // Copy current to history for next frame
        gl.bindFramebuffer(gl.FRAMEBUFFER, _taaHistoryFbo);
        gl.useProgram(_taaProgram);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, _taaCurrentTex);
        gl.uniform1i(gl.getUniformLocation(_taaProgram, "uCurrentTex"), 0);
        gl.activeTexture(gl.TEXTURE1);
        gl.bindTexture(gl.TEXTURE_2D, _taaCurrentTex);
        gl.uniform1i(gl.getUniformLocation(_taaProgram, "uHistoryTex"), 1);
        gl.uniform1f(gl.getUniformLocation(_taaProgram, "uBlend"), 0.0);
        gl.drawArrays(gl.TRIANGLES, 0, 6);

        gl.bindFramebuffer(gl.FRAMEBUFFER, null);
        gl.enable(gl.DEPTH_TEST);
    }

    function applySpectralAA() {
        const gl = _gl;
        if (!_spectralProgram1) initSpectralAA();
        if (!_spectralProgram1) return;

        const w = _canvas.width;
        const h = _canvas.height;



        // Pass 1 — Edge detect + angle into _spectralEdgeFbo
        gl.bindFramebuffer(gl.FRAMEBUFFER, _spectralEdgeFbo);
        gl.viewport(0, 0, w, h);
        gl.clearColor(0, 0, 0, 0);
        gl.clear(gl.COLOR_BUFFER_BIT);
        gl.disable(gl.DEPTH_TEST);
        gl.useProgram(_spectralProgram1);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, _fxaaColorTex);
        gl.uniform1i(gl.getUniformLocation(_spectralProgram1, "uColorTex"), 0);
        gl.uniform2f(gl.getUniformLocation(_spectralProgram1, "uResolution"), w, h);
        drawQuad(_spectralProgram1);

        // Pass 2 — Geometric composite to screen
        gl.bindFramebuffer(gl.FRAMEBUFFER, null);
        gl.viewport(0, 0, w, h);
        gl.useProgram(_spectralProgram2);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, _fxaaColorTex);
        gl.uniform1i(gl.getUniformLocation(_spectralProgram2, "uColorTex"), 0);
        gl.activeTexture(gl.TEXTURE1);
        gl.bindTexture(gl.TEXTURE_2D, _spectralEdgeTex);
        gl.uniform1i(gl.getUniformLocation(_spectralProgram2, "uEdgeTex"), 1);
        gl.uniform2f(gl.getUniformLocation(_spectralProgram2, "uResolution"), w, h);
        drawQuad(_spectralProgram2);

        gl.enable(gl.DEPTH_TEST);
    }


    function applySpectralAAV2() {
        const gl = _gl;
        if (!_spectralV2Program1) initSpectralAAV2();
        if (!_spectralV2Program1) return;

        const w = _canvas.width;
        const h = _canvas.height;


        // Pass 1 — Staircase detection into _spectralV2EdgeFbo
        gl.bindFramebuffer(gl.FRAMEBUFFER, _spectralV2EdgeFbo);
        gl.viewport(0, 0, w, h);
        gl.clearColor(0, 0, 0, 0);
        gl.clear(gl.COLOR_BUFFER_BIT);
        gl.disable(gl.DEPTH_TEST);
        gl.useProgram(_spectralV2Program1);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, _fxaaColorTex);
        gl.uniform1i(gl.getUniformLocation(_spectralV2Program1, "uColorTex"), 0);
        gl.uniform2f(gl.getUniformLocation(_spectralV2Program1, "uResolution"), w, h);
        drawQuad(_spectralV2Program1);

        // Pass 2 — Triangle fill to screen
        gl.bindFramebuffer(gl.FRAMEBUFFER, null);
        gl.viewport(0, 0, w, h);
        gl.useProgram(_spectralV2Program2);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, _fxaaColorTex);
        gl.uniform1i(gl.getUniformLocation(_spectralV2Program2, "uColorTex"), 0);
        gl.activeTexture(gl.TEXTURE1);
        gl.bindTexture(gl.TEXTURE_2D, _spectralV2EdgeTex);
        gl.uniform1i(gl.getUniformLocation(_spectralV2Program2, "uStaircaseTex"), 1);
        gl.uniform2f(gl.getUniformLocation(_spectralV2Program2, "uResolution"), w, h);
        drawQuad(_spectralV2Program2);

        gl.enable(gl.DEPTH_TEST);
    }

    function applySpectralAAV3() {
        const gl = _gl;
        if (!_spectralV3Program1) initSpectralAAV3();
        if (!_spectralV3Program1) return;

        const w = _canvas.width;
        const h = _canvas.height;



        // Pass 1 — Binary edge classification
        gl.bindFramebuffer(gl.FRAMEBUFFER, _spectralV3EdgeFbo);
        gl.viewport(0, 0, w, h);
        gl.clearColor(0, 0, 0, 0);
        gl.clear(gl.COLOR_BUFFER_BIT);
        gl.disable(gl.DEPTH_TEST);
        gl.useProgram(_spectralV3Program1);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, _fxaaColorTex);
        gl.uniform1i(gl.getUniformLocation(_spectralV3Program1, "uColorTex"), 0);
        gl.uniform2f(gl.getUniformLocation(_spectralV3Program1, "uResolution"), w, h);
        drawQuad(_spectralV3Program1);

        // Pass 2 — Line reconstruction
        gl.bindFramebuffer(gl.FRAMEBUFFER, _spectralV3LineFbo);
        gl.clearColor(0, 0, 0, 0);
        gl.clear(gl.COLOR_BUFFER_BIT);
        gl.useProgram(_spectralV3Program2);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, _spectralV3EdgeTex);
        gl.uniform1i(gl.getUniformLocation(_spectralV3Program2, "uClassifyTex"), 0);
        gl.uniform2f(gl.getUniformLocation(_spectralV3Program2, "uResolution"), w, h);
        drawQuad(_spectralV3Program2);

        // Pass 3 — Triangle coverage fill to screen
        gl.bindFramebuffer(gl.FRAMEBUFFER, null);
        gl.viewport(0, 0, w, h);
        gl.useProgram(_spectralV3Program3);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, _fxaaColorTex);
        gl.uniform1i(gl.getUniformLocation(_spectralV3Program3, "uColorTex"), 0);
        gl.activeTexture(gl.TEXTURE1);
        gl.bindTexture(gl.TEXTURE_2D, _spectralV3LineTex);
        gl.uniform1i(gl.getUniformLocation(_spectralV3Program3, "uLineTex"), 1);
        gl.uniform2f(gl.getUniformLocation(_spectralV3Program3, "uResolution"), w, h);
        drawQuad(_spectralV3Program3);

        gl.enable(gl.DEPTH_TEST);
    }



    function buildProgram(vsSrc, fsSrc) {
        const gl = _gl;
        const p = gl.createProgram();
        gl.attachShader(p, compileShader(gl.VERTEX_SHADER, vsSrc));
        gl.attachShader(p, compileShader(gl.FRAGMENT_SHADER, fsSrc));
        gl.linkProgram(p);
        return p;
    }

    function drawQuad(program) {
        if (_quadPosLocs[program] === undefined) {
            _quadPosLocs[program] = _gl.getAttribLocation(program, "aPosition");
        }
        const pos = _quadPosLocs[program];
        _gl.bindBuffer(_gl.ARRAY_BUFFER, _fullscreenQuadVbo);
        _gl.enableVertexAttribArray(pos);
        _gl.vertexAttribPointer(pos, 2, _gl.FLOAT, false, 0, 0);
        _gl.drawArrays(_gl.TRIANGLES, 0, 6);
    }

    function createColorTexture(w, h) {
        const gl = _gl;
        const tex = gl.createTexture();
        gl.bindTexture(gl.TEXTURE_2D, tex);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, w, h, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
        return tex;
    }

    function createFboForTexture(tex, w, h) {
        const gl = _gl;
        const rb = gl.createRenderbuffer();
        gl.bindRenderbuffer(gl.RENDERBUFFER, rb);
        gl.renderbufferStorage(gl.RENDERBUFFER, gl.DEPTH_COMPONENT16, w, h);
        const fbo = gl.createFramebuffer();
        gl.bindFramebuffer(gl.FRAMEBUFFER, fbo);
        gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, tex, 0);
        gl.framebufferRenderbuffer(gl.FRAMEBUFFER, gl.DEPTH_ATTACHMENT, gl.RENDERBUFFER, rb);
        gl.bindFramebuffer(gl.FRAMEBUFFER, null);
        return fbo;
    }


    function mat4Multiply(a, b) {
        const out = new Float32Array(16);
        for (let col = 0; col < 4; col++) {
            for (let row = 0; row < 4; row++) {
                let sum = 0;
                for (let k = 0; k < 4; k++)
                    sum += a[k * 4 + row] * b[col * 4 + k];
                out[col * 4 + row] = sum;
            }
        }
        return out;
    }



    function startRenderLoop(canvasRef, dotnetRef) {
        // Wait for canvas to have real dimensions before init
        function waitForSize() {
            const w = canvasRef.clientWidth || canvasRef.width;
            const h = canvasRef.clientHeight || canvasRef.height;
            if (w > 300 && h > 150) {
                canvasRef.width = w;
                canvasRef.height = h;
                init(canvasRef, dotnetRef);
                let _pendingFrame = false;
                function loop() {
                    _animationHandle = requestAnimationFrame(loop);
                    if (_pendingFrame) return;
                    _pendingFrame = true;
                    renderFrame().finally(() => { _pendingFrame = false; });
                }
                _animationHandle = requestAnimationFrame(loop);
            } else {
                requestAnimationFrame(waitForSize);
            }
        }
        requestAnimationFrame(waitForSize);
    }






    function stopRenderLoop() {
        if (_animationHandle !== null) {
            cancelAnimationFrame(_animationHandle);
            _animationHandle = null;
        }
        _dotnetRef = null;
    }

    function resizeCanvas(width, height) {
        if (!_canvas) return;
        _canvas.width = width;
        _canvas.height = height;
        if (_gl) _gl.viewport(0, 0, width, height);
    }

    function resetParticles() {
        const particleKeys = [
            'ParticleGeo_/iAssets/RainDrop01.png',
            'ParticleGeo_/iAssets/SnowFlake01.png',
            'ParticleGeo_/iAssets/GOkuCloud001.png',
            'ParticleGeo_/iAssets/LBolt002.png',
        ];
        particleKeys.forEach(key => {
            delete _textureCache[key];
            delete _meshBuffers[key];
        });
        // Reset per-type instance buffers too
        for (const type in _particleInstanceBuffers) {
            if (type !== '__preallocated__')
                delete _particleInstanceBuffers[type];
        }
        console.log('[SpectralGL] Particle textures reset');
    }
    
    function resetSkyTextures() {
        _skyTexsLoaded.day = false;
        _skyTexsLoaded.night = false;

        _skyDayTex = null;
        _skyNightTex = null;

        console.log('[SpectralGL] Sky textures RESET');
    }
    

    return { startRenderLoop, stopRenderLoop, resizeCanvas, uploadParsedMesh, resetParticles, resetSkyTextures };

})();
