namespace SpectralXAXA.SpectralXComponent

{
    /// <summary>
    /// Central repository for loading and storing mesh definitions
    /// Meshes are loaded once and can be reused by multiple entities
    /// </summary>
    public class SpectralXMeshLibrary
    {
        private readonly Dictionary<string, IMesh> meshCache = new();

        /// <summary>
        /// All loaded meshes
        /// </summary>
        public IReadOnlyDictionary<string, IMesh> Meshes => meshCache;

        /// <summary>
        /// Number of meshes in library
        /// </summary>
        public int MeshCount => meshCache.Count;

        public SpectralXMeshLibrary()
        {
            // Auto-load built-in primitives
            LoadBuiltInPrimitives();
        }

        /// <summary>
        /// Get a mesh by name
        /// </summary>
        public IMesh? GetMesh(string name)
        {
            meshCache.TryGetValue(name, out var mesh);
            return mesh;
        }

        /// <summary>
        /// Check if mesh exists in library
        /// </summary>
        public bool HasMesh(string name)
        {
            return meshCache.ContainsKey(name);
        }

        /// <summary>
        /// Add a mesh to the library
        /// </summary>
        public void AddMesh(string name, IMesh mesh)
        {
            mesh.Name = name;
            meshCache[name] = mesh;
            Console.WriteLine($"[MeshLibrary] Added mesh: {name} ({mesh.VertexCount} verts, {mesh.TriangleCount} tris)");
        }

        /// <summary>
        /// Remove a mesh from library
        /// </summary>
        public bool RemoveMesh(string name)
        {
            return meshCache.Remove(name);
        }

        /// <summary>
        /// Load built-in primitive shapes
        /// </summary>
        private void LoadBuiltInPrimitives()
        {
            AddMesh("PrimCube", CreateCube());
            AddMesh("Plane", CreatePlane());
            // Can add more primitives: Sphere, Cylinder, etc.
        }

        /// <summary>
        /// Create a unit cube mesh (same as old test cube)
        /// </summary>
        private IMesh CreateCube()
        {
            var mesh = new SpectralXMesh("PrimCube");

            mesh.Vertices.AddRange(new[]
            {
                new Vector3(-1, -1, -1), // 0
                new Vector3( 1, -1, -1), // 1
                new Vector3( 1,  1, -1), // 2
                new Vector3(-1,  1, -1), // 3
                new Vector3(-1, -1,  1), // 4
                new Vector3( 1, -1,  1), // 5
                new Vector3( 1,  1,  1), // 6
                new Vector3(-1,  1,  1), // 7
            });

            mesh.Indices.AddRange(new[]
            {
                0,1,2, 0,2,3, // back
                4,5,6, 4,6,7, // front
                0,1,5, 0,5,4, // bottom
                2,3,7, 2,7,6, // top
                1,2,6, 1,6,5, // right
                0,3,7, 0,7,4  // left
            });

            return mesh;
        }

        /// <summary>
        /// Create a unit plane mesh (for ground, etc.)
        /// </summary>
        private IMesh CreatePlane()
        {
            var mesh = new SpectralXMesh("Plane");

            mesh.Vertices.AddRange(new[]
            {
                new Vector3(-1, 0, -1),
                new Vector3( 1, 0, -1),
                new Vector3( 1, 0,  1),
                new Vector3(-1, 0,  1),
            });

            mesh.Indices.AddRange(new[] { 0, 1, 2, 0, 2, 3 });

            return mesh;
        }

        /// <summary>
        /// Load mesh using the custom SpectralXFBXParser (no Assimp).
        /// </summary>
        public async Task<SpectralXMesh?> LoadFromFBXParserAsync(HttpClient http, string url, string name)
        {
            try
            {
                Console.WriteLine($"[MeshLibrary] Downloading FBX (Custom Parser) from: {url}");

                var bytes = await http.GetByteArrayAsync(url);
                if (bytes == null || bytes.Length == 0)
                {
                    Console.WriteLine("[MeshLibrary] Downloaded FBX is empty.");
                    return null;
                }

                // 1. Parse bytes → SpectralXMesh
                var mesh = SpectralXFBXParser.Parse(bytes, name);

                if (mesh == null)
                {
                    Console.WriteLine("[MeshLibrary] FBX parser returned null mesh.");
                    return null;
                }

                // 2. Validation + store in library
                if (mesh.IsValid())
                {
                    AddMesh(name, mesh);
                    Console.WriteLine($"[MeshLibrary] Successfully added {name} (Custom Parser)!");
                    return mesh;
                }
                else
                {
                    Console.WriteLine("[MeshLibrary] Mesh was invalid after FBX parse.");
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MeshLibrary] FBX Parser load error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }


        /// <summary>
        /// Clear all meshes from library
        /// </summary>
        public void Clear()
        {
            meshCache.Clear();
            LoadBuiltInPrimitives(); // Reload primitives
        }
    }
}