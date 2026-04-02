window.SpectralGLInterop = (function () {
    let _canvas = null;
    let _ctx = null;
    let _dotnetRef = null;
    let _animationHandle = null;

    function init(canvasRef, dotnetRef) {
        _canvas = canvasRef instanceof HTMLCanvasElement
            ? canvasRef
            : (canvasRef ? canvasRef : null);

        if (!_canvas) {
            console.warn("SpectralGLInterop.init: canvasRef is null.");
            return;
        }

        _ctx = _canvas.getContext("2d");
        if (!_ctx) {
            console.error("SpectralGLInterop.init: Failed to get 2D context.");
            return;
        }

        _dotnetRef = dotnetRef;
    }

    async function renderFrame() {
        if (!_canvas || !_ctx || !_dotnetRef) {
            console.log("[SpectralGLInterop] renderFrame: Missing dependencies");
            return;
        }

        try {
            console.log("[SpectralGLInterop] Calling Tick...");
            await _dotnetRef.invokeMethodAsync("Tick");

            console.log("[SpectralGLInterop] Calling GetFrameDataAsync...");
            const frame = await _dotnetRef.invokeMethodAsync("GetFrameDataAsync");

            console.log("[SpectralGLInterop] Frame received:", frame);

            if (!frame || frame.width <= 0 || frame.height <= 0 || !(frame.data || frame.rgbaBuffer)) {
                console.error("[SpectralGLInterop] Invalid frame data!", frame);
                return;
            }


            // Normalize property names from C# -> JS
            const width = frame.width ?? frame.Width ?? frame.WidthPx ?? 0;
            const height = frame.height ?? frame.Height ?? frame.HeightPx ?? 0;
            // Accept multiple possible buffer property names
            const rawBuffer = frame.rgbaBuffer ?? frame.rgba ?? frame.data ?? frame.Data ?? null;

            if (width <= 0 || height <= 0 || !rawBuffer) {
                console.error("[SpectralGLInterop] Invalid frame data!", { width, height, rawBufferLength: rawBuffer?.length ?? 0 });
                return;
            }

            // Ensure typed array is Uint8ClampedArray for putImageData
            let pixels;
            if (rawBuffer instanceof Uint8ClampedArray) {
                pixels = rawBuffer;
            } else if (rawBuffer instanceof Uint8Array || rawBuffer instanceof ArrayBuffer || Array.isArray(rawBuffer)) {
                pixels = new Uint8ClampedArray(rawBuffer);
            } else {
                // Try to coerce if it's a JS object/Array-like
                try {
                    pixels = new Uint8ClampedArray(rawBuffer);
                } catch (e) {
                    console.error("[SpectralGLInterop] Unable to convert frame buffer to Uint8ClampedArray", e);
                    return;
                }
            }

            // Basic sanity check: expected length == width*height*4
            const expectedLen = width * height * 4;
            if (pixels.length < expectedLen) {
                console.error("[SpectralGLInterop] Frame buffer too small", { expectedLen, actualLen: pixels.length });
                return;
            }
            if (_canvas.width !== width || _canvas.height !== height) {
                _canvas.width = width;
                _canvas.height = height;
            }

            // Create ImageData (pixels is already Uint8ClampedArray)
            let imageData;
            try {
                imageData = new ImageData(pixels, width, height);
            } catch (e) {
                imageData = new ImageData(width, height);
                imageData.data.set(pixels);
            }


            // Draw to canvas
            _ctx.putImageData(imageData, 0, 0);
        } catch (ex) {
            console.error("[SpectralGLInterop] renderFrame error:", ex);
        }
    }

    function startRenderLoop(canvasRef, dotnetRef) {
        init(canvasRef, dotnetRef);

        async function loop() {
            await renderFrame();
            _animationHandle = window.requestAnimationFrame(loop);
        }

        _animationHandle = window.requestAnimationFrame(loop);
    }

    function stopRenderLoop() {
        if (_animationHandle !== null) {
            window.cancelAnimationFrame(_animationHandle);
            _animationHandle = null;
        }
    }

    function resizeCanvas(width, height) {
        if (!_canvas) return;

        _canvas.width = width;
        _canvas.height = height;
    }

    return {
        startRenderLoop,
        stopRenderLoop,
        resizeCanvas
    };
})();
