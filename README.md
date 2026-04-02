# Roboutes Tentacle Burgers Version 54.9

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)

## 🎯 Features
Custom WebGL2 C# Renderer
<img width="1920" height="1080" alt="Screenshot 2026-03-31 133951" src="https://github.com/user-attachments/assets/74183e1d-7049-4947-bc44-5966c70e8af3" />
<img width="1920" height="1080" alt="Screenshot 2026-03-31 133942" src="https://github.com/user-attachments/assets/ae4f07da-dfb2-45a6-987d-895f5e7e81bd" />

### 📁 SpectralX WebGL C# Custom XYZ Engine
SpectralX — Custom WebGL2 3D Engine (Live: roboutestentacleburgers.azurewebsites.net)
Browser-native 3D engine built from scratch in C# Blazor WASM with a JavaScript WebGL2 interop layer. No third-party engine or rendering library used.
Rendering pipeline: full deferred-style render loop with opaque/transparent sort, two-pass transparent rendering, depth masking, and per-mesh material segments.
Shadow system with per-light FBOs, supporting PCF, PCSS, and three custom shadow modes (SpecXS VDS V1, RPD V2, IGN V3) with tunable penumbra tinting, contact sharpness, and blocker search radius. 
Eight anti-aliasing modes: MSAA, FXAA, SMAA, TAA with Halton sequence jitter and variance-clip ghosting reduction, plus three original SpectralAA algorithms
(gradient-angle geometric composite, staircase topology detection, binary edge classification with triangle coverage fill). Instanced rendering for up to 5,000 particles
(billboard quads, per-instance color/size/offset) and foliage (rotation, scale, per-segment multi-material). 512×512 GPU tilemap with dynamic height/normal uploads, triplanar texture blending, 
6-material paint system, and ray-unprojection brush. Day/night sky system with procedural sun disk, atmospheric scatter, moon disk and glow, scrolling cloud and star layers, and dynamic directional light sync.
SDF text rendering with outline, glow, and CSS-style shadow blur passes. Up to 32 simultaneous lights (point, spot, area, directional) with per-light shadow maps for the first 8 slots. 
Chunked async index buffer generation to avoid main-thread stalls during tilemap init. C# engine layer handles scene graph, portal-based scene switching, TAA jitter,
FBX mesh loading, prop scatter, gamepad input, sprite sheet animation, and full JS interop serialization via [JSInvokable].
<img width="1920" height="1080" alt="Screenshot 2026-03-29 145443" src="https://github.com/user-attachments/assets/530b75c0-9253-44f4-ab80-d3cd6740803c" />
<img width="1920" height="1080" alt="Screenshot 2026-03-29 145422" src="https://github.com/user-attachments/assets/c008cd1c-d98f-47e2-a4e3-86ed1a6f14bf" />




### Built With
- **.NET 10.0** - Modern .NET framework


## 📝 Configuration

## 🤝 Contributing

Contributions, issues, and feature requests are welcome!

## 📄 License

This project is licensed under the MIT License.

## 🙏 Acknowledgments

- Built with Claude AI and ChatGPT <3.
