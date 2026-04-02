namespace SpectralXBXB.SpectralXComponent.SpectralXRender
{
    /// <summary>
    /// Interface for all mesh types in SpectralX engine
    /// </summary>
    public interface IMesh
    {
        string Name { get; set; }
        Vector3 Position { get; set; }
        Vector4 Color { get; set; }

        // Size
        Vector3 Size { get; set; }
        Vector3 Rotation { get; set; } // radians
        // Geometry
        List<Vector3> Vertices { get; }
        List<Vector2> UVs { get; }
        List<Vector3> Normals { get; }
        List<Face> Faces { get; }

        // Geometry counts
        int VertexCount { get; }
        int UVCount { get; }
        int NormalCount { get; }
        int FaceCount { get; }

        int PrimitiveCount { get; }


        // Edges
        List<Edge> Edges { get; }

        // Edge count (raw)
        int EdgeCount { get; }

        // Face type counts
        bool HasRenderableFaces { get; }
        int TriangleFaceCount { get; }
        int QuadFaceCount { get; }

        // Texture support
        string? TexturePath { get; set; }
        bool HasTexture => !string.IsNullOrEmpty(TexturePath);
        Vector3[,]? TextureData { get; set; }
        string? TextureDataUrl { get; set; }

        bool IsValid();
        IMesh Clone();
    }
}
