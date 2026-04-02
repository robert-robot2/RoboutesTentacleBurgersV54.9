 namespace SpectralXBXB.SpectralXComponent.SpectralXLighting
{
    /// <summary>
    /// 3D Point Light for SpectralX engine
    /// Calculates lighting based on world-space position and surface normals
    /// </summary>
    public class SpectralXLight
    {
        // Light properties
        public Vector3 Position { get; set; } = new Vector3(5, 5, 5);
        public Vector3 Color { get; set; } = new Vector3(1f, 1f, 1f); // RGB 0-1
        public float Intensity { get; set; } = 1.0f;
        public float Range { get; set; } = 20f; // Light falloff distance
        public bool Enabled { get; set; } = true;

        public SpectralXLight()
        {
        }

        public SpectralXLight(Vector3 position, Vector3 color, float intensity = 1.0f, float range = 20f)
        {
            Position = position;
            Color = color;
            Intensity = intensity;
            Range = range;
        }

        /// <summary>
        /// Calculate lighting contribution for a surface point
        /// </summary>
        /// <param name="worldPos">World position of the surface</param>
        /// <param name="normal">Surface normal (must be normalized)</param>
        /// <returns>Light intensity 0-1</returns>
        public float CalculateIntensity(Vector3 worldPos, Vector3 normal)
        {
            if (!Enabled) return 0f;

            // Vector from surface to light
            Vector3 lightDir = Position - worldPos;
            float distance = lightDir.Length();

            // Out of range
            if (distance > Range) return 0f;

            // Normalize light direction
            lightDir = Vector3.Normalize(lightDir);

            // Lambertian diffuse (dot product of normal and light direction)
            float diffuse = Math.Max(0f, Vector3.Dot(normal, lightDir));

            // Attenuation (inverse square law with range limit)
            float attenuation = 1.0f - (distance / Range);
            attenuation = Math.Clamp(attenuation, 0f, 1f);

            return diffuse * Intensity * attenuation;
        }

        /// <summary>
        /// Calculate RGB color contribution for a surface point
        /// </summary>
        public Vector3 CalculateColor(Vector3 worldPos, Vector3 normal)
        {
            float intensity = CalculateIntensity(worldPos, normal);
            return Color * intensity;
        }

        /// <summary>
        /// Calculate brightness as 0-255 for simple shading
        /// </summary>
        public int CalculateBrightness(Vector3 worldPos, Vector3 normal)
        {
            float intensity = CalculateIntensity(worldPos, normal);
            return (int)(intensity * 255f);
        }
    }
}
