namespace SpectralXAXA.SpectralXComponent.SpectralXRender
{
   
        /// <summary>
        /// Base implementation of IMesh
        /// Represents mesh data in local space (before world transform)
        /// </summary>
        public class SpectralXMesh : IMesh
        {
            public string Name { get; set; } = string.Empty;
            public List<Vector3> Vertices { get; set; } = new();
            public List<int> Indices { get; set; } = new();
            public List<Vector2> UVs { get; set; } = new();
            public List<Vector3> Normals { get; set; } = new();

            public int VertexCount => Vertices.Count;
            public int TriangleCount => Indices.Count / 3;

          
            public SpectralXMesh(string name)
            {
                Name = name;
            }

            /// <summary>
            /// Validate mesh has proper data
            /// </summary>
            public bool IsValid()
            {
                if (Vertices.Count == 0) return false;
                if (Indices.Count == 0) return false;
                if (Indices.Count % 3 != 0) return false; // Must be triangles

                // Check all indices are valid
                foreach (var idx in Indices)
                {
                    if (idx < 0 || idx >= Vertices.Count)
                        return false;
                }

                return true;
            }

            /// <summary>
            /// Create a deep copy of this mesh
            /// </summary>
            public IMesh Clone()
            {
                var clone = new SpectralXMesh(Name + "_Clone")
                {
                    Vertices = new List<Vector3>(Vertices),
                    Indices = new List<int>(Indices),
                    UVs = new List<Vector2>(UVs),
                    Normals = new List<Vector3>(Normals)
                };
                return clone;
            }

            /// <summary>
            /// Calculate bounds of this mesh
            /// </summary>
            public (Vector3 min, Vector3 max) GetBounds()
            {
                if (Vertices.Count == 0)
                    return (Vector3.Zero, Vector3.Zero);

                var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                foreach (var v in Vertices)
                {
                    min = Vector3.Min(min, v);
                    max = Vector3.Max(max, v);
                }

                return (min, max);
            }

            /// <summary>
            /// Get center point of mesh bounds
            /// </summary>
            public Vector3 GetCenter()
            {
                var (min, max) = GetBounds();
                return (min + max) * 0.5f;
            }

            /// <summary>
            /// Helper to add a triangle
            /// </summary>
            public void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2)
            {
                int startIdx = Vertices.Count;
                Vertices.Add(v0);
                Vertices.Add(v1);
                Vertices.Add(v2);
                Indices.Add(startIdx);
                Indices.Add(startIdx + 1);
                Indices.Add(startIdx + 2);
            }
        }
    

}
