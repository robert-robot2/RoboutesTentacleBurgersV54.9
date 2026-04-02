namespace SpectralXAXA.SpectralXComponent.SpectralXRender
{
   
        /// <summary>
        /// Interface for all mesh types in SpectralX engine
        /// Defines the contract for mesh data and transformations
        /// </summary>
        public interface IMesh
        {
            /// <summary>
            /// Unique identifier for this mesh
            /// </summary>
            string Name { get; set; }

            /// <summary>
            /// List of vertex positions in local space
            /// </summary>
            List<Vector3> Vertices { get; }

            /// <summary>
            /// Triangle indices (groups of 3)
            /// </summary>
            List<int> Indices { get; }

            /// <summary>
            /// Optional: UV coordinates for texturing (future)
            /// </summary>
            List<Vector2> UVs { get; }

            /// <summary>
            /// Optional: Vertex normals for lighting (future)
            /// </summary>
            List<Vector3> Normals { get; }

            /// <summary>
            /// Number of vertices in this mesh
            /// </summary>
            int VertexCount { get; }

            /// <summary>
            /// Number of triangles in this mesh
            /// </summary>
            int TriangleCount { get; }

            /// <summary>
            /// Validate mesh data integrity
            /// </summary>
            bool IsValid();

            /// <summary>
            /// Create a deep copy of this mesh
            /// </summary>
            IMesh Clone();

        }
    
}
