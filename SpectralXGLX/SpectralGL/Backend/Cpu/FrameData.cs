using System.Text.Json.Serialization;

namespace SpectralXGLX.SpectralGL.Backend.Cpu
{
    public class FrameData
    {
        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("data")]
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}