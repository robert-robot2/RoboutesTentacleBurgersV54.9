// SpectralGLWebGLBackend.js
// Minimal WebGL backend for Objective 8 (triangle rendering)

class SpectralGLWebGLBackend {
    constructor(canvas) {
        if (!(canvas instanceof HTMLCanvasElement)) {
            throw new Error("Must provide a canvas element");
        }

        this.canvas = canvas;
        this.gl = canvas.getContext("webgl2") || canvas.getContext("webgl");

        if (!this.gl) {
            throw new Error("WebGL not supported");
        }

        this.width = canvas.width;
        this.height = canvas.height;

        this.framebuffer = null;
        this.program = null;
        this.defaultTexture = null;

        this.init();
    }

    init() {
        const gl = this.gl;

        // --- Default shader placeholders ---
        const vsSource = `
            attribute vec3 aPosition;
            void main() {
                gl_Position = vec4(aPosition, 1.0);
            }
        `;

        const fsSource = `
            precision mediump float;
            void main() {
                gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0); // red triangle
            }
        `;

        // Compile shaders
        const vertexShader = this.compileShader(gl.VERTEX_SHADER, vsSource);
        const fragmentShader = this.compileShader(gl.FRAGMENT_SHADER, fsSource);

        // Link program
        this.program = this.createProgram(vertexShader, fragmentShader);

        // --- Framebuffer setup ---
        this.framebuffer = gl.createFramebuffer();
        gl.bindFramebuffer(gl.FRAMEBUFFER, this.framebuffer);

        // Create a default texture to attach to framebuffer
        this.defaultTexture = gl.createTexture();
        gl.bindTexture(gl.TEXTURE_2D, this.defaultTexture);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, this.width, this.height, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);

        gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, this.defaultTexture, 0);

        gl.bindFramebuffer(gl.FRAMEBUFFER, null);
        gl.clearColor(0.0, 0.0, 0.0, 1.0);
        gl.clear(gl.COLOR_BUFFER_BIT);
    }

    compileShader(type, source) {
        const gl = this.gl;
        const shader = gl.createShader(type);
        gl.shaderSource(shader, source);
        gl.compileShader(shader);

        if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
            console.error(gl.getShaderInfoLog(shader));
            gl.deleteShader(shader);
            return null;
        }

        return shader;
    }

    createProgram(vs, fs) {
        const gl = this.gl;
        const program = gl.createProgram();
        gl.attachShader(program, vs);
        gl.attachShader(program, fs);
        gl.linkProgram(program);

        if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
            console.error(gl.getProgramInfoLog(program));
            gl.deleteProgram(program);
            return null;
        }

        return program;
    }

    resize(width, height) {
        this.width = width;
        this.height = height;
        this.canvas.width = width;  
        this.canvas.height = height;

        const gl = this.gl;
        gl.viewport(0, 0, width, height);

        // Resize texture
        gl.bindTexture(gl.TEXTURE_2D, this.defaultTexture);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, width, height, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
        gl.bindTexture(gl.TEXTURE_2D, null);
    }

    clear() {
        const gl = this.gl;
        gl.bindFramebuffer(gl.FRAMEBUFFER, this.framebuffer);
        gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
    }
}

// Expose as global so non-module <script> can access the backend
window.SpectralGLWebGLBackend = SpectralGLWebGLBackend;
