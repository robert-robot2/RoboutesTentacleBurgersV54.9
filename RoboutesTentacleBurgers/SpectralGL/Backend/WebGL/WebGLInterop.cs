using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace RoboutesTentacleBurgers.SpectralGL.Backend.WebGL
{
    /// <summary>
    /// Minimal interop bridge to WebGL JS backend (Objective 8)
    /// </summary>
    public class WebGLInterop
    {
        private readonly IJSRuntime _jsRuntime;

        public WebGLInterop(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Initializes the WebGL backend with a given canvas ID and size
        /// </summary>
        public async Task InitWebGLAsync(string canvasId, int width, int height)
        {
            await _jsRuntime.InvokeVoidAsync("SpectralGLWebGLBackend_Init", canvasId, width, height);
        }

        /// <summary>
        /// Resizes the WebGL canvas and framebuffer
        /// </summary>
        public async Task ResizeAsync(int width, int height)
        {
            await _jsRuntime.InvokeVoidAsync("SpectralGLWebGLBackend_Resize", width, height);
        }

        /// <summary>
        /// Clears the WebGL framebuffer
        /// </summary>
        public async Task ClearAsync()
        {
            await _jsRuntime.InvokeVoidAsync("SpectralGLWebGLBackend_Clear");
        }

        /// <summary>
        /// Optionally, return a pointer/handle to the framebuffer for C# renderer
        /// </summary>
        /// 
        //check for lagacy hook
        public async Task<int> GetFramebufferIdAsync()
        {
            return await _jsRuntime.InvokeAsync<int>("SpectralGLWebGLBackend_GetFramebufferId");
        }
    }
}
